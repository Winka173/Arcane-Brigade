"""Street-funk / electro instrument kit for the Arcane Brigade "Neon Carnival" sound.

Direction (see Docs/Audio/Audio_Direction_Palette.md): urban funk, breakbeat, electro and lo-fi hip-hop grooves
in the spirit of Zenless Zone Zero's soundtrack, crossed with a haunted-carnival colour (calliope organ, toy
percussion). Everything is synthesised here from oscillators and noise with numpy/scipy: no samples, no
recordings, nothing extracted from any game. Build-time tool only.

All instruments return mono float64 arrays at synth.SR.
"""
import numpy as np
from scipy import signal

from synth import SR, TAU, mtof, lp, hp, bp, fade

_RNG = np.random.default_rng


# ------------------------------------------------------------------ building blocks
def ts(n):
    return np.arange(n) / SR


def env_exp(n, a=0.002, d=0.2, s=0.0, hold=0.0, r=0.01):
    t = ts(n)
    e = np.minimum(1.0, t / max(a, 1e-5))
    rel = np.where(t < a + hold, 1.0, s + (1 - s) * np.exp(-(t - a - hold) / max(d, 1e-5)))
    e = e * rel
    k = int(r * SR)
    if 0 < k < n:
        e[-k:] *= np.linspace(1, 0, k) ** 1.5
    return e


def saw(ph):
    """Band-limited-ish sawtooth from phase in radians (via polyBLEP)."""
    x = (ph / TAU) % 1.0
    y = 2 * x - 1
    dt = np.abs(np.gradient(ph)) / TAU
    dt = np.clip(dt, 1e-6, 0.5)
    m1 = x < dt
    t1 = x[m1] / dt[m1]
    y[m1] -= t1 + t1 - t1 * t1 - 1
    m2 = x > 1 - dt
    t2 = (x[m2] - 1) / dt[m2]
    y[m2] -= t2 * t2 + t2 + t2 + 1
    return y


def pulse(ph, width=0.5):
    return saw(ph) - saw(ph + TAU * width)


def phase_of(freq, n):
    f = np.broadcast_to(np.asarray(freq, dtype=np.float64), (n,))
    return TAU * np.cumsum(f) / SR


def svf_lp(x, cutoff, q=0.7):
    """Time-varying resonant low-pass (block-wise biquad with carried state). cutoff: scalar or per-sample."""
    n = len(x)
    cut = np.broadcast_to(np.asarray(cutoff, dtype=np.float64), (n,))
    out = np.empty(n)
    blk = 64
    z = np.zeros(2)
    for i in range(0, n, blk):
        fc = float(np.clip(cut[i], 30, SR * 0.45))
        w = TAU * fc / SR
        al = np.sin(w) / (2 * q)
        cw = np.cos(w)
        b0 = (1 - cw) / 2
        b = np.array([b0, 1 - cw, b0]) / (1 + al)
        a = np.array([1, -2 * cw / (1 + al), (1 - al) / (1 + al)])
        seg, z = signal.lfilter(b, a, x[i:i + blk], zi=z)
        out[i:i + blk] = seg
    return out


def svf_bp(x, center, q=4.0):
    n = len(x)
    cen = np.broadcast_to(np.asarray(center, dtype=np.float64), (n,))
    out = np.empty(n)
    blk = 64
    z = np.zeros(2)
    for i in range(0, n, blk):
        fc = float(np.clip(cen[i], 40, SR * 0.45))
        w = TAU * fc / SR
        al = np.sin(w) / (2 * q)
        b = np.array([al, 0, -al]) / (1 + al)
        a = np.array([1, -2 * np.cos(w) / (1 + al), (1 - al) / (1 + al)])
        seg, z = signal.lfilter(b, a, x[i:i + blk], zi=z)
        out[i:i + blk] = seg
    return out


def drive(x, amount=2.0):
    """Soft saturation, gain-compensated so peaks stay near the input peak."""
    if amount <= 0:
        return x
    return np.tanh(x * amount) / np.tanh(amount)


def crush(x, bits=8, down=2):
    y = np.round(x * (2 ** (bits - 1))) / (2 ** (bits - 1))
    if down > 1:
        y = np.repeat(y[::down], down)[:len(x)]
    return y


