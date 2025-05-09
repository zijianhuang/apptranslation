using Fonlow.GoogleTranslate;
using Fonlow.MsTranslator;
using Google.Cloud.Translation.V2;
using System.Xml;
using System.Xml.Linq;
using Fonlow.XliffTranslate;

namespace TestXliff
{
	[Collection("ServicesLaunch")]
	public class Xliff12Tests
	{
		string apiKey = System.Environment.GetEnvironmentVariable("GoogleTranslateApiKey", EnvironmentVariableTarget.User);
		string msApiKey = System.Environment.GetEnvironmentVariable("MsTranslatorApiKey", EnvironmentVariableTarget.User);
		string msRegion = System.Environment.GetEnvironmentVariable("MsTranslatorRegion", EnvironmentVariableTarget.User);

		[Fact]
		public async Task TestGoogleTranslate()
		{
			var g = new XWithGT2("en", "zh-hans", apiKey);
			var t = await g.TranslateHtml("There are some registered numbered annotations not existing in poem anymore: <x id=\"PH\" equiv-text=\"numberList\"/>. Do you want to remove them?");
			Assert.Equal("有一些已注册的编号注释在诗中不再存在：<x id=\"PH\" equiv-text=\"numberList\"/> 。您要删除它们吗？", t);
		}

		[Fact]
		public async Task TestMsTranslator()
		{
			var g = new XWithMT("en", "zh-hans", msApiKey, msRegion);
			var t = await g.Translate("There are some registered numbered annotations not existing in poem anymore: <x id=\"PH\" equiv-text=\"numberList\"/>. Do you want to remove them?");
			Assert.Equal("诗歌中不再存在一些已注册的编号注释：<x id=“PH” equiv-text=“numberList”/>。是否要删除它们？", t);
		}

		[Fact]
		public async Task TestMsTranslatorWithArray()
		{
			var g = new XWithMT("en", "zh-hans", msApiKey, msRegion, "general");
			string[] ss = { "There are some registered numbered annotations not existing in poem anymore: <x id=\"PH\" equiv-text=\"numberList\"/>. Do you want to remove them?", "About" };
			var t = await g.Translate(ss);
			Assert.Equal("诗歌中不再存在一些已注册的编号注释：<x id=“PH” equiv-text=“numberList”/>。是否要删除它们？", t[0]);
			Assert.Equal("大约", t[1]); //not good
		}

