using Fonlow.Cli;
using Fonlow.Translate;
using Fonlow.TranslationProgram.MsTranslator;
using Microsoft.Extensions.Logging;
using Plossum.CommandLine;

namespace Fonlow.TranslationProgram
{
	[CliManager(Description = "Use MS Translator to translate XML Text based on XPaths", OptionSeparator = "/", Assignment = ":")]
	public sealed class OptionsForXmlWithMsTranslator : OptionsWithMsTranslator
	{
		[CommandLineOption(Aliases = "XPS", Description = "XML text nodes to be translated represented by Xpaths, e.g., /XPS=`//svg:text/svg:tspan` `//ns:pp/ns:span` in Windows CMD, and add --% after the command in PowerShell 5.1, and for running in PowerShell 7 or using complex XPath queries, utilize XPathsFile")]
		public string[] XPaths { get; set; } = [];

		[CommandLineOption(Aliases = "XPSF", Description = "Each line declares a XPath for text nodes to be translated, e.g., /XPSF=XPaths.txt")]
		public string XPathsFile { get; set; }

	}

	internal sealed class TranslationProgramXmlTextWithMsTranslator : TranslationProgramWithMsTranslator
	{
		public TranslationProgramXmlTextWithMsTranslator(OptionsForXmlWithMsTranslator options, ILogger logger) : base(CreateJsonProcessor(options), options, logger)
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

		static Fonlow.XmlTranslate.XmlTextTranslation CreateJsonProcessor(OptionsForXmlWithMsTranslator options)
		{
			var d = new Fonlow.XmlTranslate.XmlTextTranslation();
			if (string.IsNullOrEmpty(options.XPathsFile))
			{
				d.SetXPaths(options.XPaths);
				Console.WriteLine("Xpaths first: " + options.XPaths[0]);
			}
			else
			{
				d.SetXPaths(File.ReadAllLines(options.XPathsFile));
				//Console.WriteLine("XPathsFile: " + options.XPathsFile);
			}

			return d;
		}

	}


}
