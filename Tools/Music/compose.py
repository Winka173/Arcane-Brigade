"""Arcane Brigade storybook soundtrack (music v5): composition library.

Every piece is written here as notes (no samples are composed, nothing is transcribed). A Song holds Parts; each Part is one
instrument that belongs to one of the four runtime stems (base / rhythm / melody / intensity). render.py turns each Part into a
one-instrument MIDI file, renders it with FluidSynth + GeneralUser GS, folds the release tail onto the loop start, mixes the
stems and exports sample-aligned looping OGG stems.
"""
import itertools
import math
import random
import zlib

import mido

TPB = 480
PC = {'C': 0, 'D': 2, 'E': 4, 'F': 5, 'G': 7, 'A': 9, 'B': 11}
VEL = {'X': 1.0, 'x': 0.8, 'o': 0.58, 'g': 0.36}

# (program, bank) in GeneralUser GS
I = dict(
    musicbox=(10, 0), celesta=(8, 0), glock=(9, 0), vibes=(11, 0), marimba=(12, 0), xylo=(13, 0), tubular=(14, 0),
    piano=(0, 0), bellpiano=(0, 12), honky=(3, 0), harp=(46, 0), pizz=(45, 0), strings=(49, 0), faststr=(48, 0), trem=(44, 0),
    violin=(40, 0), viola=(41, 0), cello=(42, 0), contrabass=(43, 0), upright=(32, 0), clarinet=(71, 0), bassoon=(70, 0),
    oboe=(68, 0), flute=(73, 0), piccolo=(72, 0), reedorgan=(20, 0), accordion=(21, 8), timpani=(47, 0), taiko=(116, 0),
)
KIT = dict(standard=0, room=8, jazz=32, brush=40, orch=48)
# GM/GS drum keys
KICK, SIDE, TAP, SLAP, SWIRL, HATC, HATP, HATO, RIDE, RIDEB, TAMB = 36, 37, 38, 39, 40, 42, 44, 46, 51, 53, 54
CABASA, MARACA, GUIRO_S, GUIRO_L, CLAVES, WB_HI, WB_LO, TRI_M, TRI_O, SHAKER, CAST = 69, 70, 73, 74, 75, 76, 77, 80, 81, 82, 85


def pn(s):
    """'C#4' -> MIDI number (C4 = 60)."""
    s = s.strip()
    i, acc = 1, 0
    while i < len(s) and s[i] in '#b':
        acc += 1 if s[i] == '#' else -1
        i += 1
    return 12 * (int(s[i:]) + 1) + PC[s[0].upper()] + acc


QUAL = {'': (0, 4, 7), 'm': (0, 3, 7), '7': (0, 4, 7, 10), 'm7': (0, 3, 7, 10), 'maj7': (0, 4, 7, 11), '6': (0, 4, 7, 9),
        'm6': (0, 3, 7, 9), 'm7b5': (0, 3, 6, 10), 'dim': (0, 3, 6), 'dim7': (0, 3, 6, 9), 'sus4': (0, 5, 7), 'sus2': (0, 2, 7),
        '7sus4': (0, 5, 7, 10), '7b9': (0, 4, 7, 10, 13), 'add9': (0, 4, 7, 14), 'madd9': (0, 3, 7, 14), 'aug': (0, 4, 8),
        'm9': (0, 3, 7, 10, 14), 'maj9': (0, 4, 7, 11, 14), '9': (0, 4, 7, 10, 14), 'mmaj7': (0, 3, 7, 11), '5': (0, 7)}


def _pc(txt):
    acc = 0
    if len(txt) > 1 and txt[1] in '#b':
        acc = 1 if txt[1] == '#' else -1
    return (PC[txt[0]] + acc) % 12, 1 + (acc != 0)