def shelf_air(x, amt=0.3, fc=6000):
    return x + amt * hp(x, fc)


# ------------------------------------------------------------------ drums
def kick(vel=1.0, kind='punch', dur=None):
    """kind: punch (tight funk kick), 808 (long boom), break (rounder, sampled-break-like), boom (cinematic)."""
    spec = {'punch': (0.32, 52, 190, 0.028, 0.11, 0.9),
            '808': (0.9, 44, 150, 0.04, 0.42, 1.6),
            'break': (0.26, 62, 160, 0.02, 0.08, 1.2),
            'boom': (1.2, 38, 140, 0.06, 0.5, 2.0)}[kind]
    d, f_end, f_start, pdec, adec, dr = spec
    n = int((dur or d) * SR)
    t = ts(n)
    f = f_end + (f_start - f_end) * np.exp(-t / pdec)
    y = np.sin(phase_of(f, n)) * np.exp(-t / adec)
    click = hp(_RNG(7).standard_normal(n), 1800) * np.exp(-t / 0.0025) * 0.35
    y = drive(y * 1.2, dr) + click
    return fade(y * vel, 0.0005, 0.02)


def snare(vel=1.0, tone=185, snap=1.0, dur=0.24, seed=0, tight=False):
    n = int(dur * SR)
    t = ts(n)
    rng = _RNG(100 + seed)
    body = (np.sin(phase_of(tone * (1 + 0.4 * np.exp(-t / 0.01)), n)) + 0.5 * np.sin(phase_of(tone * 1.83, n))) * np.exp(-t / 0.05)
    nz = bp(rng.standard_normal(n), 1400, 9000) * np.exp(-t / (0.045 if tight else 0.085)) * snap
    y = drive(body * 0.7 + nz * 0.8, 1.4)
    return fade(y * vel, 0.0005, 0.02)


def clap(vel=1.0, seed=0, dur=0.3, spread=0.011):
    n = int(dur * SR)
    t = ts(n)
    rng = _RNG(200 + seed)
    src = bp(rng.standard_normal(n), 900, 5200)
    e = np.zeros(n)
    for k in range(4):
        t0 = k * spread * (1 + 0.15 * rng.random())
        e += (t >= t0) * np.exp(-np.maximum(t - t0, 0) / (0.006 if k < 3 else 0.09)) * (0.8 if k < 3 else 1.0)
    return fade(src * e * vel * 0.8, 0.0005, 0.02)


_HAT_RATIOS = (2.0, 3.0, 4.16, 5.43, 6.79, 8.21)


def hat(vel=1.0, open_=False, seed=0, tone=1.0):
    dur = 0.42 if open_ else 0.07
    n = int(dur * SR)
    t = ts(n)
    base = 330 * tone
    y = sum(pulse(phase_of(base * r, n)) for r in _HAT_RATIOS)
    y = hp(y, 7200) * 0.5 + hp(_RNG(300 + seed).standard_normal(n), 9000) * 0.35
    y *= np.exp(-t / (0.11 if open_ else 0.016))
    return fade(lp(y, 15000) * vel * 0.55, 0.0003, 0.01)


def ride(vel=1.0, seed=0):
    n = int(0.9 * SR)
    t = ts(n)
    y = sum(pulse(phase_of(470 * r, n)) for r in _HAT_RATIOS) * 0.4
    y = bp(y, 3000, 12000) * np.exp(-t / 0.35) + hp(_RNG(400 + seed).standard_normal(n), 6000) * np.exp(-t / 0.05) * 0.2
    return fade(y * vel * 0.3, 0.0005, 0.05)


def crash(vel=1.0, seed=0, dur=2.0):
    n = int(dur * SR)
    t = ts(n)
    y = hp(_RNG(500 + seed).standard_normal(n), 3500) * np.exp(-t / 0.7)
    y += sum(pulse(phase_of(390 * r, n)) for r in _HAT_RATIOS) * 0.15 * np.exp(-t / 0.4)
    return fade(lp(y, 14000) * vel * 0.4, 0.001, 0.2)


