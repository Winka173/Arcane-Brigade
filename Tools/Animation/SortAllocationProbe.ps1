param([string]$UnityEditor = 'C:/Program Files/Unity/Hub/Editor/6000.4.8f1/Editor')
$ErrorActionPreference = 'Stop'
$project = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$data = Join-Path $UnityEditor 'Data'
$output = Join-Path $project 'Temp/EnsembleSortProbe.exe'
New-Item -ItemType Directory -Force (Split-Path $output) | Out-Null
& (Join-Path $data 'NetCoreRuntime/dotnet.exe') (Join-Path $data 'DotNetSdkRoslyn/csc.dll') /nologo /noconfig /nostdlib+ ('/reference:' + (Join-Path $data 'MonoBleedingEdge/lib/mono/4.8-api/mscorlib.dll')) ('/out:' + $output) (Join-Path $PSScriptRoot 'SortAllocationProbe.cs')
if ($LASTEXITCODE -ne 0) { throw 'Could not compile the installed-Mono allocation probe' }
& (Join-Path $data 'MonoBleedingEdge/bin/mono.exe') $output
if ($LASTEXITCODE -ne 0) { throw 'Cached comparison unexpectedly allocated managed memory' }
