"""Arcane Brigade SFX v6 toolkit: procedural foley (numpy/scipy) + CC0 Kenney samples, one dry small-room palette.

Every function returns a mono float64 array at SR. Layers are mixed with place(); finish() applies the shared palette
(high-pass, gentle top roll-off, a short small-room reflection, fade, length, peak level).
"""
import glob
import math
import os

import numpy as np
import soundfile as sf
from scipy import signal

SR = 44100
KENNEY = os.environ.get('AB_KENNEY', 'kenney')
_cache = {}


class Sig(np.ndarray):
    """1-D signal whose + zero-pads the shorter operand, so layers of different lengths mix directly."""
    def __add__(self, other):
        if isinstance(other, np.ndarray) and self.ndim == 1 and other.ndim == 1 and len(other) != len(self):
            out = np.zeros(max(len(self), len(other)))
            out[:len(self)] += self
            out[:len(other)] += other
            return out.view(Sig)
        return np.ndarray.__add__(self, other)

    __radd__ = __add__


def N(d):
    return max(1, int(round(d * SR)))


def tt(d):
    return np.arange(N(d)) / SR


# ----------------------------------------------------------------------------------------------- basics
def noise(d, rng, color='white'):
    x = rng.standard_normal(N(d))
    if color == 'pink':
        b, a = [0.049922035, -0.095993537, 0.050612699, -0.004408786], [1, -2.494956002, 2.017265875, -0.522189400]
        x = signal.lfilter(b, a, x) * 4
    elif color == 'brown':
        x = np.cumsum(x) * 0.02
        x = signal.lfilter([1, -1], [1, -0.995], x)
    return x


def lp(x, f, order=2):
    return signal.sosfilt(signal.butter(order, min(f, SR / 2 - 100), 'lowpass', fs=SR, output='sos'), x)


def hp(x, f, order=2):
    return signal.sosfilt(signal.butter(order, f, 'highpass', fs=SR, output='sos'), x)


def bp(x, f, q=2.0, order=2):
    lo, hi = f / (1 + 0.5 / q), min(f * (1 + 0.5 / q), SR / 2 - 200)
    return signal.sosfilt(signal.butter(order, [lo, hi], 'bandpass', fs=SR, output='sos'), x)


def reson(x, f, q=10.0):
    b, a = signal.iirpeak(min(f, SR / 2 - 200), q, fs=SR)
    return signal.lfilter(b, a, x)


def svf(x, fc, q=1.0, mode='bp'):
    """Time-varying state-variable filter (TPT). fc may be a scalar or an array the length of x."""
    fc = np.broadcast_to(np.asarray(fc, dtype=float), x.shape)
    g = np.tan(np.pi * np.clip(fc, 20, SR * 0.45) / SR)
    k = 1.0 / q
    a1 = 1 / (1 + g * (g + k))
    ic1 = ic2 = 0.0
    out = np.empty_like(x)
    lo_ = mode == 'lp'
    hi_ = mode == 'hp'
    for i in range(len(x)):
        v3 = x[i] - ic2
        v1 = a1[i] * ic1 + g[i] * a1[i] * v3
        v2 = ic2 + g[i] * v1
        ic1 = 2 * v1 - ic1
        ic2 = 2 * v2 - ic2
        out[i] = v2 if lo_ else (x[i] - k * v1 - v2) if hi_ else v1
    return out


def env_exp(d, tau, attack=0.002):
    t = tt(d)
    e = np.exp(-t / tau)
    a = N(attack)
    e[:a] *= np.linspace(0, 1, a)
    return e


def env_ar(d, a, r, curve=2.0):
    """Rise over a, hold, fall over the last r seconds."""
    n = N(d)
    e = np.ones(n)
    na, nr = min(N(a), n), min(N(r), n)
    e[:na] = np.linspace(0, 1, na) ** curve
    e[n - nr:] *= np.linspace(1, 0, nr) ** curve
    return e


def bell_env(d, peak=0.5, p=2.0):
    t = np.linspace(0, 1, N(d))
    return np.where(t < peak, (t / peak) ** p, ((1 - t) / (1 - peak)) ** p)


def line(d, a, b, curve=1.0):
    return a + (b - a) * np.linspace(0, 1, N(d)) ** curve


def place(out, x, t0, gain=1.0):
    i = int(round(t0 * SR))
    if i >= len(out):
        return out
    m = min(len(x), len(out) - i)
    out[i:i + m] += x[:m] * gain
    return out


def pitch(x, ratio):
    """Resample (speed + pitch) by ratio > 1 higher/shorter."""
    n = int(len(x) / ratio)
    return np.interp(np.arange(n) * ratio, np.arange(len(x)), x)


def norm(x, peak=1.0):
    m = np.abs(x).max()
    return x * (peak / m) if m > 0 else x


