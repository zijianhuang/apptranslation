Set-Location $PSScriptRoot
$langList =  "de", "es", "fil", "fr", "hi", "id", "it", "ja", "ko", "ms", "pl", "pt", "ru", "th", "tr", "vi", "zh-Hans", "zh-Hant"
$exe = "../../../XliffResXConverter/bin/Debug/net9.0/XliffResXConverter.exe"
foreach ($lang in $langList) {
    $cmdMergeToXliff = "$exe /a=merge /RXS=AppResources.resx /RXL=AppResources.$lang.resx /XF=MultilingualResources/Fonlow.VA.Languages.$lang.xlf"
    Invoke-Expression $ExecutionContext.InvokeCommand.ExpandString($cmdMergeToXliff)
}

$exeTranslate="C:/Green/GoogleTranslateXliff/GoogleTranslateXliff.exe"
foreach ($lang in $langList) {
    $cmdTranslate = "$exeTranslate /AKF=C:/Users/Public/DevApps/GtApiKey.txt /F=MultilingualResources/Fonlow.VA.Languages.$lang.xlf"
    Invoke-Expression $ExecutionContext.InvokeCommand.ExpandString($cmdTranslate)
}

foreach ($lang in $langList) {
    $cmdMergeBack = "$exe /a=mergeBack /RXL=AppResources.$lang.resx /XF=MultilingualResources/Fonlow.VA.Languages.$lang.xlf"
    Invoke-Expression $ExecutionContext.InvokeCommand.ExpandString($cmdMergeBack)
}