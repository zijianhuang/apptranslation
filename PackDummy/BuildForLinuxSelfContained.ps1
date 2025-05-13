Set-Location $PSScriptRoot
$target="../Release/All_LinuxSelfContained"

Remove-Item -Recurse $target

dotnet publish -r linux-x64 --output $target --configuration release --self-contained true

Remove-Item $target/PackDummy.*

 ."C:\Program Files\7-Zip\7z.exe" a -tzip $target/../All_LinuxSelfContained.zip $target