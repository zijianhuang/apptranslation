# Background
While GoogleTranslateResX.exe and similar apps provide translation of ResX file using Google Translate, however, due to ResX does not have state management, thus ResX alone is not good enough for ongoing update of software programs and for the sake of Contineous Integration.

Utilizing [Translation Memories](https://learn.microsoft.com/en-us/globalization/localization/translation-memories) is the solution. There are 3 common formats: TMX, TBX and XLIFF. XLIFF is chosen since XLIFF is apparently more popular among full-stack software developers.

Visual Studio's Resource Explorer has provided synchronization among source resx file and language resx files. 

And [ResX Resource Manager](https://marketplace.visualstudio.com/items?itemName=TomEnglert.ResXManager) provides some [complementary features](https://github.com/dotnet/ResXResourceManager/tree/master/Documentation), including [synchronisation between ResX and Xliff resource files](https://github.com/dotnet/ResXResourceManager/blob/master/Documentation/Topics/Xliff.md) .

XliffResXConverter.exe together with GoogleTranslateXliff.exe along with some Powershell scripts could be more efficient for Contineous Integration with the least User Interaction comparing what offered by ResX Resource Manager.


# Overview

## Initial

After creating AppResources.resx and a few AppResources.MyLang.resx files using Resource Explorer or ResX Resource Manager,

1. Convert all AppResources.MyLang.resx files into XLIFF files stored in a folder like "TranslationMemory".
1. Translate all XLIFF files using Google Translate through GoogleTranslateXliff.exe .
1. Copy translated content from XLIFF to ResX.


## Contineous Integration


## XliffResXConverter.exe

```
XliffResXConverter.exe
Convert and merge between ResX and XLIFF 1.2, and utilize XLIFF as translation memory.
ResX XLIFF converter  version 1.0.0.0
Copyright c Zijian Huang 2018-2025


   /Action, /a         Action could be new, merge, and MergeBack, e.g., /a=mergeback. New is to create XLIFF file
                       based on source ResX and language ResX; merge is to merge to change of ResX to existing XLIFF,
                       and MergeBack is to merge the translated XLIFF back to the language ResX.
   /SourceResX, /RXS   Source ResX file path, e.g., /F=AppResources.resx
   /LangResX, /RXL     Language ResX file path. e.g., /TF=c:/AppResources.ja.resx
   /XliffFile, /XF     Language XLIFF file path. e.g., /TF=c:/AppResources.ja.xlf
   /SourceLang, /SL    Source language. e.g., /SL=en
   /TargetLang, /TL    Target language. e.g., /TL=ja
   /Help, /h, /?       Shows this help text



Examples:
XliffResXConverter.exe /A=new /SL=en /TL=zh-hant /RXS:AppResources.resx /RXL=AppResources.zh-hant.resx /XF=c:/TranslationMemory/AppResources.zh-hant.xlf ---- For creating translation memory.
XliffResXConverter.exe /a=merge /RXS:AppResources.resx /RXL=AppResources.zh-hant.resx /XF=c:/TranslationMemory/AppResources.zh-hant.xlf ---- For updating translation memory with 2 ResX files.
XliffResXConverter.exe /A=MergeBack /RXL=AppResources.zh-hant.resx /XF=c:/TranslationMemory/AppResources.zh-hant.xlf ---- After translating XLIFF, merge the translated content back to language ResX file.
```


