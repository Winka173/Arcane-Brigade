"""Renders the Arcane Brigade adaptive soundtrack — "Neon Carnival Funk" (soundtrack v2).

Each composition is rendered as 4 sample-aligned stems with identical length, tempo and loop point:
  base      bass + harmonic floor (always on)
  rhythm    drums, percussion and percussive comping (clav / guitar / stabs)
  melody    lead hook (+ answers, vocal chops)
  intensity extra energy layer (brass stabs, arps, doubled hook, risers, scratches) driven by enemy density / boss
Tails wrap to the loop start, reverb / wow / sidechain are circular, so loops have no seam and no cut-off phrase.

Style: urban funk, breakbeat, electro, drum & bass, phonk and lo-fi hip-hop in the spirit of Zenless Zone Zero's
soundtrack, crossed with a haunted-carnival colour (calliope organ, cowbell, toy percussion). Every melody,
progression and pattern below is written for this project; all sound is synthesised in kit.py / synth.py.
No samples, no third-party audio, nothing transcribed from any game.

Run: python Tools/Audio/build_music.py [-j N] [piece_id ...]   (numpy, scipy, soundfile with OGG/Vorbis)
"""
import json
import os
import re
import sys
from multiprocessing import Pool

import numpy as np

sys.path.insert(0, os.path.dirname(__file__))
from synth import SR, place, circular_reverb, normalize_rms, lp, hp, _stable
import kit as K

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
OUT = os.path.join(ROOT, 'Assets', 'ArcaneBrigade', 'Resources', 'Audio', 'Arcane', 'Music')
STEMS = ('base', 'rhythm', 'melody', 'intensity')

SCALES = {
    'major': [0, 2, 4, 5, 7, 9, 11], 'minor': [0, 2, 3, 5, 7, 8, 10], 'dorian': [0, 2, 3, 5, 7, 9, 10],
    'mixolydian': [0, 2, 4, 5, 7, 9, 10], 'lydian': [0, 2, 4, 6, 7, 9, 11], 'phrygian': [0, 1, 3, 5, 7, 8, 10],
    'phrygdom': [0, 1, 4, 5, 7, 8, 10], 'harmmin': [0, 2, 3, 5, 7, 8, 11],
}
CHORDS = {
    'M': [0, 4, 7], 'm': [0, 3, 7], 'dim': [0, 3, 6], 'aug': [0, 4, 8], 'sus': [0, 5, 7],
    '7': [0, 4, 7, 10], 'm7': [0, 3, 7, 10], 'M7': [0, 4, 7, 11], '6': [0, 4, 7, 9], 'm6': [0, 3, 7, 9],
    '9': [0, 4, 7, 10, 14], 'm9': [0, 3, 7, 10, 14], 'M9': [0, 4, 7, 11, 14], '7sus': [0, 5, 7, 10],
    '7#9': [0, 4, 7, 10, 15], 'add9': [0, 4, 7, 14], 'm11': [0, 3, 7, 10, 14, 17],
}
NOTE_RE = re.compile(r'^(\^)?(-?\d+)([#b]?)$')
VEL = {'X': 1.0, 'x': 0.75, 'o': 0.42}

# drum voices: name -> fn(vel, variant)
DRUM = {
    'kick': lambda v, r: K.kick(v, 'punch'),
    'kick808': lambda v, r: K.kick(v, '808'),
    'kickbreak': lambda v, r: K.kick(v, 'break'),
    'kickboom': lambda v, r: K.kick(v, 'boom'),
    'snare': lambda v, r: K.snare(v, 190, 1.0, 0.24, r),
    'snaretight': lambda v, r: K.snare(v, 215, 0.9, 0.18, r, tight=True),
    'snarelofi': lambda v, r: lp(K.snare(v, 175, 0.8, 0.22, r), 5500),
    'snarebig': lambda v, r: K.snare(v, 165, 1.3, 0.38, r),
    'clap': lambda v, r: K.clap(v, r),
    'hat': lambda v, r: K.hat(v, False, r),
    'ohat': lambda v, r: K.hat(v, True, r),
    'hatlofi': lambda v, r: lp(K.hat(v, False, r, 0.9), 8000),
    'ride': lambda v, r: K.ride(v, r),
    'rim': lambda v, r: K.rim(v),
    'shaker': lambda v, r: K.shaker(v, r),
    'tick': lambda v, r: K.tick(v, r % 2 == 0),
    'tamb': lambda v, r: K.hat(v, False, r + 7, 1.7),
    'congahi': lambda v, r: K.conga(330, v, r % 2 == 1),
    'congalo': lambda v, r: K.conga(220, v, False),
    'tomhi': lambda v, r: K.tom(180, v),
    'tommid': lambda v, r: K.tom(130, v),
    'tomlo': lambda v, r: K.tom(95, v),
    'cowbell': lambda v, r: K.cowbell(560, v),
}
TRACK = {  # track key -> (default voice, gain, pan)
    'k': ('kick', 1.0, 0.0), 's': ('snare', 0.8, 0.0), 'c': ('clap', 0.7, 0.05), 'h': ('hat', 0.32, 0.25),
    'oh': ('ohat', 0.26, -0.2), 'r': ('rim', 0.35, 0.15), 'sh': ('shaker', 0.32, -0.3), 't': ('tick', 0.28, 0.35),
    'rd': ('ride', 0.22, -0.25), 'cb': ('cowbell', 0.3, 0.2), 'cg': ('congahi', 0.4, -0.35), 'cl': ('congalo', 0.4, -0.25),
    'tb': ('tamb', 0.25, 0.4),
}
_DRUM_CACHE = {}
_INST_CACHE = {}


def drum_clip(voice, vel, var):
    k = (voice, round(vel, 2), var)
    if k not in _DRUM_CACHE:
        _DRUM_CACHE[k] = DRUM[voice](vel, var)
    return _DRUM_CACHE[k]


def mtof(m):
    return 440.0 * 2 ** ((m - 69) / 12.0)


def inst(name, midi, dur, vel=1.0):
    """Cached pitched instrument note. name = family[:kind]."""
    k = (name, round(float(midi), 2), round(dur, 3), round(vel, 2))
    if k in _INST_CACHE:
        return _INST_CACHE[k]
    f = mtof(midi)
    fam, _, kind = name.partition(':')
    if fam == 'bass':
        y = K.bass(f, dur, vel, kind or 'funk')
    elif fam == 'lead':
        y = K.lead(f, dur, vel, kind or 'square')
    elif fam == 'rhodes':
        y = K.rhodes(f, dur, vel)
    elif fam == 'clav':
        y = K.clav(f, dur, vel)
    elif fam == 'organ':
        y = K.organ(f, dur, vel)
    elif fam == 'calliope':
        y = K.organ(f, dur, vel, calliope=True)
    elif fam == 'pad':
        y = K.pad(f, dur, vel, bright=float(kind or 0.6))
    elif fam == 'vibes':
        y = K.vibes(f, dur, vel)
    elif fam == 'glock':
        y = K.glock(f, dur, vel)
    elif fam == 'pluck':
        y = K.pluck(f, max(dur, 0.25), vel)
    elif fam == 'cowbell':
        y = K.cowbell(f, vel, 0.3)
    elif fam == 'vox':
        v0, _, v1 = kind.partition('>')
        y = K.vox(f, dur, vel, v0 or 'a', v1 or None, gender=1.15, seed=int(midi))
    elif fam == 'voxhi':
        v0, _, v1 = kind.partition('>')
        y = K.vox(f, dur, vel, v0 or 'a', v1 or None, gender=1.35, breath=0.25, seed=int(midi))
    else:
        raise KeyError(name)
    _INST_CACHE[k] = y
    return y


WORDS = {'yeah': 'y>a', 'oh': 'o', 'ah': 'a', 'hey': 'h>e', 'woo': 'u>o', 'uh': 'u>a', 'ooh': 'u', 'ay': 'a>i'}


