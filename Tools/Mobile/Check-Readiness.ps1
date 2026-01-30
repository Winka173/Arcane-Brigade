param([string]$Unity = 'C:/Program Files/Unity/Hub/Editor/6000.4.8f1/Editor/Unity.exe', [string]$Output = 'PreviewExports/FinalQuality/Mobile_Readiness.json')
$ErrorActionPreference = 'Stop'
$project = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$engines = Join-Path (Split-Path $Unity) 'Data/PlaybackEngines'
$adb = Get-Command adb -ErrorAction SilentlyContinue
$devices = @()
if ($null -ne $adb) { $devices = @(& $adb.Source devices -l 2>&1 | ForEach-Object { "$_" }) }
$onlinePhysical = @($devices | Where-Object { $_ -match '^\S+\s+device\s' -and $_ -notmatch '^emulator-' })
$isMacHost = [Environment]::OSVersion.Platform -eq [PlatformID]::Unix -and (Test-Path '/Applications/Xcode.app')
$report = [pscustomobject]@{
    Host = [Environment]::OSVersion.VersionString
    Unity = $Unity
    AndroidModule = Test-Path (Join-Path $engines 'AndroidPlayer')
    IosModule = Test-Path (Join-Path $engines 'iOSSupport')
    Adb = $(if ($null -ne $adb) { $adb.Source } else { $null })
    Devices = $devices
    OnlinePhysicalAndroid = $onlinePhysical.Count
    MacWithXcode = [bool]$isMacHost
    AndroidPhysicalGatePassed = $false
    IosPhysicalGatePassed = $false
    Note = 'Readiness inspection only, not a build or device playtest. Offline/emulator devices are not physical acceptance. Do not publish credentials or signing files in reports.'
}
$destination = Join-Path $project $Output
New-Item -ItemType Directory -Force -Path (Split-Path $destination) | Out-Null
$report | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $destination -Encoding UTF8
$report | Format-List
