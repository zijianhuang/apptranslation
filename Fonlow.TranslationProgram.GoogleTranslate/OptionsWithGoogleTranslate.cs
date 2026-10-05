using Plossum.CommandLine;
using Fonlow.TranslationProgram.Abstract;

namespace Fonlow.TranslationProgram.GoogleTranslate
{
	public class OptionsWithGoogleTranslate : OptionsBase
	{
		[CommandLineOption(Aliases = "AK", Description = "Google Translate API key. e.g., /AK=zasdfSDFSDfsdfdsfs234sdsfki")]
		public string ApiKey { get; set; }

		[CommandLineOption(Aliases = "AKF", Description = "Google Translate API key stored in a text file. e.g., /AKF=C:/Users/Public/DevApps/GtApiKey.txt")]
		public string ApiKeyFile { get; set; }

		[CommandLineOption(Aliases = "AV", Description = "Google Translate API version. Default to V2. If V3, a client secret JSON file is expected.")]
		public string ApiVersion { get; set; } = "V2";

		[CommandLineOption(Aliases = "CSF", Description = "Google Cloud Translate V3 does not support API key but rich ways of authentications. This app uses client secret JSON file you could download from your Google Cloud Service account.")]
		public string ClientSecretFile { get; set; }

		[CommandLineOption(Aliases = "Reversed", Description = "Translate from target language to source language and save the result to the target file so you can compare. Both SourceFile and TargetFile must be defined.")]
		public bool ReversedTranslation { get; set; }

		/// <summary>
		/// As described on https://docs.cloud.google.com/translate/docs/translate-text
		/// </summary>
		[CommandLineOption(Aliases = "V3M", Description = "V3 Model ID like general/nmt (default) or general/translation-llm or translation-llm-custom/{model-id}")]
		public string V3ModelId { get; set; } = "general/nmt";

		[CommandLineOption(Aliases = "L", Description = "LocationId for V3 translation, like us-central1. Default to us-central1.")]
		public string LocationId { get; set; } = "us-central1";

		[CommandLineOption(Aliases = "GL", Description = "GlossaryId for V3 translation, like mstc-dutch-en-nl, for example, this will result in projects/api-project-1234567890/locations/us-central1/glossaries/mstc-dutch-en-nl")]
		public string GlossaryId { get; set; }

		[CommandLineOption(Aliases = "LG", Description = "List all available glossaries.")]
		public bool ListGlossaries { get; set; }

	}
}
