using Fonlow.TranslationProgram;
using Fonlow.TranslationProgram.Abstract;
using Microsoft.Extensions.Logging;

namespace GoogleTranslateXml
{
	sealed class Program
	{
		static async Task<int> Main(string[] args)
		{
			Console.OutputEncoding = System.Text.Encoding.Unicode;
			using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
			var logger = loggerFactory.CreateLogger("program");
			var options = new OptionsForXmlWithGoogleTranslate();
			var errorCode = CliOptionsParser.Parse(args, options, DisplayExamples, logger);
			if (errorCode == 0)
			{
				var translationProgram = new TranslationProgramXmlTextWithGoogleTranslate(options, logger);
				var r = await translationProgram.Execute().ConfigureAwait(false);
				return r;
			}

			return errorCode;
		}


		static void DisplayExamples()
		{
			Console.WriteLine(
@"Examples:
GoogleTranslateXml.exe /AKF=apikey.txt /SL=en /TL=""zh-hant"" /XPaths=`//svg:text/svg:tspan` /F=../Tests/template1.svg /TF=../Tests/template1.zh-Hant.svg
GoogleTranslateXml.exe /CSF=$GTV3KeyFile /AV=V3 /SL=en /TL=""zh-hant"" /XPaths=`//svg:text/svg:tspan` /B /F=../Tests/template1.svg /TF=../Tests/template1.zh-Hant.svg
"
			);
		}
	}


}
