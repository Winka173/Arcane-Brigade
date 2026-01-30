param(
    [string]$Player = 'Builds/FullRun',
    [string]$Archive = 'Builds/ArcaneBrigade-Windows-FinalQuality-01.zip',
    [string]$Report = 'PreviewExports/FinalQuality/Package_Verification.json'
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$project = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$source = [System.IO.Path]::GetFullPath((Join-Path $project $Player))
$destination = [System.IO.Path]::GetFullPath((Join-Path $project $Archive))
if (Test-Path -LiteralPath $destination) { throw "Archive already exists: $destination" }
if (-not (Test-Path -LiteralPath (Join-Path $source 'ArcaneBrigade.exe'))) { throw 'Missing Windows executable' }
if (-not (Test-Path -LiteralPath (Join-Path $source 'BUILD_INFO.txt'))) { throw 'Missing build identity' }
$files = @(Get-ChildItem -LiteralPath $source -File -Recurse)
$manifest = @{}
foreach ($file in $files) {
    $relative = $file.FullName.Substring($source.Length + 1).Replace('\', '/')
    $manifest.Add($relative, [pscustomobject]@{ Path = $relative; Bytes = $file.Length; SHA256 = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash })
}
[System.IO.Compression.ZipFile]::CreateFromDirectory($source, $destination, [System.IO.Compression.CompressionLevel]::Optimal, $false)
$zip = [System.IO.Compression.ZipFile]::OpenRead($destination)
$algorithm = [System.Security.Cryptography.SHA256]::Create()
$seen = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
try {
    foreach ($entry in $zip.Entries) {
        if ($entry.FullName.EndsWith('/')) { continue }
        $relative = $entry.FullName.Replace('\', '/')
        if (-not $seen.Add($relative) -or -not $manifest.ContainsKey($relative)) { throw "Unexpected or duplicate entry: $relative" }
        $expected = $manifest[$relative]
        $stream = $entry.Open()
        try { $digest = [BitConverter]::ToString($algorithm.ComputeHash($stream)).Replace('-', '') }
        finally { $stream.Dispose() }
        if ($entry.Length -ne $expected.Bytes -or $digest -ne $expected.SHA256) { throw "Archive content mismatch: $relative" }
    }
    if ($seen.Count -ne $files.Count) { throw 'Archive is missing player files' }
    $current = @(Get-ChildItem -LiteralPath $source -File -Recurse)
    if ($current.Count -ne $files.Count) { throw 'Player file set changed during packaging' }
    foreach ($file in $current) {
        $relative = $file.FullName.Substring($source.Length + 1).Replace('\', '/')
        if (-not $manifest.ContainsKey($relative) -or (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash -ne $manifest[$relative].SHA256) { throw "Player changed during packaging: $relative" }
    }
} finally { $algorithm.Dispose(); $zip.Dispose() }
$result = [pscustomobject]@{
    VerifiedUtc = [DateTime]::UtcNow.ToString('o')
    Archive = $Archive
    Bytes = (Get-Item -LiteralPath $destination).Length
    SHA256 = (Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash
    FilesVerified = $seen.Count
    StreamDecompressedEveryFile = $true
    PlayerUnchangedDuringPackaging = $true
    Files = @($manifest.Values | Sort-Object Path)
}
$reportPath = Join-Path $project $Report
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $reportPath) | Out-Null
$result | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $reportPath -Encoding UTF8
$result | Select-Object Archive, Bytes, SHA256, FilesVerified | Format-List
