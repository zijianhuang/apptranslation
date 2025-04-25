# In addition to what you have done with "Resource Explorer" of Visual Studio, this script supports such workflow:
# 1. Use XLIFF files as translation memory, and maintain the states of translation units.
# 2. Merge changes in ResX files (original and lang) to XLIFF.
# 3. Merge changes in XLIFF back to language ResX files.
# This PS1 script is provided as an example and you should alters some variables for your own need.
# Typically you could alter $langList, $exeConverter, $exeXliffTranslate, $langXliff, GroupId and ApiKey parameters.
# To compatible with ResX Resource Manager, GroupId (GID) must be defined so XLIFF will use file/body/group to contain translation units.

Set-Location $PSScriptRoot
$langList = "ar", "de", "es", "fil", "fr", "hi", "id", "it", "ja", "ko", "ms", "pl", "pt", "ru", "th", "tr", "uk", "vi", "zh-Hans", "zh-Hant"

# Merge data elements of resx to XLIFF, and create XLIFF
$exeConverter = "../../../XliffResXConverter/bin/Debug/net9.0/XliffResXConverter.exe"
foreach ($lang in $langList) {
    $langResx="AppResources.$lang.resx"
    if (!(Test-Path $langResx)){
        Write-Warning "$langResx not exist"
        continue
    }
    $langXliff = "MultilingualResources/Fonlow.VA.Languages.$lang.xlf"
    if (Test-Path $langXliff) {
        $cmdMergeToXliff = "$exeConverter /a=merge /RXS=AppResources.resx /RXL=$langResx /XF=$langXliff"
        Invoke-Expression $ExecutionContext.InvokeCommand.ExpandString($cmdMergeToXliff)
    }
    else {
        $cmdNewToXliff = "$exeConverter /a=new /SL=en /TL=$lang /GID=APPRESOURCES.RESX /RXS=AppResources.resx /RXL=AppResources.$lang.resx /XF=$langXliff"
        Invoke-Expression $ExecutionContext.InvokeCommand.ExpandString($cmdNewToXliff)
    }
}

# Translate all XLIFF files
$exeXliffTranslate = "C:/Green/GoogleTranslateXliff/GoogleTranslateXliff.exe"
foreach ($lang in $langList) {
    $langXliff = "MultilingualResources/Fonlow.VA.Languages.$lang.xlf"
    if (Test-Path $langXliff) {
        $cmdTranslate = "$exeXliffTranslate /AKF=C:/Users/Public/DevApps/GtApiKey.txt /B /F=$langXliff"
        Invoke-Expression $ExecutionContext.InvokeCommand.ExpandString($cmdTranslate)
    }
    else {
        Write-Warning "Expect $langXliff but not found."
    }

}

# Merge back from XLIFF to ResX.
foreach ($lang in $langList) {
    $langXliff = "MultilingualResources/Fonlow.VA.Languages.$lang.xlf"
    
    if (Test-Path $langXliff) {
        $cmdMergeBack = "$exeConverter /a=mergeBack /RXL=AppResources.$lang.resx /XF=$langXliff"
        Invoke-Expression $ExecutionContext.InvokeCommand.ExpandString($cmdMergeBack)
    }
    else {
        Write-Warning "Expect $langXliff but not found."
    }

}