def osc(freq, d, phase=0.0):
    f = np.broadcast_to(np.asarray(freq, dtype=float), (N(d),))
    return np.sin(2 * np.pi * np.cumsum(f) / SR + phase)


# ----------------------------------------------------------------------------------------------- Kenney CC0 samples
def sample(pattern, idx=0, rng=None):
    """Load a Kenney CC0 sample (glob inside the packs), mono, peak 1. idx picks among matches (rng chooses if given)."""
    files = sorted(glob.glob(os.path.join(KENNEY, '*', 'Audio', pattern)))
    if not files:
        raise FileNotFoundError(pattern)
    f = files[rng.integers(len(files)) if rng is not None else idx % len(files)]
    if f not in _cache:
        x, sr = sf.read(f, always_2d=True)
        x = x.mean(axis=1)
        if sr != SR:
            x = signal.resample_poly(x, SR, sr)
        _cache[f] = norm(x)
    return _cache[f].copy().view(Sig)


# ----------------------------------------------------------------------------------------------- synthesis primitives
def modal(partials, d, rng=None):
    """partials: [(freq, decay_seconds, amp)] -> damped sines (bells, coins, porcelain, tines)."""
    t = tt(d)
    out = np.zeros_like(t)
    for f, dec, a in partials:
        if f >= SR / 2 - 500:
            continue
        ph = rng.uniform(0, 2 * np.pi) if rng is not None else 0
        out += a * np.exp(-t / dec) * np.sin(2 * np.pi * f * t + ph)
    a = N(0.0015)
    out[:a] *= np.linspace(0, 1, a)
    return out


BELL_RATIOS = {'church': [0.5, 1.0, 1.183, 1.506, 2.0, 2.514, 2.662, 3.011, 4.166], 'brass': [1.0, 2.0, 2.76, 3.0, 4.07, 5.4],
               'coin': [1.0, 2.32, 4.25, 6.63, 9.38], 'porcelain': [1.0, 2.76, 5.40, 8.93], 'glass': [1.0, 2.32, 4.25, 6.63],
               'tine': [1.0, 5.4, 11.3]}


def bell(f0, d, kind='church', decay=2.0, rng=None, bright=1.0):
    r = BELL_RATIOS[kind]
    parts = [(f0 * k, decay / (1 + 0.6 * i * bright), 1.0 / (1 + 0.5 * i)) for i, k in enumerate(r)]
    return modal(parts, d, rng)


def click(d=0.004, rng=None, f=3000, q=3):
    rng = rng or np.random.default_rng(0)
    return bp(noise(d, rng) * env_exp(d, d / 3, 0.0003), f, q).view(Sig)


def thump(f0, d, rng, drop=0.5, tau=0.12, noise_amt=0.3, lpf=900):
    """Padded low impact: a sine with a falling pitch plus a short low-passed noise burst."""
    f = f0 * (1 + drop * np.exp(-tt(d) / 0.02))
    body = osc(f, d) * env_exp(d, tau, 0.001)
    nz = lp(noise(d, rng), lpf) * env_exp(d, tau / 3, 0.0005) * noise_amt * 3
    return body + nz


def creak(d, rng, rate=(18, 40), bodies=((220, 9), (520, 11), (1150, 12), (2300, 10)), jitter=0.25, amp_env=None):
    """Stick-slip wood creak: an irregular pulse train (rate Hz, start->end) through wood-body resonances."""
    n = N(d)
    r = np.interp(np.arange(n), [0, n - 1], rate) * (1 + 0.15 * np.sin(2 * np.pi * rng.uniform(0.5, 2) * np.arange(n) / SR))
    pulses = np.zeros(n)
    pos = 0.0
    while pos < n:
        i = int(pos)
        pulses[i] = rng.uniform(0.4, 1.0)
        pos += SR / r[i] * (1 + rng.uniform(-jitter, jitter))
    exc = signal.lfilter([1, -0.6], [1], pulses) + 0.02 * noise(d, rng)
    out = sum(reson(exc, f * rng.uniform(0.95, 1.05), q) * (1.0 / (1 + k * 0.4)) for k, (f, q) in enumerate(bodies))
    return out * (amp_env if amp_env is not None else bell_env(d, 0.4, 1.0))


def scrape(d, rng, f=900, q=1.2, grain=45, rough=0.8):
    """Dragging across a floor: noise chopped by rough grains (stick-slip), band-limited."""
    n = N(d)
    g = np.abs(lp(noise(d, rng), grain)) * 8
    g = (1 - rough) + rough * np.clip(g, 0, 2)
    return bp(noise(d, rng) * g, f, q)


