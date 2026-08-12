namespace Fonlow.XliffTranslate
{
	using System;
	using System.Text.RegularExpressions;
	using System.Xml;
	using System.Xml.Linq;

	public static class XElementExtensions
	{
		private static readonly Regex XmlnsAttrRegex =
		new Regex(@"\s+xmlns=""[^""]*""", RegexOptions.Compiled);
		/// <summary>
		/// Returns the inner XML of an element: text nodes as-is,
		/// and child elements/nodes as their raw XML markup.
		/// </summary>
		public static string GetInnerXml(this XElement element)
		{
			ArgumentNullException.ThrowIfNull(element);

			using (XmlReader reader = element.CreateReader())
			{
				reader.MoveToContent(); // move to the <source> element itself
				string innerXml = reader.ReadInnerXml();
				return XmlnsAttrRegex.Replace(innerXml, string.Empty);
			}
		}

		/// <summary>
		/// Replaces the content of target with the parsed XML nodes from innerXml,
		/// preserving embedded elements (like <x/>) as real XML nodes rather than
		/// escaped text.
		/// </summary>
		public static void SetInnerXml(this XElement target, string innerXml)
		{
			ArgumentNullException.ThrowIfNull(target);
			ArgumentNullException.ThrowIfNull(innerXml);

			XNamespace ns = target.Name.Namespace;

			string wrapperOpenTag = ns == XNamespace.None
				? "<wrapper>"
				: $"<wrapper xmlns=\"{ns.NamespaceName}\">";

			string wrappedXml = wrapperOpenTag + innerXml + "</wrapper>";

			XElement wrapper;
			try
			{
				wrapper = XElement.Parse(wrappedXml);
			}
			catch (System.Xml.XmlException ex)
			{
				throw new ArgumentException(
					"innerXml is not well-formed XML (check for unescaped & < > in plain text).",
					nameof(innerXml), ex);
			}

			target.RemoveNodes();
			target.Add(wrapper.Nodes());
		}
	}
}
