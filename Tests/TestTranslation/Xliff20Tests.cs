using Fonlow.GoogleTranslate;
using Google.Cloud.Translation.V2;
using System.Xml.Linq;
using System.Xml;

namespace TestXliff
{
	[Collection("ServicesLaunch")]
	public class Xliff20Tests
	{
		string apiKey = System.Environment.GetEnvironmentVariable("GoogleTranslateApiKey", EnvironmentVariableTarget.User);

		[Fact]
		public void TestReadViaXElement()
		{
			using (FileStream fs = new System.IO.FileStream("xlf20/messages.zh-hans.xlf", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				var xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				var ns = xliffRoot.GetDefaultNamespace();
				var ver = xliffRoot.Attribute("version").Value;
				Assert.Equal("2.0", ver);
				Assert.Equal("en", xliffRoot.Attribute("srcLang").Value);
				Assert.Equal("zh-hans", xliffRoot.Attribute("trgLang").Value);
				var firstFile = xliffRoot.Element(ns + "file");

				var units = firstFile.Elements(ns + "unit").ToArray();
				Assert.NotNull(units);

				var unit = units[1];
				var segment = unit.Element(ns+"segment");
				Assert.Equal("initial", segment.Attribute("state").Value);
				var source = segment.Element(ns + "source");
				var nodes = source.Nodes().ToArray();
				Assert.Equal(3, nodes.Length);
				Assert.Equal(XmlNodeType.Text, nodes[0].NodeType);
				Assert.Equal(XmlNodeType.Element, nodes[1].NodeType);
				Assert.Equal(XmlNodeType.Text, nodes[2].NodeType);

				(nodes[0] as XText).Value += " Altered";
				(nodes[2] as XText).Value += " Altered";

				xDoc.Save("Xdocument20.xlf"); // check to ensure the order of nodes not changed.
			}
		}

		[Fact]
		public async Task TestReadAndTranslate()
		{
			using (FileStream fs = new System.IO.FileStream("xlf20/messages.zh-hans.xlf", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				var xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				var wg = new Xliff20Translate();
				var c = await wg.TranslateXliffElement(xliffRoot, ["initial"], false, new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseSimplified, apiKey), null, null);
				Assert.Equal(2, c);

				var ns = xliffRoot.GetDefaultNamespace();
				Assert.Equal("en", xliffRoot.Attribute("srcLang").Value);
				var firstFile = xliffRoot.Element(ns + "file");

				var units = firstFile.Elements(ns + "unit").ToArray();
				Assert.NotNull(units);

				var unit = units[1];
				var segment = unit.Element(ns + "segment");
				var target = segment.Element(ns + "target");
				var nodes = target.Nodes().ToArray();
				Assert.Equal(3, nodes.Length);
				Assert.Equal(XmlNodeType.Text, nodes[0].NodeType);
				Assert.Equal(XmlNodeType.Element, nodes[1].NodeType);
				Assert.Equal(XmlNodeType.Text, nodes[2].NodeType);

				Assert.Equal("有一些已注册的编号注释在诗中不再存在：", (nodes[0] as XText).Value);
				Assert.Equal("。您要删除它们吗？", (nodes[2] as XText).Value);

				xDoc.Save("XdocumentTranslated20.xlf"); // check to ensure the order of nodes not changed.
			}
		}

		[Fact]
		public async Task TestReadAndTranslateBatch()
		{
			using (FileStream fs = new System.IO.FileStream("xlf20/messages.zh-hans.xlf", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				var xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				var wg = new Xliff20Translate();
				wg.SetBatchMode(true);
				var c = await wg.TranslateXliffElement(xliffRoot, ["initial"], false, new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseSimplified, apiKey), null, null);
				Assert.Equal(2, c);

				var ns = xliffRoot.GetDefaultNamespace();
				Assert.Equal("en", xliffRoot.Attribute("srcLang").Value);
				var firstFile = xliffRoot.Element(ns + "file");

				var units = firstFile.Elements(ns + "unit").ToArray();
				Assert.NotNull(units);

				var unit = units[1];
				var segment = unit.Element(ns + "segment");
				var target = segment.Element(ns + "target");
				var nodes = target.Nodes().ToArray();
				Assert.Equal(3, nodes.Length);
				Assert.Equal(XmlNodeType.Text, nodes[0].NodeType);
				Assert.Equal(XmlNodeType.Element, nodes[1].NodeType);
				Assert.Equal(XmlNodeType.Text, nodes[2].NodeType);

				Assert.Equal("有一些已注册的编号注释在诗中不再存在：", (nodes[0] as XText).Value);
				Assert.Equal("。您要删除它们吗？", (nodes[2] as XText).Value);

				xDoc.Save("XdocumentTranslated20Batch.xlf"); // check to ensure the order of nodes not changed.
			}
		}

		[Fact]
		public async Task TestReadAndTranslateGroup()
		{
			using (FileStream fs = new System.IO.FileStream("xlf20/messagesGroup.zh-hans.xlf", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				var xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				var wg = new Xliff20Translate();
				wg.SetBatchMode(true);
				var c = await wg.TranslateXliffElement(xliffRoot, ["initial"], false, new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseSimplified, apiKey), null, null);
				Assert.Equal(2, c);

				xDoc.Save("XdocumentTranslated20Group.xlf"); // check to ensure the order of nodes not changed.
			}
		}
	}
}
