using Fonlow.TranslationProgram;
using Fonlow.TranslationProgram.Abstract;
using Microsoft.Extensions.Logging;

namespace MsTranslatorResx
{
	sealed class Program
	{
		static async Task<int> Main(string[] args)
		{
			Console.OutputEncoding = System.Text.Encoding.Unicode;
			using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
			var logger = loggerFactory.CreateLogger("program");
			var options = new Options();
			var errorCode = CliOptionsParser.Parse(args, options, DisplayExamples, logger);
			if (errorCode == 0)
			{
				var translationProgram = new TranslationProgramResxWithMsTranslator(options, logger);
				var r = await translationProgram.Execute().ConfigureAwait(false);
				return r;
			}

			return errorCode;
		}

		static void DisplayExamples()
		{
			Console.WriteLine(
@"Examples:
MsTranslatorResx.exe /AK=MsTranslatorApiKey /AG=australiaeast /SL=en /TL=zh-hant /F:AppResources.zh-hant.resx ---- For in-place translation when AppResources.zh-hant.resx is not yet translated
MsTranslatorResx.exe /AK=MsTranslatorApiKey /AG=australiaeast /SL=en /TL=ja /F:strings.xml /TF:AppResources.ja.resx ---- from the source locale file to a new target file in Japanese
MsTranslatorResx.exe /AK=MsTranslatorApiKey /AG=australiaeast /F:AppResources.resx /TF:AppResources.es.resx /TL=es ---- From the source template file to a new target file in Spanish.
"
			);
		}

	}

}
