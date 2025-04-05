# after buildRelease of the sln
Set-Location $PSScriptRoot
$target="../Release/WinStrings"
$mainDir="bin/Release/net9.0/"

Remove-Item -Recurse $target*

dotnet publish -r win-x64 --output $target --configuration release --self-contained false

