using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Fonlow.SvgTextTranslate;

/// <summary>
/// Extracts and replaces ONLY the text-node content of an SVG document
/// (the literal characters inside &lt;text&gt;, &lt;tspan&gt;, &lt;tref&gt;,
/// &lt;textPath&gt; elements), leaving every attribute, style, path,
/// embedded image, comment, and whitespace/formatting byte-for-byte intact.
///
/// Design:
///   1. Walk every element in document order.
///   2. For elements whose tag name is in TextContainerElements, look at
///      their DIRECT child text nodes only (not text belonging to nested
///      elements - those are visited separately when the traversal
///      reaches them).
///   3. Skip whitespace-only text nodes (pure indentation, not content).
///   4. Build a deterministic key per text node from the element's "id"
///      attribute (falling back to an auto-generated id), so the same
///      key sequence is produced by both ExtractTexts and ReplaceTexts
///      as long as the SVG structure is unchanged between calls.
///   5. Load with whitespace preserved and save without re-indenting,
///      so round-tripping an SVG through ExtractTexts/ReplaceTexts with
///      no actual translations is a byte-identical no-op.
/// </summary>
public static class SvgTextProcessor //crafted by Claude.ai
{
	// Element (local) names that may directly carry translatable text.
	// Add "flowRoot"/"flowPara" here if you need to support Inkscape's
	// flowed-text elements as well.
	private static readonly HashSet<string> TextContainerElements = new HashSet<string>
		{
			"text", "tspan", "tref", "textPath"
            // "title", "desc" can be added if you also want to translate
            // accessibility/metadata text - left out by default since they
            // are not visually rendered "content" in the same sense.
        };

	/// <summary>
	/// Extracts all translatable text nodes from the given SVG content.
	/// </summary>
	/// <returns>
	/// A list of (Key, Text) pairs. Key is a stable identifier you can
	/// use to look the entry up again when calling ReplaceTexts.
	/// </returns>
	public static IList<(string Key, string Text)> ExtractTexts(string svgContent)
	{
		if (string.IsNullOrEmpty(svgContent))
			return new List<(string, string)>();

		var doc = LoadPreservingWhitespace(svgContent);
		var result = new List<(string Key, string Text)>();
		var usedKeys = new HashSet<string>();
		int autoCounter = 0;

		foreach (var element in doc.Descendants())
		{
			if (!IsTextContainer(element)) continue;

			var textNodes = element.Nodes().OfType<XText>().ToList();
			for (int i = 0; i < textNodes.Count; i++)
			{
				var textNode = textNodes[i];
				if (string.IsNullOrWhiteSpace(textNode.Value)) continue;

				string key = BuildKey(element, i, textNodes.Count, usedKeys, ref autoCounter);
				usedKeys.Add(key);
				result.Add((key, textNode.Value));
			}
		}

		return result;
	}

	/// <summary>
	/// Returns a copy of the SVG content with text nodes replaced
	/// according to the supplied translations dictionary (Key -> new
	/// text). Keys are matched using exactly the same algorithm as
	/// ExtractTexts. Any text node whose key is not present in
	/// translations is left unchanged. Everything that is not a
	/// matched text node (attributes, styles, paths, images, comments,
	/// whitespace) is preserved exactly.
	/// </summary>
	public static string ReplaceTexts(string svgContent, Dictionary<string, string> translations)
	{
		if (string.IsNullOrEmpty(svgContent))
			return svgContent;
		if (translations == null || translations.Count == 0)
			return svgContent;

		var doc = LoadPreservingWhitespace(svgContent);
		var usedKeys = new HashSet<string>();
		int autoCounter = 0;

		foreach (var element in doc.Descendants())
		{
			if (!IsTextContainer(element)) continue;

			var textNodes = element.Nodes().OfType<XText>().ToList();
			for (int i = 0; i < textNodes.Count; i++)
			{
				var textNode = textNodes[i];
				if (string.IsNullOrWhiteSpace(textNode.Value)) continue;

				string key = BuildKey(element, i, textNodes.Count, usedKeys, ref autoCounter);
				usedKeys.Add(key);

				if (translations.TryGetValue(textNode.Value, out var translated) && translated != null)
				{
					// XText.Value setter handles XML-escaping (e.g. &amp;)
					// automatically on save - no manual encoding needed.
					textNode.Value = translated;
				}
			}
		}

		return SaveToString(doc);
	}

	private static bool IsTextContainer(XElement element)
	{
		return TextContainerElements.Contains(element.Name.LocalName);
	}

	/// <summary>
	/// Builds a stable, human-readable key for a text node.
	/// Preference order:
	///   1. element's id attribute (e.g. "tspan6")
	///   2. "auto_N" if the element has no id
	/// If an element has multiple direct text nodes, an index suffix
	/// is appended ("id_0", "id_1", ...). A final de-duplication pass
	/// guards against id collisions elsewhere in the document.
	/// </summary>
	private static string BuildKey(
		XElement element,
		int textIndex,
		int totalTextNodesInElement,
		HashSet<string> usedKeys,
		ref int autoCounter)
	{
		string baseKey = (string)element.Attribute("id");

		if (string.IsNullOrEmpty(baseKey))
		{
			baseKey = $"auto_{autoCounter++}";
		}

		string key = totalTextNodesInElement > 1
			? $"{baseKey}_{textIndex}"
			: baseKey;

		string uniqueKey = key;
		int dupSuffix = 1;
		while (usedKeys.Contains(uniqueKey))
		{
			uniqueKey = $"{key}__dup{dupSuffix++}";
		}

		return uniqueKey;
	}

	private static XDocument LoadPreservingWhitespace(string svgContent)
	{
		using var stringReader = new System.IO.StringReader(svgContent);
		using var xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings
		{
			IgnoreWhitespace = false,
			DtdProcessing = DtdProcessing.Ignore
		});

		return XDocument.Load(xmlReader, LoadOptions.PreserveWhitespace);
	}

	private static string SaveToString(XDocument doc)
	{
		var sb = new StringBuilder();
		var settings = new XmlWriterSettings
		{
			OmitXmlDeclaration = doc.Declaration == null,
			Indent = false, // never reformat - preserve original layout
			NewLineHandling = NewLineHandling.None
		};

		using (var writer = XmlWriter.Create(new Utf8StringWriter(sb), settings))
		{
			doc.Save(writer);
		}

		return sb.ToString();
	}

	/// <summary>
	/// StringWriter that reports UTF-8 so the XML declaration written by
	/// XmlWriter matches the encoding actually used for the resulting
	/// string (StringWriter normally reports UTF-16, which would mismatch
	/// the file's original "UTF-8" declaration).
	/// </summary>
	private sealed class Utf8StringWriter : System.IO.StringWriter
	{
		public Utf8StringWriter(StringBuilder sb) : base(sb) { }
		public override Encoding Encoding => Encoding.UTF8;
	}
}

