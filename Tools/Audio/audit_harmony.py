"""Score check for build_music.py (NOT a listening review): lists melody-stem notes that start on a beat, last at
least ~an eighth, are not chord tones and sit a minor 2nd above a chord tone or a tritone from the root.
Some are intentional colour (e.g. the phrygian b2 neighbour in boss_ringmaster); read the list, don't chase zero.

Run: python Tools/Audio/audit_harmony.py [piece_id ...]
"""
import os
import sys

sys.path.insert(0, os.path.dirname(__file__))
import build_music as bm

NAMES = 'C C# D Eb E F F# G Ab A Bb B'.split()


def notes_of(pid):
    kw, fn = bm.PIECES[pid]
    s = bm.Song(alloc=False, **kw)
    last, out = [], []
    for name in ('drums', 'bass', 'comp', 'arp', 'stab', 'fx', 'crash_at', 'snare_roll', 'tom_fill', 'riser_into', 'scratch_at', 'vox'):
        setattr(s, name, lambda *a, **k: None)

    def put(stem, clip, beat, *a, **k):
        if stem == 'melody' and last:
            name, midi, dur = last[-1]
            out.append((int(beat // s.bpb), round((beat % s.bpb) * 4), midi, dur, name))
    s.put = put
    real = bm.inst
    bm.inst = lambda name, midi, dur, vel=1.0: last.append((name, midi, dur))
    try:
        fn(s)
    finally:
        bm.inst = real
    return s, out


def main(only):
    for pid in bm.PIECES:
        if only and pid not in only:
            continue
        s, notes = notes_of(pid)
        bad = []
        for bar, st, midi, dur, name in notes:
            if name.startswith('cowbell') or dur < s.beat * 0.45 or st % 4:
                continue
            root, tones = s.chord(bar)
            pcs = {(root + t) % 12 for t in tones}
            pc = midi % 12
            if pc not in pcs and (any((pc - ct) % 12 == 1 for ct in pcs) or (pc - root) % 12 == 6):
                bad.append('bar%d.%d %s over %s%s' % (bar, st, NAMES[pc], NAMES[root], tones))
        print('%-20s %3d notes, %2d flagged %s' % (pid, len(notes), len(bad), sorted(set(bad))[:8]))


if __name__ == '__main__':
    main(set(sys.argv[1:]))
