"""Arcane Brigade audio DSP kit.

Every sound is generated from code in this repository (numpy/scipy only, no samples,
no third-party recordings), so the output is original work owned by the project.
Build-time tool only; nothing here ships in the Unity player.
"""
import numpy as np
from scipy import signal

SR = 44100
TAU = 2 * np.pi


def mtof(m):
    return 440.0 * 2.0 ** ((np.asarray(m, dtype=np.float64) - 69) / 12.0)


def t_axis(seconds):
    return np.arange(int(round(seconds * SR))) / SR


def _stable(key):
    h = 2166136261
    for ch in repr(key):
        h = ((h ^ ord(ch)) * 16777619) & 0xFFFFFFFF
    return h


# ----------------------------------------------------------------- envelopes
def adsr(n, a=0.005, d=0.1, s=0.0, r=0.05, hold=None):
    """Exponential-ish envelope over n samples. hold = sustain length in seconds."""
    t = np.arange(n) / SR
    env = np.minimum(1, t / max(a, 1e-4))
    if hold is None:
        env = env * (s + (1 - s) * np.exp(-t / max(d, 1e-4)))
    else:
        env = env * np.where(t < a + hold, 1.0, s + (1 - s) * np.exp(-(t - a - hold) / max(d, 1e-4)))
    tail = int(r * SR)
    if tail > 0 and tail < n:
        env[-tail:] *= np.linspace(1, 0, tail) ** 2
    return env


def fade(x, fin=0.003, fout=0.01):
    x = x.copy()
    a, b = int(fin * SR), int(fout * SR)
    if a: x[:a] *= np.linspace(0, 1, a)
    if b: x[-b:] *= np.linspace(1, 0, b)
    return x


# -------------------------------------------------------------------- filters
def lp(x, fc, order=2):
    return signal.sosfilt(signal.butter(order, min(fc, SR * 0.45) / (SR / 2), 'low', output='sos'), x)


def hp(x, fc, order=2):
    return signal.sosfilt(signal.butter(order, fc / (SR / 2), 'high', output='sos'), x)


def bp(x, lo, hi, order=2):
    return signal.sosfilt(signal.butter(order, [lo / (SR / 2), min(hi, SR * 0.45) / (SR / 2)], 'band', output='sos'), x)


def noise(n, rng):
    return rng.standard_normal(n)


def sweep_bp(x, f0, f1, q=2.0):
    """Time-varying band-pass by block processing (whoosh/swish)."""
    n = len(x)
    out = np.zeros(n)
    block = 512
    zi = None
    for i in range(0, n, block):
        f = f0 * (f1 / f0) ** (i / max(n, 1))
        f = float(np.clip(f, 60, SR * 0.45))
        sos = signal.butter(2, [max(f / (1 + 1 / q), 40) / (SR / 2), min(f * (1 + 1 / q), SR * 0.45) / (SR / 2)], 'band', output='sos')
        if zi is None:
            zi = np.zeros((sos.shape[0], 2))
        seg, zi = signal.sosfilt(sos, x[i:i + block], zi=zi)
        out[i:i + block] = seg
    return out


# ------------------------------------------------------------------ instruments
def modal(freq, dur, modes, vel=1.0, strike=0.0, rng=None, tremolo=0.0):
    """Sum of decaying partials: modes = [(ratio, amp, decay_seconds)]."""
    n = int(dur * SR)
    t = np.arange(n) / SR
    y = np.zeros(n)
    for ratio, amp, decay in modes:
        f = freq * ratio
        if f > SR * 0.45:
            continue
        y += amp * np.sin(TAU * f * t) * np.exp(-t / decay)
    if tremolo:
        y *= 1 - 0.28 * (0.5 + 0.5 * np.sin(TAU * tremolo * t))
    if strike and rng is not None:
        k = int(0.012 * SR)
        y[:k] += strike * lp(rng.standard_normal(k), min(freq * 6, 9000)) * np.exp(-np.arange(k) / (0.003 * SR))
    return fade(y * vel, 0.001, 0.02)


def pizz(freq, dur=0.6, vel=1.0, rng=None):
    modes = [(1, 1.0, 0.30), (2, 0.55, 0.20), (3, 0.30, 0.13), (4, 0.18, 0.09), (5, 0.10, 0.06), (6, 0.05, 0.04)]
    return modal(freq, dur, modes, vel, strike=0.25, rng=rng)


def upright(freq, dur=0.7, vel=1.0, rng=None):
    modes = [(1, 1.0, 0.34), (2, 0.45, 0.20), (3, 0.16, 0.12), (4, 0.06, 0.07)]
    y = modal(freq, dur, modes, vel, strike=0.35, rng=rng)
    return lp(y, 1400)


def marimba(freq, dur=0.8, vel=1.0, rng=None):
    modes = [(1, 1.0, 0.45 * (110 / max(freq, 110)) ** 0.3), (3.93, 0.32, 0.10), (9.2, 0.07, 0.04)]
    return modal(freq, dur, modes, vel, strike=0.22, rng=rng)


