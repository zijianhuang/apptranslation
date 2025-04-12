using System.Xml.Linq;

namespace Fonlow.XliffResX
{
	public static class XliffResXConverter
	{
		public static XDocument ConvertResXToXliff12(XElement resxSourceRoot, XElement resxRoot, string sourceLang, string targetLang)
		{
			var sourceNodes = resxSourceRoot.Elements("data").ToList();
			var nodes = resxRoot.Elements("data").ToList();
			if (nodes.Count !=sourceNodes.Count)
			{
				throw new ArgumentException($"Expect {nameof(resxSourceRoot)} and {nameof(resxRoot)} have the same amount of data.");
			}

			var bodyElement = new XElement("body");
			var fileElement = new XElement("file", new XAttribute("source-language", sourceLang), new XAttribute("target-language", targetLang), new XAttribute("datatype", "plaintext"), new XAttribute("original", "resx"),
				bodyElement);
			XNamespace ns = "urn:oasis:names:tc:xliff:document:1.2";
			var xliffRoot = new XElement(ns + "xliff",
				new XAttribute("version", "1.2"),
				fileElement);

			for (int i = 0; i < sourceNodes.Count; i++)
			{	var srcNode = sourceNodes[i];
				var unit = new XElement("trans-unit", new XAttribute("id", srcNode.Attribute("name").Value), new XAttribute("datatype", "text"),
					new XElement("source", srcNode.Element("value").Value),
					new XElement("target", new XAttribute("state", "new"), nodes[i].Element("value").Value)
				);

				bodyElement.Add(unit);
			}

			var xliffDoc = new XDocument(xliffRoot);
			return xliffDoc;
		}
	}
}
