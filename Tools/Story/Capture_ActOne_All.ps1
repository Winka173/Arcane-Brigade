param([int]$BuildProcessId=0)
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'Capture_ActOne.ps1') -BuildProcessId $BuildProcessId
if($LASTEXITCODE -and $LASTEXITCODE -ne 0){throw 'Act One captures failed'}
& (Join-Path $PSScriptRoot 'Capture_MienCho.ps1')
