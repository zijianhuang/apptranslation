$PSNativeCommandArgumentPassing = 'Legacy'
Write-Host "Setting: $PSNativeCommandArgumentPassing"
$GTV3KeyFile = $env:GoogleTranslateV3ClientSecretJsonFileForTest
./bin/Debug/net10.0/GoogleTranslateXml.exe /CSF=$GTV3KeyFile /AV=V3 --% /SL=en /TL="zh-hant" /XPaths=`//svg:text/svg:tspan` /B /F=../Tests/TestTranslation/svg/template1.svg /TF=../Tests/TestTranslation/bin/template1.zh-Hant.svg