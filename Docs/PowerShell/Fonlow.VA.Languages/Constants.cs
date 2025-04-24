using Fonlow.VA.Languages;

namespace Fonlow.VA
{
	public static class StaticConstants
	{
		/// <summary>
		/// Chart names from the language resources.
		/// </summary>
		/// <remarks>When adding new chart, remember to add for chartSelected.SelectedIndex in ChartPage.xaml.cs in addition to SelectedChartIndex in this file.</remarks>
		public static readonly string[] ChartNames = new string[] {
			AppResources.ChartSnellen,
			AppResources.ChartE,
			AppResources.ChartLandoltC
		};
	}
}
