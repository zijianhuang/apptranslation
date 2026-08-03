using Fonlow.GoogleTranslate;
using Fonlow.SvgTextTranslate;
using Fonlow.TranslationProgram.Abstract;
using Fonlow.XmlTranslate;
using Google.Cloud.Translation.V2;
using Microsoft.CodeAnalysis;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace TestXmlText
{
	[Collection("ServicesLaunch")]
	[TestClass(DisableParallelization = true)] //some test cases write to the same file, so disable parallelization
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
		public async Task TestGoogleTranslateFileZhBatch()
		{
			var g = new XmlTextTranslation();
			g.SetSourceFile("svg/template1.svg");
			g.SetTargetFile("template1.zh-tw.svg");
			g.SetXPaths(["//svg:text/svg:tspan"]);
			g.SetBatchMode(true);
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

		[Fact]
		public void TestTranslationOptions()
		{
			var options = new Fonlow.TranslationProgram.OptionsForXmlWithGoogleTranslate();
			var parser = new Fonlow.Cli.CommandLineParser(options);
			parser.Parse("/AKF=../../Secrets/GoogleTranslate/apikey.txt /SL=en /TL=\"zh-hant\" /XPaths=`//svg:text/svg:tspan` /F=../Tests/TestTranslation/svg/template1.svg /TF=../Tests/TestTranslation/bin/template1.zh-Hant.svg", false);
			Assert.False(parser.HasErrors);
			var gOptions = options as Fonlow.TranslationProgram.OptionsForXmlWithGoogleTranslate;
			Assert.Equal(1, gOptions.XPaths.Length);
			Assert.Equal("//svg:text/svg:tspan", gOptions.XPaths[0]);
			Assert.Null(parser.ExecutablePath);
		}

		[Fact]
		public void TestTranslationOptionsBackTick()
		{
			OptionsBase options = new Fonlow.TranslationProgram.OptionsForXmlWithGoogleTranslate();
			var parser = new Fonlow.Cli.CommandLineParser(options);
			parser.Parse("/AKF=../../Secrets/GoogleTranslate/apikey.txt /SL=en /TL=zh-hant /XPaths=`abc/efgf` `//abc/efg` /F=../Tests/TestTranslation/svg/template1.svg /TF=../Tests/TestTranslation/bin/template1.zh-Hant.svg", false);
			Assert.False(parser.HasErrors);
			var gOptions = options as Fonlow.TranslationProgram.OptionsForXmlWithGoogleTranslate;
			Assert.Equal(2, gOptions.XPaths.Length);
			Assert.Equal("abc/efgf", gOptions.XPaths[0]);
			Assert.Equal("//abc/efg", gOptions.XPaths[1]);
			Assert.Null(parser.ExecutablePath);
		}


	}
}
