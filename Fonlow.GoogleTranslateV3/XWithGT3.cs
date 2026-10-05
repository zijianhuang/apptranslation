using Fonlow.Translate;
using Google.Api.Gax.ResourceNames;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Translate.V3;

namespace Fonlow.GoogleTranslate
{
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
		public XWithGT3(string sourceLang, string targetLang, GoogleClientSecrets clientSecrets, string projectId, string modelId = "general/nmt", string locationId= "us-central1", string glossaryId = null)
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
				Model = this.v3Model
			};
			var response = await translationClient.TranslateTextAsync(request).ConfigureAwait(false);
			var translation = response.Translations[0];
			return translation.TranslatedText;
		}

		public async Task<string> TranslateWithGlossary(string text, string mimeType)
		{
			var request = new TranslateTextRequest
			{
				Contents = { text },
				SourceLanguageCode = this.SourceLang,
				TargetLanguageCode = this.TargetLang,
				Parent = new LocationName(projectId, locationId).ToString(),
				MimeType = mimeType,
				Model = this.v3Model,
				GlossaryConfig = new TranslateTextGlossaryConfig
				{
					Glossary = new GlossaryName(projectId, locationId, glossaryId).ToString(), // $"projects/{projectId}/locations/{location}/glossaries/{this.glossary}",
					IgnoreCase = false
				}
			};
			var response = await translationClient.TranslateTextAsync(request).ConfigureAwait(false);
			var translation = response.GlossaryTranslations[0];
			return translation.TranslatedText;
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
			};
			var response = await translationClient.TranslateTextAsync(request).ConfigureAwait(false);
			var translatedStrings = response.Translations.Select(d => d.TranslatedText).ToArray();
			return translatedStrings;
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

		/// <summary>
		/// Prints glossaries and (optionally) their entries to the console.
		/// </summary>
		public async Task DumpGlossaries(bool includeEntries = true, int maxEntriesPerGlossary = 50)
		{
			var glossaries = await ListGlossaries().ConfigureAwait(false);
			Console.WriteLine($"Found {glossaries.Count} glossaries in {projectId}/{locationId}");

			foreach (var g in glossaries)
			{
				var langs = g.LanguagePair != null
					? $"{g.LanguagePair.SourceLanguageCode} -> {g.LanguagePair.TargetLanguageCode}"
					: $"set: {string.Join(", ", g.LanguageCodesSet?.LanguageCodes ?? new())}";

				Console.WriteLine($"\n{g.Name}");
				Console.WriteLine($"  Languages : {langs}");
				Console.WriteLine($"  EntryCount: {g.EntryCount}");
				Console.WriteLine($"  Source    : {g.InputConfig?.GcsSource?.InputUri}");
				Console.WriteLine($"  Submitted : {g.SubmitTime?.ToDateTime():u}");

				if (!includeEntries)
				{
					continue;
				}

				var glossaryId = g.GlossaryName.GlossaryId;
				var entries = await ListGlossaryEntries(glossaryId, maxEntriesPerGlossary).ConfigureAwait(false);
				foreach (var e in entries)
				{
					if (e.TermsPair != null)
					{
						Console.WriteLine($"    {e.TermsPair.SourceTerm.Text}\t{e.TermsPair.TargetTerm.Text}");
					}
					else if (e.TermsSet != null)
					{
						Console.WriteLine("    " + string.Join("\t", e.TermsSet.Terms.Select(t => $"{t.LanguageCode}:{t.Text}")));
					}
				}
			}
		}
	}
}
