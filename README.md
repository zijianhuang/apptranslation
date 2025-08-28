# Overview

Batch translation of resource files through CLI tools with machine translation engines.

**Supported Translation Resource Formats:**
* XLIFF 1.2
* XLIFF 2.0
* Microsoft ResX
* Android String Resource
* JSON

**Supported Translation Engines:**
* Google Translate v2
* Google Cloud Translation v3
* Microsoft Azure AI Translator

You should setup your own accounts and API credentials of these engines.

**Supported Conversion:**
* Convert and merge between ResX and XLIFF 1.2 / 2.0

Through proper scripting, .NET translation resource could be using XLIFF as Translation Memory.

**Remarks:**
* It is presumed that you have rich experience in using each translation engine through API regarding setup and authentication. Or you study the documentations of respective engines.

**Supported Operating Systems:**

* Windows
* MacOS
* Linux

## Background
For full-stack software developers, there are wide variety of tools like:
* PO Editor
* ResX Resource Manager
* ResX Editor & Translator
* ...

The translation tools used by software developers:
1. Integration with IDE like Visual Studio, XCode and Android Studio etc.
2. Batch processing.

As of 2020s, most tools you could find are cloud based, depending to another cloud-based translation services like Google Translate API and Microsoft Translator API. And these tools are mostly subscription based. If you do translations only occasionally and casually, such subscription model may not be appealing. Also the operation overheads of these cloud based tools are too much for smooth SDLC or CI.

Additionally, if you prefer batch processing, the CLI tools included in this project, developed by a full-stack software developer for full-stack software developers, may be appealing to you.

