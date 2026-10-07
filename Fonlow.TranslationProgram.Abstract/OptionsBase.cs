using Plossum.CommandLine;

namespace Fonlow.TranslationProgram.Abstract
{
	/// <summary>
	/// Common options for derived options for different translation engines and metadata.
	/// </summary>
	public class OptionsBase
	{
		[CommandLineOption(Aliases = "F", Description = "Source file path")]
		public string SourceFile { get; set; }

		[CommandLineOption(Aliases = "TF", Description = "Target file path. Without this, the source file is also the target file.")]
		public string TargetFile { get; set; }

		/// <summary>
		/// Default en
		/// </summary>
		[CommandLineOption(Aliases = "SL", Description = "Source language. e.g., /SL=fr. Default en. If SL==TL, source file is simply copied to target file.")]
		public string SourceLang { get; set; } = "en";

		[CommandLineOption(Aliases = "TL", Description = "Target language. e.g., /TL=zh.")]
		public string TargetLang { get; set; }

		[CommandLineOption(Aliases = "B", Description = "Batch processing of string array to improve overall speed.")]
		public bool Batch { get; set; }

		[CommandLineOption(Aliases = "h ?", Name = "Help", Description = "Shows this help text")]
		public bool Help
		{
			get;
			set;
		}

		[CommandLineOption(Aliases = "GL", Description = "GlossaryId for V3 translation, like mstc-dutch-en-nl, for example, this will result in projects/api-project-1234567890/locations/us-central1/glossaries/mstc-dutch-en-nl")]
		public string GlossaryId { get; set; }

		[CommandLineOption(Aliases = "LG", Description = "List all available glossaries. For V3.")]
		public bool ListGlossaries { get; set; }

	}
}
