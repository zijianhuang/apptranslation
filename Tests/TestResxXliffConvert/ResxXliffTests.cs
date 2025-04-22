using Fonlow.XliffResX;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestResxXliffConvert
{
	[Collection("ServicesLaunch")]
	public class ResxXliffTests
	{
		[Fact]
		public void TestMergeZhHanT()
		{
			var r = XliffResXConverter.MergeResXToXliff12("ResxXliff/AppResources.resx", "ResxXliff/AppResources.zh-Hant.resx", "ResxXliff/MultilingualResources/Fonlow.VA.Languages.zh-Hant.xlf", NullLogger.Instance);
			Assert.Equal(1, r.Item1);
			Assert.Equal(166, r.Item2);
		}

		[Fact]
		public void TestMergeZhHanS()
		{
			var r = XliffResXConverter.MergeResXToXliff12("ResxXliff/AppResources.resx", "ResxXliff/AppResources.zh-Hans.resx", "ResxXliff/MultilingualResources/Fonlow.VA.Languages.zh-Hans.xlf", NullLogger.Instance);
			Assert.Equal(22, r.Item1);
			Assert.Equal(0, r.Item2);

			XliffResXConverter.MergeTranslationOfXliff12BackToResX("ResxXliff/MultilingualResources/Fonlow.VA.Languages.zh-Hans.xlf", "ResxXliff/AppResources.zh-Hans.resx");
		}


	}
}
