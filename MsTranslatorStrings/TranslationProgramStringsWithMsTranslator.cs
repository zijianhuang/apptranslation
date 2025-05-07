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

		public override void DisplayExamples()
		{
			Console.WriteLine(
@"Examples:
MsTranslatorStrings.exe /AK=MsTranslatorApiKey /SL=en /TL=zh-hant /F:AppResources.zh-hant.xml ---- For in-place translation when AppResources.zh-hant.xml is not yet translated
MsTranslatorStrings.exe /AK=MsTranslatorApiKey /SL=en /TL=ja /F:strings.xml /TF:AppResources.ja.xml ---- from the source locale file to a new target file in Japanese
MsTranslatorStrings.exe /AK=MsTranslatorApiKey /F:AppResources.xml /TF:AppResources.es.xml /TL=es ---- From the source template file to a new target file in Spanish.
"
			);
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
