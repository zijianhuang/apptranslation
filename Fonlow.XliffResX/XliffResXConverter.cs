using Microsoft.Extensions.Logging;
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
		/// <param name="resxRoot"></param>
		/// <param name="xliffRoot"></param>
		/// <param name="logger"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static Tuple<int, int> MergeResXToXliff12(XElement resxSourceRoot, XElement resxRoot, XElement xliffRoot, ILogger logger)
		{
			var ver = xliffRoot.Attribute("version").Value;
			if (ver != "1.2")
			{
				throw new ArgumentException($"Expected XLIFF 1.2 but this one is {ver}");
			}

			var sourceDataElements = resxSourceRoot.Elements("data").ToList();
			var dataElements = resxRoot.Elements("data").ToList();
			if (dataElements.Count != sourceDataElements.Count)
			{
				throw new ArgumentException($"Expect {nameof(resxSourceRoot)} and {nameof(resxRoot)} have the same amount of data.");
			}

			var ns = xliffRoot.GetDefaultNamespace();
			var firstFile = xliffRoot.Element(ns + "file");
			var fileBody = firstFile.Element(ns + "body");
			var transUnits = fileBody.Elements(ns + "trans-unit").ToList();

			int addedCount = 0;
			for (int i = 0; i < sourceDataElements.Count; i++) // add new units
			{
				var srcNode = sourceDataElements[i];
				var id = srcNode.Attribute("name").Value;
				var foundTransUnit = transUnits.Find(d => d.Attribute("id").Value == id);
				if (foundTransUnit == null)
				{
					var newId = srcNode.Attribute("name").Value;
					var unit = new XElement(ns + "trans-unit", new XAttribute("id", newId), new XAttribute("datatype", "text"),
						new XElement(ns + "source", srcNode.Element("value").Value),
						new XElement(ns + "target", new XAttribute("state", "new"), dataElements[i].Element("value").Value)
					);

					fileBody.Add(unit);
					addedCount++;
					logger.LogInformation($"Added: {newId}");
				}
			}

			int removedCount = 0;
			var transUnitsNow = fileBody.Elements(ns + "trans-unit").ToList(); // probably with some new units now
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
	}
}
