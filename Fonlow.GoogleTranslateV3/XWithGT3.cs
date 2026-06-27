using Fonlow.Translate;
using Google.Api.Gax.ResourceNames;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Translate.V3;
using static System.Net.Mime.MediaTypeNames;

namespace Fonlow.GoogleTranslate
{
	/// <summary>
	/// Wrapper of Google Translate v2 API
	/// </summary>
	public class XWithGT3 : ITranslate
	{
		public XWithGT3(string sourceLang, string targetLang, GoogleClientSecrets clientSecrets, string projectId)
		{
			ArgumentNullException.ThrowIfNullOrEmpty(projectId);
			ArgumentNullException.ThrowIfNull(clientSecrets);

			this.SourceLang = sourceLang;
			this.TargetLang = targetLang;
			this.projectId = projectId;
			var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
				clientSecrets.Secrets,
				scopes, // https://developers.google.com/identity/protocols/oauth2/scopes
				"user",
				CancellationToken.None).Result;
			translationClient = new TranslationServiceClientBuilder()
			{
				Credential = credential,
				//JsonCredentials= clientSecretJsonText,
			}.Build();
		}

		public string SourceLang { get; set; }
		public string TargetLang { get; set; }
		readonly TranslationServiceClient translationClient;
		readonly string projectId;
		private static readonly string[] scopes = ["https://www.googleapis.com/auth/cloud-translation"];

		public async Task<string> Translate(string text)
		{
			return await Translate(text, "text/plain").ConfigureAwait(false);
		}

		public async Task<string> TranslateHtml(string htmlText)
		{
			return await Translate(htmlText, "text/html").ConfigureAwait(false);
		}

		public async Task<string> Translate(string text, string mimeType)
		{
			var request = new TranslateTextRequest
			{
				Contents = { text },
				SourceLanguageCode = this.SourceLang,
				TargetLanguageCode = this.TargetLang,
				Parent = new ProjectName(projectId).ToString(),
				MimeType = mimeType,
			};
			var response = await translationClient.TranslateTextAsync(request).ConfigureAwait(false);
			var translation = response.Translations[0];
			return translation.TranslatedText;
		}

		public async Task<string[]> Translate(IList<string> strings)
		{
			ArgumentNullException.ThrowIfNull(strings);

			if (strings.Count > 1024)
			{
				throw new ArgumentException("The API supports up to 1024. Otherwise, use batch API.");
			}

			var request = new TranslateTextRequest
			{
				Contents = { strings },
				SourceLanguageCode = this.SourceLang,
				TargetLanguageCode = this.TargetLang,
				Parent = new ProjectName(projectId).ToString(),
				MimeType = "text/plain",
			};
			var response = await translationClient.TranslateTextAsync(request).ConfigureAwait(false);
			var translatedStrings = response.Translations.Select(d => d.TranslatedText).ToArray();
			return translatedStrings;
		}
	}
}
