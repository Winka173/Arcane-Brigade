param([switch]$Reduced, [switch]$Animation, [switch]$Ensemble, [switch]$Stress, [switch]$Shop, [switch]$Expanded, [string]$Output = 'PreviewExports/Nightmare/Accepted', [string]$Player = 'Builds/Nightmare/ArcaneBrigade.exe')
$ErrorActionPreference = 'Stop'
$project = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$destination = [System.IO.Path]::GetFullPath((Join-Path $project $Output))
New-Item -ItemType Directory -Force -Path $destination | Out-Null
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class NightmareCaptureWindow {
 public delegate bool Visitor(IntPtr window,IntPtr data);
 [DllImport("user32.dll")] static extern bool EnumWindows(Visitor callback,IntPtr data);
 [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr window,out uint process);
 [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr window,IntPtr after,int x,int y,int width,int height,uint flags);
 [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr window,int command);
 public static void Place(int processId) {
  EnumWindows((window,data)=>{uint owner;GetWindowThreadProcessId(window,out owner);
   if(owner==processId) {SetWindowPos(window,IntPtr.Zero,-12000,-12000,0,0,0x0015);ShowWindow(window,4);}return true;},IntPtr.Zero);
 }
}
'@
$Animation = $Animation -or $Ensemble -or $Stress -or $Expanded
$captureMode = if ($Shop) { '-arcane-shop-presentation-capture' } elseif ($Animation) { '-arcane-animation-capture' } else { '-arcane-nightmare-capture' }
$arguments = @('-force-d3d11','-screen-fullscreen','0','-screen-width','1920','-screen-height','1080',
    '-arcane-k7-capture',$captureMode,'-capturePath',('"' + $destination + '"'),'-logFile',('"' + (Join-Path $destination 'Player.log') + '"'))
if ($Reduced) { $arguments += $(if ($Shop) { '-arcane-shop-presentation-reduced' } elseif ($Animation) { '-arcane-animation-reduced' } else { '-arcane-nightmare-reduced' }) }
if ($Ensemble) { $arguments += '-arcane-ensemble-capture' }
if ($Expanded) { $arguments += '-arcane-expanded-performance-capture' }
if ($Stress) { $arguments += '-arcane-animation-stress' }
$captureProcess = Start-Process -FilePath (Join-Path $project $Player) -ArgumentList $arguments -WindowStyle Hidden -PassThru
Start-Sleep -Seconds 2
for ($attempt = 0; $attempt -lt 3; $attempt++) {
    if ($captureProcess.HasExited) { break }
    [NightmareCaptureWindow]::Place($captureProcess.Id)
    Start-Sleep -Milliseconds 100
}
Write-Output ('Diagnostic player PID: ' + $captureProcess.Id)
