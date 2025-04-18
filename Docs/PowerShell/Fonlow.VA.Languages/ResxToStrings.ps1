# Transform RESX to Google translation file.
# resx file that contain one language, generally AppResources.resx; and strings.xml that will be accepted by Google Play Console translators.
param ($xml, $output)
# based on https://devio.wordpress.com/2009/09/15/command-line-xslt-processor-with-powershell/
# however, XslCompiledTransform want absolute path.
Set-Location $PSScriptRoot

if (-not $xml -or -not $output)
{
	Write-Host "& .\xslt.ps1 [-xml] xml-input [-output] transform-output"
	exit;
}

trap [Exception]
{
	Write-Host $_.Exception;
}

$currentDir = Get-Location;
$xsl="ResxToStrings.xslt";

$xslt = New-Object System.Xml.Xsl.XslCompiledTransform;
$xslt.Load([System.IO.Path]::Combine($currentDir, $xsl));
$xslt.Transform([System.IO.Path]::Combine($currentDir, $xml), [System.IO.Path]::Combine($currentDir, $output));

Write-Host "generated" $output;