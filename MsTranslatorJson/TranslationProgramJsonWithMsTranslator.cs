using Fonlow.Cli;
using Fonlow.JsonTranslate;
using Fonlow.Translate;
using Fonlow.TranslationProgram.MsTranslator;
using Microsoft.Extensions.Logging;
using Plossum.CommandLine;

namespace Fonlow.TranslationProgram
{
	[CliManager(Description = "Use Microsoft Azure AI Translator to translate JSON object", OptionSeparator = "/", Assignment = ":")]
	sealed internal class OptionsForJsonWithMsTranslator : OptionsWithMsTranslator
	{
		[CommandLineOption(Aliases = "PS", Description = "JSON object properties to be translated, e.g., /PS=\"parent.folder.name\" \"parent.folder.address\"")]
		public string[] Properties { get; set; } = [];

		[CommandLineOption(Aliases = "PSF", Description = "Each line declares a JSON object property to be translated, e.g., /PSF=JsonProperties.txt")]
		public string PropertiesFile { get; set; }
	}

	sealed internal class TranslationProgramJsonWithMsTranslator : TranslationProgramWithMsTranslator
	{
		public TranslationProgramJsonWithMsTranslator(OptionsForJsonWithMsTranslator options, ILogger logger) : base(CreateMetaProcessor(options), options, logger)
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

		static JsonObjectTranslation CreateMetaProcessor(OptionsForJsonWithMsTranslator options)
		{
			var d = new JsonObjectTranslation();
			if (string.IsNullOrEmpty(options.PropertiesFile))
			{
				d.SetProperties(options.Properties);
			}
			else
			{
				d.SetProperties(File.ReadAllLines(options.PropertiesFile));
			}

			return d;
		}
	}


}
