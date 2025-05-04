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

		[CommandLineOption(Aliases = "RG", Description = "Region associated with the key. e.g., /AG=australiaeast")]
		public string Region { get; set; }

	}
}
