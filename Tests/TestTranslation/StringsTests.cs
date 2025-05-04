using Fonlow.AndroidStrings;
using Fonlow.GoogleTranslate;
using Google.Cloud.Translation.V2;

namespace TestStrings
{
	[Collection("ServicesLaunch")]
	public class StringsTests
	{
		string apiKey = System.Environment.GetEnvironmentVariable("GoogleTranslateApiKey", EnvironmentVariableTarget.User);

		[Fact]
		public void TestReadStrings()
		{
			var reader = new StringsRW();
			reader.Load("strings/strings.xml");
			var strings = reader.GetStrings();
			var first = strings.FirstOrDefault();
			Assert.NotNull(first);
			Assert.Equal("About", first.name);
			Assert.Equal("About", first.Value);
			Assert.Equal("the About page of the product", first.comment);
		}

		[Fact]
		public async Task TestGoogleTranslateFileZh(){
			var g = new StringsTranslate();
			Assert.Equal(3, await g.Translate("strings/strings.xml", "strings.zh-tw.xml", new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseTraditional, apiKey), null, null));

			var reader = new StringsRW();
			reader.Load("strings.zh-tw.xml");
			var strings = reader.GetStrings();
			var first = strings.FirstOrDefault();
			Assert.NotNull(first);
			Assert.Equal("關於", first.Value);
		}
	}
}
