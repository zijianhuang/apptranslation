using Fonlow.Cli;
using Fonlow.GoogleTranslate;
using Fonlow.GoogleTranslateV3;
using Fonlow.ResxTranslate;
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
GoogleTranslateResx.exe /AK=YourGoogleTranslateV2ApiKey /SL=en /TL=zh-hant /F:AppResources.zh-hant.resx ---- For in-place translation when AppResources.zh-hant.resx is not yet translated
GoogleTranslateResx.exe /AK=YourGoogleTranslateV2ApiKey /SL=en /TL=ja /F:strings.xml /TF:AppResources.ja.resx ---- from the source locale file to a new target file in Japanese
GoogleTranslateResx.exe /AK=YourGoogleTranslateV2ApiKey /F:AppResources.resx /TF:AppResources.es.resx /TL=es ---- From the source template file to a new target file in Spanish.
GoogleTranslateResx.exe /AV=v3 /CSF=client_secret.json /B  /SL=en /TL=es /F:AppResources.es.resx ---- Use Google Cloud Translate V3 and batch mode.
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
				if (options.ApiVersion.Equals("V2", StringComparison.CurrentCultureIgnoreCase))
				{
					translator = new XWithGT2(options.SourceLang, options.TargetLang, options.ApiKey);
				}
				else if (options.ApiVersion.Equals("V3", StringComparison.CurrentCultureIgnoreCase))
				{
					if (string.IsNullOrEmpty(options.ClientSecretFile))
					{
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

				var st = new ResxTranslate(options.Batch);
				var c = await st.TranslateResx(options.SourceFile, targetFile, translator, logger, ShowProgress);
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

	[CommandLineManager(ApplicationName = "Google Translate for Microsoft ResX", Description = "Use Google Translate v2 or v3 to translate Microsoft ResX", OptionSeparator = "/", Assignment = ":", Copyright = "Fonlow (c) 2025", Version = "1.0")]
	public class Options
	{
		[CommandLineOption(Aliases = "F", Description = "Source file path, e.g., /F=AppResources.resx")]
		public string SourceFile { get; set; }

		[CommandLineOption(Aliases = "TF", Description = "Target file path. e.g., /TF=c:/AppResources.ja.resx")]
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
