using Antlr4.Runtime.Misc;
using Fonlow.Cli;
using Fonlow.Translate;
using Microsoft.Extensions.Logging;

namespace Fonlow.TranslationProgram.Abstract
{
	public abstract class TranslationProgramBase
	{
		protected TranslationProgramBase(IResourceTranslation resourceTranslation, OptionsBase optionsBase, ILogger logger)
		{
			this.resourceTranslation = resourceTranslation;
			this.optionsBase = optionsBase;
			this.logger = logger;
		}

		protected readonly OptionsBase optionsBase;
		readonly protected ILogger logger;
		protected readonly IResourceTranslation resourceTranslation;

		public abstract ITranslate CreateTranslator(out int errorCode);

		protected abstract IProgressDisplay CreateProgressDisplay();

		protected abstract void InitializeResourceTranslation();

		/// <summary>
		/// Main execute function of console app
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public async Task<int> Execute()
		{
			try
			{
				ITranslate translator = CreateTranslator(out int errorCode);
				if (translator == null)
				{
					return errorCode;
				}

				InitializeResourceTranslation();
				var c = await resourceTranslation.Translate(translator, logger, CreateProgressDisplay()).ConfigureAwait(false);
				Console.WriteLine();
				Console.WriteLine($"Total translated: {c}");
			}
			catch (ArgumentException ex)
			{
				logger.LogError(ex.Message);
				return 100;
			}
			catch (Exception ex)
			{
				return HandleTranslationEngineException(ex);
			}

			return 0;
		}

		protected abstract int HandleTranslationEngineException(Exception ex);


	}

	public static class CliOptionsParser
	{
		public static int Parse(string[] args, OptionsBase optionsBase, Action displayExamples, ILogger logger)
		{
			var parser = new CommandLineParser(optionsBase);
			Console.WriteLine(parser.ApplicationDescription);

			parser.Parse();
			if (args.Length == 0 || optionsBase.Help)
			{
				Console.WriteLine(parser.UsageInfo.ToString());
				displayExamples?.Invoke();

				return 1;
			}

			if (parser.HasErrors)
			{
				logger.LogWarning(parser.ErrorMessage);
				Console.WriteLine(parser.UsageInfo.GetOptionsAsString());
				return 2;
			}

			if (string.IsNullOrEmpty(optionsBase.SourceFile))
			{
				logger.LogWarning("Need SoureFile");
				return 10;
			}

			if (!Path.Exists(optionsBase.SourceFile))
			{
				logger.LogWarning($"{optionsBase.SourceFile} NOT exists");
				return 11;
			}

			return 0;
		}
	}

}
