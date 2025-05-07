using Fonlow.Cli;
using Fonlow.ResxTranslate;
using Fonlow.Translate.Abstract;
using Fonlow.Translate;
using Fonlow.TranslationProgram.Abstract;
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

		public override void DisplayExamples()
		{
			Console.WriteLine(
@"Examples:
MsTranslatorResx.exe /AK=MsTranslatorApiKey /SL=en /TL=zh-hant /F:AppResources.zh-hant.resx ---- For in-place translation when AppResources.zh-hant.resx is not yet translated
MsTranslatorResx.exe /AK=MsTranslatorApiKey /SL=en /TL=ja /F:strings.xml /TF:AppResources.ja.resx ---- from the source locale file to a new target file in Japanese
MsTranslatorResx.exe /AK=MsTranslatorApiKey /F:AppResources.resx /TF:AppResources.es.resx /TL=es ---- From the source template file to a new target file in Spanish.
"
			);
		}

		protected override IProgressDisplay CreateProgressDisplay()
		{
			return new ResourceProgressDisplay();
		}
	}


}
