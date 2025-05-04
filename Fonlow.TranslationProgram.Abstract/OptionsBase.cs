using Plossum.CommandLine;

namespace Fonlow.TranslationProgram.Abstract
{
	public class OptionsBase
	{
		[CommandLineOption(Aliases = "F", Description = "Source file path, e.g., /F=AppResources.resx")]
		public string SourceFile { get; set; }

		[CommandLineOption(Aliases = "TF", Description = "Target file path. e.g., /TF=c:/AppResources.ja.resx")]
		public string TargetFile { get; set; }

		[CommandLineOption(Aliases = "SL", Description = "Source language. e.g., /SL=fr")]
		public string SourceLang { get; set; }

		[CommandLineOption(Aliases = "TL", Description = "Target language. e.g., /TL=zh")]
		public string TargetLang { get; set; }

		[CommandLineOption(Aliases = "B", Description = "Batch processing of strings to improve overall speed. V2 and V3 support.")]
		public bool Batch { get; set; }

		[CommandLineOption(Aliases = "h ?", Name = "Help", Description = "Shows this help text")]
		public bool Help
		{
			get;
			set;
		}
	}


}
