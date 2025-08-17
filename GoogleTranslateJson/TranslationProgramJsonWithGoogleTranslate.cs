using Fonlow.Cli;
using Fonlow.JsonTranslate;
using Fonlow.Translate;
using Fonlow.TranslationProgram.GoogleTranslate;
using Microsoft.Extensions.Logging;
using Plossum.CommandLine;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Fonlow.TranslationProgram
{
	[CliManager(Description = "Use Google Translate v2 or v3 to translate selected string value properties of JSON object", OptionSeparator = "/", Assignment = ":")]
	internal sealed class OptionsForJsonWithGoogleTranslate : OptionsWithGoogleTranslate
	{
		[CommandLineOption(Aliases = "PS", Description = "JSON object properties to be translated, e.g., /PS=\"parent.folder.name\" \"parent.fonlder.address\"")]
		public string[] Properties { get; set; } = [];

		[CommandLineOption(Aliases = "PSF", Description = "Each line declares a JSON object property to be translated, e.g., /PSF=JsonProperties.txt")]
		public string PropertiesFile { get; set; }

		[CommandLineOption(Aliases = "SC", Description = "0: JsonSerializerOptions.Default, 1: JsonSerializerOptions.Web, 3: Custom with option Intented and UnsafeRelaxedJsonEscaping.")]
		public int SerializationConfig { get; set; } = 0;

		[CommandLineOption(Aliases = "Ind", Description = "Outputted text in indented, when SerializationConfig=2.")]
		public bool Indented { get; set; }

		[CommandLineOption(Aliases = "NUE", Description = "Outputted Unicode characters not escaped, when SerializationConfig=2.")]
		public bool UnsafeRelaxedJsonEscaping { get; set; }

	}

	internal sealed class TranslationProgramJsonWithGoogleTranslate : TranslationProgramWithGoogleTranslate
	{
		public TranslationProgramJsonWithGoogleTranslate(OptionsForJsonWithGoogleTranslate options, ILogger logger) : base(CreateJsonProcessor(options), options, logger)
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

		static IResourceTranslation CreateJsonProcessor(OptionsForJsonWithGoogleTranslate options)
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
			JsonSerializerOptions serializerOptions;

			switch (options.SerializationConfig)
			{
				case 1:
					serializerOptions = JsonSerializerOptions.Web;
					break;
				case 2:
					serializerOptions = new JsonSerializerOptions();
					if (options.Indented)
					{
						serializerOptions.WriteIndented = true;
					}

					if (options.UnsafeRelaxedJsonEscaping)
					{
						serializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
					}
					break;
				default:
					serializerOptions = JsonSerializerOptions.Default;
					break;
			}

			d.SetJsonSerializerOptions(serializerOptions);

			return d;
		}
	}


}
