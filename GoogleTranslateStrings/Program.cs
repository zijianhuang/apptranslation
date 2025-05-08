using Fonlow.TranslationProgram;
using Fonlow.TranslationProgram.Abstract;
using Microsoft.Extensions.Logging;

namespace GoogleTranslateStrings
{
	sealed class Program
	{
		static async Task<int> Main(string[] args)
		{
			using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
			var logger = loggerFactory.CreateLogger("program");
			var options = new Options();
			var errorCode = CliOptionsParser.Parse(args, options, DisplayExamples, logger);
			if (errorCode == 0)
			{
				var translationProgram = new TranslationProgramStringsWithGoogleTranslate(options, logger);
				var r = await translationProgram.Execute().ConfigureAwait(false);
				return r;
			}

			return errorCode;
		}


		static void DisplayExamples()
		{
			Console.WriteLine(
@"Examples:
GoogleTranslateStrings.exe /AK=YourGoogleTranslateV2ApiKey /SL=en /TL=zh-hant /F:strings.zh-hant.xml ---- For in-place translation when strings.zh-hant.xml is not yet translated
GoogleTranslateStrings.exe /AK=YourGoogleTranslateV2ApiKey /SL=en /TL=ja /F:strings.xml /TF:strings.ja.xml ---- from the source locale file to a new target file in Japanese
GoogleTranslateStrings.exe /AK=YourGoogleTranslateV2ApiKey /F:myUiMessages.xml /TF:myUiMessages.es.xml /TL=es ---- From the source template file to a new target file in Spanish.
GoogleTranslateStrings.exe /AV=v3 /CSF=client_secret.json /B  /SL=en /TL=es /F:myUiMessages.es.xml ---- Use Google Cloud Translate V3 and batch mode.
"
			);
		}
	}


}