class Chord:
    def __init__(self, name):
        self.name = name
        main, _, bass = name.partition('/')
        self.root, k = _pc(main)
        self.ivs = QUAL[main[k:]]
        self.bass = _pc(bass)[0] if bass else self.root

    def pcs(self):
        return [(self.root + i) % 12 for i in self.ivs]

    def off(self, tok):
        """Chord-relative semitone offset for an ostinato token (R 3 5 7 8 9 10 12, '-' prefix = octave down)."""
        neg = tok.startswith('-')
        t = tok.lstrip('-')
        iv = self.ivs
        third = iv[1] if len(iv) > 1 else 4
        fifth = iv[2] if len(iv) > 2 else 7
        seventh = iv[3] if len(iv) > 3 else 12
        table = {'R': 0, '1': 0, '2': 2, '3': third, '4': 5, '5': fifth, '6': 9, '7': seventh, '8': 12,
                 '9': iv[4] if len(iv) > 4 else 14, '10': third + 12, '12': fifth + 12, '15': 24}
        v = table[t]
        return v - 12 if neg else v


def timeline(prog, qpb, bar0=0):
    """'Dm | Bb | Gm7 C7' -> [(start_beat, dur_beats, Chord)]; several chords in a bar split it evenly."""
    out = []
    bars = [b.strip() for b in prog.split('|') if b.strip()]
    for bi, b in enumerate(bars):
        cs = b.split()
        d = qpb / len(cs)
        for k, c in enumerate(cs):
            out.append(((bar0 + bi) * qpb + k * d, d, Chord(c)))
    return out


def seg(tl, qpb, b0, b1):
    return [e for e in tl if b0 * qpb - 1e-6 <= e[0] < b1 * qpb - 1e-6]


def chord_at(tl, t):
    for s, d, c in tl:
        if s - 1e-6 <= t < s + d - 1e-6:
            return c
    return tl[-1][2]


DUR = {'w': 4.0, 'h': 2.0, 'q': 1.0, 'e': 0.5, 's': 0.25, 't': 1 / 3, 'qt': 2 / 3}


def _dur(txt):
    total = 0.0
    for part in txt.split('+'):
        dots = len(part) - len(part.rstrip('.'))
        base = DUR[part.rstrip('.')]
        total += base * (1.5 if dots == 1 else 1.75 if dots == 2 else 1.0)
    return total


def mel(text, start=0.0):
    """'A4:q D5:e r:e | F5:h' -> [(start, dur, [pitches])]; '+' joins pitches into a dyad (A4+C5:q)."""
    out, t = [], start
    for tok in text.replace('|', ' ').split():
        name, _, d = tok.partition(':')
        dur = _dur(d)
        if name != 'r':
            out.append((t, dur, [pn(x) for x in name.split('+')]))
        t += dur
    return out


class Part:
    def __init__(self, name, stem, prog=0, bank=0, drum=False, level=-24.0, pan=0.0, send=0.25, hp=None, lp=None,
                 tj=9.0, vj=6, detune=0, swing=False, transpose=0):
        self.name, self.stem, self.prog, self.bank, self.drum = name, stem, prog, bank, drum
        self.level, self.pan, self.send, self.hp, self.lp = level, pan, send, hp, lp
        self.tj, self.vj, self.detune, self.swing, self.transpose = tj, vj, detune, swing, transpose
        self.notes = []

    def n(self, t, d, p, v):
        v = max(1, min(127, int(round(v))))
        self.notes.append([float(t), float(d), int(p) + self.transpose, v])
        return self.notes[-1]

    def play(self, melody, vel, shape=True, octave=0, gate=1.0):
        """Write a mel() list with a gentle phrase arc (louder toward the middle of each run of notes)."""
        n = len(melody)
        for i, (t, d, ps) in enumerate(melody):
            arc = math.sin(math.pi * (i + 0.5) / max(1, n)) if shape else 0.5
            for p in ps:
                self.n(t, d * gate, p + 12 * octave, vel * (0.88 + 0.18 * arc))


