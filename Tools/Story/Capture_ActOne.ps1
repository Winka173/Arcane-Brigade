param([int]$BuildProcessId=0)
$ErrorActionPreference='Stop'
$repoPath=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if($BuildProcessId -gt 0){$buildProcess=Get-Process -Id $BuildProcessId -ErrorAction SilentlyContinue;if($buildProcess){$buildStarted=$buildProcess.StartTime;if(-not $buildProcess.WaitForExit(300000)){throw 'Act One build did not finish in time'};$buildProcess.Refresh();if($null -ne $buildProcess.ExitCode -and $buildProcess.ExitCode -ne 0){throw 'Act One Unity build failed'}}}
$buildFile=Join-Path $repoPath 'PreviewExports/ActOne/Build.txt'
if(-not (Test-Path -LiteralPath $buildFile) -or -not (Get-Content -LiteralPath $buildFile -Raw).Contains('Result: Succeeded')){throw 'Act One build is not ready'}
if($buildStarted -and (Get-Item -LiteralPath $buildFile).LastWriteTime -lt $buildStarted){throw 'Build report predates the requested build'}
$runs=@()
foreach($name in @('Native','Reduced')){
    $started=Get-Date
    $capturePath=Join-Path $repoPath ('PreviewExports/ActOne/'+$name)
    New-Item -ItemType Directory -Path $capturePath -Force | Out-Null
    $flags=if($name -eq 'Reduced'){'-miencho-reduced'}else{''}
    $args='-screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -force-d3d11 -arcane-miencho-capture -miencho-actone '+$flags+' -capturePath "'+$capturePath+'" -logFile "'+$repoPath+'\Library\ActOne-'+$name+'.log"'
    $process=Start-Process -FilePath (Join-Path $repoPath 'Builds/HeroIdentity/ArcaneBrigade.exe') -ArgumentList $args -WindowStyle Hidden -PassThru
    if(-not $process.WaitForExit(240000)){Stop-Process -Id $process.Id;throw ('Capture timed out: '+$name)}
    $process.Refresh();$checksPath=Join-Path $capturePath 'Native_Checks.txt'
    if($process.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $checksPath)){throw ('Act One capture failed: '+$name)}
    $checks=@(Get-Content -Encoding UTF8 -LiteralPath $checksPath)
    if((Get-Item -LiteralPath $checksPath).LastWriteTime -lt $started -or $checks[-1] -ne 'PASS Act One lantern, fog and walkable-room verification finished' -or @($checks | Where-Object{-not $_.StartsWith('PASS ')}).Count -gt 0){throw ('Act One verification incomplete: '+$name)}
    $runs+=@{Name=$name;Checks=$checks.Count;ExitCode=$process.ExitCode;Started=$started.ToString('o');Finished=(Get-Date).ToString('o')}
    $runs | ConvertTo-Json -Depth 3 | Set-Content -Encoding UTF8 -LiteralPath (Join-Path $repoPath 'PreviewExports/ActOne/Capture_Runs.json')
}
