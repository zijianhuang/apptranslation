using Fonlow.Cli;
using Fonlow.TranslationProgram.GoogleTranslate;
using Microsoft.Extensions.Logging;
using Fonlow.SvgTextTranslate;
using Fonlow.Translate;

namespace Fonlow.TranslationProgram
{
	[CliManager(Description = "Use Google Translate v2 or v3 to translate SVG Text", OptionSeparator = "/", Assignment = ":")]
	internal sealed class Options : OptionsWithGoogleTranslate
	{
	}

	internal sealed class TranslationProgramSvgTextWithGoogleTranslate : TranslationProgramWithGoogleTranslate
	{
		public TranslationProgramSvgTextWithGoogleTranslate(OptionsWithGoogleTranslate options, ILogger logger) : base(new SvgTextTranslation(), options, logger)
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
