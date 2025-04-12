using Fonlow.GoogleTranslate;
using Fonlow.ResxTranslate;
using Fonlow.XliffResX;
using Google.Cloud.Translation.V2;
using System.Xml.Linq;

namespace TestResx
{
	[Collection("ServicesLaunch")]
	public class XliffResxTests
	{
		[Fact]
		public void TestResXToNewXliff()
		{
			using FileStream fs = new System.IO.FileStream("resx/AppResources.resx", System.IO.FileMode.Open, System.IO.FileAccess.Read);
			using FileStream fsLang = new System.IO.FileStream("resx/AppResources.zh-hans.resx", System.IO.FileMode.Open, System.IO.FileAccess.Read);
			var xDoc = XElement.Load(fs);
			var xDocLang = XElement.Load(fsLang);
			
			var x = XliffResXConverter.ConvertResXToXliff12(xDoc, xDocLang, "en", "zh-hans");
			Assert.Equal("{urn:oasis:names:tc:xliff:document:1.2}xliff", x.Root.Name);
			x.Save("AppResources.zh-hans.xlf", SaveOptions.OmitDuplicateNamespaces);
		}

	}
}