def tom(freq=120, vel=1.0, dur=0.35):
    n = int(dur * SR)
    t = ts(n)
    y = np.sin(phase_of(freq * (1 + 0.6 * np.exp(-t / 0.03)), n)) * np.exp(-t / 0.14)
    y += 0.2 * bp(_RNG(int(freq)).standard_normal(n), 300, 3000) * np.exp(-t / 0.02)
    return fade(drive(y, 1.2) * vel, 0.0005, 0.02)


def rim(vel=1.0, freq=1700):
    n = int(0.06 * SR)
    t = ts(n)
    y = (np.sin(TAU * freq * t) + 0.6 * np.sin(TAU * freq * 2.37 * t)) * np.exp(-t / 0.009)
    y += hp(_RNG(9).standard_normal(n), 3000) * np.exp(-t / 0.002) * 0.4
    return fade(y * vel * 0.6, 0.0003, 0.008)


def cowbell(freq=560, vel=1.0, dur=0.32):
    """808-style cowbell (two detuned square waves through a band-pass). Pitched: usable as a phonk lead."""
    n = int(dur * SR)
    t = ts(n)
    y = pulse(phase_of(freq, n)) + pulse(phase_of(freq * 1.4836, n))
    y = bp(y, freq * 0.9, min(freq * 5, 12000)) * (0.6 * np.exp(-t / 0.012) + 0.4 * np.exp(-t / 0.12))
    return fade(y * vel * 0.6, 0.0005, 0.02)


def conga(freq=220, vel=1.0, slap=False):
    n = int(0.3 * SR)
    t = ts(n)
    y = np.sin(phase_of(freq * (1 + 0.25 * np.exp(-t / 0.01)), n)) * np.exp(-t / (0.06 if slap else 0.12))
    if slap:
        y += bp(_RNG(int(freq)).standard_normal(n), 1500, 6000) * np.exp(-t / 0.008) * 0.6
    return fade(y * vel * 0.8, 0.0005, 0.02)


def shaker(vel=1.0, seed=0):
    n = int(0.08 * SR)
    t = ts(n)
    y = hp(_RNG(600 + seed).standard_normal(n), 6000) * np.minimum(1, t / 0.012) * np.exp(-t / 0.022)
    return fade(lp(y, 13000) * vel * 0.35, 0.002, 0.01)


def tick(vel=1.0, high=True):
    n = int(0.03 * SR)
    t = ts(n)
    f = 3100 if high else 2200
    y = (np.sin(TAU * f * t) + 0.5 * np.sin(TAU * f * 2.7 * t)) * np.exp(-t / 0.004)
    return fade(y * vel * 0.5, 0.0003, 0.005)


def vinyl(n, seed=0, amt=1.0):
    """Lo-fi bed: crackle pops + hiss (seamless if the caller wraps it)."""
    rng = _RNG(700 + seed)
    pops = (rng.random(n) > 0.9993) * rng.standard_normal(n) * 0.5
    pops = hp(np.convolve(pops, np.hanning(24), 'same'), 900)
    hiss = lp(hp(rng.standard_normal(n), 2500), 9000) * 0.02
    return (pops + hiss) * amt


