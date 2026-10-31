"""Wire SFX revision 7 into Resources/Audio/Arcane/cues.json and balance the mix.

1. Points each rebuilt cue at its new clips (s7_<cue>_<n>).
2. Balances: the editor step AudioMixBuilder levels every clip to one RMS (loudness.json), so a cue's gainDb alone places it
   in the mix. Each cue's gain moves halfway toward its family's median gain (outliers come in, the family's order stays);
   weapons, swings, hits and skills sit 1.5 dB further back (they were too forward). Run AudioMixBuilder afterwards.
3. Deletes the old clips no cue uses any more (with their .meta).
Usage: python sfx7_wire.py <Resources/Audio/Arcane dir> [--dry]
"""
import json
import os
import statistics
import sys


sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sfx7 import R, clip_name  # noqa: E402

ARC = sys.argv[1]
DRY = '--dry' in sys.argv
def family(cid):
    for pre in ('cmb.swing', 'cmb.hit', 'enemy.die', 'enemy.shot', 'boss', 'voice', 'amb', 'up.bell', 'up.note'):
        if cid.startswith(pre):
            return pre
    return cid.split('.')[0]


FORWARD = ('cmb.swing', 'wpn', 'sig', 'hero', 'cmb.hit')  # the attack families that sat too far forward

path = os.path.join(ARC, 'cues.json')
d = json.load(open(path, encoding='utf-8'))
cues = d['cues']
fam = {}
for q in cues:
    fam.setdefault(family(q['id']), []).append(q.get('gainDb', 0))
median = {k: statistics.median(v) for k, v in fam.items()}
old_clips = set(c for q in cues for c in q['clips'])
rebuilt = 0
for q in cues:
    cid = q['id']; f = family(cid)
    if cid in R:
        q['clips'] = ['Sfx/' + clip_name(cid, k) for k in range(R[cid][2])]
        rebuilt += 1
    q['gainDb'] = float(round(min(0.0, .5 * q.get('gainDb', 0) + .5 * median[f] - (1.5 if f in FORWARD else 0)), 1))

used = set(c for q in cues for c in q['clips'])
gone = sorted(c for c in old_clips - used if c.startswith('Sfx/'))
print('rebuilt cues', rebuilt, 'of', len(cues), '| family gains', {k: round(v, 1) for k, v in sorted(median.items())})
print('old clips no longer used', len(gone))
if DRY:
    sys.exit(0)
with open(path, 'w', encoding='utf-8', newline='\n') as fh:
    json.dump(d, fh, indent=1, ensure_ascii=False)
for c in gone:
    for ext in ('.wav', '.ogg'):
        p = os.path.join(ARC, c + ext)
        for q in (p, p + '.meta'):
            if os.path.exists(q):
                os.remove(q)
