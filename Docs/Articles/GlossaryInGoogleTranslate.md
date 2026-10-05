# Glossary in Google Translate v3

There are many ways to prepare glossaries of various business domains for Google Translate API v3. This article is focused on using the TBX files of Microsoft Term Collection, for translating app UI.

## Prepare for TSV Files

After downloading the TBX files, put them to folder "MicrosoftTermCollection", and use the following script to transform the TBX files to TSV files that Google Translate v3 would accept.

**ConvertAllTbx.ps1:**
```ps1
param(
    [Parameter(Mandatory)] [string] $InputDir,
    [Parameter(Mandatory)] [string] $OutputDir,
    [string] $Src = "en",
    [switch] $Recurse,
    [switch] $Force      # overwrite existing TSV files (default: skip them)
)

$ErrorActionPreference = 'Stop'
$converter = Join-Path $PSScriptRoot 'Convert-TbxToTsv.ps1'
if (-not (Test-Path $converter)) { throw "Convert-TbxToTsv.ps1 not found next to this script." }

$srcShort = $Src.Split('-')[0].ToLower()
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

# Returns the first xml:lang (original casing) that isn't the source language.
function Get-TargetLang([string]$Path, [string]$SrcShort, [int]$MaxEntries = 200) {
    $settings = [System.Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [System.Xml.DtdProcessing]::Ignore
    $reader = [System.Xml.XmlReader]::Create($Path, $settings)
    try {
        $entries = 0
        while ($reader.Read()) {
            if ($reader.NodeType -ne [System.Xml.XmlNodeType]::Element) { continue }
            if ($reader.LocalName -eq 'termEntry') {
                if (++$entries -gt $MaxEntries) { break }
            }
            elseif ($reader.LocalName -eq 'langSet') {
                $lang = [string]$reader.GetAttribute('xml:lang')
                if ($lang -and $lang.Split('-')[0].ToLower() -ne $SrcShort) { return $lang }
            }
        }
        return $null
    }
    finally { $reader.Dispose() }
}

$files = Get-ChildItem -Path $InputDir -Filter *.tbx -File -Recurse:$Recurse
if (-not $files) { Write-Warning "No .tbx files found in $InputDir"; return }

$ok = 0; $skipped = 0; $failed = @()
$i = 0

foreach ($f in $files) {
    $i++
    Write-Progress -Activity "Converting TBX files" -Status $f.Name -PercentComplete (100 * $i / $files.Count)

    try {
        $tgt = Get-TargetLang -Path $f.FullName -SrcShort $srcShort
        if (-not $tgt) { throw "Could not detect a target language." }

        $outFile = Join-Path $OutputDir ("{0}_{1}_{2}.tsv" -f $f.BaseName, $srcShort, $tgt)

        if ((Test-Path $outFile) -and -not $Force) {
            Write-Host "Skipping $($f.Name): $outFile already exists (use -Force)." -ForegroundColor Yellow
            $skipped++
            continue
        }

        Write-Host "[$i/$($files.Count)] $($f.Name)  ($srcShort -> $tgt)"
        & $converter -Path $f.FullName -Out $outFile -Src $srcShort -Tgt $tgt
        $ok++
    }
    catch {
        Write-Warning "Failed on $($f.Name): $($_.Exception.Message)"
        $failed += $f.Name
    }
}

Write-Progress -Activity "Converting TBX files" -Completed
Write-Host "`nDone. Converted: $ok, skipped: $skipped, failed: $($failed.Count)"
if ($failed) { Write-Host "Failed files: $($failed -join ', ')" -ForegroundColor Red }
```

**Convert-TbxToTsv.ps1:**
```ps1
param(
    [Parameter(Mandatory)] [string] $Path,
    [Parameter(Mandatory)] [string] $Out,
    [string] $Src = "en",
    [Parameter(Mandatory)] [string] $Tgt
)

$Src = $Src.ToLower(); $Tgt = $Tgt.ToLower()

function Clean([string]$s) { ($s -replace '\s+', ' ').Trim() }

$settings = [System.Xml.XmlReaderSettings]::new()
$settings.DtdProcessing = [System.Xml.DtdProcessing]::Ignore

$reader = [System.Xml.XmlReader]::Create((Resolve-Path -LiteralPath $Path).Path, $settings)
$writer = [System.IO.StreamWriter]::new(
    [System.IO.Path]::GetFullPath($Out), $false, [System.Text.UTF8Encoding]::new($false))
$seen  = [System.Collections.Generic.HashSet[string]]::new()
$terms = @{}
$lang  = $null
$count = 0
$needRead = $true

