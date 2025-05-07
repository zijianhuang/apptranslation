using Fonlow.Cli;
using Fonlow.ResxTranslate;
using Fonlow.Translate;
using Fonlow.TranslationProgram.Abstract;
using Fonlow.TranslationProgram.GoogleTranslate;
using Microsoft.Extensions.Logging;

namespace Fonlow.TranslationProgram
{
	[CliManager(Description = "Use Google Translate v2 or v3 to translate Microsoft ResX", OptionSeparator = "/", Assignment = ":")]
	internal class OptionsForResxWithGoogleTranslate : OptionsWithGoogleTranslate
	{
	}

	internal class TranslationProgramResxWithGoogleTranslate : TranslationProgramWithGoogleTranslate
	{
		public TranslationProgramResxWithGoogleTranslate(OptionsForResxWithGoogleTranslate options, ILogger logger) : base(new ResxTranslation(), options, logger)
		{
		}

		public override void DisplayExamples()
		{
			Console.WriteLine(
@"Examples:
GoogleTranslateResx.exe /AK=YourGoogleTranslateV2ApiKey /SL=en /TL=zh-hant /F:AppResources.zh-hant.resx ---- For in-place translation when AppResources.zh-hant.resx is not yet translated
GoogleTranslateResx.exe /AK=YourGoogleTranslateV2ApiKey /SL=en /TL=ja /F:strings.xml /TF:AppResources.ja.resx ---- from the source locale file to a new target file in Japanese
GoogleTranslateResx.exe /AK=YourGoogleTranslateV2ApiKey /F:AppResources.resx /TF:AppResources.es.resx /TL=es ---- From the source template file to a new target file in Spanish.
GoogleTranslateResx.exe /AV=v3 /CSF=client_secret.json /B  /SL=en /TL=es /F:AppResources.es.resx ---- Use Google Cloud Translate V3 and batch mode.
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
