using Microsoft.Extensions.Logging;

namespace Fonlow.GoogleTranslate
{
	public interface IResxTranslation
	{
		Task<int> TranslateResx(string filePath, string targetFile, ITranslate g, ILogger logger, Action<int, int> progressCallback);

	}
}
