param([switch]$Reduced, [string]$Output = 'PreviewExports/BuildMorphs/Standard')
$ErrorActionPreference = 'Stop'
$morphProject = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$morphOutput = [System.IO.Path]::GetFullPath((Join-Path $morphProject $Output))
New-Item -ItemType Directory -Force -Path $morphOutput | Out-Null
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class MorphCaptureWindow {
 public delegate bool Visitor(IntPtr w,IntPtr data);
 [DllImport("user32.dll")] static extern bool EnumWindows(Visitor cb,IntPtr data);
 [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr w,out uint pid);
 [DllImport("user32.dll")] static extern bool MoveWindow(IntPtr w,int x,int y,int width,int height,bool redraw);
 [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr w,int command);
 public static void RenderOffscreen(int pid) {
  EnumWindows((w,d)=>{uint owner;GetWindowThreadProcessId(w,out owner);
   if(owner==pid) {MoveWindow(w,-12000,-12000,1936,1119,true);ShowWindow(w,4);}return true;},IntPtr.Zero);
 }
}
'@
$morphArgs = @('-force-d3d11','-screen-fullscreen','0','-screen-width','1920','-screen-height','1080',
    '-arcane-k7-capture','-arcane-morph-capture','-capturePath',$morphOutput,'-logFile',(Join-Path $morphOutput 'Player.log'))
if ($Reduced) { $morphArgs += '-arcane-morph-reduced' }
$morphPlayer = Start-Process -FilePath (Join-Path $morphProject 'Builds/BuildMorphs/ArcaneBrigade.exe') -ArgumentList $morphArgs -WindowStyle Hidden -PassThru
Start-Sleep -Seconds 2
for ($morphAttempt = 0; $morphAttempt -lt 3; $morphAttempt++) {
    if ($morphPlayer.HasExited) { break }
    [MorphCaptureWindow]::RenderOffscreen($morphPlayer.Id)
    Start-Sleep -Milliseconds 100
}
Write-Output ('Diagnostic player PID: ' + $morphPlayer.Id)
