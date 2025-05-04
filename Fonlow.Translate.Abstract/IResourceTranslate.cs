using Microsoft.Extensions.Logging;

namespace Fonlow.Translate
{
	public interface IResourceTranslation
	{
		Task<int> Translate(string filePath, string targetFile, ITranslate translator, ILogger logger, Action<int, int> progressCallback);
		void SetBatchMode(bool batchMode);
	}
}
