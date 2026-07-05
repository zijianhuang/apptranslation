$PSNativeCommandArgumentPassing = 'Legacy'
Write-Host "Setting: $PSNativeCommandArgumentPassing"
$MsTranslatorApiKey = $env:MsTranslatorApiKey
$Region = $env:MsTranslatorRegion
./bin/Debug/net10.0/MsTranslatorXml.exe /AK=$MsTranslatorApiKey /RG=$Region --% /SL=en /TL="zh-hant" /XPaths=`//svg:text/svg:tspan` /B /F=../Tests/TestTranslation/svg/template1.svg /TF=../Tests/TestTranslation/bin/template1.zh-Hant.svg