		[Fact]
		public void TestReadViaXElement()
		{
			using (FileStream fs = new System.IO.FileStream("xlf12/messages.zh-hans.xlf", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				var xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				var ns = xliffRoot.GetDefaultNamespace();
				var firstFile = xliffRoot.Element(ns + "file");
				Assert.Equal("en-US", firstFile.Attribute("source-language").Value);
				var body = firstFile.Element(ns + "body");
				Assert.NotNull(body);

				var units = body.Elements(ns + "trans-unit").ToArray();
				Assert.NotNull(units);

				var unit = units[1];
				var source = unit.Element(ns + "source");
				var nodes = source.Nodes().ToArray();
				Assert.Equal(3, nodes.Length);
				Assert.Equal(XmlNodeType.Text, nodes[0].NodeType);
				Assert.Equal(XmlNodeType.Element, nodes[1].NodeType);
				Assert.Equal(XmlNodeType.Text, nodes[2].NodeType);

				(nodes[0] as XText).Value += " Altered";
				(nodes[2] as XText).Value += " Altered";

				xDoc.Save("Xdocument.xlf"); // check to ensure the order of nodes not changed.
			}
		}

		[Fact]
		public void TestReadViaXElementWithGroup()
		{
			using (FileStream fs = new System.IO.FileStream("xlf12/Fonlow.VA.Languages.zh-Hans.xlf", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				var xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				var ns = xliffRoot.GetDefaultNamespace();
				var firstFile = xliffRoot.Element(ns + "file");
				Assert.Equal("en-US", firstFile.Attribute("source-language").Value);
				var body = firstFile.Element(ns + "body");
				Assert.NotNull(body);

				var units = body.Elements(ns + "trans-unit").ToArray();
				Assert.NotNull(units);
				Assert.Empty(units);
				
				var group = body.Elements(ns + "group");
				units = group.Elements(ns + "trans-unit").ToArray();
				var unit = units[1];
				unit.Element(ns + "target").Attribute("state").Value = "translated";

				xDoc.Save("XdocumentWithGroup.xlf"); // check to ensure the order of nodes not changed.
			}
		}

		[Fact]
		public async Task TestReadAndTranslate()
		{
			using (FileStream fs = new System.IO.FileStream("xlf12/messages.zh-hans.xlf", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				var xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				var wg = new Xliff12Translate();
				var c = await wg.TranslateXliffElement(xliffRoot, ["new"], false, new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseSimplified, apiKey), null, null);
				Assert.Equal(1, c);

				var ns = xliffRoot.GetDefaultNamespace();
				var firstFile = xliffRoot.Element(ns + "file");
				Assert.Equal("en-US", firstFile.Attribute("source-language").Value);
				var body = firstFile.Element(ns + "body");
				Assert.NotNull(body);

				var units = body.Elements(ns + "trans-unit").ToArray();
				Assert.NotNull(units);

				var unit = units[1];
				var target = unit.Element(ns + "target");
				var nodes = target.Nodes().ToArray();
				Assert.Equal(3, nodes.Length);
				Assert.Equal(XmlNodeType.Text, nodes[0].NodeType);
				Assert.Equal(XmlNodeType.Element, nodes[1].NodeType);
				Assert.Equal(XmlNodeType.Text, nodes[2].NodeType);

				Assert.Equal("有一些已注册的编号注释在诗中不再存在：", (nodes[0] as XText).Value);
				Assert.Equal("。您要删除它们吗？", (nodes[2] as XText).Value);

				xDoc.Save("XdocumentTranslated.xlf"); // check to ensure the order of nodes not changed.
			}
		}

		[Fact]
		public async Task TestReadAndTranslateWithTranslateNo()
		{
			using (FileStream fs = new System.IO.FileStream("xlf12/messagesTranslate.zh-hans.xlf", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				var xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				var wg = new Xliff12Translate();
				var c = await wg.TranslateXliffElement(xliffRoot, ["new"], false, new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseSimplified, apiKey), null, null);
				Assert.Equal(1, c);

				xDoc.Save("XdocumentTranslatedSomeNot.xlf"); // check to ensure the order of nodes not changed.
			}
		}

		[Fact]
		public async Task TestReadAndTranslateWithBatchMode()
		{
			using (FileStream fs = new System.IO.FileStream("xlf12/messages.zh-hans.xlf", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				var xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				var wg = new Xliff12Translate();
				wg.SetBatchMode(true);
				var c = await wg.TranslateXliffElement(xliffRoot, ["new"], false, new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseSimplified, apiKey), null, null);
				Assert.Equal(1, c);

				var ns = xliffRoot.GetDefaultNamespace();
				var firstFile = xliffRoot.Element(ns + "file");
				Assert.Equal("en-US", firstFile.Attribute("source-language").Value);
				var body = firstFile.Element(ns + "body");
				Assert.NotNull(body);

				var units = body.Elements(ns + "trans-unit").ToArray();
				Assert.NotNull(units);

				var unit = units[1];
				var target = unit.Element(ns + "target");
				var nodes = target.Nodes().ToArray();
				Assert.Equal(3, nodes.Length);
				Assert.Equal(XmlNodeType.Text, nodes[0].NodeType);
				Assert.Equal(XmlNodeType.Element, nodes[1].NodeType);
				Assert.Equal(XmlNodeType.Text, nodes[2].NodeType);

				Assert.Equal("有一些已注册的编号注释在诗中不再存在：", (nodes[0] as XText).Value);
				Assert.Equal("。您要删除它们吗？", (nodes[2] as XText).Value);

				xDoc.Save("XdocumentTranslated.xlf"); // check to ensure the order of nodes not changed.
			}
		}

		[Fact]
		public async Task TestReadAndTranslateWithGroup()
		{
			using (FileStream fs = new System.IO.FileStream("xlf12/Fonlow.VA.Languages.zh-Hans.xlf", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				var xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				var wg = new Xliff12Translate();
				wg.SetBatchMode(true);
				var c = await wg.TranslateXliffElement(xliffRoot, ["new"], false, new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseSimplified, apiKey), null, null);
				Assert.Equal(10, c);

				var ns = xliffRoot.GetDefaultNamespace();
				var firstFile = xliffRoot.Element(ns + "file");
				Assert.Equal("en-US", firstFile.Attribute("source-language").Value);
				var body = firstFile.Element(ns + "body");
				Assert.NotNull(body);

				xDoc.Save("XdocumentWithGroupTranslated.xlf"); // check to ensure the order of nodes not changed.
			}
		}



	}
}
