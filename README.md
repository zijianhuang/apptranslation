# Background
This series of translation tools is based on my quests to batch translations of software UIs in last 10 years, after searching and buying existing products like:
* ResX Editor & Translator

As of 2020s, most tools I could find are cloud based, depending to another cloud-based translation services like Google Translate. And these tools are mostly subscription based. As I do translations only occasionally and casually, such subscription model is not appealing to me. 

Additionally, I prefer batch processing. Here you are some CLI tools included in this project, developed by a full-stack software developer for full-stack software developers.

**Remarks:**
* I have searched the Internet to find tools commercial, free or open source, before starting the development. Hopefully the scope and the features of these CLI tools meet your need since you are a full stack software developer.

# Overview

**Supported Translation Resource Formats:**
* XLIFF 1.2
* XLIFF 2.0
* Microsoft ResX
* Android String Resource

**Supported Translation Engines:**
* Google Translate v2
* Google Cloud Translation v3

# Features
According to [Cloud Translation pricing](https://cloud.google.com/translate/pricing#charged-characters):

* You are charged for all characters that you include in a Cloud Translation request, even untranslated characters. This includes, for example, whitespace characters. If you translate `<p>こんにちは</p>` to English, it counts as 12 characters for the purposes of billing.
* Cloud Translation also charges for empty queries. If you make a request without any content, Cloud Translation charges one character for the request.

Since a XLIFF translation unit may contain elements of interpolation, simply sending the content to Google Translate with `translateHtml` may trigger unnecessary code points and charging, the core logic of these tools sends only the plain text content to save money.

For the detailed features, just run the CLI tool without parameters.

## GoogleTranslateXliff.exe

```
GoogleTranslateXliff.exe
Use Google Translate v2 or v3 to translate XLIFF v1.2 or v2.0 file.
Google Translate for XLIFF  version 1.0
Fonlow (c) 2025


   /SourceFile, /F     Source file path, e.g., /F=myfile.zh.xliff
   /TargetFile, /TF    Target file path. If not defined, the source file will be overwritten, e.g.,
                       /TF=c:/locales/myfileTT.zh.xliff
   /SourceLang, /SL    Source language. Default to xliff/file/source-language or xliff/srcLang. e.g., /SL=fr
   /TargetLang, /TL    Target language. Default to xliff/file/target-language or xliff/trgLang.  e.g., /TL=es
   /ApiKey, /AK        Google Translate API key. e.g., /AK=zasdfSDFSDfsdfdsfs234sdsfki
   /ForStates, /SS     For translation unit of states. Default to new for v1.2 and initial for v2.0, e.g.,
                       /SS="initial" "translated"
   /NotChangeState,     Not to change the state of translation unit to translated after translation.
   /NCS
   /Batch, /B          Batch processing of strings to improve overall speed.
   /ApiVersion, /AV    Google Translate API version. Default to V2. If V3, a client secret JSON file is expected.
   /ClientSecretFile,  Google Cloud Translate V3 does not support API key but rich ways of authentications. This app
   /CSF                uses client secret JSON file you could download from your Google Cloud Service account.
   /Help, /h, /?       Shows this help text



Examples:
GoogleTranslateXliff.exe /AK=YourGoogleTranslateV2ApiKey /F=myUiMessages.es.xlf ---- For in-place translation.
GoogleTranslateXliff.exe /AK=YourGoogleTranslateV2ApiKey /F:myUiMessages.ja.xlf /TF:myUiMessagesTranslated.ja.xlf ---- from the source locale file to a new target file in Japanese
GoogleTranslateXliff.exe /AK=YourGoogleTranslateV2ApiKey /F:myUiMessages.xlf /TF:myUiMessages.es.xlf /TL=es ---- From the source template file to a new target file in Spanish.
GoogleTranslateXliff.exe /AV=v3 /CSF=client_secret.json /B /F:myUiMessages.es.xlf ---- Use Google Cloud Translate V3 and batch mode.

```


## GoogleTranslateResx.exe

```
GoogleTranslateResx.exe
Use Google Translate v2 or v3 to translate Microsoft ResX
Google Translate for Microsoft ResX  version 1.0
Fonlow (c) 2025


   /SourceFile, /F     Source file path, e.g., /F=AppResources.resx
   /TargetFile, /TF    Target file path. e.g., /TF=c:/AppResources.ja.resx
   /SourceLang, /SL    Source language. e.g., /SL=fr
   /TargetLang, /TL    Target language. e.g., /TL=zh
   /ApiKey, /AK        Google Translate API key. e.g., /AK=zasdfSDFSDfsdfdsfs234sdsfki
   /Batch, /B          Batch processing of strings to improve overall speed. V2 and V3 support.
   /ApiVersion, /AV    Google Translate API version. Default to V2. If V3, a client secret JSON file is expected.
   /ClientSecretFile,  Google Cloud Translate V3 does not support API key but rich ways of authentications. This app uses client
   /CSF                secret JSON file you could download from your Google Cloud Service account.
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

## Build and Deployment

This repository does not release binary builds. You may check-out the source codes of master or a latest tag like v1_stable, and then use respective PS1 scripts to build each CLI apps for Windows, MacOS or Linux.