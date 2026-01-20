"""SFX building blocks for the "Neon Carnival Funk" sound effects (sfx_gen.py).

Small, punchy, layered primitives on top of kit.py (the instrument kit shared with the music):
pitched thumps, noise cracks, filtered swooshes with a tonal edge, FM tings, coin chimes, filtered square
UI blips, digital glitches, pitch zips, crackles, sparkles, a tiny room reverb and loop-safe helpers.
Everything is synthesised from oscillators and noise (numpy/scipy only). Build-time tool only.
All functions return mono float64 arrays at synth.SR.
"""
import numpy as np
from scipy import signal

from synth import SR, TAU, mtof, lp, hp, bp, fade
from kit import ts, env_exp, saw, pulse, phase_of, svf_lp, svf_bp, drive, crush

PENT = [0, 2, 4, 7, 9]


def N(d):
    return max(1, int(round(d * SR)))


def nf(m):
    return float(mtof(m))


def pent(root, step):
    return root + PENT[step % 5] + 12 * (step // 5)


def seq(parts, total=None):
    """parts = [(offset_s, clip, gain)] -> mono (length = total or the end of the last part)."""
    end = max(int(o * SR) + len(c) for o, c, g in parts)
    out = np.zeros(int(total * SR) if total else end)
    for o, c, g in parts:
        i = int(o * SR)
        if i >= len(out):
            continue
        e = min(len(out), i + len(c))
        out[i:e] += c[:e - i] * g
    return out


def fit(x, n):
    return x[:n] if len(x) >= n else np.pad(x, (0, n - len(x)))


def env_ar(n, att, peak_shape=1.0, rel_shape=1.6, peak=None):
    """Attack/decay window over n samples: rises to 1 at `peak` (fraction) then falls to 0."""
    r = np.arange(n) / max(n - 1, 1)
    p = peak if peak is not None else min(0.95, att * SR / max(n, 1))
    p = max(p, 1e-3)
    up = np.clip(r / p, 0, 1) ** peak_shape
    dn = np.clip((1 - r) / (1 - p), 0, 1) ** rel_shape
    return np.where(r < p, up, dn)


# ------------------------------------------------------------------ percussive layers
def thump(f_hi=180.0, f_lo=55.0, dur=0.16, pdec=0.02, adec=0.05, drv=1.6, click=0.25, seed=0):
    """Pitched sine drop (kick-like body) with optional click transient."""
    n = N(dur)
    t = ts(n)
    f = f_lo + (f_hi - f_lo) * np.exp(-t / pdec)
    y = np.sin(phase_of(f, n)) * np.exp(-t / adec)
    y = drive(y * 1.2, drv)
    if click:
        y += lp(hp(np.random.default_rng(31 + seed).standard_normal(n), 1800), 9000) * np.exp(-t / 0.0016) * click
    return fade(y, 0.0005, min(0.01, dur * 0.2))


def crack(rng, dur=0.08, lo=1200, hi=7000, dec=0.012, tone=0.0, tone_f=220):
    """Snare/clap-like noise snap; tone adds a short pitched body under it."""
    n = N(dur)
    t = ts(n)
    y = bp(rng.standard_normal(n), lo, hi) * np.exp(-t / dec)
    if tone:
        y += tone * np.sin(phase_of(tone_f * (1 + 0.5 * np.exp(-t / 0.006)), n)) * np.exp(-t / (dec * 2.5))
    return fade(y, 0.0004, min(0.008, dur * 0.2))


def clapish(rng, dur=0.12, spread=0.007, taps=3, lo=900, hi=5200, tail=0.03):
    n = N(dur)
    t = ts(n)
    src = bp(rng.standard_normal(n), lo, hi)
    e = np.zeros(n)
    for k in range(taps + 1):
        t0 = k * spread * (1 + 0.2 * rng.random())
        d = 0.004 if k < taps else tail
        e += (t >= t0) * np.exp(-np.maximum(t - t0, 0) / d)
    return fade(src * e * 0.8, 0.0004, 0.006)


def grains(rng, dur, count, lo, hi, gdur=0.004, decay=None, start=0.0):
    """Crunchy micro-burst cluster (paper crumple, debris, crackle)."""
    n = N(dur)
    y = np.zeros(n)
    g = N(gdur)
    w = np.hanning(g)
    src = bp(rng.standard_normal(n + g), lo, hi)
    for k in range(count):
        o = int((start + rng.random() ** 1.6 * (dur - start - gdur)) * SR)
        amp = (0.4 + 0.6 * rng.random())
        if decay:
            amp *= np.exp(-(o / SR) / decay)
        y[o:o + g] += src[o:o + g] * w[:len(y[o:o + g])] * amp
    return y


def crackle(rng, dur, rate=40.0, lo=1500, hi=7000, amp_dec=None):
    n = N(dur)
    imp = (rng.random(n) < rate / SR) * rng.standard_normal(n)
    if amp_dec:
        imp *= np.exp(-ts(n) / amp_dec)
    y = np.convolve(imp, np.hanning(28), 'same')
    return bp(y, lo, hi)


# ------------------------------------------------------------------ tonal layers
def ting(f, dur=0.35, vel=1.0, ratio=2.76, idx=1.6, dec=0.14, partial2=0.25):
    """FM 'digital ting' / glass bell."""
    n = N(dur)
    t = ts(n)
    I = idx * np.exp(-t / (dec * 0.35))
    y = np.sin(TAU * f * t + I * np.sin(TAU * f * ratio * t)) * np.exp(-t / dec)
    if partial2:
        y += partial2 * np.sin(TAU * f * 2.0 * t) * np.exp(-t / (dec * 0.5))
    return fade(y * vel, 0.0005, min(0.02, dur * 0.2))


def coin_ring(f, dur=0.26, dec=0.11, seed=0, bright=1.0):
    """Crisp metallic coin chime: inharmonic partial set + bright click."""
    n = N(dur)
    t = ts(n)
    y = np.zeros(n)
    for r, a, d in ((1.0, 1.0, 1.0), (1.503, 0.35, 0.55), (2.0, 0.55, 0.8), (2.76, 0.4 * bright, 0.45), (4.07, 0.18 * bright, 0.3)):
        if f * r < 12500:
            y += a * np.sin(TAU * f * r * t + r) * np.exp(-t / (dec * d))
    y += lp(hp(np.random.default_rng(55 + seed).standard_normal(n), 3000), 10000) * np.exp(-t / 0.0015) * 0.5
    return fade(y * 0.6, 0.0004, min(0.02, dur * 0.25))


def fm_pluck(f, dur=0.3, vel=1.0, ratio=2.0, idx=3.0, dec=0.12, idx_dec=0.04):
    n = N(dur)
    t = ts(n)
    I = idx * np.exp(-t / idx_dec)
    y = np.sin(TAU * f * t + I * np.sin(TAU * f * ratio * t)) * np.exp(-t / dec)
    return fade(y * vel, 0.0005, min(0.02, dur * 0.2))


def blip(f, dur=0.05, width=0.5, cut=3200, q=1.0, dec=0.025, glide=1.0, vel=1.0, att=0.0006):
    """Filtered square UI blip; glide = end/start frequency ratio."""
    n = N(dur)
    t = ts(n)
    ff = f * glide ** (t / max(dur, 1e-3))
    y = pulse(phase_of(ff, n), width)
    y = svf_lp(y, cut, q) * np.exp(-t / dec) * np.minimum(1, t / att)
    return fade(y * vel * 0.6, att, min(0.006, dur * 0.2))


def zip_sweep(f0, f1, dur, kind='pulse', cut=None, width=0.35, curve=1.0):
    """Exponential pitch zip (up or down); kind pulse/saw/sine."""
    n = N(dur)
    t = ts(n)
    r = (t / dur) ** curve
    f = f0 * (f1 / f0) ** r
    ph = phase_of(f, n)
    if kind == 'sine':
        y = np.sin(ph)
    elif kind == 'saw':
        y = saw(ph)
    else:
        y = pulse(ph, width)
    if cut is not None:
        y = svf_lp(y, cut, 1.1)
    return y


def sub_drop(f0=90.0, f1=36.0, dur=0.3, dec=0.12):
    n = N(dur)
    t = ts(n)
    f = f1 + (f0 - f1) * np.exp(-t / (dur * 0.35))
    y = np.sin(phase_of(f, n)) * np.exp(-t / dec) * np.minimum(1, t / 0.004)
    return fade(drive(y, 1.2), 0.001, min(0.03, dur * 0.2))


def swoosh(rng, dur, f0, f1, q=2.0, peak=0.3, a_shape=1.5, d_shape=1.6, tone=0.0, tone_ratio=0.25, tone_cut=2500,
           flutter=0.0, flutter_hz=0.0):
    """Filtered-noise swoosh, exponential band sweep f0->f1 with a tonal (saw) edge riding the sweep."""
    n = N(dur)
    t = ts(n)
    r = t / dur
    fc = f0 * (f1 / f0) ** r
    y = svf_bp(rng.standard_normal(n), fc, q)
    if tone:
        y += tone * svf_lp(saw(phase_of(fc * tone_ratio, n)), tone_cut, 1.2) * 0.5
    e = env_ar(n, 0, a_shape, d_shape, peak)
    if flutter:
        e *= 1 - flutter * (0.5 + 0.5 * np.sin(TAU * flutter_hz * t))
    return fade(y * e, 0.0008, min(0.01, dur * 0.1))


def glitch(rng, dur=0.12, f=880.0, slices=4, fall=0.8, bits=5, down=3, noise_amt=0.4):
    """Stuttered digital glitch: a short buzzy grain repeated with falling pitch, bit-crushed."""
    n = N(dur)
    y = np.zeros(n)
    sl = n // slices
    for k in range(slices):
        m = int(sl * (0.55 + 0.35 * rng.random()))
        tt = ts(m)
        ff = f * fall ** k * (1 + 0.05 * rng.standard_normal())
        g = pulse(phase_of(ff, m), 0.25 + 0.2 * rng.random()) + noise_amt * rng.standard_normal(m)
        g *= np.minimum(1, tt / 0.001) * np.minimum(1, (m / SR - tt) / 0.002)
        y[k * sl:k * sl + m] += g * (1 - 0.12 * k)
    y = crush(lp(y, 7000), bits, down)
    return fade(lp(y, 9000), 0.0005, 0.005)


def sparkle(rng, dur, root=88, count=6, spread=None, vel=1.0, dec=0.12, ratio=3.01):
    """Cluster of small high tings on pentatonic notes (reward glitter)."""
    spread = spread or dur * 0.5
    parts = []
    for k in range(count):
        o = spread * (k / max(count - 1, 1)) * (0.85 + 0.3 * rng.random())
        m = pent(root, int(rng.integers(0, 7)))
        parts.append((o, ting(nf(m), min(0.3, dur), 1.0 - 0.07 * k, ratio=ratio, idx=0.8, dec=dec, partial2=0.1), 1.0))
    return fit(seq(parts), N(dur)) * vel


def air(rng, dur, lo=3000, hi=9000, dec=0.08, att=0.002):
    n = N(dur)
    t = ts(n)
    return fade(bp(rng.standard_normal(n), lo, hi) * np.exp(-t / dec) * np.minimum(1, t / att), att, 0.01)


def vinyl_noise(rng, dur, lo=400, hi=4000):
    return bp(rng.standard_normal(N(dur)), lo, hi)


# ------------------------------------------------------------------ space / glue
_IR_CACHE = {}


def room(x, size=0.35, wet=0.18, damp=5000, seed=3, pre=0.008):
    """Tiny mono room/plate (FFT convolution). Adds polish without long tails (tail appended)."""
    key = (size, damp, seed, pre)
    if key not in _IR_CACHE:
        n = N(size)
        t = ts(n)
        ir = np.random.default_rng(900 + seed).standard_normal(n) * np.exp(-t / (size / 5.0))
        ir = lp(ir, damp)
        ir[:N(pre)] = 0
        ir /= np.sqrt((ir ** 2).sum()) + 1e-9
        _IR_CACHE[key] = ir
    ir = _IR_CACHE[key]
    wetsig = signal.fftconvolve(np.pad(x, (0, len(ir))), ir)[:len(x) + len(ir)]
    dry = np.pad(x, (0, len(ir)))
    return dry + wet * wetsig * 2.0


def trim_tail(x, thresh=1e-4):
    a = np.abs(x)
    idx = np.nonzero(a > thresh * a.max())[0]
    return x[:idx[-1] + 1] if len(idx) else x


def master(x, dur=None, hpf=32, lpf=13500, fin=0.0008, fout=0.02, drv=0.0):
    """Final clip glue: trim to dur (with musical fade), DC/sub HPF, air LPF, optional saturation."""
    x = np.asarray(x, dtype=np.float64)
    if dur and len(x) > N(dur):
        x = x[:N(dur)]
        fout = max(fout, min(0.2, dur * 0.25))
    x = hp(x, hpf)
    x = lp(x, lpf)
    if drv:
        pk = np.max(np.abs(x)) + 1e-9
        x = drive(x / pk, drv) * pk
    return fade(x, fin, fout)


# ------------------------------------------------------------------ loop-safe helpers (ambience)
def circ_apply(fn, y):
    """Apply a causal filter to a periodic signal without a seam: process 3 tiled copies and keep the middle."""
    n = len(y)
    z = fn(np.tile(y, 3))
    return z[n:2 * n]


def place_wrap(buf, clip, start_s, gain=1.0):
    n = len(buf)
    i = int(round(start_s * SR)) % n
    idx = (i + np.arange(len(clip))) % n
    np.add.at(buf, idx, clip * gain)


def periodic_noise(rng, n, lo, hi, slope=0.0):
    """Band-limited noise that is exactly periodic over n samples (FFT shaped)."""
    w = np.fft.rfft(rng.standard_normal(n))
    fr = np.fft.rfftfreq(n, 1 / SR)
    m = ((fr > lo) & (fr < hi)).astype(float) * (np.maximum(fr, 1) / 1000.0) ** slope
    y = np.fft.irfft(w * m, n)
    return y / (np.std(y) + 1e-12)


def loop_lfo(n, cycles, phase=0.0):
    """Sine LFO completing an integer number of cycles over n samples (0..1 range)."""
    return 0.5 + 0.5 * np.sin(TAU * cycles * np.arange(n) / n + phase)


def loop_freq(f, seconds):
    return max(1, round(f * seconds)) / seconds


# ------------------------------------------------------------------ loudness shaping
def st_crest(x, win=0.05):
    """Peak (dB) minus the loudest short-term RMS (dB) over `win` seconds."""
    k = N(win)
    e = np.convolve(x ** 2, np.ones(k) / k, 'valid') if len(x) > k else np.array([np.mean(x ** 2)])
    return 20 * np.log10(np.max(np.abs(x)) + 1e-12) - 10 * np.log10(e.max() + 1e-12)


def crest_to(x, target_db=7.0, max_drive=8.0):
    """Saturate just enough that the clip's peak-to-short-term-loudness ratio is at most target_db.
    The builder peak-normalises every clip, so this evens out loudness between sibling variations/materials."""
    pk = np.max(np.abs(x)) + 1e-12
    x = x / pk
    if st_crest(x) <= target_db:
        return x * pk
    lo, hi = 0.05, max_drive
    for _ in range(16):
        mid = 0.5 * (lo + hi)
        if st_crest(drive(x, mid)) > target_db:
            lo = mid
        else:
            hi = mid
    return drive(x, hi) * pk
