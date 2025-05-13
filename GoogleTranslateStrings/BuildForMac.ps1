Set-Location $PSScriptRoot
$target="../Release/GoogleTranslateStrings_Mac"

Remove-Item -Recurse $target

dotnet publish -r osx-x64 --output $target --configuration release --self-contained false

