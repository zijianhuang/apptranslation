Set-Location $PSScriptRoot
$target="../Release/GoogleTranslateXliff_LinuxSelfContained"

Remove-Item -Recurse $target

dotnet publish -r linux-x64 --output $target --configuration release --self-contained true

