using Fonlow.Cli;
using Fonlow.GoogleTranslate;
using Fonlow.GoogleTranslateV3;
using Fonlow.ResxTranslate;
using Fonlow.Translate;
using Fonlow.TranslationProgram.Abstract;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Plossum.CommandLine;

namespace Fonlow.TranslationProgram
{
	[CliManager(Description = "Use Google Translate v2 or v3 to translate Microsoft ResX", OptionSeparator = "/", Assignment = ":")]
	public class Options : OptionsBase
	{
		[CommandLineOption(Aliases = "AK", Description = "Google Translate API key. e.g., /AK=zasdfSDFSDfsdfdsfs234sdsfki")]
		public string ApiKey { get; set; }

		[CommandLineOption(Aliases = "AKF", Description = "Google Translate API key stored in a text file. e.g., /AKF=C:/Users/Public/DevApps/GtApiKey.txt")]
		public string ApiKeyFile { get; set; }

		[CommandLineOption(Aliases = "AV", Description = "Google Translate API version. Default to V2. If V3, a client secret JSON file is expected.")]
		public string ApiVersion { get; set; } = "V2";

		[CommandLineOption(Aliases = "CSF", Description = "Google Cloud Translate V3 does not support API key but rich ways of authentications. This app uses client secret JSON file you could download from your Google Cloud Service account.")]
		public string ClientSecretFile { get; set; }
	}

	public class TranslationProgramWithGoogleTranslate : TranslationProgramBase
	{
		public TranslationProgramWithGoogleTranslate(Options options, ILogger logger) : base(new ResxTranslation(), options, logger)
		{
			this.options = options;
		}

		readonly Options options;

		public override void DisplayExamples()
		{
			Console.WriteLine(
@"Examples:
GoogleTranslateResx.exe /AK=YourGoogleTranslateV2ApiKey /SL=en /TL=zh-hant /F:AppResources.zh-hant.resx ---- For in-place translation when AppResources.zh-hant.resx is not yet translated
GoogleTranslateResx.exe /AK=YourGoogleTranslateV2ApiKey /SL=en /TL=ja /F:strings.xml /TF:AppResources.ja.resx ---- from the source locale file to a new target file in Japanese
GoogleTranslateResx.exe /AK=YourGoogleTranslateV2ApiKey /F:AppResources.resx /TF:AppResources.es.resx /TL=es ---- From the source template file to a new target file in Spanish.
GoogleTranslateResx.exe /AV=v3 /CSF=client_secret.json /B  /SL=en /TL=es /F:AppResources.es.resx ---- Use Google Cloud Translate V3 and batch mode.
"
			);
		}

		public override ITranslate CreateTranslator(out int errorCode)
		{
			ITranslate translator = null;
			if (options.ApiVersion.Equals("V2", StringComparison.OrdinalIgnoreCase))
			{
				var goodCombination = string.IsNullOrEmpty(options.ApiKey) ^ string.IsNullOrEmpty(options.ApiKeyFile);
				if (goodCombination)
				{
					if (!string.IsNullOrEmpty(options.ApiKey))
					{
						translator = new XWithGT2(options.SourceLang, options.TargetLang, options.ApiKey);
					}
					else
					{
						if (!File.Exists(options.ApiKeyFile))
						{
							logger.LogWarning($"ApiKeyFile {options.ApiKeyFile} does not exist.");
							errorCode = 129;
							return null;
						}

						var apiKey = (File.ReadAllLines(options.ApiKeyFile)).FirstOrDefault();
						if (!string.IsNullOrEmpty(apiKey))
						{
							translator = new XWithGT2(options.SourceLang, options.TargetLang, apiKey);
						}
						else
						{
							logger.LogWarning($"ApiKeyFile {options.ApiKeyFile} has no valid content. The first line must be the API key.");
							errorCode = 130;
							return null;
						}
					}
				}
				else
				{
					logger.LogWarning("Either ApiKey or ApiKeyFile is needed.");
					errorCode = 100;
				}
			}
			else if (options.ApiVersion.Equals("V3", StringComparison.OrdinalIgnoreCase))
			{
				if (string.IsNullOrEmpty(options.ClientSecretFile))
				{
					logger.LogWarning("Expect ClientSecretFile for V3.");
					errorCode = 120;
					return null;
				}

				var clientSecrets = GoogleClientSecrets.FromFile(options.ClientSecretFile);
				var projectId = ClientSecretReader.ReadProjectId(options.ClientSecretFile);
				translator = new XWithGT3(options.SourceLang, options.TargetLang, clientSecrets, projectId);
				Console.WriteLine("Using Google Cloud Translate V3 ...");
			}
			else
			{
				logger.LogWarning($"ApiVersion {options.ApiVersion} not supported.");
				errorCode = 110;
				return null;
			}

			errorCode = 0;
			return translator;
		}
	}


}