class Song:
    def __init__(self, id, title, key, scenes, qbpm, qpb, bars, json_bpm=None, json_bpb=None, swing=0.0, seed=1,
                 scale=None, desc=''):
        self.id, self.title, self.key, self.scenes = id, title, key, scenes
        self.qbpm, self.qpb, self.bars = qbpm, qpb, bars
        self.json_bpm = json_bpm if json_bpm is not None else qbpm
        self.json_bpb = json_bpb if json_bpb is not None else qpb
        self.swing, self.seed, self.desc = swing, seed, desc
        self.scale = scale or [0, 2, 3, 5, 7, 8, 10]
        self.parts = []
        self.rng = random.Random(seed)

    def part(self, name, stem, inst=None, **kw):
        if inst is not None:
            kw['prog'], kw['bank'] = I[inst]
        p = Part(name, stem, **kw)
        self.parts.append(p)
        return p

    def kit(self, name, stem, kit='brush', **kw):
        return self.part(name, stem, prog=KIT[kit], drum=True, **kw)

    @property
    def beats(self):
        return self.bars * self.qpb

    @property
    def loop_seconds(self):
        return self.beats * 60.0 / self.qbpm

    def tl(self, prog):
        return timeline(prog, self.qpb)


# ----------------------------------------------------------------------------------------------- harmony helpers
def voice(c, lo, hi, n, prev=None):
    """n-note voicing of chord c inside [lo, hi], closest to the previous voicing (smooth voice leading)."""
    pcs = c.pcs()
    order = []
    if len(pcs) > 1:
        order.append(pcs[1])
    order += pcs[3:]
    order.append(pcs[0])
    if len(pcs) > 2:
        order.append(pcs[2])
    sel = order[:n]
    fill = [pcs[0], pcs[2] if len(pcs) > 2 else pcs[0], pcs[1] if len(pcs) > 1 else pcs[0]]
    while len(sel) < n:
        sel.append(fill[(len(sel) - len(order)) % len(fill)])
    options = [[p for p in range(lo, hi + 1) if p % 12 == pc] for pc in sel]
    best = None
    for combo in itertools.product(*options):
        s = sorted(combo)
        if len(set(s)) < n or s[-1] - s[0] > (19 if n <= 4 else 26):
            continue
        if any(b - a < 3 and a < 55 for a, b in zip(s, s[1:])):
            continue
        if prev and len(prev) == n:
            cost = sum(abs(a - b) for a, b in zip(s, prev))
        else:
            cost = abs(sum(s) / n - (lo + hi) / 2) * n
        if best is None or cost < best[0]:
            best = (cost, s)
    if best is None:
        return sorted(o[0] for o in options if o)[:n]
    return best[1]


def bass_pitch(pc, lo):
    p = lo
    while p % 12 != pc:
        p += 1
    return p


def chord_pitches(c, lo, hi):
    pcs = c.pcs()
    return [x for x in range(lo, hi + 1) if x % 12 in pcs]


def scale_step(p, direction, scale):
    q = p + direction
    while q % 12 not in scale:
        q += direction
    return q


# ----------------------------------------------------------------------------------------------- generators
def pad(p, tl, lo, hi, n, vel, restrike_same=False, gate=1.0):
    prev, held = None, None
    for t, d, c in tl:
        v = voice(c, lo, hi, n, prev)
        if held and v == prev and not restrike_same:
            for note in held:
                note[1] += d
        else:
            held = [p.n(t, d * gate, x, vel) for x in v]
        prev = v


def roots(p, tl, lo, pattern, vel, use_bass=True):
    """pattern: [(offset, dur or None, token, vel_mult)] inside every chord span; 'R' is the (slash) bass."""
    for t, d, c in tl:
        b = bass_pitch(c.bass if use_bass else c.root, lo)
        r = bass_pitch(c.root, lo)
        for off, dur, tok, vm in pattern:
            if off >= d - 1e-6:
                continue
            pitch = b if tok == 'R' else r + c.off(tok)
            p.n(t + off, (d - off) if dur is None else min(dur, d - off), pitch, vel * vm)


def ostinato(p, tl, pat, lo, vel, step, gate=0.9, accent=1.22, use_bass=False):
    """Chord-following ostinato on an absolute grid: tokens R 3 5 7 8 10 12 (-5 ...), '!' accent, '.' rest, '-' tie."""
    toks = pat.replace('|', ' ').split()
    last = None
    for t, d, c in tl:
        root = bass_pitch(c.bass if use_bass else c.root, lo)
        for k in range(int(round(t / step)), int(round((t + d) / step))):
            tok = toks[k % len(toks)]
            if tok == '.':
                last = None
                continue
            if tok == '-':
                if last is not None:
                    last[1] += step
                continue
            acc = tok.endswith('!')
            tok = tok.rstrip('!')
            last = p.n(k * step, step * gate, root + c.off(tok), vel * (accent if acc else 1.0))


