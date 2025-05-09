using Plossum.CommandLine;
using Fonlow.TranslationProgram.Abstract;

namespace Fonlow.TranslationProgram.MsTranslator
{
	public class OptionsWithMsTranslator : OptionsBase
	{
		[CommandLineOption(Aliases = "AK", Description = "Microsoft Translator API key. e.g., /AK=zasdfSDFSDfsdfdsfs234sdsfki")]
		public string ApiKey { get; set; }

		[CommandLineOption(Aliases = "AKF", Description = "MS Translator API key stored in a text file. e.g., /AKF=C:/Users/Public/DevApps/GtApiKey.txt")]
		public string ApiKeyFile { get; set; }

		[CommandLineOption(Aliases = "RG", Description = "Region associated with the key. Always required. e.g., /RG=australiaeast")]
		public string Region { get; set; }

		[CommandLineOption(Aliases = "CA", Description = "Category ID from one of your custom translator's projects in the form of WorkspaceID+CategoryCode, used by Batch mode, while the default is general . e.g., /CA=a3a1eeb1-7e2b-4098-b293-da762fe3bb79-INTERNT")]
		public string CategoryId { get; set; }

	}
}