# ------------------------------------------------------------------ bass
def bass(freq, dur, vel=1.0, kind='funk', glide_from=None):
    """kind: funk (round finger/synth hybrid), slap (bright pop), acid (resonant squelch), reese (detuned growl),
    sub (pure-ish 808 sub), wobble (filter LFO), saw (distorted electro saw)."""
    n = int((dur + 0.03) * SR)
    t = ts(n)
    f = np.full(n, freq)
    if glide_from:
        f = freq + (glide_from - freq) * np.exp(-t / 0.035)
    ph = phase_of(f, n)
    if kind == 'sub':
        y = np.sin(ph) + 0.18 * np.sin(2 * ph)
        y = drive(y, 1.4) * env_exp(n, 0.003, 9, 1, r=0.03)
    elif kind == 'slap':
        y = saw(ph) * 0.6 + np.sin(ph) * 0.8
        cut = 400 + 4200 * np.exp(-t / 0.05)
        y = svf_lp(y, cut, 1.2) + 0.25 * hp(_RNG(int(freq)).standard_normal(n), 2500) * np.exp(-t / 0.004)
        y = drive(y, 1.6) * env_exp(n, 0.001, 0.35, 0.25, r=0.03)
    elif kind == 'acid':
        y = saw(ph)
        cut = 260 + 3400 * np.exp(-t / 0.09) * vel
        y = drive(svf_lp(y, cut, 4.5) * 0.7, 2.2) * env_exp(n, 0.002, 9, 1, r=0.02)
    elif kind == 'reese':
        y = saw(ph) + saw(phase_of(f * 1.009, n)) + saw(phase_of(f * 0.993, n)) * 0.8
        y = svf_lp(y * 0.4, 700 + 500 * np.sin(TAU * 0.7 * t) ** 2, 1.1) + np.sin(ph) * 0.6
        y = drive(y, 1.8) * env_exp(n, 0.004, 9, 1, r=0.03)
    elif kind == 'wobble':
        y = saw(ph) + pulse(phase_of(f * 1.003, n), 0.3) * 0.6
        lfo = 0.5 - 0.5 * np.cos(TAU * 4 * t)
        y = drive(svf_lp(y * 0.5, 180 + 2200 * lfo, 3.0), 2.0) + np.sin(ph) * 0.4
        y *= env_exp(n, 0.003, 9, 1, r=0.03)
    elif kind == 'saw':
        y = saw(ph) + saw(phase_of(f * 2.004, n)) * 0.3
        y = drive(svf_lp(y, 900 + 1800 * np.exp(-t / 0.12), 1.4) * 1.3, 3.0) + np.sin(ph) * 0.5
        y *= env_exp(n, 0.002, 9, 1, r=0.025)
    else:  # funk
        y = np.sin(ph) + 0.45 * saw(ph)
        cut = 320 + 1700 * np.exp(-t / 0.07)
        y = drive(svf_lp(y, cut, 0.9), 1.5) * env_exp(n, 0.0015, 0.6, 0.45, r=0.03)
    return fade(y * vel, 0.0008, 0.02)


# ------------------------------------------------------------------ keys / harmony
def rhodes(freq, dur, vel=1.0):
    """FM electric piano with bell tine and amp tremolo."""
    n = int((dur + 0.45) * SR)
    t = ts(n)
    idx = (1.6 + 1.8 * vel) * np.exp(-t / 0.35)
    mod = np.sin(TAU * freq * t) * idx
    y = np.sin(TAU * freq * t + mod)
    tine = np.sin(TAU * freq * 14.0 * t) * np.exp(-t / 0.02) * 0.12 * vel
    amp = env_exp(n, 0.002, 1.4, 0.0, r=0.06) * (0.92 + 0.08 * np.sin(TAU * 4.5 * t))
    rel = np.ones(n)
    k0 = int(dur * SR)
    if k0 < n:
        rel[k0:] = np.exp(-(t[k0:] - t[k0]) / 0.09)
    return fade((y + tine) * amp * rel * vel * 0.7, 0.001, 0.03)


def clav(freq, dur, vel=1.0):
    n = int((dur + 0.06) * SR)
    t = ts(n)
    y = pulse(phase_of(freq, n), 0.22)
    y = svf_lp(y, 1800 + 3500 * np.exp(-t / 0.05), 1.6)
    y = hp(y, 220) * env_exp(n, 0.0008, 0.18, 0.15, hold=0, r=0.03)
    return fade(drive(y, 1.5) * vel * 0.55, 0.0005, 0.02)


def organ(freq, dur, vel=1.0, calliope=False):
    """Drawbar organ; calliope=True gives the breathy, slightly wobbly carnival pipe colour."""
    n = int((dur + 0.08) * SR)
    t = ts(n)
    vib = 1 + (0.006 if calliope else 0.003) * np.sin(TAU * (6.2 if calliope else 6.8) * t)
    ph = phase_of(freq * vib, n)
    bars = ((1, 1.0), (2, 0.55), (3, 0.35), (4, 0.25), (6, 0.12)) if not calliope else ((1, 1.0), (2, 0.4), (3, 0.18))
    y = sum(a * np.sin(ph * h) for h, a in bars)
    if calliope:
        y += 0.035 * bp(_RNG(int(freq)).standard_normal(n), freq * 1.8, min(freq * 4, 9000)) * np.exp(-t / 0.25)
    click = np.sin(TAU * freq * 8 * t) * np.exp(-t / 0.003) * 0.15
    y = (y + click) * env_exp(n, 0.006, 9, 1.0, r=0.05)
    return fade(y * vel * 0.4, 0.002, 0.03)


