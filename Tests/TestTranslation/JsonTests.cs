using Fonlow.GoogleTranslate;
using Fonlow.JsonTranslate;
using Google.Cloud.Translation.V2;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace TestJson
{
	[Collection("ServicesLaunch")]
	public class JsonTests
	{
		string apiKey = System.Environment.GetEnvironmentVariable("GoogleTranslateApiKey", EnvironmentVariableTarget.User);

		//[Fact]
		//public void TestReadViaXElement()
		//{
		//	using (FileStream fs = new System.IO.FileStream("json/AppResources.json", System.IO.FileMode.Open, System.IO.FileAccess.Read))
		//	{
		//		var xDoc = XDocument.Load(fs);
		//		var jsonRoot = xDoc.Root;
		//		//var ns = xliffRoot.GetDefaultNamespace();
		//		var dataNodes = jsonRoot.Elements("data").ToList();
		//		Assert.True(dataNodes.Count > 1);
		//		var second = dataNodes[1];
		//		Assert.Equal("ChartSnellen", second.Attribute("name").Value);
		//		Assert.Equal("Snellen", second.Element("value").Value);
		//	}
		//}

		//[Fact]
		//public async Task TestGoogleTranslateFileZh()
		//{
		//	var g = new JsonTranslation();
		//	g.SetSourceFile("json/AppResources.json");
		//	g.SetTargetFile("AppResources.translated.json");
		//	Assert.Equal(3, await g.Translate(new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseTraditional, apiKey), NullLogger.Instance, null));
		//}

		[Fact]
		public void TestJsonBasic()
		{
			string jsonString = "{\"data\": {\"user\": {\"name\": \"John Doe\", \"age\": 30}}}";
			JsonObject jsonObject = JsonNode.Parse(jsonString).AsObject();
			var n = jsonObject["data"]["user"]["name"];
			Assert.Equal("$.data.user.name", n.GetPath());
			Assert.Equal("John Doe", n.GetValue<string>());
			n = "AAA";
			Assert.Equal("AAA", n.GetValue<string>());

			var n2 = jsonObject["data"]["user"]["name"];
			Assert.Equal("John Doe", n2.GetValue<string>());
		}

		[Fact]
		public void TestJsonBasic2()
		{
			string jsonString = "{\"data\": {\"user\": {\"name\": \"John Doe\", \"age\": 30}}}";
			JsonObject jsonObject = JsonNode.Parse(jsonString).AsObject();
			var n = jsonObject["data"]["user"]["name"];
			Assert.Equal("$.data.user.name", n.GetPath());
			jsonObject["data"]["user"]["name"].ReplaceWith("BBB");

			var n2 = jsonObject["data"]["user"]["name"];
			Assert.Equal("BBB", n2.GetValue<string>());
		}

		[Fact]
		public void TestJsonBasic3()
		{
			string jsonString = "{\"data\": {\"user\": {\"name\": \"John Doe\", \"age\": 30}}}";
			JsonObject jsonObject = JsonNode.Parse(jsonString).AsObject();
			var n = jsonObject["data"]["user"]["name"];
			Assert.Equal("$.data.user.name", n.GetPath());
			n.ReplaceWith("BBB");

			var n2 = jsonObject["data"]["user"]["name"];
			Assert.Equal("BBB", n2.GetValue<string>());
		}

		[Fact]
		public async Task TestJsonTranslate()
		{
			string jsonString = "{\"data\": {\"user\": {\"name\": \"Someone loves you\", \"age\": 30}}}";
			JsonObject jsonObject = JsonNode.Parse(jsonString).AsObject();
			var translation = new JsonObjectTranslation();
			var c = await translation.TranslateJsonObject(jsonObject, ["data.user.name"], new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseTraditional, apiKey), NullLogger.Instance, null);
			Assert.Equal(1, c);
			var n2 = jsonObject["data"]["user"]["name"];
			Assert.Equal("有人愛你", n2.GetValue<string>());

		}

		[Fact]
		public async Task TestJsonTranslateWithWrongPath()
		{
			string jsonString = "{\"data\": {\"user\": {\"name\": \"Someone loves you\", \"age\": 30}}}";
			JsonObject jsonObject = JsonNode.Parse(jsonString).AsObject();
			var translation = new JsonObjectTranslation();
			var c = await translation.TranslateJsonObject(jsonObject, ["data.something.name"], new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseTraditional, apiKey), NullLogger.Instance, null);
			Assert.Equal(0, c);
		}

		[Fact]
		public async Task TestJsonTranslateWithEmptyPath()
		{
			string jsonString = "{\"data\": {\"user\": {\"name\": \"Someone loves you\", \"age\": 30}}}";
			JsonObject jsonObject = JsonNode.Parse(jsonString).AsObject();
			var translation = new JsonObjectTranslation();
			var c = await translation.TranslateJsonObject(jsonObject, [], new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseTraditional, apiKey), NullLogger.Instance, null);
			Assert.Equal(0, c);
		}

		[Fact]
		public async Task TestJsonTranslateWithNullPath()
		{
			string jsonString = "{\"data\": {\"user\": {\"name\": \"Someone loves you\", \"age\": 30}}}";
			JsonObject jsonObject = JsonNode.Parse(jsonString).AsObject();
			var translation = new JsonObjectTranslation();
			var c = await translation.TranslateJsonObject(jsonObject, [null], new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseTraditional, apiKey), NullLogger.Instance, null);
			Assert.Equal(0, c);
		}



	}
}
