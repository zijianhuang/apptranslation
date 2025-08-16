using Fonlow.Translate;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Xml.Linq;

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

		public Task<int> Translate(ITranslate translator, ILogger logger, IProgressDisplay progressDisplay)
		{
			throw new NotImplementedException();
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
		public async Task<int> TranslateJsonObject(JsonObject jsonObject, string[] properties, ITranslate translator, ILogger logger, IProgressDisplay progressDisplay)
		{
			ArgumentNullException.ThrowIfNull(jsonObject);
			ArgumentNullException.ThrowIfNull(properties);
			const int maxUnits = 200;
			int translatedCount = 0;
			var allNestedPropertySegmentsList = properties.Select(d => d?.Split(['.', '/'])).ToArray();
			var total = allNestedPropertySegmentsList.Length;

			if (batchMode)
			{
				var chunks = allNestedPropertySegmentsList.SplitLists(maxUnits);
				foreach (var chunk in chunks)
				{
					await Batch(chunk).ConfigureAwait(false); // always countsForUnit
				}
			}
			else
			{
				await TextByText(allNestedPropertySegmentsList);
			}

			return translatedCount;

			async Task<int> TextByText(string[][] nestedPropertySegmentsList)
			{
				foreach (var segments in nestedPropertySegmentsList) // each represents a node to be translated
				{
					if (segments == null || segments.Length == 0)
					{
						continue;
					}

					var jsonNode = FindValueNode(jsonObject, segments);
					if (jsonNode == null)
					{
						continue;
					}

					if (jsonNode is not JsonValue){
						continue;
					}

					var nodeText = jsonNode.GetValue<string>(); // only text node worth of translation.
					if (string.IsNullOrWhiteSpace(nodeText))
					{
						continue;
					}

					var translatedText = await translator.Translate(nodeText).ConfigureAwait(false);
					jsonNode.ReplaceWith(translatedText);
					translatedCount++;
					progressDisplay?.Show(translatedCount, total);
				}

				return translatedCount;
			}

			async Task<int> Batch(IList<string[]> nestedPropertySegmentsList)
			{
				var strings = nestedPropertySegmentsList.Where(segments =>
				{
					if (segments == null || segments.Length == 0)
					{
						return false;
					}

					var jsonNode = FindValueNode(jsonObject, segments);
					if (jsonNode == null)
					{
						return false;
					}

					if (jsonNode is not JsonValue)
					{
						return false;
					}

					var nodeText = jsonNode.GetValue<string>(); // only text node worth of translation.
					if (string.IsNullOrWhiteSpace(nodeText))
					{
						return false;
					}

					return true;
				}).Select(segments => {
					var jsonNode = FindValueNode(jsonObject, segments);
					var nodeText = jsonNode.GetValue<string>(); // only text node worth of translation.
					return nodeText;
				}).ToList();

				if (strings.Count == 0)
				{
					return 0;
				}

				var translatedStrings = await translator.Translate(strings).ConfigureAwait(false);
				int translatedIndex = 0;
				foreach (var segments in nestedPropertySegmentsList)
				{
					if (segments == null || segments.Length == 0)
					{
						continue;
					}

					var jsonNode = FindValueNode(jsonObject, segments);
					if (jsonNode == null)
					{
						continue;
					}

					if (jsonNode is not JsonValue)
					{
						continue;
					}

					var nodeText = jsonNode.GetValue<string>(); // only text node worth of translation.
					if (string.IsNullOrWhiteSpace(nodeText))
					{
						continue;
					}

					jsonNode.ReplaceWith(translatedStrings[translatedIndex]);
					translatedIndex++;
					translatedCount++;
				}

				progressDisplay?.Show(translatedCount, total);
				return translatedCount;
			}


		}

		/// <summary>
		/// Find the valueNode. Null if not found.
		/// </summary>
		/// <param name="jsonObject"></param>
		/// <param name="nestedPropertySegments"></param>
		/// <returns></returns>
		JsonNode FindValueNode(JsonObject jsonObject, string[] nestedPropertySegments)
		{
			Debug.Assert(nestedPropertySegments.Length > 0);
			var n = jsonObject[nestedPropertySegments[0]];
			if (n == null)
			{
				return null;
			}
			for (int i = 1; i < nestedPropertySegments.Length; i++)
			{
				n = n[nestedPropertySegments[i]];
				if (n == null)
				{
					return null;
				}
			}

			return n;
		}
	}
}
