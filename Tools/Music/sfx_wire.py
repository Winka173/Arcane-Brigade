"""Wire SFX v6 into Resources/Audio/Arcane/cues.json: 15 boss sets (boss.<key>.<enter|windup|hit|phase|defeat>, settings copied from
the generic boss cues) and new clips for the weapon / pickup cues. Removes old clips no cue references any more.
Usage: python sfx_wire.py <Resources/Audio/Arcane dir>"""
import copy
import json
import os
import sys

ARC = sys.argv[1]
BOSSES = ['chairporter', 'doorthatstayedshut', 'bedside', 'lastcustomer', 'keeper', 'undeliveredking', 'scarletindex', 'sleepcollector',
          'waxadmiral', 'lastlantern', 'marqueemaw', 'motherumbra', 'coinwidow', 'howlingorgan', 'suitcaseleviathan']
PARTS = [('enter', 'boss.enter', 'entrance'), ('windup', 'boss.telegraph', 'windup'), ('hit', 'boss.attack', 'hit'),
         ('phase', 'boss.phase', 'phase'), ('defeat', 'boss.defeat', 'defeat')]
WEAPONS = {'cmb.swing.blade': 'slash', 'cmb.swing.spear': 'slash', 'cmb.swing.hammer': 'blunt', 'wpn.gun.pop': 'toy_gun',
           'wpn.launcher': 'paper_shot', 'wpn.split': 'paper_shot', 'wpn.cast.arcane': 'ink_shot', 'wpn.boomerang.fly': 'thrown',
           'wpn.yoyo.throw': 'thrown', 'cmb.hit.paper': 'impact', 'cmb.hit.fabric': 'impact', 'cmb.crit': 'big_impact',
           'loot.coin.pickup': 'coin_pickup', 'loot.egg': 'egg_pickup'}

path = os.path.join(ARC, 'cues.json')
raw = open(path, 'rb').read()
d = json.loads(raw.decode('utf-8'))
cues = d['cues']
by = {c['id']: c for c in cues}
old = {clip for c in cues for clip in c['clips']}
for cid, fam in WEAPONS.items():
    by[cid]['clips'] = [f'Sfx/sfx_{fam}_{k}' for k in 'abc']
    by[cid]['pitchJitter'] = max(by[cid].get('pitchJitter', 0), 0.04)
cues = [c for c in cues if not (c['id'].count('.') == 2 and c['id'].split('.')[1] in BOSSES)]
at = max(i for i, c in enumerate(cues) if c['id'] == 'boss.defeat') + 1
new = []
for key in BOSSES:
    for part, generic, fname in PARTS:
        c = copy.deepcopy(by[generic])
        c['id'] = f'boss.{key}.{part}'
        c['clips'] = [f'Sfx/boss_{key}_{fname}']
        c['pitchJitter'] = 0.02
        new.append(c)
cues[at:at] = new
d['cues'] = cues
nl = '\r\n' if b'\r\n' in raw else '\n'
txt = json.dumps(d, indent=1, ensure_ascii=False).replace('\n', nl)
open(path, 'w', encoding='utf-8', newline='').write(txt + (nl if raw.endswith(b'\n') else ''))
now = {clip for c in cues for clip in c['clips']}
removed = 0
for clip in sorted(old - now):
    for ext in ('.wav', '.wav.meta'):
        f = os.path.join(ARC, clip + ext)
        if os.path.exists(f):
            os.remove(f)
            removed += ext == '.wav'
print('cues', len(cues), 'clip refs', sum(len(c['clips']) for c in cues), 'orphans removed', removed, sorted(old - now)[:6])
