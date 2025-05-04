using Fonlow.TranslationProgram;
using Microsoft.Extensions.Logging;

namespace MsTranslatorResx
{
	internal class Program
	{
		static async Task<int> Main(string[] args)
		{
			using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
			var logger = loggerFactory.CreateLogger("program");
			var options = new Options();

			var translationProgram = new TranslationProgramResxWithMsTranslator(options, logger);
			var r = await translationProgram.Execute(args).ConfigureAwait(false);
			return r;
		}
	}

}
