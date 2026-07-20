using Fonlow.Translate;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;

namespace Fonlow.HtmlTranslate
{
	/// <summary>
	/// Translate HTML doc or nodes
	/// </summary>
	public class HtmlTranslation : IResourceTranslation
	{
		public HtmlTranslation()
		{

		}

		bool batchMode;
		string sourceFile;
		string targetFile;
		string[] xpaths;

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

		public void SetXPaths(string[] xpaths)
		{
			this.xpaths = xpaths;
		}

		public async Task<int> Translate(ITranslate translator, ILogger logger, IProgressDisplay progressDisplay)
		{
			int c;
			var xdoc = new HtmlDocument();
			xdoc.Load(sourceFile);

			c = await HtmlNodesHandler.TranslateXmlTextNode(xdoc, xpaths, translator, logger, progressDisplay, batchMode).ConfigureAwait(false);
			xdoc.Save(targetFile);
			return c;
		}

	}

	public static class HtmlNodesHandler
	{
		/// <summary>
		/// Get all the norminated nodes which are text leaf nodes not empty.
		/// </summary>
		/// <param name="root"></param>
		/// <param name="xpaths"></param>
		/// <param name="nsManager"></param>
		/// <returns></returns>
		public static IEnumerable<HtmlNode> SelectElementsByXPaths(HtmlNode root, IEnumerable<string> xpaths)
		{
			foreach (var xpath in xpaths)
			{
				if (string.IsNullOrWhiteSpace(xpath))
					continue;

				foreach (var el in root.SelectNodes(xpath))
				{
					yield return el;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xdoc"></param>
		/// <param name="xpaths">Each element could be like "object/NodeLevel1/NodeLevel2/ThisNode", or "Object.NodeLevel1.NodeLevel2.ThatNode"</param>
		/// <param name="translator"></param>
		/// <param name="logger"></param>
		/// <param name="progressDisplay"></param>
		/// <returns></returns>
		public static async Task<int> TranslateXmlTextNode(HtmlDocument xdoc, string[] xpaths, ITranslate translator, ILogger logger, IProgressDisplay progressDisplay, bool batchMode)
		{
			ArgumentNullException.ThrowIfNull(xdoc);

			if (xpaths==null || xpaths.Length==0){
				xdoc.DocumentNode.InnerHtml = await translator.TranslateHtml(xdoc.DocumentNode.InnerHtml);
				return 1;
			}

			const int maxUnits = 200;
			int translatedCount = 0;
			var matchedElements = SelectElementsByXPaths(xdoc.DocumentNode, xpaths).ToArray();
			var total = matchedElements.Length;

			if (batchMode)
			{
				var chunks = matchedElements.SplitLists(maxUnits);
				foreach (var chunk in chunks)
				{
					await Batch(chunk).ConfigureAwait(false); // always countsForUnit
				}
			}
			else
			{
				await TextByText(matchedElements);
			}

			return translatedCount;

			async Task<int> TextByText(HtmlNode[] elements)
			{

				foreach (var el in elements) // each represents a node to be translated
				{
					var translatedText = await translator.TranslateHtml(el.InnerHtml).ConfigureAwait(false);
					el.InnerHtml = translatedText;
					translatedCount++;
					progressDisplay?.Show(translatedCount, total);
				}

				return translatedCount;
			}

			async Task<int> Batch(IList<HtmlNode> elements)
			{
				var strings = elements.Select(d => d.InnerHtml).ToArray();

				if (strings.Length == 0)
				{
					return 0;
				}

				var translatedStrings = await translator.TranslateHtmlItems(strings).ConfigureAwait(false);
				int translatedIndex = 0;
				foreach (var el in elements)
				{
					el.InnerHtml = translatedStrings[translatedIndex];
					translatedIndex++;
					translatedCount++;
				}

				progressDisplay?.Show(translatedCount, total);
				return translatedCount;
			}
		}

	}
}
