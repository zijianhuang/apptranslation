using Fonlow.Translate;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace Fonlow.ResxTranslate
{
	public class ResxTranslation : IResourceTranslation
	{
		public ResxTranslation()
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


		public async Task<int> TranslateResx(XElement resxRoot, ITranslate translator, ILogger logger, IProgressDisplay progressDisplay)
		{
//#pragma warning disable CA2264
			ArgumentNullException.ThrowIfNull(resxRoot);
			ArgumentNullException.ThrowIfNull(translator);
			ArgumentNullException.ThrowIfNull(logger);
//#pragma warning restore CA2264

			var dataNodes = resxRoot.Elements("data").ToList();
			var total=dataNodes.Count;

			const int maxUnits = 200;
			int translatedCount = 0;
			if (batchMode)
			{
				var chunks = dataNodes.SplitLists(maxUnits);
				foreach (var chunk in chunks)
				{
					await Batch(chunk).ConfigureAwait(false); // always countsForUnit
				}
			}
			else
			{
				await TextByText(dataNodes).ConfigureAwait(false);
			}

			return translatedCount;

			async Task<int> TextByText(IList<XElement> someNodes){
				foreach (var node in someNodes)
				{
					var valueNode = node.Element("value");
					if (valueNode != null)
					{
						valueNode.Value = await translator.Translate(valueNode.Value).ConfigureAwait(false);
						translatedCount++;
						progressDisplay?.Show(translatedCount, total);
					}
				}

				return translatedCount;
			}

			async Task<int> Batch(IList<XElement> someNodes)
			{
				var strings = someNodes.Where(d =>
				{
					var valueNode = d.Element("value");
					return valueNode != null && !string.IsNullOrEmpty(valueNode.Value);
				}).Select(d => d.Element("value").Value).ToList();

				if (strings.Count == 0)
				{
					return 0;
				}

				var translatedStrings = await translator.Translate(strings).ConfigureAwait(false);
				int translatedIndex = 0;
				foreach (var n in someNodes)
				{
					var valueNode = n.Element("value");
					if (valueNode != null && !string.IsNullOrEmpty(valueNode.Value))
					{
						n.Value = translatedStrings[translatedIndex];
						translatedIndex++;
						translatedCount++;
					}
				}

				progressDisplay?.Show(translatedCount, total);
				return translatedCount;

			}
		}

		public async Task<int> Translate(ITranslate translator, ILogger logger, IProgressDisplay progressDisplay)
		{
			XDocument xDoc;
			int c;
			using (FileStream fs = new System.IO.FileStream(sourceFile, System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				xDoc = XDocument.Load(fs);
				var resxRoot = xDoc.Root;
				c = await TranslateResx(resxRoot, translator, logger, progressDisplay).ConfigureAwait(false);
			}

			xDoc.Save(targetFile);
			return c;
		}
	}
}
