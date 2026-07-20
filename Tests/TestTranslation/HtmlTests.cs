using Fonlow.GoogleTranslate;
using Fonlow.HtmlTranslate;
using Google.Cloud.Translation.V2;


namespace TestHtml
{
	[Collection("ServicesLaunch")]
	public class HtmlTests
	{
		string apiKey = System.Environment.GetEnvironmentVariable("GoogleTranslateApiKey", EnvironmentVariableTarget.User);

		[Fact]
		public async Task TestGoogleTranslateNodesZh()
		{
			var g = new HtmlTranslation();
			g.SetSourceFile("html/startupHelp.html");
			g.SetTargetFile("startupHelp.zh-Hans.html");
			g.SetXPaths(["//h2", "ul"]);
			Assert.Equal(3, await g.Translate(new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseSimplified, apiKey), null, null));
		}

		[Fact]
		public async Task TestGoogleTranslateDocZhHant()
		{
			var g = new HtmlTranslation();
			g.SetSourceFile("html/startupHelp.html");
			g.SetTargetFile("startupHelp.zh-Hant.html");
			Assert.Equal(1, await g.Translate(new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseTraditional, apiKey), null, null));
		}

		//[Fact]
		//public async Task TestGoogleTranslateFileZhBatch()
		//{
		//	var g = new HtmlTranslation();
		//	g.SetSourceFile("svg/template1.svg");
		//	g.SetTargetFile("template1.zh-tw.svg");
		//	g.SetXPaths(["//svg:text/svg:tspan"]);
		//	g.SetBatchMode(true);
		//	Assert.Equal(7, await g.Translate(new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseTraditional, apiKey), null, null));

		//	var r = SvgTextProcessor.ExtractTexts(File.ReadAllText("template1.zh-tw.svg"));
		//	var first = r.FirstOrDefault();
		//	Assert.Equal("tspan5", first.Key);
		//	Assert.Equal("測量遠距離視力", first.Text);
		//	Assert.Equal(7, r.Count);
		//}

		//[Fact]
		//public void TestTranslationOptions()
		//{
		//	var options = new Fonlow.TranslationProgram.OptionsForXmlWithGoogleTranslate();
		//	var parser = new Fonlow.Cli.CommandLineParser(options);
		//	parser.Parse("/AKF=../../Secrets/GoogleTranslate/apikey.txt /SL=en /TL=\"zh-hant\" /XPaths=`//svg:text/svg:tspan` /F=../Tests/TestTranslation/svg/template1.svg /TF=../Tests/TestTranslation/bin/template1.zh-Hant.svg", false);
		//	Assert.False(parser.HasErrors);
		//	var gOptions = options as Fonlow.TranslationProgram.OptionsForXmlWithGoogleTranslate;
		//	Assert.Equal(1, gOptions.XPaths.Length);
		//	Assert.Equal("//svg:text/svg:tspan", gOptions.XPaths[0]);
		//	Assert.Null(parser.ExecutablePath);
		//}

		//[Fact]
		//public void TestTranslationOptionsBackTick()
		//{
		//	OptionsBase options = new Fonlow.TranslationProgram.OptionsForXmlWithGoogleTranslate();
		//	var parser = new Fonlow.Cli.CommandLineParser(options);
		//	parser.Parse("/AKF=../../Secrets/GoogleTranslate/apikey.txt /SL=en /TL=zh-hant /XPaths=`abc/efgf` `//abc/efg` /F=../Tests/TestTranslation/svg/template1.svg /TF=../Tests/TestTranslation/bin/template1.zh-Hant.svg", false);
		//	Assert.False(parser.HasErrors);
		//	var gOptions = options as Fonlow.TranslationProgram.OptionsForXmlWithGoogleTranslate;
		//	Assert.Equal(2, gOptions.XPaths.Length);
		//	Assert.Equal("abc/efgf", gOptions.XPaths[0]);
		//	Assert.Equal("//abc/efg", gOptions.XPaths[1]);
		//	Assert.Null(parser.ExecutablePath);
		//}


	}
}