def pad(freq, dur, vel=1.0, bright=1.0, voices=5, detune=0.008):
    n = int((dur + 0.5) * SR)
    t = ts(n)
    y = np.zeros(n)
    for v in range(voices):
        d = 1 + detune * (v - (voices - 1) / 2) / max(1, (voices - 1) / 2)
        y += saw(phase_of(freq * d, n) + v * 1.3)
    y = svf_lp(y / voices, (900 + 2200 * bright) * (0.75 + 0.25 * np.sin(TAU * 0.25 * t)), 0.8)
    a = 0.12
    e = np.minimum(1, t / a)
    k0 = int(dur * SR)
    if k0 < n:
        e[k0:] *= np.exp(-(t[k0:] - t[k0]) / 0.15)
    return fade(y * e * vel * 0.6, 0.002, 0.05)


def chord_stab(freqs, dur, vel=1.0, kind='brass'):
    """Short punchy stabs. brass = filter-swell saw section; disco = filtered house chord; pluck = bright synth."""
    out = None
    for i, f in enumerate(freqs):
        n = int((dur + 0.12) * SR)
        t = ts(n)
        if kind == 'brass':
            y = saw(phase_of(f, n)) + saw(phase_of(f * 1.006, n)) * 0.8 + saw(phase_of(f * 0.995, n)) * 0.6
            cut = 500 + 3800 * (1 - np.exp(-t / 0.025)) * np.exp(-t / 0.25) + 600
            y = svf_lp(y * 0.4, cut, 1.0) * env_exp(n, 0.012, 0.35, 0.55, r=0.05)
        elif kind == 'disco':
            y = saw(phase_of(f, n)) + pulse(phase_of(f * 1.004, n), 0.4) * 0.6
            y = svf_lp(y * 0.4, 1300 + 2400 * np.exp(-t / 0.08), 2.2) * env_exp(n, 0.002, 0.25, 0.3, r=0.04)
        else:  # pluck
            y = saw(phase_of(f, n)) + saw(phase_of(f * 1.01, n))
            y = svf_lp(y * 0.4, 600 + 6000 * np.exp(-t / 0.04), 1.4) * env_exp(n, 0.001, 0.16, 0.0, r=0.03)
        k0 = int(dur * SR)
        if k0 < n:
            y[k0:] *= np.exp(-(t[k0:] - t[k0]) / 0.04)
        out = y if out is None else out + y * (1 - 0.05 * i)
    return fade(drive(out, 1.3) * vel * 0.6, 0.001, 0.03)


def vibes(freq, dur, vel=1.0):
    n = int(max(dur * 1.5, 0.9) * SR)
    t = ts(n)
    y = np.sin(TAU * freq * t) * np.exp(-t / 1.0) + 0.18 * np.sin(TAU * freq * 4 * t) * np.exp(-t / 0.3)
    y *= 1 - 0.3 * (0.5 + 0.5 * np.sin(TAU * 5.2 * t))
    return fade(y * vel * 0.6, 0.001, 0.05)


def glock(freq, dur, vel=1.0):
    n = int(max(dur, 0.6) * SR)
    t = ts(n)
    y = np.sin(TAU * freq * t) * np.exp(-t / 0.5) + 0.35 * np.sin(TAU * freq * 2.76 * t) * np.exp(-t / 0.15) \
        + 0.15 * np.sin(TAU * freq * 5.4 * t) * np.exp(-t / 0.05)
    return fade(y * vel * 0.5, 0.0005, 0.04)


