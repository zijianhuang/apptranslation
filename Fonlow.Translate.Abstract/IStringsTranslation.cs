using Fonlow.GoogleTranslate;
using Microsoft.Extensions.Logging;

namespace Fonlow.Translate.Abstract
{
	public interface IStringsTranslation
	{
		Task<int> TranslateStrings(string filePath, string targetFile, ITranslate g, ILogger logger, Action<int, int> progressCallback);

	}
}
