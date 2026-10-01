# usage: hidden_unity.ps1 <unity.exe> <argument string...>  : runs Unity with no visible window, below-normal priority, and waits.
param([string]$Exe, [Parameter(ValueFromRemainingArguments = $true)] $Rest)
$p = Start-Process -FilePath $Exe -ArgumentList ($Rest -join ' ') -WindowStyle $(if ($env:FX_WINDOW) { $env:FX_WINDOW } else { 'Hidden' }) -PassThru
try { $p.PriorityClass = 'BelowNormal' } catch {}
$p.WaitForExit()
exit $p.ExitCode