def comp(p, tl, pat, lo, hi, n, step, vel, gate=0.9):
    """Chord stabs on a grid: pattern chars X x o g . (cycled on the absolute grid)."""
    toks = pat.replace(' ', '').replace('|', '')
    prev = None
    for t, d, c in tl:
        v = voice(c, lo, hi, n, prev)
        prev = v
        for k in range(int(round(t / step)), int(round((t + d) / step))):
            ch = toks[k % len(toks)]
            if ch == '.':
                continue
            for x in v:
                p.n(k * step, step * gate, x, vel * VEL[ch])


def arp(p, tl, lo, hi, idx, step, vel, gate=1.6, accent_first=1.12):
    """Arpeggio over all chord tones in [lo, hi]; idx = indices into that list ('.' rest), cycled on the grid."""
    seq = idx.split()
    for t, d, c in tl:
        cp = chord_pitches(c, lo, hi)
        for k in range(int(round(t / step)), int(round((t + d) / step))):
            tok = seq[k % len(seq)]
            if tok == '.':
                continue
            i = min(int(tok), len(cp) - 1)
            p.n(k * step, step * gate, cp[i], vel * (accent_first if k % len(seq) == 0 else 1.0))


def strum(p, tl, lo, hi, n, vel, spread=0.07, ring=None, every=1, qpb=4):
    prev = None
    for t, d, c in tl:
        if every > 1 and int(round(t / qpb)) % every:
            continue
        v = voice(c, lo, hi, n, prev)
        prev = v
        for i, x in enumerate(v):
            p.n(t + i * spread, (ring or d) - i * spread, x, vel * (0.9 + 0.05 * i))


def drums(p, s, b0, b1, lanes, vel=100, fill=None, fill_every=8, ghost_keep=0.72):
    """lanes: {note: 'x..o|...'} (one pattern per bar, '|' separates bars of a multi-bar cycle)."""
    rng = s.rng
    for b in range(b0, b1):
        use = dict(lanes)
        if fill and (b + 1) % fill_every == 0:
            use.update(fill)
        for note, pat in use.items():
            bars = [x.replace(' ', '') for x in pat.split('|')]
            pt = bars[b % len(bars)]
            if not pt:
                continue
            step = s.qpb / len(pt)
            for k, ch in enumerate(pt):
                if ch == '.':
                    continue
                if ch == 'g' and rng.random() > ghost_keep:
                    continue
                p.n(b * s.qpb + k * step, step * 0.9, note, vel * VEL[ch])


def guide_line(p, tl, lo, hi, vel, scale, rng, pass_prob=0.6):
    """A slow counter-line on chord guide tones (3rds / 7ths) with stepwise passing notes into the next chord."""
    items, prev = [], None
    for t, d, c in tl:
        pcs = c.pcs()
        cands = [x for x in range(lo, hi + 1) if x % 12 in (pcs[1:2] + pcs[3:4] + pcs[:1])]
        g = min(cands, key=lambda x: (abs(x - prev) if prev else abs(x - (lo + hi) / 2), x % 12 != pcs[1]))
        items.append((t, d, g))
        prev = g
    for i, (t, d, g) in enumerate(items):
        nxt = items[(i + 1) % len(items)][2]
        if d >= 2 and nxt != g and rng.random() < pass_prob:
            p.n(t, d - 1, g, vel)
            p.n(t + d - 1, 1, scale_step(g, 1 if nxt > g else -1, scale), vel * 0.86)
        else:
            p.n(t, d, g, vel)


def sprinkle(p, s, tl, b0, b1, lo, hi, vel, n=3, step=0.5, every=2, phase=1):
    """Sparse celesta / glockenspiel figures: a few chord tones every other bar at a varied place in the bar."""
    rng = s.rng
    for b in range(b0, b1):
        if (b - b0) % every != phase:
            continue
        beat = rng.choice([x * 0.5 for x in range(1, int(s.qpb * 2) - n)])
        t = b * s.qpb + beat
        cp = chord_pitches(chord_at(tl, t), lo, hi)
        i = rng.randrange(0, max(1, len(cp) - n))
        seq = cp[i:i + n]
        if rng.random() < 0.4:
            seq = seq[::-1]
        for k, x in enumerate(seq):
            p.n(t + k * step, step * 2.5, x, vel * (1 - 0.08 * k))


