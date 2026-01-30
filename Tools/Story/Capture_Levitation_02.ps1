param([int]$BuildProcessId=0,[switch]$SkipStory)
$ErrorActionPreference='Stop'
$repoPath=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(-not $SkipStory){& (Join-Path $PSScriptRoot 'Capture_ActOne_All.ps1') -BuildProcessId $BuildProcessId}
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class LevitationCaptureWindow {
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
foreach($name in @('Native','Reduced')){
    $started=Get-Date
    $capturePath=Join-Path $repoPath ('PreviewExports/Levitation02/Shop/'+$name)
    New-Item -ItemType Directory -Path $capturePath -Force | Out-Null
    $flag=if($name -eq 'Reduced'){'-arcane-shop-presentation-reduced'}else{''}
    $playerArgs='-screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -force-d3d11 -arcane-k7-capture -arcane-shop-presentation-capture '+$flag+' -capturePath "'+$capturePath+'" -logFile "'+$capturePath+'\Player.log"'
    $process=Start-Process -FilePath (Join-Path $repoPath 'Builds/HeroIdentity/ArcaneBrigade.exe') -ArgumentList $playerArgs -WindowStyle Hidden -PassThru
    Start-Sleep -Seconds 1
    for($attempt=0;$attempt -lt 3;$attempt++){
        if($process.HasExited){break}
        [LevitationCaptureWindow]::Place($process.Id)
        Start-Sleep -Milliseconds 150
    }
    if(-not $process.WaitForExit(180000)){Stop-Process -Id $process.Id;throw ('Shop capture timed out: '+$name)}
    $process.Refresh();$path=Join-Path $capturePath 'Native_Checks.txt'
    if($process.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $path)){throw ('Shop capture failed: '+$name)}
    $checks=@(Get-Content -LiteralPath $path -Encoding UTF8)
    if((Get-Item -LiteralPath $path).LastWriteTime -lt $started -or $checks[-1] -ne 'PASS Shop presentation native review complete' -or @($checks | Where-Object{-not $_.StartsWith('PASS ')}).Count -gt 0){throw ('Incomplete shop capture: '+$name)}
    $runs+=@{Name=$name;Checks=$checks.Count;ExitCode=$process.ExitCode;Started=$started.ToString('o');Finished=(Get-Date).ToString('o')}
    $runs | ConvertTo-Json -Depth 3 | Set-Content -LiteralPath (Join-Path $repoPath 'PreviewExports/Levitation02/Capture_Runs.json') -Encoding UTF8
}
