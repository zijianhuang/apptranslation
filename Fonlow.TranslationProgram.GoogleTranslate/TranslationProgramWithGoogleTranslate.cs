using Fonlow.Cli;
using Fonlow.GoogleTranslate;
using Fonlow.GoogleTranslateV3;
using Fonlow.Translate;
using Fonlow.TranslationProgram.Abstract;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;

namespace Fonlow.TranslationProgram.GoogleTranslate
{
	public abstract class TranslationProgramWithGoogleTranslate : TranslationProgramBase
	{
		public TranslationProgramWithGoogleTranslate(IResourceTranslation resourceTranslation, OptionsWithGoogleTranslate options, ILogger logger) : base(resourceTranslation, options, logger)
		{
			this.options = options;
		}

		readonly OptionsWithGoogleTranslate options;


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

		protected override int HandleTranslationEngineException(Exception ex)
		{
			if (ex is Google.GoogleApiException){
				logger.LogError(ex.ToString());
				logger.LogInformation("Program exit. status: 200");
				return 200;
			}

			throw ex;
		}
	}

}
