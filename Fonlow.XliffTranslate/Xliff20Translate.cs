using Fonlow.Translate;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace Fonlow.XliffTranslate
{
	/// <summary>
	/// XLIFF 2.0 with translation engine.
	/// </summary>
	public class Xliff20Translate : IXliffTranslation
	{
		public Xliff20Translate()
		{
		}

		bool batchMode;
		string sourceFile;
		string targetFile;
		string[] forStates;
		bool unchangeState;
		public void SetBatchMode(bool batchMode)
		{
			this.batchMode=batchMode;
		}

		public void SetSourceFile(string sourceFile)
		{
			this.sourceFile=sourceFile;
		}

		public void SetTargetFile(string targetFile)
		{
			this.targetFile=targetFile;
		}

		public void SetForStates(string[] forStates)
		{
			this.forStates=forStates;
		}

		public void SetUnchangeState(bool unchangeState)
		{
			this.unchangeState=unchangeState;
		}


		public async Task<int> TranslateXliffElement(XElement xliffRoot, string[] forStates, bool unchangeState, ITranslate translator, ILogger logger, IProgressDisplay progressDisplay)
		{
			var ver = xliffRoot.Attribute("version").Value;
			if (ver != "2.0")
			{
				throw new ArgumentException($"Expected XLIFF 2.0 but this one is {ver}");
			}

			var ns = xliffRoot.GetDefaultNamespace();

			if (string.IsNullOrEmpty(translator.SourceLang))
			{
				translator.SourceLang = xliffRoot.Attribute("srcLang").Value; //use source file's source language
			}

			bool toCreateTargetFile = false;
			if (string.IsNullOrEmpty(translator.TargetLang))
			{
				translator.TargetLang = xliffRoot.Attribute("trgLang")?.Value; //use source file 's target language
				if (string.IsNullOrEmpty(translator.TargetLang))
				{
					throw new ArgumentException("TargetLang must be declared in command parameters or xliff/trgLang");
				}
			}

			if (string.IsNullOrEmpty(xliffRoot.Attribute("trgLang")?.Value))
			{
				xliffRoot.Add(new XAttribute("trgLang", translator.TargetLang));//.Attribute("trgLang").Value = g.TargetLang; //source file has no target lang declared, then use the commandline TargetLang
				toCreateTargetFile = true; // The rest of the codes need to add trans-unit/target
			}

			Console.WriteLine($"\nTranslating from {translator.SourceLang} to {translator.TargetLang} ...");

			var fileElements = xliffRoot.Elements(ns + "file");
			var total = 0;
			foreach (var fileElement in fileElements)
			{
				var c = await TranslateXliffFileElement(fileElement, ns, toCreateTargetFile, forStates, unchangeState, translator, logger, progressDisplay).ConfigureAwait(false);
				total += c;
			}

			return total;
		}

		public async Task<int> Translate(ITranslate translator, ILogger logger, IProgressDisplay progressDisplay)
		{
			XDocument xDoc;
			int c;
			using (FileStream fs = new System.IO.FileStream(sourceFile, System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				c = await TranslateXliffElement(xliffRoot, forStates, unchangeState, translator, logger, progressDisplay).ConfigureAwait(false);
			}

			if (c > 0)
			{
				xDoc.Save(targetFile);
			}

			return c;
		}

		async Task<int> TranslateXliffFileElement(XElement fileElement, XNamespace ns, bool toCreateTargetFile, string[] forStates, bool unchangeState, ITranslate translator, ILogger logger, IProgressDisplay progressDisplay)
		{
			var units = fileElement.Elements(ns + "unit").ToList(); //buffering may be slower and more memory usage, however, better UX, since user get count first.
			var firstGroup = fileElement.Element(ns + "group"); //handle one group for now
			if (firstGroup != null)
			{
				var groupUnits = firstGroup.Elements(ns + "unit").ToList();
				units.AddRange(groupUnits);
			}

			var totalUnits = units.Count;
			var totalUnitsToTranslate = units.Count(unit =>
			{
				var segment = unit.Element(ns + "segment");
				var unitSource = segment.Element(ns + "source");
				var unitTarget = segment.Element(ns + "target");

				return unitSource != null && unitSource.Nodes().OfType<XText>().Any() //somehting to translate
					&& (unitTarget == null || forStates.Contains(segment.Attribute("state")?.Value) || string.IsNullOrEmpty(segment.Attribute("state")?.Value));
			});

			if (totalUnitsToTranslate == 0)
			{
				logger.LogWarning("Nothing to translate.");
				return 0;
			}
			else if (totalUnitsToTranslate < totalUnits) //some units are badly defined.
			{
				var badUnits = units.Where(unit =>
				{
					var segment = unit.Element(ns + "segment");
					var unitSource = segment.Element(ns + "source");
					var unitTarget = segment.Element(ns + "target");

					return unitSource != null && !unitSource.Nodes().OfType<XText>().Any() //nothing to translate
					&& (unitTarget == null || forStates.Contains(segment.Attribute("state")?.Value) || string.IsNullOrEmpty(segment.Attribute("state")?.Value)); //though should be translated
				});

				var ids = badUnits.Select(unit => unit.Attribute("id").Value).ToArray();
				var csv = string.Join(", ", ids);
				if (csv.Length > 0)
				{
					logger.LogWarning($"These units with appropriate states have nothing to translate: {csv}");
				}
			}

			var isAllNew = totalUnits == totalUnitsToTranslate;
			Console.WriteLine(isAllNew ? $"Current / Total Units" : $"Current / Total Units To Translate / Total Units");
			int countForUnit = 0;

			if (batchMode)
			{
				const int maxUnits = 100;
				var chunks = units.SplitLists(maxUnits);
				int kc = 0;
				foreach (var chunk in chunks)
				{
					kc = await Batch(chunk).ConfigureAwait(false); // always countsForUnit
				}

				return kc;
			}
			else
			{
				return await TextByText(units).ConfigureAwait(false);
			}

			async Task<int> TextByText(IList<XElement> someUnits)
			{
				foreach (var unit in someUnits)
				{
					var segment = unit.Element(ns + "segment");
					var unitSource = segment.Element(ns + "source");
					var unitTarget = segment.Element(ns + "target");
					if (unitSource != null && unitSource.Nodes().OfType<XText>().Any())
					{
						var state = segment.Attribute("state")?.Value;
						if (state != null && !forStates.Contains(state))
						{
							continue;
						}

						if (state == null && unitTarget == null)
						{
							unitTarget = new XElement(ns + "target");
							segment.Add(unitTarget);
						}
						else
						{
							unitTarget.Nodes().Remove();
						}

						foreach (var n in unitSource.Nodes())
						{
							if (n.NodeType == System.Xml.XmlNodeType.Text)
							{
								var textNode = n as XText;
								try
								{
									var tr = await translator.Translate(textNode.Value).ConfigureAwait(false);
									unitTarget.Add(new XText(tr));
								}
								catch (HttpRequestException ex)
								{
									logger?.LogError(ex.Message);
									return countForUnit;
								}
							}
							else if (n.NodeType == System.Xml.XmlNodeType.Element)
							{
								unitTarget.Add(new XElement(n as XElement));
							}
						}

						if (toCreateTargetFile)
						{
							segment.SetAttributeValue("state", "initial");
						}

						if (!unchangeState)
						{
							segment.SetAttributeValue("state", "translated");
						}

						countForUnit++;

						progressDisplay?.Show(countForUnit, totalUnits, isAllNew, totalUnitsToTranslate);

					}
				}

				return countForUnit;
			}

			async Task<int> Batch(IList<XElement> someUnits)
			{
				var strings = new List<string>();
				foreach (var unit in someUnits)
				{
					var segment = unit.Element(ns + "segment");
					var unitSource = segment.Element(ns + "source");
					var unitTarget = segment.Element(ns + "target");
					if (unitSource != null && unitSource.Nodes().OfType<XText>().Any())
					{
						var state = segment.Attribute("state")?.Value;
						if (state != null && !forStates.Contains(state))
						{
							continue;
						}

						foreach (var n in unitSource.Nodes())
						{
							if (n.NodeType == System.Xml.XmlNodeType.Text)
							{
								var textNode = n as XText;
								strings.Add(textNode.Value);
							}
						}
					}
				}

				if (strings.Count == 0)
				{
					return 0;
				}

				var translatedStrings = await translator.Translate(strings).ConfigureAwait(false);

				int translatedIndex = 0;
				foreach (var unit in someUnits)
				{
					var segment = unit.Element(ns + "segment");
					var unitSource = segment.Element(ns + "source");
					var unitTarget = segment.Element(ns + "target");
					if (unitSource != null && unitSource.Nodes().OfType<XText>().Any())
					{
						var state = segment.Attribute("state")?.Value;
						if (state != null && !forStates.Contains(state))
						{
							continue;
						}

						if (state == null && unitTarget == null)
						{
							unitTarget = new XElement(ns + "target");
							segment.Add(unitTarget);
						}
						else
						{
							unitTarget.Nodes().Remove();
						}

						foreach (var n in unitSource.Nodes())
						{
							if (n.NodeType == System.Xml.XmlNodeType.Text)
							{
								var textNode = n as XText;
								try
								{
									var tr = translatedStrings[translatedIndex];
									translatedIndex++;
									unitTarget.Add(new XText(tr));
								}
								catch (HttpRequestException ex)
								{
									logger?.LogError(ex.Message);
									return countForUnit;
								}
							}
							else if (n.NodeType == System.Xml.XmlNodeType.Element)
							{
								unitTarget.Add(new XElement(n as XElement));
							}
						}

						if (toCreateTargetFile)
						{
							segment.SetAttributeValue("state", "initial");
						}

						if (!unchangeState)
						{
							segment.SetAttributeValue("state", "translated");
						}

						countForUnit++;

						progressDisplay?.Show(countForUnit, totalUnits, isAllNew, totalUnitsToTranslate);

					}
				}

				return countForUnit;
			}

		}

	}

}
