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
	[CliManager(Description = "Use Google Translate v2 or v3 to translate JSON object", OptionSeparator = "/", Assignment = ":")]
	internal sealed class OptionsForJsonWithGoogleTranslate : OptionsWithGoogleTranslate
	{
		[CommandLineOption(Aliases = "PS", Description = "Json object properties to be translated, e.g., /SS=\"parent.folder.name\" \"parent.fonlder.address\"")]
		public string[] Properties { get; set; } = [];

		[CommandLineOption(Aliases = "Ind", Description = "Outputted text in indented")]
		public bool Indented { get; set; }

		[CommandLineOption(Aliases = "NUE", Description = "Outputted Unicode characters not escaped.")]
		public bool UnsafeRelaxedJsonEscaping { get; set; }

		[CommandLineOption(Aliases = "Web", Description = "Use JsonSerializerOptions.Web, otherwise, Default.")]
		public bool UseWebConfig { get; set; }

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
			d.SetProperties(options.Properties);
			var serializerOptions = options.UseWebConfig ? JsonSerializerOptions.Web : JsonSerializerOptions.Default;
			//if (options.Indented)
			//{
			//	serializerOptions.WriteIndented = true;
			//}

			//if (options.UnsafeRelaxedJsonEscaping)
			//{
			//	serializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
			//}

			return d;
		}
	}


}
