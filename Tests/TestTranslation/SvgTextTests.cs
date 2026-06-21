using Fonlow.GoogleTranslate;
using Google.Cloud.Translation.V2;
using Fonlow.SvgTextTranslate;

namespace TestStrings
{
	[Collection("ServicesLaunch")]
	public class SvgTextTests
	{
		string apiKey = System.Environment.GetEnvironmentVariable("GoogleTranslateApiKey", EnvironmentVariableTarget.User);

		[Fact]
		public void TestReadStrings()
		{
			var r = SvgTextProcessor.ExtractTexts(File.ReadAllText("svg/template1.svg"));
			var first = r.FirstOrDefault();
			Assert.Equal("tspan5", first.Key);
			Assert.Equal("Measure distance visual acuity", first.Text);
			Assert.Equal(7, r.Count);
			//Assert.Equal("the About page of the product", first.comment);
		}

		[Fact]
		public async Task TestGoogleTranslateFileZh()
		{
			var g = new SvgTextTranslation();
			g.SetSourceFile("svg/template1.svg");
			g.SetTargetFile("template1.zh-tw.svg");
			Assert.Equal(7, await g.Translate(new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseTraditional, apiKey), null, null));

			var r = SvgTextProcessor.ExtractTexts(File.ReadAllText("template1.zh-tw.svg"));
			var first = r.FirstOrDefault();
			Assert.Equal("tspan5", first.Key);
			Assert.Equal("測量遠距離視力", first.Text);
			Assert.Equal(7, r.Count);
		}
	}
}
