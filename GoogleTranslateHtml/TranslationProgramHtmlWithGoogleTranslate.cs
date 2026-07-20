using Fonlow.Cli;
using Fonlow.Translate;
using Fonlow.TranslationProgram.GoogleTranslate;
using Microsoft.Extensions.Logging;
using Plossum.CommandLine;

namespace Fonlow.TranslationProgram
{
	[CliManager(Description = "Use Google Translate v2 or v3 to translate HTML document or nodes based on XPaths", OptionSeparator = "/", Assignment = ":")]
	internal sealed class OptionsForHtmlWithGoogleTranslate : OptionsWithGoogleTranslate
	{
		[CommandLineOption(Aliases = "XPS", Description = "HTML nodes to be translated represented by Xpaths, e.g., /XPS=`//h2` `ul` in Windows CMD, and add --% after the command in PowerShell 5.1, and for running in PowerShell 7 or using complex XPath queries, utilize XPathsFile")]
		public string[] XPaths { get; set; } = [];

		[CommandLineOption(Aliases = "XPSF", Description = "Each line declares a XPath for HTML nodes to be translated, e.g., /XPSF=XPaths.txt")]
		public string XPathsFile { get; set; }

	}

	internal sealed class TranslationProgramHtmlTextWithGoogleTranslate : TranslationProgramWithGoogleTranslate
	{
		public TranslationProgramHtmlTextWithGoogleTranslate(OptionsForHtmlWithGoogleTranslate options, ILogger logger) : base(CreateMetaProcessor(options), options, logger)
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

		static Fonlow.HtmlTranslate.HtmlTranslation CreateMetaProcessor(OptionsForHtmlWithGoogleTranslate options)
		{
			var d = new Fonlow.HtmlTranslate.HtmlTranslation();
			if (string.IsNullOrEmpty(options.XPathsFile))
			{
				d.SetXPaths(options.XPaths);
			}
			else
			{
				d.SetXPaths(File.ReadAllLines(options.XPathsFile));
			}

			return d;
		}

	}


}
