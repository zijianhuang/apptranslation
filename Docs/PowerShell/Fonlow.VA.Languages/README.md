# Translations

The translation is based on Microsoft Windows’s resx mechanism which is also used in Xamarin/MAUI.
2 additional tools are used:

* VS ResX Manager
* ResX/ResW Editor & Translator (http://www.neolib.net/). MS API key obtained through https://portal.azure.com/#create/Microsoft.CognitiveServicesTextTranslation free with up to 2M characters per month, as well as Google Cloud Translation API.
MAUI and VS ResX Manager can handle well both 2-letter language code and 3-letter language code.
ResX/ResW Editor & Translator (released in 2019) could not handle “fil” the 3-letter language code of Filipino when loading resource files, though its language includes “fil”. Google Translate actually uses “tl” – Tagalog.  Apparently this is a bug of the app.

When involving external translators, the Google Android translation file in XML format is used for the translators to work on. And the following scripts are used to prepare:
* ResxToStrings.ps1 for giving strings.xml to translators.
* StringsToResx.ps1 for transforming translated strings.xml to one of the resx files.


## Android
* [Translate and localize your app](https://support.google.com/googleplay/android-developer/answer/9844778)
* [Localize the UI with Translations Editor](https://developer.android.com/studio/write/translations-editor)


## iOS

Remarks: The development of VAC does not need to care about the technical details of iOS localization mechanism.

* [Localizing and varying text with a string catalog](https://developer.apple.com/documentation/xcode/localizing-and-varying-text-with-a-string-catalog). This is the recommended mechanism after Xcode 15 (the latest is 16)
* [Localizing and varying text with a string catalog](https://developer.apple.com/documentation/xcode/localizing-and-varying-text-with-a-string-catalog)
* [XCode Localization](https://developer.apple.com/documentation/xcode/localization)
* [From old strings and stringdict to String Catalog](https://belief-driven-design.com/xcode-string-catalogs-101-672f5/), also with the text format of String Catalog.
* [Examples of scstrings](https://poeditor.com/localization/files/xcstrings)

### Conversion Resources

* From strings.xml to localizable strings https://gunhansancar.com/tools/converter/
* [Convert ](https://localise.biz/free/converter/xml)