"""Compile project and run balance telemetry checks without launching Unity.

Uses the existing managed Unity check adapter. This does not simulate the production
asset catalog or generate balance results; run BalanceSim.Run in Unity for those.
"""
from pathlib import Path
import subprocess

root = Path(__file__).resolve().parents[1]
out = root / '.utmp/balance-sim-checks'
out.mkdir(parents=True, exist_ok=True)
script = (root / 'Tools/Verify-WideFix.ps1').read_text(encoding='utf-8-sig')
script = script.replace('$taskRoot = Split-Path $PSScriptRoot -Parent',
                        "$taskRoot = '" + str(root).replace("'", "''") + "'")
script = script.replace("'.utmp/wide-fix'", "'.utmp/balance-sim-checks'")
script = script.replace("'WideMergeRegressionChecks')", "'WideMergeRegressionChecks', "
                        "'BalanceSimChecks', 'BalanceSim', 'BalanceSimRecipes', "
                        "'BalanceSimTelemetry', 'BalanceSimReport')")
script = script.replace('ArcaneBrigade.Editor.WideMergeRegressionChecks.Run(); return 0;',
                        'ArcaneBrigade.Editor.WideMergeRegressionChecks.Run(); '
                        'ArcaneBrigade.Editor.BalanceSimChecks.Run(); return 0;')
# The check executable compiles the real entry point but never calls AssetDatabase.
script = script.replace('$coreRef = ', '$editorCoreRef = \'-r:"\' + '
                        '"$UnityData/Managed/UnityEngine/UnityEditor.CoreModule.dll" + \'"\'\n$coreRef = ')
script = script.replace('$runtimeRef, $coreRef)', '$runtimeRef, $coreRef, $editorCoreRef)')
path = out / 'Verify.ps1'
path.write_text(script, encoding='utf-8-sig')
result = subprocess.run(['powershell', '-ExecutionPolicy', 'Bypass', '-File', str(path)], cwd=root,
                        text=True, capture_output=True)
log = result.stdout + result.stderr
(out / 'verification.log').write_text(log, encoding='utf-8')
print(log)
raise SystemExit(result.returncode)
