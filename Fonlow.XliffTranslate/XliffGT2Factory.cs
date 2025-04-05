﻿using System.Xml.Linq;

namespace Fonlow.GoogleTranslate
{
	public sealed class XliffGT2Factory
	{
		public static IXliffTranslation CreateXliffGT2(string filePath, bool batchMode, Action<string> versionCallback)
		{
			string version;
			using (FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				var xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				var verNode = xliffRoot.Attribute("version");
				if (verNode == null)
				{
					throw new ArgumentException("Expect xliff version attribute.");
				}

				version = xliffRoot.Attribute("version").Value;
				versionCallback(version);

				if (version == "1.2")
				{
					return new Xliff12Translate(batchMode);
				}
				else if (version == "2.0")
				{
					return new Xliff20Translate(batchMode);
				}

				throw new ArgumentException("Expect either XLIFF v1.2 or v2.0");
			}
		}
	}
}
