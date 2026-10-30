# Compile every runtime/editor source and run the deterministic rules checks without launching Unity.
param([string]$UnityData = 'C:/Program Files/Unity/Hub/Editor/6000.4.8f1/Editor/Data')
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$outDir = Join-Path $taskRoot '.utmp/wide-fix'
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
$dotnetExe = 'C:/Program Files/dotnet/dotnet.exe'
$compiler = (Get-ChildItem 'C:/Program Files/dotnet/sdk/*/Roslyn/bincore/csc.dll' | Sort-Object FullName | Select-Object -Last 1).FullName
$utf8 = New-Object System.Text.UTF8Encoding($false)
function Write-Source($path, $source) { [IO.File]::WriteAllText($path, $source, $utf8) }
function Compile-Source($name, $arguments) {
    $response = Join-Path $outDir ($name + '.rsp')
    Write-Source $response ($arguments -join "`n")
    $output = & $dotnetExe $compiler ('@' + $response) 2>&1
    $code = $LASTEXITCODE
    Write-Source (Join-Path $outDir ($name + '.log')) ($output -join "`n")
    if ($code -ne 0) { $output | Write-Output; throw "$name compile failed: $code" }
    Write-Output "$name compile PASS"
}
function Reference-Args($files) { @($files | ForEach-Object { '-r:"' + $_.FullName + '"' }) }
function Source-Args($files) { @($files | ForEach-Object { '"' + $_.FullName + '"' }) }
$common = @('-nologo', '-langversion:latest', '-nostdlib+', '-define:UNITY_EDITOR,UNITY_6000_0_OR_NEWER')
$unityRefs = @(Get-ChildItem "$UnityData/NetStandard/ref" -Recurse -Filter *.dll) + @(Get-ChildItem "$UnityData/NetStandard/compat" -Recurse -Filter *.dll) + @(Get-ChildItem "$UnityData/Managed/UnityEngine" -Filter *.dll)
$runtimePath = Join-Path $outDir 'Runtime.dll'
Compile-Source 'runtime' ($common + @('-target:library', ('-out:"' + $runtimePath + '"')) + (Reference-Args $unityRefs) + (Source-Args (Get-ChildItem "$taskRoot/Assets/ArcaneBrigade/Runtime" -Recurse -Filter *.cs)))
$runtimeRef = '-r:"' + $runtimePath + '"'
$spriteEditor = Get-ChildItem "$UnityData/Resources/PackageManager/ProjectTemplates/libcache" -Recurse -Filter Unity.2D.Sprite.Editor.dll | Select-Object -First 1
Compile-Source 'editor-all' ($common + @('-target:library', ('-out:"' + (Join-Path $outDir 'Editor.dll') + '"'), $runtimeRef) + (Reference-Args ($unityRefs + @($spriteEditor))) + (Source-Args (Get-ChildItem "$taskRoot/Assets/ArcaneBrigade/Editor" -Recurse -Filter *.cs)))
# Unity's Euler conversion is a native ECall. Adapt only disposable harness copies to the
# equivalent managed formula; runtime/editor compilation above always uses original sources.
$managedRuntime = @()
foreach ($file in Get-ChildItem "$taskRoot/Assets/ArcaneBrigade/Runtime" -Recurse -Filter *.cs) {
    $path = Join-Path $outDir ('runtime-' + $managedRuntime.Count + '.cs')
    Write-Source $path ([IO.File]::ReadAllText($file.FullName).Replace('Quaternion.Euler(', 'ManagedMath.Euler('))
    $managedRuntime += Get-Item $path
}
$mathPath = Join-Path $outDir 'ManagedMath.cs'
Write-Source $mathPath @'
static class ManagedMath {
    public static UnityEngine.Quaternion Euler(float x, float y, float z) {
        double h=System.Math.PI/360;
        double sx=System.Math.Sin(x*h),cx=System.Math.Cos(x*h),sy=System.Math.Sin(y*h),cy=System.Math.Cos(y*h),sz=System.Math.Sin(z*h),cz=System.Math.Cos(z*h);
        return new UnityEngine.Quaternion((float)(sx*cy*cz+cx*sy*sz),(float)(cx*sy*cz-sx*cy*sz),(float)(cx*cy*sz-sx*sy*cz),(float)(cx*cy*cz+sx*sy*sz));
    }
}
'@
Compile-Source 'runtime-managed' ($common + @('-target:library', ('-out:"' + $runtimePath + '"')) + (Reference-Args $unityRefs) + (Source-Args ($managedRuntime + @(Get-Item $mathPath))))
$checks = @('BondEngineChecks', 'BondV5RulesChecks', 'MortalityChecks', 'CompanionRulesV3Checks', 'HeroRulesV3Checks', 'EnemyRulesV3Checks', 'RulesV3IntegrationChecks', 'RulesV3AllChecks', 'WideMergeRegressionChecks')
$adapted = @()
foreach ($name in $checks) {
    $source = [IO.File]::ReadAllText("$taskRoot/Assets/ArcaneBrigade/Editor/$name.cs")
    $source = [regex]::Replace($source, 'ScriptableObject.CreateInstance<([^>]+)>\(\)', 'ManagedAssets.Create<$1>()')
    $source = [regex]::Replace($source, '(?:UnityEngine\.)?Object.DestroyImmediate\(([^;]+)\);', '{}')
    $source = $source.Replace('UnityEngine.Debug.Log(', 'System.Console.WriteLine(').Replace('Debug.Log(', 'System.Console.WriteLine(')
    $path = Join-Path $outDir ($name + '.cs')
    Write-Source $path $source
    $adapted += Get-Item $path
}
$program = @'
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
static class ManagedAssets {
    static int nextId;
    public static T Create<T>() where T: UnityEngine.ScriptableObject {
        T value=(T)RuntimeHelpers.GetUninitializedObject(typeof(T));
        typeof(UnityEngine.Object).GetField("m_CachedPtr",BindingFlags.NonPublic|BindingFlags.Instance)?.SetValue(value,new IntPtr(1));
        typeof(UnityEngine.Object).GetField("m_InstanceID",BindingFlags.NonPublic|BindingFlags.Instance)?.SetValue(value,++nextId);
        foreach(var field in typeof(T).GetFields(BindingFlags.Public|BindingFlags.Instance))
            if(field.FieldType.IsArray)field.SetValue(value,Array.CreateInstance(field.FieldType.GetElementType(),0));
        return value;
    }
}
class Program { static int Main() { try {
    ArcaneBrigade.Editor.RulesV3AllChecks.Run();
    ArcaneBrigade.Editor.WideMergeRegressionChecks.Run(); return 0;
} catch(Exception error) { Console.Error.WriteLine(error); return 1; } } }
'@
Write-Source (Join-Path $outDir 'Program.cs') $program
$netRefs = Get-ChildItem 'C:/Program Files/dotnet/packs/Microsoft.NETCore.App.Ref/*/ref/net9.0/*.dll'
$coreRef = '-r:"' + "$UnityData/Managed/UnityEngine/UnityEngine.CoreModule.dll" + '"'
Compile-Source 'managed-checks' ($common + @('-target:exe', ('-out:"' + (Join-Path $outDir 'Checks.dll') + '"'), $runtimeRef, $coreRef) + (Reference-Args $netRefs) + (Source-Args ($adapted + @(Get-Item (Join-Path $outDir 'Program.cs')))))
Get-ChildItem "$UnityData/Managed/UnityEngine" -Filter *.dll | Copy-Item -Destination $outDir
Write-Source (Join-Path $outDir 'Checks.runtimeconfig.json') '{"runtimeOptions":{"tfm":"net9.0","framework":{"name":"Microsoft.NETCore.App","version":"9.0.0"}}}'
$ErrorActionPreference = 'Continue'
$result = & $dotnetExe (Join-Path $outDir 'Checks.dll') 2>&1
$code = $LASTEXITCODE
$ErrorActionPreference = 'Stop'
Write-Source (Join-Path $outDir 'checks.log') ($result -join "`n")
$result | Write-Output
if ($code -ne 0) { throw "Managed checks failed: $code" }
