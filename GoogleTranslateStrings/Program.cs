using Fonlow.Cli;
using Fonlow.GoogleTranslate;
using Fonlow.GoogleTranslateV3;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Plossum.CommandLine;

namespace GoogleTranslateStrings
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
GoogleTranslateStrings.exe /AK=YourGoogleTranslateV2ApiKey /SL=en /TL=zh-hant /F:strings.zh-hant.xml ---- For in-place translation when strings.zh-hant.xml is not yet translated
GoogleTranslateStrings.exe /AK=YourGoogleTranslateV2ApiKey /SL=en /TL=ja /F:strings.xml /TF:strings.ja.xml ---- from the source locale file to a new target file in Japanese
GoogleTranslateStrings.exe /AK=YourGoogleTranslateV2ApiKey /F:myUiMessages.xml /TF:myUiMessages.es.xml /TL=es ---- From the source template file to a new target file in Spanish.
GoogleTranslateStrings.exe /AV=v3 /CSF=client_secret.json /B  /SL=en /TL=es /F:myUiMessages.es.xml ---- Use Google Cloud Translate V3 and batch mode.
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

			try
			{
				ITranslate translator;
				if (options.ApiVersion.Equals("V2", StringComparison.OrdinalIgnoreCase))
				{
					translator = new XWithGT2(options.SourceLang, options.TargetLang, options.ApiKey);
				}
				else if (options.ApiVersion.Equals("V3", StringComparison.OrdinalIgnoreCase))
				{
					if (string.IsNullOrEmpty(options.ClientSecretFile))
					{
						logger.LogWarning("Expect ClientSecretFile for V3.");
						return 120;
					}

					var clientSecrets =  await GoogleClientSecrets.FromFileAsync(options.ClientSecretFile).ConfigureAwait(false);
					var projectId = ClientSecretReader.ReadProjectId(options.ClientSecretFile);
					translator = new XWithGT3(options.SourceLang, options.TargetLang, clientSecrets, projectId);
					Console.WriteLine("Using Google Cloud Translate V3 ...");
				}
				else
				{
					logger.LogWarning($"ApiVersion {options.ApiVersion} not supported.");
					return 110;
				}

				var st = new StringsTranslate(options.Batch);
				var c = await st.TranslateStrings(options.SourceFile, targetFile, translator, logger, ShowProgress).ConfigureAwait(false);
				Console.WriteLine();
				Console.WriteLine($"Total translated: {c}");
			}
			catch (ArgumentException ex)
			{
				logger.LogError(ex.Message);
				return 100;
			}
			catch (Google.GoogleApiException ex)
			{
				logger.LogError(ex.Message);
				return 200;
			}

			return 0;
		}

		static void ShowProgress(int current, int totalUnits)
		{
			Console.CursorLeft = 10;
			Console.Write($"{current} / {totalUnits}");
		}
	}

	[CliManager(Description = "Use Google Translate v2 or v3 to translate String Resource", OptionSeparator = "/", Assignment = ":")]
	internal class Options
	{
		[CommandLineOption(Aliases = "F", Description = "Source file path, e.g., /F=strings.xml")]
		public string SourceFile { get; set; }

		[CommandLineOption(Aliases = "TF", Description = "Target file path. e.g., /TF=c:/strings.zh.xml")]
		public string TargetFile { get; set; }

		[CommandLineOption(Aliases = "SL", Description = "Source language. e.g., /SL=fr")]
		public string SourceLang { get; set; }

		[CommandLineOption(Aliases = "TL", Description = "Target language. e.g., /TL=zh")]
		public string TargetLang { get; set; }

		[CommandLineOption(Aliases = "AK", Description = "Google Translate API key. e.g., /AK=zasdfSDFSDfsdfdsfs234sdsfki")]
		public string ApiKey { get; set; }


		[CommandLineOption(Aliases = "B", Description = "Batch processing of strings to improve overall speed. V2 and V3 support.")]
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
