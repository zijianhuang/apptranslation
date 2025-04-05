
namespace Fonlow.GoogleTranslate
{
	/// <summary>
	/// Interface to a translation engine like Google Translate v2, v3 or MS Translator etc.
	/// </summary>
	public interface ITranslate
	{
		string SourceLang { get; set; }
		string TargetLang { get; set; }

		Task<string> Translate(string text);

		/// <summary>
		/// Batch processing
		/// </summary>
		/// <param name="strings"></param>
		/// <returns></returns>
		Task<string[]> Translate(IList<string> strings);
	}
}