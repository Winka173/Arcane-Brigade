"""Capture actual native control hover states; commands marked fixture set up review data, not flow passes."""
import json
import os
import pathlib
import time
from uiux_audit import ROOT, send

OUT = ROOT / os.environ.get('BRIGADE_AUDIT_OUTPUT', 'PreviewExports/UIUXAuditSynthetic')
STEPS = json.loads((OUT / 'steps.json').read_text(encoding='utf-8')) if (OUT / 'steps.json').exists() else []
POSITION = 0


def action(op, **kwargs):
    global POSITION
    if POSITION < len(STEPS):
        previous = STEPS[POSITION]
        if previous['op'] != op or not all(previous.get(k) == value for k, value in kwargs.items()):
            raise RuntimeError('Resume sequence differs from saved audit steps')
        POSITION += 1
        return
    began = time.monotonic()
    result = send({'op': op, **kwargs}, timeout=300)
    STEPS.append({'op': op, **kwargs, 'seconds': round(time.monotonic() - began, 2), 'status': result['status']})
    POSITION += 1
    (OUT / 'steps.json').write_text(json.dumps(STEPS, ensure_ascii=False, indent=2), encoding='utf-8')
    print(op, kwargs, flush=True)


def sweep(name):
    action('sweep', name=name)


if __name__ == '__main__':
    for language in ('en', 'vi'):
        action('language', value=language)
        for page in ('Landing', 'Settings', 'Roster', 'Help', 'Museum', 'Records', 'Achievements', 'WeaponSandbox'):
            action('page', value=page)
            sweep(f'{language}-{page.lower()}-fresh')
            if page == 'Settings':
                for y in (440, 880, 1320):
                    action('scroll', value='settings', y=y)
                    sweep(f'{language}-settings-scroll-{y}')
                action('scroll', value='settings', y=0)
        action('page', value='Roster')
        action('roster-overlay', value='difficultyPickerAt')
        sweep(f'{language}-difficulty')
        action('page', value='Roster')
        action('roster-overlay', value='pactPickerAt')
        sweep(f'{language}-pacts')
    action('fixture')
    action('language', value='vi')
    for page in ('Museum', 'WeaponSandbox', 'Achievements'):
        action('page', value=page)
        sweep('vi-' + page.lower() + '-populated')
    for station in ('Lantern', 'Postbox', 'Wardrobe', 'Museum', 'Ledger', 'Clock'):
        action('hub', value=station)
        sweep('vi-hub-' + station.lower())
        if station == 'Lantern':
            action('branch', number=0)
            action('lamp', value='hero.wick')
            sweep('vi-lamp-node')
    action('load', value='shop', number=4, name='vi-shop-room')
    action('shop-focus', number=-1)
    sweep('vi-shop-offer')
    action('shop-detail')
    sweep('vi-shop-detail')
    action('pause')
    sweep('vi-pause')
    for number in (1, 2, 3, 4):
        action('pause-page', number=number)
        sweep('vi-pause-page-' + str(number))
    action('pause-page', number=0)
    action('details')
    for number in (0, 1, 2, 3):
        action('tab', number=number)
        sweep('vi-brigade-tab-' + str(number))
    print('NATIVE_UIUX_AUDIT_CAPTURE_COMPLETE', flush=True)
