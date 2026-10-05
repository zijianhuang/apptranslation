using Fonlow.GoogleTranslate;
using Fonlow.GoogleTranslateV3;
using Google.Apis.Auth.OAuth2;
using System.Xml;
using System.Xml.Linq;
using Fonlow.XliffTranslate;

namespace TestXliff
{
	[TestClass(DisableParallelization = true)] // GT V3 does not so support parallel
	[Collection("ServicesLaunch")]
	public class Xliff20GT3Tests
	{
		string googleTranslateV3ClientSecretJsonFile = Environment.GetEnvironmentVariable("GoogleTranslateV3ClientSecretJsonFileForTest", EnvironmentVariableTarget.User); //Secrets\GoogleTranslate\client_secret_OpenSourceTest.json with project OpenSourceTest in Z account

		[Fact]
		public async Task TestReadAndTranslateWithV3()
		{
			using (FileStream fs = new System.IO.FileStream("xlf20/messages.zh-hans.xlf", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				var xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				var wg = new Xliff20Translate();
				var clientSecrets = GoogleClientSecrets.FromFile(googleTranslateV3ClientSecretJsonFile);
				var projectId = ClientSecretReader.ReadProjectId(googleTranslateV3ClientSecretJsonFile);
				var c = await wg.TranslateXliffElement(xliffRoot, ["initial"], false, new XWithGT3("en", "zh-hans", clientSecrets, projectId), null, null, false);
				Assert.Equal(3, c);

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

				Assert.Equal("诗中已不再包含一些已登记编号的注释：", (nodes[0] as XText).Value);
				Assert.Equal("你想删除它们吗？", (nodes[2] as XText).Value);

				var unit2 = units[2];
				var segment2 = unit2.Element(ns + "segment");
				var target2 = segment2.Element(ns + "target");
				var nodes2 = target2.Nodes().ToArray();
				Assert.Equal("家", (nodes2[0] as XText).Value);

				xDoc.Save("XdocumentTranslated20V3.xlf"); // check to ensure the order of nodes not changed.
			}
		}

		[Fact]
		public async Task TestReadAndTranslateWithV3AsHtmlLLM()
		{
			using (FileStream fs = new System.IO.FileStream("xlf20/messages.zh-hans.xlf", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				var xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				var wg = new Xliff20Translate();
				wg.SetAsHtml(true);
				var clientSecrets = GoogleClientSecrets.FromFile(googleTranslateV3ClientSecretJsonFile);
				var projectId = ClientSecretReader.ReadProjectId(googleTranslateV3ClientSecretJsonFile);
				var c = await wg.TranslateXliffElement(xliffRoot, ["initial"], false, new XWithGT3("en", "zh-hans", clientSecrets, projectId, "general/translation-llm"), null, null, false);
				Assert.Equal(3, c);

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

				Assert.Equal("有些已登记的编号注释在诗中已不复存在：", (nodes[0] as XText).Value);
				Assert.Equal(" . 你想移除它们吗？", (nodes[2] as XText).Value);
				var s = target.GetInnerXml();
				Assert.Equal("有些已登记的编号注释在诗中已不复存在：<ph id=\"0\" equiv=\"PH\" disp=\"numberList\" /> . 你想移除它们吗？", s);

				xDoc.Save("XdocumentTranslated20V3.xlf"); // check to ensure the order of nodes not changed.
			}
		}

		[Fact]
		public async Task TestReadAndTranslateWithV3Batch()
		{
			using (FileStream fs = new System.IO.FileStream("xlf20/messages.zh-hans.xlf", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				var xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				var wg = new Xliff20Translate();
				wg.SetBatchMode(true);
				var clientSecrets = GoogleClientSecrets.FromFile(googleTranslateV3ClientSecretJsonFile);
				var projectId = ClientSecretReader.ReadProjectId(googleTranslateV3ClientSecretJsonFile);
				var c = await wg.TranslateXliffElement(xliffRoot, ["initial"], false, new XWithGT3("en", "zh-hans", clientSecrets, projectId), null, null, false);
				Assert.Equal(3, c);

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

				Assert.Equal("诗中已不再包含一些已登记编号的注释：", (nodes[0] as XText).Value); //one less space with GT3 in batch. What happened to Google Translate v3?
				Assert.Equal("你想删除它们吗？", (nodes[2] as XText).Value);

				xDoc.Save("XdocumentTranslated20V3.xlf"); // check to ensure the order of nodes not changed.
			}
		}

		[Fact]
		public async Task TestReadAndTranslateWithV3BatchAsHtmlLLM()
		{
			using (FileStream fs = new System.IO.FileStream("xlf20/messages.zh-hans.xlf", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				var xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				var wg = new Xliff20Translate();
				wg.SetBatchMode(true);
				wg.SetAsHtml(true);
				var clientSecrets = GoogleClientSecrets.FromFile(googleTranslateV3ClientSecretJsonFile);
				var projectId = ClientSecretReader.ReadProjectId(googleTranslateV3ClientSecretJsonFile);
				var c = await wg.TranslateXliffElement(xliffRoot, ["initial"], false, new XWithGT3("en", "zh-hans", clientSecrets, projectId, "general/translation-llm"), null, null, false);
				Assert.Equal(3, c);

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

				Assert.Equal("有些已登记的编号注释在诗中已不复存在：", (nodes[0] as XText).Value);
				Assert.Equal(" . 你想移除它们吗？", (nodes[2] as XText).Value);
				var s = target.GetInnerXml();
				Assert.Equal("有些已登记的编号注释在诗中已不复存在：<ph id=\"0\" equiv=\"PH\" disp=\"numberList\" /> . 你想移除它们吗？", s);

				xDoc.Save("XdocumentTranslated20V3.xlf"); // check to ensure the order of nodes not changed.
			}
		}

		[Fact]
		public async Task ListGlossariesAndGet(){
			var clientSecrets = GoogleClientSecrets.FromFile(googleTranslateV3ClientSecretJsonFile);
			var projectId = ClientSecretReader.ReadProjectId(googleTranslateV3ClientSecretJsonFile);
			var gt = new XWithGT3("en", "zh-hans", clientSecrets, projectId);
			var glossaries = await gt.ListGlossaries();
			Assert.NotEmpty(glossaries);
			Assert.Equal("mstc-arabic-en-ar", glossaries[0].GlossaryName.GlossaryId);

			var glossary = await gt.GetGlossary("mstc-chinese-simplified-en-zh-hans");
			Assert.NotNull(glossary);
		}

		[Fact]
		public async Task ListGlossaryEntries(){
			var clientSecrets = GoogleClientSecrets.FromFile(googleTranslateV3ClientSecretJsonFile);
			var projectId = ClientSecretReader.ReadProjectId(googleTranslateV3ClientSecretJsonFile);
			var gt = new XWithGT3("en", "zh-Hans", clientSecrets, projectId, locationId: "us-central1", glossaryId: "mstc-chinese-simplified-en-zh-hans"); //zh-CN works too, while the glossary may be with zh-CN. The translation engine is smart enough to handle the difference, and fall back to the closest match.
			var entries = await gt.ListGlossaryEntries("mstc-chinese-simplified-en-zh-hans", 100);
			Assert.NotEmpty(entries);

			var rt = await gt.Translate("key roaming");
			Assert.Equal("关键漫游", rt);

			var rtg = await gt.TranslateWithGlossary("key roaming", "text/plain");
			Assert.Equal("密钥漫游", rtg);

			var rt2 = await gt.Translate("logging");
			Assert.Equal("日志记录", rt2);
			var rtg2 = await gt.TranslateWithGlossary("logging", "text/plain");
			Assert.Equal("事件日志", rtg2);

			var rt3 = await gt.Translate("Grab");
			Assert.Equal("抓住", rt3);
			var rtg3 = await gt.TranslateWithGlossary("Grab", "text/plain");
			Assert.Equal("抓取按钮", rtg3);

			var rt4 = await gt.Translate("Home");
			Assert.Equal("家", rt4);
			var rtg4 = await gt.TranslateWithGlossary("Home", "text/plain");
			Assert.Equal("主文件夹", rtg4);

			var rtg5 = await gt.TranslateWithGlossary("home", "text/plain");
			Assert.Equal("家", rtg5);

			//foreach (var s in new[] { "Home", "home", "HOME", "Home folder", "Open the Home folder.", "Home page", "Go Home" })
			//{
			//	var plain = await gt.Translate(s);
			//	var withGlossary = await gt.TranslateWithGlossary(s, "text/plain");
			//	Console.WriteLine($"{s,-24} plain={plain,-10} glossary={withGlossary}");   // or ITestOutputHelper in xUnit
			//}
		}

	}
}
