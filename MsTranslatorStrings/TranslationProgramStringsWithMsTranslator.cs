using Fonlow.Cli;
using Fonlow.StringsTranslate;
using Fonlow.Translate;
using Fonlow.TranslationProgram.MsTranslator;
using Microsoft.Extensions.Logging;

namespace Fonlow.TranslationProgram
{
	[CliManager(Description = "Use Microsoft Azure AI Translator to translate Android String Resource", OptionSeparator = "/", Assignment = ":")]
	internal class Options : OptionsWithMsTranslator
	{
	}

	internal class TranslationProgramStringsWithMsTranslator : TranslationProgramWithMsTranslator
	{
		public TranslationProgramStringsWithMsTranslator(Options options, ILogger logger) : base(new StringsTranslation(), options, logger)
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
