"""Compile against installed Unity references, then run deterministic managed rules checks.

Does not launch Unity. The managed check adapter replaces ScriptableObject allocation,
destruction and logging only; gameplay, vectors and math use the compiled runtime.
Usage: python Tools/Verify-RulesV3.py [Unity Editor Data directory]
"""
from pathlib import Path
import json
import os
import re
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
UNITY = Path(sys.argv[1] if len(sys.argv) > 1 else
             'C:/Program Files/Unity/Hub/Editor/6000.4.8f1/Editor/Data')
OUT = ROOT / '.utmp/rules-v3-lead'
OUT.mkdir(parents=True, exist_ok=True)
DOTNET = Path(os.environ.get('ProgramFiles', 'C:/Program Files')) / 'dotnet'
CSC = sorted((DOTNET / 'sdk').glob('*/Roslyn/bincore/csc.dll'))[-1]


def compile_cs(name, args):
    response = OUT / (name + '.rsp')
    response.write_text('\n'.join(args), encoding='utf-8')
    result = subprocess.run(['dotnet', str(CSC), '@' + str(response)], cwd=ROOT,
                            text=True, capture_output=True)
    (OUT / (name + '.log')).write_text(result.stdout + result.stderr, encoding='utf-8')
    if result.returncode:
        print(result.stdout + result.stderr)
        raise SystemExit(result.returncode)
    print(name + ': compile PASS')


def reference(paths):
    return ['-r:"' + str(path) + '"' for path in paths]


common = ['-nologo', '-langversion:latest', '-nostdlib+', '-define:UNITY_EDITOR,UNITY_6000_0_OR_NEWER']
unity_refs = list((UNITY / 'NetStandard/ref').rglob('*.dll'))
unity_refs += list((UNITY / 'NetStandard/compat').rglob('*.dll'))
unity_refs += list((UNITY / 'Managed/UnityEngine').glob('*.dll'))
runtime = OUT / 'Runtime.dll'
compile_cs('runtime', common + ['-target:library', '-out:"' + str(runtime) + '"'] +
           reference(unity_refs) + ['"' + str(p) + '"' for p in (ROOT / 'Assets/ArcaneBrigade/Runtime').rglob('*.cs')])
checks = ['BondEngineChecks', 'MortalityChecks', 'CompanionRulesV3Checks', 'HeroRulesV3Checks',
          'EnemyRulesV3Checks', 'RulesV3IntegrationChecks']
editor_sources = [ROOT / ('Assets/ArcaneBrigade/Editor/' + name + '.cs') for name in checks]
editor_sources += [ROOT / 'Assets/ArcaneBrigade/Editor/BondReworkBuilder.cs']
editor_refs = []  # UnityEditor modules are already in Managed/UnityEngine; omit the facade.
compile_cs('editor', common + ['-target:library', '-out:"' + str(OUT / 'Editor.dll') + '"'] +
           reference(unity_refs + editor_refs + [runtime]) +
           ['"' + str(p) + '"' for p in editor_sources])
adapted = []
for source in editor_sources[:-1]:
    if not source.exists():
        raise SystemExit('Missing required check: ' + str(source))
    text = source.read_text(encoding='utf-8-sig')
    text = re.sub(r'ScriptableObject.CreateInstance<([^>]+)>\(\)', r'ManagedAssets.Create<\1>()', text)
    text = re.sub(r'(?:UnityEngine\.)?Object.DestroyImmediate\(([^;]+)\);', '{}', text)
    text = text.replace('Debug.Log(', 'System.Console.WriteLine(')
    path = OUT / source.name
    path.write_text(text, encoding='utf-8')
    adapted.append(path)
program = '''using System;
using System.Reflection;
using System.Runtime.CompilerServices;
static class ManagedAssets {
    static int nextId;
    public static T Create<T>() where T: UnityEngine.ScriptableObject {
        T value=(T)RuntimeHelpers.GetUninitializedObject(typeof(T));
        typeof(UnityEngine.Object).GetField("m_CachedPtr",BindingFlags.NonPublic|BindingFlags.Instance)?.SetValue(value,new IntPtr(1));
        typeof(UnityEngine.Object).GetField("m_InstanceID",BindingFlags.NonPublic|BindingFlags.Instance)?.SetValue(value,++nextId);
        foreach(var field in typeof(T).GetFields(BindingFlags.Public|BindingFlags.Instance))
            if(field.FieldType.IsArray) field.SetValue(value,Array.CreateInstance(field.FieldType.GetElementType(),0));
        return value;
    }
}
class Program { static int Main() { try {
'''
program += '\n'.join('ArcaneBrigade.Editor.' + name + '.Run();' for name in checks)
program += '\nreturn 0; } catch(Exception error) { Console.Error.WriteLine(error); return 1; } } }'
(OUT / 'Program.cs').write_text(program, encoding='utf-8')
netrefs = sorted((DOTNET / 'packs/Microsoft.NETCore.App.Ref').glob('*/ref/net9.0'))[-1]
compile_cs('managed-checks', common + ['-target:exe', '-out:"' + str(OUT / 'Checks.dll') + '"'] +
           reference(list(netrefs.glob('*.dll')) + [runtime, UNITY / 'Managed/UnityEngine/UnityEngine.CoreModule.dll']) +
           ['"' + str(p) + '"' for p in adapted + [OUT / 'Program.cs']])
for assembly in (UNITY / 'Managed/UnityEngine').glob('*.dll'):
    shutil.copy2(assembly, OUT / assembly.name)
(OUT / 'Checks.runtimeconfig.json').write_text(json.dumps({'runtimeOptions': {
    'tfm': 'net9.0', 'framework': {'name': 'Microsoft.NETCore.App', 'version': '9.0.0'}}}))
result = subprocess.run(['dotnet', str(OUT / 'Checks.dll')], cwd=ROOT, text=True, capture_output=True, timeout=60)
print(result.stdout + result.stderr)
(OUT / 'checks.log').write_text(result.stdout + result.stderr, encoding='utf-8')
raise SystemExit(result.returncode)