try {
    while ($true) {
        if ($needRead) { if (-not $reader.Read()) { break } }
        $needRead = $true

        if ($reader.NodeType -eq [System.Xml.XmlNodeType]::Element) {
            $name = $reader.LocalName
            if ($name -eq 'termEntry') {
                $terms = @{}
                $lang = $null
            }
            elseif ($name -eq 'langSet') {
                $lang = ([string]$reader.GetAttribute('xml:lang')).ToLower()
            }
            elseif ($name -eq 'term' -and $lang) {
                # ReadElementContentAsString moves the reader past </term>,
                # so the next loop iteration must not call Read() again.
                $text = Clean $reader.ReadElementContentAsString()
                $needRead = $false
                if ($text) {
                    $short = $lang.Split('-')[0]
                    if (-not $terms.ContainsKey($lang))  { $terms[$lang]  = $text }
                    if (-not $terms.ContainsKey($short)) { $terms[$short] = $text }
                }
            }
        }
        elseif ($reader.NodeType -eq [System.Xml.XmlNodeType]::EndElement) {
            $name = $reader.LocalName
            if ($name -eq 'langSet') {
                $lang = $null
            }
            elseif ($name -eq 'termEntry') {
                if ($terms.ContainsKey($Src) -and $terms.ContainsKey($Tgt)) {
                    $line = "$($terms[$Src])`t$($terms[$Tgt])"
                    if ($seen.Add($line)) { $writer.WriteLine($line); $count++ }
                }
            }
        }
    }
}
finally {
    $writer.Dispose(); $reader.Dispose()
}

if ($count -eq 0) { Write-Warning "No term pairs written for $Path (Src=$Src, Tgt=$Tgt)." }
"Wrote $count term pairs to $Out"
```

**Remarks:**
* Both scripts are crafted by Claude.

Run script:
```
./ConvertAllTbx.ps1 C:\someFolder\MicrosoftTermCollection C:\someFolder\MicrosoftTermCollectionTsv
```

## Upload TSV Files to Google Cloud Buckets

In your Web UI API Project in https://console.cloud.google.com/apis/api/translate.googleapis.com/metrics?project=api-project-123456789, click the Cloud Shell button on the top right corner. And in the CloudShell, you can open editor to add or modify scripts.

There are many ways to upload, and here I would just use The Cloud Storage UI to upload: https://console.cloud.google.com/storage/browser and create a bucket like my_glossaries, then drag all the TSV files to the bucket.

### Clean up the TSV files.

The glossary API requires every source term to appear only once. My Dutch file has 13,615 entries (about 29%) where the same English term maps to several Dutch terms, because Microsoft lists different translations for different products and contexts. Google rejected the file, so nothing will be created. The fix is to reduce each source term to a single translation before import.

The script below does that cleaning automatically for each file:

1. It downloads the TSV from the bucket.
2. It keeps one translation per source term.
3. It uploads the cleaned file to gs://tt_glossaries/cleaned/, so your originals aren't touched.
4. It creates the glossary from the cleaned file.

**import_glossaries.py**
Replace PROJECT and BUCKET for your own usage. Use the editor of Cloud Shell to add and modify.
```py
import os
import re
import shutil
import subprocess
import sys
import tempfile

import google.oauth2.credentials
from google.api_core.exceptions import NotFound
from google.cloud import translate_v3 as translate

PROJECT  = "api-project-1234567890"
LOCATION = "us-central1"
BUCKET   = "my_glossaries"
SRC      = "en"
PREFIX   = "mstc"
ONLY     = sys.argv[1].lower() if len(sys.argv) > 1 else ""   # optional filename filter

# How to resolve a source term that has several translations:
#   "first"    keep the first one listed in the file
#   "shortest" keep the shortest translation
#   "longest"  keep the longest translation
PICK = "first"

# True: "Open" and "open" count as the same source term (only one is kept).
# Leave False unless Google still reports conflicts after cleaning.
CASE_INSENSITIVE = False

parent = f"projects/{PROJECT}/locations/{LOCATION}"


def make_client():
    """Create a client using gcloud's current access token (fresh token each time)."""
    token = subprocess.run(
        ["gcloud", "auth", "print-access-token"],
        capture_output=True, text=True, check=True,
    ).stdout.strip()
    return translate.TranslationServiceClient(
        credentials=google.oauth2.credentials.Credentials(token),
        client_options={"quota_project_id": PROJECT},
    )


def pick(cands):
    if PICK == "shortest":
        return min(cands, key=len)
    if PICK == "longest":
        return max(cands, key=len)
    return cands[0]


def clean_tsv(src_path, dst_path):
    """Keep exactly one target per source term. Returns (rows_read, rows_kept)."""
    groups = {}   # key -> [original source text, [candidate targets]]
    total = 0
    with open(src_path, encoding="utf-8") as f:
        for line in f:
            parts = line.rstrip("\r\n").split("\t")
            if len(parts) != 2:
                continue
            s, t = parts[0].strip(), parts[1].strip()
            if not s or not t:
                continue
            total += 1
            key = s.lower() if CASE_INSENSITIVE else s
            if key not in groups:
                groups[key] = [s, []]
            if t not in groups[key][1]:
                groups[key][1].append(t)
    with open(dst_path, "w", encoding="utf-8", newline="\n") as out:
        for s, cands in groups.values():
            out.write(f"{s}\t{pick(cands)}\n")
    return total, len(groups)


def gcloud(*args):
    subprocess.run(["gcloud", *args, "--quiet"], check=True)


