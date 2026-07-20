using Fonlow.TranslationProgram;
using Fonlow.TranslationProgram.Abstract;
using Microsoft.Extensions.Logging;

namespace MsTranslatorHtml
{
	sealed class Program
	{
		static async Task<int> Main(string[] args)
		{
			Console.OutputEncoding = System.Text.Encoding.Unicode;
			using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
			var logger = loggerFactory.CreateLogger("program");
			var options = new OptionsForHtmlWithMsTranslator();
			var errorCode = CliOptionsParser.Parse(args, options, DisplayExamples, logger);
			if (errorCode == 0)
			{
				var translationProgram = new TranslationProgramHtmlTextWithMsTranslator(options, logger);
				var r = await translationProgram.Execute().ConfigureAwait(false);
				return r;
			}

			return errorCode;
		}


		static void DisplayExamples()
		{
			Console.WriteLine(
@"Examples:
MsTranslatorHtml.exe /AK=abcdefg /RG=uswest /SL=en /TL=""zh-hant"" /XPaths=`//body/h1` /B /F=../Tests/template1.svg /TF=../Tests/template1.zh-Hant.svg
"
			);
		}
	}


}
