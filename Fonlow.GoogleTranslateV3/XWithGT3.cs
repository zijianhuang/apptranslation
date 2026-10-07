using Fonlow.Translate;
using Google.Api.Gax.ResourceNames;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Translate.V3;
using static System.Net.Mime.MediaTypeNames;

namespace Fonlow.GoogleTranslate
{
	public class GlossariesWithGT3 : IGlossarySupport
	{
		public GlossariesWithGT3(GoogleClientSecrets clientSecrets, string projectId, string locationId)
		{
			this.projectId = projectId;
			this.locationId = locationId;
			var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
				clientSecrets.Secrets,
				scopes, // https://developers.google.com/identity/protocols/oauth2/scopes
				"user",
				CancellationToken.None).Result;
			translationClient = new TranslationServiceClientBuilder()
			{
				Credential = credential,
			}.Build();

		}
		readonly string projectId;
		private static readonly string[] scopes = ["https://www.googleapis.com/auth/cloud-translation"];
		readonly string locationId;
		readonly TranslationServiceClient translationClient;

		/// <summary>
		/// Glossary Ids.
		/// </summary>
		/// <returns></returns>
		public async Task<IReadOnlyList<string>> ListNamesOfGlossaries()
		{
			var glossaries = await ListGlossaries().ConfigureAwait(false);
			return glossaries.Select(g => $"{g.GlossaryName.GlossaryId} ~ {g.LanguagePair.SourceLanguageCode} -> {g.LanguagePair.TargetLanguageCode} ({g.EntryCount})").ToList();
		}

		/// <summary>
		/// Lists all glossaries in the project/location with their metadata.
		/// </summary>
		public async Task<IReadOnlyList<Glossary>> ListGlossaries()
		{
			var request = new ListGlossariesRequest
			{
				Parent = new LocationName(projectId, locationId).ToString(),
			};

			var result = new List<Glossary>();
			await foreach (var glossary in translationClient.ListGlossariesAsync(request).ConfigureAwait(false))
			{
				result.Add(glossary);
			}

			return result;
		}

		/// <summary>
		/// Gets metadata of a single glossary (entry count, language pair, source file, etc.).
		/// </summary>
		public async Task<Glossary> GetGlossary(string glossaryId)
		{
			var name = new GlossaryName(projectId, locationId, glossaryId);
			return await translationClient.GetGlossaryAsync(name).ConfigureAwait(false);
		}

		/// <summary>
		/// Lists the actual term entries of a glossary.
		/// </summary>
		public async Task<IReadOnlyList<GlossaryEntry>> ListGlossaryEntries(string glossaryId, int maxEntries = int.MaxValue)
		{
			var request = new ListGlossaryEntriesRequest
			{
				Parent = new GlossaryName(projectId, locationId, glossaryId).ToString(),
			};

			var result = new List<GlossaryEntry>();
			var response = translationClient.ListGlossaryEntriesAsync(request).ConfigureAwait(false);
			await foreach (var entry in response)
			{
				result.Add(entry);
				if (result.Count >= maxEntries)// || entry.TermsPair.SourceTerm.Text=="Home")
				{
					break;
				}
			}

			return result;
		}
	}

	/// <summary>
	/// Wrapper of Google Translate v3 API
	/// </summary>
	public class XWithGT3 : ITranslate
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceLang"></param>
		/// <param name="targetLang"></param>
		/// <param name="clientSecrets"></param>
		/// <param name="projectId"></param>
		/// <param name="modelId">also general/translation-llm, and translation-llm-custom/{model-id} as well</param>
		public XWithGT3(string sourceLang, string targetLang, GoogleClientSecrets clientSecrets, string projectId, string modelId = "general/nmt", string locationId = "us-central1", string glossaryId = null)
		{
			ArgumentNullException.ThrowIfNullOrEmpty(projectId);
			ArgumentNullException.ThrowIfNull(clientSecrets);

			this.SourceLang = sourceLang;
			this.TargetLang = targetLang;
			this.projectId = projectId;
			this.locationId = locationId;
			this.v3Model = $"projects/{projectId}/locations/{locationId}/models/{modelId}"; // new ModelName(projectId, locationId, modelId).ToString(); throw exception
			this.glossaryId = glossaryId;
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
		readonly string v3Model;
		readonly string locationId;
		readonly string glossaryId;
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
				Parent = new LocationName(projectId, locationId).ToString(),
				MimeType = mimeType,
				Model = this.v3Model,
				GlossaryConfig = string.IsNullOrEmpty(glossaryId) ? null : new TranslateTextGlossaryConfig
				{
					Glossary = new GlossaryName(projectId, locationId, glossaryId).ToString(), // $"projects/{projectId}/locations/{location}/glossaries/{this.glossary}",
					IgnoreCase = false // case sensitive is good for almost all scenarios, except for some cases like "Home" vs "home", and "CARD" vs "card". The glossary should be built with the right case.
				}
			};
			var response = await translationClient.TranslateTextAsync(request).ConfigureAwait(false);
			var translation = string.IsNullOrEmpty(glossaryId) ? response.Translations[0] : response.GlossaryTranslations[0];
			var translatedText = translation.TranslatedText;
			if (v3Model.Contains("translation-llm"))
			{
				var firstPick = SingleTermHelper.Normalize(text, translatedText, TargetLang); // for LLM model, normalize the translation to remove the extra explanation text.
				if (firstPick != translatedText)
				{
					Console.WriteLine($"Normalized: {translatedText} => {firstPick}");
					translatedText = firstPick;
				}
			}

			return translatedText;
		}

		public async Task<string[]> Translate(IList<string> strings)
		{
			return await Translate(strings, "text/plain").ConfigureAwait(false);
		}

		public async Task<string[]> TranslateHtmlItems(IList<string> htmlItems)
		{
			return await Translate(htmlItems, "text/html").ConfigureAwait(false);
		}

		async Task<string[]> Translate(IList<string> strings, string mimeType)
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
				Parent = new LocationName(projectId, locationId).ToString(),
				MimeType = mimeType,
				Model = this.v3Model,
				GlossaryConfig = string.IsNullOrEmpty(glossaryId) ? null : new TranslateTextGlossaryConfig
				{
					Glossary = new GlossaryName(projectId, locationId, glossaryId).ToString(),
				}
			};
			var response = await translationClient.TranslateTextAsync(request).ConfigureAwait(false);
			var translatedStrings = response.Translations.Select(d => d.TranslatedText).ToArray();
			if (v3Model.Contains("translation-llm"))
			{
				for (int i = 0; i < strings.Count; i++)
				{
					var firstPick = SingleTermHelper.Normalize(strings[i], translatedStrings[i], TargetLang); // for LLM model, normalize the translation to remove the extra explanation text.
					if (firstPick != translatedStrings[i])
					{
						Console.WriteLine($"Normalized: {translatedStrings[i]} => {firstPick}");
						translatedStrings[i] = firstPick;
					}
				}
			}

			return translatedStrings;
		}

	}
}