# Language codes Google accepts as a translation target
client = make_client()
resp = client.get_supported_languages(
    parent=parent, display_language_code="en", timeout=60
)
targets = {
    l.language_code.lower(): l.language_code
    for l in resp.languages if l.support_target
}
OVERRIDES = {"zh-hans": "zh-CN", "zh-hant": "zh-TW", "zh-hk": "zh-TW"}


def resolve(code):
    c = code.lower()
    for cand in (OVERRIDES.get(c), c, c.split("-")[0]):
        if cand and cand.lower() in targets:
            return targets[cand.lower()]
    return None


out = subprocess.run(
    ["gcloud", "storage", "ls", f"gs://{BUCKET}/*.tsv"],
    capture_output=True, text=True, check=True,
).stdout
uris = [l.strip() for l in out.splitlines() if l.strip().endswith(".tsv")]
print(f"Found {len(uris)} TSV file(s) in gs://{BUCKET}")

ok, skipped, failed = 0, 0, []

for uri in uris:
    fname = uri.rsplit("/", 1)[1]
    if ONLY and ONLY not in fname.lower():
        continue
    tmp = tempfile.mkdtemp()
    try:
        m = re.match(r"^(.+)_([^_]+)_([^_]+)\.tsv$", fname)
        if not m:
            raise ValueError("file name doesn't match <Name>_<src>_<locale>.tsv")
        name, _, tgt_raw = m.groups()
        tgt = resolve(tgt_raw)
        if not tgt:
            raise ValueError(f"no Google target language matches '{tgt_raw}'")

        gid = re.sub(r"[^a-z0-9]+", "-", f"{PREFIX}-{name}-{SRC}-{tgt_raw}".lower()).strip("-")

        client = make_client()   # fresh token for every file
        glossary_name = client.glossary_path(PROJECT, LOCATION, gid)

        try:
            client.get_glossary(name=glossary_name, timeout=60)
            print(f"SKIP   {gid} (already exists)")
            skipped += 1
            continue
        except NotFound:
            pass

        # Download, clean, upload the cleaned copy to gs://BUCKET/cleaned/
        safe = re.sub(r"[^A-Za-z0-9._-]+", "_", fname)
        raw_path = os.path.join(tmp, "raw_" + safe)
        clean_path = os.path.join(tmp, safe)
        gcloud("storage", "cp", uri, raw_path)
        total, kept = clean_tsv(raw_path, clean_path)
        print(f"CLEAN  {fname}: {total} rows -> {kept} unique source terms "
              f"({total - kept} conflicting rows dropped)")
        clean_uri = f"gs://{BUCKET}/cleaned/{safe}"
        gcloud("storage", "cp", clean_path, clean_uri)

        print(f"CREATE {gid}  ({SRC} -> {tgt})  from {clean_uri}  (this can take a few minutes)")
        glossary = translate.Glossary(
            name=glossary_name,
            language_pair=translate.Glossary.LanguageCodePair(
                source_language_code=SRC, target_language_code=tgt
            ),
            input_config=translate.GlossaryInputConfig(
                gcs_source=translate.GcsSource(input_uri=clean_uri)
            ),
        )
        client.create_glossary(parent=parent, glossary=glossary, timeout=120).result(timeout=1800)
        print(f"DONE   {gid}")
        ok += 1
    except Exception as e:
        print(f"FAIL   {fname}: {e}")
        failed.append(fname)
    finally:
        shutil.rmtree(tmp, ignore_errors=True)

print(f"\nCreated: {ok}, skipped: {skipped}, failed: {len(failed)}")
if failed:
    print("Failed files:", ", ".join(failed))

print("\nGlossaries in project:")
client = make_client()
for g in client.list_glossaries(parent=parent, timeout=60):
    print(" ", g.name.split("/")[-1],
          g.language_pair.source_language_code, "->",
          g.language_pair.target_language_code,
          "entries:", g.entry_count)
```


# Similar GUI-oriented terminology and translation sources

Here are the links. I'm giving them from memory rather than checking each one, so a few pages may have moved.

## Terminology and translation data
- OPUS (TMX/Moses downloads of GNOME, KDE, Ubuntu, Mozilla and other software UI corpora): https://opus.nlpl.eu/ . Huge.
- Apple localization glossaries: available through the Apple Developer downloads area (https://developer.apple.com/download/all/?q=glossaries), which requires an Apple ID and has restrictive licensing. A DMG file can be extracted using 7zip, getting a set of LG files.

These files are basically the translation files for each app, and the items may contains placeholders. The entries may not be good enough to be used as a glossary resource by Google Translation API directly without some cherry picking process.

## Reference and tooling
- Google Cloud Translation glossary docs (formats and limits): https://cloud.google.com/translate/docs/advanced/glossary
- Microsoft Terminology (the source you mentioned, with its license terms): https://learn.microsoft.com/en-us/globalization/reference/microsoft-terminology
- Translate Toolkit (`po2csv`, `po2tmx` and other converters): https://toolkit.translatehouse.org/