def whoosh(d, rng, f0=400, f1=1500, q=1.5, color='pink', peak=0.55):
    fc = np.exp(np.interp(np.linspace(0, 1, N(d)), [0, peak, 1], [math.log(f0), math.log(f1), math.log(f0 * 0.8)]))
    return svf(noise(d, rng, color), fc, q, 'bp') * bell_env(d, peak, 1.6)


def flutter(d, rng, rate=40, f=(2500, 6000), dur=(0.004, 0.02), env=None):
    """Paper flutter / card riffle: many short high noise bursts."""
    out = np.zeros(N(d))
    k = int(rate * d)
    for _ in range(k):
        bd = rng.uniform(*dur)
        b = bp(noise(bd, rng), rng.uniform(*f), 1.5) * env_exp(bd, bd / 3, 0.0005) * rng.uniform(0.3, 1)
        place(out, b, rng.uniform(0, d - bd))
    return out * (env if env is not None else 1)


def crackle(d, rng, rate=60, f=(1500, 6000), amp=(0.2, 1.0), env=None):
    out = np.zeros(N(d))
    for _ in range(int(rate * d)):
        c = click(rng.uniform(0.0006, 0.003), rng, rng.uniform(*f), 2) * rng.uniform(*amp)
        place(out, c, rng.uniform(0, d))
    return out * (env if env is not None else 1)


def wind(d, rng, f=500, depth=0.6, speed=0.4, q=0.8):
    lfo = 1 + depth * np.sin(2 * np.pi * speed * tt(d) + rng.uniform(0, 6)) * np.sin(2 * np.pi * speed * 0.37 * tt(d))
    return svf(noise(d, rng, 'pink'), f * lfo, q, 'bp') * (0.6 + 0.4 * lfo)


def rumble(d, rng, f=90):
    return lp(noise(d, rng, 'brown'), f, 4) * 3 + 0.3 * osc(f * 0.45 * (1 + 0.05 * np.sin(2 * np.pi * 0.7 * tt(d))), d)


def roar(d, rng, f=600):
    """Flame roar: low-passed noise with flicker."""
    flick = 1 + 0.5 * lp(noise(d, rng), 12) * 10
    return lp(noise(d, rng, 'pink'), f, 2) * np.clip(flick, 0.2, 2)


def sizzle(d, rng, rate=900):
    am = np.clip(lp(noise(d, rng), rate) * 6, 0, None)
    return hp(noise(d, rng), 3500) * am


def drip(rng, f=900, d=0.06):
    f_curve = f * (1 + 1.8 * (1 - np.exp(-tt(d) / 0.01)))
    return osc(f_curve, d) * env_exp(d, 0.012, 0.0005) + click(0.002, rng, 4000) * 0.4


def ticks(times, rng, f=(2600, 900), q=(14, 9), tau=0.012, d=None):
    d = d or (max(times) + 0.1)
    out = np.zeros(N(d))
    for i, t0 in enumerate(times):
        exc = noise(0.003, rng) * env_exp(0.003, 0.0008, 0.0002)
        c = reson(exc, f[0] * (1.0 if i % 2 == 0 else 0.84), q[0]) + 0.6 * reson(exc, f[1], q[1])
        c = np.concatenate([c, np.zeros(N(tau * 4))])
        c[:] = signal.lfilter([1], [1, -math.exp(-1 / (tau * SR))], c) * 0.05 + c
        place(out, c, t0, 1.0 if i % 2 == 0 else 0.75)
    return out


def tine_notes(notes, d, rng, decay=1.2, vel=None, jitter=0.0):
    """Music-box style plucked tines: notes = [(time, midi)]."""
    out = np.zeros(N(d))
    for i, (t0, m) in enumerate(notes):
        f = 440 * 2 ** ((m - 69) / 12)
        x = bell(f, min(decay * 2, d), 'tine', decay, rng)
        place(x, click(0.002, rng, 5000), 0, 0.15)
        place(out, x, t0 + rng.uniform(-jitter, jitter), (vel[i] if vel else 1.0))
    return out


def voiced(d, f0, rng, formants=((500, 6), (1000, 7), (2500, 8)), breath=0.15, shimmer=0.03):
    """Glottal-ish pulse source through formant resonators (grunts, hums, snores, chuckles). f0 scalar or array."""
    f = np.broadcast_to(np.asarray(f0, dtype=float), (N(d),)) * (1 + shimmer * lp(noise(d, rng), 8) * 6)
    ph = np.cumsum(f) / SR
    saw = 2 * (ph - np.floor(ph)) - 1
    src = lp(saw, 1800) + breath * noise(d, rng, 'pink')
    out = sum(reson(src, fq, q) * (1 / (1 + i * 0.5)) for i, (fq, q) in enumerate(formants))
    return out


def breath(d, rng, f=(700, 1200, 2600), q=4, env=None):
    src = noise(d, rng, 'pink')
    out = sum(reson(src, fq, q) for fq in f) * 0.4 + hp(src, 2000) * 0.15
    return out * (env if env is not None else bell_env(d, 0.4))


