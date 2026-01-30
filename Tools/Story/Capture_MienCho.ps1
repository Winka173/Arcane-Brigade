param([switch]$SkipEncounterNative)
$ErrorActionPreference = 'Stop'
$repoPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$playerPath = Join-Path $repoPath 'Builds/HeroIdentity/ArcaneBrigade.exe'
$cases = @(
    @{ Name = 'Encounters/Native'; Flags = '-miencho-encounters'; Log = 'EncountersNativeFinal' },
    @{ Name = 'Encounters/Reduced'; Flags = '-miencho-encounters -miencho-reduced'; Log = 'EncountersReducedFinal' },
    @{ Name = 'Native'; Flags = ''; Log = 'StoryNativeFinal' },
    @{ Name = 'Reduced'; Flags = '-miencho-reduced'; Log = 'StoryReducedFinal' }
)
$results = @()
foreach ($case in $cases) {
    if ($SkipEncounterNative -and $case.Name -eq 'Encounters/Native') { continue }
    $capturePath = Join-Path $repoPath ('PreviewExports/MienCho/' + $case.Name)
    New-Item -ItemType Directory -Path $capturePath -Force | Out-Null
    $logPath = Join-Path $repoPath ('Library/MienCho-' + $case.Log + '.log')
    $started = Get-Date
    $playerArgs = '-screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -force-d3d11 -arcane-miencho-capture ' + $case.Flags + ' -capturePath "' + $capturePath + '" -logFile "' + $logPath + '"'
    $process = Start-Process -FilePath $playerPath -ArgumentList $playerArgs -WindowStyle Hidden -PassThru
    if (-not $process.WaitForExit(180000)) {
        Stop-Process -Id $process.Id
        throw ('Capture timed out: ' + $case.Name)
    }
    $process.Refresh()
    $checkPath = Join-Path $capturePath 'Native_Checks.txt'
    if ($process.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $checkPath)) { throw ('Player capture failed: ' + $case.Name) }
    if ((Get-Item -LiteralPath $checkPath).LastWriteTime -lt $started) { throw ('Stale checks: ' + $case.Name) }
    $checks = @(Get-Content -LiteralPath $checkPath -Encoding UTF8)
    $expected = if ($case.Flags.Contains('-miencho-encounters')) { 'PASS Waiting shop encounters and reading review finished' } else { 'PASS Waiting Realm native verification finished' }
    if ($checks[-1] -ne $expected -or @($checks | Where-Object { -not $_.StartsWith('PASS ') }).Count -gt 0) { throw ('Incomplete checks: ' + $case.Name) }
    $results += @{ Name = $case.Name; Checks = $checks.Count; ExitCode = $process.ExitCode; Started = $started.ToString('o'); Finished = (Get-Date).ToString('o') }
    $results | ConvertTo-Json -Depth 3 | Set-Content -LiteralPath (Join-Path $repoPath 'PreviewExports/MienCho/Capture_Runs.json') -Encoding UTF8
}
