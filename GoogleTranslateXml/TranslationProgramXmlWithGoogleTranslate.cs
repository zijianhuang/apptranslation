using Fonlow.Cli;
using Fonlow.Translate;
using Fonlow.TranslationProgram.GoogleTranslate;
using Microsoft.Extensions.Logging;
using Plossum.CommandLine;

namespace Fonlow.TranslationProgram
{
	[CliManager(Description = "Use Google Translate v2 or v3 to translate XML Text based on XPaths", OptionSeparator = "/", Assignment = ":")]
	internal sealed class OptionsForXmlWithGoogleTranslate : OptionsWithGoogleTranslate
	{
		[CommandLineOption(Aliases = "XPS", Description = "XML text nodes to be translated represented by Xpaths, e.g., /XPS=\"//svg:text/svg:tspan\" \"//ns:pp/ns:span\"")]
		public string[] XPaths { get; set; } = [];

		[CommandLineOption(Aliases = "XPSF", Description = "Each line declares a XPath for text nodes to be translated, e.g., /XPSF=XPaths.txt")]
		public string XPathsFile { get; set; }

	}

	internal sealed class TranslationProgramXmlTextWithGoogleTranslate : TranslationProgramWithGoogleTranslate
	{
		public TranslationProgramXmlTextWithGoogleTranslate(OptionsForXmlWithGoogleTranslate options, ILogger logger) : base(CreateJsonProcessor(options), options, logger)
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

		static Fonlow.XmlTranslate.XmlTextTranslation CreateJsonProcessor(OptionsForXmlWithGoogleTranslate options)
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
