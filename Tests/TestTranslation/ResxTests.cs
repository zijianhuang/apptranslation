using Fonlow.GoogleTranslate;
using Fonlow.ResxTranslate;
using Google.Cloud.Translation.V2;
using Microsoft.Extensions.Logging.Abstractions;
using System.Xml.Linq;

namespace TestResx
{
	[Collection("ServicesLaunch")]
	public class ResxTests
	{
		string apiKey = System.Environment.GetEnvironmentVariable("GoogleTranslateApiKey", EnvironmentVariableTarget.User);

		[Fact]
		public void TestReadViaXElement()
		{
			using (FileStream fs = new System.IO.FileStream("resx/AppResources.resx", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				var xDoc = XDocument.Load(fs);
				var resxRoot = xDoc.Root;
				//var ns = xliffRoot.GetDefaultNamespace();
				var dataNodes = resxRoot.Elements("data").ToList();
				Assert.True(dataNodes.Count > 1);
				var second = dataNodes[1];
				Assert.Equal("ChartSnellen", second.Attribute("name").Value);
				Assert.Equal("Snellen", second.Element("value").Value);
			}
		}

		[Fact]
		public async Task TestGoogleTranslateFileZh()
		{
			var g = new ResxTranslate(false);
			Assert.Equal(3, await g.TranslateResx("resx/AppResources.resx", "AppResources.translated.resx", new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseTraditional, apiKey), NullLogger.Instance, null));
		}
	}
}
