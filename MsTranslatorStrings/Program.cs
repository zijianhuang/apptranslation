using Fonlow.TranslationProgram;
using Fonlow.TranslationProgram.Abstract;
using Microsoft.Extensions.Logging;

namespace MsTranslatorStrings
{
	internal class Program
	{
		static async Task<int> Main(string[] args)
		{
			using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
			var logger = loggerFactory.CreateLogger("program");
			var options = new Options();

			var errorCode = CliOptionsParser.Parse(args, options, DisplayExamples, logger);
			if (errorCode == 0)
			{
				var translationProgram = new TranslationProgramStringsWithMsTranslator(options, logger);
				var r = await translationProgram.Execute().ConfigureAwait(false);
				return r;
			}

			return errorCode;
		}

		static void DisplayExamples()
		{
			Console.WriteLine(
@"Examples:
MsTranslatorStrings.exe /AK=MsTranslatorApiKey /SL=en /TL=zh-hant /F:AppResources.zh-hant.xml ---- For in-place translation when AppResources.zh-hant.xml is not yet translated
MsTranslatorStrings.exe /AK=MsTranslatorApiKey /SL=en /TL=ja /F:strings.xml /TF:AppResources.ja.xml ---- from the source locale file to a new target file in Japanese
MsTranslatorStrings.exe /AK=MsTranslatorApiKey /F:AppResources.xml /TF:AppResources.es.xml /TL=es ---- From the source template file to a new target file in Spanish.
"
			);
		}
	}



}
