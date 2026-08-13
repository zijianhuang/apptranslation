using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fonlow.GoogleTranslateV3
{
	/// <summary>
	/// Google API does not read the project_id from the client_secret file.
	/// </summary>
	public static class ClientSecretReader
	{
		public static string ReadProjectId(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException(nameof(filePath), "Expect client secret JSON file.");
			}

			var doc = JsonDocument.Parse(File.ReadAllText(filePath));
			var installedNode = doc.RootElement.GetProperty("installed");
			var projectIdNode = installedNode.GetProperty("project_id");
			return projectIdNode.ToString();
		}
	}

	//public class GoogleOAuthClientSecrets
	//{
	//	[JsonPropertyName("installed")]
	//	public InstalledAppCredentials Installed { get; set; }
	//}

	//public class InstalledAppCredentials
	//{
	//	[JsonPropertyName("client_id")]
	//	public string ClientId { get; set; }

	//	[JsonPropertyName("project_id")]
	//	public string ProjectId { get; set; }

	//	[JsonPropertyName("auth_uri")]
	//	public string AuthUri { get; set; }

	//	[JsonPropertyName("token_uri")]
	//	public string TokenUri { get; set; }

	//	[JsonPropertyName("auth_provider_x509_cert_url")]
	//	public string AuthProviderX509CertUrl { get; set; }

	//	[JsonPropertyName("client_secret")]
	//	public string ClientSecret { get; set; }

	//	[JsonPropertyName("redirect_uris")]
	//	public List<string> RedirectUris { get; set; }
	//}
}
