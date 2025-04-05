using Fonlow.GoogleTranslate;
using Fonlow.GoogleTranslateV3;
using Google.Apis.Auth.OAuth2;
using System.Xml;
using System.Xml.Linq;

namespace TestXliff
{
	[Collection("ServicesLaunch")]
	public class Xliff20GT3Tests
	{
		string googleTranslateV3ClientSecretJsonFile = Environment.GetEnvironmentVariable("GoogleTranslateV3ClientSecretJsonFile", EnvironmentVariableTarget.User);

		[Fact]
		public void TestReadClientSecretFile()
		{
			var projectId = ClientSecretReader.ReadProjectId(googleTranslateV3ClientSecretJsonFile);
			Assert.Equal("api-9214668015421348345-252306", projectId);
		}

		[Fact]
		public async Task TestReadAndTranslateWithV3()
		{
			using (FileStream fs = new System.IO.FileStream("xlf20/messages.zh-hans.xlf", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				var xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				var wg = new Xliff20Translate(false);
				var clientSecrets = GoogleClientSecrets.FromFile(googleTranslateV3ClientSecretJsonFile);
				var projectId = ClientSecretReader.ReadProjectId(googleTranslateV3ClientSecretJsonFile);
				var c = await wg.TranslateXliff(xliffRoot, ["initial"], false, new XWithGT3("en", "zh-hans", clientSecrets, projectId), null, null);
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

				Assert.Equal("有一些已注册的编号注释在诗中不再存在： ", (nodes[0] as XText).Value);
				Assert.Equal("。要删除它们吗？", (nodes[2] as XText).Value);

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
				var wg = new Xliff20Translate(true);
				var clientSecrets = GoogleClientSecrets.FromFile(googleTranslateV3ClientSecretJsonFile);
				var projectId = ClientSecretReader.ReadProjectId(googleTranslateV3ClientSecretJsonFile);
				var c = await wg.TranslateXliff(xliffRoot, ["initial"], false, new XWithGT3("en", "zh-hans", clientSecrets, projectId), null, null);
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

				Assert.Equal("有一些已注册的编号注释在诗中不再存在：", (nodes[0] as XText).Value); //one less space with GT3 in batch. What happened to Google Translate v3?
				Assert.Equal("。要删除它们吗？", (nodes[2] as XText).Value);

				xDoc.Save("XdocumentTranslated20V3.xlf"); // check to ensure the order of nodes not changed.
			}
		}

	}
}
