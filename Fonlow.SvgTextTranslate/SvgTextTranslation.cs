using Fonlow.Translate;
using Microsoft.Extensions.Logging;

namespace Fonlow.SvgTextTranslate
{
	public class SvgTextTranslation : IResourceTranslation
	{
		public SvgTextTranslation()
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

		public async Task<int> Translate(ITranslate g, ILogger logger, IProgressDisplay progressDisplay)
		{
			var svgText = File.ReadAllText(this.sourceFile);
			var tranUnits = SvgTextProcessor.ExtractTexts(svgText);
			var totalUnits = tranUnits.Count;
			var totalUnitsToTranslate = tranUnits.Count;

			const int maxUnits = 200;
			int translatedCount = 0;
			var translations = new Dictionary<string, string>();
			if (batchMode)
			{
				var chunks = tranUnits.SplitLists(maxUnits);
				foreach (var chunk in chunks)
				{
					await Batch(chunk); // always countsForUnit
				}
			}
			else
			{
				await TextByText(tranUnits);
			}


			Console.WriteLine();
			string translatedSvg = SvgTextProcessor.ReplaceTexts(svgText, translations);
			File.WriteAllText(targetFile, translatedSvg);
			return translatedCount;

			async Task<int> TextByText(IList<(string Key, string Text)> someUnits)
			{
				foreach (var unit in someUnits)
				{
					if (string.IsNullOrEmpty(unit.Text))
					{
						continue;
					}

					try
					{
						translations[unit.Text] = await g.Translate(unit.Text);
					}
					catch (HttpRequestException ex)
					{
						logger?.LogError(ex.Message);
						return translatedCount;
					}

					translatedCount++;
					progressDisplay?.Show(translatedCount, totalUnits);
				}

				return translatedCount;
			}

			async Task<int> Batch(IList<(string Key, string Text)> someUnits)
			{
				var strings = someUnits.Where(d => !string.IsNullOrEmpty(d.Text)).Select(d => d.Text).ToList();

				if (strings.Count == 0)
				{
					return 0;
				}

				var translatedStrings = await g.Translate(strings);

				int translatedIndex = 0;
				foreach (var unit in someUnits)
				{
					if (!string.IsNullOrEmpty(unit.Text))
					{
						translations[unit.Text] = translatedStrings[translatedIndex];
						translatedIndex++;
						translatedCount++;
					}
				}

				progressDisplay?.Show(translatedCount, totalUnits);
				return translatedCount;
			}

		}
	}
}
