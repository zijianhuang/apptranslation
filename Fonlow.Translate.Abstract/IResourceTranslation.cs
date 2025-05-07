using Microsoft.Extensions.Logging;

namespace Fonlow.Translate
{
	public interface IResourceTranslation
	{
		Task<int> Translate(ITranslate translator, ILogger logger, Action<int, int> progressCallback);
		void SetBatchMode(bool batchMode);
		void SetSourceFile(string sourceFile);
		void SetTargetFile(string targetFile);
	}
}
