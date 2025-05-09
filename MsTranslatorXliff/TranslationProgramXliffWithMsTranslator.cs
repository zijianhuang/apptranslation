using Fonlow.Cli;
using Fonlow.XliffTranslate;
using Fonlow.Translate;
using Fonlow.TranslationProgram.MsTranslator;
using Microsoft.Extensions.Logging;
using Plossum.CommandLine;

namespace Fonlow.TranslationProgram
{
	[CliManager(Description = "Use Microsoft Azure AI Translator to translate XLIFF v1.2 or v2.0 file.", OptionSeparator = "/", Assignment = ":")]
	sealed internal class OptionsForXliffWithMsTranslator : OptionsWithMsTranslator
	{
		[CommandLineOption(Aliases = "SS", Description = "For translation unit of states. Default to new for v1.2 and initial for v2.0, e.g., /SS=\"initial\" \"translated\"")]
		public string[] ForStates { get; set; } = [];

		[CommandLineOption(Aliases = "NCS", Description = "Not to change the state of translation unit to translated after translation.")]
		public bool NotChangeState { get; set; }

	}

	sealed internal class TranslationProgramXliffWithMsTranslator : TranslationProgramWithMsTranslator
	{
		public TranslationProgramXliffWithMsTranslator(OptionsForXliffWithMsTranslator options, ILogger logger) : base(CreateXliffProcessor(options), options, logger)
		{
			this.optionsForXliff= options;
		}

		readonly OptionsForXliffWithMsTranslator optionsForXliff;

		static IXliffTranslation CreateXliffProcessor(OptionsForXliffWithMsTranslator options)
		{
			var xliffProcessor = XliffProcessorFactory.CreateXliffGT2(options.SourceFile, options.Batch, (v) =>
			{
				if (v == "1.2")
				{
					if (options.ForStates.Length == 0)
					{
						options.ForStates = ["new"];
					}
				}
				else if (v == "2.0")
				{
					if (options.ForStates.Length == 0)
					{
						options.ForStates = ["initial"];
					}
				}

				Console.WriteLine($"Processing XLIFF v{v}...");
			});

			return xliffProcessor;

		}

		protected override IProgressDisplay CreateProgressDisplay()
		{
			return new TmProgressDisplay();
		}

		protected override void InitializeResourceTranslation()
		{
			resourceTranslation.SetBatchMode(optionsBase.Batch);
			resourceTranslation.SetSourceFile(optionsBase.SourceFile);
			var targetFile = string.IsNullOrEmpty(optionsBase.TargetFile) ? optionsBase.SourceFile : optionsBase.TargetFile;
			resourceTranslation.SetTargetFile(targetFile);

			(resourceTranslation as IXliffTranslation)?.SetForStates(optionsForXliff.ForStates);
			(resourceTranslation as IXliffTranslation)?.SetUnchangeState(optionsForXliff.NotChangeState);

		}
	}


}
