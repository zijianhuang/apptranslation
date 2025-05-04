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
	}
}
