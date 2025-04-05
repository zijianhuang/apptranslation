using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Fonlow.AndroidStrings
{
	public class StringsRW
	{
		resources root;
		public bool Load(string filePath)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(resources));
			try
			{
				using (FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
				{
#pragma warning disable CA5369 // Use XmlReader for 'XmlSerializer.Deserialize()'
					root = serializer.Deserialize(fs) as resources;
#pragma warning restore CA5369 // Use XmlReader for 'XmlSerializer.Deserialize()'
					return root != null;
				}

			}
			catch (Exception ex) when (ex is ArgumentException || ex is System.IO.IOException || ex is System.Security.SecurityException)
			{
				Trace.TraceWarning("Cannot locate the doc xml of the assembly: " + ex.ToString());
				return false;
			}
		}

		/// <summary>
		/// Called after being loaded and modified.
		/// </summary>
		/// <param name="filePath"></param>
		public void WriteToFile(string filePath)
		{
			using (FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
			{
				XmlSerializer serializer = new XmlSerializer(typeof(resources));
				using var writer = new XmlTextWriter(fs, Encoding.UTF8);
				writer.Formatting = Formatting.Indented;
				serializer.Serialize(writer, root);
			}
		}

		public resourcesString[] GetStrings()
		{
			return root.@string;
		}

	}
}
