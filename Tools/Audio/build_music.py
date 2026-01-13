"""Renders the Arcane Brigade adaptive soundtrack.

Each composition is rendered as 4 sample-aligned stems with identical length, tempo and loop point:
  base      bass + harmonic floor (always on)
  rhythm    percussion + off-beat comping
  melody    lead line (+ answers)
  intensity extra energy layer (arps, doubled lines, fills) driven by enemy density / boss
Tails wrap to the loop start and reverb is circular, so loops have no seam and no cut-off phrase.
All material is original and synthesised here (see synth.py); no samples, no third-party audio.

Run: python3 Tools/Audio/build_music.py [piece_id ...]   (needs scipy, numpy, ffmpeg with libvorbis)
"""
import json
import os
import subprocess
import sys

import numpy as np

sys.path.insert(0, os.path.dirname(__file__))
from synth import *
from synth import _stable

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
OUT = os.path.join(ROOT, 'Assets', 'ArcaneBrigade', 'Resources', 'Audio', 'Arcane', 'Music')

SCALES = {
    'dorian': [0, 2, 3, 5, 7, 9, 10], 'minor': [0, 2, 3, 5, 7, 8, 10], 'major': [0, 2, 4, 5, 7, 9, 11],
    'lydian': [0, 2, 4, 6, 7, 9, 11], 'phrygdom': [0, 1, 4, 5, 7, 8, 10], 'harmmin': [0, 2, 3, 5, 7, 8, 11],
}
CHORDS = {'m': [0, 3, 7], 'M': [0, 4, 7], 'm7': [0, 3, 7, 10], '7': [0, 4, 7, 10], 'M7': [0, 4, 7, 11],
          'dim': [0, 3, 6], 'sus': [0, 5, 7], 'm9': [0, 3, 7, 10, 14], 'M9': [0, 4, 7, 11, 14]}

# 4/4 two-bar rhythm cells (beat, length) and 3/4 cells
RH = {
    'lilt': [(0, 1.5), (1.5, .5), (2, 1), (3, 1), (4, 1.5), (5.5, .5), (6, 1), (7, 1)],
    'sync': [(0, .5), (.5, .5), (1, 1), (2.5, .5), (3, 1), (4, .5), (4.5, .5), (5, 1.5), (6.5, .5), (7, 1)],
    'long': [(0, 2), (2, 1), (3, 1), (4, 2), (6, 1), (7, 1)],
    'run': [(k * .5, .5) for k in range(16)],
    'call': [(0, 1), (1, .5), (1.5, .5), (2, 2), (4, 1), (5, .5), (5.5, .5), (6, 2)],
    'stab': [(0, .5), (1, .5), (2.5, .5), (3.5, .5), (4, .5), (5, .5), (6.5, .5), (7, 1)],
    'march': [(0, 1), (1, 1), (2, 1), (3, .5), (3.5, .5), (4, 1), (5, 1), (6, 1), (7, 1)],
    'w1': [(0, 1.5), (1.5, .5), (2, 1), (3, 2), (5, 1)],
    'w2': [(0, 1), (1, 1), (2, 1), (3, 1.5), (4.5, .5), (5, 1)],
    'w3': [(0, 3), (3, 1.5), (4.5, .5), (5, 1)],
}


