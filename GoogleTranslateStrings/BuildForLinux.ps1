Set-Location $PSScriptRoot
$target="../Release/GoogleTranslateStrings_Linux"

Remove-Item -Recurse $target*

dotnet publish -r linux-x64 --output $target --configuration release --self-contained false

