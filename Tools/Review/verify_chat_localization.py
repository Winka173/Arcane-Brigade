"""Render the reported merchant caption through SpeechBubbles, using only the isolated test bridge."""
import json
import os
from uiux_audit import ROOT, send

OUT = ROOT / os.environ.get('BRIGADE_AUDIT_OUTPUT', 'PreviewExports/ChatLocalization')


def action(op, **values):
    result = send({'op': op, **values})
    print(op, json.dumps(values, ensure_ascii=True), result['status'], flush=True)


if __name__ == '__main__':
    action('snapshot')  # Wait until the audit player has reached the menu before starting.
    action('language', value='vi')
    action('load', value='shop', number=4)
    action('chat', value='Not those? Fair.', name='vi-merchant-reroll')
    action('language', value='en', name='en-merchant-reroll')
    action('language', value='vi', name='vi-merchant-reroll-switched-back')
    expected = {'vi-merchant-reroll': 'Chưa ưng à? Được thôi.',
                'en-merchant-reroll': 'Not those? Fair.',
                'vi-merchant-reroll-switched-back': 'Chưa ưng à? Được thôi.'}
    notes = []
    for name, caption in expected.items():
        snapshot = json.loads((OUT / (name + '.json')).read_text(encoding='utf-8'))
        assert any(e['text'] == caption for e in snapshot['labels']), name
        notes.append('PASS native SpeechBubbles caption: ' + name + ' = ' + caption)
    action('quit')
    assert not (OUT / 'runtime-errors.txt').read_text(encoding='utf-8').strip()
    notes.append('PASS native runtime errors empty; no OS mouse, keyboard or focus control used.')
    report = ROOT / 'Docs/Verification/ChatLocalization'
    report.mkdir(parents=True, exist_ok=True)
    (report / 'NativeChecks.txt').write_text('\n'.join(notes) + '\n', encoding='utf-8')
    print('CHAT_LOCALIZATION_NATIVE_PASS', flush=True)
