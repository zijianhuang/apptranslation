using Microsoft.Extensions.Logging;

namespace Fonlow.Translate
{
	/// <summary>
	/// To combine Xliff 1.2 or 2.0 with a translation engine.
	/// </summary>
	public interface IXliffTranslation : IResourceTranslation
	{
		void SetForStates(string[] forStates);
		void SetUnchangeState(bool unchangeState);
		void SetReversed(bool reversed);
		Task<int> Translate(ITranslate translator, ILogger logger, IProgressDisplay progressDisplay, bool reversedTranslation);
	}
}