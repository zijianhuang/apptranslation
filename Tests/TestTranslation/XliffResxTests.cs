using Fonlow.GoogleTranslate;
using Fonlow.XliffResX;
using Google.Cloud.Translation.V2;
using Microsoft.Extensions.Logging.Abstractions;
using System.Xml.Linq;

namespace TestResx
{
	[Collection("ServicesLaunch")]
	public class XliffResxTests
	{
		string apiKey = System.Environment.GetEnvironmentVariable("GoogleTranslateApiKey", EnvironmentVariableTarget.User);
		[Fact]
		public async Task TestResXToNewXliff()
		{
			using FileStream fs = new System.IO.FileStream("resx/AppResources.resx", System.IO.FileMode.Open, System.IO.FileAccess.Read);
			using FileStream fsLang = new System.IO.FileStream("resx/AppResources.zh-hans.resx", System.IO.FileMode.Open, System.IO.FileAccess.Read);
			var xDoc = XElement.Load(fs);
			var xDocLang = XElement.Load(fsLang);

			var x = XliffResXConverter.ConvertResXToXliff12(xDoc, xDocLang, "en", "zh-hans");
			Assert.Equal("{urn:oasis:names:tc:xliff:document:1.2}xliff", x.Root.Name);
			x.Save("AppResources.zh-hans.xlf");
			var c = await TranslateXliff("AppResources.zh-hans.xlf");
			Assert.Equal(3, c);
		}

		async Task<int> TranslateXliff(string filePath)
		{
			var xDoc = XDocument.Load(filePath);
			var xliffRoot = xDoc.Root;
			var wg = new Xliff12Translate(false);
			var c = await wg.TranslateXliff(xliffRoot, ["new"], false, new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseSimplified, apiKey), null, null);
			xDoc.Save(filePath);
			return c;
		}

		[Fact]
		public void TestResXToNewXliffFile()
		{
			XliffResXConverter.ConvertResXToXliff12("resx/AppResources.resx", "resx/AppResources.zh-hans.resx", "en", "zh-hans", "AppResourcesFile.zh-hans.xlf");
		}

		[Fact]
		public async Task TestMergeResXToNewXliff()
		{
			await TestResXToNewXliff();
			using FileStream fs = new System.IO.FileStream("resx/AppResourcesNew.resx", System.IO.FileMode.Open, System.IO.FileAccess.Read);
			using FileStream fsLang = new System.IO.FileStream("resx/AppResourcesNew.zh-hans.resx", System.IO.FileMode.Open, System.IO.FileAccess.Read);
			var xDoc = XElement.Load(fs);
			var xDocLang = XElement.Load(fsLang);

			using FileStream fsXliff = new System.IO.FileStream("AppResources.zh-hans.xlf", System.IO.FileMode.Open, System.IO.FileAccess.Read);
			var xliffDoc = XElement.Load(fsXliff);
			var r = XliffResXConverter.MergeResXToXliff12(xDoc, xDocLang, xliffDoc, NullLogger.Instance);
			Assert.Equal(2, r.Item1);
			Assert.Equal(1, r.Item2);
			xliffDoc.Save("AppResourcesNew.zh-hans.xlf");
		}

		[Fact]
		public async Task TestMergeResXToNewXliffAndTranslate()
		{
			await TestResXToNewXliff();
			var xDoc = XElement.Load("resx/AppResourcesNew.resx");
			var xDocLang = XElement.Load("resx/AppResourcesNew.zh-hans.resx");

			using FileStream fsXliff = new System.IO.FileStream("AppResources.zh-hans.xlf", System.IO.FileMode.Open, System.IO.FileAccess.Read);
			var xliffDoc = XElement.Load(fsXliff);
			var r = XliffResXConverter.MergeResXToXliff12(xDoc, xDocLang, xliffDoc, NullLogger.Instance);
			Assert.Equal(2, r.Item1);
			Assert.Equal(1, r.Item2);

			xliffDoc.Save("AppResourcesNew.zh-hans.xlf");
			var c = await TranslateXliff("AppResourcesNew.zh-hans.xlf");
			Assert.Equal(2, c);
		}

		[Fact]
		public async Task TestMergeResXToNewXliffAndTranslateAndMergeBackToResX()
		{
			var xDocLang = XElement.Load("resx/AppResourcesNew.zh-hans.resx");
			xDocLang.Save("AppResourcesNew.zh-hans.resx"); //just a local copy for testing

			await TestMergeResXToNewXliffAndTranslate();
			XliffResXConverter.MergeTranslationOfXliff12BackToResX("AppResourcesNew.zh-hans.xlf", "AppResourcesNew.zh-hans.resx");

		}

	}
}
