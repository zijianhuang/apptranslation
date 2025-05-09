using Fonlow.TranslationProgram;
using Fonlow.TranslationProgram.Abstract;
using Microsoft.Extensions.Logging;

namespace MsTranslatorXliff
{
	class Program
	{
		static async Task<int> Main(string[] args)
		{
			using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
			var logger = loggerFactory.CreateLogger("program");
			var options = new OptionsForXliffWithMsTranslator();
			var errorCode = CliOptionsParser.Parse(args, options, DisplayExamples, logger);
			if (errorCode == 0)
			{
				var translationProgram = new TranslationProgramXliffWithMsTranslator(options, logger);
				var r = await translationProgram.Execute().ConfigureAwait(false);
				return r;
			}

			return errorCode;
		}

		static void DisplayExamples()
		{
			Console.WriteLine(
@"Examples:
MsTranslatorXliff.exe /AK=MsTranslatorApiKey /AG=australiaeast /F=myUiMessages.es.xlf ---- For in-place translation.
MsTranslatorXliff.exe /AK=MsTranslatorApiKey /AG=australiaeast /F:myUiMessages.ja.xlf /TF:myUiMessagesTranslated.ja.xlf ---- from the source locale file to a new target file in Japanese
MsTranslatorXliff.exe /AK=MsTranslatorApiKey /AG=australiaeast /F:myUiMessages.xlf /TF:myUiMessages.es.xlf /TL=es ---- From the source template file to a new target file in Spanish.
"
			);

		}

	}
}
