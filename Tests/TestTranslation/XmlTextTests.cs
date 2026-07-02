using Fonlow.GoogleTranslate;
using Fonlow.XmlTranslate;
using Fonlow.SvgTextTranslate;
using Google.Cloud.Translation.V2;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace TestXmlText
{
	[Collection("ServicesLaunch")]
	public class XmlTextTests
	{
		string apiKey = System.Environment.GetEnvironmentVariable("GoogleTranslateApiKey", EnvironmentVariableTarget.User);

		[Fact]
		public void TestReadStrings()
		{
			var xdoc = XElement.Load("svg/template1.svg");
			var nsManager = new XmlNamespaceManager(new NameTable());
			XNamespace rootElementNs = xdoc.Name.Namespace;
			nsManager.AddNamespace(xdoc.Name.LocalName, rootElementNs.NamespaceName);
			var r = XElementsHandler.SelectElementsByXPaths(xdoc, ["//svg:text/svg:tspan"], nsManager).ToArray();

			var first = r.FirstOrDefault();
			Assert.Equal("Measure distance visual acuity", first.Value);
			Assert.Equal(7, r.Length);
			//Assert.Equal("the About page of the product", first.comment);
		}

		[Fact]
		public async Task TestGoogleTranslateFileZh()
		{
			var g = new XmlTextTranslation();
			g.SetSourceFile("svg/template1.svg");
			g.SetTargetFile("template1.zh-tw.svg");
			g.SetXPaths(["//svg:text/svg:tspan"]);
			Assert.Equal(7, await g.Translate(new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseTraditional, apiKey), null, null));

			var r = SvgTextProcessor.ExtractTexts(File.ReadAllText("template1.zh-tw.svg"));
			var first = r.FirstOrDefault();
			Assert.Equal("tspan5", first.Key);
			Assert.Equal("測量遠距離視力", first.Text);
			Assert.Equal(7, r.Count);
		}

		[Fact]
		public async Task TestXElementWithXPath()
		{
			var xdoc = XElement.Load("svg/template1.svg");

			var nsManager = new XmlNamespaceManager(new NameTable());
			XNamespace rootElementNs = xdoc.Name.Namespace;
			nsManager.AddNamespace(xdoc.Name.LocalName, rootElementNs.NamespaceName);
			var textNodes = xdoc.XPathSelectElements("//svg:text/svg:tspan", nsManager).ToArray();
			Assert.True(textNodes.Length > 2);
		}

		[Fact]
		public async Task TestXElement()
		{
			var xdoc = XElement.Load("svg/template1.svg");

			XNamespace svg = xdoc.Name.Namespace;
			var tspans = xdoc.Descendants(svg + "tspan").ToArray();
			Assert.True(tspans.Length > 2);
		}

	}
}