def vibes(freq, dur=1.6, vel=1.0, rng=None):
    modes = [(1, 1.0, 1.1), (4.0, 0.16, 0.45), (10.0, 0.04, 0.15)]
    return modal(freq, dur, modes, vel, strike=0.08, rng=rng, tremolo=4.6)


def celesta(freq, dur=1.2, vel=1.0, rng=None):
    modes = [(1, 1.0, 0.55), (2.0, 0.35, 0.35), (5.4, 0.20, 0.18), (8.93, 0.08, 0.08)]
    return modal(freq, dur, modes, vel, strike=0.05, rng=rng)


def musicbox(freq, dur=1.0, vel=1.0, rng=None):
    modes = [(1, 1.0, 0.45), (2.76, 0.30, 0.20), (5.40, 0.18, 0.10), (8.93, 0.09, 0.05)]
    return modal(freq * 2, dur, modes, vel, strike=0.12, rng=rng)


def bell(freq, dur=1.8, vel=1.0, rng=None):
    modes = [(0.5, 0.5, 1.3), (1, 1.0, 1.0), (1.19, 0.6, 0.8), (1.56, 0.4, 0.6), (2.0, 0.5, 0.5), (2.74, 0.3, 0.3), (3.0, 0.2, 0.25), (4.07, 0.12, 0.15)]
    return modal(freq, dur, modes, vel, strike=0.15, rng=rng)


def clarinet(freq, dur, vel=1.0, vib=0.0, bright=1.0, rng=None, breath=0.03):
    """Odd-harmonic reed tone with breath noise, soft attack, gentle vibrato."""
    n = int(dur * SR)
    t = np.arange(n) / SR
    f = freq * (1 + vib * 0.006 * np.sin(TAU * 5.2 * t) * np.minimum(1, t / 0.3))
    ph = TAU * np.cumsum(f) / SR
    y = np.zeros(n)
    for h in range(1, 14, 2):
        if freq * h > 7000:
            break
        y += np.sin(ph * h) * (1.0 / h ** (1.15 / bright)) * (1 + 0.1 * (h == 3))
    y += 0.05 * np.sin(ph * 2)
    if rng is not None and breath:
        y += breath * bp(rng.standard_normal(n), 1500, 4000)
    env = adsr(n, a=0.035, d=1e9, s=1.0, r=0.06)
    env *= 0.82 + 0.18 * np.minimum(1, t / 0.15)
    return lp(y * env * vel, 3600 * bright)


def saw_pad(freq, dur, vel=1.0, detune=0.004, voices=3, cutoff=1800, a=0.25, r=0.4, rng=None):
    n = int(dur * SR)
    t = np.arange(n) / SR
    y = np.zeros(n)
    for v in range(voices):
        d = 1 + detune * (v - (voices - 1) / 2)
        ph = TAU * freq * d * t + (v * 1.7)
        for h in range(1, 10):
            if freq * d * h > 6000:
                break
            y += np.sin(ph * h) / h
    y /= voices
    return lp(y * adsr(n, a=a, d=1e9, s=1.0, r=r) * vel, cutoff)


def string_pizz_chord(freqs, dur=0.5, vel=1.0, rng=None):
    out = np.zeros(int(dur * SR))
    for i, f in enumerate(freqs):
        y = pizz(f, dur, vel, rng)
        out[:len(y)] += y * (1 - 0.04 * i)
    return out


# ---------------------------------------------------------------------- drums
def felt_kick(vel=1.0, dur=0.28):
    n = int(dur * SR)
    t = np.arange(n) / SR
    f = 52 + 70 * np.exp(-t / 0.03)
    y = np.sin(TAU * np.cumsum(f) / SR) * np.exp(-t / 0.09)
    y += 0.25 * lp(np.random.default_rng(3).standard_normal(n), 400) * np.exp(-t / 0.01)
    return fade(y * vel, 0.001, 0.02)


def toy_tom(freq=140, vel=1.0, dur=0.3, rng=None):
    n = int(dur * SR)
    t = np.arange(n) / SR
    f = freq * (1 + 0.5 * np.exp(-t / 0.02))
    y = np.sin(TAU * np.cumsum(f) / SR) * np.exp(-t / 0.08)
    y += 0.18 * np.sin(TAU * freq * 2.3 * t) * np.exp(-t / 0.03)
    return fade(y * vel, 0.001, 0.02)


def brush(vel=1.0, dur=0.12, seed=1, bright=3500):
    rng = np.random.default_rng(seed)
    n = int(dur * SR)
    y = bp(rng.standard_normal(n), 1200, bright)
    t = np.arange(n) / SR
    return fade(y * np.minimum(1, t / 0.012) * np.exp(-t / (dur * 0.35)) * vel * 0.6, 0.002, 0.02)


