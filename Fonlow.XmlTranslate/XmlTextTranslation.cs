using Fonlow.Translate;
using Microsoft.Extensions.Logging;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Fonlow.XmlTranslate
{
	/// <summary>
	/// Translate non empty text leafs norminated by xpaths, html?????????????
	/// </summary>
	public class XmlTextTranslation : IResourceTranslation
	{
		public XmlTextTranslation()
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
			var xdoc = XElement.Load(sourceFile,
				LoadOptions.PreserveWhitespace); // good for SVG texts
			var nsManager = new XmlNamespaceManager(new NameTable());
			XNamespace rootElementNs = xdoc.Name.Namespace;
			nsManager.AddNamespace(xdoc.Name.LocalName, rootElementNs.NamespaceName);

			c = await XElementsHandler.TranslateXmlTextNode(xdoc, nsManager, xpaths, translator, logger, progressDisplay, batchMode).ConfigureAwait(false);
			xdoc.Save(targetFile,
				SaveOptions.DisableFormatting);// for SVG texts
			return c;
		}

	}

	public static class XElementsHandler
	{
		/// <summary>
		/// Get all the norminated nodes which are text leaf nodes not empty.
		/// </summary>
		/// <param name="root"></param>
		/// <param name="xpaths"></param>
		/// <param name="nsManager"></param>
		/// <returns></returns>
		public static IEnumerable<XElement> SelectElementsByXPaths(XElement root, IEnumerable<string> xpaths, XmlNamespaceManager nsManager)
		{
			foreach (var xpath in xpaths)
			{
				if (string.IsNullOrWhiteSpace(xpath)) 
					continue;

				foreach (var el in root.XPathSelectElements(xpath, nsManager))
				{

					if (IsTextLeaf(el))
					{
						yield return el;
					}
				}
			}
		}

		/// <summary>
		/// Text leaf not empty
		/// </summary>
		/// <param name="el"></param>
		/// <returns></returns>
		static bool IsTextLeaf(XElement el)
		{
			return !el.Elements().Any() &&
				   !string.IsNullOrWhiteSpace(el.Value);
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
		public static async Task<int> TranslateXmlTextNode(XElement xdoc, XmlNamespaceManager nsManager, string[] xpaths, ITranslate translator, ILogger logger, IProgressDisplay progressDisplay, bool batchMode)
		{
			ArgumentNullException.ThrowIfNull(xdoc);
			ArgumentNullException.ThrowIfNull(xpaths);
			const int maxUnits = 200;
			int translatedCount = 0;
			var matchedElements = SelectElementsByXPaths(xdoc, xpaths, nsManager).ToArray();
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

			async Task<int> TextByText(XElement[] elements)
			{

				foreach (var el in elements) // each represents a node to be translated
				{
					var translatedText = await translator.Translate(el.Value).ConfigureAwait(false);
					el.Value = translatedText;
					translatedCount++;
					progressDisplay?.Show(translatedCount, total);
				}

				return translatedCount;
			}

			async Task<int> Batch(IList<XElement> elements)
			{
				var strings = elements.Select(d => d.Value).ToArray();

				if (strings.Length == 0)
				{
					return 0;
				}

				var translatedStrings = await translator.Translate(strings).ConfigureAwait(false);
				int translatedIndex = 0;
				foreach (var el in elements)
				{
					el.Value = translatedStrings[translatedIndex];
					translatedIndex++;
					translatedCount++;
				}

				progressDisplay?.Show(translatedCount, total);
				return translatedCount;
			}


		}

	}
}
