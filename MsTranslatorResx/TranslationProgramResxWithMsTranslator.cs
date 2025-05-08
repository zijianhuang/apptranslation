using Fonlow.Cli;
using Fonlow.ResxTranslate;
using Fonlow.Translate;
using Fonlow.TranslationProgram.MsTranslator;
using Microsoft.Extensions.Logging;

namespace Fonlow.TranslationProgram
{
	[CliManager(Description = "Use Microsoft Azure AI Translator to translate Microsoft ResX", OptionSeparator = "/", Assignment = ":")]
	internal class Options : OptionsWithMsTranslator
	{
	}

	internal class TranslationProgramResxWithMsTranslator : TranslationProgramWithMsTranslator
	{
		public TranslationProgramResxWithMsTranslator(Options options, ILogger logger) : base(new ResxTranslation(), options, logger)
		{
		}

		protected override IProgressDisplay CreateProgressDisplay()
		{
			return new ResourceProgressDisplay();
		}

		protected override void InitializeResourceTranslation()
		{
			resourceTranslation.SetBatchMode(optionsBase.Batch);
			resourceTranslation.SetSourceFile(optionsBase.SourceFile);
			var targetFile = string.IsNullOrEmpty(optionsBase.TargetFile) ? optionsBase.SourceFile : optionsBase.TargetFile;
			resourceTranslation.SetTargetFile(targetFile);
		}
	}


}