def woodblock(freq=900, vel=1.0, dur=0.1):
    n = int(dur * SR)
    t = np.arange(n) / SR
    y = (np.sin(TAU * freq * t) + 0.5 * np.sin(TAU * freq * 1.9 * t)) * np.exp(-t / 0.018)
    return fade(lp(y, 5000) * vel, 0.0005, 0.01)


def shaker(vel=1.0, dur=0.09, seed=2):
    rng = np.random.default_rng(seed)
    n = int(dur * SR)
    t = np.arange(n) / SR
    y = hp(rng.standard_normal(n), 5500) * np.exp(-t / 0.025) * np.minimum(1, t / 0.008)
    return fade(lp(y, 12000) * vel * 0.35, 0.002, 0.01)


def clock_tick(vel=1.0, high=True):
    n = int(0.05 * SR)
    t = np.arange(n) / SR
    f = 2400 if high else 1700
    y = (np.sin(TAU * f * t) * 0.7 + np.sin(TAU * f * 2.41 * t) * 0.3) * np.exp(-t / 0.006)
    return fade(y * vel * 0.6, 0.0004, 0.01)


def metal_hat(vel=1.0, dur=0.07, seed=5):
    rng = np.random.default_rng(seed)
    n = int(dur * SR)
    t = np.arange(n) / SR
    y = hp(rng.standard_normal(n), 7000) * np.exp(-t / 0.014)
    return fade(lp(y, 13000) * vel * 0.25, 0.001, 0.008)


def gong(freq=110, dur=2.4, vel=1.0):
    modes = [(1, 1.0, 1.6), (1.51, 0.6, 1.3), (2.09, 0.5, 1.0), (2.76, 0.4, 0.8), (3.7, 0.25, 0.5), (5.1, 0.15, 0.3)]
    return modal(freq, dur, modes, vel, strike=0.2, rng=np.random.default_rng(9))


# ----------------------------------------------------------------- mixing tools
def place(buf, clip, start_s, gain=1.0, pan=0.0, wrap=False):
    """Add mono clip into stereo buf (n,2). wrap=True lets tails wrap to the loop head."""
    i = int(round(start_s * SR))
    n = len(buf)
    gl = gain * np.cos((pan + 1) * np.pi / 4)
    gr = gain * np.sin((pan + 1) * np.pi / 4)
    L = len(clip)
    if wrap:
        idx = (i + np.arange(L)) % n
        np.add.at(buf[:, 0], idx, clip * gl)
        np.add.at(buf[:, 1], idx, clip * gr)
    else:
        e = min(n, i + L)
        if e > i:
            buf[i:e, 0] += clip[:e - i] * gl
            buf[i:e, 1] += clip[:e - i] * gr


def make_ir(seconds=1.4, seed=11, damp=3500, early=True):
    rng = np.random.default_rng(seed)
    n = int(seconds * SR)
    t = np.arange(n) / SR
    ir = rng.standard_normal((n, 2)) * np.exp(-t / (seconds / 5.5))[:, None]
    ir = np.stack([lp(ir[:, 0], damp), lp(ir[:, 1], damp)], 1)
    ir[:int(0.012 * SR)] *= np.linspace(0, 1, int(0.012 * SR))[:, None]
    return ir / np.sqrt((ir ** 2).sum())


def circular_reverb(buf, wet=0.18, seconds=1.4, seed=11, damp=3500):
    """FFT circular convolution: tail wraps, so a looping stem has no seam."""
    ir = make_ir(seconds, seed, damp)
    n = len(buf)
    out = np.zeros_like(buf)
    for c in range(2):
        H = np.fft.rfft(np.pad(ir[:, c], (0, max(0, n - len(ir))))[:n])
        out[:, c] = np.fft.irfft(np.fft.rfft(buf[:, c]) * H, n)
    return buf * (1 - wet * 0.5) + out * wet * 2.2


def soft_limit(x, ceiling=0.89):
    return np.tanh(x / ceiling * 0.9) * ceiling / np.tanh(0.9)


def to_peak(x, peak_db=-1.0):
    p = np.max(np.abs(x))
    return x * (10 ** (peak_db / 20) / p) if p > 0 else x


def rms_db(x):
    return 20 * np.log10(np.sqrt(np.mean(np.square(x))) + 1e-9)


def normalize_rms(x, target_db, peak_db=-1.0):
    """Scale to RMS target but never exceed peak ceiling (transient-safe via soft limit first)."""
    cur = rms_db(x)
    y = x * (10 ** ((target_db - cur) / 20))
    p = np.max(np.abs(y))
    ceil = 10 ** (peak_db / 20)
    if p > ceil:
        y = soft_limit(y, ceil)
        y = y * min(1.0, ceil / np.max(np.abs(y)))
    return y


def write_wav(path, x, bits=16):
    import wave
    x = np.asarray(x)
    if x.ndim == 1:
        x = x[:, None]
    pcm = (np.clip(x, -1, 1) * 32767).astype('<i2')
    with wave.open(path, 'wb') as w:
        w.setnchannels(pcm.shape[1]); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes(pcm.tobytes())
