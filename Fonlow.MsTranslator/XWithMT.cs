using Azure;
using Azure.AI.Translation.Text;
using Fonlow.Translate;

namespace Fonlow.MsTranslator
{
	public class XWithMT : ITranslate
	{
		public string SourceLang { get; set; }
		public string TargetLang { get; set; }

		public XWithMT(string sourceLang, string targetLang, string key, string region)
		{
			this.SourceLang = sourceLang;
			this.TargetLang = targetLang;
			AzureKeyCredential credential = new(key);
			translationClient = new(credential, region);
		}

		readonly TextTranslationClient translationClient;

		public async Task<string> Translate(string text)
		{
			var response = await translationClient.TranslateAsync(TargetLang, text, SourceLang);
			var translationTextItem = response.Value.FirstOrDefault();
			return translationTextItem.Translations?.FirstOrDefault()?.Text;
		}

		public async Task<string[]> Translate(IList<string> strings)
		{
			var response = await translationClient.TranslateAsync(TargetLang, strings, SourceLang);
			return response.Value.Select(item => item.Translations?.FirstOrDefault()?.Text).ToArray();
		}
	}
}
