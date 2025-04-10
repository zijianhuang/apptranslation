using Google.Cloud.Translation.V2;

namespace Fonlow.GoogleTranslate
{
	/// <summary>
	/// Wrapper of Google Translate v2 API
	/// </summary>
	public class XWithGT2 : ITranslate
	{
		public XWithGT2(string sourceLang, string targetLang, string apiKey)
		{
			ArgumentNullException.ThrowIfNullOrEmpty(apiKey);

			this.SourceLang = sourceLang;
			this.TargetLang = targetLang;
			translationClient = TranslationClient.CreateFromApiKey(apiKey);
		}

		public string SourceLang { get; set; }
		public string TargetLang { get; set; }
		readonly TranslationClient translationClient;

		public async Task<string> Translate(string text)
		{
			var r = await translationClient.TranslateTextAsync(text, TargetLang, SourceLang);
			return r.TranslatedText;
		}

		public async Task<string[]> Translate(IList<string> strings)
		{
			var r = await translationClient.TranslateTextAsync(strings, TargetLang, SourceLang);
			return r.Select(d => d.TranslatedText).ToArray();
		}

		public async Task<string> TranslateHtml(string htmlText)
		{
			var r = await translationClient.TranslateHtmlAsync(htmlText, TargetLang, SourceLang);
			return r.TranslatedText;
		}


	}
}
