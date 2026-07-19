using Fonlow.Translate;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace Fonlow.JsonTranslate
{
	public class JsonObjectTranslation : IResourceTranslation
	{
		public JsonObjectTranslation()
		{

		}

		bool batchMode;
		string sourceFile;
		string targetFile;
		string[] properties;

		JsonSerializerOptions jsonSerializerOptions;

		public void SetBatchMode(bool batchMode)
		{
			this.batchMode = batchMode;
		}

		public void SetSourceFile(string sourceFile)
		{
			this.sourceFile = sourceFile;
		}

		public void SetTargetFile(string targetFile)
		{
			this.targetFile = targetFile;
		}

		public void SetProperties(string[] properties)
		{
			this.properties = properties;
		}

		//public void SetJsonSerializerOptions(JsonSerializerOptions jsonSerializerOptions)
		//{
		//	this.jsonSerializerOptions = jsonSerializerOptions;
		//}

		public async Task<int> Translate(ITranslate translator, ILogger logger, IProgressDisplay progressDisplay)
		{
			int c;
			var jsonText = File.ReadAllText(sourceFile);
			var jsonObject = JObject.Parse(jsonText);
			c = await JsonObjectHandler.TranslateJsonObject(jsonObject, properties, translator, logger, progressDisplay, batchMode).ConfigureAwait(false);

			File.WriteAllText(targetFile, jsonObject.ToString(Newtonsoft.Json.Formatting.Indented));
			return c;
		}

	}

	public static class JsonObjectHandler
	{
		public static IEnumerable<JToken> SelectElementsByJsonPaths(JObject root, IEnumerable<string> jsonPaths)
		{
			foreach (var jp in jsonPaths)
			{
				if (string.IsNullOrWhiteSpace(jp))
				{
					continue;
				}

				foreach (var el in root.SelectTokens(jp))
				{

					if (IsTextLeaf(el))
					{
						yield return el;
					}
				}
			}
		}

		static bool IsTextLeaf(JToken token)
		{
			return token is JValue value && value.Type == JTokenType.String;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="jsonObject"></param>
		/// <param name="properties">Each element could be like "object/NodeLevel1/NodeLevel2/ThisNode", or "Object.NodeLevel1.NodeLevel2.ThatNode"</param>
		/// <param name="translator"></param>
		/// <param name="logger"></param>
		/// <param name="progressDisplay"></param>
		/// <returns></returns>
		public static async Task<int> TranslateJsonObject(JObject jRoot, string[] jsonPaths, ITranslate translator, ILogger logger, IProgressDisplay progressDisplay, bool batchMode)
		{
			ArgumentNullException.ThrowIfNull(jRoot);
			ArgumentNullException.ThrowIfNull(jsonPaths);
			const int maxUnits = 200;
			int translatedCount = 0;
			var matchedTokens = SelectElementsByJsonPaths(jRoot, jsonPaths).ToArray();
			var total = matchedTokens.Length;

			if (batchMode)
			{
				var chunks = matchedTokens.SplitLists(maxUnits);
				foreach (var chunk in chunks)
				{
					await Batch(chunk).ConfigureAwait(false); // always countsForUnit
				}
			}
			else
			{
				await TextByText(matchedTokens);
			}

			return translatedCount;

			async Task<int> TextByText(JToken[] tokens)
			{

				foreach (var jt in tokens) // each represents a node to be translated
				{
					var translatedText = await translator.Translate((jt as JValue).Value<string>()).ConfigureAwait(false);
					(jt as JValue).Value = translatedText;
					translatedCount++;
					progressDisplay?.Show(translatedCount, total);
				}

				return translatedCount;
			}

			async Task<int> Batch(IList<JToken> tokens)
			{
				var strings = tokens.Select(d => d.Value<string>()).ToArray();

				if (strings.Length == 0)
				{
					return 0;
				}

				var translatedStrings = await translator.Translate(strings).ConfigureAwait(false);
				int translatedIndex = 0;
				foreach (var jt in tokens)
				{
					(jt as JValue).Value = translatedStrings[translatedIndex];
					translatedIndex++;
					translatedCount++;
				}

				progressDisplay?.Show(translatedCount, total);
				return translatedCount;
			}


		}

	}
}
