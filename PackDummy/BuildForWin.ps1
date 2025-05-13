Set-Location $PSScriptRoot
$target="../Release/All_Win"

Remove-Item -Recurse $target

dotnet publish -r win-x64 --output $target --configuration release --self-contained false

Remove-Item $target/PackDummy.*

 ."C:\Program Files\7-Zip\7z.exe" a -tzip $target/../All_Win.zip $target