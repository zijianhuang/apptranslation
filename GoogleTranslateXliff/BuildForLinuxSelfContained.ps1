# after buildRelease of the sln
Set-Location $PSScriptRoot
$target="../Release/Linux_SelfContained"
$mainDir="bin/Release/net9.0/"

Remove-Item -Recurse $target*

dotnet publish -r linux-x64 --output $target --configuration release --self-contained true

