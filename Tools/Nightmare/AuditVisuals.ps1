param([string]$Root = 'PreviewExports/Nightmare/Lighting', [string]$Completion = 'PASS Results revealed after complete ending')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$project = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$destination = Join-Path $project $Root
$profiles = @()
foreach ($mode in @('Final', 'Final-Reduced')) {
    $folder = Join-Path $destination $mode
    $checks = Get-Content (Join-Path $folder 'Native_Checks.txt')
    $frames = @()
    foreach ($file in Get-ChildItem -LiteralPath $folder -Filter '*.png') {
        $bitmap = [System.Drawing.Bitmap]::new($file.FullName)
        try {
            if ($bitmap.Width -ne 1920 -or $bitmap.Height -ne 1080) { throw "Wrong native dimensions: $($file.Name)" }
            $frames += [pscustomobject]@{ Name = $file.Name; Width = $bitmap.Width; Height = $bitmap.Height; SHA256 = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash }
        } finally { $bitmap.Dispose() }
    }
    if ($checks -match '^FAIL ') { throw "Native checks failed: $mode" }
    if (-not ($checks -contains $Completion)) { throw "Incomplete native run: $mode" }
    $profiles += [pscustomobject]@{ Mode = $mode; Passed = @($checks | Select-String '^PASS ').Count; Failed = @($checks | Select-String '^FAIL ').Count; Frames = $frames }
}
$report = [pscustomobject]@{
    AssemblySHA256 = (Get-FileHash -LiteralPath (Join-Path $project 'Builds/FullRun/ArcaneBrigade_Data/Managed/Assembly-CSharp.dll') -Algorithm SHA256).Hash
    PlayerFiles = @(Get-ChildItem -LiteralPath (Join-Path $project 'Builds/FullRun') -File -Recurse | Where-Object { $_.Name -ne 'BUILD_INFO.txt' } | ForEach-Object {
        [pscustomobject]@{ Path = $_.FullName.Substring($project.Length + 1); Bytes = $_.Length; SHA256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }
    })
    Profiles = $profiles
}
$report | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $destination 'Capture_Audit.json') -Encoding UTF8
$profiles | Select-Object Mode, Passed, Failed, @{ Name = 'Screenshots'; Expression = { $_.Frames.Count } } | Format-Table -AutoSize