# Tools
According to [Google Cloud Translation pricing](https://cloud.google.com/translate/pricing#charged-characters):

* You are charged for all characters that you include in a Cloud Translation request, even untranslated characters. This includes, for example, whitespace characters. If you translate `<p>こんにちは</p>` to English, it counts as 12 characters for the purposes of billing.
* Cloud Translation also charges for empty queries. If you make a request without any content, Cloud Translation charges one character for the request.

XLIFF translation units may contain elements of interpolation like:
 ```xml
 <source>File size is <ph id="0" equiv="PH" disp="ByteFormatPipe.formatBytes(this.file.size)"/>, and it may take sometime to upload.</source>
 ```
 Simply sending the content to Google Translate with `translate` or `translateHtml` may trigger unnecessary code points and charging, the core logic of these tools sends only the plain text content to save money.

For the detailed features, just run the CLI tool without parameters you will see help and examples.

## GoogleTranslateXliff.exe

```
GoogleTranslateXliff.exe
Use Google Translate v2 or v3 to translate XLIFF v1.2 or v2.0 file.
XLIFF Translator with Google Translate  version 1.4.0.0
Copyright © Zijian Huang 2018-2025


   /ForStates, /SS     For translation unit of states. Default to new for v1.2 and initial for v2.0, e.g.,
                       /SS="initial" "translated"
   /NotChangeState,    Not to change the state of translation unit to translated after translation.
   /NCS
   /ApiKey, /AK        Google Translate API key. e.g., /AK=zasdfSDFSDfsdfdsfs234sdsfki
   /ApiKeyFile, /AKF   Google Translate API key stored in a text file. e.g.,
                       /AKF=C:/Users/Public/DevApps/GtApiKey.txt
   /ApiVersion, /AV    Google Translate API version. Default to V2. If V3, a client secret JSON file is expected.
   /ClientSecretFile,  Google Cloud Translate V3 does not support API key but rich ways of authentications. This app
   /CSF                uses client secret JSON file you could download from your Google Cloud Service account.
   /SourceFile, /F     Source file path
   /TargetFile, /TF    Target file path
   /SourceLang, /SL    Source language. e.g., /SL=fr
   /TargetLang, /TL    Target language. e.g., /TL=zh
   /Batch, /B          Batch processing of string array to improve overall speed.
   /Help, /h, /?       Shows this help text



Examples:
GoogleTranslateXliff.exe /AK=YourGoogleTranslateV2ApiKey /F=myUiMessages.es.xlf ---- For in-place translation.
GoogleTranslateXliff.exe /AK=YourGoogleTranslateV2ApiKey /F:myUiMessages.ja.xlf /TF:myUiMessagesTranslated.ja.xlf ---- from the source locale file to a new target file in Japanese
GoogleTranslateXliff.exe /AK=YourGoogleTranslateV2ApiKey /F:myUiMessages.xlf /TF:myUiMessages.es.xlf /TL=es ---- From the source template file to a new target file in Spanish.
GoogleTranslateXliff.exe /AV=v3 /CSF=client_secret.json /B /F:myUiMessages.es.xlf ---- Use Google Cloud Translate V3 and batch mode.
```

**Hints:**
* By default among all [states](https://docs.oasis-open.org/xliff/v1.2/os/xliff-core.html#state) of XLIFF 1.2, this program cares about only "new" and "translated".
* By default among all [states](https://docs.oasis-open.org/xliff/xliff-core/v2.0/xliff-core-v2.0.html#state) of XLIFF 2.0, this program cares about only "initial" and "translated".
* If you have human translators involved in the SDLC, the translators may change the states to "reviewed" or "final" etc.


## GoogleTranslateResx.exe

```
GoogleTranslateResx.exe
Use Google Translate v2 or v3 to translate Microsoft ResX
ResX Translator with Google Translate  version 1.2.0.0
Copyright © Zijian Huang 2018-2025


   /SourceFile, /F     Source file path, e.g., /F=AppResources.resx
   /TargetFile, /TF    Target file path. e.g., /TF=c:/AppResources.ja.resx
   /SourceLang, /SL    Source language. e.g., /SL=fr
   /TargetLang, /TL    Target language. e.g., /TL=zh
   /ApiKey, /AK        Google Translate API key. e.g., /AK=zasdfSDFSDfsdfdsfs234sdsfki
   /ApiKeyFile, /AKF   Google Translate API key stored in a text file. e.g.,
                       /AKF=C:/Users/Public/DevApps/GtApiKey.txt
   /Batch, /B          Batch processing of strings to improve overall speed. V2 and V3 support.
   /ApiVersion, /AV    Google Translate API version. Default to V2. If V3, a client secret JSON file is expected.
   /ClientSecretFile,  Google Cloud Translate V3 does not support API key but rich ways of authentications. This app
   /CSF                uses client secret JSON file you could download from your Google Cloud Service account.
   /Help, /h, /?       Shows this help text



Examples:
GoogleTranslateResx.exe /AK=YourGoogleTranslateV2ApiKey /SL=en /TL=zh-hant /F:AppResources.zh-hant.resx ---- For in-place translation when AppResources.zh-hant.resx is not yet translated
GoogleTranslateResx.exe /AK=YourGoogleTranslateV2ApiKey /SL=en /TL=ja /F:strings.xml /TF:AppResources.ja.resx ---- from the source locale file to a new target file in Japanese
GoogleTranslateResx.exe /AK=YourGoogleTranslateV2ApiKey /F:AppResources.resx /TF:AppResources.es.resx /TL=es ---- From the source template file to a new target file in Spanish.
GoogleTranslateResx.exe /AV=v3 /CSF=client_secret.json /B  /SL=en /TL=es /F:AppResources.es.resx ---- Use Google Cloud Translate V3 and batch mode.
```

## GoogleTranslateStrings.exe

```
GoogleTranslateStrings.exe
Use Google Translate v2 or v3 to translate String Resource
Google Translate for Android String Resource  version 1.0
Fonlow (c) 2025


   /SourceFile, /F     Source file path, e.g., /F=strings.xml
   /TargetFile, /TF    Target file path. e.g., /TF=c:/strings.zh.xml
   /SourceLang, /SL    Source language. e.g., /SL=fr
   /TargetLang, /TL    Target language. e.g., /TL=zh
   /ApiKey, /AK        Google Translate API key. e.g., /AK=zasdfSDFSDfsdfdsfs234sdsfki
   /Batch, /B          Batch processing of strings to improve overall speed. V2 and V3 support.
   /ApiVersion, /AV    Google Translate API version. Default to V2. If V3, a client secret JSON file is expected.
   /ClientSecretFile,  Google Cloud Translate V3 does not support API key but rich ways of authentications. This app uses client
   /CSF                secret JSON file you could download from your Google Cloud Service account.
   /Help, /h, /?       Shows this help text



Examples:
GoogleTranslateStrings.exe /AK=YourGoogleTranslateV2ApiKey /SL=en /TL=zh-hant /F:strings.zh-hant.xml ---- For in-place translation when strings.zh-hant.xml is not yet translated
GoogleTranslateStrings.exe /AK=YourGoogleTranslateV2ApiKey /SL=en /TL=ja /F:strings.xml /TF:strings.ja.xml ---- from the source locale file to a new target file in Japanese
GoogleTranslateStrings.exe /AK=YourGoogleTranslateV2ApiKey /F:myUiMessages.xml /TF:myUiMessages.es.xml /TL=es ---- From the source template file to a new target file in Spanish.
GoogleTranslateStrings.exe /AV=v3 /CSF=client_secret.json /B  /SL=en /TL=es /F:myUiMessages.es.xml ---- Use Google Cloud Translate V3 and batch mode.
```

## GoogleTranslateJson.exe
```
GoogleTranslateJson.exe
Use Google Translate v2 or v3 to translate selected string value properties of JSON object
GoogleTranslateJson  version 1.0.0.0


   /Properties, /PS    JSON object properties to be translated, e.g., /PS="parent.folder.name" "parent.fonlder.address"
   /PropertiesFile,    Each line declares a JSON object property to be translated, e.g., /PSF=JsonProperties.txt
   /PSF
                       0: JsonSerializerOptions.Default, 1: JsonSerializerOptions.Web, 3: Custom with option Intented and UnsafeRelaxedJsonEscaping.
   /SerializationConfig,                                                                                                                                                              
   /SC
   /Indented, /Ind     Outputted text in indented, when SerializationConfig=2.
                       Outputted Unicode characters not escaped, when SerializationConfig=2.
   /UnsafeRelaxedJsonEscaping,                                                                                                                                                        
   /NUE
   /ApiKey, /AK        Google Translate API key. e.g., /AK=zasdfSDFSDfsdfdsfs234sdsfki
   /ApiKeyFile, /AKF   Google Translate API key stored in a text file. e.g., /AKF=C:/Users/Public/DevApps/GtApiKey.txt
   /ApiVersion, /AV    Google Translate API version. Default to V2. If V3, a client secret JSON file is expected.
   /ClientSecretFile,  Google Cloud Translate V3 does not support API key but rich ways of authentications. This app uses client secret JSON file you could download from your
   /CSF                Google Cloud Service account.
                       Translate from target language to source language and save the result to the target file so you can compare. Both SourceFile and TargetFile must be defined.
   /ReversedTranslation,                                                                                                                                                              
   /Reversed
   /SourceFile, /F     Source file path
   /TargetFile, /TF    Target file path
   /SourceLang, /SL    Source language. e.g., /SL=fr
   /TargetLang, /TL    Target language. e.g., /TL=zh
   /Batch, /B          Batch processing of string array to improve overall speed.
   /Help, /h, /?       Shows this help text



Examples:
GoogleTranslateJson.exe /AK=YourGoogleTranslateV2ApiKey /SL=en /TL=zh-hant /F:jsonld.zh-hant.json /PS:data.user.name data.user.address ---- For in-place translation when jsonld.zh-hant.json is not yet translated
GoogleTranslateJson.exe /AK=YourGoogleTranslateV2ApiKey /SL=en /TL=ja /F:strings.xml /TF:jsonld.ja.json /PS:data.user.name ---- from the source locale file to a new target file in Japanese
GoogleTranslateJson.exe /AK=YourGoogleTranslateV2ApiKey /F:jsonld.json /TF:jsonld.es.json /TL=es /PS:data.user.name ---- From the source template file to a new target file in Spanish.
GoogleTranslateJson.exe /AV=v3 /CSF=client_secret.json /B  /SL=en /TL=es /F:jsonld.es.json /PS:data.user.name ---- Use Google Cloud Translate V3 and batch mode.
```

### SerializationConfig
#### JsonSerializerOptions.Default vs Web

| Feature | Default | Web | 
| ---- | ---- | ---- | 
| Naming Policy | null (preserves original property names) | CamelCase (e.g., UserName → userName) | 
| Encoder | JavaScriptEncoder.Default (escapes non-ASCII) | JavaScriptEncoder.Default (same) | 
| IgnoreNullValues (obsolete) | false | true (in .NET 5 and earlier) | 
| DefaultIgnoreCondition | Never | WhenWritingNull (in .NET 6+) | 
| PropertyNameCaseInsensitive | false | true | 
| ReadCommentHandling | Disallow | Disallow | 
| AllowTrailingCommas | false | false | 
| Indented Output | false | false | 

And option "Indented" and "UnsafeRelaxedJsonEscaping" are to have an instance of JsonSerializerOptions similar to "Default" but with a little adjustment.

### When to Use Which?
Use "Default" when:
- You want exact property names preserved
- You need full control over null handling
- You're serializing for internal systems or config files

Use "Web" when:
- You're building REST APIs or web services
- You want camelCase naming and cleaner payloads
- You want to follow JSON conventions used in JavaScript clients


**Remarks:**
* Since this utility is to translate some properties, it will be rare that you need what offered by "Web".

### When Is It Safe to Use UnsafeRelaxedJsonEscaping?
| Scenario | Safe? | Notes | 
| ---- | ---- | ---- |
| Internal apps with full UTF-8 support | ✅ | Improves readability and localization | 
| Web APIs with proper headers | ✅ | Ensure charset=utf-8 is set | 
| Embedded in HTML/JS | ⚠️ | Escape manually or sanitize carefully | 
| Legacy systems or unknown consumers | ❌ | Prefer escaped Unicode for safety | 






### Summary
- Default is a neutral baseline—ideal for internal serialization where you want full control over naming and behavior.
- Web is optimized for web APIs, aligning with common JSON conventions like camelCase and ignoring nulls.





## MsTranslatorXliff.exe

```
MsTranslatorXliff.exe
Use Microsoft Azure AI Translator to translate XLIFF v1.2 or v2.0 file.
MsTranslatorXliff  version 1.0.0.0


   /ForStates, /SS     For translation unit of states. Default to new for v1.2 and initial for v2.0, e.g.,
                       /SS="initial" "translated"
   /NotChangeState,    Not to change the state of translation unit to translated after translation.
   /NCS
   /ApiKey, /AK        Microsoft Translator API key. e.g., /AK=zasdfSDFSDfsdfdsfs234sdsfki
   /ApiKeyFile, /AKF   MS Translator API key stored in a text file. e.g., /AKF=C:/Users/Public/DevApps/GtApiKey.txt
   /Region, /RG        Region associated with the key. Always required. e.g., /RG=australiaeast
   /CategoryId, /CA    Category ID from one of your custom translator's projects in the form of
                       WorkspaceID+CategoryCode, used by Batch mode, while the default is general . e.g.,
                       /CA=a3a1eeb1-7e2b-4098-b293-da762fe3bb79-INTERNT
   /SourceFile, /F     Source file path
   /TargetFile, /TF    Target file path
   /SourceLang, /SL    Source language. e.g., /SL=fr
   /TargetLang, /TL    Target language. e.g., /TL=zh
   /Batch, /B          Batch processing of string array to improve overall speed.
   /Help, /h, /?       Shows this help text



Examples:
MsTranslatorXliff.exe /AK=MsTranslatorApiKey /RG=australiaeast /F=myUiMessages.es.xlf ---- For in-place translation.
MsTranslatorXliff.exe /AK=MsTranslatorApiKey /RG=australiaeast /F:myUiMessages.ja.xlf /TF:myUiMessagesTranslated.ja.xlf ---- from the source locale file to a new target file in Japanese
MsTranslatorXliff.exe /AK=MsTranslatorApiKey /RG=australiaeast /F:myUiMessages.xlf /TF:myUiMessages.es.xlf /TL=es ---- From the source template file to a new target file in Spanish.
```

## MsTranslatorResx.exe

```
MsTranslatorResx.exe
Use Microsoft Azure AI Translator to translate Microsoft ResX
MsTranslatorResx  version 1.0.0.0


   /ApiKey, /AK        Microsoft Translator API key. e.g., /AK=zasdfSDFSDfsdfdsfs234sdsfki
   /ApiKeyFile, /AKF   MS Translator API key stored in a text file. e.g., /AKF=C:/Users/Public/DevApps/GtApiKey.txt
   /Region, /RG        Region associated with the key. Always required. e.g., /RG=australiaeast
   /CategoryId, /CA    Category ID from one of your custom translator's projects in the form of
                       WorkspaceID+CategoryCode, used by Batch mode, while the default is general . e.g.,
                       /CA=a3a1eeb1-7e2b-4098-b293-da762fe3bb79-INTERNT
   /SourceFile, /F     Source file path
   /TargetFile, /TF    Target file path
   /SourceLang, /SL    Source language. e.g., /SL=fr
   /TargetLang, /TL    Target language. e.g., /TL=zh
   /Batch, /B          Batch processing of string array to improve overall speed.
   /Help, /h, /?       Shows this help text



Examples:
MsTranslatorResx.exe /AK=MsTranslatorApiKey /RG=australiaeast /SL=en /TL=zh-hant /F:AppResources.zh-hant.resx ---- For in-place translation when AppResources.zh-hant.resx is not yet translated
MsTranslatorResx.exe /AK=MsTranslatorApiKey /RG=australiaeast /SL=en /TL=ja /F:strings.xml /TF:AppResources.ja.resx ---- from the source locale file to a new target file in Japanese
MsTranslatorResx.exe /AK=MsTranslatorApiKey /RG=australiaeast /F:AppResources.resx /TF:AppResources.es.resx /TL=es ---- From the source template file to a new target file in Spanish.
```

## MsTranslatorStrings.exe

```
MsTranslatorStrings.exe
Use Microsoft Azure AI Translator to translate Android String Resource
MsTranslatorStrings  version 1.0.0.0


   /ApiKey, /AK        Microsoft Translator API key. e.g., /AK=zasdfSDFSDfsdfdsfs234sdsfki
   /ApiKeyFile, /AKF   MS Translator API key stored in a text file. e.g., /AKF=C:/Users/Public/DevApps/GtApiKey.txt
   /Region, /RG        Region associated with the key. Always required. e.g., /RG=australiaeast
   /CategoryId, /CA    Category ID from one of your custom translator's projects in the form of
                       WorkspaceID+CategoryCode, used by Batch mode, while the default is general . e.g.,
                       /CA=a3a1eeb1-7e2b-4098-b293-da762fe3bb79-INTERNT
   /SourceFile, /F     Source file path
   /TargetFile, /TF    Target file path
   /SourceLang, /SL    Source language. e.g., /SL=fr
   /TargetLang, /TL    Target language. e.g., /TL=zh
   /Batch, /B          Batch processing of string array to improve overall speed.
   /Help, /h, /?       Shows this help text



Examples:
MsTranslatorStrings.exe /AK=MsTranslatorApiKey /RG=australiaeast /SL=en /TL=zh-hant /F:AppResources.zh-hant.xml ---- For in-place translation when AppResources.zh-hant.xml is not yet translated
MsTranslatorStrings.exe /AK=MsTranslatorApiKey /RG=australiaeast /SL=en /TL=ja /F:strings.xml /TF:AppResources.ja.xml ---- from the source locale file to a new target file in Japanese
MsTranslatorStrings.exe /AK=MsTranslatorApiKey /RG=australiaeast /F:AppResources.xml /TF:AppResources.es.xml /TL=es ---- From the source template file to a new target file in Spanish.
```

## MsTranslatorJson.exe

```
/MsTranslatorJson.exe
Use Microsoft Azure AI Translator to translate JSON object
MsTranslatorJson  version 1.0.0.0


   /Properties, /PS    JSON object properties to be translated, e.g., /PS="parent.folder.name" "parent.folder.address"
   /PropertiesFile,    Each line declares a JSON object property to be translated, e.g., /PSF=JsonProperties.txt
   /PSF
                       0: JsonSerializerOptions.Default, 1: JsonSerializerOptions.Web, 3: Custom with option Intented and
   /SerializationConfig, UnsafeRelaxedJsonEscaping.
   /SC
   /Indented, /Ind     Outputted text in indented, when SerializationConfig=2.
                       Outputted Unicode characters not escaped, when SerializationConfig=2.
   /UnsafeRelaxedJsonEscaping,
   /NUE
   /ApiKey, /AK        Microsoft Translator API key. e.g., /AK=zasdfSDFSDfsdfdsfs234sdsfki
   /ApiKeyFile, /AKF   MS Translator API key stored in a text file. e.g., /AKF=C:/Users/Public/DevApps/GtApiKey.txt
   /Region, /RG        Region associated with the key. Always required. e.g., /RG=australiaeast
   /CategoryId, /CA    Category ID from one of your custom translator's projects in the form of WorkspaceID+CategoryCode, used by Batch mode, while
                       the default is general . e.g., /CA=a3a1eeb1-7e2b-4098-b293-da762fe3bb79-INTERNT
   /SourceFile, /F     Source file path
   /TargetFile, /TF    Target file path
   /SourceLang, /SL    Source language. e.g., /SL=fr
   /TargetLang, /TL    Target language. e.g., /TL=zh
   /Batch, /B          Batch processing of string array to improve overall speed.
   /Help, /h, /?       Shows this help text



Examples:
MsTranslatorteJson.exe /AK=YourMsTranslatorteApiKey /RG=australiaeast /SL=en /TL=zh-hant /F:jsonld.zh-hant.json /PS:data.user.name data.user.address ---- For in-place translation when jsonld.zh-hant.json is not yet translated
MsTranslatorteJson.exe /AK=YourMsTranslatorteApiKey /RG=australiaeast /SL=en /TL=ja /F:jsonld.json /TF:jsonld.ja.json /PS:data.user.name ---- from the source locale file to a new target file in Japanese
MsTranslatorteJson.exe /AK=YourMsTranslatorteApiKey /RG=australiaeast /Ind /NUE /SC=2 /F:jsonld.json /TF:jsonld.es.json /TL=es /PS:data.user.name ---- From the source template file to a new target file in Spanish.
```

## XliffResXConverter.exe

This program can merge what in ResX to XLIFF, and merge XLIFF back to ResX. Together with GoogleTranslateXliff.exe and some PowerShell scripts, you may establish seamless SDLC and Continuous Integration. Check [README](XliffResXConverter/README.md) for details.

# Build and Deployment

**Prerequisites:**
* .NET 9 SDK for development and build
* .NET 9 Runtime for execution

This repository does not release binary builds generally. You may check-out the source codes of master or a latest tag like v1_stable, and then use respective PS1 scripts to build each CLI app for Windows, [MacOS](https://learn.microsoft.com/en-us/dotnet/core/install/macos) or [Linux](https://learn.microsoft.com/en-us/dotnet/core/install/linux).

![Build Scripts](Docs/Articles/Screenshots/BuildScripts.png)

![MacOS run](Docs/Articles//Screenshots//MacStart.png)

## Microsoft Translator

The translator service on MS Azure keep evolving rapidly. As of August 2025, the translation API keys are managed through:
`Azure / AI Foundry / Translator / YourTranslatorInstance / Resource Management / Keys and Endpoint`

## Google Translate

# Continuous Integration

The [PowerShell folder](Docs/PowerShell) of this repository provides a few examples of PowerShell scripts for CI:
* [GoogleTranslate.ps1](Docs/PowerShell/Angular/GoogleTranslate.ps1) for Angular 2+.
* [MergeTranslateMerge.ps1](Docs/PowerShell/Fonlow.VA.Languages/MergeTranslateMerge.ps1) for [Visual Studio ResX using XLIFF as Translation Memory](XliffResXConverter/README.md).

# Articles

* [Batch Processing of Translation Resource for Angular Apps](/Docs//Articles/BatchProcessingAngular.md) also published on [dev.to](https://dev.to/zijianhuang/batch-processing-of-translation-resource-for-angular-apps-1il9) .

# Contributing

Please check [CONTRIBUTING.md](CONTRIBUTING.md).

# Artificial Intelligence 

The capability of AI in recent years is amazing. When you have a bunch of data in simple meta structure, AI like Copilot could understand the structure and do the repetitive tasks for you. For example, in Google Play Console, before submitting an update, the "What's New" section presents:
```xml
<en-US>
1. Most built-in help contents are refined.
2. When the visual acuity notation is LogMAR, MAR  or VAS, the scoring of the assessment is LogMAR Scoring.
</en-US>
<ar>
Enter or paste your release notes for ar here
</ar>
<de-DE>
Enter or paste your release notes for de-DE here
</de-DE>
<es-ES>
Enter or paste your release notes for es-ES here
</es-ES>
<fil>
Enter or paste your release notes for fil here
</fil>
<fr-FR>
Enter or paste your release notes for fr-FR here
</fr-FR>
<hi-IN>
Enter or paste your release notes for hi-IN here
</hi-IN>
<id>
Enter or paste your release notes for id here
</id>
...
```

I asked Copilot to translate, and it gives fairly accurate translations conforming to the meta format.

```xml
<en-US>
1. Most built-in help contents are refined.
2. When the visual acuity notation is LogMAR, MAR or VAS, the scoring of the assessment is LogMAR Scoring.
</en-US>
<ar>
1. تم تحسين معظم محتويات المساعدة المدمجة.
2. عند استخدام رموز حدة البصر LogMAR أو MAR أو VAS، يتم احتساب التقييم باستخدام نظام LogMAR.
</ar>
<de-DE>
1. Die meisten integrierten Hilfetexte wurden überarbeitet.
2. Bei den Sehschärfeangaben LogMAR, MAR oder VAS erfolgt die Bewertung nach dem LogMAR-System.
</de-DE>
<es-ES>
1. La mayoría de los contenidos de ayuda integrados han sido refinados.
2. Cuando la notación de agudeza visual es LogMAR, MAR o VAS, la evaluación se realiza con puntuación LogMAR.
</es-ES>
...
<vi>
1. Hầu hết nội dung trợ giúp tích hợp đã được cải tiến.
2. Khi ký hiệu thị lực là LogMAR, MAR hoặc VAS, việc đánh giá sử dụng thang điểm LogMAR.
</vi>
<zh-CN>
1. 大多数内置帮助内容已优化。
2. 当视力表示为 LogMAR、MAR 或 VAS 时，评估采用 LogMAR 评分。
</zh-CN>
<zh-HK>
1. 大部分內置說明內容已經優化。
2. 當視力標示為 LogMAR、MAR 或 VAS 時，評估將採用 LogMAR 評分方式。
</zh-HK>

```

And you may try to use Copilot or alike to see if AI could replace the tools introduced in this open source project.