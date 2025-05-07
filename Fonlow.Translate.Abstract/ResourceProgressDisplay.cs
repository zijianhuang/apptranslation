using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fonlow.Translate.Abstract
{
	public class ResourceProgressDisplay : IProgressDisplay
	{
		public void Show(int current, int totalUnits, bool isAllNew = false, int totalUnitsToTranslate = 0)
		{
			Console.CursorLeft = 10;
			Console.Write($"{current} / {totalUnits}");
		}
	}
}
