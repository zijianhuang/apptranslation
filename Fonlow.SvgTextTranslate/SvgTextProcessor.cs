using System.Xml.Linq;

namespace Fonlow.SvgTextTranslate;

public static class SvgTextProcessor //crafted by Claude.ai
{
	// SVG namespace
	private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";

	/// <summary>
	/// Extracts all text content from SVG <text> and <tspan> elements.
	/// Returns a list of (element unique key, text) pairs for translation.
	/// </summary>
	public static IList<(string Key, string Text)> ExtractTexts(string svgContent)
	{
		var doc = XDocument.Parse(svgContent);
		var results = new List<(string Key, string Text)>();
		int index = 0;

		// Handle both namespaced and non-namespaced SVG files
		var textElements = doc.Descendants(Svg + "text")
			.Concat(doc.Descendants("text"))       // fallback: no namespace
			.Concat(doc.Descendants(Svg + "tspan"))
			.Concat(doc.Descendants("tspan"));

		foreach (var el in textElements)
		{
			// Only grab leaf text nodes (skip elements whose text is in child tspans)
			if (!el.HasElements && !string.IsNullOrWhiteSpace(el.Value))
			{
				string key = el.Attribute("id")?.Value ?? $"__svg_text_{index++}";
				results.Add((key, el.Value.Trim()));
			}
		}

		return results;
	}

	/// <summary>
	/// Replaces text content in SVG <text> and <tspan> elements using a
	/// dictionary of { originalText -> translatedText }.
	/// Returns the modified SVG string.
	/// </summary>
	public static string ReplaceTexts(string svgContent, Dictionary<string, string> translations)
	{
		var doc = XDocument.Parse(svgContent);

		var textElements = doc.Descendants(Svg + "text")
			.Concat(doc.Descendants("text"))
			.Concat(doc.Descendants(Svg + "tspan"))
			.Concat(doc.Descendants("tspan"));

		foreach (var el in textElements)
		{
			if (!el.HasElements && !string.IsNullOrWhiteSpace(el.Value))
			{
				string original = el.Value.Trim();
				if (translations.TryGetValue(original, out string? translated))
				{
					el.Value = translated;
				}
			}
		}

		// Preserve XML declaration if present
		return doc.Declaration != null
			? doc.Declaration + Environment.NewLine + doc.ToString()
			: doc.ToString();
	}
}

