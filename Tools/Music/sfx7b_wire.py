"""SFX revision 7b wiring: the map mechanic cues (map.*) and the elite's arrival join cues.json, and the blows sit forward of
the music (requirement 1/11/2026: weapon sounds were too quiet against it).
Usage: python sfx7b_wire.py <Resources/Audio/Arcane dir>"""
import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sfx7 import R, clip_name  # noqa: E402

ARC = sys.argv[1]
path = os.path.join(ARC, 'cues.json')
d = json.load(open(path, encoding='utf-8'))
cues = d['cues']
have = {q['id'] for q in cues}

WARN = ('warn', 'map.room.call', 'map.post.planes', 'map.vault.potwarn')  # heard wherever the hero is (danger lane)
added = 0
for name, (fn, dur, count, wet, lpf) in R.items():
    if name in have or not (name.startswith('map.') or name == 'enemy.elite.arrive'):
        continue
    danger = any(w in name for w in WARN)
    cues.append({'id': name, 'bus': 'Combat', 'priority': 170 if danger else 140, 'gainDb': -9.0 if danger else -11.0,
                 'cooldown': .35 if danger else .08, 'maxVoices': 2 if danger else 3, 'pitchJitter': .03, 'loop': False, 'danger': danger,
                 'clips': ['Sfx/' + clip_name(name, k) for k in range(count)]})
    added += 1

# the blows: 4 dB forward (they had been pulled 1.5 dB back), never above -4 dB
FORWARD = ('cmb.swing', 'cmb.hit', 'cmb.crit', 'wpn.', 'sig.', 'hero.', 'enemy.die', 'fx.knockback', 'fx.reaction')
raised = 0
for q in cues:
    if q['id'].startswith(FORWARD):
        q['gainDb'] = float(round(min(-4.0, q.get('gainDb', -16) + 4.0), 1)); raised += 1

with open(path, 'w', encoding='utf-8', newline='\n') as fh:
    json.dump(d, fh, indent=1, ensure_ascii=False)
print('added', added, 'raised', raised)
