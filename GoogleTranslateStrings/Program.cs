using Fonlow.TranslationProgram;
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

			var translationProgram = new TranslationProgramStringsWithGoogleTranslate(options, logger);
			var r = await translationProgram.Execute(args).ConfigureAwait(false);
			return r;
		}

	}
}