def pedal(p, s, b0, b1, pitch, vel, every=2):
    for b in range(b0, b1, every):
        p.n(b * s.qpb, min(every, b1 - b) * s.qpb, pitch, vel)


def roll(p, t_end, pitch, beats, vel0, vel1, step=0.125):
    k = int(round(beats / step))
    for i in range(k):
        p.n(t_end - beats + i * step, step, pitch, vel0 + (vel1 - vel0) * i / max(1, k - 1))


def harmonize_below(melody, scale, steps=2):
    out = []
    for t, d, ps in melody:
        q = ps[0]
        for _ in range(steps):
            q = scale_step(q, -1, scale)
        out.append((t, d, [q]))
    return out


def shift(melody, beats=0.0, semis=0):
    return [(t + beats, d, [x + semis for x in ps]) for t, d, ps in melody]


# ----------------------------------------------------------------------------------------------- MIDI out
def write_part(song, part, path, tail_beats):
    rng = random.Random(song.seed * 7919 + zlib.crc32(part.name.encode()))
    beat_per_ms = song.qbpm / 60000.0
    ch = 9 if part.drum else 0
    ev = [(0, 0, mido.MetaMessage('set_tempo', tempo=int(round(60e6 / song.qbpm))))]
    if not part.drum:
        ev.append((0, 1, mido.Message('control_change', channel=ch, control=0, value=part.bank)))
    ev.append((0, 2, mido.Message('program_change', channel=ch, program=part.prog)))
    for cc, val in ((7, 100), (11, 127), (10, 64), (91, 0), (93, 0)):
        ev.append((0, 3, mido.Message('control_change', channel=ch, control=cc, value=val)))
    if part.detune:
        ev.append((0, 3, mido.Message('pitchwheel', channel=ch, pitch=int(part.detune / 200 * 8191))))
    notes = []
    for t, d, pitch, v in part.notes:
        if part.swing and song.swing:
            fr = t - math.floor(t)
            if abs(fr - 0.5) < 1e-3:
                t = math.floor(t) + song.swing
                d = min(d, 1 - song.swing)
            elif fr < 1e-3 and abs(d - 0.5) < 1e-3:
                d = song.swing * 0.95
        jit = rng.gauss(0, part.tj) * beat_per_ms
        t2 = max(0.0, t + jit)
        v2 = max(1, min(127, v + int(round(rng.gauss(0, part.vj)))))
        notes.append([t2, max(0.03, d), pitch, v2])
    notes.sort()
    last = {}
    for nt in notes:  # same pitch re-struck: end the previous note first
        if nt[2] in last and last[nt[2]][0] + last[nt[2]][1] > nt[0]:
            last[nt[2]][1] = max(0.01, nt[0] - last[nt[2]][0] - 0.005)
        last[nt[2]] = nt
    for t, d, pitch, v in notes:
        if not 0 <= pitch <= 127:
            continue
        on = int(round(t * TPB))
        off = max(on + 1, int(round((t + d) * TPB)))
        ev.append((on, 5, mido.Message('note_on', channel=ch, note=pitch, velocity=v)))
        ev.append((off, 4, mido.Message('note_off', channel=ch, note=pitch, velocity=0)))
    end = int(round((song.beats + tail_beats) * TPB))
    ev.append((end, 9, mido.Message('control_change', channel=ch, control=110, value=0)))
    ev.sort(key=lambda e: (e[0], e[1]))
    mid = mido.MidiFile(ticks_per_beat=TPB)
    tr = mido.MidiTrack()
    mid.tracks.append(tr)
    now = 0
    for tick, _, msg in ev:
        tr.append(msg.copy(time=tick - now))
        now = tick
    tr.append(mido.MetaMessage('end_of_track', time=0))
    mid.save(path)
