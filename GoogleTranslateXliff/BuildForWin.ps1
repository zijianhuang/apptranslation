Set-Location $PSScriptRoot
$target="../Release/GoogleTranslateXliff_Win"

Remove-Item -Recurse $target

dotnet publish -r win-x64 --output $target --configuration release --self-contained false

