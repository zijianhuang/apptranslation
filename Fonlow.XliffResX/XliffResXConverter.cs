using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Fonlow.XliffResX
{
	public static class XliffResXConverter
	{
		/// <summary>
		/// Convert ResX to new Xliff v1.2
		/// </summary>
		/// <param name="resxSourceRoot">source ResX</param>
		/// <param name="resxRoot">Target language ResX</param>
		/// <param name="sourceLang"></param>
		/// <param name="targetLang"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">Throw when the total data nodes of the source and the lang one are not equal.</exception>
		public static XDocument ConvertResXToXliff12(XElement resxSourceRoot, XElement resxRoot, string sourceLang, string targetLang)
		{
			var sourceDataElements = resxSourceRoot.Elements("data").ToList();
			var dataElements = resxRoot.Elements("data").ToList();
			if (dataElements.Count != sourceDataElements.Count)
			{
				throw new ArgumentException($"Expect {nameof(resxSourceRoot)} and {nameof(resxRoot)} have the same amount of data.");
			}

			XNamespace ns = "urn:oasis:names:tc:xliff:document:1.2";
			var bodyElement = new XElement(ns + "body");
			var fileElement = new XElement(ns + "file", new XAttribute("source-language", sourceLang), new XAttribute("target-language", targetLang), new XAttribute("datatype", "plaintext"), new XAttribute("original", "resx"),
				bodyElement);
			var xliffRoot = new XElement(ns + "xliff",
				new XAttribute("version", "1.2"),
				fileElement);

			for (int i = 0; i < sourceDataElements.Count; i++)
			{
				var srcNode = sourceDataElements[i];
				var unit = new XElement(ns + "trans-unit", new XAttribute("id", srcNode.Attribute("name").Value), new XAttribute("datatype", "text"),
					new XElement(ns + "source", srcNode.Element("value").Value),
					new XElement(ns + "target", new XAttribute("state", "new"), dataElements[i].Element("value").Value)
				);

				bodyElement.Add(unit);
			}

			var xliffDoc = new XDocument(xliffRoot);
			return xliffDoc;
		}

		public static void ConvertResXToXliff12(string resxSourcePath, string resxPath, string sourceLang, string targetLang, string xliffPath)
		{
			var resxSourceRoot = XDocument.Load(resxSourcePath).Root;
			var resxRoot = XDocument.Load(resxPath).Root;
			var xliffDoc = ConvertResXToXliff12(resxSourceRoot, resxRoot, sourceLang, targetLang);
			xliffDoc.Save(xliffPath);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="resxSourceRoot"></param>
		/// <param name="resxLangRoot"></param>
		/// <param name="xliffRoot"></param>
		/// <param name="logger"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static Tuple<int, int> MergeResXToXliff12(XElement resxSourceRoot, XElement resxLangRoot, XElement xliffRoot, ILogger logger)
		{
			var ver = xliffRoot.Attribute("version").Value;
			if (ver != "1.2")
			{
				throw new ArgumentException($"Expected XLIFF 1.2 but this one is {ver}");
			}

			var sourceDataElements = resxSourceRoot.Elements("data").ToList();
			var langDataElements = resxLangRoot.Elements("data").ToList();
			if (langDataElements.Count > sourceDataElements.Count)
			{
				throw new ArgumentException($"Expect {nameof(resxLangRoot)} has equal or less number of data nodes than {nameof(resxSourceRoot)}.");
				/*If the lang ResX has been through translation one by one and some of them missing, the lang ResX will have less nodes than the source RexX because the ResX Editor/Manager has such behavior.
				 * Those missing elements in lang Resx are to be assuming the soure element data.
				 * However, in batch processing with machine translation, it is better not to leave the data element undefined in the lang resx, but to fill  in the value in source resx.
				 */
			}

			var ns = xliffRoot.GetDefaultNamespace();
			var firstFile = xliffRoot.Element(ns + "file");
			var fileBody = firstFile.Element(ns + "body");
			var transUnits = fileBody.Elements(ns + "trans-unit").ToList();
			var firstGroup = fileBody.Element(ns + "group"); //handle one group for now
			if (firstGroup != null)
			{
				var groupUnits = firstGroup.Elements(ns + "trans-unit").Where(d => d.Attribute("translate") == null || d.Attribute("translate").Value == "yes").ToList();
				transUnits.AddRange(groupUnits);
			}

			logger.LogInformation($"Soure ResX has {sourceDataElements.Count} data elements");
			logger.LogInformation($"Lang ResX has {langDataElements.Count} data elements");
			logger.LogInformation($"Merge target XLIFF has {transUnits.Count} units");

			int addedCount = 0;
			for (int i = 0; i < sourceDataElements.Count; i++) // add new units
			{
				var sourceDataElement = sourceDataElements[i];
				var id = sourceDataElement.Attribute("name").Value;
				var langDataElement = langDataElements.Find(d => d.Attribute("name")?.Value == id);
				var foundTransUnit = transUnits.Find(d => d.Attribute("id").Value == id);
				if (foundTransUnit == null)
				{
					var newId = sourceDataElement.Attribute("name").Value;
					var unit = langDataElement == null ? new XElement(ns + "trans-unit", new XAttribute("id", newId), new XAttribute("datatype", "text"),
						new XElement(ns + "source", sourceDataElement.Element("value").Value),
						new XElement(ns + "target", new XAttribute("state", "new"), string.Empty)
					)
					: new XElement(ns + "trans-unit", new XAttribute("id", newId), new XAttribute("datatype", "text"),
						new XElement(ns + "source", sourceDataElement.Element("value").Value),
						new XElement(ns + "target", new XAttribute("state", "new"), langDataElement.Element("value").Value)
					);

					if (firstGroup != null)
					{
						firstGroup.Add(unit);
					}
					else
					{
						fileBody.Add(unit);
					}

					addedCount++;
					logger.LogInformation($"Added Id: {newId}");
				}
			}

			int removedCount = 0;
			var transUnitsNow = fileBody.Elements(ns + "trans-unit").ToList();
			var firstGroupNow = fileBody.Element(ns + "group"); //handle one group for now
			if (firstGroupNow != null)
			{
				var groupUnits = firstGroupNow.Elements(ns + "trans-unit").Where(d => d.Attribute("translate") == null || d.Attribute("translate").Value == "yes").ToList();
				transUnitsNow.AddRange(groupUnits);
			}


			foreach (var n in transUnitsNow) //Purge
			{
				var id = n.Attribute("id").Value;
				var foundDataNode = sourceDataElements.Find(d => d.Attribute("name").Value == id);
				if (foundDataNode == null)
				{
					n.Remove();
					removedCount++;
					logger.LogInformation($"Removed: {id}");
				}
			}

			return Tuple.Create(addedCount, removedCount);

		}

		public static Tuple<int, int> MergeResXToXliff12(string resxSourcePath, string resxPath, string xliffPath, ILogger logger)
		{
			var resxSourceRoot = XDocument.Load(resxSourcePath).Root;
			var resxRoot = XDocument.Load(resxPath).Root;
			var xliffRoot = XDocument.Load(xliffPath).Root;
			var r = MergeResXToXliff12(resxSourceRoot, resxRoot, xliffRoot, logger);
			xliffRoot.Save(xliffPath);
			return r;
		}

		/// <summary>
		/// Copy the translated content of XLIFF to target language resX. Presumbly the XLIFF file is created from the resX or has been merged with the updated resX.
		/// </summary>
		/// <param name="xliffRoot"></param>
		/// <param name="resxLangRoot"></param>
		/// <exception cref="ArgumentException"></exception>
		public static void MergeTranslationOfXliff12BackToResX(XElement xliffRoot, XElement resxLangRoot)
		{
			var ver = xliffRoot.Attribute("version").Value;
			if (ver != "1.2")
			{
				throw new ArgumentException($"Expected XLIFF 1.2 but this one is {ver}");
			}

			var ns = xliffRoot.GetDefaultNamespace();
			var firstFile = xliffRoot.Element(ns + "file");
			var fileBody = firstFile.Element(ns + "body");
			var body = firstFile.Element(ns + "body");
			var transUnits = body.Elements(ns + "trans-unit").ToList();
			var firstGroup = fileBody.Element(ns + "group"); //handle one group for now
			if (firstGroup != null)
			{
				var groupUnits = firstGroup.Elements(ns + "trans-unit").Where(d => d.Attribute("translate") == null || d.Attribute("translate").Value == "yes").ToList();
				transUnits.AddRange(groupUnits);
			}

			var dataElements = resxLangRoot.Elements("data").ToList();
			if (dataElements.Count != transUnits.Count)
			{
				throw new ArgumentException($"Expect units of {nameof(xliffRoot)} and data of {nameof(resxLangRoot)} must match.");
			}

			foreach (var unit in transUnits)
			{
				var id = unit.Attribute("id").Value;
				var unitTarget = unit.Element(ns + "target");
				if (unitTarget == null)
				{
					throw new ArgumentException($"unit target of {id} has no target. XLIFF is malformed.");
				}

				var found = dataElements.Find(d => d.Attribute("name").Value == id);
				if (found == null)
				{
					throw new ArgumentException($"ResX does not contain a data node matching unit id {id}");
				}

				found.Element("value").Value = unitTarget.Value;
			}
		}

		public static void MergeTranslationOfXliff12ToResX(string xliffPath, string resxPath)
		{
			var xliffRoot = XDocument.Load(xliffPath).Root;
			var resxRoot = XDocument.Load(resxPath).Root;
			MergeTranslationOfXliff12BackToResX(xliffRoot, resxRoot);
			resxRoot.Save(resxPath);
		}
	}
}
