using Fonlow.AndroidStrings;
using Fonlow.Translate;
using Fonlow.Translate.Abstract;
using Microsoft.Extensions.Logging;

namespace Fonlow.GoogleTranslate
{
	public class StringsTranslate : IStringsTranslation
	{
		public StringsTranslate(bool batchMode)
		{
			this.batchMode = batchMode;
		}

		readonly bool batchMode;

		public async Task<int> TranslateStrings(string filePath, string targetFile, ITranslate g, ILogger logger, Action<int, int> progressCallback)
		{
			var reader = new StringsRW();
			reader.Load(filePath);

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
					progressCallback?.Invoke(translatedCount, totalUnits);
				}

				return translatedCount;
			}

			async Task<int> Batch(IList<resourcesString> someUnits)
			{
				var strings = someUnits.Where(d=>!string.IsNullOrEmpty(d.Value)).Select(d => d.Value).ToList();

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

				progressCallback?.Invoke(translatedCount, totalUnits);
				return translatedCount;
			}

		}
	}
}
