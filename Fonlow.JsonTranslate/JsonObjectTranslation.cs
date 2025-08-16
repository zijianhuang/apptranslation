using Fonlow.Translate;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json.Nodes;

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
			var nestedPropertySegmentsList = properties.Select(d => d?.Split(['.', '/'])).ToArray();
			foreach (var segments in nestedPropertySegmentsList)
			{
				if (segments==null || segments.Length == 0)
				{
					continue;
				}
				var node = FindValueNode(jsonObject, segments);
				if (node == null)
				{
					continue;
				}

				var nodeText = node.GetValue<string>(); // only text node worth of translation.
				var translatedText = await translator.Translate(nodeText).ConfigureAwait(false);
				node.ReplaceWith(translatedText);
				translatedCount++;
			}

			return translatedCount;
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
