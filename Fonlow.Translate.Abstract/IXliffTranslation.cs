using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace Fonlow.Translate
{
	/// <summary>
	/// To combine Xliff 1.2 or 2.0 with a translation engine.
	/// </summary>
	public interface IXliffTranslation
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="translator"></param>
		/// <param name="logger"></param>
		/// <param name="progressCallback">current, totalUnits, isAllNew, totalUnitsToTranslate. And the last 2 are valid for TM only like XLIFF</param>
		/// <param name="totalUnitsToTranslate"></param>

		/// <returns></returns>
		Task<int> Translate(ITranslate translator, ILogger logger, Action<int, int, bool, int> progressCallback);
		//Task<int> TranslateXliffElement(XElement xliffRoot, string[] forStates, bool unchangeState, ITranslate translator, ILogger logger, Action<int, int, bool, int> progressCallback);
		void SetBatchMode(bool batchMode);
		void SetSourceFile(string sourceFile);
		void SetTargetFile(string targetFile);

		void SetForStates(string[] forStates);
		void SetUnchangeState(bool unchangeState);

	}
}