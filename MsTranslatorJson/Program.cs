using Fonlow.TranslationProgram;
using Fonlow.TranslationProgram.Abstract;
using Microsoft.Extensions.Logging;

namespace MsTranslatorJson
{
	sealed class Program
	{
		static async Task<int> Main(string[] args)
		{
			Console.OutputEncoding = System.Text.Encoding.Unicode;
			using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
			var logger = loggerFactory.CreateLogger("program");
			var options = new OptionsForJsonWithMsTranslator();
			var errorCode = CliOptionsParser.Parse(args, options, DisplayExamples, logger);
			if (errorCode == 0)
			{
				var translationProgram = new TranslationProgramJsonWithMsTranslator(options, logger);
				var r = await translationProgram.Execute().ConfigureAwait(false);
				return r;
			}

			return errorCode;
		}

		static void DisplayExamples()
		{
			Console.WriteLine(
@"Examples:
MsTranslatorteJson.exe /AK=YourMsTranslatorteApiKey /RG=australiaeast /SL=en /TL=zh-hant /F:jsonld.zh-hant.json /PS:data.user.name data.user.address ---- For in-place translation when jsonld.zh-hant.json is not yet translated
MsTranslatorteJson.exe /AK=YourMsTranslatorteApiKey /RG=australiaeast /SL=en /TL=ja /F:jsonld.json /TF:jsonld.ja.json /PS:data.user.name ---- from the source locale file to a new target file in Japanese
MsTranslatorteJson.exe /AK=YourMsTranslatorteApiKey /RG=australiaeast /F:jsonld.json /TF:jsonld.es.json /TL=es /PS:data.user.name ---- From the source template file to a new target file in Spanish.
"
			);
		}

	}
}
