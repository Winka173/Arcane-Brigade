param([ValidateSet('Native','Reduced')][string]$Mode='Native',[string]$Output='PreviewExports/ShopDescriptions/Native',[int]$Width=1920,[int]$Height=1080)
$ErrorActionPreference='Stop'
$artRepo=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$artPlayerPath=(Resolve-Path (Join-Path $artRepo 'Builds/ShopDescriptions/ArcaneBrigade.exe')).Path
$artOutput=[IO.Path]::GetFullPath((Join-Path $artRepo $Output))
if(-not $artOutput.StartsWith($artRepo+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Capture output must stay inside the project'}
New-Item -ItemType Directory -Path $artOutput -Force | Out-Null
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class DescriptionWindow {
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
$artStarted=Get-Date
$artFlags=if($Mode -eq 'Reduced'){'-description-reduced'}else{''}
$artArgs='-screen-fullscreen 0 -screen-width '+$Width+' -screen-height '+$Height+' -force-d3d11 -arcane-description-capture '+$artFlags+' -capturePath "'+$artOutput+'" -logFile "'+$artRepo+'\Library\ShopDescriptions-'+$Mode+'.log"'
$artProcess=Start-Process -FilePath $artPlayerPath -WindowStyle Hidden -ArgumentList $artArgs -PassThru
for($artAttempt=0;$artAttempt -lt 12;$artAttempt++){
 if($artProcess.HasExited){break};[DescriptionWindow]::Place($artProcess.Id);Start-Sleep -Milliseconds 250
}
while(-not $artProcess.HasExited){
 if((Get-Date) -gt $artStarted.AddSeconds(180)){Stop-Process -Id $artProcess.Id;throw 'Description capture timed out'}
 Start-Sleep -Seconds 2
}
$artProcess.WaitForExit();$artProcess.Refresh()
$artChecks=@(Get-Content -LiteralPath (Join-Path $artOutput 'Checks.txt') -Encoding UTF8)
$artReport=@{Mode=$Mode;ExitCode=$artProcess.ExitCode;Checks=$artChecks.Count;Failures=@($artChecks | Where-Object { -not $_.StartsWith('PASS ') }).Count;Images=@(Get-ChildItem -LiteralPath $artOutput -Filter '*.png').Count;Started=$artStarted.ToString('o');Finished=(Get-Date).ToString('o');RuntimeSha256=(Get-FileHash (Join-Path (Split-Path $artPlayerPath) 'ArcaneBrigade_Data/Managed/ArcaneBrigade.Runtime.dll') -Algorithm SHA256).Hash}
$artReport | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $artOutput 'Run.json') -Encoding UTF8
$artReport | ConvertTo-Json -Compress
if($artProcess.ExitCode -ne 0 -or $artReport.Failures -gt 0){throw 'Description capture contains failed checks'}
