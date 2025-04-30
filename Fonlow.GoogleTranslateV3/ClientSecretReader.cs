using System.Text.Json;

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
}