# ------------------------------------------------------------------ leads
def lead(freq, dur, vel=1.0, kind='square', glide_from=None):
    """kind: square (chippy hero lead), saw (bright synth), super (7-voice supersaw), whistle (sine + air),
    sax (formant-filtered saw, funky horn), guitar (overdriven pluck)."""
    n = int((dur + 0.08) * SR)
    t = ts(n)
    vib = 1 + 0.007 * np.sin(TAU * 5.6 * t) * np.clip((t - 0.12) / 0.2, 0, 1)
    f = np.full(n, float(freq))
    if glide_from:
        f = freq + (glide_from - freq) * np.exp(-t / 0.03)
    f = f * vib
    ph = phase_of(f, n)
    rel_t = 0.06
    if kind == 'square':
        y = pulse(ph, 0.5 - 0.15 * np.exp(-t / 0.15)) * 0.6
        y = svf_lp(y, 4500, 0.8)
    elif kind == 'saw':
        y = saw(ph) * 0.5 + saw(phase_of(f * 1.005, n)) * 0.3
        y = svf_lp(y, 2500 + 2500 * np.exp(-t / 0.2), 1.0)
    elif kind == 'super':
        y = sum(saw(phase_of(f * (1 + 0.011 * k), n) + k) for k in (-3, -2, -1, 0, 1, 2, 3)) / 4
        y = svf_lp(y, 6500, 0.7)
    elif kind == 'whistle':
        y = np.sin(ph) + 0.05 * np.sin(2 * ph) + 0.06 * bp(_RNG(int(freq)).standard_normal(n), freq, min(freq * 3, 12000))
        rel_t = 0.08
    elif kind == 'sax':
        src = saw(ph) + 0.3 * pulse(ph, 0.3)
        y = 0.6 * svf_bp(src, 650, 3) + 0.4 * svf_bp(src, 1200, 4) + 0.18 * svf_bp(src, 2600, 5)
        y = drive(y * 2.0, 1.5) + 0.04 * bp(_RNG(int(freq)).standard_normal(n), 1500, 4500)
        y *= 0.75 + 0.25 * np.minimum(1, t / 0.08)
    elif kind == 'guitar':
        y = saw(ph) + pulse(phase_of(f * 2.002, n), 0.4) * 0.4
        y = drive(svf_lp(y, 2600, 1.2) * 2.5, 3.5) * 0.5
    else:
        raise KeyError(kind)
    e = env_exp(n, 0.004 if kind != 'whistle' else 0.02, 9, 1.0, r=0.01)
    k0 = int(dur * SR)
    if k0 < n:
        e[k0:] *= np.exp(-(t[k0:] - t[k0]) / rel_t)
    return fade(y * e * vel, 0.0015, 0.02)


def pluck(freq, dur=0.6, vel=1.0, bright=0.6):
    """Karplus-Strong plucked string (funk guitar mute / koto-ish)."""
    n = int(max(dur, 0.25) * SR)
    p = max(2, int(SR / freq))
    buf = _RNG(int(freq * 10)).uniform(-1, 1, p)
    buf = lp(buf, 1500 + 9000 * bright) if p > 12 else buf
    y = np.empty(n)
    damp = 0.496 + 0.003 * bright
    for i in range(n):
        v = buf[i % p]
        y[i] = v
        buf[i % p] = damp * (v + buf[(i + 1) % p])
    return fade(y * env_exp(n, 0.0005, dur * 0.5, 0.0, r=0.02) * vel * 0.7, 0.0005, 0.02)


# ------------------------------------------------------------------ voice
VOWELS = {  # (F1, F2, F3) Hz
    'a': (800, 1200, 2600), 'e': (500, 1750, 2600), 'i': (320, 2300, 3000), 'o': (500, 900, 2400),
    'u': (350, 700, 2400), 'y': (420, 1900, 2500), 'h': (600, 1500, 2600),
}


def vox(freq, dur, vel=1.0, vowel='a', to_vowel=None, gender=1.0, breath=0.1, seed=0):
    """Formant-synth vocal chop: 'ah', 'oh', 'yeah' (y->e->a). gender>1 lifts formants (brighter/cuter)."""
    n = int((dur + 0.05) * SR)
    t = ts(n)
    vib = 1 + 0.01 * np.sin(TAU * 5.5 * t) * np.clip((t - 0.1) / 0.15, 0, 1)
    src = saw(phase_of(freq * vib, n)) + breath * _RNG(800 + seed).standard_normal(n)
    v0 = np.array(VOWELS[vowel], dtype=float) * gender
    v1 = np.array(VOWELS[to_vowel or vowel], dtype=float) * gender
    mix = np.clip(t / max(dur * 0.6, 0.05), 0, 1)
    y = np.zeros(n)
    for k, (amp, q) in enumerate(((1.0, 6), (0.55, 9), (0.25, 12))):
        y += amp * svf_bp(src, v0[k] + (v1[k] - v0[k]) * mix, q)
    e = env_exp(n, 0.012, 9, 1.0, r=0.04)
    k0 = int(dur * SR)
    if k0 < n:
        e[k0:] *= np.exp(-(t[k0:] - t[k0]) / 0.04)
    return fade(drive(y * 2.2, 1.3) * e * vel * 0.7, 0.002, 0.02)


