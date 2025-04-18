# After running `ng extract-i18n` to update xlf files, run this to translate those of state "new" (Xliff 1.2) or "initial" (Xliff 2.0)
# This script statys in the same folder with xlf files.
# You are welcome to write a more generic script that works on any number of xlf files, and the script could stay anywhere.
$commandPath='Somewhere/GoogleTranslateXliff.exe'
$apiKey='MyGoogleTranslateV2ApiKey'

foreach ($lang in 'es', 'fr', 'hi', 'ja', 'pt', 'th', 'zh-hans', 'zh-hant'){
    $cmd='$commandPath /AK=$apiKey /B /F=messages.$lang.xlf'
    Invoke-Expression $ExecutionContext.InvokeCommand.ExpandString($cmd)
}