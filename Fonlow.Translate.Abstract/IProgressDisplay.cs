namespace Fonlow.Translate
{
	public interface IProgressDisplay
	{
		void Show(int current, int totalUnits, bool isAllNew = false, int totalUnitsToTranslate = 0);
	}

}
