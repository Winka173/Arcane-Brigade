"""Final native smoke/profile through the isolated bridge; no OS input or focus control."""
import json
import os
from pathlib import Path
from uiux_audit import ROOT, send

OUT = ROOT / os.environ.get('BRIGADE_AUDIT_OUTPUT', 'PreviewExports/UIUXAuditAfter')


def action(op, **values):
    result = send({'op': op, **values})
    print(op, json.dumps(values, ensure_ascii=True), result['status'], flush=True)
    return result


def snapshot(name):
    return json.loads((OUT / (name + '.json')).read_text(encoding='utf-8'))


def verify_artifacts():
    description = 'Lượng sát thương gây ra trong mỗi đòn đánh.'
    assert any(e['text'] == description for e in snapshot('final-reviewed-glossary')['labels'])
    assert not any(e['text'] == description for e in snapshot('final-glossary-closed')['labels'])
    focus = snapshot('final-settings-visible-focus')
    assert focus['navigationSelected'] >= 30
    assert any(e['text'] == 'Giảm chuyển động' for e in focus['labels'])
    for tab in range(4):
        assert snapshot('final-brigade-tab-' + str(tab) + '-baseline')['status'] == 'captured'
    assert not (OUT / 'runtime-errors.txt').read_text(encoding='utf-8').strip()
    report = ROOT / 'Docs/Verification/Performance/native-flow.txt'
    report.write_text('\n'.join([
        'PASS reviewed Vietnamese glossary description rendered',
        'PASS painted close control activated; glossary description removed',
        'PASS settings navigation moves the viewport to Comfort controls; selected=' + str(focus['navigationSelected']),
        'PASS all four brigade tabs rendered and hover sweeps completed',
        'PASS native runtime error collection empty',
        'Method: isolated fixture, virtual GUI pointer/activation and direct navigation; no OS input or focus control.',
        'Profiler samples are separate commands without screenshot logging; hardware/touch behaviour is not certified.'
    ]) + '\n', encoding='utf-8')


if __name__ == '__main__':
    action('snapshot')  # Player must already have reached its menu.
    action('language', value='vi')
    action('page', value='Help')
    action('glossary', value='Damage', name='final-reviewed-glossary')
    description = 'Lượng sát thương gây ra trong mỗi đòn đánh.'
    assert any(e['text'] == description for e in snapshot('final-reviewed-glossary')['labels'])
    action('activate', x=1210, y=375, name='final-glossary-closed')
    assert not any(e['text'] == description for e in snapshot('final-glossary-closed')['labels'])
    action('page', value='Settings')
    action('navigation', number=30, name='final-settings-visible-focus')
    action('page', value='Roster')
    action('sweep', name='final-roster')
    action('load', value='expanded', number=15)
    action('profile', value='final-expanded-combat', number=600)
    action('details', name='final-expanded-book')
    action('profile', value='final-expanded-book', number=600)
    for tab in range(4):
        action('tab', number=tab)
        action('sweep', name='final-brigade-tab-' + str(tab))
    action('quit')
    verify_artifacts()
    print('FINAL_NATIVE_UIUX_PERFORMANCE_PASS', flush=True)
