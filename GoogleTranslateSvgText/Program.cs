using Fonlow.TranslationProgram;
using Fonlow.TranslationProgram.Abstract;
using Microsoft.Extensions.Logging;

namespace GoogleTranslateSvgText
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
				var translationProgram = new TranslationProgramSvgTextWithGoogleTranslate(options, logger);
				var r = await translationProgram.Execute().ConfigureAwait(false);
				return r;
			}

			return errorCode;
		}


		static void DisplayExamples()
		{
			Console.WriteLine(
@"Examples:
GoogleTranslateSvgText.exe /AK=YourGoogleTranslateV2ApiKey /SL=en /TL=zh-hant /F:myart.zh-hant.svg ---- For in-place translation when myart.zh-hant.svg is not yet translated
GoogleTranslateSvgText.exe /AK=YourGoogleTranslateV2ApiKey /SL=en /TL=ja /F:myart.svg /TF:myart.ja.svg ---- from the source locale file to a new target file in Japanese
GoogleTranslateSvgText.exe /AK=YourGoogleTranslateV2ApiKey /F:myUiMessages.svg /TF:myUiMessages.es.svg /TL=es ---- From the source template file to a new target file in Spanish.
GoogleTranslateSvgText.exe /AV=v3 /CSF=client_secret.json /B  /SL=en /TL=es /F:myUiMessages.es.svg ---- Use Google Cloud Translate V3 and batch mode.
"
			);
		}
	}


}
