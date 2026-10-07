using System.Globalization;
using System.Text;
namespace Fonlow.Translate
{
	public static class SingleTermHelper // crafted by Claude.
	{
		// ---------- 1. Single-term detection (source text) ----------

		/// <param name="maxWords">Max whitespace-separated tokens for spaced scripts (Latin, Cyrillic, Greek, Hangul, Arabic...).</param>
		/// <param name="maxUnspacedChars">Max characters for scripts written without spaces (Han, Kana, Thai, Lao, Khmer, Myanmar).</param>
		public static bool IsSingleTerm(string? text, int maxWords = 1, int maxUnspacedChars = 6)
		{
			if (string.IsNullOrWhiteSpace(text)) return false;
			text = text.Trim();

			// Anything sentence-like is not a "term"
			if (text.IndexOfAny(new[] { '\n', '\r', '。', '！', '？', '!', '?', '…' }) >= 0) return false;
			if (text.Contains(". ")) return false;

			string[] tokens = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
			if (tokens.Length > maxWords) return false;

			// Decide whether the text is mostly a script without word spacing
			int letters = 0, unspaced = 0;
			foreach (Rune r in text.EnumerateRunes())
			{
				if (!Rune.IsLetter(r)) continue;
				letters++;
				if (IsUnspacedScript(r.Value)) unspaced++;
			}

			if (letters > 0 && unspaced * 2 >= letters)
			{
				// Chinese / Japanese / Thai etc.: word count is meaningless, so use length
				string compact = string.Concat(tokens);
				return new StringInfo(compact).LengthInTextElements <= maxUnspacedChars;
			}

			return true; // spaced script with <= maxWords tokens (Korean falls here too)
		}

		private static bool IsUnspacedScript(int cp) =>
			(cp >= 0x4E00 && cp <= 0x9FFF) ||  // CJK Unified Ideographs
			(cp >= 0x3400 && cp <= 0x4DBF) ||  // Ext A
			(cp >= 0x20000 && cp <= 0x2FA1F) ||  // Ext B..F + compatibility supplement
			(cp >= 0xF900 && cp <= 0xFAFF) ||  // Compatibility ideographs
			(cp >= 0x3040 && cp <= 0x30FF) ||  // Hiragana + Katakana
			(cp >= 0x31F0 && cp <= 0x31FF) ||  // Katakana phonetic ext
			(cp >= 0xFF66 && cp <= 0xFF9F) ||  // Half-width Katakana
			(cp >= 0x0E00 && cp <= 0x0EFF) ||  // Thai + Lao
			(cp >= 0x1000 && cp <= 0x109F) ||  // Myanmar
			(cp >= 0x1780 && cp <= 0x17FF);      // Khmer

		// ---------- 2. First-candidate extraction (translation) ----------

		// Separators that indicate "alternative translations", per target language.
		// Deliberately excluded: '/' (gender forms like Lehrer/in) and '・' (inside Japanese compounds).
		private static readonly Dictionary<string, string> SeparatorsByLang =
			new(StringComparer.OrdinalIgnoreCase)
			{
				["zh"] = "；;，,、",
				["ja"] = "、；;，",
				["ko"] = ";,；",
				["ar"] = "؛،;,",
				["fa"] = "؛،;,",
				["ur"] = "؛،;,",
			};

		// European and all other languages
		private const string DefaultSeparators = ";,；，、؛،";

		private const string OpenBrackets = "([{（［｛【「『";
		private const string CloseBrackets = ")]}）］｝】」』";

		public static string PickFirstCandidate(string translation, string targetLang)
		{
			if (string.IsNullOrWhiteSpace(translation)) return translation;

			string primary = targetLang.Split('-', '_')[0];
			string seps = SeparatorsByLang.TryGetValue(primary, out var s) ? s : DefaultSeparators;

			int depth = 0, start = 0;
			for (int i = 0; i < translation.Length; i++)
			{
				char c = translation[i];

				if (OpenBrackets.IndexOf(c) >= 0) { depth++; continue; }
				if (CloseBrackets.IndexOf(c) >= 0) { if (depth > 0) depth--; continue; }
				if (depth > 0 || seps.IndexOf(c) < 0) continue;

				// Don't split numbers like "1,000"
				if (c == ',' && i > 0 && i < translation.Length - 1 &&
					char.IsDigit(translation[i - 1]) && char.IsDigit(translation[i + 1])) continue;

				string candidate = translation[start..i].Trim();
				if (candidate.Length > 0) return candidate;
				start = i + 1; // leading separator, so skip the empty piece
			}

			return translation.Trim();
		}

		// ---------- 3. Combined entry point ----------

		public static string Normalize(string source, string translation, string targetLang) =>
			IsSingleTerm(source) ? PickFirstCandidate(translation, targetLang) : translation;
	}
}
