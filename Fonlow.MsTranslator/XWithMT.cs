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
			return await TranslateText(text, TextType.Plain);
		}

		public async Task<string> TranslateHtml(string text)
		{
			return await TranslateText(text, TextType.Html);
		}

		public async Task<string> TranslateText(string text, TextType textType)
		{
			var item = new TranslateInputItem(text, new TranslationTarget(TargetLang), SourceLang, textType: textType);
			var response = await translationClient.TranslateAsync(item); //Azure AI Translator API not supporting category in single text.
			var translationTextItem = response.Value;
			return translationTextItem.Translations?.FirstOrDefault()?.Text;
		}

		public async Task<string[]> Translate(IList<string> strings)
		{
			return await TranslateItems(strings, TextType.Plain);
		}

		public async Task<string[]> TranslateHtmlItems(IList<string> strings)
		{
			return await TranslateItems(strings, TextType.Html);
		}

		async Task<string[]> TranslateItems(IList<string> strings, TextType textType)
		{
			string[] targetLanguanges = { TargetLang };
			var items = strings.Select(d => new TranslateInputItem(d, new TranslationTarget(TargetLang), SourceLang, textType: textType));
			var response = await translationClient.TranslateAsync(items);
			return response.Value.Select(item => item.Translations?.FirstOrDefault()?.Text).ToArray();
		}
	}
}
