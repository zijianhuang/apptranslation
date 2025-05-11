using Fonlow.Cli;
using Fonlow.XliffResX;
using Microsoft.Extensions.Logging;
using Plossum.CommandLine;

namespace XliffResXConverterProgram
{
	internal class Program
	{
		static int Main(string[] args)
		{
			Console.OutputEncoding = System.Text.Encoding.Unicode;
			using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
			var logger = loggerFactory.CreateLogger("program");
			var options = new Options();
			var parser = new CommandLineParser(options);
			Console.WriteLine(parser.ApplicationDescription);

			parser.Parse();
			if (args.Length == 0 || options.Help)
			{
				Console.WriteLine(parser.UsageInfo.ToString());
				Console.WriteLine(
@"Examples:
XliffResXConverter.exe /A=new /SL=en /TL=zh-hant /RXS:AppResources.resx /RXL=AppResources.zh-hant.resx /XF=c:/TranslationMemory/AppResources.zh-hant.xlf ---- For creating translation memory.
XliffResXConverter.exe /a=merge /RXS:AppResources.resx /RXL=AppResources.zh-hant.resx /XF=c:/TranslationMemory/AppResources.zh-hant.xlf ---- For updating translation memory with 2 ResX files.
XliffResXConverter.exe /A=MergeBack /RXL=AppResources.zh-hant.resx /XF=c:/TranslationMemory/AppResources.zh-hant.xlf ---- After translating XLIFF, merge the translated content back to language ResX file.
"
				);

				return 0;
			}

			if (parser.HasErrors)
			{
				logger.LogWarning(parser.ErrorMessage);
				Console.WriteLine(parser.UsageInfo.GetOptionsAsString());
				return 1;
			}

			if (string.IsNullOrEmpty(options.Action))
			{
				logger.LogWarning("Need Action");
				return 10;
			}

			if (string.IsNullOrEmpty(options.XliffFile))
			{
				logger.LogWarning("Need XliffFile");
				return 11;
			}

			if (options.Action.Equals("new", StringComparison.OrdinalIgnoreCase))
			{
				XliffResXConverter.ConvertResXToXliff12(options.SourceResX, options.LangResX, options.SourceLang, options.TargetLang, options.XliffFile, options.GroupId);
				Console.WriteLine($"Created {options.XliffFile}");
			}
			else if (options.Action.Equals("merge", StringComparison.OrdinalIgnoreCase))
			{
				var r = XliffResXConverter.MergeResXToXliff12(options.SourceResX, options.LangResX, options.XliffFile, logger);
				Console.WriteLine($"Added: {r.Item1}; Removed: {r.Item2} .");
				Console.WriteLine($"Updated {options.XliffFile}");
			}
			else if (options.Action.Equals("mergeback", StringComparison.OrdinalIgnoreCase))
			{
				XliffResXConverter.MergeTranslationOfXliff12BackToResX(options.XliffFile, options.LangResX);
				Console.WriteLine($"Merge back to {options.LangResX}");
			}
			else
			{
				logger.LogWarning($"Action {options.Action} not recognized.");
				return 100;
			}


			return 0;
		}
	}

	[CliManager(Description = "Convert and merge between ResX and XLIFF 1.2, and utilize XLIFF as translation memory.", OptionSeparator = "/", Assignment = ":")]
	public class Options
	{
		[CommandLineOption(Aliases = "a", Description = "Action could be new, merge, and MergeBack, e.g., /a=mergeback. New is to create XLIFF file based on source ResX and language ResX; merge is to merge to change of ResX to existing XLIFF, and MergeBack is to merge the translated XLIFF back to the language ResX.")]
		public string Action { get; set; }

		[CommandLineOption(Aliases = "RXS", Description = "Source ResX file path, e.g., /F=AppResources.resx")]
		public string SourceResX { get; set; }

		[CommandLineOption(Aliases = "RXL", Description = "Language ResX file path. e.g., /TF=c:/AppResources.ja.resx")]
		public string LangResX { get; set; }

		[CommandLineOption(Aliases = "XF", Description = "Language XLIFF file path. e.g., /TF=c:/AppResources.ja.xlf")]
		public string XliffFile { get; set; }

		[CommandLineOption(Aliases = "SL", Description = "Source language. e.g., /SL=en")]
		public string SourceLang { get; set; }

		[CommandLineOption(Aliases = "TL", Description = "Target language. e.g., /TL=ja")]
		public string TargetLang { get; set; }

		[CommandLineOption(Aliases = "GID", Description = "XLIFF Group, optional to be compatible with ResX Resource Manager. e.g., /GID=APPRESOURCES.RESX")]
		public string GroupId { get; set; }

		[CommandLineOption(Aliases = "h ?", Name = "Help", Description = "Shows this help text")]
		public bool Help
		{
			get;
			set;
		}
	}
}
