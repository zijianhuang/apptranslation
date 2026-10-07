using Fonlow.AndroidStrings;
using Fonlow.GoogleTranslate;
using Fonlow.StringsTranslate;
using Fonlow.Translate;
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
			var g = new StringsTranslation();
			g.SetSourceFile("strings/strings.xml");
			g.SetTargetFile("strings.zh-tw.xml");
			Assert.Equal(3, await g.Translate(new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseTraditional, apiKey), null, null));

			var reader = new StringsRW();
			reader.Load("strings.zh-tw.xml");
			var strings = reader.GetStrings();
			var first = strings.FirstOrDefault();
			Assert.NotNull(first);
			Assert.Equal("關於", first.Value);
		}

		[Fact]
		public void TestSingleTermHelper(){
			Assert.Equal("缺乏", SingleTermHelper.Normalize("Deficiency", "缺乏；不足；缺陷", "zh-CN"));   // 缺乏
			Assert.Equal("欠乏", SingleTermHelper.Normalize("Deficiency", "欠乏、不足、欠陥", "ja"));      // 欠乏
			Assert.Equal("결핍", SingleTermHelper.Normalize("Deficiency", "결핍, 부족, 결함", "ko"));      // 결핍
			Assert.Equal("Mangel", SingleTermHelper.Normalize("Deficiency", "Mangel; Defizit", "de"));      // Mangel
			Assert.Equal("运行（动词，名词）", SingleTermHelper.Normalize("Run", "运行（动词，名词）；跑", "zh-CN"));     // 运行（动词，名词）
			Assert.Equal("保存文件；存储文件", SingleTermHelper.Normalize("Save the file.", "保存文件；存储文件", "zh-CN")); // unchanged (sentence)
			Assert.False(SingleTermHelper.IsSingleTerm("System Languages: ", 1));
		}
	}
}
