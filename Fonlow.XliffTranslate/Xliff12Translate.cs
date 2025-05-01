using Fonlow.Translate;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace Fonlow.GoogleTranslate
{
	/// <summary>
	/// XLIFF 1.2 with translation engine.
	/// </summary>
	public class Xliff12Translate : IXliffTranslation
	{
		public Xliff12Translate(bool batchMode)
		{
			this.batchMode = batchMode;
		}

		readonly bool batchMode;

		public async Task<int> TranslateXliff(XElement xliffRoot, string[] forStates, bool unchangeState, ITranslate translator, ILogger logger, Action<bool, int, int, int> progressCallback)
		{
			var ver = xliffRoot.Attribute("version").Value;
			if (ver != "1.2")
			{
				throw new ArgumentException($"Expected XLIFF 1.2 but this one is {ver}");
			}

			var ns = xliffRoot.GetDefaultNamespace();
			var fileElements = xliffRoot.Elements(ns + "file");

			var total = 0;
			foreach (var fileElement in fileElements)
			{
				var c = await TranslateXliffFileElement(fileElement, ns, forStates, unchangeState, translator, logger, progressCallback).ConfigureAwait(false);
				total += c;
			}
			return total;
		}

		public async Task<int> TranslateXliff(string filePath, string targetFile, string[] forStates, bool unchangeState, ITranslate translator, ILogger logger, Action<bool, int, int, int> progressCallback)
		{
			XDocument xDoc;
			int c;
			using (FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				c = await TranslateXliff(xliffRoot, forStates, unchangeState, translator, logger, progressCallback).ConfigureAwait(false);
			}

			xDoc.Save(targetFile);
			return c;
		}

		async Task<int> TranslateXliffFileElement(XElement fileElement, XNamespace ns, string[] forStates, bool unchangeState, ITranslate translator, ILogger logger, Action<bool, int, int, int> progressCallback)
		{
			var fileBody = fileElement.Element(ns + "body");

			if (string.IsNullOrEmpty(translator.SourceLang))
			{
				translator.SourceLang = fileElement.Attribute("source-language").Value; //use source file's source language
			}

			bool toCreateTargetFile = false;
			if (string.IsNullOrEmpty(translator.TargetLang))
			{
				translator.TargetLang = fileElement.Attribute("target-language")?.Value; //use source file 's target language
				if (string.IsNullOrEmpty(translator.TargetLang))
				{
					throw new ArgumentException("TargetLang must be declared in command parameters or xliff/file/target-language");
				}
			}

			if (string.IsNullOrEmpty(fileElement.Attribute("target-language")?.Value))
			{
				fileElement.Add(new XAttribute("target-language", translator.TargetLang)); //source file has no target lang declared, then use the commandline TargetLang
				toCreateTargetFile = true; // The rest of the codes need to add trans-unit/target
			}

			Console.WriteLine($"\nTranslating from {translator.SourceLang} to {translator.TargetLang} ...");

			var body = fileElement.Element(ns + "body");
			var transUnits = body.Elements(ns + "trans-unit").Where(d => d.Attribute("translate") == null || d.Attribute("translate").Value == "yes").ToList();
			var firstGroup = body.Element(ns + "group"); //handle one group for now
			if (firstGroup != null)
			{
				var groupUnits = firstGroup.Elements(ns + "trans-unit").Where(d => d.Attribute("translate") == null || d.Attribute("translate").Value == "yes").ToList();
				transUnits.AddRange(groupUnits);
			}

			var totalUnits = transUnits.Count;
			var totalUnitsToTranslate = transUnits.Count(unit =>
			{
				var unitSource = unit.Element(ns + "source");
				var unitTarget = unit.Element(ns + "target");

				return unitSource != null
				&& unitSource.Nodes().OfType<XText>().Any()
					&& (unitTarget == null || forStates.Contains(unitTarget.Attribute("state")?.Value) || string.IsNullOrEmpty(unitTarget.Attribute("state")?.Value));
			});

			if (totalUnitsToTranslate < totalUnits) //some units are badly defined.
			{
				var badUnits = transUnits.Where(unit =>
				{
					var unitSource = unit.Element(ns + "source");
					var unitTarget = unit.Element(ns + "target");

					return unitSource != null
					&& !unitSource.Nodes().OfType<XText>().Any(); ;
				});

				var ids = badUnits.Select(unit => unit.Attribute("id").Value).ToArray();
				var csv = string.Join(", ", ids);
				logger?.LogWarning($"These units have nothing to translate: {csv}");
			}

			var isAllNew = totalUnits == totalUnitsToTranslate;
			Console.WriteLine(isAllNew ? $"Current / Total Units" : $"Current / Total Units To Translate / Total Units");
			int countForUnit = 0;

			const int maxUnits = 100; //not likely reaching the 1024 limit of batch processing because not likely the text have over 10 interpolations each.
			if (batchMode)
			{
				var chunks = transUnits.SplitLists(maxUnits);
				int kc = 0;
				foreach (var chunk in chunks)
				{
					kc = await Batch(chunk).ConfigureAwait(false); // always countsForUnit
				}

				return kc;
			}
			else
			{
				return await TextByText(transUnits).ConfigureAwait(false);
			}

			async Task<int> TextByText(IList<XElement> someUnits)
			{
				foreach (var unit in someUnits)
				{
					var unitSource = unit.Element(ns + "source");
					var unitTarget = unit.Element(ns + "target");
					if (unitSource != null && unitSource.Nodes().OfType<XText>().Any())
					{
						var state = unitTarget?.Attribute("state")?.Value;
						if (state != null && !forStates.Contains(state))
						{
							continue;
						}


						if (state == null && unitTarget == null)
						{
							unitTarget = new XElement(ns + "target");
							unit.Add(unitTarget);
						}
						else
						{
							unitTarget.Nodes().Remove(); // so keep the attributes
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
							unitTarget.SetAttributeValue("state", "new");
						}

						if (!unchangeState)
						{
							unitTarget.SetAttributeValue("state", "translated");
						}

						countForUnit++;

						progressCallback?.Invoke(isAllNew, countForUnit, totalUnits, totalUnitsToTranslate);
					}
				}

				return countForUnit;

			}

			async Task<int> Batch(IList<XElement> someUnits)
			{
				var strings = new List<string>();
				foreach (var unit in someUnits)
				{
					var unitSource = unit.Element(ns + "source");
					var unitTarget = unit.Element(ns + "target");
					if (unitSource != null && unitSource.Nodes().OfType<XText>().Any())
					{
						var state = unitTarget?.Attribute("state")?.Value;
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

				var translatedStrings = await translator.Translate(strings).ConfigureAwait(false); //batch translation

				int translatedIndex = 0;
				foreach (var unit in someUnits)
				{
					var unitSource = unit.Element(ns + "source");
					var unitTarget = unit.Element(ns + "target");
					if (unitSource != null && unitSource.Nodes().OfType<XText>().Any())
					{
						var state = unitTarget?.Attribute("state")?.Value;
						if (state != null && !forStates.Contains(state))
						{
							continue;
						}


						if (state == null && unitTarget == null)
						{
							unitTarget = new XElement(ns + "target");
							unit.Add(unitTarget);
						}
						else
						{
							unitTarget.Nodes().Remove(); // so keep the attributes
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
							unitTarget.SetAttributeValue("state", "new");
						}

						if (!unchangeState)
						{
							unitTarget.SetAttributeValue("state", "translated");
						}

						countForUnit++;

						progressCallback?.Invoke(isAllNew, countForUnit, totalUnits, totalUnitsToTranslate);
					}
				}

				return countForUnit;
			}
		}


	}

}