def whispers(d, rng, voices=6, rate=5):
    """Unintelligible overlapping whispers: noise through jumping formant pairs, syllable-gated."""
    out = np.zeros(N(d))
    vowels = [(300, 2300), (450, 1900), (700, 1200), (600, 1000), (350, 800), (500, 1700)]
    for _ in range(voices):
        seg = int(SR / rate * rng.uniform(0.7, 1.3))
        n = N(d)
        f1 = np.zeros(n)
        f2 = np.zeros(n)
        gate = np.zeros(n)
        i = int(rng.uniform(0, seg))
        while i < n:
            v = vowels[rng.integers(len(vowels))]
            L = int(seg * rng.uniform(0.6, 1.4))
            f1[i:i + L], f2[i:i + L] = v
            g = np.hanning(min(L, n - i)) * (rng.uniform(0.2, 1.0) if rng.random() > 0.2 else 0)
            gate[i:i + len(g)] = g
            i += L
        f1[f1 == 0] = 500
        f2[f2 == 0] = 1500
        src = noise(d, rng, 'pink')
        v = svf(src, lp(f1, 30), 6, 'bp') + 0.7 * svf(src, lp(f2, 30), 7, 'bp') + 0.25 * hp(src, 3000)
        out += v * gate * rng.uniform(0.4, 1.0)
    return out


def organ(freqs, d, rng, detune=0.004, breath_amt=0.12, ranks=(1, 2, 3, 4, 6, 8), env=None):
    t = tt(d)
    out = np.zeros_like(t)
    for f in freqs:
        for k, h in enumerate(ranks):
            for dt in (-detune, detune):
                out += np.sin(2 * np.pi * f * h * (1 + dt) * t + rng.uniform(0, 6)) / (h ** 1.1) * 0.5
    out = lp(out, 4500)
    out += breath_amt * bp(noise(d, rng), 1500, 0.7) * len(freqs)
    return out * (env if env is not None else env_ar(d, 0.06, 0.12, 1))


def kazoo(f0, d, rng):
    f = np.broadcast_to(np.asarray(f0, dtype=float), (N(d),))
    ph = np.cumsum(f * (1 + 0.012 * np.sin(2 * np.pi * 5.5 * tt(d)))) / SR
    saw = 2 * (ph - np.floor(ph)) - 1
    buzz = np.tanh(saw * 3 + 0.3 * noise(d, rng))
    return sum(reson(buzz, fq, q) for fq, q in ((650, 5), (1100, 6), (2600, 5))) * 0.5


def boing(d, f0, rng):
    f = f0 * (1 + 0.35 * np.exp(-tt(d) / 0.08)) * (1 + 0.06 * np.sin(2 * np.pi * 14 * tt(d)) * np.exp(-tt(d) / 0.3))
    return osc(f, d) * env_exp(d, 0.25, 0.002) + 0.3 * osc(f * 2.7, d) * env_exp(d, 0.08)


def snare_tap(rng, v=1.0, d=0.12):
    return (hp(noise(d, rng), 1500) * env_exp(d, 0.04, 0.0005) * 0.8 + osc(190, d) * env_exp(d, 0.03) * 0.5) * v


def coin_clink(rng, f=None, d=0.35):
    f = f or rng.uniform(2600, 4200)
    return bell(f, d, 'coin', rng.uniform(0.06, 0.14), rng) + click(0.002, rng, 6000) * 0.3


# ----------------------------------------------------------------------------------------------- palette
_ROOM = None


def room_ir():
    global _ROOM
    if _ROOM is None:
        rng = np.random.default_rng(7)
        d = 0.45
        ir = noise(d, rng) * np.exp(-tt(d) * 6.91 / 0.32)
        ir = lp(ir, 5000)
        for dl, g in ((0.007, 0.5), (0.013, 0.35), (0.019, 0.3), (0.027, 0.2)):
            ir[N(dl)] += g
        ir[:N(0.004)] = 0
        _ROOM = ir / np.sqrt((ir ** 2).sum())
    return _ROOM


def finish(x, d, wet=0.12, peak_db=-3.0, hpf=45, lpf=12500, fade=0.03):
    x = hp(x, hpf, 2)
    x = lp(x, lpf, 2)
    y = x + wet * signal.fftconvolve(x, room_ir())[:len(x)] * 3
    n = N(d)
    y = y[:n] if len(y) >= n else np.concatenate([y, np.zeros(n - len(y))])
    f = min(N(fade), n // 4)
    y[n - f:] *= np.linspace(1, 0, f) ** 2
    y[:N(0.001)] *= np.linspace(0, 1, N(0.001))
    return norm(y, 10 ** (peak_db / 20))
