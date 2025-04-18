﻿using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace Fonlow.GoogleTranslate
{
	/// <summary>
	/// To combine Xliff 1.2 or 2.0 with a translation engine.
	/// </summary>
	public interface IXliffTranslation
	{
		Task<int> TranslateXliff(string filePath, string targetFile, string[] forStates, bool unchangeState, ITranslate g, ILogger logger, Action<bool, int, int, int> progressCallback);
		Task<int> TranslateXliff(XElement xliffRoot, string[] forStates, bool unchangeState, ITranslate g, ILogger logger, Action<bool, int, int, int> progressCallback);
	}
}