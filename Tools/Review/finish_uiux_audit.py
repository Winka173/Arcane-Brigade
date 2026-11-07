"""Targeted flow checks using in-process virtual activation only, never OS input."""
import json
import time
import sys
from pathlib import Path
from uiux_audit import ROOT, send

OUT = ROOT / 'PreviewExports/UIUXAuditSynthetic'


def action(op, **kwargs):
    result = send({'op': op, **kwargs})
    print(op, json.dumps(kwargs, ensure_ascii=True), result['status'], flush=True)


def menu_checks():
    until = time.monotonic() + 60
    while time.monotonic() < until:
        action('snapshot', name='final-ready')
        state = json.loads((OUT / 'final-ready.json').read_text(encoding='utf-8'))
        if state['scene'] == 'RunFrontend':
            break
        time.sleep(1)
    else:
        raise RuntimeError('Menu did not start')
    action('language', value='en')
    action('page', value='Settings')
    action('sweep', name='en-settings-with-sliders')
    action('activate', x=1000, y=239, name='vi-settings-after-virtual-activation')
    state = json.loads((OUT / 'vi-settings-after-virtual-activation.json').read_text(encoding='utf-8'))
    if state['language'] != 'vi':
        raise RuntimeError('Settings virtual activation did not select Vietnamese')
    action('navigation', number=30, name='vi-settings-navigation-offscreen')
    action('page', value='Achievements', name='vi-achievements-tooltip-baseline')
    data = json.loads((OUT / 'vi-achievements-tooltip-baseline.json').read_text(encoding='utf-8'))
    labels = [e for e in data['labels'] if e['tooltip']]
    if labels:
        rect = labels[0]['rect']
        action('pointer', x=rect['x'] + rect['width'] / 2, y=rect['y'] + rect['height'] / 2, name='vi-achievements-full-name-hover')
    action('page', value='Help')
    action('language', value='en')
    action('glossary', value='Damage', name='en-glossary-known-key')
    action('language', value='vi')
    action('glossary', value='Sát thương', name='vi-glossary-translated-key')


def shop_checks():
    action('language', value='vi')
    action('fixture')
    action('load', value='shop', number=4, name='vi-shop-stocked-final')
    action('shop-focus', number=1)
    action('sweep', name='vi-shop-item-final')
    action('shop-detail')
    action('sweep', name='vi-shop-item-detail-final')
    action('load', value='poor', number=4)
    action('shop-focus', number=1)
    action('sweep', name='vi-shop-insufficient-gold')
    action('pause')
    action('return-confirm')
    action('sweep', name='vi-return-confirm-final')
    action('quit')
    print('TARGETED_FLOW_AUDIT_COMPLETE', flush=True)


if __name__ == '__main__':
    if '--shop-only' not in sys.argv:
        menu_checks()
    shop_checks()
