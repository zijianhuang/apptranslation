# after buildRelease of the sln
Set-Location $PSScriptRoot
$target="../Release/WinStrings_MacSelfContained"
$mainDir="bin/Release/net9.0/"

Remove-Item -Recurse $target*

dotnet publish -r osx-x64 --output $target --configuration release --self-contained true

