param(
    [string]$Label = ('Release-' + (Get-Date -Format 'yyyyMMdd-HHmmss')),
    [string]$Difficulties = '0,1,2,3,4',
    [string]$Seeds = '271026',
    [string]$Policies = 'Hybrid',
    [string]$Heroes = '',
    [ValidateSet('required', 'none', 'full')][string]$Meta = 'required',
    [switch]$SkipDps
)
$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$unityVersion = (Select-String -LiteralPath (Join-Path $projectRoot 'ProjectSettings/ProjectVersion.txt') -Pattern '^m_EditorVersion: (.+)$').Matches.Groups[1].Value
$unityEditor = "C:/Program Files/Unity/Hub/Editor/$unityVersion/Editor/Unity.exe"
if ($Label -notmatch '^[A-Za-z0-9_-]+$') { throw 'Use a simple, unique label.' }
if ($Difficulties -notmatch '^[0-7](,[0-7])*$' -or $Seeds -notmatch '^\d+(,\d+)*$' -or $Policies -notmatch '^(Buff|Recruit|Hybrid)(,(Buff|Recruit|Hybrid))*$' -or $Heroes -notmatch '^[A-Za-z0-9,]*$') { throw 'Invalid matrix arguments.' }
New-Item -ItemType Directory -Force (Join-Path $projectRoot 'Logs') | Out-Null
function Invoke-Audit([string]$Method, [string]$Extra, [string]$Log) {
    $auditArgs = "-batchmode -nographics -quit -projectPath `"$projectRoot`" -executeMethod $Method $Extra -logFile `"Logs/$Log.log`""
    $auditProcess = Start-Process -FilePath $unityEditor -ArgumentList $auditArgs -WorkingDirectory $projectRoot -WindowStyle Hidden -PassThru
    # Wait for the Editor itself; -Wait also waits for its persistent licensing child.
    $auditProcess.WaitForExit()
    if ($auditProcess.ExitCode -ne 0) { throw "$Method failed ($($auditProcess.ExitCode)). Read Logs/$Log.log" }
}
Push-Location $projectRoot
try {
    Invoke-Audit 'ArcaneBrigade.Editor.ReleaseCombatChecks.Run' '' "$Label-Combat"
    if (!$SkipDps) { Invoke-Audit 'ArcaneBrigade.Editor.HeroDpsRangeAudit.Final' "-audit-label $Label" "$Label-Dps" }
    $matrix = "-release-label $Label -release-difficulties $Difficulties -release-seeds $Seeds -release-policies $Policies -release-meta $($Meta.ToLowerInvariant())"
    if ($Heroes) { $matrix += " -release-heroes $Heroes" }
    Invoke-Audit 'ArcaneBrigade.Editor.ReleaseBalanceAudit.Run' $matrix "$Label-Campaigns"
    Write-Output "Measurements complete: PreviewExports/ReleaseBalance/$Label. Review Checks.tsv: completion does not mean every gate passed."
} finally { Pop-Location }
