using Fonlow.Cli;
using Fonlow.GoogleTranslate;
using Fonlow.GoogleTranslateV3;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Plossum.CommandLine;

namespace GoogleTranslateXliff
{
	internal class Program
	{
		static async Task<int> Main(string[] args)
		{
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
GoogleTranslateXliff.exe /AK=YourGoogleTranslateV2ApiKey /F=myUiMessages.es.xlf ---- For in-place translation.
GoogleTranslateXliff.exe /AK=YourGoogleTranslateV2ApiKey /F:myUiMessages.ja.xlf /TF:myUiMessagesTranslated.ja.xlf ---- from the source locale file to a new target file in Japanese
GoogleTranslateXliff.exe /AK=YourGoogleTranslateV2ApiKey /F:myUiMessages.xlf /TF:myUiMessages.es.xlf /TL=es ---- From the source template file to a new target file in Spanish.
GoogleTranslateXliff.exe /AV=v3 /CSF=client_secret.json /B /F:myUiMessages.es.xlf ---- Use Google Cloud Translate V3 and batch mode.
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

			if (string.IsNullOrEmpty(options.SourceFile))
			{
				logger.LogWarning("Need SoureFile");
				return 10;
			}

			if (!Path.Exists(options.SourceFile))
			{
				logger.LogWarning($"{options.SourceFile} NOT exists");
				return 11;
			}

			var targetFile = string.IsNullOrEmpty(options.TargetFile) ? options.SourceFile : options.TargetFile;
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

			try
			{
				ITranslate translator;
				if (options.ApiVersion.Equals("V2", StringComparison.CurrentCultureIgnoreCase))
				{
					translator = new XWithGT2(options.SourceLang, options.TargetLang, options.ApiKey);
				}
				else if (options.ApiVersion.Equals("V3", StringComparison.CurrentCultureIgnoreCase))
				{
					if (string.IsNullOrEmpty(options.ClientSecretFile)){
						logger.LogWarning("Expect ClientSecretFile for V3.");
						return 120;
					}

					var clientSecrets = GoogleClientSecrets.FromFile(options.ClientSecretFile);
					var projectId = ClientSecretReader.ReadProjectId(options.ClientSecretFile);
					translator = new XWithGT3(options.SourceLang, options.TargetLang, clientSecrets, projectId);
					Console.WriteLine("Using Google Cloud Translate V3 ...");
				}
				else
				{
					logger.LogWarning($"ApiVersion {options.ApiVersion} not supported.");
					return 110;
				}

				var c = await xliffProcessor.TranslateXliff(options.SourceFile, targetFile, options.ForStates, options.NotChangeState, translator, logger, ShowProgress);
				Console.WriteLine();
				Console.WriteLine($"Total translated: {c}");
			}
			catch (ArgumentException ex)
			{
				logger.LogError(ex.Message);
				return 100;
			}

			return 0;
		}

		static void ShowProgress(bool isAllNew, int current, int totalUnits, int totalUnitsToTranslate)
		{
			Console.CursorLeft = 10;
			Console.Write(isAllNew ? $"{current} / {totalUnits}" : $"{current} / {totalUnitsToTranslate} / {totalUnits}");
		}
	}

	[CliManager(Description = "Use Google Translate v2 or v3 to translate XLIFF v1.2 or v2.0 file.", OptionSeparator = "/", Assignment = ":")]
	public class Options
	{
		[CommandLineOption(Aliases = "F", Description = "Source file path, e.g., /F=myfile.zh.xliff")]
		public string SourceFile { get; set; }

		[CommandLineOption(Aliases = "TF", Description = "Target file path. If not defined, the source file will be overwritten, e.g., /TF=c:/locales/myfileTT.zh.xliff")]
		public string TargetFile { get; set; }

		[CommandLineOption(Aliases = "SL", Description = "Source language. Default to xliff/file/source-language or xliff/srcLang. e.g., /SL=fr")]
		public string SourceLang { get; set; }

		[CommandLineOption(Aliases = "TL", Description = "Target language. Default to xliff/file/target-language or xliff/trgLang.  e.g., /TL=es")]
		public string TargetLang { get; set; }

		[CommandLineOption(Aliases = "AK", Description = "Google Translate API key. e.g., /AK=zasdfSDFSDfsdfdsfs234sdsfki")]
		public string ApiKey { get; set; }

		[CommandLineOption(Aliases = "SS", Description = "For translation unit of states. Default to new for v1.2 and initial for v2.0, e.g., /SS=\"initial\" \"translated\"")]
		public string[] ForStates { get; set; } = [];

		[CommandLineOption(Aliases = "NCS", Description = "Not to change the state of translation unit to translated after translation.")]
		public bool NotChangeState { get; set; }

		[CommandLineOption(Aliases = "B", Description = "Batch processing of strings to improve overall speed.")]
		public bool Batch { get; set; }

		[CommandLineOption(Aliases = "AV", Description = "Google Translate API version. Default to V2. If V3, a client secret JSON file is expected.")]
		public string ApiVersion { get; set; } = "V2";

		[CommandLineOption(Aliases = "CSF", Description = "Google Cloud Translate V3 does not support API key but rich ways of authentications. This app uses client secret JSON file you could download from your Google Cloud Service account.")]
		public string ClientSecretFile { get; set; }

		[CommandLineOption(Aliases = "h ?", Name = "Help", Description = "Shows this help text")]
		public bool Help
		{
			get;
			set;
		}


	}
}
