$PSNativeCommandArgumentPassing = 'Legacy'
Write-Host "Setting: $PSNativeCommandArgumentPassing"
./bin/Debug/net10.0/GoogleTranslateXml.exe --% /AKF=../../Secrets/GoogleTranslate/apikey.txt /SL=en /TL="zh-hant" /XPaths=`//svg:text/svg:tspan` /F=../Tests/TestTranslation/svg/template1.svg /TF=../Tests/TestTranslation/bin/template1.zh-Hant.svg