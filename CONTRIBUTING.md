# Roadmap

The scope is within the needs of full-stack software developers. That is, the developer coordinates translation efforts by fellow developers, staffs and external translators.

* Converter between ResX and XLIFF
* Converter between Android Resource and XLIFF
* Support MS translator
* Support [Translation Memory eXchange (TMX)](https://learn.microsoft.com/en-us/globalization/localization/translation-memories)

# Design and Implementation

## Prerequisites

* .NET 9 SDK
* Visual Studio 2022

## Design Choices

During the initial development, XML serialization is used for reading and writing XML files of XLIFF. However, it turned out that the schemas of XLIFF 1.2 and 2.0 are not friendly to XML serialization. That is, though the generated .NET codes could read and write XLIFF files, the orders of plain text and interpolations can not be preserved, thus the semantic structure of text is corrupted. Therefore, the implementation is using XElement of XmlDOM.

Additionally, through XElement the original layout of the source document can be preserved.

Please create issue before forking and PR.