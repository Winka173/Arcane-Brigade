param([switch]$Reduced, [string]$Output = 'PreviewExports/Secrets/Accepted')
$ErrorActionPreference = 'Stop'
$secretProject = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$secretOutput = [System.IO.Path]::GetFullPath((Join-Path $secretProject $Output))
New-Item -ItemType Directory -Force -Path $secretOutput | Out-Null
# A renderable off-desktop window provides a real D3D11 backbuffer without
# interrupting the user's desktop. All window operations are restricted to
# the newly launched diagnostic player's PID. No normal profile is loaded.
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class SecretCaptureWindow {
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
$secretArgs = @('-force-d3d11','-screen-fullscreen','0','-screen-width','1920','-screen-height','1080',
    '-arcane-k7-capture','-arcane-secret-capture','-capturePath',$secretOutput,'-logFile',(Join-Path $secretOutput 'Player.log'))
if ($Reduced) { $secretArgs += '-arcane-secret-reduced' }
$secretPlayer = Start-Process -FilePath (Join-Path $secretProject 'Builds/Secrets/ArcaneBrigade.exe') -ArgumentList $secretArgs -WindowStyle Hidden -PassThru
Start-Sleep -Seconds 2
for ($secretAttempt = 0; $secretAttempt -lt 3; $secretAttempt++) {
    if ($secretPlayer.HasExited) { break }
    [SecretCaptureWindow]::RenderOffscreen($secretPlayer.Id)
    Start-Sleep -Milliseconds 100
}
Write-Output ('Diagnostic player PID: ' + $secretPlayer.Id)
