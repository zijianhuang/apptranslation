Set-Location $PSScriptRoot
$langList = "de", "es", "fil", "fr", "hi", "id", "it", "ja", "ko", "ms", "pl", "pt", "ru", "th", "tr", "uk", "vi", "zh-Hans", "zh-Hant"

# Merge data elements of resx to XLIFF, and create XLIFF
$exe = "../../../XliffResXConverter/bin/Debug/net9.0/XliffResXConverter.exe"
$exeResxTranslate="C:/Green/GoogleTranslateResX/GoogleTranslateResX.exe"
foreach ($lang in $langList) {
    # AppResources.$lang.resx is presumed there, being created by IDE, while lang xliff file may not be there.
    $langXliff = "MultilingualResources/Fonlow.VA.Languages.$lang.xlf"
    if (Test-Path $langXliff) {
        $cmdMergeToXliff = "$exe /a=merge /RXS=AppResources.resx /RXL=AppResources.$lang.resx /XF=$langXliff"
        Invoke-Expression $ExecutionContext.InvokeCommand.ExpandString($cmdMergeToXliff)
    }
    else {
        $cmdNewToXliff = "$exe /a=new /SL=en /TL=$lang /RXS=AppResources.resx /RXL=AppResources.$lang.resx /XF=$langXliff"
        Invoke-Expression $ExecutionContext.InvokeCommand.ExpandString($cmdNewToXliff)
    }
}

# Translate all XLIFF files
$exeXliffTranslate = "C:/Green/GoogleTranslateXliff/GoogleTranslateXliff.exe"
foreach ($lang in $langList) {
    $langXliff = "MultilingualResources/Fonlow.VA.Languages.$lang.xlf"
    if (Test-Path $langXliff) {
        $cmdTranslate = "$exeXliffTranslate /AKF=C:/Users/Public/DevApps/GtApiKey.txt /F=$langXliff"
        Invoke-Expression $ExecutionContext.InvokeCommand.ExpandString($cmdTranslate)
    }
    else {
        <# Action when all if and elseif conditions are false #>
    }

}

# Merge back from XLIFF to ResX.
foreach ($lang in $langList) {
    $langXliff = "MultilingualResources/Fonlow.VA.Languages.$lang.xlf"
    
    if (Test-Path $langXliff) {
        $cmdMergeBack = "$exe /a=mergeBack /RXL=AppResources.$lang.resx /XF=$langXliff"
        Invoke-Expression $ExecutionContext.InvokeCommand.ExpandString($cmdMergeBack)
    }
    else {
        <# Action when all if and elseif conditions are false #>
    }

}