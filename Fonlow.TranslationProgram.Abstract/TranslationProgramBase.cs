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

		readonly OptionsBase optionsBase;
		readonly protected ILogger logger;
		readonly IResourceTranslation resourceTranslation;

		public abstract void DisplayExamples();

		public abstract ITranslate CreateTranslator(out int errorCode);

		public async Task<int> Execute(string[] args)
		{
			var parser = new CommandLineParser(optionsBase);
			Console.WriteLine(parser.ApplicationDescription);

			parser.Parse();
			if (args.Length == 0 || optionsBase.Help)
			{
				Console.WriteLine(parser.UsageInfo.ToString());
				DisplayExamples();

				return 0;
			}

			if (parser.HasErrors)
			{
				logger.LogWarning(parser.ErrorMessage);
				Console.WriteLine(parser.UsageInfo.GetOptionsAsString());
				return 1;
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

			var targetFile = string.IsNullOrEmpty(optionsBase.TargetFile) ? optionsBase.SourceFile : optionsBase.TargetFile;

			try
			{
				ITranslate translator = CreateTranslator(out int errorCode);
				if (translator == null)
				{
					return errorCode;
				}

				resourceTranslation.SetBatchMode(optionsBase.Batch);
				var c = await resourceTranslation.Translate(optionsBase.SourceFile, targetFile, translator, logger, ShowProgress).ConfigureAwait(false);
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

		static void ShowProgress(int current, int totalUnits)
		{
			Console.CursorLeft = 10;
			Console.Write($"{current} / {totalUnits}");
		}

	}

}
