namespace Fonlow.Translate
{
	/// <summary>
	/// To combine Xliff 1.2 or 2.0 with a translation engine.
	/// </summary>
	public interface IXliffTranslation : IResourceTranslation
	{
		void SetForStates(string[] forStates);
		void SetUnchangeState(bool unchangeState);

	}
}