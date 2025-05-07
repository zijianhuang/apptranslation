using Fonlow.Cli;
using Fonlow.TranslationProgram.GoogleTranslate;
using Microsoft.Extensions.Logging;
using Plossum.CommandLine;

namespace Fonlow.TranslationProgram
{
	[CliManager(Description = "Use Google Translate v2 or v3 to translate XLIFF v1.2 or v2.0 file.", OptionSeparator = "/", Assignment = ":")]
	internal class OptionsForXliffWithGoogleTranslate : OptionsWithGoogleTranslate
	{
		[CommandLineOption(Aliases = "SS", Description = "For translation unit of states. Default to new for v1.2 and initial for v2.0, e.g., /SS=\"initial\" \"translated\"")]
		public string[] ForStates { get; set; } = [];

		[CommandLineOption(Aliases = "NCS", Description = "Not to change the state of translation unit to translated after translation.")]
		public bool NotChangeState { get; set; }

	}

//	internal class TranslationProgramXliffWithGoogleTranslate : TranslationProgramWithGoogleTranslate
//	{
//		public TranslationProgramXliffWithGoogleTranslate(OptionsForXliffWithGoogleTranslate options, ILogger logger) : base(new ResxTranslation(), options, logger)
//		{
//		}

//		public override void DisplayExamples()
//		{
//			Console.WriteLine(
//@"Examples:
//GoogleTranslateXliff.exe /AK=YourGoogleTranslateV2ApiKey /F=myUiMessages.es.xlf ---- For in-place translation.
//GoogleTranslateXliff.exe /AK=YourGoogleTranslateV2ApiKey /F:myUiMessages.ja.xlf /TF:myUiMessagesTranslated.ja.xlf ---- from the source locale file to a new target file in Japanese
//GoogleTranslateXliff.exe /AK=YourGoogleTranslateV2ApiKey /F:myUiMessages.xlf /TF:myUiMessages.es.xlf /TL=es ---- From the source template file to a new target file in Spanish.
//GoogleTranslateXliff.exe /AV=v3 /CSF=client_secret.json /B /F:myUiMessages.es.xlf ---- Use Google Cloud Translate V3 and batch mode.
//"
//			);
//		}
//	}


}