class Piece:
    def __init__(self, **kw):
        self.__dict__.update(kw)
        self.bpb = kw.get('bpb', 4)
        self.beat = 60.0 / self.bpm
        self.total_beats = self.bars * self.bpb
        self.n = int(round(self.total_beats * self.beat * SR))
        self.loop_seconds = self.n / SR

    def midi(self, deg, octave=5):
        sc = SCALES[self.scale]
        return self.key + 12 * (octave - 5) + sc[deg % 7] + 12 * (deg // 7) + 60 - 60

    def chord_at(self, bar):
        progs = self.progs[self.section_of(bar)]
        off, q = progs[bar % len(progs)]
        return self.key + off, CHORDS[q]

    def section_of(self, bar):
        return self.sections[(bar // 8) % len(self.sections)][0]

    def melody_cells(self, bar):
        sec = self.sections[(bar // 8) % len(self.sections)]
        return sec[1][(bar % 8) // 2]


_cache = {}


def inst(name, midi, dur, vel=1.0):
    k = (name, round(float(midi), 2), round(dur, 3), round(vel, 2))
    if k not in _cache:
        rng = np.random.default_rng(int(midi * 100) + int(dur * 1000))
        f = float(mtof(midi))
        fn = dict(pizz=pizz, upright=upright, marimba=marimba, vibes=vibes, celesta=celesta, musicbox=musicbox, bell=bell)
        if name in fn:
            y = fn[name](f, min(max(dur * 1.4, 0.35), 2.2) if name in ('vibes', 'bell', 'celesta') else min(max(dur * 1.2, 0.3), 1.2), vel, rng)
        elif name == 'clarinet':
            y = clarinet(f, max(dur, 0.12), vel, vib=1.0, rng=rng)
        elif name == 'bassclar':
            y = clarinet(f, max(dur, 0.12), vel, vib=0.3, bright=0.55, rng=rng, breath=0.05)
        elif name == 'pad':
            y = saw_pad(f, dur + 0.4, vel, cutoff=1500, a=0.35, r=0.5, rng=rng)
        elif name == 'dreampad':
            y = saw_pad(f, dur + 0.6, vel, detune=0.011, voices=4, cutoff=1100, a=0.7, r=0.9, rng=rng)
        else:
            raise KeyError(name)
        _cache[k] = y
    return _cache[k]


def swing_off(p, beat_pos):
    """Delay off-beat eighths by swing ratio."""
    if not getattr(p, 'swing', 0):
        return 0.0
    frac = beat_pos % 1.0
    return p.swing * p.beat * 0.5 if abs(frac - 0.5) < 1e-6 else 0.0


def snap_to_chord(m, chord_root, tones, strong):
    if not strong:
        return m
    cands = []
    for t in tones:
        pc = (chord_root + t) % 12
        for o in range(-2, 3):
            cands.append(pc + 12 * ((int(m) // 12) + o))
    return min(cands, key=lambda c: abs(c - m))


def render_stems(p):
    rng = np.random.default_rng(_stable(p.id))
    bufs = {k: np.zeros((p.n, 2)) for k in ('base', 'rhythm', 'melody', 'intensity')}

    def put(stem, clip, beat_pos, gain=1.0, pan=0.0, human=True):
        t = beat_pos * p.beat + swing_off(p, beat_pos)
        if human:
            t += rng.normal(0, 0.004)
            gain *= 1 + rng.normal(0, 0.06)
        place(bufs[stem], clip, t % p.loop_seconds, gain, pan, wrap=True)

    for bar in range(p.bars):
        b0 = bar * p.bpb
        root, tones = p.chord_at(bar)
        sec = p.section_of(bar)
        last_bar = bar == p.bars - 1
        e = p.energy.get(sec, 1.0) if hasattr(p, 'energy') else 1.0

        # ---------------------------------------------------------- base
        p.bass(put, inst, bar, b0, root, tones, sec, e)
        # ---------------------------------------------------------- rhythm
        p.drums(put, inst, bar, b0, root, tones, sec, last_bar, rng)
        # ---------------------------------------------------------- intensity
        p.extra(put, inst, bar, b0, root, tones, sec, last_bar, rng)

    # ------------------------------------------------------------ melody
    for cell_bar in range(0, p.bars, 2):
        rhythm, degs, opts = p.melody_cells(cell_bar)
        pattern = RH[rhythm]
        if len(degs) != len(pattern):
            raise ValueError('%s: cell at bar %d has %d degrees for %d-note rhythm %s' % (p.id, cell_bar, len(degs), len(pattern), rhythm))
        lead = opts.get('inst', p.lead)
        oct_ = opts.get('oct', 5)
        sec = p.section_of(cell_bar)
        for (start, length), d in zip(pattern, degs):
            if d is None:
                continue
            bar = cell_bar + int(start // p.bpb)
            root, tones = p.chord_at(bar)
            m = p.midi(d, oct_)
            strong = (start % p.bpb) == 0 and length >= 1
            m = snap_to_chord(m, root, tones, strong)
            dur = length * p.beat * (0.92 if lead in ('clarinet',) else 1.0)
            vel = 0.85 + 0.15 * (start % p.bpb == 0)
            put('melody', inst(lead, m, dur, vel), cell_bar * p.bpb + start, p.mel_gain, -0.1)
            if opts.get('double'):
                put('intensity', inst(opts['double'], m + 12, dur, vel), cell_bar * p.bpb + start, p.mel_gain * 0.5, 0.25)

    # ------------------------------------------------------- finish stems
    reverb = dict(base=(0.10, 0.9), rhythm=(0.08, 0.6), melody=(0.18, 1.4), intensity=(0.16, 1.2))
    targets = dict(base=-22.0, rhythm=-25.0, melody=-22.5, intensity=-25.5)
    out = {}
    for k, b in bufs.items():
        wet, sec_ = reverb[k]
        if p.id.startswith('shop_dream'):
            wet, sec_ = wet * 1.8, sec_ * 1.6
        b = circular_reverb(b, wet, sec_, seed=_stable((p.id, k)) % 1000)
        for c in range(2):  # gentle high cut / DC block, filtered on a 3x tiled copy so the loop point stays continuous
            tiled = np.tile(b[:, c], 3)
            b[:, c] = hp(lp(tiled, 11000), 28, 1)[p.n:2 * p.n]
        out[k] = normalize_rms(b, targets[k] + p.trim.get(k, 0.0), peak_db=-3.0)
    return out


# =============================================================== piece vocab
def walking_bass(put, inst, bar, b0, root, tones, sec, e):
    r = root - 24
    t3 = tones[1]
    pattern = [0, t3, 7, 9 if t3 == 4 else 10]
    for k, iv in enumerate(pattern[:4]):
        put('base', inst('upright', r + iv, 0.5, 0.9 if k == 0 else 0.7), b0 + k, 1.0, -0.05)


def tuba_bass(put, inst, bar, b0, root, tones, sec, e):
    r = root - 24
    put('base', inst('upright', r, 0.7, 1.0), b0, 1.0)
    put('base', inst('upright', r + 7, 0.5, 0.75), b0 + 1.5, 1.0)
    put('base', inst('upright', r + 7, 0.6, 0.85), b0 + 2, 1.0)
    put('base', inst('upright', r + 12, 0.4, 0.6), b0 + 3.5, 1.0)
    put('base', inst('pad', root - 12, 3.8, 0.2), b0, 0.5, 0.0, human=False)


def ostinato_bass(put, inst, bar, b0, root, tones, sec, e):
    r = root - 24
    seq_ = [0, 0, 12, 0, 10, 0, 12, 7]
    for k, iv in enumerate(seq_):
        put('base', inst('bassclar', r + iv, 0.28, 0.9 if k % 2 == 0 else 0.65), b0 + k * 0.5, 0.65, 0.0)
    put('base', inst('upright', r, 0.5, 0.8), b0, 0.7)


def pedal_pad_bass(put, inst, bar, b0, root, tones, sec, e):
    r = root - 24
    put('base', inst('upright', r, 1.2, 0.9), b0, 0.9)
    for t in tones[:3]:
        put('base', inst('dreampad', root + t - 12, 2.4, 0.35), b0, 0.4, 0.0, human=False)



# ---- drums
def drums_march(put, inst, bar, b0, root, tones, sec, last_bar, rng):
    put('rhythm', felt_kick(0.8), b0, 0.9)
    put('rhythm', felt_kick(0.7), b0 + 2, 0.8)
    put('rhythm', brush(0.9, 0.11, bar), b0 + 1, 0.8)
    put('rhythm', brush(0.9, 0.11, bar + 5), b0 + 3, 0.8)
    for k in range(4):
        put('rhythm', woodblock(880 if k % 2 else 760, 0.45, 0.07), b0 + k + 0.5, 0.6, 0.25)
    for ch in tones[:3]:
        put('rhythm', inst('pizz', root + ch, 0.25, 0.5), b0 + 1.5, 0.45, -0.3)
        put('rhythm', inst('pizz', root + ch, 0.25, 0.5), b0 + 3.5, 0.45, -0.3)
    if last_bar:
        for k in range(4):
            put('rhythm', toy_tom(200 - 18 * k, 0.55), b0 + 3 + k * 0.25, 0.7, -0.3 + 0.2 * k)


def drums_swing(put, inst, bar, b0, root, tones, sec, last_bar, rng):
    put('rhythm', felt_kick(0.55), b0, 0.8)
    put('rhythm', felt_kick(0.4), b0 + 2.5, 0.6)
    for k in range(4):
        put('rhythm', brush(0.7, 0.09, bar * 3 + k, 6000), b0 + k, 0.55)
        put('rhythm', shaker(0.8, 0.07, bar + k), b0 + k + 0.5, 0.45, 0.3)
    put('rhythm', woodblock(700, 0.4, 0.06), b0 + 1, 0.5, -0.3)
    put('rhythm', woodblock(700, 0.4, 0.06), b0 + 3, 0.5, -0.3)
    for ch in tones[1:4]:
        put('rhythm', inst('vibes', root + ch, 0.5, 0.35), b0 + 1.5, 0.35, -0.25)
        put('rhythm', inst('vibes', root + ch, 0.5, 0.3), b0 + 3.5 if bar % 2 else b0 + 2.5, 0.3, 0.25)


def drums_tick(put, inst, bar, b0, root, tones, sec, last_bar, rng):
    put('rhythm', felt_kick(0.9), b0, 0.9)
    put('rhythm', felt_kick(0.8), b0 + 1.5 if bar % 2 else b0 + 2, 0.8)
    for k in range(8):
        put('rhythm', clock_tick(0.5 + 0.2 * (k % 2 == 0), k % 2 == 0), b0 + k * 0.5, 0.55, -0.35 + 0.1 * k)
    put('rhythm', brush(1.0, 0.1, bar), b0 + 1, 0.8)
    put('rhythm', brush(1.0, 0.1, bar + 9), b0 + 3, 0.9)
    for ch in tones[:3]:
        put('rhythm', inst('pizz', root + ch + 12, 0.2, 0.45), b0 + 0.5, 0.35, 0.3)
        put('rhythm', inst('pizz', root + ch + 12, 0.2, 0.45), b0 + 2.5, 0.35, -0.3)
    if last_bar:
        for k in range(8):
            put('rhythm', toy_tom(260 - 16 * k, 0.5 + 0.05 * k), b0 + 2 + k * 0.25, 0.8, -0.4 + 0.1 * k)


def drums_drive(put, inst, bar, b0, root, tones, sec, last_bar, rng):
    for k in range(4):
        put('rhythm', felt_kick(0.85 if k % 2 == 0 else 0.6), b0 + k, 0.9)
    for k in range(8):
        put('rhythm', shaker(0.9, 0.07, bar * 8 + k), b0 + k * 0.5 + 0.25, 0.5, 0.35)
        put('rhythm', metal_hat(0.6, 0.05, k), b0 + k * 0.5, 0.4, -0.3)
    put('rhythm', brush(1.0, 0.14, bar, 5000), b0 + 1, 1.0)
    put('rhythm', brush(1.0, 0.14, bar + 4, 5000), b0 + 3, 1.0)
    put('rhythm', toy_tom(170, 0.6), b0 + 2.75, 0.7, -0.4)
    if last_bar or bar % 8 == 7:
        for k in range(8):
            put('rhythm', toy_tom(240 - 14 * k, 0.55 + 0.05 * k), b0 + 2 + k * 0.25, 0.85, -0.5 + 0.14 * k)
        put('rhythm', gong(note_m(40), 2.0, 0.5), b0 + 4 - 4 + 0, 0.3, 0.0)


def note_m(m):
    return float(mtof(m))


def drums_boss(put, inst, bar, b0, root, tones, sec, last_bar, rng):
    put('rhythm', felt_kick(1.0), b0, 1.0)
    put('rhythm', felt_kick(0.9), b0 + 0.75, 0.9)
    put('rhythm', felt_kick(1.0), b0 + 2, 1.0)
    put('rhythm', toy_tom(110, 0.9), b0 + 3, 0.9, -0.3)
    put('rhythm', toy_tom(140, 0.8), b0 + 3.5, 0.9, 0.3)
    for k in range(8):
        put('rhythm', clock_tick(0.55, k % 2 == 0), b0 + k * 0.5, 0.5, 0.0)
    put('rhythm', brush(1.0, 0.16, bar, 4500), b0 + 1, 1.0)
    put('rhythm', brush(1.1, 0.16, bar + 2, 4500), b0 + 3, 1.0)
    if bar % 4 == 0:
        put('rhythm', gong(note_m(root - 24), 2.6, 0.7), b0, 0.5, 0.0, human=False)
    if bar % 8 == 7:
        for k in range(12):
            put('rhythm', toy_tom(300 - 15 * k, 0.5 + 0.04 * k), b0 + 2 + k / 6, 0.85, -0.5 + 0.1 * k)


def drums_waltz(put, inst, bar, b0, root, tones, sec, last_bar, rng):
    put('rhythm', felt_kick(0.35), b0, 0.5)
    for k in (1, 2):
        put('rhythm', brush(0.5, 0.1, bar + k, 4000), b0 + k, 0.5, 0.2)
    put('rhythm', clock_tick(0.4, True), b0 + 1, 0.4, -0.3)
    put('rhythm', clock_tick(0.35, False), b0 + 2, 0.4, 0.3)
    for ch in tones[1:4]:
        put('rhythm', inst('musicbox', root + ch + 12, 0.5, 0.35), b0 + 1, 0.3, -0.2)
        put('rhythm', inst('musicbox', root + ch + 12, 0.5, 0.3), b0 + 2, 0.3, 0.3)


# ---- extra / intensity layers
def extra_arp(put, inst, bar, b0, root, tones, sec, last_bar, rng, name='marimba', step=0.5, gain=0.5):
    pool = [root + t + o for o in (12, 24) for t in tones]
    for k in range(int(4 / step)):
        m = pool[(k * 2 + bar) % len(pool)] if k % 2 else pool[(k // 2) % len(pool)]
        put('intensity', inst(name, m, step * 0.9 * 0.6, 0.55 + 0.25 * (k % 4 == 0)), b0 + k * step, gain, -0.3 + 0.6 * ((k % 4) / 3))


def extra_pulse(put, inst, bar, b0, root, tones, sec, last_bar, rng):
    for k in range(4):
        for ch in tones[:3]:
            put('intensity', inst('pad', root + ch, 0.9, 0.18), b0 + k, 0.5, 0.0, human=False)
    for k in range(8):
        put('intensity', inst('pizz', root + tones[k % 3] + 12, 0.2, 0.6), b0 + k * 0.5, 0.45, -0.4 + 0.1 * k)


def extra_boss(put, inst, bar, b0, root, tones, sec, last_bar, rng):
    for k, iv in enumerate([0, 1, 0, 6, 0, 1, 7, 6]):
        put('intensity', inst('celesta', root + 12 + iv, 0.3, 0.7), b0 + k * 0.5, 0.45, -0.4 + 0.1 * k)
    put('intensity', inst('dreampad', root, 4.0, 0.3), b0, 0.5, 0.0, human=False)
    if bar % 2 == 0:
        for ch in tones[:3]:
            put('intensity', inst('bell', root + ch + 12, 1.0, 0.4), b0, 0.35, 0.0)


def extra_dream(put, inst, bar, b0, root, tones, sec, last_bar, rng):
    pool = [root + t + 12 for t in tones] + [root + 24]
    for k in range(3):
        put('intensity', inst('celesta', pool[(bar + k * 2) % len(pool)], 0.8, 0.5), b0 + k, 0.5, -0.5 + 0.5 * k)
    put('intensity', inst('vibes', root + tones[2] + 24, 1.6, 0.35), b0 + 0.5, 0.35, 0.4)


def extra_shop(put, inst, bar, b0, root, tones, sec, last_bar, rng):
    # warm gold: register-bell off-beats and a pizzicato answer
    for k in (0.5, 2.5):
        put('intensity', inst('bell', root + tones[2] + 24, 0.4, 0.4), b0 + k, 0.35, 0.4)
    for k, iv in enumerate([0, 4, 7, 12]):
        put('intensity', inst('marimba', root + iv + 12, 0.3, 0.6), b0 + 1.0 + k * 0.5, 0.45, -0.3 + 0.2 * k)


# =============================================================== compositions
def cells(**kw):
    return kw


PIECES = []


def add(**kw):
    PIECES.append(Piece(**kw))


# 1. MENU — "Curtain Call" : D dorian, lazy swing, clarinet & vibes
add(id='menu_curtain', title='Curtain Call', bpm=88, bars=32, key=62, scale='dorian', swing=0.28, lead='clarinet', mel_gain=0.9,
    scenes=['menu'], family='stage', trim={'melody': 0.5},
    progs={'A': [(0, 'm7'), (0, 'm7'), (5, '7'), (5, '7'), (3, 'M7'), (10, '7'), (0, 'm7'), (7, '7')],
           'B': [(8, 'M7'), (8, 'M7'), (3, 'M7'), (3, 'M7'), (5, '7'), (5, '7'), (7, 'm7'), (7, '7')]},
    sections=[('A', [('lilt', [4, 6, 5, 4, 2, 4, 3, 2], {}), ('long', [4, 5, 6, 7, 6, 4], {}), ('lilt', [4, 6, 5, 4, 2, 4, 3, 2], {}), ('call', [1, 2, 3, 4, 5, 4, 3, 1], {})]),
              ('A', [('lilt', [4, 6, 5, 4, 2, 4, 3, 2], {'inst': 'vibes'}), ('long', [4, 5, 6, 8, 7, 4], {'inst': 'vibes'}), ('sync', [4, 5, 6, 7, 6, 5, 4, 3, 2, 1], {}), ('long', [2, 3, 4, 2, 1, 0], {})]),
              ('B', [('call', [7, 6, 5, 5, 4, 3, 2, 1], {}), ('sync', [2, 3, 4, 5, 6, 5, 4, 3, 2, 3], {}), ('call', [8, 7, 6, 6, 5, 4, 3, 2], {}), ('long', [4, 3, 2, 1, 0, -1], {})]),
              ('A', [('lilt', [4, 6, 5, 4, 2, 4, 3, 2], {'double': 'celesta'}), ('long', [4, 5, 6, 7, 6, 4], {'double': 'celesta'}), ('sync', [8, 7, 6, 5, 4, 3, 2, 1, 2, 3], {}), ('long', [0, 2, 4, 2, 1, 0], {})])],
    bass=walking_bass, drums=drums_swing, extra=lambda *a: extra_arp(*a, name='celesta', step=1.0, gain=0.5),
    energy={})

# 2. CHARACTER SELECT — "Dressing Room" : F major, soft music-box
add(id='select_dressing', title='Dressing Room', bpm=76, bars=16, key=65, scale='major', swing=0.12, lead='celesta', mel_gain=0.9,
    scenes=['select'], family='stage', trim={'melody': 0.0},
    progs={'A': [(0, 'M7'), (0, 'M7'), (9, 'm7'), (9, 'm7'), (2, 'm7'), (7, '7'), (0, 'M7'), (7, 'sus')],
           'B': [(5, 'M7'), (5, 'M7'), (0, 'M7'), (0, 'M7'), (2, 'm7'), (7, '7'), (0, 'M7'), (0, 'M7')]},
    sections=[('A', [('lilt', [4, 5, 6, 5, 4, 2, 4, 5], {}), ('long', [6, 5, 4, 2, 4, 1], {}), ('lilt', [4, 5, 6, 7, 6, 4, 5, 6], {}), ('long', [7, 6, 5, 3, 4, 0], {})]),
              ('B', [('call', [8, 7, 6, 4, 5, 4, 3, 2], {'inst': 'vibes'}), ('long', [4, 5, 6, 4, 2, 4], {'inst': 'vibes'}), ('call', [8, 9, 8, 7, 6, 5, 4, 3], {}), ('long', [4, 2, 1, 0, 2, 0], {})])],
    bass=tuba_bass, drums=drums_swing, extra=lambda *a: extra_arp(*a, name='musicbox', step=1.0, gain=0.4),
    energy={})

# 3. COMBAT EARLY — "Toys on Parade" : A minor march, marimba
add(id='combat_toys', title='Toys on Parade', bpm=112, bars=32, key=57, scale='minor', swing=0.0, lead='marimba', mel_gain=1.0,
    scenes=['combat_early'], family='toyroom', trim={},
    progs={'A': [(0, 'm'), (0, 'm'), (8, 'M'), (8, 'M'), (5, 'M'), (7, 'M'), (0, 'm'), (7, '7')],
           'B': [(3, 'M'), (3, 'M'), (8, 'M'), (7, 'M'), (0, 'm'), (5, 'M'), (7, '7'), (7, '7')]},
    sections=[('A', [('march', [4, 4, 5, 4, 2, 4, 5, 6, 4], {}), ('stab', [4, 5, 4, 2, 4, 5, 6, 4], {}), ('march', [4, 4, 5, 4, 2, 4, 5, 6, 4], {}), ('call', [7, 6, 5, 4, 3, 2, 1, 0], {})]),
              ('A', [('march', [4, 4, 5, 4, 2, 4, 5, 6, 4], {'double': 'celesta'}), ('sync', [4, 5, 6, 7, 6, 5, 4, 5, 4, 2], {}), ('march', [7, 7, 6, 7, 9, 7, 6, 5, 4], {}), ('call', [5, 4, 3, 2, 3, 2, 1, 0], {})]),
              ('B', [('lilt', [2, 4, 6, 4, 5, 7, 6, 4], {'inst': 'clarinet'}), ('long', [7, 6, 5, 4, 3, 2], {'inst': 'clarinet'}), ('lilt', [2, 4, 6, 7, 9, 7, 6, 4], {'inst': 'clarinet'}), ('call', [5, 4, 3, 2, 1, 2, 1, 0], {})]),
              ('A', [('march', [4, 4, 5, 4, 2, 4, 5, 6, 4], {'double': 'celesta'}), ('stab', [7, 6, 5, 4, 7, 6, 5, 4], {}), ('march', [7, 7, 8, 7, 5, 7, 8, 9, 7], {'double': 'celesta'}), ('long', [6, 4, 2, 0, 1, 0], {})])],
    bass=tuba_bass, drums=drums_march, extra=lambda *a: extra_arp(*a, name='marimba', step=0.5, gain=0.45),
    energy={})

# 4. COMBAT MID — "Clockwork Conga" : E minor, ticking, clarinet
add(id='combat_clockwork', title='Clockwork Conga', bpm=124, bars=32, key=64, scale='dorian', swing=0.0, lead='clarinet', mel_gain=0.95,
    scenes=['combat_mid'], family='workshop', trim={'melody': 0.5},
    progs={'A': [(0, 'm'), (0, 'm'), (10, 'M'), (10, 'M'), (8, 'M'), (7, 'M'), (0, 'm'), (5, 'M')],
           'B': [(5, 'M'), (5, 'M'), (0, 'm'), (0, 'm'), (8, 'M'), (10, 'M'), (7, 'M'), (7, '7')]},
    sections=[('A', [('sync', [4, 4, 5, 6, 4, 2, 4, 5, 3, 2], {}), ('stab', [4, 5, 4, 3, 2, 3, 4, 2], {}), ('sync', [4, 4, 5, 6, 4, 7, 6, 5, 3, 2], {}), ('call', [3, 2, 1, 2, 3, 2, 1, 0], {})]),
              ('A', [('sync', [4, 4, 5, 6, 4, 2, 4, 5, 3, 2], {'inst': 'marimba'}), ('stab', [4, 5, 4, 3, 2, 3, 4, 2], {'inst': 'marimba'}), ('run', [4, 5, 4, 3, 4, 5, 6, 5, 4, 3, 4, 5, 4, 3, 2, 3], {}), ('call', [3, 2, 1, 2, 3, 2, 1, 0], {})]),
              ('B', [('long', [7, 6, 5, 4, 5, 6], {}), ('call', [7, 6, 5, 4, 3, 4, 2, 1], {}), ('long', [7, 8, 9, 7, 6, 5], {'double': 'celesta'}), ('sync', [7, 6, 5, 4, 3, 2, 1, 2, 1, 0], {})]),
              ('A', [('sync', [4, 4, 5, 6, 4, 2, 4, 5, 3, 2], {'double': 'celesta'}), ('stab', [4, 5, 4, 3, 2, 3, 4, 2], {}), ('run', [7, 8, 7, 6, 7, 8, 9, 8, 7, 6, 7, 6, 5, 4, 3, 4], {'double': 'celesta'}), ('long', [4, 2, 0, 2, 1, 0], {})])],
    bass=ostinato_bass, drums=drums_tick, extra=extra_pulse, energy={})

# 5. COMBAT LATE — "Midnight Overtime" : G minor, relentless
add(id='combat_overtime', title='Midnight Overtime', bpm=138, bars=32, key=67, scale='minor', swing=0.0, lead='clarinet', mel_gain=0.95,
    scenes=['combat_late'], family='office', trim={'melody': 0.5},
    progs={'A': [(0, 'm'), (0, 'm'), (8, 'M'), (8, 'M'), (3, 'M'), (10, 'M'), (7, 'm'), (7, '7')],
           'B': [(8, 'M'), (7, 'm'), (0, 'm'), (3, 'M'), (8, 'M'), (7, 'm'), (10, 'M'), (7, '7')]},
    sections=[('A', [('stab', [4, 4, 5, 4, 2, 4, 5, 7], {}), ('call', [6, 5, 4, 2, 4, 3, 2, 1], {}), ('stab', [4, 4, 5, 4, 2, 4, 5, 9], {}), ('sync', [8, 7, 6, 5, 4, 3, 2, 1, 2, 0], {})]),
              ('A', [('run', [4, 5, 4, 2, 4, 5, 6, 5, 4, 2, 4, 5, 6, 7, 6, 5], {'inst': 'marimba'}), ('call', [6, 5, 4, 2, 4, 3, 2, 1], {}), ('stab', [4, 4, 5, 4, 2, 4, 5, 9], {'double': 'celesta'}), ('sync', [8, 7, 6, 5, 4, 3, 2, 1, 2, 0], {})]),
              ('B', [('sync', [7, 7, 8, 7, 5, 7, 9, 8, 7, 5], {}), ('call', [6, 5, 4, 3, 2, 3, 4, 2], {}), ('sync', [7, 7, 8, 9, 7, 9, 11, 9, 8, 7], {'double': 'celesta'}), ('long', [8, 6, 4, 2, 1, 0], {})]),
              ('A', [('run', [4, 5, 4, 2, 4, 5, 6, 5, 4, 2, 4, 5, 6, 7, 8, 9], {'double': 'marimba'}), ('call', [8, 7, 6, 4, 6, 5, 4, 2], {}), ('stab', [4, 4, 5, 4, 2, 4, 5, 9], {'double': 'celesta'}), ('sync', [8, 7, 6, 5, 4, 3, 2, 1, 0, 0], {})])],
    bass=ostinato_bass, drums=drums_drive, extra=lambda *a: extra_arp(*a, name='marimba', step=0.25, gain=0.45), energy={})

# 6. BOSS — "Ringmaster of Drawers" : D phrygian-dominant, bass clarinet + gong
add(id='boss_ringmaster', title='Ringmaster of Drawers', bpm=148, bars=32, key=62, scale='phrygdom', swing=0.0, lead='bassclar', mel_gain=1.0,
    scenes=['boss'], family='all', trim={},
    progs={'A': [(0, 'm'), (0, 'm'), (1, 'M'), (0, 'm'), (0, 'm'), (6, 'dim'), (1, 'M'), (7, '7')],
           'B': [(5, 'm'), (5, 'm'), (1, 'M'), (1, 'M'), (0, 'm'), (6, 'dim'), (7, '7'), (7, '7')]},
    sections=[('A', [('stab', [0, 0, 1, 0, 3, 2, 1, 0], {}), ('call', [4, 3, 2, 3, 1, 0, 1, 0], {}), ('stab', [0, 0, 1, 0, 3, 5, 4, 3], {'inst': 'clarinet'}), ('sync', [2, 1, 0, 1, 2, 3, 4, 3, 1, 0], {'inst': 'clarinet'})]),
              ('A', [('stab', [7, 7, 8, 7, 10, 9, 8, 7], {'inst': 'clarinet'}), ('call', [11, 10, 9, 10, 8, 7, 8, 7], {'inst': 'clarinet'}), ('stab', [0, 0, 1, 0, 3, 5, 4, 3], {'double': 'celesta'}), ('sync', [4, 3, 2, 1, 2, 3, 4, 5, 4, 3], {})]),
              ('B', [('long', [7, 6, 5, 4, 5, 4], {'inst': 'clarinet'}), ('call', [4, 3, 2, 1, 2, 1, 0, 1], {}), ('long', [7, 8, 9, 8, 7, 6], {'inst': 'clarinet', 'double': 'celesta'}), ('stab', [4, 3, 2, 1, 4, 3, 2, 1], {})]),
              ('A', [('stab', [7, 7, 8, 7, 10, 9, 8, 7], {'inst': 'clarinet', 'double': 'celesta'}), ('call', [11, 10, 9, 10, 8, 7, 8, 7], {'inst': 'clarinet', 'double': 'celesta'}), ('stab', [0, 0, 1, 0, 3, 5, 4, 3], {'double': 'celesta'}), ('sync', [4, 3, 2, 1, 0, 1, 2, 1, 0, 0], {})])],
    bass=ostinato_bass, drums=drums_boss, extra=extra_boss, energy={})

# 7. GOLD SHOP — "Cash Register Rag" : C major, warm swing, marimba
add(id='shop_gold', title='Cash Register Rag', bpm=104, bars=32, key=60, scale='major', swing=0.3, lead='marimba', mel_gain=0.9,
    scenes=['shop_gold'], family='toyroom', trim={},
    progs={'A': [(0, 'M7'), (9, '7'), (2, 'm7'), (7, '7'), (0, 'M7'), (5, 'M7'), (2, 'm7'), (7, '7')],
           'B': [(5, 'M7'), (5, 'M7'), (0, 'M7'), (9, '7'), (2, 'm7'), (7, '7'), (0, 'M7'), (7, '7')]},
    sections=[('A', [('sync', [4, 5, 4, 2, 4, 5, 6, 4, 2, 0], {}), ('lilt', [4, 2, 1, 2, 4, 5, 4, 2], {}), ('sync', [4, 5, 4, 2, 4, 5, 6, 7, 6, 4], {}), ('call', [5, 4, 3, 2, 1, 2, 3, 0], {})]),
              ('A', [('sync', [4, 5, 4, 2, 4, 5, 6, 4, 2, 0], {'inst': 'vibes'}), ('lilt', [4, 2, 1, 2, 4, 5, 4, 2], {'inst': 'vibes'}), ('sync', [7, 6, 5, 4, 5, 6, 7, 8, 7, 5], {}), ('call', [6, 5, 4, 2, 4, 2, 1, 0], {})]),
              ('B', [('call', [7, 6, 5, 4, 5, 4, 3, 2], {'inst': 'clarinet'}), ('long', [4, 5, 6, 5, 4, 2], {'inst': 'clarinet'}), ('call', [7, 8, 7, 6, 5, 6, 5, 4], {'inst': 'clarinet'}), ('long', [4, 2, 1, 0, 1, 0], {'inst': 'clarinet'})]),
              ('A', [('sync', [4, 5, 4, 2, 4, 5, 6, 4, 2, 0], {'double': 'bell'}), ('lilt', [4, 2, 1, 2, 4, 5, 4, 2], {}), ('sync', [4, 5, 4, 2, 4, 5, 6, 7, 6, 4], {'double': 'bell'}), ('long', [2, 4, 2, 1, 0, 0], {})])],
    bass=walking_bass, drums=drums_swing, extra=extra_shop, energy={})

# 8. DREAM SHOP — "Lullaby Till Open" : Ab lydian, 3/4, detuned
add(id='shop_dream', title='Lullaby Till Open', bpm=80, bars=24, bpb=3, key=68, scale='lydian', swing=0.0, lead='vibes', mel_gain=0.9,
    scenes=['shop_dream'], family='dream', trim={},
    progs={'A': [(0, 'M7'), (0, 'M7'), (2, 'M'), (2, 'M'), (9, 'm7'), (9, 'm7'), (7, 'M'), (4, 'm7')],
           'B': [(5, 'M7'), (5, 'M7'), (2, 'M'), (2, 'M'), (0, 'M7'), (4, 'm7'), (9, 'm7'), (7, 'sus')]},
    sections=[('A', [('w1', [4, 6, 5, 4, 2], {}), ('w2', [4, 5, 6, 7, 6, 4], {}), ('w1', [4, 6, 5, 4, 2], {'inst': 'celesta'}), ('w3', [8, 7, 6, 5], {})]),
              ('B', [('w2', [7, 6, 5, 6, 4, 2], {'inst': 'celesta'}), ('w1', [8, 7, 6, 5, 4], {}), ('w2', [4, 5, 6, 7, 8, 7], {}), ('w3', [6, 5, 4, 2], {})]),
              ('A', [('w1', [4, 6, 5, 4, 2], {'double': 'musicbox'}), ('w2', [4, 5, 6, 7, 6, 4], {'double': 'musicbox'}), ('w1', [4, 6, 5, 4, 2], {'inst': 'celesta'}), ('w3', [4, 2, 1, 0], {})])],
    bass=pedal_pad_bass, drums=drums_waltz, extra=extra_dream, energy={})

# 9. ENDLESS — "Never-Closing Time" : Bb dorian, driving
add(id='endless_neverclose', title='Never-Closing Time', bpm=132, bars=32, key=70, scale='dorian', swing=0.0, lead='marimba', mel_gain=0.95,
    scenes=['endless'], family='all', trim={},
    progs={'A': [(0, 'm7'), (0, 'm7'), (5, '7'), (5, '7'), (3, 'M7'), (10, 'M'), (0, 'm7'), (7, 'm7')],
           'B': [(8, 'M7'), (10, 'M'), (0, 'm7'), (0, 'm7'), (8, 'M7'), (10, 'M'), (7, 'm7'), (5, '7')]},
    sections=[('A', [('sync', [4, 4, 6, 4, 2, 4, 5, 6, 4, 2], {}), ('stab', [4, 6, 5, 4, 2, 4, 5, 4], {}), ('sync', [4, 4, 6, 4, 2, 4, 5, 7, 6, 4], {'double': 'celesta'}), ('call', [5, 4, 3, 2, 1, 2, 3, 0], {})]),
              ('A', [('run', [4, 6, 4, 2, 4, 6, 7, 6, 4, 2, 4, 6, 7, 8, 7, 6], {}), ('stab', [4, 6, 5, 4, 2, 4, 5, 4], {'inst': 'clarinet'}), ('sync', [8, 8, 9, 8, 6, 8, 9, 11, 9, 8], {'inst': 'clarinet'}), ('call', [7, 6, 5, 4, 3, 4, 2, 0], {'inst': 'clarinet'})]),
              ('B', [('long', [7, 6, 5, 4, 5, 6], {'inst': 'clarinet'}), ('sync', [7, 7, 6, 5, 4, 5, 6, 7, 6, 5], {'inst': 'clarinet'}), ('long', [8, 9, 10, 9, 8, 7], {'inst': 'clarinet', 'double': 'celesta'}), ('call', [7, 6, 5, 4, 3, 2, 1, 0], {})]),
              ('A', [('run', [4, 6, 4, 2, 4, 6, 7, 6, 4, 2, 4, 6, 7, 8, 9, 10], {'double': 'celesta'}), ('stab', [8, 10, 9, 8, 6, 8, 9, 8], {}), ('sync', [8, 8, 9, 8, 6, 8, 9, 11, 9, 8], {'double': 'celesta'}), ('long', [7, 5, 3, 1, 2, 0], {})])],
    bass=ostinato_bass, drums=drums_drive, extra=lambda *a: extra_arp(*a, name='marimba', step=0.25, gain=0.45), energy={})



# ---------------------------------------------------------------- second wave: catchier hooks, rotate with the first set
HOOK_B = ('sync', [4, 4, 6, 4, 2, 4, 5, 6, 4, 2], {})

# 10. COMBAT EARLY B — "Spoon Brigade" : D mixolydian-ish major, bouncy two-note hook, answered by clarinet
add(id='combat_spoons', title='Spoon Brigade', bpm=118, bars=32, key=62, scale='major', swing=0.0, lead='marimba', mel_gain=1.0,
    scenes=['combat_early'], family='toyroom', trim={},
    progs={'A': [(0, 'M'), (0, 'M'), (10, 'M'), (10, 'M'), (5, 'M'), (7, 'm'), (0, 'M'), (7, '7')],
           'B': [(9, 'm'), (9, 'm'), (5, 'M'), (5, 'M'), (2, 'm'), (7, 'M'), (0, 'M'), (7, '7')]},
    sections=[('A', [('sync', [4, 4, 6, 4, 2, 4, 5, 6, 4, 2], {}), ('call', [4, 5, 6, 5, 4, 2, 3, 1], {'inst': 'clarinet'}), ('sync', [4, 4, 6, 4, 2, 4, 5, 6, 7, 4], {}), ('long', [6, 5, 4, 2, 1, 0], {'inst': 'clarinet'})]),
              ('A', [('sync', [4, 4, 6, 4, 2, 4, 5, 6, 4, 2], {'double': 'celesta'}), ('call', [8, 7, 6, 5, 6, 5, 4, 2], {'inst': 'clarinet'}), ('sync', [4, 4, 6, 4, 2, 4, 5, 6, 7, 8], {'double': 'celesta'}), ('long', [9, 8, 7, 6, 5, 4], {'inst': 'clarinet'})]),
              ('B', [('lilt', [5, 6, 7, 6, 5, 4, 5, 3], {'inst': 'vibes'}), ('stab', [4, 4, 5, 4, 2, 4, 5, 4], {}), ('lilt', [5, 6, 7, 8, 9, 8, 7, 5], {'inst': 'vibes'}), ('call', [4, 3, 2, 1, 2, 1, 0, 0], {})]),
              ('A', [('sync', [4, 4, 6, 4, 2, 4, 5, 6, 4, 2], {'double': 'celesta'}), ('call', [8, 7, 6, 5, 6, 5, 4, 2], {'inst': 'clarinet', 'double': 'celesta'}), ('run', [4, 5, 6, 7, 6, 5, 4, 3, 4, 5, 6, 7, 8, 7, 6, 5], {}), ('long', [4, 2, 0, 2, 1, 0], {'inst': 'clarinet'})])],
    bass=tuba_bass, drums=drums_march, extra=lambda *a: extra_arp(*a, name='marimba', step=0.5, gain=0.45), energy={})

# 11. COMBAT MID B — "Haunted Carousel" : A harmonic minor, galloping celesta/music-box hook
add(id='combat_carousel', title='Haunted Carousel', bpm=126, bars=32, key=69, scale='harmmin', swing=0.0, lead='celesta', mel_gain=1.0,
    scenes=['combat_mid'], family='toyroom', trim={'melody': 0.5},
    progs={'A': [(0, 'm'), (0, 'm'), (7, '7'), (7, '7'), (5, 'M'), (7, '7'), (0, 'm'), (7, '7')],
           'B': [(8, 'M'), (7, '7'), (0, 'm'), (0, 'm'), (5, 'M'), (3, 'dim'), (7, '7'), (7, '7')]},
    sections=[('A', [('stab', [7, 6, 7, 5, 4, 5, 4, 2], {}), ('stab', [3, 4, 3, 2, 1, 2, 1, 0], {}), ('stab', [7, 6, 7, 8, 9, 8, 7, 5], {'double': 'musicbox'}), ('call', [4, 3, 2, 1, 2, 3, 4, 0], {})]),
              ('A', [('stab', [7, 6, 7, 5, 4, 5, 4, 2], {'inst': 'clarinet'}), ('stab', [3, 4, 3, 2, 1, 2, 1, 0], {'inst': 'clarinet'}), ('sync', [7, 8, 9, 8, 7, 5, 4, 5, 6, 7], {'inst': 'clarinet'}), ('long', [6, 5, 4, 3, 2, 0], {'inst': 'clarinet'})]),
              ('B', [('call', [9, 8, 7, 6, 7, 6, 5, 4], {}), ('sync', [4, 5, 6, 7, 6, 5, 4, 3, 2, 3], {}), ('call', [9, 10, 9, 8, 7, 6, 7, 5], {'double': 'musicbox'}), ('long', [4, 3, 2, 1, 0, 0], {})]),
              ('A', [('stab', [7, 6, 7, 5, 4, 5, 4, 2], {'double': 'musicbox'}), ('stab', [3, 4, 3, 2, 1, 2, 1, 0], {'double': 'musicbox'}), ('stab', [7, 6, 7, 8, 9, 8, 7, 9], {'double': 'musicbox'}), ('long', [11, 9, 7, 5, 4, 2], {'double': 'musicbox'})])],
    bass=ostinato_bass, drums=drums_tick, extra=extra_pulse, energy={})

# 12. COMBAT LATE B — "Last Train Overtime" : C minor, driving riff hook on bass clarinet + clarinet
add(id='combat_lasttrain', title='Last Train Overtime', bpm=142, bars=32, key=60, scale='minor', swing=0.0, lead='clarinet', mel_gain=0.95,
    scenes=['combat_late'], family='office', trim={'melody': 0.5},
    progs={'A': [(0, 'm'), (0, 'm'), (8, 'M'), (8, 'M'), (10, 'M'), (10, 'M'), (7, 'm'), (7, '7')],
           'B': [(5, 'm'), (5, 'm'), (3, 'M'), (3, 'M'), (8, 'M'), (7, 'm'), (0, 'm'), (7, '7')]},
    sections=[('A', [('stab', [4, 4, 6, 4, 7, 6, 4, 2], {}), ('stab', [4, 4, 6, 4, 8, 7, 6, 4], {}), ('stab', [4, 4, 6, 4, 7, 6, 4, 2], {'double': 'marimba'}), ('sync', [8, 7, 6, 4, 6, 4, 2, 1, 2, 0], {})]),
              ('A', [('run', [4, 6, 4, 2, 4, 6, 7, 6, 4, 6, 7, 9, 8, 7, 6, 4], {'inst': 'marimba'}), ('stab', [4, 4, 6, 4, 8, 7, 6, 4], {}), ('run', [4, 6, 4, 2, 4, 6, 7, 6, 8, 7, 6, 4, 6, 4, 2, 1], {'inst': 'marimba'}), ('sync', [8, 7, 6, 4, 6, 4, 2, 1, 2, 0], {})]),
              ('B', [('long', [7, 6, 4, 6, 7, 9], {'double': 'celesta'}), ('sync', [9, 8, 7, 6, 4, 6, 7, 6, 4, 2], {}), ('long', [9, 10, 9, 7, 6, 4], {'double': 'celesta'}), ('call', [6, 4, 2, 1, 2, 1, 0, 0], {})]),
              ('A', [('stab', [4, 4, 6, 4, 7, 6, 4, 2], {'double': 'celesta'}), ('stab', [4, 4, 6, 4, 8, 7, 6, 4], {'double': 'celesta'}), ('stab', [8, 8, 9, 8, 11, 9, 8, 6], {'double': 'celesta'}), ('sync', [8, 7, 6, 4, 6, 7, 8, 9, 11, 9], {'double': 'marimba'})])],
    bass=ostinato_bass, drums=drums_drive, extra=lambda *a: extra_arp(*a, name='marimba', step=0.25, gain=0.45), energy={})

# 13. BOSS B — "Clown at the Cash Register" : E phrygian-dominant, stabbing hook, bells
add(id='boss_clown', title='Clown at the Cash Register', bpm=152, bars=32, key=64, scale='phrygdom', swing=0.0, lead='bassclar', mel_gain=1.0,
    scenes=['boss'], family='all', trim={},
    progs={'A': [(0, 'm'), (1, 'M'), (0, 'm'), (6, 'dim'), (0, 'm'), (1, 'M'), (7, '7'), (0, 'm')],
           'B': [(8, 'M'), (7, '7'), (5, 'm'), (1, 'M'), (0, 'm'), (6, 'dim'), (7, '7'), (7, '7')]},
    sections=[('A', [('stab', [0, 0, 1, 0, 4, 3, 1, 0], {}), ('stab', [0, 0, 1, 0, 5, 4, 3, 1], {}), ('stab', [0, 0, 1, 0, 4, 3, 1, 0], {'inst': 'clarinet'}), ('sync', [4, 3, 2, 1, 2, 3, 4, 5, 4, 3], {'inst': 'clarinet'})]),
              ('A', [('call', [7, 8, 7, 5, 4, 5, 4, 3], {'inst': 'clarinet', 'double': 'bell'}), ('stab', [7, 7, 8, 7, 11, 10, 8, 7], {'inst': 'clarinet'}), ('call', [7, 8, 7, 5, 4, 5, 4, 3], {'inst': 'clarinet', 'double': 'bell'}), ('long', [5, 4, 3, 2, 1, 0], {})]),
              ('B', [('long', [8, 7, 5, 4, 5, 4], {'inst': 'clarinet', 'double': 'celesta'}), ('stab', [4, 3, 2, 1, 4, 3, 2, 1], {}), ('long', [9, 8, 7, 5, 7, 8], {'inst': 'clarinet', 'double': 'celesta'}), ('sync', [8, 7, 5, 4, 3, 2, 1, 2, 1, 0], {})]),
              ('A', [('stab', [0, 0, 1, 0, 4, 3, 1, 0], {'double': 'celesta'}), ('stab', [0, 0, 1, 0, 5, 4, 3, 1], {'double': 'celesta'}), ('stab', [7, 7, 8, 7, 11, 10, 8, 7], {'inst': 'clarinet', 'double': 'celesta'}), ('sync', [8, 7, 5, 4, 3, 2, 1, 0, 1, 0], {'double': 'bell'})])],
    bass=ostinato_bass, drums=drums_boss, extra=extra_boss, energy={})

# 14. GOLD SHOP B — "Layaway Shuffle" : F major, relaxed shuffle, clarinet hook
add(id='shop_layaway', title='Layaway Shuffle', bpm=98, bars=32, key=65, scale='major', swing=0.34, lead='clarinet', mel_gain=0.9,
    scenes=['shop_gold'], family='toyroom', trim={'melody': 0.5},
    progs={'A': [(0, 'M7'), (0, 'M7'), (5, '7'), (5, '7'), (2, 'm7'), (7, '7'), (0, 'M7'), (7, '7')],
           'B': [(9, 'm7'), (2, 'm7'), (7, '7'), (0, 'M7'), (5, 'M7'), (5, 'm7'), (0, 'M7'), (7, '7')]},
    sections=[('A', [('lilt', [4, 6, 5, 4, 2, 4, 5, 2], {}), ('call', [4, 5, 6, 4, 5, 4, 2, 0], {}), ('lilt', [4, 6, 5, 4, 2, 4, 7, 6], {}), ('long', [5, 4, 2, 1, 0, 0], {})]),
              ('A', [('lilt', [4, 6, 5, 4, 2, 4, 5, 2], {'inst': 'vibes'}), ('call', [4, 5, 6, 4, 5, 4, 2, 0], {'inst': 'vibes'}), ('sync', [7, 6, 5, 4, 5, 6, 7, 8, 7, 5], {}), ('long', [6, 5, 4, 2, 1, 0], {})]),
              ('B', [('call', [7, 6, 5, 4, 3, 4, 5, 3], {'inst': 'marimba'}), ('sync', [4, 5, 6, 7, 6, 5, 4, 3, 2, 3], {'inst': 'marimba'}), ('call', [7, 8, 7, 6, 5, 6, 5, 4], {'inst': 'marimba'}), ('long', [4, 2, 1, 0, 1, 0], {'inst': 'marimba'})]),
              ('A', [('lilt', [4, 6, 5, 4, 2, 4, 5, 2], {'double': 'bell'}), ('call', [4, 5, 6, 4, 5, 4, 2, 0], {}), ('lilt', [4, 6, 5, 4, 2, 4, 7, 6], {'double': 'bell'}), ('long', [2, 4, 2, 1, 0, 0], {})])],
    bass=walking_bass, drums=drums_swing, extra=extra_shop, energy={})

# 15. ENDLESS B — "Encore Forever" : G dorian, big hook, relentless
add(id='endless_encore', title='Encore Forever', bpm=136, bars=32, key=67, scale='dorian', swing=0.0, lead='marimba', mel_gain=0.95,
    scenes=['endless'], family='all', trim={},
    progs={'A': [(0, 'm7'), (0, 'm7'), (3, 'M7'), (3, 'M7'), (10, 'M'), (10, 'M'), (5, '7'), (7, 'm7')],
           'B': [(8, 'M7'), (10, 'M'), (0, 'm7'), (0, 'm7'), (8, 'M7'), (10, 'M'), (7, 'm7'), (5, '7')]},
    sections=[('A', [('sync', [4, 4, 6, 7, 6, 4, 2, 4, 5, 4], {}), ('stab', [4, 6, 7, 6, 4, 2, 4, 5], {}), ('sync', [4, 4, 6, 7, 6, 4, 8, 7, 6, 4], {'double': 'celesta'}), ('call', [5, 4, 3, 2, 1, 2, 3, 0], {})]),
              ('A', [('run', [4, 6, 7, 6, 4, 6, 7, 9, 8, 7, 6, 4, 6, 4, 2, 4], {}), ('stab', [4, 6, 7, 6, 4, 2, 4, 5], {'inst': 'clarinet'}), ('sync', [8, 8, 9, 8, 6, 8, 9, 11, 9, 8], {'inst': 'clarinet'}), ('call', [7, 6, 5, 4, 3, 4, 2, 0], {'inst': 'clarinet'})]),
              ('B', [('long', [7, 6, 7, 9, 8, 7], {'inst': 'clarinet', 'double': 'celesta'}), ('sync', [7, 7, 6, 5, 4, 5, 6, 7, 6, 5], {'inst': 'clarinet'}), ('long', [9, 10, 11, 10, 9, 8], {'inst': 'clarinet', 'double': 'celesta'}), ('call', [8, 7, 6, 5, 4, 3, 2, 0], {})]),
              ('A', [('run', [4, 6, 7, 6, 4, 6, 7, 9, 8, 7, 6, 4, 6, 7, 9, 11], {'double': 'celesta'}), ('stab', [8, 10, 9, 8, 6, 8, 9, 8], {'double': 'celesta'}), ('sync', [8, 8, 9, 8, 6, 8, 9, 11, 9, 8], {'double': 'celesta'}), ('long', [7, 5, 4, 2, 1, 0], {})])],
    bass=ostinato_bass, drums=drums_drive, extra=lambda *a: extra_arp(*a, name='marimba', step=0.25, gain=0.45), energy={})


def encode(wav_path, ogg_path):
    subprocess.run(['ffmpeg', '-y', '-loglevel', 'error', '-i', wav_path, '-c:a', 'libvorbis', '-q:a', '5', ogg_path], check=True)


def entry(p):
    return dict(id=p.id, title=p.title, bpm=p.bpm, beatsPerBar=p.bpb, bars=p.bars, loopSeconds=round(p.loop_seconds, 6),
                loopSamples=p.n, sampleRate=SR, scenes=p.scenes, family=p.family,
                stems=['Music/%s/%s_%s' % (p.id, p.id, k) for k in ('base', 'rhythm', 'melody', 'intensity')])


def main(only):
    os.makedirs(OUT, exist_ok=True)
    for p in PIECES:
        if only and p.id not in only:
            continue
        stems = render_stems(p)
        d = os.path.join(OUT, p.id)
        os.makedirs(d, exist_ok=True)
        for k, buf in stems.items():
            wav = os.path.join(d, '%s_%s.wav' % (p.id, k))
            write_wav(wav, buf)
            encode(wav, os.path.join(d, '%s_%s.ogg' % (p.id, k)))
            os.remove(wav)
        print('rendered', p.id, '%.1fs' % p.loop_seconds, flush=True)
    # manifest lists every piece whose stems exist on disk (so partial renders never drop earlier pieces)
    present = [entry(p) for p in PIECES if all(os.path.exists(os.path.join(ROOT, 'Assets', 'ArcaneBrigade', 'Resources', 'Audio', 'Arcane', s + '.ogg')) for s in entry(p)['stems'])]
    with open(os.path.join(OUT, 'music.json'), 'w') as f:
        json.dump({'version': 1, 'pieces': present}, f, indent=1)


if __name__ == '__main__':
    main(set(sys.argv[1:]))