def scratch(dur=0.5, seed=0, rate=7.0, src_freq=220):
    """Turntable scratch: a vocal 'ah' read back with an oscillating platter speed."""
    rng = _RNG(900 + seed)
    srcd = 0.6
    source = vox(src_freq, srcd, 1.0, 'a', 'o', seed=seed) + 0.15 * bp(rng.standard_normal(int((srcd + 0.05) * SR)), 300, 4000)
    n = int(dur * SR)
    t = ts(n)
    speed = np.sin(TAU * rate * t + rng.random()) * (1.2 + 0.6 * np.sin(TAU * 1.3 * t))
    pos = np.cumsum(speed) + len(source) * 0.3
    pos = np.clip(pos, 0, len(source) - 2)
    y = np.interp(pos, np.arange(len(source)), source)
    gate = (np.abs(speed) > 0.15).astype(float)
    gate = np.convolve(gate, np.ones(64) / 64, 'same')
    return fade(hp(y * gate, 250) * 0.9, 0.002, 0.02)


# ------------------------------------------------------------------ transitions / fx
def riser(dur=2.0, seed=0, f0=300, f1=6000):
    n = int(dur * SR)
    t = ts(n)
    r = t / dur
    y = svf_bp(_RNG(1000 + seed).standard_normal(n), f0 * (f1 / f0) ** r, 3) * r ** 1.5
    y += saw(phase_of(80 * (8 ** r), n)) * 0.08 * r ** 2
    return fade(y * 0.6, 0.01, 0.01)


def downlifter(dur=1.2, seed=0):
    n = int(dur * SR)
    t = ts(n)
    r = t / dur
    y = svf_bp(_RNG(1100 + seed).standard_normal(n), 6000 * (0.05 ** r), 2.5) * (1 - r) ** 1.2
    return fade(y * 0.6, 0.002, 0.05)


def impact_boom(dur=1.6, vel=1.0, seed=0):
    n = int(dur * SR)
    t = ts(n)
    y = np.sin(phase_of(36 + 90 * np.exp(-t / 0.05), n)) * np.exp(-t / 0.5)
    y += lp(_RNG(1200 + seed).standard_normal(n), 900) * np.exp(-t / 0.25) * 0.4
    y += hp(_RNG(1300 + seed).standard_normal(n), 3000) * np.exp(-t / 0.03) * 0.3
    return fade(drive(y, 1.8) * vel, 0.0005, 0.1)


def gong_hit(freq=90, dur=2.5, vel=1.0):
    n = int(dur * SR)
    t = ts(n)
    y = sum(a * np.sin(TAU * freq * r * t * (1 + 0.003 * np.exp(-t / 0.4))) * np.exp(-t / d)
            for r, a, d in ((1, 1.0, 1.6), (1.51, 0.6, 1.2), (2.09, 0.5, 0.9), (2.76, 0.4, 0.7), (3.7, 0.25, 0.5), (5.1, 0.15, 0.3)))
    y += hp(_RNG(int(freq)).standard_normal(n), 2000) * np.exp(-t / 0.05) * 0.2
    return fade(y * vel * 0.4, 0.001, 0.2)


def tape_stop(x, dur=None):
    """Slow a clip to a halt (DJ brake). Returns same length, silent after the stop."""
    n = len(x)
    m = int((dur or n / SR) * SR)
    m = min(m, n)
    speed = np.linspace(1, 0, m) ** 0.8
    pos = np.cumsum(speed)
    y = np.zeros(n)
    y[:m] = np.interp(np.clip(pos, 0, n - 1), np.arange(n), x)
    return y
