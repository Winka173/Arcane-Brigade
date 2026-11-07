"""Native Unity UI review. Temporary logging hooks are restored byte-for-byte after building.

prepare / restore: install/remove the isolated audit runner and two presentation logging hooks.
send: send a JSON command to a running -uiux-audit player and wait for the matching response.
"""
import argparse
import hashlib
import json
import os
import pathlib
import time

ROOT = pathlib.Path(__file__).resolve().parents[2]
WORK = pathlib.Path(os.environ.get('BRIGADE_AUDIT_BRIDGE', str(ROOT / 'Logs/UIUXAudit')))
STATE = WORK / 'source-backup.json'
RUNTIME = ROOT / 'Assets/ArcaneBrigade/Runtime/Core/UIUXAuditProbe.cs'
EDITOR = ROOT / 'Assets/ArcaneBrigade/Editor/UIUXAuditBuild.cs'
BUILDER = '''using System; using System.IO; using System.Linq; using UnityEditor; using UnityEditor.Build.Reporting;
namespace ArcaneBrigade.Editor { public static class UIUXAuditBuild {
[Serializable] private sealed class Result { public string result; public int errors, warnings; }
public static void Run() {
PerformanceReviewChecks.MeasureLocalization(); LocalizationChecks.Run(); UIUXPerformanceChecks.Run();
Directory.CreateDirectory("Builds/UIUXAudit"); var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(), target=BuildTarget.StandaloneWindows64,
locationPathName="Builds/UIUXAudit/ArcaneBrigade.exe", options=BuildOptions.None });
File.WriteAllText("Logs/UIUXAudit/build-result.json", UnityEngine.JsonUtility.ToJson(new Result { result=report.summary.result.ToString(), errors=report.summary.totalErrors, warnings=report.summary.totalWarnings }));
if(report.summary.result!=BuildResult.Succeeded) throw new InvalidOperationException("Audit build failed");
} } }'''


def prepare():
    if STATE.exists():
        raise SystemExit('Audit hooks already prepared; restore before preparing again')
    template = pathlib.Path(__file__).with_name('UIUXAuditProbe.cs.txt').read_text(encoding='utf-8')
    forbidden = ('SetCursorPos', 'SendInput', 'mouse_event', 'keybd_event', 'PostMessage', 'SendMessage', 'SetForegroundWindow', 'AttachThreadInput')
    if any(api in template for api in forbidden):
        raise SystemExit('Audit runner must not control OS mouse, keyboard or focus')
    WORK.mkdir(parents=True, exist_ok=True)
    substitutions = {
        'Assets/ArcaneBrigade/Runtime/Core/RunPaths.cs': (
            'public static bool Diagnostic => ', 'public static bool Diagnostic => UIUXAuditProbe.Requested || '),
        'Assets/ArcaneBrigade/Runtime/UI/TouchTargets.cs': (
            'public static bool Button(Rect bounds, GUIContent content, GUIStyle style)\n        {',
            'public static bool Button(Rect bounds, GUIContent content, GUIStyle style)\n        {\n            if (UIUXAuditProbe.Activate(bounds)) return true;\n            UIUXAuditProbe.Record(bounds, content, style);'),
        'Assets/ArcaneBrigade/Runtime/UI/SettingsSheet.cs': (
            'GUI.HorizontalSlider(Shift(', 'UIUXAuditProbe.Slider(Shift('),
        'Assets/ArcaneBrigade/Runtime/UI/RunMenu.cs': (
            'private void OnGUI()\n        {', 'private void OnGUI()\n        {\n            UIUXAuditProbe.ApplyPointer();'),
        'Assets/ArcaneBrigade/Runtime/Core/ArenaController.cs': (
            'private void OnGUI()\n        {', 'private void OnGUI()\n        {\n            UIUXAuditProbe.ApplyPointer();'),
        'Assets/ArcaneBrigade/Runtime/Input/ArcadeNavigation.cs': (
            'public bool Button(Rect bounds, string caption, GUIStyle style)\n        {', 'public bool Button(Rect bounds, string caption, GUIStyle style)\n        {\n            UIUXAuditProbe.ApplyPointer();'),
        'Assets/ArcaneBrigade/Runtime/UI/ArcadeSkinKit.cs': (
            'private static int State(Rect r, bool enabled)\n        {', 'private static int State(Rect r, bool enabled)\n        {\n            UIUXAuditProbe.ApplyPointer();'),
        'Assets/ArcaneBrigade/Runtime/UI/LocalizedGUI.cs': (
            'GUI.Label(bounds, shown, fitted);',
            'UIUXAuditProbe.Record(bounds, shown, fitted, true);\n            GUI.Label(bounds, shown, fitted);')
    }
    backup = {}
    for name, (old, new) in substitutions.items():
        path = ROOT / name
        raw = path.read_bytes()
        text = raw.decode('utf-8-sig').replace('\r\n', '\n')
        if text.count(old) != 1:
            raise SystemExit(f'Unexpected hook point in {name}')
        backup[name] = {'original': raw.hex(), 'patched': hashlib.sha256(text.replace(old, new).encode()).hexdigest()}
    STATE.write_text(json.dumps(backup), encoding='utf-8')
    for name, (old, new) in substitutions.items():
        path = ROOT / name
        path.write_text(path.read_text(encoding='utf-8-sig').replace(old, new), encoding='utf-8')
    RUNTIME.write_bytes(template.encode('utf-8'))
    EDITOR.write_text(BUILDER, encoding='utf-8')
    print('Prepared isolated native audit instrumentation')


def restore():
    backup = json.loads(STATE.read_text(encoding='utf-8'))
    for name, item in backup.items():
        path = ROOT / name
        if hashlib.sha256(path.read_text(encoding='utf-8-sig').replace('\r\n', '\n').encode()).hexdigest() != item['patched']:
            raise SystemExit(f'Refusing to overwrite edits made after instrumentation: {name}')
    for name, item in backup.items():
        (ROOT / name).write_bytes(bytes.fromhex(item['original']))
    for path in (RUNTIME, EDITOR):
        path.unlink(missing_ok=True)
        pathlib.Path(str(path) + '.meta').unlink(missing_ok=True)
    STATE.unlink()
    print('Restored every instrumented product file byte-for-byte')


def send(command, timeout=180):
    WORK.mkdir(parents=True, exist_ok=True)
    command['id'] = str(time.time_ns())
    temporary = WORK / 'command.tmp'
    temporary.write_text(json.dumps(command, ensure_ascii=False), encoding='utf-8')
    for attempt in range(100):
        try:
            temporary.replace(WORK / 'command.json')
            break
        except PermissionError:
            if attempt == 99:
                raise
            time.sleep(.05)
    until = time.monotonic() + timeout
    while time.monotonic() < until:
        try:
            response = json.loads((WORK / 'response.json').read_text(encoding='utf-8'))
            if response['id'] == command['id']:
                if response['status'] == 'error':
                    raise RuntimeError(response.get('error'))
                return response
        except (FileNotFoundError, json.JSONDecodeError):
            pass
        time.sleep(.15)
    raise TimeoutError(command)


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('operation', choices=['prepare', 'restore', 'send'])
    parser.add_argument('command', nargs='?')
    args = parser.parse_args()
    if args.operation == 'prepare':
        prepare()
    elif args.operation == 'restore':
        restore()
    else:
        print(json.dumps(send(json.loads(args.command)), ensure_ascii=True))
