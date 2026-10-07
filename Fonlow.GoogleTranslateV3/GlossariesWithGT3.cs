using Fonlow.Translate;
using Google.Api.Gax.ResourceNames;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Translate.V3;

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

}
