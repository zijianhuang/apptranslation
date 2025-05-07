using Fonlow.AndroidStrings;
using Fonlow.Translate;
using Microsoft.Extensions.Logging;

namespace Fonlow.StringsTranslate
{
	public class StringsTranslation : IResourceTranslation
	{
		public StringsTranslation()
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
			var reader = new StringsRW();
			reader.Load(sourceFile);

			var tranUnits = new List<resourcesString>(reader.GetStrings());
			var totalUnits = tranUnits.Count;
			var totalUnitsToTranslate = tranUnits.Count;

			const int maxUnits = 200;
			int translatedCount = 0;
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
			reader.WriteToFile(targetFile);
			return translatedCount;

			async Task<int> TextByText(IList<resourcesString> someUnits)
			{
				foreach (var unit in someUnits)
				{
					if (string.IsNullOrEmpty(unit.Value))
					{
						continue;
					}

					try
					{
						unit.Value = await g.Translate(unit.Value);
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

			async Task<int> Batch(IList<resourcesString> someUnits)
			{
				var strings = someUnits.Where(d => !string.IsNullOrEmpty(d.Value)).Select(d => d.Value).ToList();

				if (strings.Count == 0)
				{
					return 0;
				}

				var translatedStrings = await g.Translate(strings);

				int translatedIndex = 0;
				foreach (var unit in someUnits)
				{
					if (!string.IsNullOrEmpty(unit.Value))
					{
						unit.Value = translatedStrings[translatedIndex];
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
