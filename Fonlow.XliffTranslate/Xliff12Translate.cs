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

		public async Task<int> TranslateXliff(XElement xliffRoot, string[] forStates, bool unchangeState, ITranslate g, ILogger logger, Action<bool, int, int, int> progressCallback)
		{
			var ver = xliffRoot.Attribute("version").Value;
			if (ver != "1.2")
			{
				throw new ArgumentException($"Expected XLIFF 1.2 but this one is {ver}");
			}

			var ns = xliffRoot.GetDefaultNamespace();
			var firstFile = xliffRoot.Element(ns + "file");
			var fileBody = firstFile.Element(ns + "body");

			if (string.IsNullOrEmpty(g.SourceLang))
			{
				g.SourceLang = firstFile.Attribute("source-language").Value; //use source file's source language
			}

			bool toCreateTargetFile = false;
			if (string.IsNullOrEmpty(g.TargetLang))
			{
				g.TargetLang = firstFile.Attribute("target-language")?.Value; //use source file 's target language
				if (string.IsNullOrEmpty(g.TargetLang))
				{
					throw new ArgumentException("TargetLang must be declared in command parameters or xliff/file/target-language");
				}
			}

			if (string.IsNullOrEmpty(firstFile.Attribute("target-language")?.Value))
			{
				firstFile.Add(new XAttribute("target-language", g.TargetLang)); //source file has no target lang declared, then use the commandline TargetLang
				toCreateTargetFile = true; // The rest of the codes need to add trans-unit/target
			}

			Console.WriteLine($"Translating from {g.SourceLang} to {g.TargetLang} ...");

			var body = firstFile.Element(ns + "body");
			var tranUnits = body.Elements(ns + "trans-unit").ToList();
			var totalUnits = tranUnits.Count;
			var totalUnitsToTranslate = tranUnits.Count(unit =>
			{
				var unitSource = unit.Element(ns + "source");
				var unitTarget = unit.Element(ns + "target");

				return unitSource != null
				&& unitSource.Nodes().OfType<XText>().Any()
					&& (unitTarget == null || forStates.Contains(unitTarget.Attribute("state")?.Value));
			});

			if (totalUnitsToTranslate < totalUnits) //some units are badly defined.
			{
				var badUnits = tranUnits.Where(unit =>
				{
					var unitSource = unit.Element(ns + "source");
					var unitTarget = unit.Element(ns + "target");

					return unitSource != null
					&& !unitSource.Nodes().OfType<XText>().Any(); ;
				});

				var ids = badUnits.Select(unit => unit.Attribute("id").Value).ToArray();
				var csv = string.Join(", ", ids);
				logger.LogWarning($"These units have nothing to translate: {csv}");
			}

			var isAllNew = totalUnits == totalUnitsToTranslate;
			Console.WriteLine(isAllNew ? $"Current / Total Units" : $"Current / Total Units To Translate / Total Units");
			int countForUnit = 0;

			const int maxUnits = 100; //not likely reaching the 1024 limit of batch processing because not likely the text have over 10 interpolations each.
			if (batchMode)
			{
				var chunks = tranUnits.SplitLists(maxUnits);
				int kc = 0;
				foreach (var chunk in chunks)
				{
					kc = await Batch(chunk); // always countsForUnit
				}

				return kc;
			}
			else
			{
				return await TextByText(tranUnits);
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
									var tr = await g.Translate(textNode.Value);
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

				var translatedStrings = await g.Translate(strings);

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

		public async Task<int> TranslateXliff(string filePath, string targetFile, string[] forStates, bool unchangeState, ITranslate g, ILogger logger, Action<bool, int, int, int> progressCallback)
		{
			XDocument xDoc;
			int c;
			using (FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				c = await TranslateXliff(xliffRoot, forStates, unchangeState, g, logger, progressCallback);
			}

			xDoc.Save(targetFile);
			return c;
		}
	}

}
