using Fonlow.TranslationProgram;
using Fonlow.TranslationProgram.Abstract;
using Microsoft.Extensions.Logging;

namespace GoogleTranslateXliff
{
	class Program
	{
		static async Task<int> Main(string[] args)
		{
			using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
			var logger = loggerFactory.CreateLogger("program");
			var options = new OptionsForXliffWithGoogleTranslate();
			var errorCode = CliOptionsParser.Parse(args, options, DisplayExamples, logger);
			if (errorCode == 0)
			{
				var translationProgram = new TranslationProgramXliffWithGoogleTranslate(options, logger);
				var r = await translationProgram.Execute().ConfigureAwait(false);
				return r;
			}

			return errorCode;
		}

		static void DisplayExamples()
		{
			Console.WriteLine(
@"Examples:
GoogleTranslateXliff.exe /AK=YourGoogleTranslateV2ApiKey /F=myUiMessages.es.xlf ---- For in-place translation.
GoogleTranslateXliff.exe /AK=YourGoogleTranslateV2ApiKey /F:myUiMessages.ja.xlf /TF:myUiMessagesTranslated.ja.xlf ---- from the source locale file to a new target file in Japanese
GoogleTranslateXliff.exe /AK=YourGoogleTranslateV2ApiKey /F:myUiMessages.xlf /TF:myUiMessages.es.xlf /TL=es ---- From the source template file to a new target file in Spanish.
GoogleTranslateXliff.exe /AV=v3 /CSF=client_secret.json /B /F:myUiMessages.es.xlf ---- Use Google Cloud Translate V3 and batch mode.
"
			);
		}

	}

	//[CliManager(Description = "Use Google Translate v2 or v3 to translate XLIFF v1.2 or v2.0 file.", OptionSeparator = "/", Assignment = ":")]
	//internal class Options
	//{
	//	[CommandLineOption(Aliases = "F", Description = "Source file path, e.g., /F=myfile.zh.xliff")]
	//	public string SourceFile { get; set; }

	//	[CommandLineOption(Aliases = "TF", Description = "Target file path. If not defined, the source file will be overwritten, e.g., /TF=c:/locales/myfileTT.zh.xliff")]
	//	public string TargetFile { get; set; }

	//	[CommandLineOption(Aliases = "SL", Description = "Source language. Default to xliff/file/source-language or xliff/srcLang. e.g., /SL=fr")]
	//	public string SourceLang { get; set; }

	//	[CommandLineOption(Aliases = "TL", Description = "Target language. Default to xliff/file/target-language or xliff/trgLang.  e.g., /TL=es")]
	//	public string TargetLang { get; set; }

	//	[CommandLineOption(Aliases = "AK", Description = "Google Translate API key. e.g., /AK=zasdfSDFSDfsdfdsfs234sdsfki")]
	//	public string ApiKey { get; set; }

	//	[CommandLineOption(Aliases = "AKF", Description = "Google Translate API key stored in a text file. e.g., /AKF=C:/Users/Public/DevApps/GtApiKey.txt")]
	//	public string ApiKeyFile { get; set; }

	//	[CommandLineOption(Aliases = "SS", Description = "For translation unit of states. Default to new for v1.2 and initial for v2.0, e.g., /SS=\"initial\" \"translated\"")]
	//	public string[] ForStates { get; set; } = [];

	//	[CommandLineOption(Aliases = "NCS", Description = "Not to change the state of translation unit to translated after translation.")]
	//	public bool NotChangeState { get; set; }

	//	[CommandLineOption(Aliases = "B", Description = "Batch processing of strings to improve overall speed.")]
	//	public bool Batch { get; set; }

	//	[CommandLineOption(Aliases = "AV", Description = "Google Translate API version. Default to V2. If V3, a client secret JSON file is expected.")]
	//	public string ApiVersion { get; set; } = "V2";

	//	[CommandLineOption(Aliases = "CSF", Description = "Google Cloud Translate V3 does not support API key but rich ways of authentications. This app uses client secret JSON file you could download from your Google Cloud Service account.")]
	//	public string ClientSecretFile { get; set; }

	//	[CommandLineOption(Aliases = "h ?", Name = "Help", Description = "Shows this help text")]
	//	public bool Help
	//	{
	//		get;
	//		set;
	//	}


	//}
}
