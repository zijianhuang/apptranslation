using Fonlow.Translate;
using Fonlow.TranslationProgram.Abstract;
using Microsoft.Extensions.Logging;
using Fonlow.MsTranslator;

namespace Fonlow.TranslationProgram.MsTranslator
{
	public abstract class TranslationProgramWithMsTranslator : TranslationProgramBase
	{
		public TranslationProgramWithMsTranslator(IResourceTranslation resourceTranslation, OptionsWithMsTranslator options, ILogger logger) : base(resourceTranslation, options, logger)
		{
			this.options = options;
		}

		readonly OptionsWithMsTranslator options;


		public override ITranslate CreateTranslator(out int errorCode)
		{
			ITranslate translator = null;
			var goodCombination = string.IsNullOrEmpty(options.ApiKey) ^ string.IsNullOrEmpty(options.ApiKeyFile);
			if (goodCombination)
			{
				if (!string.IsNullOrEmpty(options.ApiKey))
				{
					translator = new XWithMT(options.SourceLang, options.TargetLang, options.ApiKey, options.Region);
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
						translator = new XWithMT(options.SourceLang, options.TargetLang, apiKey, options.Region);
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

			errorCode = 0;
			return translator;
		}

		protected override int HandleTranslationEngineException(Exception ex)
		{
			if (ex is Azure.RequestFailedException)
			{
				logger.LogError(ex.ToString());
				logger.LogInformation("Program exit. status: 200");
				return 200;
			}

			throw ex;
		}
	}

}
