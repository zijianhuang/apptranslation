using Fonlow.GoogleTranslate;
using Fonlow.XliffResX;
using Google.Cloud.Translation.V2;
using Microsoft.Extensions.Logging.Abstractions;
using System.Xml.Linq;

namespace TestResxXliffConvert
{
	[Collection("ServicesLaunch")]
	public class ResxXliffTests
	{
		string apiKey = System.Environment.GetEnvironmentVariable("GoogleTranslateApiKey", EnvironmentVariableTarget.User);

		[Fact]
		public async Task TestMergeZhHanT()
		{
			var r = XliffResXConverter.MergeResXToXliff12("ResxXliff/AppResources.resx", "ResxXliff/AppResources.zh-Hant.resx", "ResxXliff/MultilingualResources/Fonlow.VA.Languages.zh-Hant.xlf", NullLogger.Instance);
			Assert.Equal(1, r.Item1);
			Assert.Equal(166, r.Item2);

			XDocument xDoc;
			using (FileStream fs = new System.IO.FileStream("ResxXliff/MultilingualResources/Fonlow.VA.Languages.zh-Hant.xlf", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				var wg = new Xliff12Translate(true);
				var c = await wg.TranslateXliff(xliffRoot, ["new"], false, new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseTraditional, apiKey), null, null);
				Assert.Equal(4, c);
			}

			xDoc.Save("ResxXliff/MultilingualResources/Fonlow.VA.Languages.zh-Hant.xlf"); // check to ensure the order of nodes not changed.
			XliffResXConverter.MergeTranslationOfXliff12BackToResX("ResxXliff/MultilingualResources/Fonlow.VA.Languages.zh-Hant.xlf", "ResxXliff/AppResources.zh-Hant.resx");
		}

		[Fact]
		public async Task TestMergeZhHanS()
		{
			var r = XliffResXConverter.MergeResXToXliff12("ResxXliff/AppResources.resx", "ResxXliff/AppResources.zh-Hans.resx", "ResxXliff/MultilingualResources/Fonlow.VA.Languages.zh-Hans.xlf", NullLogger.Instance);
			Assert.Equal(0, r.Item1);
			Assert.Equal(0, r.Item2);

			XDocument xDoc;
			using (FileStream fs = new System.IO.FileStream("ResxXliff/MultilingualResources/Fonlow.VA.Languages.zh-Hans.xlf", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				var wg = new Xliff12Translate(true);
				var c = await wg.TranslateXliff(xliffRoot, ["new"], false, new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseSimplified, apiKey), null, null);
				Assert.Equal(22, c);
			}

			xDoc.Save("ResxXliff/MultilingualResources/Fonlow.VA.Languages.zh-Hans.xlf"); // check to ensure the order of nodes not changed.
			XliffResXConverter.MergeTranslationOfXliff12BackToResX("ResxXliff/MultilingualResources/Fonlow.VA.Languages.zh-Hans.xlf", "ResxXliff/AppResources.zh-Hans.resx");
		}

		[Fact]
		public async Task TestMergeJa()
		{
			var r = XliffResXConverter.MergeResXToXliff12("ResxXliff/AppResources.resx", "ResxXliff/AppResources.ja.resx", "ResxXliff/MultilingualResources/Fonlow.VA.Languages.ja.xlf", NullLogger.Instance);
			Assert.Equal(0, r.Item1);
			Assert.Equal(166, r.Item2);

			XDocument xDoc;
			using (FileStream fs = new System.IO.FileStream("ResxXliff/MultilingualResources/Fonlow.VA.Languages.ja.xlf", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				var wg = new Xliff12Translate(true);
				var c = await wg.TranslateXliff(xliffRoot, ["new"], false, new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseSimplified, apiKey), null, null);
				Assert.Equal(0, c);
			}

			xDoc.Save("ResxXliff/MultilingualResources/Fonlow.VA.Languages.ja.xlf"); // check to ensure the order of nodes not changed.
			XliffResXConverter.MergeTranslationOfXliff12BackToResX("ResxXliff/MultilingualResources/Fonlow.VA.Languages.ja.xlf", "ResxXliff/AppResources.ja.resx");
		}


	}
}
