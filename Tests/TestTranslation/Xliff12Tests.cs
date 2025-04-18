﻿using Fonlow.GoogleTranslate;
using Google.Cloud.Translation.V2;
using System.Xml;
using System.Xml.Linq;

namespace TestXliff
{
	[Collection("ServicesLaunch")]
	public class Xliff12Tests
	{
		string apiKey = System.Environment.GetEnvironmentVariable("GoogleTranslateApiKey", EnvironmentVariableTarget.User);

		[Fact]
		public async Task TestGoogleTranslate(){
			var g = new XWithGT2("en", "zh-hans", apiKey);
			var t = await g.TranslateHtml("There are some registered numbered annotations not existing in poem anymore: <x id=\"PH\" equiv-text=\"numberList\"/>. Do you want to remove them?");
			Assert.Equal("有一些已注册的编号注释在诗中不再存在：<x id=\"PH\" equiv-text=\"numberList\"/> 。您要删除它们吗？", t);
		}

		[Fact]
		public void TestReadViaXElement()
		{
			using (FileStream fs = new System.IO.FileStream("xlf12/messages.zh-hans.xlf", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				var xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				var ns = xliffRoot.GetDefaultNamespace();
				var firstFile = xliffRoot.Element(ns + "file");
				Assert.Equal("en-US", firstFile.Attribute("source-language").Value);
				var body = firstFile.Element(ns + "body");
				Assert.NotNull(body);

				var units = body.Elements(ns + "trans-unit").ToArray();
				Assert.NotNull(units);

				var unit = units[1];
				var source = unit.Element(ns + "source");
				var nodes = source.Nodes().ToArray();
				Assert.Equal(3, nodes.Length);
				Assert.Equal(XmlNodeType.Text, nodes[0].NodeType);
				Assert.Equal(XmlNodeType.Element, nodes[1].NodeType);
				Assert.Equal(XmlNodeType.Text, nodes[2].NodeType);

				(nodes[0] as XText).Value += " Altered";
				(nodes[2] as XText).Value += " Altered";

				xDoc.Save("Xdocument.xlf"); // check to ensure the order of nodes not changed.
			}
		}

		[Fact]
		public void TestReadViaXElementWithGroup()
		{
			using (FileStream fs = new System.IO.FileStream("xlf12/Fonlow.VA.Languages.zh-Hans.xlf", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				var xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				var ns = xliffRoot.GetDefaultNamespace();
				var firstFile = xliffRoot.Element(ns + "file");
				Assert.Equal("en-US", firstFile.Attribute("source-language").Value);
				var body = firstFile.Element(ns + "body");
				Assert.NotNull(body);

				var units = body.Elements(ns + "trans-unit").ToArray();
				Assert.NotNull(units);
				Assert.Empty(units);
				
				var group = body.Elements(ns + "group");
				units = group.Elements(ns + "trans-unit").ToArray();
				var unit = units[1];
				unit.Element(ns + "target").Attribute("state").Value = "translated";

				xDoc.Save("XdocumentWithGroup.xlf"); // check to ensure the order of nodes not changed.
			}
		}

		[Fact]
		public async Task TestReadAndTranslate()
		{
			using (FileStream fs = new System.IO.FileStream("xlf12/messages.zh-hans.xlf", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				var xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				var wg = new Xliff12Translate(false);
				var c = await wg.TranslateXliff(xliffRoot, ["new"], false, new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseSimplified, apiKey), null, null);
				Assert.Equal(1, c);

				var ns = xliffRoot.GetDefaultNamespace();
				var firstFile = xliffRoot.Element(ns + "file");
				Assert.Equal("en-US", firstFile.Attribute("source-language").Value);
				var body = firstFile.Element(ns + "body");
				Assert.NotNull(body);

				var units = body.Elements(ns + "trans-unit").ToArray();
				Assert.NotNull(units);

				var unit = units[1];
				var target = unit.Element(ns + "target");
				var nodes = target.Nodes().ToArray();
				Assert.Equal(3, nodes.Length);
				Assert.Equal(XmlNodeType.Text, nodes[0].NodeType);
				Assert.Equal(XmlNodeType.Element, nodes[1].NodeType);
				Assert.Equal(XmlNodeType.Text, nodes[2].NodeType);

				Assert.Equal("有一些已注册的编号注释在诗中不再存在：", (nodes[0] as XText).Value);
				Assert.Equal("。您要删除它们吗？", (nodes[2] as XText).Value);

				xDoc.Save("XdocumentTranslated.xlf"); // check to ensure the order of nodes not changed.
			}
		}

		[Fact]
		public async Task TestReadAndTranslateWithBatchMode()
		{
			using (FileStream fs = new System.IO.FileStream("xlf12/messages.zh-hans.xlf", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				var xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				var wg = new Xliff12Translate(true);
				var c = await wg.TranslateXliff(xliffRoot, ["new"], false, new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseSimplified, apiKey), null, null);
				Assert.Equal(1, c);

				var ns = xliffRoot.GetDefaultNamespace();
				var firstFile = xliffRoot.Element(ns + "file");
				Assert.Equal("en-US", firstFile.Attribute("source-language").Value);
				var body = firstFile.Element(ns + "body");
				Assert.NotNull(body);

				var units = body.Elements(ns + "trans-unit").ToArray();
				Assert.NotNull(units);

				var unit = units[1];
				var target = unit.Element(ns + "target");
				var nodes = target.Nodes().ToArray();
				Assert.Equal(3, nodes.Length);
				Assert.Equal(XmlNodeType.Text, nodes[0].NodeType);
				Assert.Equal(XmlNodeType.Element, nodes[1].NodeType);
				Assert.Equal(XmlNodeType.Text, nodes[2].NodeType);

				Assert.Equal("有一些已注册的编号注释在诗中不再存在：", (nodes[0] as XText).Value);
				Assert.Equal("。您要删除它们吗？", (nodes[2] as XText).Value);

				xDoc.Save("XdocumentTranslated.xlf"); // check to ensure the order of nodes not changed.
			}
		}

		[Fact]
		public async Task TestReadAndTranslateWithGroup()
		{
			using (FileStream fs = new System.IO.FileStream("xlf12/Fonlow.VA.Languages.zh-Hans.xlf", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				var xDoc = XDocument.Load(fs);
				var xliffRoot = xDoc.Root;
				var wg = new Xliff12Translate(true);
				var c = await wg.TranslateXliff(xliffRoot, ["new"], false, new XWithGT2(LanguageCodes.English, LanguageCodes.ChineseSimplified, apiKey), null, null);
				Assert.Equal(10, c);

				var ns = xliffRoot.GetDefaultNamespace();
				var firstFile = xliffRoot.Element(ns + "file");
				Assert.Equal("en-US", firstFile.Attribute("source-language").Value);
				var body = firstFile.Element(ns + "body");
				Assert.NotNull(body);

				xDoc.Save("XdocumentWithGroupTranslated.xlf"); // check to ensure the order of nodes not changed.
			}
		}



	}
}
