using Azure;
using Azure.AI.Translation.Text;
using Fonlow.Translate;

namespace Fonlow.MsTranslator
{
	public class XWithMT : ITranslate
	{
		public string SourceLang { get; set; }
		public string TargetLang { get; set; }
		public string CategoryId { get; set; } = "general";

		public XWithMT(string sourceLang, string targetLang, string key, string region, string categoryId="general")
		{
			this.SourceLang = sourceLang;
			this.TargetLang = targetLang;
			this.CategoryId= categoryId;
			AzureKeyCredential credential = new(key);
			translationClient = new(credential, region);
		}

		readonly TextTranslationClient translationClient;

		public async Task<string> Translate(string text)
		{
			var response = await translationClient.TranslateAsync(TargetLang, text, SourceLang); //Azure AI Translator API not supporting category in single text.
			var translationTextItem = response.Value.FirstOrDefault();
			return translationTextItem.Translations?.FirstOrDefault()?.Text;
		}

		public async Task<string[]> Translate(IList<string> strings)
		{
			string[] targetLanguanges = { TargetLang };
			var response = await translationClient.TranslateAsync(targetLanguanges, strings, sourceLanguage: SourceLang, category: CategoryId); //something like "a3a1eeb1-7e2b-4098-b293-da762fe3bb79-INTERNT"
			return response.Value.Select(item => item.Translations?.FirstOrDefault()?.Text).ToArray();
		}
	}
}
