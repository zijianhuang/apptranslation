namespace Fonlow.Translate
{
	public class TmProgressDisplay : IProgressDisplay
	{
		public void Show(int current, int totalUnits, bool isAllNew = false, int totalUnitsToTranslate = 0)
		{
			Console.CursorLeft = 10;
			Console.Write(isAllNew ? $"{current} / {totalUnits}" : $"{current} / {totalUnitsToTranslate} / {totalUnits}");
		}
	}
}
