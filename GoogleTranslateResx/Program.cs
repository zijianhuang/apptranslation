using Fonlow.TranslationProgram;
using Microsoft.Extensions.Logging;

namespace GoogleTranslateStrings
{
	class Program
	{
		static async Task<int> Main(string[] args)
		{
			using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
			var logger = loggerFactory.CreateLogger("program");
			var options = new OptionsForResxWithGoogleTranslate();

			var translationProgram = new TranslationProgramResxWithGoogleTranslate(options, logger);
			var r = await translationProgram.Execute(args).ConfigureAwait(false);
			return r;
		}
	}

}
