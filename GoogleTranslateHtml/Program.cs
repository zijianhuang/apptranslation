using Fonlow.TranslationProgram;
using Fonlow.TranslationProgram.Abstract;
using Fonlow.TranslationProgram.GoogleTranslate;
using Microsoft.Extensions.Logging;

namespace GoogleTranslateHtml
{
	sealed class Program
	{
		static async Task<int> Main(string[] args)
		{
			Console.OutputEncoding = System.Text.Encoding.Unicode;
			using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
			var logger = loggerFactory.CreateLogger("program");
			var options = new OptionsForHtmlWithGoogleTranslate();
			var errorCode = CliOptionsParser.Parse(args, options, DisplayExamples, logger);
			if (options.ListGlossaries)
			{
				await ListGlossariesHelper.ListGlossaries(options).ConfigureAwait(false);
				return 0;
			}

			if (errorCode == 0)
			{
				var translationProgram = new TranslationProgramHtmlTextWithGoogleTranslate(options, logger);
				var r = await translationProgram.Execute().ConfigureAwait(false);
				return r;
			}

			return errorCode;
		}


		static void DisplayExamples()
		{
			Console.WriteLine(
@"Examples:
GoogleTranslateHtml.exe /AKF=apikey.txt /SL=en /TL=""zh-hant"" /F=../Tests/template1.html /TF=../Tests/template1.zh-Hant.html -- HTML document
GoogleTranslateHtml.exe /CSF=$GTV3KeyFile /AV=V3 /SL=en /TL=""de"" /XPaths=`//body/h1` /B /F=../Tests/template1.html /TF=../Tests/template1.de.html -- HTML nodes
GoogleTranslateHtml.exe /CSF=client_secret.json /LG ---- List glossaires
"
			);
		}
	}


}
