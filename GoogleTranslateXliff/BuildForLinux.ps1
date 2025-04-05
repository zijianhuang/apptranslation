# after buildRelease of the sln
Set-Location $PSScriptRoot
$target="../Release/Linux"
$mainDir="bin/Release/net9.0/"

Remove-Item -Recurse $target*

dotnet publish -r linux-x64 --output $target --configuration release --self-contained false