class Song:
    def __init__(self, id, title, bpm, key, scale, scenes, family, bpb=4, swing=0.0, swing8=0.0, form='AABA',
                 sec_bars=8, lofi=False, duck=0.0, duck_stems=('base', 'intensity'), wet=1.0, trim=None, alloc=True):
        self.id, self.title, self.bpm, self.key, self.scale = id, title, bpm, key % 12, scale
        self.scenes, self.family, self.bpb = scenes, family, bpb
        self.swing, self.swing8, self.lofi, self.duck, self.duck_stems = swing, swing8, lofi, duck, duck_stems
        self.wet, self.trim = wet, trim or {}
        self.form = form.split() if ' ' in form else list(form)
        self.sec_bars = sec_bars
        self.bars = len(self.form) * sec_bars
        self.beat = 60.0 / bpm
        self.spb = bpb * 4  # 16th steps per bar
        self.n = int(round(self.bars * bpb * self.beat * SR))
        self.loop_seconds = self.n / SR
        self.progs = {}
        self.kicks = []
        self.rng = np.random.default_rng(_stable(id))
        self.bufs = {k: np.zeros((self.n, 2)) for k in STEMS} if alloc else {}

    # ------------------------------------------------------------ structure
    def prog(self, name, text):
        out = []
        for tok in text.split():
            off, q = tok.split(':')
            out.append(((self.key + int(off)) % 12, q))
        self.progs[name] = out

    def section(self, bar):
        si = bar // self.sec_bars
        return self.form[si % len(self.form)], si, bar % self.sec_bars

    def chord(self, bar):
        name, _, k = self.section(bar)
        p = self.progs[name]
        root, q = p[k % len(p)]
        return root, CHORDS[q]

    # ------------------------------------------------------------ placement
    def t_of(self, beat_pos, swing=True):
        t = beat_pos * self.beat
        if swing:
            step = beat_pos * 4
            if abs(step - round(step)) < 1e-6:
                s = int(round(step))
                if self.swing and s % 2 == 1:
                    t += self.swing * 0.25 * self.beat
                if self.swing8 and s % 4 == 2:
                    t += self.swing8 * 0.5 * self.beat
        return t

    def put(self, stem, clip, beat_pos, gain=1.0, pan=0.0, human=True, swing=True):
        t = self.t_of(beat_pos, swing)
        if human:
            t += self.rng.normal(0, 0.0025)
            gain *= 1 + self.rng.normal(0, 0.04)
        place(self.bufs[stem], clip, t % self.loop_seconds, gain, pan, wrap=True)

    def b(self, bar, step=0):
        return bar * self.bpb + step / 4.0

    def step_s(self):
        return self.beat / 4.0

    # ------------------------------------------------------------ parts
    def drums(self, bar, pats, voices=None, stem='rhythm', gain=1.0):
        """pats: track -> pattern string (spb chars per bar, or 2*spb for a 2-bar pattern). X accent, x hit, o ghost."""
        voices = voices or {}
        for trk, pat in pats.items():
            if not pat:
                continue
            pat = pat.replace(' ', '').replace('|', '')
            if len(pat) % self.spb:
                raise ValueError('%s: drum pattern %s length %d not multiple of %d' % (self.id, trk, len(pat), self.spb))
            nb = len(pat) // self.spb
            seg = pat[(bar % nb) * self.spb:(bar % nb + 1) * self.spb]
            base_voice, g, pan = TRACK[trk]
            voice = voices.get(trk, base_voice)
            for st, ch in enumerate(seg):
                if ch not in VEL:
                    continue
                v = VEL[ch]
                var = int(self.rng.integers(0, 3))
                clip = drum_clip(voice, v, var)
                self.put(stem, clip, self.b(bar, st), g * gain, pan, human=trk not in ('k',))
                if trk == 'k' and v > 0.5:
                    self.kicks.append(self.t_of(self.b(bar, st)))

    def bass(self, bar, pattern, kind='funk', gain=1.0, octave=0, stem='base', legato=0.9):
        root, tones = self.chord(bar)
        r = 28 + ((root - 28) % 12) + 12 * octave
        toks = pattern.split()
        if len(toks) != self.spb:
            raise ValueError('%s: bass pattern has %d tokens, need %d' % (self.id, len(toks), self.spb))
        third = tones[1]
        fifth = tones[2]
        seventh = tones[3] if len(tones) > 3 and tones[3] < 12 else 10
        prev = None
        i = 0
        while i < len(toks):
            tk = toks[i]
            if tk in ('.', '-'):
                i += 1
                continue
            j = i + 1
            while j < len(toks) and toks[j] == '-':
                j += 1
            acc = tk.startswith('^')
            tk = tk.lstrip('^')
            ghost = tk == 'g'
            iv = {'R': 0, '3': third, '5': fifth, '7': seventh, '8': 12, '9': 14, '4': 5, '6': 9, 'b': -1, 'v': -5, 'g': 0,
                  'o': 12}.get(tk)
            if iv is None:
                iv = int(tk)
            m = r + iv
            length = (j - i) * self.step_s()
            dur = 0.06 if ghost else max(0.07, length * legato)
            vel = 0.45 if ghost else (1.0 if acc or i % 4 == 0 else 0.82)
            clip = inst('bass:' + kind, m, dur, vel)
            self.put(stem, clip, self.b(bar, i), gain * (0.6 if ghost else 1.0), 0.0)
            prev = m
            i = j

    def voicing(self, bar, lo=55, top=None, drop_root=True, count=4):
        root, tones = self.chord(bar)
        pcs = [(root + t) % 12 for t in tones]
        if drop_root and len(pcs) >= 4:
            pcs = pcs[1:]
        pcs = pcs[:count]
        notes = []
        for pc in pcs:
            m = lo + ((pc - lo) % 12)
            notes.append(m)
        notes.sort()
        if top is not None:
            notes = [m - 12 if m > top else m for m in notes]
            notes.sort()
        return notes

    def comp(self, bar, rhythm, name='rhodes', lo=55, gain=1.0, stem='base', pan=0.0, hold=None, vel=0.8,
             drop_root=True, stab=None):
        """Chord hits at X/x/o positions. Each hit lasts until the next one (or `hold` steps)."""
        rhythm = rhythm.replace(' ', '')
        if len(rhythm) != self.spb:
            raise ValueError('%s: comp rhythm length %d != %d' % (self.id, len(rhythm), self.spb))
        notes = self.voicing(bar, lo, drop_root=drop_root)
        hits = [i for i, ch in enumerate(rhythm) if ch in VEL]
        for n_, i in enumerate(hits):
            nxt = hits[n_ + 1] if n_ + 1 < len(hits) else self.spb
            steps = hold if hold else (nxt - i)
            dur = steps * self.step_s() * 0.92
            v = VEL[rhythm[i]] * vel
            if stab:
                clip = chord_clip(stab, tuple(notes), dur, v)
                self.put(stem, clip, self.b(bar, i), gain, pan)
            else:
                for k, m in enumerate(notes):
                    self.put(stem, inst(name, m, dur, v), self.b(bar, i), gain / len(notes) ** 0.5,
                             pan + 0.15 * (k - (len(notes) - 1) / 2))

    def arp(self, bar, name='glock', rate=2, order=(0, 1, 2, 3, 2, 1), lo=67, gain=1.0, stem='intensity', octaves=2,
            dur_steps=None, vel=0.7):
        notes = self.voicing(bar, lo, drop_root=False)
        pool = [m + 12 * o for o in range(octaves) for m in notes]
        for k, st in enumerate(range(0, self.spb, rate)):
            m = pool[order[k % len(order)] % len(pool)]
            dur = (dur_steps or rate) * self.step_s() * 0.8
            self.put(stem, inst(name, m, dur, vel * (1.0 if st % 4 == 0 else 0.8)), self.b(bar, st), gain,
                     -0.4 + 0.8 * ((k % 4) / 3))

    def mel(self, bar0, text, name='lead:square', octave=5, gain=1.0, stem='melody', grid=8, pan=-0.05,
            double=(), snap=True, legato=0.92, vel=0.88):
        """Scale-degree melody. Tokens per `grid` note: degree (0 = tonic; '#'/'b' suffix; '^' accent), '-' tie, '.' rest.
        Bars separated by '|'. double = [(name, semitones, gain, stem), ...]."""
        per_bar = self.bpb * grid // 4
        bars = [b_.split() for b_ in text.split('|')]
        for k, b_ in enumerate(bars):
            if len(b_) != per_bar:
                raise ValueError('%s: melody bar %d at %d has %d tokens, need %d: %s' % (self.id, k, bar0, len(b_), per_bar, b_))
        toks = [t for b_ in bars for t in b_]
        mult = 16 // grid
        sc = SCALES[self.scale]
        i = 0
        while i < len(toks):
            tk = toks[i]
            if tk in ('.', '-'):
                i += 1
                continue
            j = i + 1
            while j < len(toks) and toks[j] == '-':
                j += 1
            m_ = NOTE_RE.match(tk)
            if not m_:
                raise ValueError('%s: bad melody token %r' % (self.id, tk))
            acc, deg, alt = m_.group(1), int(m_.group(2)), m_.group(3)
            midi = 12 * (octave + 1) + self.key + sc[deg % 7] + 12 * (deg // 7) + (1 if alt == '#' else -1 if alt == 'b' else 0)
            step = i * mult
            bar = bar0 + step // self.spb
            st = step % self.spb
            length = (j - i) * mult
            if snap and length >= 4 and st % 4 == 0:
                midi = snap_clash(midi, self.chord(bar))
            dur = length * self.step_s() * legato
            v = 1.0 if acc else (vel if st % 4 == 0 else vel * 0.9)
            self.put(stem, inst(name, midi, dur, v), self.b(bar, st), gain, pan)
            for dn, semis, dg, dstem in double:
                self.put(dstem, inst(dn, midi + semis, dur, v * 0.9), self.b(bar, st), dg, -pan + 0.2)
            i = j

    def vox(self, stem, beat_pos, word, midi, dur_beats=0.5, gain=0.6, pan=0.0, hi=False):
        name = ('voxhi:' if hi else 'vox:') + WORDS.get(word, word)
        self.put(stem, inst(name, midi, dur_beats * self.beat, 0.9), beat_pos, gain, pan)

    def stab(self, stem, beat_pos, bar, kind='brass', lo=60, dur_beats=0.4, gain=0.7, pan=0.0, vel=0.9):
        notes = self.voicing(bar, lo, drop_root=False)
        self.put(stem, chord_clip(kind, tuple(notes), dur_beats * self.beat, vel), beat_pos, gain, pan)

    def fx(self, stem, clip, beat_pos, gain=1.0, pan=0.0, end=False):
        """end=True: clip ends exactly at beat_pos (risers)."""
        if end:
            beat_pos -= len(clip) / SR / self.beat
        self.put(stem, clip, beat_pos, gain, pan, human=False, swing=False)

    # ------------------------------------------------------------ common moves
    def crash_at(self, bar, gain=0.5, stem='rhythm'):
        self.put(stem, K.crash(0.9, bar % 3), self.b(bar), gain, -0.1, human=False)

    def snare_roll(self, bar, from_step=8, voice='snare', gain=0.6, stem='rhythm'):
        for st in range(from_step, self.spb):
            v = 0.35 + 0.6 * (st - from_step) / max(1, self.spb - from_step)
            self.put(stem, drum_clip(voice, round(v, 1), st % 3), self.b(bar, st), gain, 0.0)

    def tom_fill(self, bar, from_step=8, gain=0.7, stem='rhythm'):
        seq_ = ['tomhi', 'tomhi', 'tommid', 'tommid', 'tomlo', 'tomlo', 'tomlo', 'tomlo']
        for k, st in enumerate(range(from_step, self.spb)):
            self.put(stem, drum_clip(seq_[min(k, 7)], 0.9, 0), self.b(bar, st), gain, 0.4 - 0.1 * k)

    def riser_into(self, bar, beats=4, gain=0.5, stem='intensity'):
        self.fx(stem, K.riser(beats * self.beat, bar), self.b(bar), gain, end=True)

    def scratch_at(self, beat_pos, beats=1.0, gain=0.5, stem='intensity', rate=7.0):
        self.fx(stem, K.scratch(beats * self.beat, int(beat_pos * 7) % 13, rate), beat_pos, gain, 0.2)

    # ------------------------------------------------------------ mixdown
    def finish(self):
        out = {}
        tau = 0.22 * self.beat
        duck_env = np.ones(self.n)
        if self.duck and self.kicks:
            L = int(4 * tau * SR)
            pre = int(0.004 * SR)
            curve = 1 - self.duck * np.exp(-np.arange(L) / (tau * SR))
            ramp = np.linspace(1, 1 - self.duck, pre)
            for tk in self.kicks:
                i0 = int(round(tk * SR))
                idx = (i0 + np.arange(L)) % self.n
                duck_env[idx] = np.minimum(duck_env[idx], curve)
                idx = (i0 - pre + np.arange(pre)) % self.n
                duck_env[idx] = np.minimum(duck_env[idx], ramp)
        reverb = dict(base=(0.08, 0.9), rhythm=(0.06, 0.6), melody=(0.16, 1.5), intensity=(0.15, 1.3))
        targets = dict(base=-22.0, rhythm=-24.0, melody=-22.5, intensity=-25.5)
        t = np.arange(self.n) / SR
        for k, b_ in self.bufs.items():
            if self.lofi and k == 'rhythm':
                vin = K.vinyl(self.n, _stable(self.id) % 97, 0.8)
                b_ += np.stack([vin, np.roll(vin, 997)], 1) * np.abs(b_).max() * 0.15
            if self.duck and k in self.duck_stems:
                b_ *= duck_env[:, None]
            wet, sec_ = reverb[k]
            b_ = circular_reverb(b_, wet * self.wet, sec_ * (1.5 if self.wet > 1.3 else 1.0), seed=_stable((self.id, k)) % 1000)
            for c in range(2):  # filters on a 3x tiled copy so the loop point stays continuous
                tiled = np.tile(b_[:, c], 3)
                y = hp(lp(tiled, 7200 if self.lofi and k != 'rhythm' else 15000), 30, 2)
                b_[:, c] = y[self.n:2 * self.n]
            if self.lofi and k in ('base', 'melody', 'intensity'):
                cycles = max(1, round(0.55 * self.loop_seconds))
                d = 0.0016 * np.sin(2 * np.pi * cycles * t / self.loop_seconds) * SR
                pos = (np.arange(self.n) - d) % self.n
                for c in range(2):
                    ext = np.concatenate([b_[:, c], b_[:1, c]])
                    b_[:, c] = np.interp(pos, np.arange(self.n + 1), ext)
            out[k] = normalize_rms(b_, targets[k] + self.trim.get(k, 0.0), peak_db=-3.0)
        return out


_CHORD_CACHE = {}


def chord_clip(kind, notes, dur, vel):
    k = (kind, notes, round(dur, 3), round(vel, 2))
    if k not in _CHORD_CACHE:
        _CHORD_CACHE[k] = K.chord_stab([mtof(m) for m in notes], dur, vel, kind)
    return _CHORD_CACHE[k]


def snap_clash(midi, chord):
    """Long notes on a beat that sit a minor 2nd above a chord tone (and aren't chord tones) move onto it."""
    root, tones = chord
    pcs = {(root + t) % 12 for t in tones}
    pc = midi % 12
    if pc in pcs:
        return midi
    if any((pc - ct) % 12 == 1 for ct in pcs):
        return midi - 1
    return midi


# =================================================================== compositions
PIECES = {}


def piece(**kw):
    def deco(fn):
        PIECES[kw['id']] = (kw, fn)
        return fn
    return deco


def every(s):
    for bar in range(s.bars):
        name, si, k = s.section(bar)
        yield bar, name, si, k


# ---------------------------------------------------------------- 1. MENU
@piece(id='menu_curtain', title='Neon Marquee', bpm=88, key=5, scale='dorian', scenes=['menu'], family='stage',
       swing=0.32, form='AABA', lofi=True, duck=0.25, trim={'melody': 0.5})
def menu_curtain(s):
    """Lo-fi neo-soul hip-hop: dusty boom-bap, Rhodes ninths, whistle hook, vinyl."""
    s.prog('A', '0:m9 5:9 10:M9 3:M7 8:M7 7:7#9 0:m9 7:7#9')
    s.prog('B', '8:M7 10:7 0:m9 0:m9 8:M7 10:7 7:7sus 7:7#9')
    hook = '4 - 6 4 3 - 2 . | 0 - - - . . 2 3 | 4 - 6 4 3 - 1 . | 7 - 6 - 4 . . . | 4 - 6 4 3 - 2 . | 4 - - - 6 - 4 . | 3 - 2 - 0 . 2 . | 4 - - - . . . .'
    hook2 = hook.rsplit('|', 2)[0] + '| 6 - 4 - 3 . 2 . | 0 - - - . . . .'
    bridge = '. . 4 - 3 - 2 - | 1 - - - . . . . | . . 4 - 6 - 7 - | 6 - 4 - - - . . | . . 4 - 3 - 2 - | 1 - 2 - 3 - 4 - | 3 - - - 2 - - - | 4 - - - . . . .'
    for bar, name, si, k in every(s):
        drop = si == 2 and k < 2
        s.drums(bar, dict(
            k='' if drop else 'X......x..X.....X......x..X..x..',
            s='' if drop else '....X.......X...',
            r='.......o.......o' if not drop else '',
            h='xoxoxoxoxoxoxoxo' if not drop else 'x.x.x.x.x.x.x.x.',
            sh='..o...o...o...o.' if si >= 1 else ''),
            dict(k='kickbreak', s='snarelofi', h='hatlofi'))
        s.bass(bar, 'R - - - - - . R . . 5 - 7 - 8 .' if k % 2 == 0 else 'R - - - - - . . R . . . 5 - b .', 'sub', 0.9)
        s.comp(bar, 'X.........x.....' if k % 2 == 0 else 'X.......x.....x.', 'rhodes', 53, 0.9, 'base', vel=0.7)
        s.comp(bar, '..x...x...x...x.', 'pluck', 62, 0.25, 'rhythm', pan=0.3, hold=1, vel=0.6)
        s.comp(bar, 'X...............', 'pad:0.3', 60, 0.35, 'intensity', hold=16)
        if k in (0, 4) and si != 2:
            s.arp(bar, 'glock', 2, (0, 2, 1, 3, 2, 4, 3, 5), 72, 0.4, 'intensity')
        if k == 7:
            s.scratch_at(s.b(bar, 12), 1.0, 0.35)
    for si, name in enumerate(s.form):
        b0 = si * 8
        if name == 'B':
            s.mel(b0, bridge, 'lead:sax', 5, 0.8)
            for k in (1, 3, 5):
                s.vox('melody', s.b(b0 + k, 8), 'ooh', 12 * 5 + s.key + 7, 1.5, 0.35, 0.3, hi=True)
        else:
            s.mel(b0, hook if si == 0 else hook2, 'lead:whistle', 5, 0.9,
                  double=[('vibes', 0, 0.45, 'melody')] if si >= 1 else [('vibes', 12, 0.3, 'intensity')])
        if si == 3:
            s.vox('melody', s.b(b0 + 7, 8), 'yeah', 12 * 5 + s.key + 7, 1.0, 0.4, -0.2)


# ---------------------------------------------------------------- 2. SELECT
@piece(id='select_dressing', title='Pick Your Crew', bpm=96, key=3, scale='major', scenes=['select'], family='stage',
       swing=0.14, form='AB', duck=0.2)
def select_dressing(s):
    """City-pop / chill funk: shuffled light kit, Rhodes, round bass, vibes + whistle hook."""
    s.prog('A', '5:M9 4:7 9:m9 9:m9 2:m9 7:7sus 0:M9 0:M9')
    s.prog('B', '2:m9 7:9 0:M9 9:m9 5:M9 7:7sus 0:M9 7:7sus')
    a = '4 - 5 - 4 . 2 - | 1 - - - . . 2 1 | 2 - 4 - 5 - 4 2 | 4 - - - - . . . | 5 - 4 - 2 - 4 - | 5 - - - 7 - - . | 4 - 2 - 1 - 2 - | 0 - - - . . . .'
    b = '. . 7 - 6 - 5 - | 4 - - - 5 - 4 - | . . 7 - 8 - 9 - | 8 - 7 - 4 - - - | . . 4 - 5 - 7 - | 8 - - - 7 - 5 - | 4 - 2 - 4 - 5 - | 4 - - - . . . .'
    for bar, name, si, k in every(s):
        s.drums(bar, dict(k='X.....x...x.....X.....x...x...x.', r='....x.......x...', h='x.xxx.xxx.xxx.xo', sh='.o.o.o.o.o.o.o.o'),
                dict(h='hatlofi'), gain=0.85)
        s.bass(bar, 'R - . R . . 8 . . R . . 5 . 7 .', 'funk', 0.9)
        s.comp(bar, 'X.....x.......x.', 'rhodes', 55, 0.9, 'base', vel=0.65)
        s.comp(bar, '..x..x....x..x..', 'clav', 60, 0.2, 'rhythm', pan=-0.3, hold=1, vel=0.5)
        s.arp(bar, 'glock', 4, (0, 2, 1, 3), 72, 0.35, 'intensity')
        if k == 7:
            s.riser_into(bar + 1, 4, 0.25)
    s.mel(0, a, 'lead:whistle', 5, 0.85, double=[('vibes', 0, 0.5, 'melody')])
    s.mel(8, b, 'vibes', 5, 1.0, double=[('lead:whistle', 12, 0.35, 'melody')])


# ---------------------------------------------------------------- 3. COMBAT EARLY
@piece(id='combat_toys', title='Wind-Up Riot', bpm=120, key=9, scale='dorian', scenes=['combat_early'], family='toyroom',
       form='AABA', duck=0.25)
def combat_toys(s):
    """Funk breakbeat + carnival: slap bass, chicken-scratch clav, calliope hook, brass answers."""
    s.prog('A', '0:m7 0:m7 5:9 5:9 8:M7 10:7 7:7#9 7:7#9')
    s.prog('B', '3:M7 10:M 5:9 8:M7 3:M7 10:M 7:7sus 7:7#9')
    hook = '4 . 4 3 4 . 6 . | 7 . 6 4 3 - 2 . | 4 . 4 3 4 . 5 . | 7 - 6 - 4 - - . | 2 . 2 0 2 . 4 . | 3 - 2 - 3 - 6 - | 4 . 4 3 4 . 6 . | 6# - - - 4 - . .'
    bridge = '4 - 2 - 0 - 2 - | 3 - - - 1 - - - | 4 - 5 - 7 - - - | 7 - 6 - 4 - 2 - | 4 - 2 - 0 - 2 - | 3 - 4 - 6 - 7 - | 8 - - - 7 - 6 - | 6# - - - . . . .'
    for bar, name, si, k in every(s):
        s.drums(bar, dict(k='X.x.......X..x..X.x...x...X.....', s='....X..o.o..X..o....X..o.o..X.oX',
                          h='x.x.x.x.x.x.x.x.', oh='' if si == 0 else '..............x.', tb='.x.x.x.x.x.x.x.x' if si >= 1 else ''),
                dict(k='kickbreak'))
        s.bass(bar, 'R . 8 . . R . 8 . . R . 7 - 5 .' if k % 2 == 0 else 'R . . R 8 . R . . 5 . 7 8 . b .', 'slap', 1.0)
        s.comp(bar, '..x...x.x.x...x.', 'clav', 57, 0.55, 'rhythm', pan=0.3, hold=1, vel=0.7)
        s.comp(bar, 'X...............', 'organ', 57, 0.35, 'base', hold=14, vel=0.6)
        if k % 2 == 1:
            s.stab('intensity', s.b(bar, 6), bar, 'brass', 62, 0.25, 0.7)
            s.stab('intensity', s.b(bar, 8), bar, 'brass', 62, 0.4, 0.8)
        if si >= 1:
            s.arp(bar, 'glock', 2, (0, 1, 2, 3, 4, 3, 2, 1), 69, 0.3, 'intensity')
        if k == 0:
            s.crash_at(bar, 0.4)
        if k == 7:
            s.snare_roll(bar, 12, 'snaretight', 0.5)
            s.vox('intensity', s.b(bar, 14), 'hey', 12 * 5 + s.key, 0.5, 0.5)
    for si, name in enumerate(s.form):
        if name == 'B':
            s.mel(si * 8, bridge, 'lead:sax', 5, 0.85, double=[('calliope', 12, 0.3, 'intensity')])
        else:
            dbl = [('glock', 12, 0.3, 'intensity')] if si == 0 else [('lead:square', 12, 0.3, 'intensity')]
            s.mel(si * 8, hook, 'calliope', 5, 1.0, double=dbl)


# ---------------------------------------------------------------- 4. COMBAT MID
@piece(id='combat_clockwork', title='Gearbox Groove', bpm=124, key=4, scale='dorian', scenes=['combat_mid'], family='workshop',
       form='AABA', duck=0.4)
def combat_clockwork(s):
    """Electro-funk: four-on-the-floor, acid bass, ticking hats, robot vox chops, square hook."""
    s.prog('A', '0:m9 0:m9 5:9 5:9 8:M7 10:6 0:m9 7:7#9')
    s.prog('B', '8:M7 10:M 0:m9 0:m9 8:M7 10:M 7:7sus 7:7#9')
    hook = '0 . 0 2 . 3 . 4 | 6 - 4 - 3 - 2 - | 0 . 0 2 . 3 . 4 | 5 - 4 - 3 - - - | 0 . 0 2 . 3 . 4 | 6 - 4 - 6 - 7 - | 8 - 7 - 6 - 4 - | 4 - - - 1 - 3 -'
    bridge = '4 - - - 2 - 4 - | 5 - - - 3 - - - | 4 - - - 6 - 7 - | 8 - - - - - . . | 9 - - - 7 - 6 - | 5 - 6 - 7 - 8 - | 7 - - - 6 - - - | 6# - - - 4 - - -'
    for bar, name, si, k in every(s):
        brk = name == 'B' and k < 4
        s.drums(bar, dict(k='' if brk else 'X...X...X...X...', c='....X.......X...', h='..x...x...x...x.',
                          oh='..x...x...x...x.' if si == 3 else '', t='xxxxxxxxxxxxxxxx' if si >= 1 else 'x.x.x.x.x.x.x.x.',
                          sh='' if brk else '.o.o.o.o.o.o.o.o'))
        s.bass(bar, 'R . R 8 . R . 5 R . 8 . 7 . 5 .' if not brk else 'R - - - - - - - 5 - - - 7 - - -', 'acid', 0.95)
        s.comp(bar, 'X...............', 'pad:0.4', 55, 0.45, 'base', hold=16)
        s.comp(bar, '...x..x....x..x.', 'clav', 60, 0.35, 'rhythm', pan=-0.3, hold=1, vel=0.6, stab='disco')
        if si >= 1 and not brk:
            s.arp(bar, 'lead:square', 1, (0, 4, 1, 5, 2, 6, 3, 7), 64, 0.18, 'intensity', dur_steps=1, vel=0.5)
        if k in (1, 5):
            for st, w in ((0, 'oh'), (3, 'oh'), (6, 'ah')):
                s.vox('intensity', s.b(bar, st + 8), w, 12 * 5 + s.key + (7 if w == 'oh' else 10), 0.5, 0.45, 0.25)
        if k == 0:
            s.crash_at(bar, 0.35)
        if k == 7:
            s.riser_into(bar + 1, 4, 0.4)
            s.snare_roll(bar, 8, 'clap', 0.35)
    for si, name in enumerate(s.form):
        if name == 'B':
            s.mel(si * 8, bridge, 'lead:saw', 5, 0.8, double=[('voxhi:a', 0, 0.3, 'intensity')])
        else:
            s.mel(si * 8, hook, 'lead:square', 5, 0.85, double=[('lead:saw', 12, 0.25, 'intensity')] if si else [])


# ---------------------------------------------------------------- 5. COMBAT LATE
@piece(id='combat_overtime', title='Overtime Rush', bpm=174, key=7, scale='minor', scenes=['combat_late'], family='office',
       form='AABA', duck=0.3)
def combat_overtime(s):
    """Drum & bass: two-step break, reese + sub, supersaw hook, ride and vocal 'yeah' chops."""
    s.prog('A', '0:m 0:m 8:M 8:M 3:M 3:M 10:M 7:7')
    s.prog('B', '8:M 10:M 0:m 0:m 8:M 10:M 7:7 7:7')
    hook = '4 - - - 3 - 2 - | 3 - 4 - - - 0 - | 4 - - - 3 - 2 - | 5 - 4 - - - 2 - | 4 - - - 3 - 2 - | 3 - 4 - 6 - 7 - | 8 - - - 7 - 6 - | 6# - - - 4 - - -'
    bridge = '9 - - - - - - - | 8 - - - 7 - - - | 7 - - - 9 - - - | 11 - - - - - - - | 12 - - - 11 - 10 - | 8 - - - 10 - - - | 11 - - - 8 - - - | 13# - - - - - - -'
    for bar, name, si, k in every(s):
        half = name == 'B' and k < 4
        s.drums(bar, dict(k='X.........X.......X.......X..x..' if not half else 'X...............',
                          s='....X..o.o..X..o....X.......XoXo' if not half else '........X.......',
                          h='xoxoxoxoxoxoxoxo', rd='x.x.x.x.x.x.x.x.' if si >= 1 and not half else '',
                          sh='' if half else 'o.o.o.o.o.o.o.o.'),
                dict(k='kickbreak', s='snaretight'))
        s.bass(bar, 'R - - - - - - - - - R - - - 8 -', 'reese', 0.8)
        s.bass(bar, 'R - - - - - - - - - R - - - - -', 'sub', 0.6)
        s.comp(bar, 'X...............', 'pad:0.7', 55, 0.4, 'base', hold=16)
        if not half:
            s.arp(bar, 'lead:square', 2, (0, 2, 4, 2, 1, 3, 5, 3), 67, 0.2, 'intensity', dur_steps=1)
            s.comp(bar, '..x..x....x..x..', 'pluck', 60, 0.3, 'rhythm', pan=0.35, hold=2, vel=0.6)
        if k % 4 == 3:
            s.vox('intensity', s.b(bar, 12), 'yeah', 12 * 5 + s.key + 7, 1.0, 0.45, -0.2)
        if k == 0:
            s.crash_at(bar, 0.4)
        if k == 7:
            s.riser_into(bar + 1, 8, 0.4)
    for si, name in enumerate(s.form):
        if name == 'B':
            s.mel(si * 8, bridge, 'lead:saw', 4, 0.75, double=[('voxhi:a>o', 12, 0.3, 'intensity')])
        else:
            s.mel(si * 8, hook, 'lead:super', 5, 0.8, double=[('lead:square', -12, 0.3, 'melody')] + ([('lead:super', 12, 0.25, 'intensity')] if si else []))


# ---------------------------------------------------------------- 6. BOSS
@piece(id='boss_ringmaster', title='Ringmaster Breakdown', bpm=150, key=2, scale='phrygdom', scenes=['boss'], family='all',
       form='AABA', duck=0.3)
def boss_ringmaster(s):
    """Big-beat electro-rock with a sinister circus: distorted saw bass, guitar hook, calliope, brass, gong."""
    s.prog('A', '0:M 1:M 0:M 10:m 0:M 1:M 5:m 0:7')
    s.prog('B', '5:m 5:m 1:M 1:M 10:m 10:m 0:M 0:7')
    hook = '0 - 1 0 . 2 - . | 1 - - - . . . . | 0 - 1 0 . 4 - . | 6 - 5 - 4 - 3 - | 7 - 8 7 . 9 - . | 8 - - - 7 - 5 - | 3 - 5 - 4 - 3 - | 2 - - - 0 - . .'
    bridge = '3 - 4 - 5 - 4 - | 3 - - - - - . . | 5 - 6 - 8 - 6 - | 5 - - - - - . . | 6 - 5 - 3 - 1 - | 3 - - - - - . . | 2 - 3 - 4 - 6 - | 7 - - - - - . .'
    for bar, name, si, k in every(s):
        s.drums(bar, dict(k='X..X..X...X..x..', s='....X.......X...', h='x.x.x.x.x.x.x.x.', oh='......x.......x.',
                          tb='.x.x.x.x.x.x.x.x' if si >= 1 else ''), dict(s='snarebig'))
        s.bass(bar, 'R - R - R - 8 - R - R - +1 - 8 -' if name != 'B' else 'R - - R - - R - - R - - 8 - 7 -', 'saw', 0.9)
        s.comp(bar, 'X.......X.......', 'organ', 50, 0.3, 'base', hold=8, vel=0.6, drop_root=False)
        s.stab('intensity', s.b(bar), bar, 'brass', 60, 0.5 if k % 2 == 0 else 0.25, 0.75)
        s.arp(bar, 'calliope', 2, (0, 1, 2, 1, 3, 2, 1, 2), 62, 0.35, 'intensity', dur_steps=2)
        if k == 0:
            s.crash_at(bar, 0.5)
            s.fx('rhythm', K.gong_hit(mtof(38 + s.key), 2.6, 0.9), s.b(bar), 0.5)
        if k == 7:
            s.tom_fill(bar, 8, 0.7)
            s.riser_into(bar + 1, 4, 0.45)
        if k == 3:
            s.scratch_at(s.b(bar, 12), 1.0, 0.4)
    for si, name in enumerate(s.form):
        if name == 'B':
            s.mel(si * 8, bridge, 'calliope', 5, 0.9, double=[('lead:guitar', -12, 0.35, 'melody')])
        else:
            s.mel(si * 8, hook, 'lead:guitar', 4, 0.9, double=[('lead:saw', 12, 0.3, 'melody' if si else 'intensity')])


# ---------------------------------------------------------------- 7. GOLD SHOP
@piece(id='shop_gold', title='Cash Rules the Carnival', bpm=100, key=0, scale='mixolydian', scenes=['shop_gold'], family='toyroom',
       swing=0.28, form='AABA', duck=0.2)
def shop_gold(s):
    """Acid-jazz boom-bap: slap bass, Rhodes ninths, funky sax, register 'ching' on the glock."""
    s.prog('A', '0:9 0:9 5:9 5:9 0:9 9:7 2:m9 7:9')
    s.prog('B', '5:M9 5:M9 4:m7 9:7 2:m9 7:9 0:9 7:7sus')
    hook = '4 . 4 6 7 . 6 4 | 2b - 2 - 0 - . . | 4 . 4 6 7 . 5 4 | 5 - 4 - 2b - . . | 7 . 7 9 . 8 - 7 | 5 - 4 - 2 - 1 - | 3 - 5 - 7 - 8 - | 8 - - - 10 - 8 -'
    bridge = '2 - 3 - 5 - 7 - | 9 - - - 7 - - - | 8 - 7 - 4 - 2 - | 1 - 2 - 4 - 5 - | 3 - - - 2 - 3 - | 5 - - - 4 - 1 - | 0 - 2 - 4 - 6 - | 7 - - - . . . .'
    for bar, name, si, k in every(s):
        s.drums(bar, dict(k='X.....x.X.....x.X.....x.X..x..x.', s='....X.......X...', r='.......o.o......',
                          h='x.x.x.x.x.x.x.x.', sh='.o.o.o.o.o.o.o.o' if si else '', cb='......x.......x.' if si >= 1 else ''),
                dict(k='kickbreak', s='snaretight'))
        s.bass(bar, 'R - . R . . 7 . 8 . . 5 . R . .' if k % 2 == 0 else 'R - . R . . 8 . . 5 . 7 . 3 b .', 'slap', 0.95)
        s.comp(bar, 'X.....x...x.....', 'rhodes', 55, 0.95, 'base', vel=0.7)
        s.comp(bar, '..x...x...x...x.', 'pluck', 64, 0.3, 'rhythm', pan=0.35, hold=1, vel=0.55)
        if k in (3, 7):
            for j, st in enumerate((8, 10, 12)):
                s.put('intensity', inst('glock', 12 * 7 + s.key + (0, 4, 7)[j], 0.4, 0.8), s.b(bar, st), 0.35, 0.4)
        if k % 2 == 1:
            s.stab('intensity', s.b(bar, 14), bar, 'brass', 60, 0.3, 0.55)
        if si == 3:
            s.arp(bar, 'vibes', 4, (0, 2, 1, 3), 67, 0.35, 'intensity')
        if k == 7:
            s.scratch_at(s.b(bar, 12), 1.0, 0.35)
    for si, name in enumerate(s.form):
        if name == 'B':
            s.mel(si * 8, bridge, 'rhodes', 5, 1.0, double=[('vibes', 12, 0.4, 'melody')])
        else:
            s.mel(si * 8, hook, 'lead:sax', 5, 0.85, double=[('lead:whistle', 12, 0.2, 'intensity')] if si else [])
        if si != 2:
            s.vox('melody', s.b(si * 8 + 7, 12), 'yeah', 12 * 5 + s.key + 7, 1.0, 0.35, 0.3)


# ---------------------------------------------------------------- 8. DREAM SHOP
@piece(id='shop_dream', title='Velvet Daydream', bpm=84, key=8, scale='lydian', scenes=['shop_dream'], family='dream',
       bpb=3, form='ABA', lofi=True, wet=1.8, duck=0.15)
def shop_dream(s):
    """Lo-fi dream waltz (3/4): tape wobble, Rhodes, airy vox pads, vibes + whistle, glock dust."""
    s.prog('A', '0:M9 0:M9 2:M 2:M 9:m7 9:m7 7:M 4:m7')
    s.prog('B', '2:M 2:M 0:M9 0:M9 9:m7 4:m7 7:sus 7:M')
    a = '4 - 6 - 5 - | 4 - - - 2 - | 3 - 5 - 3 - | 1 - - - - - | 2 - 4 - 5 - | 7 - - - 6 - | 6 - 4 - 1 - | 2 - - - - -'
    b = '8 - 10 - 9 - | 8 - - - - - | 6 - 8 - 7 - | 4 - - - - - | 5 - 4 - 2 - | 4 - - - 6 - | 5 - 4 - 1 - | 1 - - - - -'
    for bar, name, si, k in every(s):
        s.drums(bar, dict(k='X.......x...', r='....o...o...', h='x.x.x.x.x.x.' if si != 1 else 'x...x...x...', sh='..o...o...o.'),
                dict(k='kickbreak', h='hatlofi'), gain=0.75)
        s.bass(bar, 'R - - - - - - - 5 - - -', 'sub', 0.8)
        s.comp(bar, 'X...........', 'rhodes', 55, 0.9, 'base', vel=0.6)
        s.comp(bar, 'X...........', 'pad:0.25', 60, 0.4, 'base', hold=12)
        s.arp(bar, 'glock', 2, (0, 2, 1, 3, 2, 4), 72, 0.3, 'intensity')
        if k % 2 == 0:
            s.vox('intensity', s.b(bar), 'ooh', 12 * 5 + s.voicing(bar, 60)[1] % 12, 2.6, 0.35, -0.3, hi=True)
    s.mel(0, a, 'vibes', 5, 1.0, double=[('lead:whistle', 0, 0.5, 'melody')])
    s.mel(8, b, 'lead:whistle', 5, 0.85, double=[('vibes', -12, 0.5, 'melody')])
    s.mel(16, a, 'vibes', 5, 1.0, double=[('lead:whistle', 12, 0.3, 'melody'), ('glock', 12, 0.25, 'intensity')])


# ---------------------------------------------------------------- 9. ENDLESS
@piece(id='endless_neverclose', title='Neon Never Closes', bpm=128, key=10, scale='dorian', scenes=['endless'], family='all',
       form='AABA', duck=0.5, duck_stems=('base', 'intensity', 'melody'))
def endless_neverclose(s):
    """Future-funk / filter house: pumping disco chords, octave bass, supersaw hook, 'yeah' chops."""
    s.prog('A', '0:m9 0:m9 5:9 5:9 3:M7 10:M 7:m7 7:7')
    s.prog('B', '8:M7 10:M 0:m9 0:m9 8:M7 10:M 3:M7 7:7')
    hook = '4 . 4 . 6 . 4 3 | 4 - - - . . 7 6 | 4 . 4 . 5 . 4 3 | 5 - - - . . . . | 4 . 4 . 6 . 4 3 | 3 - - - 2 - 1 - | 1 - 3 - 4 - 6 - | 8 - - - 11 - . .'
    bridge = '9 - - - 7 - - - | 8 - - - 7 - 6 - | 11 - - - - - . . | 10 - 11 - 13 - 14 - | 13 - - - 11 - - - | 12 - - - 10 - - - | 11 - - - 9 - - - | 8 - - - - - . .'
    for bar, name, si, k in every(s):
        brk = name == 'B' and k < 4
        s.drums(bar, dict(k='' if brk else 'X...X...X...X...', c='....X.......X...', oh='' if brk else '..x...x...x...x.',
                          h='xxxxxxxxxxxxxxxx' if si >= 1 else 'x.x.x.x.x.x.x.x.', sh='.o.o.o.o.o.o.o.o'))
        s.bass(bar, 'R . 8 . R . 8 . R . 8 . R . 8 .' if not brk else 'R - - - - - - - - - - - - - - -', 'funk', 0.9)
        s.comp(bar, 'x.x.x.x.x.x.x.x.' if not brk else 'X...............', 'pad', 55, 0.6, 'base', stab='disco' if not brk else None,
               hold=None if not brk else 16)
        if not brk:
            s.arp(bar, 'pluck', 1, (0, 2, 4, 2, 1, 3, 5, 3), 67, 0.25, 'intensity', dur_steps=1)
        if k in (1, 3, 5):
            s.vox('intensity', s.b(bar, 10), 'yeah', 12 * 5 + s.key + 5, 0.75, 0.5, 0.25)
        if k == 0:
            s.crash_at(bar, 0.35)
        if k == 7:
            s.riser_into(bar + 1, 8, 0.45)
    for si, name in enumerate(s.form):
        if name == 'B':
            s.mel(si * 8, bridge, 'voxhi:o>a', 4, 0.7, double=[('lead:saw', 0, 0.35, 'melody')])
        else:
            s.mel(si * 8, hook, 'lead:super', 5, 0.8, double=[('lead:square', 12, 0.2, 'intensity')] if si else [])


# ---------------------------------------------------------------- 10. COMBAT EARLY B
@piece(id='combat_spoons', title='Spoon Bandits', bpm=116, key=2, scale='mixolydian', scenes=['combat_early'], family='toyroom',
       form='AABA', duck=0.2)
def combat_spoons(s):
    """Street funk with a horn section: tight drums, popping bass, clav + guitar scratch, sax hook, brass hits."""
    s.prog('A', '0:7 0:7 5:M 5:M 0:7 0:7 10:M 5:M')
    s.prog('B', '9:m7 9:m7 2:m7 7:7 9:m7 2:m7 5:M 7:7')
    hook = '7 . 6 4 . 2 3 - | 2 - 0 - . . . . | 7 . 5 4 . 2 3 - | 5 - 4 - . . . . | 7 . 6 4 . 2 3 - | 4 - 6 - 7 - 9 - | 8 - 7 - 6 - 4 - | 5 - - - 3 - . .'
    bridge = '5 - 4 - 2 - 1 - | 2 - - - . . . . | 1 - 3 - 5 - 7 - | 8 - - - 6 - 4 - | 5 - 4 - 2 - 1 - | 3 - 5 - 7 - 8 - | 9 - 8 - 7 - 5 - | 8 - - - . . . .'
    for bar, name, si, k in every(s):
        s.drums(bar, dict(k='X..x..x...X..x..X..x..x...X.....', s='....X..o....X..o....X..o.o..X.o.',
                          h='xoxoxoxoXoxoxoxo', cg='' if si == 0 else '..o..o.o..o..o.o'), dict(k='kick', s='snaretight'))
        s.bass(bar, 'R . . R 8 . R . . R . 7 8 . 5 .', 'slap', 1.0)
        s.comp(bar, '.x.x..x..x.x..x.', 'clav', 60, 0.5, 'rhythm', pan=-0.3, hold=1, vel=0.65)
        s.comp(bar, 'x...x...x...x...', 'pluck', 64, 0.25, 'rhythm', pan=0.35, hold=1, vel=0.5)
        s.comp(bar, 'X...............', 'organ', 55, 0.3, 'base', hold=15, vel=0.55)
        if k % 2 == 1 and name != 'B':
            for st in (6, 8):
                s.stab('intensity', s.b(bar, st), bar, 'brass', 62, 0.22, 0.75)
            s.vox('intensity', s.b(bar, 12), 'hey', 12 * 5 + s.key + 4, 0.5, 0.4, -0.2)
        if si >= 1:
            s.arp(bar, 'glock', 2, (0, 2, 1, 3, 2, 4, 3, 1), 69, 0.25, 'intensity')
        if k == 0:
            s.crash_at(bar, 0.35)
        if k == 7:
            s.snare_roll(bar, 12, 'snaretight', 0.5)
    for si, name in enumerate(s.form):
        if name == 'B':
            s.mel(si * 8, bridge, 'lead:square', 5, 0.8, double=[('glock', 12, 0.3, 'intensity')])
        else:
            s.mel(si * 8, hook, 'lead:sax', 5, 0.9, double=[('lead:saw', -12, 0.25, 'intensity')] + ([('lead:sax', 12, 0.3, 'melody')] if si == 3 else []))


# ---------------------------------------------------------------- 11. COMBAT MID B
@piece(id='combat_carousel', title='Carousel Heist', bpm=124, key=9, scale='harmmin', scenes=['combat_mid'], family='toyroom',
       swing8=0.33, form='AABA', duck=0.35)
def combat_carousel(s):
    """Electro-swing heist: four-on-the-floor + swung hats, walking bass, sax/calliope hook, brass, scratches."""
    s.prog('A', '0:m6 0:m6 5:m6 5:m6 7:7 7:7 0:m6 7:7')
    s.prog('B', '5:m6 5:m6 0:m6 0:m6 1:7 7:7 0:m6 7:7')
    hook = '7 - 6 7 4 - . . | 2 - 1 - 2 - 4 - | 8 - 7 8 5 - . . | 3 - 2 - 3 - 5 - | 6 - 4 - 1 - 3 - | 4 - 3 - 6 - 7 - | 7 - - - 4 - 2 - | 1 - 6 - 4 - . .'
    bridge = '3 - 5 - 7 - 8 - | 10 - - - 8 - 7 - | 9 - 7 - 4 - 2 - | 4 - - - . . . . | 5 - 3 - 6 - 5 - | 4 - 6 - 8 - 7 - | 7 - 4 - 2 - 4 - | 6 - - - . . . .'
    for bar, name, si, k in every(s):
        s.drums(bar, dict(k='X...X...X...X...', c='....X.......X...', h='x.x.x.x.x.x.x.x.', oh='..x...x...x...x.' if si else '',
                          rd='x...x.x.x...x.x.' if si >= 1 else ''), dict(k='kickbreak'))
        walk = 'R - . . 3 - . . 5 - . . 6 - . .' if k % 2 == 0 else 'R - . . 5 - . . 8 - . . b - . .'
        s.bass(bar, walk, 'funk', 1.0)
        s.comp(bar, '....x.......x...', 'pluck', 60, 0.45, 'rhythm', hold=2, vel=0.7, pan=0.3)
        s.comp(bar, 'X.......X.......', 'organ', 55, 0.3, 'base', hold=6, vel=0.55)
        s.stab('intensity', s.b(bar, 6), bar, 'brass', 60, 0.25, 0.6)
        if k % 2 == 1:
            s.stab('intensity', s.b(bar, 12), bar, 'brass', 60, 0.5, 0.7)
        if si >= 1:
            s.arp(bar, 'calliope', 2, (0, 1, 2, 3, 2, 1, 0, 1), 69, 0.25, 'intensity')
        if k in (3, 7):
            s.scratch_at(s.b(bar, 12), 1.0, 0.45)
        if k == 0:
            s.crash_at(bar, 0.35)
        if k == 7 and si != 3:
            s.vox('intensity', s.b(bar, 8), 'hey', 12 * 5 + s.key + 7, 0.5, 0.45)
    for si, name in enumerate(s.form):
        if name == 'B':
            s.mel(si * 8, bridge, 'calliope', 5, 1.0, double=[('lead:sax', -12, 0.35, 'melody')])
        else:
            s.mel(si * 8, hook, 'lead:sax', 5, 0.85, double=[('calliope', 12, 0.3, 'intensity')] if si else [])


# ---------------------------------------------------------------- 12. COMBAT LATE B
@piece(id='combat_lasttrain', title='Last Train Breakbeat', bpm=166, key=0, scale='minor', scenes=['combat_late'], family='office',
       form='AABA', duck=0.3)
def combat_lasttrain(s):
    """Jungle / breakbeat: chopped two-bar break, reese + wobble, square hook, brass hits, ride."""
    s.prog('A', '0:m9 0:m9 8:M7 8:M7 3:M 10:M 5:m9 7:7')
    s.prog('B', '8:M 10:M 0:m 0:m 8:M 10:M 7:7sus 7:7#9')
    hook = '7 . 6 4 . 4 6 7 | 9 - 8 - 7 - - - | 7 . 6 4 . 4 6 7 | 5 - - - 4 - 2 - | 7 . 6 4 . 4 6 7 | 8 - 6 - 3 - 1 - | 5 - 3 - 7 - 9 - | 8 - - - 4 - . .'
    bridge = '9 - - - 7 - - - | 8 - - - 10 - - - | 11 - - - - - . . | 12 - 11 - 9 - 8 - | 9 - - - 11 - - - | 10 - - - 8 - - - | 7 - - - 6 - - - | 6 - - - . . . .'
    for bar, name, si, k in every(s):
        half = name == 'B' and k < 4
        s.drums(bar, dict(k='X.........X.......X...X...X.....' if not half else 'X.......X.......',
                          s='....X..o.o..X..o....X.o.....XoXo' if not half else '........X.......',
                          h='xoxoxoxoxoxoxoxo', rd='x.x.x.x.x.x.x.x.' if si >= 1 else '', sh='o.o.o.o.o.o.o.o.'),
                dict(k='kickbreak', s='snaretight'))
        s.bass(bar, 'R - - - - - - - . . R - - - 5 -', 'wobble' if name == 'B' else 'reese', 0.8)
        s.bass(bar, 'R - - - - - - - . . R - - - - -', 'sub', 0.55)
        s.comp(bar, 'X...............', 'pad:0.5', 55, 0.4, 'base', hold=16)
        if not half:
            s.stab('intensity', s.b(bar, 0), bar, 'brass', 60, 0.25 if k % 2 else 0.5, 0.6)
            s.arp(bar, 'lead:square', 2, (0, 3, 2, 5, 4, 7, 6, 3), 64, 0.18, 'intensity', dur_steps=1)
        if k == 0:
            s.crash_at(bar, 0.4)
        if k == 7:
            s.riser_into(bar + 1, 8, 0.4)
            s.scratch_at(s.b(bar, 8), 2.0, 0.35, rate=9.0)
    for si, name in enumerate(s.form):
        if name == 'B':
            s.mel(si * 8, bridge, 'lead:saw', 4, 0.75, double=[('voxhi:a', 12, 0.3, 'intensity')])
        else:
            s.mel(si * 8, hook, 'lead:square', 5, 0.85, double=[('lead:super', 0, 0.3, 'melody' if si else 'intensity')])


# ---------------------------------------------------------------- 13. BOSS B
@piece(id='boss_clown', title='Clown Kingpin', bpm=150, key=4, scale='phrygian', scenes=['boss'], family='all',
       form='AABA', duck=0.35)
def boss_clown(s):
    """Drift phonk: cowbell hook in dotted rhythm, 808 slides, half-time clap, distorted guitar answers."""
    s.prog('A', '0:m 1:M 0:m 1:M 0:m 1:M 8:M 7:dim')
    s.prog('B', '5:m 5:m 8:M 8:M 1:M 1:M 3:M 7:dim')
    cow = ['7 . . 7 . . 6 . 5 . . 4 . . 5 .', '5 . . 5 . . 3 . 5 . . 7 . . 8 .', '7 . . 7 . . 6 . 5 . . 4 . . 5 .',
           '8 . . 8 . . 7 . 5 . . 3 . . 1 .', '7 . . 7 . . 6 . 5 . . 4 . . 5 .', '5 . . 5 . . 3 . 5 . . 7 . . 8 .',
           '9 . . 9 . . 7 . 5 . . 4 . . 2 .', '8 . . 8 . . 6 . 4 . . 1 . . 4 .']
    cowbell = ' | '.join(cow)
    bridge = '3 - 5 - 7 - 8 - | 7 - - - - - . . | 9 - 8 - 7 - 5 - | 7 - - - - - . . | 8 - 9 - 10 - 12 - | 10 - - - - - . . | 9 - - - 6 - - - | 8 - - - 4 - - -'
    for bar, name, si, k in every(s):
        s.drums(bar, dict(k='X......X..X.....X......X..X..X..', c='........X.......',
                          h='x.x.x.x.x.xxx.x.x.x.x.x.xxxxx.x.', oh='......x.......x.' if si else ''),
                dict(k='kick'))
        s.bass(bar, 'R - - - - - - R - - R - - - - -' if k % 2 == 0 else 'R - - - - - - R - - 8 - - - 7 -', 'sub', 1.0)
        s.bass(bar, 'R - - - - - - R - - R - - - - -', 'saw', 0.3)
        s.comp(bar, 'X...............', 'pad:0.3', 52, 0.45, 'base', hold=16)
        if name != 'B':
            s.comp(bar, '........x.......', 'lead:guitar', 52, 0.3, 'intensity', hold=6, vel=0.7, drop_root=False)
        if k == 0:
            s.crash_at(bar, 0.4)
            s.fx('rhythm', K.impact_boom(1.6, 0.8, bar), s.b(bar), 0.35)
        if k == 7:
            s.riser_into(bar + 1, 4, 0.4)
            s.vox('intensity', s.b(bar, 12), 'hey', 12 * 4 + s.key + 7, 0.5, 0.5)
    for si, name in enumerate(s.form):
        if name == 'B':
            s.mel(si * 8, bridge, 'lead:guitar', 4, 0.85, double=[('lead:saw', 12, 0.25, 'melody')])
            s.mel(si * 8, cowbell, 'cowbell', 5, 0.6, stem='intensity', grid=16, snap=False)
        else:
            s.mel(si * 8, cowbell, 'cowbell', 5, 1.0, grid=16, snap=False,
                  double=[('glock', 12, 0.25, 'intensity')] if si else [])


# ---------------------------------------------------------------- 14. GOLD SHOP B
@piece(id='shop_layaway', title='Layaway Lounge', bpm=104, key=5, scale='major', scenes=['shop_gold'], family='toyroom',
       swing=0.16, form='AABA', duck=0.15)
def shop_layaway(s):
    """Smooth city-pop / jazz-funk lounge: slap bass, Rhodes, sax hook, vibes bridge, congas."""
    s.prog('A', '0:M9 4:m7 2:m9 7:9 0:M9 9:m9 2:m9 7:7sus')
    s.prog('B', '5:M9 4:m7 2:m9 0:M9 5:M9 4:m7 9:m9 7:7sus')
    hook = '4 - 5 4 2 - 1 - | 2 - - - 4 - 6 - | 5 - 4 - 3 - 1 - | 2 - - - 1 - - - | 4 - 5 4 2 - 1 - | 2 - 4 - 6 - 7 - | 8 - 7 - 5 - 3 - | 4 - - - . . . .'
    bridge = '5 - 4 - 7 - 9 - | 8 - - - 6 - - - | 7 - 5 - 3 - 1 - | 2 - - - 6 - - - | 5 - 4 - 7 - 9 - | 11 - - - 9 - - - | 8 - 7 - 6 - 4 - | 5 - - - 4 - . .'
    for bar, name, si, k in every(s):
        s.drums(bar, dict(k='X..x..X...x.....', s='....X.......X...', r='..o.......o....o', h='x.xxx.xxx.xxx.xx',
                          cg='..o..o....o..o..' if si else '', cl='o.......o.o.....' if si else ''),
                dict(k='kickbreak', s='snaretight', h='hatlofi'), gain=0.9)
        s.bass(bar, 'R - . R . . 8 . . R . . 5 . 7 .' if k % 2 == 0 else 'R - . . 5 . 8 . . R . 7 . 5 3 .', 'slap', 0.9)
        s.comp(bar, 'X.....x.......x.', 'rhodes', 55, 0.95, 'base', vel=0.65)
        s.comp(bar, '..x..x....x..x..', 'clav', 62, 0.2, 'rhythm', pan=-0.3, hold=1, vel=0.5)
        s.arp(bar, 'vibes', 4, (0, 2, 1, 3), 67, 0.3, 'intensity')
        if k == 7:
            s.stab('intensity', s.b(bar, 12), bar, 'brass', 60, 0.5, 0.5)
    for si, name in enumerate(s.form):
        if name == 'B':
            s.mel(si * 8, bridge, 'vibes', 5, 1.0, double=[('lead:whistle', 12, 0.3, 'melody')])
        else:
            s.mel(si * 8, hook, 'lead:sax', 5, 0.85, double=[('vibes', 12, 0.3, 'intensity')] if si else [])


# ---------------------------------------------------------------- 15. ENDLESS B
@piece(id='endless_encore', title='Encore Overdrive', bpm=138, key=7, scale='dorian', scenes=['endless'], family='all',
       form='AABA', duck=0.45, duck_stems=('base', 'intensity', 'melody'))
def endless_encore(s):
    """Hard electro-funk: pumping supersaw hook, saw bass, brass stabs, breakdown with vocal 'ah' and build."""
    s.prog('A', '0:m9 0:m9 3:M7 3:M7 10:M 10:M 5:9 5:9')
    s.prog('B', '8:M7 10:M 0:m9 0:m9 8:M7 10:M 2:m7 7:7')
    hook = '4 . 4 . 6 4 . 2 | 3 - 2 - 0 - . . | 4 . 4 . 6 4 . 2 | 6 - 4 - 2 - . . | 4 . 4 . 6 4 . 2 | 7 - 8 - 7 - 6 - | 4 . 4 . 5 4 . 2 | 5 - - - 4 - . .'
    bridge = '9 - - - 7 - - - | 7 - - - 6 - - - | 9 - - - 11 - - - | 10 - 9 - 7 - 6 - | 9 - - - 11 - 10 - | 9 - - - 8 - - - | 8 - - - 10 - - - | 8 - - - 11 - . .'
    for bar, name, si, k in every(s):
        brk = name == 'B' and k < 4
        build = name == 'B' and k >= 4
        s.drums(bar, dict(k='' if brk else ('X...X...X...X...' if not build else 'X.X.X.X.X.X.X.X.'),
                          c='....X.......X...' if not brk else '', oh='' if brk else '..x...x...x...x.',
                          h='xxxxxxxxxxxxxxxx', sh='.o.o.o.o.o.o.o.o'), dict(k='kick'))
        s.bass(bar, 'R . R 8 R . R 8 R . R 8 R . 7 8' if not brk else 'R - - - - - - - - - - - - - - -', 'saw', 0.85)
        s.comp(bar, 'x.x.x.x.x.x.x.x.' if not brk else 'X...............', 'pad', 55, 0.55, 'base',
               stab='pluck' if not brk else None, hold=None if not brk else 16)
        if not brk:
            s.stab('intensity', s.b(bar), bar, 'brass', 62, 0.4, 0.6)
            s.stab('intensity', s.b(bar, 10), bar, 'brass', 62, 0.25, 0.55)
            s.arp(bar, 'lead:square', 1, (0, 2, 4, 2, 1, 3, 5, 3), 67, 0.15, 'intensity', dur_steps=1)
        if k == 0:
            s.crash_at(bar, 0.4)
        if build and k == 7:
            s.snare_roll(bar, 0, 'clap', 0.45)
        if k == 7:
            s.riser_into(bar + 1, 8 if build else 4, 0.45)
        if k in (1, 5) and not brk:
            s.vox('intensity', s.b(bar, 12), 'woo', 12 * 6 + s.key, 0.75, 0.4, 0.3, hi=True)
    for si, name in enumerate(s.form):
        if name == 'B':
            s.mel(si * 8, bridge, 'voxhi:a', 4, 0.7, double=[('lead:saw', 0, 0.35, 'melody')])
        else:
            s.mel(si * 8, hook, 'lead:super', 5, 0.8, double=[('lead:square', 12, 0.2, 'intensity')] + ([('lead:super', -12, 0.3, 'melody')] if si == 3 else []))


# =================================================================== render / encode
def make(pid):
    kw, fn = PIECES[pid]
    s = Song(**kw)
    fn(s)
    return s


def entry(s):
    return dict(id=s.id, title=s.title, bpm=s.bpm, beatsPerBar=s.bpb, bars=s.bars, loopSeconds=round(s.loop_seconds, 6),
                loopSamples=s.n, sampleRate=SR, scenes=s.scenes, family=s.family,
                stems=['Music/%s/%s_%s' % (s.id, s.id, k) for k in STEMS])


def write_ogg(path, buf):
    """Vorbis via libsndfile. Written in blocks: one huge write() call crashes libsndfile's Vorbis encoder on Windows."""
    import soundfile as sf
    x = np.clip(buf, -1, 1).astype(np.float32)
    with sf.SoundFile(path, 'w', SR, 2, format='OGG', subtype='VORBIS', compression_level=0.25) as f:
        for i in range(0, len(x), 32768):
            f.write(x[i:i + 32768])


def render(pid):
    import time
    t0 = time.time()
    s = make(pid)
    stems = s.finish()
    d = os.path.join(OUT, pid)
    os.makedirs(d, exist_ok=True)
    for k, buf in stems.items():
        path = os.path.join(d, '%s_%s.ogg' % (pid, k))
        write_ogg(path, buf)
    return pid, s.loop_seconds, time.time() - t0


def main(only, jobs=1):
    ids = [p for p in PIECES if not only or p in only]
    if jobs > 1:
        with Pool(min(jobs, len(ids))) as pool:
            results = pool.imap_unordered(render, ids)
            for pid, secs, dt in results:
                print('rendered %-20s %5.1fs loop  (%.0fs)' % (pid, secs, dt), flush=True)
    else:
        for pid in ids:
            pid, secs, dt = render(pid)
            print('rendered %-20s %5.1fs loop  (%.0fs)' % (pid, secs, dt), flush=True)
    present = []
    for pid in PIECES:
        e = entry(make_meta(pid))
        if all(os.path.exists(os.path.join(ROOT, 'Assets', 'ArcaneBrigade', 'Resources', 'Audio', 'Arcane', st + '.ogg')) for st in e['stems']):
            present.append(e)
    with open(os.path.join(OUT, 'music.json'), 'w', newline='\n') as f:
        json.dump({'version': 1, 'pieces': present}, f, indent=1)


def make_meta(pid):
    kw, _ = PIECES[pid]
    return Song(alloc=False, **kw)


if __name__ == '__main__':
    args = sys.argv[1:]
    jobs = 1
    if '-j' in args:
        i = args.index('-j')
        jobs = int(args[i + 1])
        del args[i:i + 2]
    main(set(args), jobs)
