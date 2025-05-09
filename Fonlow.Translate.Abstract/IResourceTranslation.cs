using Microsoft.Extensions.Logging;

namespace Fonlow.Translate
{
	public interface IResourceTranslation
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="translator"></param>
		/// <param name="logger"></param>
		/// <param name="progressDisplay"></param>
		/// <returns></returns>
		//Task<int> Translate(ResourceTranslationFactory resourceTranslationFactory, ILogger logger, IProgressDisplay progressDisplay);
		Task<int> Translate(ITranslate translator, ILogger logger, IProgressDisplay progressDisplay);
		void SetBatchMode(bool batchMode);
		void SetSourceFile(string sourceFile);
		void SetTargetFile(string targetFile);
	}
}
