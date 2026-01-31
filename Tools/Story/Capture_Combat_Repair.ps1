param([string]$Executable='Builds/HeroIdentity/ArcaneBrigade.exe')
$ErrorActionPreference='Stop'
$repoPath=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$playerPath=(Resolve-Path (Join-Path $repoPath $Executable)).Path
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class CombatRepairWindow {
 public delegate bool Visitor(IntPtr window,IntPtr data);
 [DllImport("user32.dll")] static extern bool EnumWindows(Visitor callback,IntPtr data);
 [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr window,out uint process);
 [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr window,IntPtr after,int x,int y,int width,int height,uint flags);
 [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr window,int command);
 public static void Place(int processId) {
  EnumWindows((window,data)=>{uint owner;GetWindowThreadProcessId(window,out owner);
   if(owner==processId){SetWindowPos(window,IntPtr.Zero,-12000,-12000,0,0,0x0015);ShowWindow(window,4);}return true;},IntPtr.Zero);
 }
}
'@
$runs=@()
foreach($mode in @('Native','Reduced')){
 $started=Get-Date
 $capturePath=Join-Path $repoPath ('PreviewExports/CombatRepair/'+$mode)
 New-Item -ItemType Directory -Path $capturePath -Force | Out-Null
 $flags=if($mode -eq 'Reduced'){'-repair-reduced'}else{''}
 $playerArgs='-screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -force-d3d11 -arcane-combat-repair-capture '+$flags+' -capturePath "'+$capturePath+'" -logFile "'+$capturePath+'\Player.log"'
 $player=Start-Process -FilePath $playerPath -ArgumentList $playerArgs -WindowStyle Hidden -PassThru
 for($attempt=0;$attempt -lt 8;$attempt++){
  if($player.HasExited){break}
  [CombatRepairWindow]::Place($player.Id)
  Start-Sleep -Milliseconds 300
 }
 $deadline=$started.AddSeconds(150)
 while(-not $player.HasExited){
  if((Get-Date) -gt $deadline){Stop-Process -Id $player.Id;throw ('Capture timed out: '+$mode)}
  Start-Sleep -Seconds 2
 }
 $player.WaitForExit();$player.Refresh()
 $checkPath=Join-Path $capturePath 'Native_Checks.txt'
 $checks=@(Get-Content -LiteralPath $checkPath -Encoding UTF8)
 if($player.ExitCode -ne 0 -or (Get-Item -LiteralPath $checkPath).LastWriteTime -lt $started -or [string]$checks[-1] -ne 'PASS Combat repair native verification complete' -or @($checks | Where-Object{-not $_.StartsWith('PASS ')}).Count -gt 0){throw ('Incomplete combat repair verification: '+$mode)}
 $runs+=@{Mode=$mode;Checks=$checks.Count;Images=@(Get-ChildItem -LiteralPath $capturePath -Filter '*.png').Count;ExitCode=$player.ExitCode;Started=$started.ToString('o');Finished=(Get-Date).ToString('o')}
 $report=@{Player=$Executable;RuntimeSha256=(Get-FileHash -LiteralPath (Join-Path (Split-Path $playerPath) 'ArcaneBrigade_Data/Managed/ArcaneBrigade.Runtime.dll') -Algorithm SHA256).Hash;Runs=$runs}
 $report | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $repoPath 'PreviewExports/CombatRepair/Capture_Runs.json') -Encoding UTF8
 $runs[-1] | ConvertTo-Json -Compress
}
