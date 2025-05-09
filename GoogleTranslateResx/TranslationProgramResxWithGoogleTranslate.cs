using Fonlow.Cli;
using Fonlow.ResxTranslate;
using Fonlow.Translate;
using Fonlow.TranslationProgram.GoogleTranslate;
using Microsoft.Extensions.Logging;

namespace Fonlow.TranslationProgram
{
	[CliManager(Description = "Use Google Translate v2 or v3 to translate Microsoft ResX", OptionSeparator = "/", Assignment = ":")]
	internal sealed class OptionsForResxWithGoogleTranslate : OptionsWithGoogleTranslate
	{
	}

	internal sealed class TranslationProgramResxWithGoogleTranslate : TranslationProgramWithGoogleTranslate
	{
		public TranslationProgramResxWithGoogleTranslate(OptionsForResxWithGoogleTranslate options, ILogger logger) : base(new ResxTranslation(), options, logger)
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
