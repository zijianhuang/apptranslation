using Fonlow.Cli;
using Fonlow.JsonTranslate;
using Fonlow.Translate;
using Fonlow.TranslationProgram.GoogleTranslate;
using Microsoft.Extensions.Logging;
using Plossum.CommandLine;

namespace Fonlow.TranslationProgram
{
	[CliManager(Description = "Use Google Translate v2 or v3 to translate selected string value properties of JSON object", OptionSeparator = "/", Assignment = ":")]
	internal sealed class OptionsForJsonWithGoogleTranslate : OptionsWithGoogleTranslate
	{
		[CommandLineOption(Aliases = "PS", Description = "JSON object properties to be translated represented by JSONPath, e.g., /PS=\"parent.folder.name\" \"parent.folder.address\"")]
		public string[] Properties { get; set; } = [];

		[CommandLineOption(Aliases = "PSF", Description = "Each line declares a JSON object property to be translated represented by JSONPath is accepted, e.g., /PSF=JsonProperties.txt")]
		public string PropertiesFile { get; set; }
	}

	internal sealed class TranslationProgramJsonWithGoogleTranslate : TranslationProgramWithGoogleTranslate
	{
		public TranslationProgramJsonWithGoogleTranslate(OptionsForJsonWithGoogleTranslate options, ILogger logger) : base(CreateMetaProcessor(options), options, logger)
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

		static JsonObjectTranslation CreateMetaProcessor(OptionsForJsonWithGoogleTranslate options)
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
