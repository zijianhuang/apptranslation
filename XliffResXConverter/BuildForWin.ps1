# after buildRelease of the sln
Set-Location $PSScriptRoot
$target="../Release/XliffResXConverter_Win"

Remove-Item -Recurse $target*

dotnet publish -r win-x64 --output $target --configuration release --self-contained false

