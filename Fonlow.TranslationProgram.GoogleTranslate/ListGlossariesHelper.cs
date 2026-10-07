using Fonlow.GoogleTranslate;
using Fonlow.GoogleTranslateV3;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fonlow.TranslationProgram.GoogleTranslate
{
	public static class ListGlossariesHelper
	{
		public static async Task ListGlossaries(OptionsWithGoogleTranslate options)
		{
			var clientSecrets = GoogleClientSecrets.FromFile(options.ClientSecretFile);
			var projectId = ClientSecretReader.ReadProjectId(options.ClientSecretFile);
			var ggt = new GlossariesWithGT3(clientSecrets, projectId, options.LocationId);
			var gNames = await ggt.ListNamesOfGlossaries().ConfigureAwait(false);
			Console.WriteLine($"Glossaries: {gNames.Count}");
			foreach (var name in gNames)
			{
				Console.WriteLine(name);
			}
		}
	}
}
