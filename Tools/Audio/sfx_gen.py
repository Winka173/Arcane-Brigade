"""Procedural SFX / voice / ambience generators for Arcane Brigade -- "Neon Carnival Funk" sound design.

Punchy, layered, urban-electronic sound effects (thump body + crack + material colour, filtered swooshes with a
tonal edge, FM tings, crisp coin chimes, square-wave UI blips, brass/rhodes/glock stingers, formant gibberish)
built on kit.py, the same instrument kit the music uses, plus sfx_kit_extra.py primitives. Everything is original
procedural synthesis (numpy/scipy only): no samples, no recordings, nothing taken from any game.

Each generator: fn(i, rng, **params) -> mono float array at 44.1 kHz. `i` is the variation index so variations
differ in pitch/time/timbre without being renames of each other. build_sfx.py peak-normalises every clip.
"""
import numpy as np

import kit as K
from synth import SR, TAU, mtof, lp, hp, bp, fade
from kit import ts, env_exp, saw, pulse, phase_of, svf_lp, svf_bp, drive, crush
from sfx_kit_extra import (N, nf, pent, seq, fit, env_ar, crest_to, st_crest, thump, crack, clapish, grains, crackle, ting, coin_ring,
                           fm_pluck, blip, zip_sweep, sub_drop, swoosh, glitch, sparkle, air, room, master,
                           circ_apply, place_wrap, periodic_noise, loop_lfo, loop_freq, PENT)


# ------------------------------------------------------------------ helpers (kept for compatibility)
def F(x, n):
    """Pad/truncate x to exactly n samples."""
    return fit(x, n)


def note(m):
    return nf(m)


def finish(x, dur=None, fout=0.015, drv=0.0):
    return master(x, dur, fout=fout, drv=drv)


def _env_dec(n, dec, att=0.0005):
    t = ts(n)
    return np.minimum(1, t / att) * np.exp(-t / dec)


def _var(i, spread=0.06, cyc=5):
    """Deterministic pitch factor for variation i: a stride coprime with cyc visits every slot once per cycle."""
    k = next(k for k in (3, 2, 5, 7) if np.gcd(k, cyc) == 1 and k % cyc != 1) if cyc > 2 else 1
    return 1 + spread * (((i * k) % cyc) - (cyc - 1) / 2) / ((cyc - 1) / 2)


# =================================================================== combat
SWING = {  # kind: (f0, f1, dur, q, tone, tone_ratio)
    'blade': (1000, 4000, 0.20, 2.2, 0.45, 0.11),
    'spear': (1500, 5000, 0.16, 3.0, 0.35, 0.09),
    'hammer': (220, 1000, 0.25, 1.3, 0.8, 0.28),
    'scissors': (2100, 5200, 0.12, 3.2, 0.25, 0.07),
    'umbrella': (400, 1600, 0.22, 1.5, 0.3, 0.18),
    'whip': (1500, 5600, 0.15, 3.4, 0.35, 0.09),
}


def swing(i, rng, kind='blade', light=False):
    """Tight filtered-noise swoosh with a saw edge riding the sweep; transient at +0 ms."""
    f0, f1, dur, q, tone, tr = SWING[kind]
    j = _var(i, 0.08)
    dur = dur * (0.62 if light else 1.0) * (1 + 0.05 * (i % 3))
    peak = (0.14 + 0.05 * (i % 2)) if not light else 0.3
    y = swoosh(rng, dur, f0 * j, f1 * j, q, peak=peak, a_shape=1.2, d_shape=1.9, tone=tone * (0.5 if light else 1.0),
               tone_ratio=tr, tone_cut=2800 * j)
    parts = [(0, y, 1.0)]
    n = len(y)
    t = ts(n)
    if not light:
        parts.append((0, crack(rng, 0.02, 1800, 6000, 0.0025), 0.35))
    if kind == 'blade' and not light:
        parts.append((0.012, ting(2700 * j, 0.12, ratio=1.41, idx=0.7, dec=0.035, partial2=0.0), 0.16))
    elif kind == 'spear':
        z = zip_sweep(520 * j, 1500 * j, dur * 0.6, 'pulse', cut=2600) * _env_dec(N(dur * 0.6), dur * 0.2)
        parts.append((0, z, 0.18))
    elif kind == 'hammer':
        body = np.sin(phase_of(85 * j + 70 * (t / dur), n)) * env_ar(n, 0, 1.2, 1.5, 0.3)
        parts.append((0, drive(body * 1.5, 1.6), 0.5))
    elif kind == 'scissors':
        parts.append((0.005, ting(3000 * j, 0.08, ratio=1.53, idx=0.8, dec=0.02, partial2=0.0), 0.2))
    elif kind == 'umbrella':
        flap = svf_lp(rng.standard_normal(n), 900, 1.2) * np.exp(-t / 0.07) * (0.6 + 0.4 * np.sin(TAU * 38 * t) ** 2)
        parts.append((0, flap, 0.55))
    elif kind == 'whip' and not light:
        parts.append((dur * 0.55, clapish(rng, 0.05, 0.004, 2, 1500, 6500, 0.01), 0.45))
    return finish(seq(parts), fout=0.012)


def windup(i, rng, kind='blade'):
    """Charge-up: reverse swoosh + rising filtered pulse, length matches weapon.Windup (0.15-0.24 s)."""
    dur = (0.17, 0.19, 0.21)[i % 3]
    n = N(dur)
    t = ts(n)
    r = t / dur
    y = swoosh(rng, dur, 700 + 80 * i, 3200 + 300 * i, 2.2, peak=0.94, a_shape=2.2, d_shape=1.0)
    tone = pulse(phase_of(170 * (1 + 0.04 * i) * 2.6 ** r, n), 0.3)
    tone = svf_lp(tone, 500 + 2200 * r, 2.5) * r ** 1.4
    tick = np.zeros(n)
    rate = 30 + 60 * r
    clicks = (np.diff(np.floor(np.cumsum(rate) / SR), prepend=0) > 0).astype(float)
    tick = bp(np.convolve(clicks, np.hanning(40), 'same'), 1500, 5000) * r
    return finish(y * 0.8 + tone * 0.35 + tick * 0.25, fout=0.018)


def thrust(i, rng):
    return swing(i, rng, 'spear')


def miss(i, rng):
    """Quiet, airy whiff (lighter and shorter than any swing)."""
    y = swing(i + 1, rng, ('blade', 'spear', 'blade', 'umbrella')[i % 4], light=True)
    return finish(y * 0.6, fout=0.03)


def _clang(f, n, dec=0.16, beat=1.007):
    t = ts(n)
    y = np.zeros(n)
    for r, a, d in ((1.0, 1.0, 1.0), (beat, 0.6, 1.0), (2.32, 0.5, 0.5), (3.71, 0.3, 0.3), (5.12, 0.15, 0.2)):
        y += a * np.sin(TAU * f * r * t + r * 2) * np.exp(-t / (dec * d))
    return y


def impact(i, rng, material='wood', weight=1.0, dur=0.28):
    """Layered hit: low thump body + mid crack + material colour, with a little saturation/grit."""
    j = _var(i, 0.05, 6)
    w = weight
    n = N(dur * (0.6 + 0.4 * w))
    t = ts(n)
    dk = 0.7 + 0.5 * w
    wf = w ** -0.4
    if material == 'paper':
        body = thump(260 * j * wf, 120 * wf, n / SR, 0.012, 0.03 * dk, 1.8, 0.2, i)
        snap = crack(rng, 0.06, 1500, 6500, 0.009)
        col = crush(grains(rng, 0.09, 26, 1800, 7000, 0.003, 0.035), 6, 2)
        y = seq([(0, body, 0.85), (0, snap, 0.6), (0.004, col, 0.9)], n / SR)
    elif material == 'metal':
        body = thump(330 * j * wf, 190 * wf, n / SR, 0.01, 0.045 * dk, 2.0, 0.2, i)
        snap = crush(crack(rng, 0.04, 2200, 7500, 0.006), 7, 2)
        col = _clang(1380 * j, n, 0.13 * dk) * np.minimum(1, t / 0.0008)
        y = seq([(0, body, 0.8), (0, snap, 0.5), (0, col, 0.45)], n / SR)
    elif material == 'wax':
        body = thump(170 * j * wf, 70 * wf, n / SR, 0.018, 0.06 * dk, 1.5, 0.1, i)
        splat = svf_lp(rng.standard_normal(n), 300 + 2400 * np.exp(-t / 0.018), 2.2) * np.exp(-t / 0.05)
        bloop = np.sin(phase_of(130 * j + 220 * np.exp(-t / 0.02), n)) * np.exp(-t / 0.05)
        y = body * 0.9 + splat * 0.8 + bloop * 0.4
    elif material == 'fabric':
        body = thump(210 * j * wf, 75 * wf, n / SR, 0.016, 0.055 * dk, 1.4, 0.12, i)
        punch = lp(rng.standard_normal(n), 1700) * np.exp(-t / 0.028)
        snap = crack(rng, 0.05, 700, 3200, 0.012)
        y = seq([(0, body, 1.0), (0, punch, 0.55), (0, snap, 0.4)], n / SR)
    elif material == 'glass':
        body = thump(520 * j * wf, 300 * wf, n / SR, 0.008, 0.025 * dk, 1.6, 0.15, i)
        tg = ting(2600 * j, n / SR, ratio=1.47, idx=1.2, dec=0.08 * dk, partial2=0.2)
        tg2 = ting(3900 * j * (1 + 0.01 * rng.standard_normal()), n / SR, ratio=1.31, idx=0.8, dec=0.05 * dk, partial2=0.0)
        shards = grains(rng, n / SR, 14, 2800, 7500, 0.002, 0.04)
        snap = crack(rng, 0.03, 2500, 7000, 0.005)
        y = seq([(0, body, 0.8), (0, tg, 0.3), (0.003, tg2, 0.16), (0, shards, 0.55), (0, snap, 0.45)], n / SR)
    else:  # wood
        body = thump(260 * j * wf, 150 * wf, n / SR, 0.01, 0.04 * dk, 1.8, 0.2, i)
        knock = sum(a * np.sin(TAU * 610 * j * r * t) * np.exp(-t / d) for r, a, d in ((1, 1.0, 0.05), (1.58, 0.45, 0.03), (2.72, 0.3, 0.018)))
        snap = crush(crack(rng, 0.04, 1500, 6000, 0.005), 7, 2)
        y = seq([(0, body, 0.75), (0, knock * np.minimum(1, t / 0.0006), 0.7), (0, snap, 0.5)], n / SR)
    y = crest_to(y / (np.max(np.abs(y)) + 1e-9), 7.0)
    return finish(y, fout=0.02)


def crit_sting(i, rng, material='wood'):
    """Critical = reward: harder hit + bright digital ting/glass shimmer + tiny sub drop."""
    base = impact(i + 3, rng, material, 1.25, 0.3)
    root = 86 + PENT[i % 5]
    tg = ting(nf(root), 0.45, ratio=3.0, idx=2.2, dec=0.15, partial2=0.2)
    dig = crush(tg, 7, 2) * 0.25
    sh = seq([(0.022, ting(nf(root + 7), 0.3, ratio=2.0, idx=1.0, dec=0.08), 0.45),
              (0.044, ting(nf(root + 12), 0.3, ratio=2.0, idx=0.8, dec=0.07), 0.35)])
    snap = air(rng, 0.05, 3000, 8000, 0.006)
    parts = [(0, base, 0.95), (0.004, tg + dig, 0.6), (0, sh, 1.0), (0, sub_drop(120, 42, 0.3, 0.1), 0.6), (0, snap, 0.3)]
    return finish(seq(parts), 0.5, 0.1)


MATS_DIE = ['paper', 'wax', 'fabric', 'wood', 'metal', 'glass']
DIE_ZIP = {'paper': 1500, 'wax': 900, 'fabric': 1000, 'wood': 1250, 'metal': 1700, 'glass': 2000}


def enemy_die(i, rng, size='small'):
    """Pop/crunch + quick downward pitch zip; large adds a boom and debris."""
    mat = MATS_DIE[i % 6]
    v = i // 6
    big = size == 'large'
    if big:
        v = i
    jj = 1 + 0.07 * (v - 1)
    crunch = impact(i, rng, mat, 1.5 if big else 1.0, 0.3 if big else 0.22)
    zd = 0.28 if big else 0.13
    zn = N(zd)
    zt = ts(zn)
    z = zip_sweep(DIE_ZIP[mat] * jj * (0.6 if big else 1.0), 70 if big else 150, zd, 'pulse',
                  cut=900 + 4000 * np.exp(-zt / (zd * 0.4)), curve=0.7) * np.exp(-zt / (zd * 0.45))
    pop = thump(420 * jj, 150, 0.08, 0.008, 0.022, 1.6, 0.0, i)
    puff = air(rng, 0.2, 300, 2500, 0.05, 0.004)
    parts = [(0, crunch, 1.0), (0.012, pop, 0.55), (0.02, z, 0.32), (0.015, puff, 0.35)]
    if not big:
        parts.append((0.03, grains(rng, 0.25, 10, 800, 5000, 0.004, 0.06), 0.25))
    if big:
        boom = K.kick(1.0, 'boom', dur=0.6)
        deb = grains(rng, 0.4, 30, 600, 4000, 0.006, 0.12)
        parts += [(0, boom, 0.9), (0.03, deb, 0.45), (0, clapish(rng, 0.15), 0.4)]
    return finish(room(seq(parts), 0.25, 0.1), 0.6 if big else 0.34, 0.08, drv=1.2)


def enemy_hurt(i, rng):
    return finish(impact(i + 7, rng, MATS_DIE[i % 6], 0.8, 0.2) * 0.9, 0.2, 0.03)


# ------------------------------------------------------------------- weapons / signatures
def bow_draw(i, rng):
    dur = 0.2 + 0.012 * i   # matches weapon.Windup (0.15-0.24 s): string draws exactly during the windup pose
    n = N(dur)
    t = ts(n)
    r = t / dur
    rate = 32 + 45 * r
    clicks = (np.diff(np.floor(np.cumsum(rate) / SR), prepend=0) > 0).astype(float)
    creak = bp(np.convolve(clicks, np.hanning(70), 'same') + 0.15 * rng.standard_normal(n), 380, 2000) * (0.4 + 0.6 * r)
    tension = svf_lp(saw(phase_of((130 + 15 * i) * 2.4 ** r, n)), 500 + 1600 * r, 1.8) * r ** 1.2
    rise = swoosh(rng, dur, 700, 2600 + 200 * i, 2.0, peak=0.95, a_shape=2.0, d_shape=1.0)
    return finish(creak * 0.7 + tension * 0.3 + rise * 0.45, fout=0.02)


def bow_release(i, rng):
    """Thwip: string twang + tight arrow swoosh + crisp click."""
    f = nf(45 + (0, 2, 4, 1, 3)[i % 5])
    n = N(0.32)
    t = ts(n)
    tw = saw(phase_of(f * (1 + 0.35 * np.exp(-t / 0.012)), n))
    tw = svf_lp(tw, 400 + 4000 * np.exp(-t / 0.03), 1.6) * np.exp(-t / 0.09)
    thw = np.sin(phase_of(260 + 1100 * np.exp(-t / 0.012), n)) * np.exp(-t / 0.03)
    w = swing(i, rng, 'whip', light=True)
    parts = [(0, tw, 0.6), (0, thw, 0.55), (0.008, w, 0.6), (0, crack(rng, 0.02, 2000, 6500, 0.002), 0.4)]
    return finish(crest_to(seq(parts), 6.5), 0.32, 0.05)


def pop_gun(i, rng, big=False):
    j = _var(i, 0.06, 4)
    if not big:   # "pew-pop"
        body = thump(270 * j, 120, 0.13, 0.01, 0.028, 2.0, 0.2, i)
        pew = zip_sweep(2100 * j, 480 * j, 0.07, 'pulse', cut=3800, width=0.3) * _env_dec(N(0.07), 0.03)
        cork = crack(rng, 0.05, 900, 5000, 0.008)
        puff = air(rng, 0.12, 800, 3000, 0.03)
        y = seq([(0, body, 1.0), (0, pew, 0.3), (0, cork, 0.65), (0.005, puff, 0.3)])
        return finish(crush(y, 9, 1) * 0.3 + y * 0.7, 0.18, 0.03)
    # launcher: "thoomp" + mechanical clunk + leaving whoosh
    body = thump(150 * j, 44, 0.32, 0.03, 0.1, 2.6, 0.3, i)
    n = N(0.32)
    t = ts(n)
    tube = svf_lp(rng.standard_normal(n), 250 + 1400 * np.sin(np.pi * np.clip(t / 0.12, 0, 1)), 2.0) * np.exp(-t / 0.08)
    clunk = seq([(0, K.rim(0.9, 900 * j), 1.0), (0.025, K.tick(0.6, False), 1.0)])
    wh = swoosh(rng, 0.24, 600, 2400, 1.6, peak=0.25)
    zd = zip_sweep(380 * j, 90, 0.2, 'saw', cut=1200) * _env_dec(N(0.2), 0.07)
    return finish(seq([(0, body, 1.0), (0, tube, 0.5), (0, clunk, 0.4), (0.03, wh, 0.35), (0, zd, 0.25)]), 0.32, 0.05, drv=1.3)


def fire_cast(i, rng):
    """Whoomp + crackle + saturated low."""
    dur = 0.45
    n = N(dur)
    t = ts(n)
    pk = 0.11 + 0.015 * i
    cut = 220 + (2400 + 250 * i) * np.sin(np.pi * np.clip(t / (pk * 2.4), 0, 1)) ** 1.5 + 300 * np.exp(-t / 0.2)
    wh = svf_lp(rng.standard_normal(n), cut, 1.5) * env_ar(n, 0, 1.3, 1.4, pk / dur)
    low = np.sin(phase_of(62 * (1 + 0.04 * i) + 40 * np.exp(-t / 0.05), n)) * env_ar(n, 0, 1.0, 1.8, 0.12)
    cr = crackle(rng, dur, 75, 1500, 6000, 0.25)
    ign = crack(rng, 0.04, 2000, 7000, 0.006)
    fw = zip_sweep(180, 640, 0.18, 'saw', cut=1400) * env_ar(N(0.18), 0, 1, 1.5, 0.4)
    y = seq([(0, wh, 1.0), (0, drive(low * 1.6, 2.0), 0.55), (0.02, cr, 0.5), (0, ign, 0.3), (0, fw, 0.18)])
    return finish(drive(y / (np.max(np.abs(y)) + 1e-9), 1.8), dur, 0.08)


def ice_cast(i, rng):
    """Glassy glock arpeggio + crystalline shards + freezing 'shhk'."""
    root = 84 + PENT[i % 5]
    parts = [(0.0, K.glock(nf(root), 0.5, 0.9), 1.0), (0.035, K.glock(nf(root + 7), 0.45, 0.7), 0.8),
             (0.07, K.glock(nf(root + 12), 0.4, 0.6), 0.6)]
    parts.append((0, ting(nf(root + 12) * 1.003, 0.3, ratio=1.41, idx=1.0, dec=0.07, partial2=0.0), 0.25))
    parts.append((0, grains(rng, 0.3, 26, 3000, 8000, 0.0025, 0.1), 0.35))
    parts.append((0, swoosh(rng, 0.22, 2000, 6500, 2.5, peak=0.2), 0.3))
    parts.append((0, thump(220, 110, 0.12, 0.01, 0.03, 1.4, 0.0, i), 0.45))
    return finish(seq(parts), 0.5, 0.1)


def arcane_cast(i, rng):
    """Quick FM pluck arpeggio + shimmer pad + low 'vwomp'."""
    root = 62 + (i % 3) * 2 + (i // 3)
    parts = []
    for k, s in enumerate((0, 7, 10, 15)):
        parts.append((0.038 * k, fm_pluck(nf(root + s), 0.35, 0.9 - 0.12 * k, ratio=2.0, idx=2.8, dec=0.11, idx_dec=0.03), 1.0))
    pad = K.pad(nf(root - 12), 0.32, 0.5, bright=0.8, voices=4, detune=0.012)
    parts.append((0, pad, 0.35))
    n = N(0.3)
    t = ts(n)
    vw = svf_lp(saw(phase_of(nf(root - 24) * (1 + 0.6 * t / 0.3), n)), 300 + 900 * np.exp(-t / 0.1), 2.0) * np.exp(-t / 0.1)
    parts.append((0, vw, 0.4))
    parts.append((0, zip_sweep(500, 2600, 0.07, 'pulse', cut=4500) * _env_dec(N(0.07), 0.03), 0.15))
    parts.append((0.05, sparkle(rng, 0.4, root + 24, 4, 0.2, dec=0.06), 0.18))
    return finish(room(seq(parts), 0.4, 0.15), 0.62, 0.12)


def boomerang_fly(i, rng, loop=False):
    """Whirring spin (AM at the rotation rate) with a doppler rise/fall."""
    dur = 0.6
    n = N(dur)
    t = ts(n)
    r = t / dur
    dop = 1 + 0.18 * np.sin(np.pi * r) - 0.1 * r
    rot = 16 + 2.5 * i
    am = 0.35 + 0.65 * (0.5 + 0.5 * np.sin(TAU * rot * t)) ** 2
    tone = svf_lp(saw(phase_of(170 * (1 + 0.06 * i) * dop, n)), 1300, 1.4)
    ns = svf_bp(rng.standard_normal(n), 900 * dop, 2.0)
    y = (tone * 0.45 + ns * 0.7) * am * np.sin(np.pi * np.clip(r, 0, 1)) ** 0.6
    return finish(y, fout=0.06)


def boomerang_return(i, rng):
    dur = 0.5
    n = N(dur)
    t = ts(n)
    r = t / dur
    rot_ph = TAU * np.cumsum(14 + 18 * r) / SR
    am = 0.3 + 0.7 * (0.5 + 0.5 * np.sin(rot_ph)) ** 2
    tone = svf_lp(saw(phase_of((200 + 15 * i) * 2 ** r, n)), 1500, 1.4)
    ns = svf_bp(rng.standard_normal(n), 600 * 4 ** r, 2.0)
    whirr = (tone * 0.4 + ns * 0.6) * am * np.clip(r / 0.7, 0, 1) ** 1.2 * np.clip((0.4 - t) / 0.04, 0, 1)
    catch = seq([(0, clapish(rng, 0.1, 0.005, 2, 1000, 5000, 0.02), 0.8), (0, K.rim(0.8, 1100 + 60 * i), 0.6),
                 (0, thump(300, 130, 0.09, 0.008, 0.025, 1.6, 0.1, i), 0.8)])
    return finish(seq([(0, whirr, 1.0), (0.38, catch, 1.0)]), 0.55, 0.04)


def pierce(i, rng):
    s = swing(i, rng, 'spear', light=True)
    zi = zip_sweep(1100 * (1 + 0.05 * i), 3000 * (1 + 0.05 * i), 0.07, 'sine') * _env_dec(N(0.07), 0.03)
    tg = ting(nf(81 + 2 * i), 0.2, ratio=2.0, idx=1.4, dec=0.06)
    return finish(seq([(0, s, 0.7), (0.03, crack(rng, 0.03, 1500, 6000, 0.004), 0.5), (0.03, zi, 0.2), (0.04, tg, 0.4)]), 0.28)


def split(i, rng):
    parts = []
    for k in range(3):
        f = nf(76 + (0, 3, 7)[k] + 2 * i)
        parts.append((0.04 * k, blip(f, 0.08, 0.3, 4200, 1.4, 0.03), 0.8))
        parts.append((0.04 * k, crack(rng, 0.02, 1500, 6000, 0.003), 0.35))
    parts.append((0, thump(320, 160, 0.07, 0.006, 0.02, 1.4, 0.0, i), 0.5))
    return finish(seq(parts), 0.25)


def bounce(i, rng):
    """Rubbery electronic 'bwoing'."""
    n = N(0.25)
    t = ts(n)
    f0 = 240 * (1 + 0.09 * i)
    f = f0 * (1 + 0.9 * np.exp(-t / 0.012)) * (1 + 0.06 * np.sin(TAU * 26 * t) * np.exp(-t / 0.08))
    y = svf_lp(pulse(phase_of(f, n), 0.4), 600 + 2800 * np.exp(-t / 0.04), 2.0) * np.exp(-t / 0.07)
    y += np.sin(phase_of(f, n)) * np.exp(-t / 0.06) * 0.6
    return finish(seq([(0, y, 1.0), (0, K.rim(0.6, 1500 + 120 * i), 0.35)]), 0.25)


def bell_ring(i, rng, big=True):
    """Signature bell: FM bell with beating + strike thump + low hum + glitter."""
    f = nf(60 - 12 * (not big) + (0, 2, 4, 7)[i % 4])
    b = ting(f, 1.4, ratio=1.4, idx=2.6, dec=0.55, partial2=0.35)
    b2 = ting(f * 1.004, 1.4, ratio=2.76, idx=1.0, dec=0.4, partial2=0.0)
    n = N(1.4)
    t = ts(n)
    hum = np.sin(TAU * f * 0.5 * t) * np.exp(-t / 0.8) * np.minimum(1, t / 0.01)
    parts = [(0, b, 0.8), (0, b2, 0.4), (0, hum, 0.35), (0, thump(260, 110, 0.15, 0.01, 0.04, 1.6, 0.2, i), 0.6),
             (0, crush(crack(rng, 0.04, 2000, 7000, 0.005), 7, 2), 0.3), (0.03, sparkle(rng, 0.5, 91, 4, 0.18, dec=0.1), 0.12)]
    return finish(room(seq(parts), 0.6, 0.15), 1.4, 0.25)


def umbrella_pop(i, rng):
    n = N(0.3)
    t = ts(n)
    flap = svf_lp(rng.standard_normal(n), 300 + 3000 * np.exp(-t / 0.02), 1.4) * np.exp(-t / 0.06) * (0.55 + 0.45 * np.sin(TAU * 46 * t) ** 2)
    boing = svf_lp(pulse(phase_of(300 * (1 + 0.08 * i) * (1 + 1.3 * (1 - np.exp(-t / 0.03))), n), 0.4), 2200, 1.5) * np.exp(-t / 0.05)
    body = thump(190, 75, 0.2, 0.012, 0.045, 2.0, 0.1, i)
    return finish(seq([(0, flap, 0.9), (0.01, boing, 0.3), (0, body, 0.8)]), 0.3)


def stamp(i, rng):
    """Heavy 'KA-CHUNK' rubber stamp."""
    j = _var(i, 0.06, 3)
    body = thump(150 * j, 50, 0.28, 0.02, 0.07, 2.6, 0.3, i)
    n = N(0.05)
    t = ts(n)
    squeak = np.sin(phase_of(850 * j + 500 * t / 0.05, n)) * np.sin(np.pi * t / 0.05) * 0.5
    parts = [(0, body, 1.0), (0, clapish(rng, 0.12, 0.006, 2, 1000, 5500, 0.025), 0.6), (0.005, K.rim(0.7, 700 * j), 0.4),
             (0.03, squeak, 0.25), (0, grains(rng, 0.06, 10, 1500, 6000, 0.003, 0.02), 0.4)]
    return finish(crush(seq(parts), 9, 1), 0.3, 0.04, drv=1.4)


def snip(i, rng):
    """Two metallic cuts at +0 and +0.12 s (Strike fields)."""
    parts = []
    for k in range(2):
        f = (2900 + 90 * i) * (1.07 if k else 1.0)
        parts.append((0.12 * k, ting(f, 0.1, ratio=1.53, idx=1.0, dec=0.025, partial2=0.0), 0.45))
        parts.append((0.12 * k, ting(f * 1.41, 0.08, ratio=1.2, idx=0.6, dec=0.018, partial2=0.0), 0.2))
        parts.append((0.12 * k, crack(rng, 0.025, 2500, 7000, 0.0025), 0.5))
        parts.append((0.12 * k, swoosh(rng, 0.06, 2000, 4500, 2.5, peak=0.3), 0.3))
        parts.append((0.12 * k, thump(400, 220, 0.04, 0.005, 0.012, 1.2, 0.0, i + k), 0.3))
    return finish(seq(parts), 0.25)


def camera_flash(i, rng):
    n = N(0.2)
    t = ts(n)
    r = t / 0.2
    whine = np.sin(phase_of(700 * (4.4 ** r ** 1.5) * (1 + 0.04 * i), n)) * r ** 1.5 * 0.5
    whine += svf_lp(pulse(phase_of(350 * (4.4 ** r ** 1.5), n), 0.3), 2500, 1.0) * r ** 2 * 0.25
    shut = seq([(0, K.rim(0.9, 2000 + 90 * i), 1.0), (0, thump(600, 260, 0.05, 0.004, 0.012, 1.4, 0.0, i), 0.6),
                (0, crush(air(rng, 0.08, 2000, 7000, 0.02), 6, 3), 0.5)])
    shut2 = seq([(0, K.rim(0.7, 1450 + 60 * i), 1.0), (0, thump(450, 200, 0.04, 0.004, 0.01, 1.2, 0.0, i + 1), 0.5)])
    return finish(seq([(0, whine, 0.5), (0.2, shut, 0.9), (0.235, shut2, 0.7)]), 0.32)


def music_box_note(i, rng):
    """Three rising music-box notes, glock + FM sparkle, mechanism ticks, tiny room."""
    parts = []
    for k, s in enumerate((0, 2, 3)):
        f = nf(72 + pent(0, s + i))
        parts.append((0.07 * k, K.glock(f, 0.6, 0.75 - 0.1 * k), 1.0))
        parts.append((0.07 * k, fm_pluck(f * 2, 0.3, 0.25, ratio=3.5, idx=1.0, dec=0.08), 1.0))
        parts.append((0.07 * k, K.tick(0.25, k % 2 == 0), 1.0))
    parts.append((0, K.rhodes(nf(60 + PENT[i % 5]), 0.3, 0.3), 0.3))
    return finish(room(seq(parts), 0.5, 0.2), 0.8, 0.12)


def tea_splash(i, rng):
    n = N(0.3)
    t = ts(n)
    splash = bp(rng.standard_normal(n), 600, 4200) * env_ar(n, 0, 1, 2.0, 0.06)
    parts = [(0, splash, 0.6), (0, air(rng, 0.25, 2500, 6500, 0.08, 0.01), 0.25)]
    for k in range(3):
        m = N(0.05)
        tt = ts(m)
        f0 = (420 + 90 * k + 40 * i) * (1 + 0.1 * rng.random())
        bub = np.sin(phase_of(f0 * (1 + 1.6 * tt / 0.05), m)) * np.exp(-tt / 0.015)
        parts.append((0.02 + 0.045 * k + 0.01 * rng.random(), bub, 0.5))
    parts.append((0, thump(260, 120, 0.08, 0.008, 0.025, 1.4, 0.0, i), 0.5))
    return finish(seq(parts), 0.3)


def fish_slap(i, rng):
    n = N(0.15)
    t = ts(n)
    squelch = svf_bp(rng.standard_normal(n), 2200 * np.exp(-t / 0.05) + 400, 3.0) * np.exp(-t / 0.05)
    blub = np.sin(phase_of((520 + 40 * i) * (1 - 0.5 * t / 0.15), n)) * np.exp(-t / 0.04)
    parts = [(0, clapish(rng, 0.12, 0.004, 2, 900, 5000, 0.02), 0.9), (0, thump(230, 90, 0.18, 0.012, 0.04, 2.0, 0.0, i), 0.8),
             (0.005, squelch, 0.6), (0.02, blub, 0.35)]
    return finish(seq(parts), 0.3)


def alarm_ring(i, rng):
    """Digital alarm-clock trill: square beeps + bell strikes alternating two pitches."""
    parts = []
    f0 = 1700 + 120 * i
    for k in range(6):
        f = f0 * (1.19 if k % 2 else 1.0)
        parts.append((0.055 * k, blip(f, 0.045, 0.5, 4200, 1.0, 0.02), 0.4))
        parts.append((0.055 * k, ting(f * 1.25, 0.06, ratio=2.76, idx=1.2, dec=0.02, partial2=0.0), 0.35))
    parts.append((0, K.rim(0.8, 900), 0.4))
    return finish(seq(parts), 0.4, 0.05)


def iron_press(i, rng):
    n = N(0.32)
    t = ts(n)
    hiss = bp(rng.standard_normal(n), 1500, 5500) * env_ar(n, 0, 1.0, 1.5, 0.08)
    hiss *= 0.75 + 0.25 * np.sin(TAU * 31 * t)
    parts = [(0, impact(i, rng, 'metal', 1.4, 0.3), 1.0), (0.02, hiss, 0.4), (0.03, crackle(rng, 0.3, 50, 2000, 6000, 0.1), 0.25),
             (0, thump(140, 50, 0.3, 0.02, 0.07, 2.0, 0.0, i), 0.6)]
    return finish(seq(parts), 0.4, 0.06)


def hammer_slam(i, rng):
    parts = [(0, thump(150 * (1 + 0.05 * i), 42, 0.5, 0.025, 0.13, 2.6, 0.35, i), 1.0), (0, impact(i, rng, 'wood', 1.8, 0.35), 0.75),
             (0, clapish(rng, 0.2, 0.008, 3, 900, 5000, 0.05), 0.55), (0.02, grains(rng, 0.35, 28, 400, 3500, 0.006, 0.1), 0.4)]
    return finish(crush(seq(parts), 10, 1), 0.55, 0.1, drv=1.4)


def burn_tick(i, rng):
    n = N(0.12)
    t = ts(n)
    wh = svf_lp(rng.standard_normal(n), 400 + 1400 * np.exp(-t / 0.03), 1.2) * np.exp(-t / 0.035)
    cr = crackle(rng, 0.12, 120, 1800, 6000, 0.05)
    pop = crack(rng, 0.02, 1500, 5000, 0.003)
    return finish(wh * 0.6 + cr * 0.8 + fit(pop, n) * 0.4 * (1 + 0.1 * i), 0.12)


def chill_tick(i, rng):
    f = nf(93 + PENT[i % 5])
    parts = [(0, ting(f, 0.22, ratio=1.41, idx=0.9, dec=0.05, partial2=0.1), 0.7),
             (0, grains(rng, 0.12, 8, 3000, 7500, 0.002, 0.04), 0.35), (0, thump(600, 380, 0.03, 0.004, 0.01, 1.0, 0.0, i), 0.2)]
    return finish(seq(parts), 0.25)


def knockback(i, rng):
    parts = [(0, swoosh(rng, 0.22, 1800, 380, 1.5, peak=0.12, tone=0.3, tone_ratio=0.2, tone_cut=1800), 0.7),
             (0, thump(170 * (1 + 0.05 * i), 58, 0.22, 0.015, 0.06, 2.2, 0.2, i), 0.8), (0, crack(rng, 0.03, 900, 4000, 0.006), 0.3)]
    return finish(seq(parts), 0.22)


def reaction(i, rng, kind='steam'):
    n = N(0.5)
    t = ts(n)
    if kind == 'steam':
        hiss = svf_bp(rng.standard_normal(n), 1500 * 2.6 ** np.clip(t / 0.15, 0, 1), 1.5) * env_ar(n, 0, 1.0, 1.6, 0.12)
        psh = crush(air(rng, 0.15, 2000, 6000, 0.04), 6, 2)
        y = seq([(0, hiss, 0.8), (0, psh, 0.35), (0, ting(nf(84 + 2 * i), 0.3, ratio=1.41, idx=0.8, dec=0.08), 0.3),
                 (0, thump(300, 140, 0.1, 0.01, 0.03, 1.4, 0.0, i), 0.4)], 0.5)
    elif kind == 'shatter':
        y = seq([(0, crack(rng, 0.05, 1800, 7000, 0.008), 0.7), (0, thump(480, 220, 0.12, 0.008, 0.03, 1.6, 0.1, i), 0.6),
                 (0, sparkle(rng, 0.45, 86 + 2 * i, 8, 0.12, dec=0.07, ratio=1.47), 0.6),
                 (0, grains(rng, 0.35, 30, 2500, 7500, 0.003, 0.1), 0.5)], 0.5)
    else:  # arcane flare
        parts = [(0.035 * k, fm_pluck(nf(69 + s + 2 * i), 0.35, 0.9 - 0.1 * k, ratio=2.0, idx=2.5, dec=0.1), 1.0) for k, s in enumerate((0, 3, 7, 12))]
        parts += [(0, zip_sweep(400, 3000, 0.1, 'pulse', cut=4000) * _env_dec(N(0.1), 0.04), 0.2),
                  (0, sub_drop(110, 45, 0.3, 0.1), 0.5), (0.08, sparkle(rng, 0.35, 93, 4, 0.15, dec=0.06), 0.2)]
        y = room(seq(parts), 0.35, 0.15)
    return finish(y, 0.5, 0.08)


# -------------------------------------------------------------- movement/body
def step(i, rng, mat='felt'):
    """Soft sneaker step on stage boards."""
    n = N(0.09)
    t = ts(n)
    body = thump(150 + 9 * i, 85 + 4 * i, 0.09, 0.008, 0.018, 1.2, 0.0, i)
    scuff = lp(hp(rng.standard_normal(n), 300), 1100 + 110 * i) * np.exp(-t / 0.014)
    tick = crack(rng, 0.015, 1200, 3500, 0.002)
    return finish(seq([(0, body, 0.7), (0, scuff, 0.8), (0.003, tick, 0.15)]), 0.09, 0.02)


def dash_whoosh(i, rng):
    j = _var(i, 0.08, 4)
    parts = [(0, swoosh(rng, 0.28, 450 * j, 3300 * j, 1.3, peak=0.3, tone=0.4, tone_ratio=0.15, tone_cut=2000), 0.9),
             (0, svf_lp(rng.standard_normal(N(0.1)), 1000, 1.0) * _env_dec(N(0.1), 0.03), 0.35),
             (0, zip_sweep(90 * j, 160 * j, 0.2, 'sine') * env_ar(N(0.2), 0, 1, 2, 0.25), 0.3)]
    return finish(seq(parts), 0.3)


def jump(i, rng):
    n = N(0.2)
    t = ts(n)
    bw = svf_lp(pulse(phase_of(220 * (1 + 0.07 * i) * 3 ** (t / 0.2), n), 0.35), 1800, 1.4) * np.exp(-t / 0.07)
    parts = [(0, bw, 0.5), (0, swoosh(rng, 0.2, 500, 2500, 1.6, peak=0.2), 0.5), (0, thump(200, 110, 0.06, 0.006, 0.02, 1.2, 0.0, i), 0.4)]
    return finish(seq(parts), 0.22)


def land(i, rng):
    parts = [(0, thump(175 * (1 + 0.06 * i), 58, 0.2, 0.014, 0.05, 2.0, 0.15, i), 1.0),
             (0, lp(rng.standard_normal(N(0.12)), 1100) * _env_dec(N(0.12), 0.03), 0.5), (0, crack(rng, 0.04, 600, 2500, 0.01), 0.3)]
    return finish(seq(parts), 0.2)


def dodge(i, rng):
    """Slick evade: reverse-shaped swoosh + crushed 'phase' shimmer + pitch dip."""
    j = _var(i, 0.07, 3)
    sw = swoosh(rng, 0.26, 600 * j, 4200 * j, 1.8, peak=0.55, a_shape=2.0, d_shape=1.2, tone=0.3, tone_ratio=0.12)
    sh = crush(ting(nf(88 + 2 * i), 0.18, ratio=1.5, idx=1.2, dec=0.05), 6, 3)
    dip = zip_sweep(900 * j, 300 * j, 0.15, 'sine') * _env_dec(N(0.15), 0.05)
    return finish(seq([(0, sw, 0.9), (0.13, sh, 0.25), (0.12, dip, 0.25)]), 0.3)


def hurt(i, rng):
    """Player damage (danger lane): crunchy low hit + short digital glitch -- unlike any enemy sound."""
    j = 1 - 0.05 * (i % 3)
    body = thump(210 * j, 52, 0.26, 0.014, 0.06, 3.5, 0.3, i)
    cr = crush(crack(rng, 0.06, 900, 5000, 0.012, tone=0.5, tone_f=180), 5, 3)
    gl = glitch(rng, 0.13, 620 * j * (1 + 0.06 * (i // 3)), 4, 0.8, 5, 3, 0.35)
    err = blip(nf(57 - i), 0.09, 0.25, 1600, 2.0, 0.05, glide=0.8)
    return finish(seq([(0, body, 1.0), (0, cr, 0.6), (0.03, gl, 0.45), (0.04, err, 0.35)]), 0.35, 0.06, drv=1.3)


def heartbeat(i, rng):
    """Low HP: sub-thump pair (lub-dub) + filtered alarm tone."""
    lub = thump(100, 44, 0.2, 0.012, 0.07, 1.8, 0.0, i)
    dub = thump(95, 42, 0.2, 0.012, 0.06, 1.8, 0.0, i + 1)
    n = N(0.42)
    t = ts(n)
    fa = nf(81 - 5 * i)
    al = svf_lp(pulse(phase_of(fa, n), 0.5), 1500, 2.0) * (0.6 + 0.4 * np.sin(TAU * 14 * t)) * env_ar(n, 0, 1.2, 1.5, 0.2)
    return finish(seq([(0, lub, 1.0), (0.17, dub, 0.75), (0.02, al, 0.22)]), 0.5, 0.08)


def heal(i, rng):
    parts = [(0.055 * k, K.glock(nf(pent(74 + 2 * i, s)), 0.6, 0.65 - 0.06 * k), 1.0) for k, s in enumerate((0, 1, 2, 3, 5))]
    parts.append((0, K.pad(nf(62 + 2 * i), 0.35, 0.5, bright=0.6, voices=4), 0.3))
    parts.append((0, K.pad(nf(69 + 2 * i), 0.35, 0.5, bright=0.6, voices=4), 0.25))
    parts.append((0.05, sparkle(rng, 0.5, 91, 5, 0.3, dec=0.08), 0.2))
    return finish(room(seq(parts), 0.5, 0.2), 0.8, 0.14)


def player_death(i, rng):
    """Glitch + crunchy hit + minor brass chord tape-stopping down + boom."""
    hit = seq([(0, thump(200, 45, 0.4, 0.02, 0.1, 3.0, 0.3, i), 1.0), (0, crush(crack(rng, 0.08, 800, 5000, 0.015), 5, 3), 0.5)])
    ch = K.chord_stab([nf(m + i) for m in (50, 53, 57, 60)], 1.1, 0.8, 'brass')
    ch = K.tape_stop(fit(ch, N(1.25)), 1.15)
    gl = glitch(rng, 0.18, 700 - 60 * i, 5, 0.85, 5, 3, 0.3)
    zd = zip_sweep(500, 40, 1.0, 'saw', cut=1500) * env_ar(N(1.0), 0, 1, 1.5, 0.05)
    boom = K.impact_boom(1.2, 0.9, seed=i)
    return finish(seq([(0, hit, 1.0), (0, gl, 0.4), (0.04, ch, 0.55), (0.04, zd, 0.2), (0.0, boom, 0.6)]), 1.4, 0.2)


# --------------------------------------------------------------------- enemies
def spit_windup(i, rng):
    """Rising gurgle (danger: mid-range so it cuts through)."""
    dur = 0.3
    n = N(dur)
    t = ts(n)
    r = t / dur
    f = (110 + 15 * i) * 3 ** r
    am = 0.5 + 0.5 * np.sin(TAU * np.cumsum(22 + 20 * r) / SR) ** 2
    y = svf_lp(pulse(phase_of(f, n), 0.3), 500 + 2400 * r, 3.0) * am * (0.35 + 0.65 * r ** 0.6)
    y += svf_bp(rng.standard_normal(n), 600 + 1400 * r, 3.0) * am * (0.2 + 0.8 * r) * 0.4
    return finish(drive(y / (np.max(np.abs(y)) + 1e-9), 1.8), fout=0.03)


def spit_fire(i, rng):
    n = N(0.22)
    t = ts(n)
    z = zip_sweep(1500 * (1 + 0.05 * i), 280, 0.22, 'pulse', cut=3000) * np.exp(-t / 0.06)
    parts = [(0, thump(520, 180, 0.12, 0.008, 0.03, 1.8, 0.2, i), 0.9), (0, z, 0.35), (0, crack(rng, 0.05, 900, 4500, 0.01), 0.5),
             (0.01, svf_bp(rng.standard_normal(N(0.08)), 1800, 3.0) * _env_dec(N(0.08), 0.02), 0.4)]
    return finish(seq(parts), 0.22)


def enemy_lunge(i, rng):
    dur = 0.3
    n = N(dur)
    t = ts(n)
    sw = swoosh(rng, dur, 300, 1600 * (1 + 0.05 * i), 1.3, peak=0.55, a_shape=1.5, d_shape=1.2)
    gr = svf_lp(saw(phase_of((85 + 8 * i) * (1 + t / dur), n)) + saw(phase_of((85 + 8 * i) * 1.01 * (1 + t / dur), n)), 900, 1.4)
    gr = drive(gr * 1.5, 2.5) * env_ar(n, 0, 1.5, 1.2, 0.6) * (0.75 + 0.25 * np.sin(TAU * 30 * t))
    return finish(sw * 0.8 + gr * 0.4, fout=0.03)


def big_step(i, rng):
    n = N(0.35)
    t = ts(n)
    body = thump(110 * (1 + 0.06 * i), 38, 0.35, 0.03, 0.11, 2.4, 0.2, i)
    rumble = lp(rng.standard_normal(n), 300) * np.exp(-t / 0.08)
    rattle = grains(rng, 0.25, 14, 1500, 4500, 0.004, 0.08)
    return finish(seq([(0, body, 1.0), (0, rumble, 0.5), (0.01, rattle, 0.2)]), 0.35, 0.05)


def big_roar(i, rng):
    """Formant growl + detuned saw growl + rasp."""
    dur = 0.9
    n = N(dur)
    t = ts(n)
    f0 = 68 + 9 * i
    v = fit(K.vox(f0, 0.8, 1.0, ('a', 'o', 'e')[i % 3], 'o', gender=0.62, breath=0.5, seed=i), n)
    fc = f0 * (1 + 0.25 * np.sin(np.pi * t / dur))
    g = saw(phase_of(fc, n)) + saw(phase_of(fc * 1.012, n)) + saw(phase_of(fc * 0.5, n)) * 0.6
    g = svf_lp(g, 500 + 900 * np.sin(np.pi * t / dur), 1.6) * (0.65 + 0.35 * np.sin(TAU * 7 * t))
    rasp = bp(rng.standard_normal(n), 300, 2500) * (0.5 + 0.5 * np.sin(TAU * 31 * t)) * 0.35
    e = env_ar(n, 0, 0.8, 1.2, 0.25)
    return finish(drive((v * 1.0 + g * 0.45 + rasp) * e, 2.0), fout=0.15)


def boss_enter(i, rng):
    """Riser + heartbeat kicks -> big impact at 1.05 s (gong + brass hit + boom + crash)."""
    hit_t = 1.05
    parts = [(0, K.riser(hit_t, seed=i, f0=250, f1=5000), 0.45)]
    for k, o in enumerate((0.0, 0.35, 0.7, 0.88)):
        parts.append((o, K.kick(0.7 + 0.1 * k, 'punch'), 0.6))
    root = 38 + 2 * i
    parts += [(hit_t, K.impact_boom(1.8, 1.0, seed=i), 1.0), (hit_t, K.gong_hit(nf(root - 7), 1.9, 1.0), 0.8),
              (hit_t, K.chord_stab([nf(root), nf(root + 7), nf(root + 12), nf(root + 15)], 0.7, 1.0, 'brass'), 0.65),
              (hit_t, K.crash(0.8, seed=i, dur=1.8), 0.2), (hit_t, K.kick(1.0, '808'), 0.9),
              (hit_t + 0.05, K.vox(nf(root - 12), 0.5, 0.8, 'a', 'o', gender=0.6, breath=0.4, seed=i), 0.25)]
    return finish(seq(parts), 3.0, 0.4, drv=1.2)


def telegraph(i, rng):
    """Danger: stuttered rising siren cutting through the mix (1-3 kHz)."""
    dur = 0.5
    n = N(dur)
    t = ts(n)
    r = t / dur
    f = (560 + 60 * i) * 2.4 ** r
    gate = (np.sin(TAU * np.cumsum(12 + 14 * r) / SR) > -0.2).astype(float)
    gate = np.convolve(gate, np.ones(80) / 80, 'same')
    s = svf_lp(pulse(phase_of(f, n), 0.4) + 0.5 * saw(phase_of(f * 2.005, n)), 3500, 1.2) * gate
    rise = svf_bp(rng.standard_normal(n), 800 * 5 ** r, 3.0) * r ** 1.5
    y = s * (0.5 + 0.5 * r) + rise * 0.5
    return finish(seq([(0, y, 1.0), (0, crack(rng, 0.03, 1500, 5000, 0.004), 0.3)]), 0.5, 0.05)


def boss_attack(i, rng):
    parts = [(0, thump(130 * (1 + 0.05 * i), 40, 0.6, 0.03, 0.15, 2.8, 0.3, i), 1.0), (0, impact(i, rng, 'metal', 2.0, 0.4), 0.6),
             (0, swing(i, rng, 'hammer'), 0.5), (0, clapish(rng, 0.2, 0.008, 3, 800, 4500, 0.05), 0.4)]
    return finish(seq(parts), 0.6, 0.12, drv=1.3)


def _funk_bar(root, bpm=118, seed=0, bars=1.0):
    """Short drum + slap bass + brass groove, used by phase change / defeat stings."""
    s16 = 60 / bpm / 4
    parts = []
    for k in range(int(16 * bars)):
        o = k * s16
        if k % 8 in (0, 3) or k % 16 == 10:
            parts.append((o, K.kick(0.9, 'punch'), 0.7))
        if k % 8 == 4:
            parts.append((o, K.snare(0.9, seed=seed + k), 0.55))
        parts.append((o, K.hat(0.5 if k % 2 else 0.8, seed=seed + k), 0.25))
    for k, (o, m) in enumerate(((0, 0), (3, 0), (4, 12), (6, 10), (8, 7), (10, 0), (11, 3), (14, 5))):
        if o < 16 * bars:
            parts.append((o * s16, K.bass(nf(root + m), s16 * 1.2, 0.9, 'slap'), 0.6))
    return seq(parts)


def phase_change(i, rng):
    """Groove + chord tape-stops into a big impact (boss <=50% HP)."""
    root = 38 + 3 * i
    groove = _funk_bar(root, 124, seed=i)
    ch = K.chord_stab([nf(root + 24), nf(root + 27), nf(root + 31), nf(root + 34)], 0.8, 0.8, 'brass')
    pre = fit(seq([(0, groove, 1.0), (0, ch, 0.5)]), N(0.95))
    pre = K.tape_stop(pre, 0.95)
    hit_t = 0.95
    parts = [(0, pre, 1.0), (hit_t, K.impact_boom(1.3, 1.0, seed=i + 5), 1.0), (hit_t, K.gong_hit(nf(root - 5), 1.25, 1.0), 0.7),
             (hit_t, K.crash(0.8, seed=i + 3, dur=1.25), 0.35),
             (hit_t, K.chord_stab([nf(root + 12), nf(root + 13), nf(root + 19), nf(root + 24)], 0.5, 1.0, 'brass'), 0.5),
             (hit_t - 0.3, K.riser(0.3, seed=i, f0=600, f1=6000), 0.3)]
    return finish(seq(parts), 2.2, 0.35, drv=1.2)


def boss_defeat(i, rng):
    """Big boom + crash + brass victory stabs + glock sparkle cascade."""
    root = 50 + 2 * i
    maj = [nf(root + m) for m in (0, 4, 7, 12)]
    parts = [(0, K.impact_boom(1.8, 1.0, seed=i + 9), 1.0), (0, K.kick(1.0, 'boom'), 0.7), (0, K.crash(0.9, seed=i, dur=2.6), 0.22),
             (0.32, K.chord_stab(maj, 0.12, 1.0, 'brass'), 0.6), (0.5, K.chord_stab(maj, 0.14, 1.0, 'brass'), 0.6),
             (0.72, K.chord_stab([f * 2 ** (5 / 12) for f in maj], 0.12, 1.0, 'brass'), 0.6),
             (0.95, K.chord_stab([f * 2 for f in maj[:3]] + [maj[0] * 2 ** (11 / 12)], 1.3, 1.0, 'brass'), 0.7),
             (0.95, K.bass(nf(root - 12), 1.0, 1.0, 'sub'), 0.6), (0.95, K.crash(0.7, seed=i + 1, dur=2.0), 0.18)]
    for k in range(8):
        parts.append((1.0 + 0.06 * k, K.glock(nf(pent(root + 24, 8 - k)), 0.5, 0.5), 0.5))
    return finish(room(seq(parts), 0.8, 0.12), 3.0, 0.5)


# ------------------------------------------------------------------------ loot
def coin(i, rng, mode='pickup'):
    """Bright, crisp metallic pentatonic chimes. pickup variations rise in pitch."""
    if mode == 'pickup':
        f = nf(pent(81, i))
        parts = [(0, coin_ring(f, 0.26, 0.1, i), 0.8), (0.045, coin_ring(f * 1.4983, 0.22, 0.09, i + 9), 0.75),
                 (0, K.tick(0.4, True), 0.3), (0, thump(900, 500, 0.02, 0.003, 0.006, 1.0, 0.0, i), 0.15)]
        return finish(seq(parts), 0.3, 0.07)
    if mode == 'drop':
        f = nf(pent(79, (0, 2, 4, 1)[i % 4]))
        parts = [(0, coin_ring(f, 0.25, 0.08, i + 20), 0.8), (0, crack(rng, 0.02, 2500, 7000, 0.0025), 0.4),
                 (0, thump(420, 220, 0.04, 0.004, 0.01, 1.2, 0.0, i), 0.3)]
        return finish(seq(parts), 0.25, 0.05)
    if mode == 'bounce':
        f = nf(pent(84, (0, 2, 1, 3, 4)[i % 5])) * (1 + 0.004 * i)
        y = coin_ring(f, 0.13, 0.035, i + 40, bright=0.7)
        return finish(seq([(0, y, 0.8), (0, K.tick(0.35, i % 2 == 0), 0.3)]), 0.13, 0.03)
    # vacuum: rising zip + crisp ticks
    n = N(0.14)
    t = ts(n)
    z = np.sin(phase_of((900 + 120 * i) * 2.8 ** (t / 0.14), n)) * (0.4 + 0.6 * t / 0.14) * 0.6
    tk = seq([(0.03 * k, coin_ring(nf(pent(88, k + i)), 0.04, 0.012, k + i), 0.35) for k in range(4)])
    return finish(seq([(0, z, 0.6), (0, tk, 1.0)]), 0.14, 0.03)


def egg_pickup(i, rng):
    o = (0, 2, 4, 5)[i % 4]
    parts = [(0, thump(520, 210, 0.06, 0.006, 0.02, 1.2, 0.0, i), 0.7),
             (0.03, fm_pluck(nf(72 + o), 0.25, 0.8, ratio=2.0, idx=2.0, dec=0.08), 0.8),
             (0.09, fm_pluck(nf(79 + o), 0.25, 0.8, ratio=2.0, idx=2.0, dec=0.09), 0.8),
             (0.09, sparkle(rng, 0.2, 91 + o, 3, 0.1, dec=0.05), 0.2)]
    return finish(seq(parts), 0.35, 0.06)


def heart_pickup(i, rng):
    parts = [(0, K.rhodes(nf(67), 0.2, 0.8), 0.6), (0.11, K.rhodes(nf(74 + 2 * (i % 2) + (i // 2)), 0.35, 0.85), 0.6),
             (0, thump(95, 45, 0.18, 0.012, 0.06, 1.6, 0.0, i), 0.6), (0.15, thump(90, 44, 0.16, 0.012, 0.05, 1.6, 0.0, i + 1), 0.45),
             (0.12, sparkle(rng, 0.45, 86, 4, 0.2, dec=0.08), 0.18)]
    return finish(room(seq(parts), 0.4, 0.15), 0.8, 0.15)


def chest_appear(i, rng):
    o = 2 * i
    parts = [(0, swoosh(rng, 0.2, 3000, 700, 1.5, peak=0.7, tone=0.2), 0.45), (0.18, thump(160, 55, 0.3, 0.015, 0.07, 2.2, 0.2, i), 0.9),
             (0.18, clapish(rng, 0.12), 0.5), (0.2, K.glock(nf(84 + o), 0.6, 0.7), 0.6), (0.24, K.glock(nf(91 + o), 0.6, 0.6), 0.45),
             (0.2, sparkle(rng, 0.5, 91, 5, 0.25, dec=0.08), 0.2)]
    return finish(room(seq(parts), 0.4, 0.12), 0.8, 0.12)


def chest_open(i, rng):
    n = N(0.32)
    t = ts(n)
    creak = svf_bp(rng.standard_normal(n), 500 + 300 * t / 0.32, 6.0) * (np.sin(TAU * 11 * t) ** 2) * (t < 0.3)
    latch = seq([(0, K.rim(0.9, 800 + 60 * i), 1.0), (0, thump(260, 110, 0.12, 0.01, 0.03, 1.8, 0.1, i), 0.8)])
    root = 64 + 2 * i
    parts = [(0, creak, 0.6), (0.28, latch, 0.8),
             (0.33, K.chord_stab([nf(root + m) for m in (0, 4, 7, 11)], 0.35, 0.9, 'pluck'), 0.5),
             (0.33, K.lead(nf(root + 12), 0.3, 0.6, 'super'), 0.18),
             (0.33, sub_drop(100, 45, 0.3, 0.12), 0.5), (0.33, K.crash(0.5, seed=i, dur=0.8), 0.15)]
    for k, s in enumerate((0, 2, 4, 7, 9)):
        parts.append((0.36 + 0.05 * k, K.glock(nf(pent(root + 12, s)), 0.6, 0.6), 0.45))
    parts.append((0.36, sparkle(rng, 0.6, 93, 6, 0.4, dec=0.08), 0.2))
    return finish(room(seq(parts), 0.5, 0.15), 1.2, 0.2)


# ------------------------------------------------------------------ upgrades
def merge(i, rng):
    """MergeReveal flash (age 0.54 s): the impact only. The converging build-up is up.merge.gather, the star arpeggio is up.star2/3."""
    root = 67 + 2 * i
    parts = [(0, K.kick(1.0, 'punch'), 0.8), (0, clapish(rng, 0.2, 0.008, 3, 900, 5000, 0.06), 0.45),
             (0, K.chord_stab([nf(root + m) for m in (0, 4, 7, 12)], 0.18, 0.9, 'pluck'), 0.5),
             (0, ting(nf(root + 24), 0.5, ratio=3.0, idx=1.6, dec=0.14), 0.3),
             (0.02, sparkle(rng, 0.5, 91, 6, 0.3, dec=0.07), 0.2), (0, sub_drop(110, 42, 0.35, 0.12), 0.5)]
    return finish(room(seq(parts), 0.4, 0.12), 0.8, 0.15)


def merge_gather(i, rng):
    """MergeReveal 0..0.54 s: materials spiral together (gather 0.32 s), result appears at 0.36 s, flash at 0.54 s."""
    dur = 0.54
    n = N(dur)
    t = ts(n)
    r = t / dur
    sw = svf_bp(rng.standard_normal(n), 300 * (16 + i) ** r, 2.2) * r ** 1.5
    tone = svf_lp(saw(phase_of((110 + 10 * i) * 4 ** (r ** 1.3), n)), 400 + 3000 * r, 1.6) * r ** 1.2
    parts = [(0, sw, 0.6), (0, tone, 0.3)]
    tk = 0.0
    gap = 0.11
    k = 0
    while tk < 0.5 and k < 9:
        parts.append((tk, blip(nf(84 + 2 * k), 0.03, 0.5, 4000, 1.0, 0.01), 0.18))
        tk += gap
        gap *= 0.78
        k += 1
    parts.append((0.36, fm_pluck(nf(72 + 2 * i), 0.18, 0.7, ratio=2.0, idx=2.0, dec=0.06), 0.4))
    return finish(seq(parts, 0.55), 0.55, 0.03)


def star_up(i, rng, stars=2):
    """Star-up stinger: glock+supersaw arpeggio -> chord stab (+ brass/sub/crash on 3 stars)."""
    big = stars >= 3
    root = 72 + 2 * i
    steps = 2 + stars * 2
    sp = 0.06 if big else 0.07
    parts = []
    for k in range(steps):
        m = pent(root, k)
        parts.append((sp * k, K.glock(nf(m), 0.6, 0.6), 0.5))
        parts.append((sp * k, K.lead(nf(m - 12), 0.06, 0.5, 'super'), 0.16))
    t_end = sp * steps
    top = [nf(root + m) for m in (0, 4, 7, 11, 14)]
    parts.append((t_end, K.chord_stab(top[:4], 0.5 if big else 0.3, 0.9, 'brass' if big else 'pluck'), 0.5))
    parts.append((t_end, ting(nf(root + 24), 0.9, ratio=3.0, idx=1.4, dec=0.3), 0.25))
    parts.append((t_end, sparkle(rng, 0.8, 93, 8 if big else 5, 0.5, dec=0.1), 0.2))
    if big:
        parts += [(t_end, K.crash(0.6, seed=i, dur=1.2), 0.25), (t_end, sub_drop(100, 40, 0.5, 0.18), 0.6),
                  (t_end, K.bass(nf(root - 24), 0.5, 0.9, 'slap'), 0.4)]
    return finish(room(seq(parts), 0.6, 0.15), 1.8, 0.35)


def ally_land(i, rng):
    """UnitVisual.BeginArrival: the arc lasts 0.45*0.9 = 0.405 s, touchdown at 0.405 s. Call at BeginArrival."""
    o = (0, 2, 4, 5)[i % 4]
    arc = swoosh(rng, 0.4, 2600, 700, 1.2, peak=0.55, a_shape=1.3, d_shape=0.8, tone=0.25, tone_ratio=0.2)
    parts = [(0, arc, 0.35), (0.405, thump(170, 55, 0.3, 0.014, 0.06, 2.2, 0.2, i), 1.0), (0.405, clapish(rng, 0.15), 0.4),
             (0.41, K.bass(nf(38 + o), 0.3, 0.9, 'slap'), 0.5), (0.41, K.rhodes(nf(62 + o), 0.25, 0.6), 0.25)]
    return finish(seq(parts), 1.0, 0.12)


def bond_tier(i, rng, tier=1):
    root = 60 + 2 * tier + (5 if i else 0)
    ivs = [0, 4, 7, 11] + ([14] if tier > 1 else [])
    parts = []
    for k, s in enumerate(ivs):
        parts.append((0.035 * k, K.rhodes(nf(root + s), 0.6, 0.7), 0.35))
    parts.append((0, K.bass(nf(root - 24), 0.35, 0.9, 'slap'), 0.5))
    parts.append((0, K.snare(0.6, seed=i, tight=True), 0.15))
    parts.append((0.14, K.glock(nf(root + 24), 0.7, 0.6), 0.35))
    if tier > 1:
        parts += [(0.0, K.chord_stab([nf(root + 12 + m) for m in (0, 4, 7)], 0.14, 0.9, 'brass'), 0.35),
                  (0.18, K.glock(nf(root + 31), 0.7, 0.5), 0.3), (0.1, sparkle(rng, 0.6, 93, 5, 0.3), 0.15)]
    return finish(room(seq(parts), 0.6, 0.18), 1.4, 0.25)


def chain_trigger(i, rng):
    parts = [(0.04 * k * (1 - 0.1 * k), blip(nf(76 + 3 * k + i), 0.05, 0.3, 4000, 1.2, 0.02), 0.5) for k in range(4)]
    parts += [(0.04 * k, K.rim(0.5, 1200 + 200 * k), 0.2) for k in range(4)]
    parts.append((0.17, zip_sweep(800, 2600, 0.05, 'pulse', cut=4000) * _env_dec(N(0.05), 0.02), 0.15))
    parts.append((0.2, ting(nf(88 + PENT[i % 5]), 0.6, ratio=3.0, idx=1.8, dec=0.15), 0.55))
    parts.append((0.2, fm_pluck(nf(76 + PENT[i % 5]), 0.4, 0.6, ratio=2.0, idx=2.0, dec=0.12), 0.35))
    return finish(room(seq(parts), 0.4, 0.12), 0.8, 0.14)


def rarity(i, rng, tier='blue'):
    """Rarity stinger: blue < purple < gold (gold = riser -> brass stab + sparkle + sub hit)."""
    tr = 2 * i
    if tier == 'blue':
        parts = [(0, K.glock(nf(79 + tr), 0.6, 0.7), 0.6), (0.08, K.glock(nf(84 + tr), 0.8, 0.7), 0.6),
                 (0, K.vibes(nf(67 + tr), 0.6, 0.6), 0.4), (0.08, K.vibes(nf(72 + tr), 0.8, 0.6), 0.4),
                 (0.08, K.chord_stab([nf(60 + tr), nf(67 + tr), nf(72 + tr)], 0.12, 0.6, 'pluck'), 0.25)]
        dur = 1.0
    elif tier == 'purple':
        parts = [(0.1 * k, K.rhodes(nf(m + tr), 0.4, 0.75), 0.5) for k, m in enumerate((63, 70, 75))]
        parts += [(0.2, K.lead(nf(82 + tr), 0.5, 0.5, 'super', glide_from=nf(75 + tr)), 0.18),
                  (0, K.riser(0.25, seed=i, f0=1000, f1=6000), 0.15), (0.2, K.glock(nf(87 + tr), 0.8, 0.6), 0.4),
                  (0.22, sparkle(rng, 0.7, 87, 5, 0.4, dec=0.1), 0.18), (0.2, sub_drop(90, 45, 0.3, 0.1), 0.35)]
        dur = 1.4
    else:  # gold
        hit = 0.28
        maj = [nf(m + tr) for m in (60, 64, 67, 72, 74)]
        parts = [(0, K.riser(hit, seed=i + 3, f0=600, f1=7000), 0.4),
                 (hit, K.chord_stab(maj, 0.4, 1.0, 'brass'), 0.6), (hit, K.kick(1.0, 'punch'), 0.6),
                 (hit, sub_drop(110, 38, 0.6, 0.2), 0.7), (hit, K.crash(0.7, seed=i, dur=1.6), 0.25),
                 (hit, K.lead(nf(84 + tr), 0.45, 0.6, 'super'), 0.15)]
        for k in range(6):
            parts.append((hit + 0.05 + 0.055 * k, K.glock(nf(pent(84 + tr, k)), 0.6, 0.6), 0.4))
        parts.append((hit + 0.05, sparkle(rng, 1.0, 93, 10, 0.6, dec=0.1), 0.25))
        dur = 2.0
    return finish(room(seq(parts), 0.6, 0.15), dur, 0.3)


# ----------------------------------------------------------------------- shop
def keeper_enter(i, rng, special=False):
    """WorldShop.Build: merchant arrives after MerchantArrivalDelay 0.18 s over 0.8 s => landing at 0.98 s. Call at Build (t=0).
    Gold giant drops from above (falling whistle); the dream merchant rises out of the floor (swelling riser)."""
    land_t = 0.98
    n = N(land_t)
    t = ts(n)
    span = np.clip((t - 0.18) / 0.8, 0, 1)
    if not special:
        f = 1500 * (1 + 0.05 * i) * (0.18 ** span)
        whistle = (np.sin(phase_of(f, n)) + 0.15 * np.sin(2 * phase_of(f, n))) * (span > 0) * (0.3 + 0.7 * span)
        down = svf_bp(rng.standard_normal(n), 3000 * (0.12 ** span), 2.0) * span
        pre = whistle * 0.35 + down * 0.4
        root = 36 + i
        parts = [(0, pre, 1.0), (land_t, K.kick(1.0, 'boom', dur=1.0), 1.0), (land_t, clapish(rng, 0.25, 0.01, 3, 700, 4500, 0.08), 0.5),
                 (land_t, K.chord_stab([nf(root + 12 + m) for m in (0, 4, 7, 10)], 0.3, 1.0, 'brass'), 0.55),
                 (land_t, K.bass(nf(root), 0.5, 1.0, 'slap'), 0.6),
                 (land_t + 0.12, coin_ring(nf(91), 0.6, 0.25, i), 0.4), (land_t + 0.17, coin_ring(nf(96), 0.6, 0.25, i + 1), 0.35),
                 (land_t + 0.1, sparkle(rng, 0.6, 91, 6, 0.3, dec=0.08), 0.15)]
    else:
        rise = K.riser(land_t - 0.18, seed=i + 7, f0=200, f1=3500)
        gl = np.sin(phase_of(nf(58) * 2 ** (span * 1.0), n)) * span ** 1.5 * (span > 0)
        root = 56 + i
        parts = [(0.18, rise, 0.5), (0, gl, 0.12), (land_t, K.impact_boom(1.2, 0.7, seed=i), 0.6),
                 (land_t, K.vibes(nf(root + 12), 1.2, 0.8), 0.4), (land_t + 0.06, K.vibes(nf(root + 16), 1.2, 0.7), 0.35),
                 (land_t + 0.12, K.vibes(nf(root + 19), 1.2, 0.7), 0.35),
                 (land_t, K.vox(nf(root + 12), 0.6, 0.8, 'a', 'o', gender=1.3, breath=0.35, seed=i), 0.25),
                 (land_t, K.pad(nf(root), 0.8, 0.6, bright=0.5), 0.3), (land_t + 0.05, sparkle(rng, 0.9, 91, 8, 0.6, dec=0.12), 0.15)]
    return finish(room(seq(parts), 0.6, 0.15), 2.6, 0.35)


def keeper_exit(i, rng, special=False):
    dur = 0.8
    n = N(dur)
    t = ts(n)
    r = t / dur
    if not special:
        sw = swoosh(rng, dur, 1800, 250, 1.3, peak=0.3, tone=0.3, tone_ratio=0.15)
        wah = svf_lp(saw(phase_of(nf(55 + i) * 0.5 ** r, n)), 300 + 1500 * np.sin(np.pi * np.clip(r * 1.4, 0, 1)), 2.5)
        wah *= env_ar(n, 0, 1, 1.5, 0.1)
        y = sw * 0.7 + wah * 0.35
    else:
        sh = svf_bp(rng.standard_normal(n), 5000 * 0.12 ** r, 3.0) * env_ar(n, 0, 1.5, 1.0, 0.3)
        pd = svf_lp(saw(phase_of(nf(70 + i) * 0.5 ** r, n)) + saw(phase_of(nf(75 + i) * 0.5 ** r * 1.003, n)), 2500 * 0.3 ** r, 1.0)
        pd *= env_ar(n, 0, 1, 1.2, 0.2)
        y = sh * 0.5 + pd * 0.25 + fit(K.vox(nf(67 + i), 0.5, 0.6, 'o', 'u', gender=1.3, breath=0.4, seed=i), n) * 0.2
    return finish(y, fout=0.2)


def carpet(i, rng, mode='open'):
    """MarketLifecycle: opening progress 0.9 s from Build; departure starts CarpetDepartureDelay 0.55 s after Depart and lasts 0.75 s."""
    if mode == 'open':
        n = N(0.9)
        t = ts(n)
        cloth = svf_lp(rng.standard_normal(n), 700 + 500 * t / 0.9, 1.2) * (np.sin(TAU * (6.5 + 0.5 * i) * t) ** 2 * 0.6 + 0.4)
        cloth *= np.sin(np.pi * np.clip(t / 0.9, 0, 1)) ** 0.8
        flop = seq([(0, thump(160, 60, 0.3, 0.015, 0.06, 2.0, 0.1, i), 1.0), (0, lp(rng.standard_normal(N(0.15)), 1500) * _env_dec(N(0.15), 0.04), 0.6)])
        ch = swoosh(rng, 0.3, 400, 1500, 1.4, peak=0.5, tone=0.3, tone_ratio=0.25)
        return finish(seq([(0, cloth, 0.6), (0.55, ch, 0.3), (0.84, flop, 0.9)]), 1.1, 0.12)
    n2 = N(0.75)
    t2 = ts(n2)
    cl = svf_lp(rng.standard_normal(n2), 1100 - 400 * t2 / 0.75, 1.2) * (np.sin(TAU * (8 + i) * t2) ** 2 * 0.6 + 0.4)
    cl *= np.sin(np.pi * np.clip(t2 / 0.75, 0, 1)) ** 0.6
    clack = seq([(0, K.rim(0.9, 620 + 40 * i), 1.0), (0, thump(320, 140, 0.1, 0.008, 0.025, 1.6, 0.1, i), 0.7)])
    return finish(seq([(0, cl, 0.7), (0.7, clack, 0.8)]), 0.85, 0.1)


def stand_appear(i, rng):
    m = pent(64, i)
    n = N(0.14)
    t = ts(n)
    bw = svf_lp(pulse(phase_of(300 * 3 ** (t / 0.14), n), 0.4), 2200, 1.4) * env_ar(n, 0, 1, 1, 0.8)
    parts = [(0, bw, 0.3), (0, swoosh(rng, 0.14, 600, 2400, 1.6, peak=0.8), 0.3), (0.14, thump(190, 70, 0.25, 0.012, 0.05, 2.0, 0.15, i), 0.9),
             (0.14, clapish(rng, 0.1), 0.35), (0.15, fm_pluck(nf(m), 0.35, 0.8, ratio=2.0, idx=2.2, dec=0.1), 0.5),
             (0.15, K.bass(nf(m - 24), 0.2, 0.8, 'slap'), 0.3)]
    return finish(seq(parts), 0.5, 0.08)


def item_pop(i, rng):
    m = pent(76, i % 5) + (i // 5)
    parts = [(0, thump(560, 220, 0.07, 0.006, 0.02, 1.4, 0.1, i), 0.8), (0.025, fm_pluck(nf(m), 0.3, 0.8, ratio=2.0, idx=2.4, dec=0.09), 0.6),
             (0.025, K.glock(nf(m + 12), 0.4, 0.5), 0.3), (0.04, sparkle(rng, 0.25, 91, 3, 0.1, dec=0.05), 0.15)]
    return finish(seq(parts), 0.35, 0.06)


def approach(i, rng):
    m = pent(84, i % 5)
    parts = [(0, K.vibes(nf(m), 0.3, 0.6), 0.6), (0, fm_pluck(nf(m), 0.25, 0.4, ratio=3.0, idx=1.0, dec=0.07), 0.3),
             (0, air(rng, 0.15, 2500, 7000, 0.04, 0.01), 0.08)]
    return finish(seq(parts), 0.3, 0.08)


def focus(i, rng):
    return finish(seq([(0, K.tick(0.6, i % 2 == 0), 0.7), (0, blip(1200 + 80 * i, 0.04, 0.5, 2600, 1.0, 0.012), 0.5)]), 0.06, 0.02)


def buy_gold(i, rng):
    """Cash-register 'ka-ching' reinterpreted: drawer clunk + bell chime + coin cascade + brass blip."""
    root = 72 + (0, 2, 4, 5)[i % 4]
    parts = [(0, thump(240, 90, 0.15, 0.01, 0.04, 2.0, 0.2, i), 0.8), (0, K.rim(0.9, 1300), 0.4),
             (0.04, coin_ring(nf(root + 19), 0.7, 0.3, i), 0.7), (0.06, coin_ring(nf(root + 24), 0.7, 0.3, i + 1), 0.6),
             (0.05, K.chord_stab([nf(root + m) for m in (0, 4, 7)], 0.1, 1.0, 'brass'), 0.4)]
    for k in range(4):
        parts.append((0.1 + 0.035 * k, coin_ring(nf(pent(88, k + i)), 0.2, 0.06, i + 10 + k), 0.3))
    parts.append((0.06, sparkle(rng, 0.6, 93, 5, 0.3, dec=0.08), 0.15))
    return finish(room(seq(parts), 0.4, 0.12), 0.9, 0.15)


def buy_dream(i, rng):
    """Shimmering vibes + airy vox 'ahh' + pad."""
    root = 72 + 2 * i
    parts = [(0, K.vibes(nf(root), 1.0, 0.7), 0.45), (0.08, K.vibes(nf(root + 4), 1.0, 0.65), 0.4), (0.16, K.vibes(nf(root + 9), 1.3, 0.7), 0.45),
             (0.1, K.vox(nf(root + 4), 0.7, 0.8, 'a', 'a', gender=1.35, breath=0.4, seed=i), 0.3),
             (0, K.pad(nf(root - 12), 0.9, 0.6, bright=0.5), 0.3), (0, K.pad(nf(root - 5), 0.9, 0.6, bright=0.5), 0.25),
             (0.1, sparkle(rng, 1.0, 91, 8, 0.7, dec=0.12), 0.18)]
    return finish(room(seq(parts), 0.8, 0.22), 1.6, 0.3)


def deny(i, rng, currency='gold'):
    """Low buzzy double blip + currency colour."""
    f = {'gold': 210, 'egg': 180, 'hp': 150}[currency] * (1 + 0.06 * i)
    parts = [(0, blip(f, 0.08, 0.28, 1500, 2.0, 0.05), 0.8), (0.1, blip(f * 0.84, 0.1, 0.28, 1300, 2.0, 0.06), 0.8)]
    if currency == 'gold':
        n = N(0.24)
        t = ts(n)
        wah = svf_lp(saw(phase_of(nf(46 + i) * (1 - 0.06 * t / 0.24), n)), 300 + 900 * np.sin(np.pi * np.clip(t / 0.24, 0, 1)), 3.0)
        parts.append((0.0, wah * env_ar(n, 0, 1, 1.5, 0.15), 0.4))
    elif currency == 'egg':
        n = N(0.2)
        t = ts(n)
        bonk = np.sin(phase_of(260 * (1 + 0.6 * np.exp(-t / 0.02)) * (1 - 0.3 * t / 0.2), n)) * np.exp(-t / 0.06)
        parts.append((0.0, bonk, 0.5))
    else:
        parts.append((0.0, thump(100, 42, 0.25, 0.015, 0.07, 1.8, 0.0, i), 0.7))
    return finish(seq(parts), 0.35, 0.05)


def invalid(i, rng):
    return finish(seq([(0, blip(160 + 20 * i, 0.08, 0.2, 1200, 1.6, 0.04), 0.9), (0, K.tick(0.3, False), 0.3)]), 0.08, 0.02)


def reroll(i, rng):
    """Slot-machine shuffle: rapid random-pitch blips + whirr, then a ding."""
    parts = []
    for k in range(8):
        m = pent(72, int(rng.integers(0, 8)))
        parts.append((0.038 * k, blip(nf(m), 0.04, 0.4, 3600, 1.2, 0.015), 0.4))
        parts.append((0.038 * k, K.tick(0.3, k % 2 == 0), 0.2))
    n = N(0.32)
    t = ts(n)
    wh = svf_bp(rng.standard_normal(n), 900 + 1500 * t / 0.32, 2.0) * (0.5 + 0.5 * np.sin(TAU * 26 * t)) * env_ar(n, 0, 1, 1, 0.5)
    parts.append((0, wh, 0.25))
    parts.append((0.34, coin_ring(nf(86 + 2 * i), 0.5, 0.2, i), 0.6))
    parts.append((0.34, K.chord_stab([nf(60 + 2 * i), nf(67 + 2 * i), nf(72 + 2 * i)], 0.1, 0.8, 'pluck'), 0.35))
    parts.append((0.34, thump(200, 80, 0.12, 0.01, 0.03, 1.6, 0.1, i), 0.4))
    return finish(seq(parts), 0.9, 0.15)


def fusion(i, rng):
    hit = 0.6
    root = 60 + 2 * i
    pre = K.pad(nf(root - 12), hit, 0.6, bright=0.4, voices=6, detune=0.014)
    n = N(hit)
    pre = fit(pre, n) * np.linspace(0.2, 1, n) ** 2
    parts = [(0, K.riser(hit, seed=i, f0=300, f1=6000), 0.55), (0, pre, 0.3),
             (hit, K.impact_boom(1.0, 0.8, seed=i), 0.7), (hit, K.chord_stab([nf(root + m) for m in (0, 4, 7, 11, 14)], 0.6, 1.0, 'brass'), 0.5),
             (hit, K.crash(0.6, seed=i, dur=1.4), 0.25), (hit, sub_drop(100, 38, 0.6, 0.2), 0.5)]
    for k, s in enumerate((0, 4, 7, 11, 14)):
        parts.append((hit + 0.05 + 0.05 * k, K.glock(nf(root + 12 + s), 0.7, 0.6), 0.4))
    parts.append((hit + 0.05, sparkle(rng, 1.0, 93, 10, 0.6, dec=0.12), 0.2))
    return finish(room(seq(parts), 0.7, 0.15), 2.0, 0.35)


def descend(i, rng):
    parts = [(0, K.downlifter(1.0, seed=i), 0.6)]
    for k in range(5):
        parts.append((0.1 * k, blip(nf(79 - 3 * k - 2 * i), 0.09, 0.35, 2600 - 300 * k, 1.4, 0.04), 0.4))
    n = N(0.9)
    t = ts(n)
    zd = svf_lp(saw(phase_of(nf(67 - i) * 0.25 ** (t / 0.9), n)), 1800 * 0.3 ** (t / 0.9), 1.4) * env_ar(n, 0, 1, 1.2, 0.1)
    parts += [(0, zd, 0.25), (0.75, thump(110, 42, 0.25, 0.02, 0.07, 1.8, 0.0, i), 0.5)]
    return finish(seq(parts), 1.0, 0.2)


# ------------------------------------------------------------------------- UI
def ui(i, rng, kind='select'):
    """Retro-tech / video-store / CRT menu sounds: clicks, tape-deck clunks, chirps, filtered square blips."""
    if kind == 'hover':
        y = seq([(0, blip(2100 + 140 * i, 0.03, 0.5, 3200, 0.9, 0.006), 0.6), (0, K.tick(0.3, i % 2 == 0)[:N(0.03)], 0.25)])
        return finish(y, 0.035, 0.012)
    if kind == 'select':
        m = pent(76, i % 5)
        return finish(seq([(0, K.rim(0.6, 2000 + 100 * i), 0.4), (0, blip(nf(m), 0.12, 0.35, 3800, 1.2, 0.04), 0.7),
                           (0, fm_pluck(nf(m + 12), 0.12, 0.4, ratio=2.0, idx=1.5, dec=0.04), 0.3)]), 0.18, 0.04)
    if kind == 'confirm':
        r = 72 + 2 * i
        return finish(seq([(0, blip(nf(r + 7), 0.1, 0.4, 4500, 1.2, 0.05), 0.7), (0.065, blip(nf(r + 14), 0.2, 0.4, 5000, 1.2, 0.08), 0.75),
                           (0.065, fm_pluck(nf(r + 19), 0.25, 0.4, ratio=3.0, idx=1.4, dec=0.08), 0.35),
                           (0, thump(300, 140, 0.06, 0.006, 0.02, 1.2, 0.0, i), 0.3)]), 0.38, 0.08)
    if kind == 'back':
        r = 74 - 2 * i
        return finish(seq([(0, blip(nf(r + 5), 0.08, 0.4, 3000, 1.2, 0.04), 0.7), (0.065, blip(nf(r - 2), 0.16, 0.4, 2400, 1.2, 0.06, glide=0.97), 0.7),
                           (0, K.rim(0.4, 1500), 0.2)]), 0.3, 0.07)
    if kind == 'open':
        clunk = seq([(0, thump(320, 120, 0.08, 0.008, 0.025, 1.6, 0.2, i), 1.0), (0, K.rim(0.7, 900 + 100 * i), 0.5)])
        return finish(seq([(0, clunk, 0.7), (0.02, swoosh(rng, 0.2, 500, 3000, 1.5, peak=0.6, tone=0.3), 0.4),
                           (0.12, blip(nf(84 + i), 0.15, 0.4, 4500, 1.2, 0.05, glide=1.06), 0.45)]), 0.38, 0.08)
    if kind == 'close':
        clunk = seq([(0, thump(280, 100, 0.08, 0.008, 0.025, 1.6, 0.2, i), 1.0), (0, K.rim(0.7, 750 + 80 * i), 0.5)])
        return finish(seq([(0, swoosh(rng, 0.16, 3000, 500, 1.5, peak=0.3, tone=0.3), 0.45),
                           (0, blip(nf(79 + i), 0.08, 0.4, 3000, 1.2, 0.03, glide=0.9), 0.35), (0.09, clunk, 0.7)]), 0.3, 0.06)
    if kind == 'page':   # cassette/paper flick
        parts = []
        for k in range(2 + i % 2):
            parts.append((0.035 * k, grains(rng, 0.04, 6, 1500, 6000, 0.003, 0.02), 0.6))
        parts.append((0, swoosh(rng, 0.14, 1200, 4000, 1.6, peak=0.4), 0.4))
        parts.append((0.01, K.tick(0.3, True), 0.25))
        return finish(seq(parts), 0.2, 0.05)
    if kind == 'tooltip':
        return finish(seq([(0, blip(nf(91 + 2 * i), 0.07, 0.5, 4500, 0.9, 0.025), 0.5),
                           (0, ting(nf(91 + 2 * i), 0.1, ratio=2.0, idx=0.6, dec=0.03), 0.3)]), 0.1, 0.03)
    if kind == 'tab':
        return finish(seq([(0, K.rim(0.6, 1700 + 150 * i), 0.5), (0, blip(nf(pent(79, i)), 0.05, 0.4, 3500, 1.2, 0.02), 0.6)]), 0.06, 0.02)
    if kind == 'pause':   # tape-deck stop: clunk + two notes drooping down
        n = N(0.5)
        a = seq([(0, blip(nf(79), 0.14, 0.4, 3000, 1.0, 0.07), 1.0), (0.08, blip(nf(72), 0.35, 0.4, 2400, 1.0, 0.15, glide=0.85), 1.0)])
        a = K.tape_stop(fit(a, n), 0.48) * 0.5 + fit(a, n) * 0.5
        return finish(seq([(0, a, 0.8), (0, thump(260, 100, 0.08, 0.008, 0.025, 1.6, 0.2, i), 0.5), (0, K.rim(0.6, 800), 0.3)]), 0.5, 0.1)
    if kind == 'resume':
        return finish(seq([(0, thump(260, 100, 0.08, 0.008, 0.025, 1.6, 0.2, i), 0.5), (0, blip(nf(72), 0.1, 0.4, 3000, 1.0, 0.05), 0.7),
                           (0.07, blip(nf(76), 0.1, 0.4, 3800, 1.0, 0.05), 0.7), (0.14, blip(nf(79), 0.25, 0.4, 4500, 1.0, 0.1, glide=1.02), 0.8),
                           (0.14, fm_pluck(nf(91), 0.25, 0.3, ratio=3.0, idx=1.2, dec=0.08), 0.3)]), 0.5, 0.1)
    if kind == 'setting':
        return finish(seq([(0, K.tick(0.6, i % 2 == 0), 0.6), (0, blip(1500 + 160 * i, 0.03, 0.5, 2800, 1.0, 0.008), 0.4)]), 0.04, 0.012)
    if kind == 'unlock':
        r = 67 + 3 * i
        parts = [(0, seq([(0, thump(300, 110, 0.1, 0.008, 0.03, 1.8, 0.2, i), 1.0), (0, K.rim(0.8, 1000), 0.6)]), 0.7),
                 (0.08, K.chord_stab([nf(r + m) for m in (0, 4, 7)], 0.12, 0.9, 'pluck'), 0.45), (0.08, K.glock(nf(r + 12), 0.8, 0.7), 0.5),
                 (0.17, K.glock(nf(r + 19), 1.0, 0.7), 0.5), (0.17, sparkle(rng, 0.7, 93, 6, 0.4, dec=0.1), 0.18)]
        return finish(room(seq(parts), 0.5, 0.15), 1.2, 0.2)
    if kind == 'achievement':
        r = 72 + 2 * i
        parts = [(0.06 * k, K.glock(nf(pent(r, k)), 0.6, 0.6), 0.45) for k in range(5)]
        hit = 0.32
        parts += [(hit, K.chord_stab([nf(r - 12 + m) for m in (0, 4, 7, 11, 14)], 0.5, 1.0, 'brass'), 0.55),
                  (hit, K.kick(0.9, 'punch'), 0.5), (hit, K.crash(0.6, seed=i, dur=1.4), 0.25), (hit, sub_drop(100, 40, 0.4, 0.15), 0.4),
                  (hit, sparkle(rng, 1.2, 93, 10, 0.8, dec=0.12), 0.2), (hit, ting(nf(r + 24), 1.2, ratio=3.0, idx=1.4, dec=0.4), 0.2)]
        return finish(room(seq(parts), 0.6, 0.15), 1.8, 0.3)
    if kind == 'receipt':   # dot-matrix printer + tear + ding
        parts = []
        for k in range(10):
            for q in range(3):
                parts.append((0.045 * k + 0.009 * q, blip(1300 + 90 * ((k + i) % 3) + 40 * q, 0.008, 0.3, 3500, 1.0, 0.004), 0.3))
            parts.append((0.045 * k, K.tick(0.25, k % 2 == 0), 0.2))
        parts.append((0.46, swoosh(rng, 0.08, 1500, 5000, 1.6, peak=0.2), 0.3))
        parts.append((0.5, coin_ring(nf(81 + 2 * i), 0.4, 0.15, i), 0.5))
        parts.append((0.5, fm_pluck(nf(93 + 2 * i), 0.3, 0.4, ratio=3.0, idx=1.0, dec=0.1), 0.25))
        return finish(seq(parts), 0.9, 0.12)
    raise KeyError(kind)


# --------------------------------------------------------------------- stings
def _hit(parts, o, clip, g=1.0):
    parts.append((o, clip, g))


def sting(i, rng, kind='waveclear'):
    """Short funky musical jingles: brass stabs, slap bass, drums, vox chops, scratch."""
    if kind == 'waveclear':
        bpm = 120
        s = 60 / bpm / 4
        root = 40 + 2 * i
        parts = []
        for k, (o, m) in enumerate(((0, 0), (2, 0), (3, 12), (4, 10), (6, 7), (7, 12))):
            _hit(parts, o * s, K.bass(nf(root + m), s * 0.9, 0.9, 'slap'), 0.6)
        for o in (0, 3, 6):
            _hit(parts, o * s, K.kick(0.9, 'punch'), 0.55)
        _hit(parts, 4 * s, K.snare(0.9, seed=i), 0.45)
        for o in range(8):
            _hit(parts, o * s, K.hat(0.6 if o % 2 else 0.9, seed=o + i), 0.2)
        ch = [nf(root + 24 + m) for m in (0, 3, 7, 10)]
        for o in (1, 5):
            _hit(parts, o * s, K.chord_stab(ch, s * 0.6, 0.9, 'brass'), 0.4)
        _hit(parts, 6 * s, K.scratch(0.18, seed=i, rate=9.0, src_freq=220), 0.25)
        hit = 8 * s
        fin = [nf(root + 24 + m) for m in (0, 4, 7, 11, 14)]
        _hit(parts, hit, K.chord_stab(fin, 0.6, 1.0, 'brass'), 0.55)
        _hit(parts, hit, K.kick(1.0, 'punch'), 0.6)
        _hit(parts, hit, K.crash(0.6, seed=i, dur=1.2), 0.25)
        _hit(parts, hit, K.bass(nf(root), 0.5, 1.0, 'slap'), 0.6)
        _hit(parts, hit + 0.02, K.vox(nf(root + 24 + 7), 0.32, 0.9, 'y', 'a', gender=1.05, breath=0.12, seed=i), 0.35)
        for k in range(4):
            _hit(parts, hit + 0.06 + 0.06 * k, K.glock(nf(pent(root + 48, k + 1)), 0.5, 0.5), 0.3)
        return finish(room(seq(parts), 0.6, 0.12), 1.9, 0.3)
    if kind == 'victory':
        bpm = 126
        s = 60 / bpm / 4
        root = 43
        parts = []
        for k in range(4):   # drum fill pickup
            _hit(parts, k * s * 0.5, K.snare(0.5 + 0.12 * k, seed=k, tight=True), 0.35)
        st = 2 * s
        prog = [(0, (0, 4, 7, 11)), (4, (5, 9, 12, 16)), (6, (7, 11, 14, 17)), (10, (5, 9, 12, 16)), (12, (7, 11, 14, 17))]
        for o, ch in prog:
            _hit(parts, st + o * s, K.chord_stab([nf(root + 24 + m) for m in ch], s * 0.8, 1.0, 'brass'), 0.4)
        bassline = [(0, 0), (2, 0), (3, 12), (4, 5), (6, 7), (8, 0), (10, 5), (11, 7), (12, 7), (14, 10)]
        for o, m in bassline:
            _hit(parts, st + o * s, K.bass(nf(root + m), s * 0.9, 0.9, 'slap'), 0.55)
        for k in range(16):
            o = st + k * s
            if k % 8 in (0, 3, 6):
                _hit(parts, o, K.kick(0.9, 'punch'), 0.5)
            if k % 8 == 4:
                _hit(parts, o, K.clap(0.9, seed=k), 0.4)
            _hit(parts, o, K.hat(0.5 if k % 2 else 0.85, seed=k), 0.2)
        _hit(parts, st + 8 * s, K.vox(nf(root + 31), 0.3, 0.9, 'y', 'a', gender=1.05, breath=0.12, seed=1), 0.3)
        _hit(parts, st + 13 * s, K.scratch(0.22, seed=2, rate=10.0), 0.25)
        hit = st + 16 * s
        fin = [nf(root + 24 + m) for m in (0, 4, 7, 11, 14)]
        _hit(parts, hit, K.chord_stab(fin, 1.2, 1.0, 'brass'), 0.6)
        _hit(parts, hit, K.lead(nf(root + 36 + 7), 1.0, 0.6, 'super', glide_from=nf(root + 36)), 0.15)
        _hit(parts, hit, K.kick(1.0, 'boom'), 0.6)
        _hit(parts, hit, K.crash(0.8, seed=5, dur=2.2), 0.3)
        _hit(parts, hit, K.bass(nf(root), 1.0, 1.0, 'slap'), 0.6)
        _hit(parts, hit + 0.03, K.vox(nf(root + 36), 0.6, 0.9, 'o', 'a', gender=1.0, breath=0.15, seed=3), 0.3)
        for k in range(8):
            _hit(parts, hit + 0.1 + 0.06 * k, K.glock(nf(pent(root + 48, k)), 0.6, 0.5), 0.3)
        _hit(parts, hit + 0.1, sparkle(rng, 1.5, 93, 10, 1.0, dec=0.15), 0.15)
        return finish(room(seq(parts), 0.8, 0.14), 3.6, 0.6)
    if kind == 'defeat':
        bpm = 104
        s = 60 / bpm / 4
        root = 41
        groove = _funk_bar(root, bpm, seed=7, bars=1.0)
        parts = [(0, groove, 0.8)]
        line = [(0, 10, 3), (4, 9, 3), (8, 8, 3)]
        for o, m, ln in line:
            _hit(parts, o * s, _wah_horn(nf(root + 24 + m), ln * s), 0.45)
        pre = seq(parts)
        stop_t = 16 * s
        tail = _wah_horn(nf(root + 31), 0.9, fall=0.62)
        body = seq([(0, pre, 1.0), (12 * s, tail, 0.5)])
        body = fit(body, N(stop_t + 0.9))
        stopped = np.concatenate([body[:N(12 * s)], K.tape_stop(body[N(12 * s):], 1.0)])
        parts2 = [(0, stopped, 1.0), (stop_t + 0.45, K.impact_boom(1.6, 0.8, seed=11), 0.5), (stop_t + 0.45, K.gong_hit(nf(29), 2.0, 0.8), 0.35),
                  (stop_t + 0.5, K.vox(nf(root + 12), 0.8, 0.8, 'o', 'u', gender=0.85, breath=0.25, seed=4), 0.3),
                  (stop_t + 0.45, K.downlifter(1.0, seed=4), 0.2)]
        return finish(room(seq(parts2), 0.8, 0.12), 3.8, 0.6)
    if kind == 'bossdeath':
        return boss_defeat(i, rng)
    raise KeyError(kind)


def _wah_horn(f, dur, fall=1.0):
    """Muted 'wah' horn for comedic-cool defeat lines (formant saw with a closing wah)."""
    n = N(dur + 0.05)
    t = ts(n)
    ff = f * fall ** np.clip(t / dur, 0, 1)
    src = saw(phase_of(ff * (1 + 0.006 * np.sin(TAU * 5.5 * t)), n)) + 0.3 * pulse(phase_of(ff, n), 0.3)
    wah = 500 + 1300 * np.sin(np.pi * np.clip(t / (dur * 0.9), 0, 1)) ** 0.7
    y = svf_bp(src, wah, 3.0) * 0.8 + svf_bp(src, wah * 2.2, 5.0) * 0.3
    return fade(drive(y * 2.0, 1.4) * env_exp(n, 0.015, 9, 1.0, r=0.06), 0.002, 0.04)


# ------------------------------------------------------------------ ambience
def _circ_room(y, seconds=1.2, seed=5, damp=3500, wet=0.3):
    """Circular (loop-wrapping) reverb for ambience beds."""
    n = len(y)
    m = N(seconds)
    r = np.random.default_rng(seed)
    ir = r.standard_normal(m) * np.exp(-ts(m) / (seconds / 5))
    ir = lp(ir, damp)
    ir /= np.sqrt((ir ** 2).sum())
    H = np.fft.rfft(np.pad(ir, (0, max(0, n - m)))[:n])
    return y + wet * np.fft.irfft(np.fft.rfft(y) * H, n) * 2.0


def _drone(freqs, seconds, n, wob_cycles=0, wob_depth=0.0, harms=((1, 1.0),)):
    """Sum of partials snapped to whole cycles over the loop; optional integer-cycle phase wobble (tape)."""
    t = np.arange(n) / SR
    y = np.zeros(n)
    for f, a in freqs:
        for h, ha in harms:
            fh = loop_freq(f * h, seconds)
            if fh > 12000:
                continue
            pm = wob_depth * h * np.sin(TAU * wob_cycles * t / seconds + f) if wob_cycles else 0.0
            y += a * ha * np.sin(TAU * fh * t + pm + h * f)
    return y


def ambience(i, rng, kind='stage', seconds=24.0):
    """Seamless loops: periodic FFT-shaped noise, partials/LFOs with whole cycles over the loop, wrap-placed events
    and circular (tiled) filtering, so sample 0 continues sample n-1."""
    n = int(seconds * SR)
    t = np.arange(n) / SR
    if kind == 'transition':
        dur = 1.4
        m = N(dur)
        tt = ts(m)
        r = tt / dur
        if i == 0:   # rising whoosh/riser bed into the next room
            z = svf_bp(rng.standard_normal(m), 300 * 12 ** r, 1.6) * np.sin(np.pi * np.clip(r, 0, 1)) ** 0.8
            z += svf_lp(saw(phase_of(nf(43) * 2 ** r, m)) + saw(phase_of(nf(43) * 1.006 * 2 ** r, m)), 400 + 2500 * r, 1.0) * np.sin(np.pi * r) * 0.25
            z += np.sin(phase_of(55 + 30 * r, m)) * np.sin(np.pi * r) * 0.2
        else:        # swell then tape-fluttered fall
            fc = 2500 * np.where(r < 0.45, 0.15 + 0.85 * r / 0.45, 1 - 0.85 * (r - 0.45) / 0.55)
            z = svf_bp(rng.standard_normal(m), fc, 1.4) * np.sin(np.pi * np.clip(r, 0, 1)) ** 0.7
            z *= 0.8 + 0.2 * np.sin(TAU * 9 * tt)
            z += svf_lp(saw(phase_of(nf(50) * (1 + 0.3 * np.sin(np.pi * r)) * (1 + 0.01 * np.sin(TAU * 5 * tt)), m)), 1200, 1.0) * np.sin(np.pi * r) * 0.2
        return finish(z, fout=0.2)

    if kind == 'stage':
        # crowd murmur: formant-ish band noise breathing on integer-cycle LFOs + low hall air + distant calliope hum
        mur = np.zeros(n)
        for k, (lo, hi, cyc) in enumerate(((250, 700, 5), (500, 1300, 7), (900, 2200, 11))):
            b = periodic_noise(rng, n, lo, hi, -0.6)
            mur += b * (0.45 + 0.55 * loop_lfo(n, cyc, k * 1.7) * loop_lfo(n, cyc * 3 + 2, k)) * (0.6 - 0.15 * k)
        air_ = periodic_noise(rng, n, 35, 350, -0.5)
        org = _drone([(nf(48), 0.5), (nf(55), 0.35), (nf(60), 0.3), (nf(64), 0.22)], seconds, n, 37, 0.004,
                     harms=((1, 1.0), (2, 0.45), (3, 0.2), (4, 0.1)))
        org = circ_apply(lambda x: lp(x, 900), org) * (0.5 + 0.5 * loop_lfo(n, 2))
        y = mur * 0.55 + air_ * 0.45 + org / (np.std(org) + 1e-9) * 0.3
        y = _circ_room(y, 1.4, 21, 3000, 0.2)
    elif kind == 'office':
        hum = sum(np.sin(TAU * 120 * h * t + h) / h ** 1.3 for h in range(1, 9))
        hum = circ_apply(lambda x: lp(x, 1200), hum) * (0.92 + 0.08 * loop_lfo(n, 13))
        ac = periodic_noise(rng, n, 30, 450, -0.8) * 0.8 + periodic_noise(rng, n, 1500, 6000, -1.0) * 0.1
        keys = np.zeros(n)
        tk = rng.uniform(0, seconds, 9)
        for s0 in tk:   # bursts of distant keyboard typing
            o = s0
            for q in range(int(rng.integers(4, 11))):
                c = K.rim(0.3 + 0.3 * rng.random(), 1700 + 900 * rng.random())
                place_wrap(keys, lp(c, 5000), o, 0.2 + 0.2 * rng.random())
                o += 0.06 + 0.1 * rng.random()
        y = hum * 0.1 + ac * 0.5 + keys * 4.0
        y = _circ_room(y, 0.6, 22, 4000, 0.15)
    elif kind == 'lostshop':
        roomt = periodic_noise(rng, n, 40, 600, -0.8) * 0.6 + periodic_noise(rng, n, 1500, 5000, -1.2) * 0.06
        ticks = np.zeros(n)
        for s0 in range(int(seconds)):
            place_wrap(ticks, seq([(0, K.tick(0.6, s0 % 2 == 0), 1.0), (0, K.rim(0.3, 1200 if s0 % 2 else 1500), 0.3)]), s0 + 0.25, 1.0 if s0 % 2 == 0 else 0.8)
        dr = _drone([(nf(33), 0.6), (nf(40) * 1.002, 0.3), (nf(45), 0.2)], seconds, n, 3, 0.02, harms=((1, 1.0), (2, 0.3)))
        mb = np.zeros(n)
        for k, (s0, m) in enumerate(((3.0, 76), (3.45, 79), (3.9, 74), (13.0, 72), (13.45, 76), (13.9, 71))):
            place_wrap(mb, K.glock(nf(m), 0.8, 0.5), s0, 0.5)
        mb = circ_apply(lambda x: lp(x, 2500), mb)
        cr = np.zeros(n)
        for s0 in (7.3, 18.6):
            m = N(0.8)
            tt = ts(m)
            c = svf_bp(rng.standard_normal(m), 450 + 150 * tt, 7.0) * (np.sin(TAU * 9 * tt) ** 4) * np.sin(np.pi * tt / 0.8)
            place_wrap(cr, c, s0, 0.5)
        y = roomt * 0.3 + ticks * 2.6 + dr / (np.std(dr) + 1e-9) * 0.1 + mb * 0.5 + cr * 0.8
        y = _circ_room(y, 1.6, 23, 2500, 0.3)
    elif kind == 'goldshop':
        roomt = periodic_noise(rng, n, 60, 800, -0.6) * 0.6 + periodic_noise(rng, n, 2000, 6000, -1.0) * 0.04
        ev = np.zeros(n)
        for s0 in (1.7, 5.2, 8.9, 12.4, 15.1, 19.6, 22.3):
            for q in range(int(rng.integers(1, 4))):
                place_wrap(ev, coin_ring(nf(pent(86, int(rng.integers(0, 6)))), 0.3, 0.08, int(s0 * 10) + q), s0 + 0.06 * q, 0.25)
        for s0 in (6.0, 18.0):
            place_wrap(ev, seq([(0, K.rim(0.5, 900), 1.0), (0.05, coin_ring(nf(91), 0.9, 0.35, 3), 0.6), (0.08, coin_ring(nf(96), 0.9, 0.35, 4), 0.5)]), s0, 0.35)
        rh = np.zeros(n)
        for s0, ch in ((0.0, (60, 64, 67, 71)), (6.0, (57, 60, 64, 67)), (12.0, (62, 65, 69, 72)), (18.0, (55, 59, 62, 65))):
            for m in ch:
                place_wrap(rh, K.rhodes(nf(m), 4.0, 0.35), s0, 0.15)
        rh = circ_apply(lambda x: lp(x, 1400), rh)
        y = roomt * 0.3 + ev * 1.6 + rh * 1.0
        y = _circ_room(y, 1.2, 24, 3500, 0.25)
    elif kind == 'dreamshop':
        pad = _drone([(nf(44), 0.5), (nf(51) * 1.003, 0.35), (nf(55), 0.3), (nf(60) * 0.997, 0.25), (nf(63), 0.18), (nf(70), 0.1)],
                     seconds, n, 5, 0.35, harms=((1, 1.0), (2, 0.3), (3, 0.12)))
        pad = circ_apply(lambda x: lp(x, 1600), pad) * (0.75 + 0.25 * loop_lfo(n, 3))
        sh = periodic_noise(rng, n, 3000, 8000, -0.5) * (0.3 + 0.7 * loop_lfo(n, 7, 1.0) * loop_lfo(n, 4))
        tw = np.zeros(n)
        for k in range(14):
            s0 = rng.uniform(0, seconds)
            place_wrap(tw, ting(nf(pent(87, int(rng.integers(0, 8)))), 0.8, ratio=3.0, idx=0.8, dec=0.25), s0, 0.15 + 0.1 * rng.random())
        y = pad / (np.std(pad) + 1e-9) * 0.35 + sh * 0.08 + tw * 1.5
        y = _circ_room(y, 2.5, 25, 4000, 0.45)
    elif kind == 'toys':
        roomt = periodic_noise(rng, n, 50, 500, -0.7) * 0.5
        wind = np.zeros(n)
        for s0 in (2.2, 9.0, 15.5, 20.5):
            dur = 0.7 + 0.3 * rng.random()
            m = N(dur)
            tt = ts(m)
            rate = 18 + 6 * rng.random()
            clicks = (np.diff(np.floor(np.cumsum(np.full(m, rate)) / SR), prepend=0) > 0).astype(float)
            c = bp(np.convolve(clicks, np.hanning(30), 'same') + 0.05 * rng.standard_normal(m), 1500, 6000) * np.sin(np.pi * tt / dur) ** 0.5
            place_wrap(wind, c, s0, 0.4)
        mb = np.zeros(n)
        phrase = (79, 76, 74, 76, 72, 74, 71, 72)
        for k, m in enumerate(phrase):
            place_wrap(mb, seq([(0, K.glock(nf(m), 0.8, 0.6), 1.0), (0, fm_pluck(nf(m + 12), 0.4, 0.5, 3.5, 1.0, 0.1), 0.3)]), 4.0 + 0.5 * k, 0.5)
            place_wrap(mb, K.glock(nf(m - 2), 0.8, 0.5), 16.0 + 0.55 * k, 0.4)
        mb = circ_apply(lambda x: lp(x, 2200), mb)
        y = roomt * 0.2 + wind * 3.0 + mb * 1.6
        y = _circ_room(y, 1.4, 26, 3000, 0.35)
    else:
        raise KeyError(kind)
    y = circ_apply(lambda x: hp(lp(x, 13000), 30), y)
    y = y / (np.max(np.abs(y)) + 1e-9) * 0.5
    return y


# ----------------------------------------------------------------------- voice
VOW = {'a': (800, 1250, 2600), 'e': (500, 1800, 2600), 'i': (320, 2300, 3000), 'o': (520, 900, 2450),
       'u': (360, 720, 2400), 'y': (420, 1900, 2500), 'm': (280, 1000, 2300)}


def _voice(f0, dur, v0, v1=None, gender=1.0, breath=0.1, rasp=0.0, seed=0, cons=None, att=0.008, contour=None, vib=0.0):
    """Formant gibberish unit: optional consonant onset + glottal source through three time-varying formants."""
    rng = np.random.default_rng(seed)
    cd = {'k': 0.022, 't': 0.018, 'p': 0.016, 'b': 0.02, 'd': 0.018, 'm': 0.035, 'n': 0.03, 's': 0.04, 'h': 0.03,
          'y': 0.02, 'r': 0.022, 'g': 0.02, None: 0.0}[cons]
    nc = N(cd) if cd else 0
    n = N(dur)
    t = ts(n)
    fc = np.full(n, float(f0)) if contour is None else f0 * contour(t / dur)
    if vib:
        fc = fc * (1 + vib * np.sin(TAU * 6.0 * t))
    if rasp:
        jit = lp(rng.standard_normal(n), 60)
        fc = fc * (1 + rasp * 0.03 * jit / (np.std(jit) + 1e-9))
    ph = phase_of(fc, n)
    src = saw(ph) * 0.8 + 0.3 * np.sin(ph)
    if rasp:
        src += rasp * 0.5 * np.sin(ph * 0.5) * (0.5 + 0.5 * np.sign(np.sin(ph * 0.5)))
        src *= 1 + rasp * 0.4 * lp(rng.standard_normal(n), 120) / 0.3
    src = lp(src, 5000) + breath * bp(rng.standard_normal(n), 400, 6000)
    a0 = np.array(VOW[v0], dtype=float) * gender
    a1 = np.array(VOW[v1 or v0], dtype=float) * gender
    mix = np.clip(t / max(dur * 0.7, 0.03), 0, 1)
    y = np.zeros(n)
    for k, (amp, q) in enumerate(((1.0, 5), (0.6, 8), (0.25, 10))):
        y += amp * svf_bp(src, a0[k] + (a1[k] - a0[k]) * mix, q)
    e = np.minimum(1, t / att) * np.clip((dur - t) / 0.025, 0, 1) ** 1.2 * (1 - 0.25 * t / dur)
    y = drive(y * 2.0 * e, 1.2 + rasp)
    if nc:
        tc = ts(nc)
        if cons in ('k', 't', 'p', 'g', 'b', 'd'):
            lo, hi = {'k': (1500, 4000), 'g': (900, 3000), 't': (3000, 7000), 'd': (2000, 5000), 'p': (500, 2500), 'b': (200, 1500)}[cons]
            c = bp(rng.standard_normal(nc), lo, hi) * np.exp(-tc / 0.004) * (1.2 if cons in 'ktp' else 0.8)
            c[int(nc * 0.5):] *= 0.2
        elif cons in ('m', 'n'):
            c = lp(np.sin(phase_of(fc[0], nc)) + 0.3 * saw(phase_of(fc[0], nc)), 450 if cons == 'm' else 600) * np.minimum(1, tc / 0.006) * 0.6
        elif cons == 's':
            c = bp(rng.standard_normal(nc), 3500, 8000) * np.sin(np.pi * tc / cd) * 0.35
        elif cons == 'h':
            c = svf_bp(rng.standard_normal(nc), a0[1], 4) * np.minimum(1, tc / 0.01) * 0.5
        else:  # y / r: quick glide-in voiced onset
            c = svf_bp(saw(phase_of(fc[0], nc)), 2000 * gender if cons == 'y' else 1300 * gender, 6) * np.minimum(1, tc / 0.005) * 0.6
        y = np.concatenate([c, y])
    return fade(y, 0.0008, 0.012)


SYL = [('k', 'a', 'e'), ('n', 'e', None), ('b', 'o', 'a'), ('t', 'i', None), ('m', 'a', 'o'), ('h', 'e', 'i'), ('d', 'u', 'a'), ('y', 'a', None)]


def syllable(i, rng, keeper='gold'):
    """Gibberish phoneme (~70-140 ms). Gold = low, raspy, merchant-y; Dream = high, breathy, airy."""
    gold = keeper == 'gold'
    cons, v0, v1 = SYL[i % 8]
    dur = (0.075 + 0.012 * ((i * 3) % 5)) if gold else (0.08 + 0.012 * ((i * 5) % 5))
    base = (118 if gold else 245) * (1 + 0.05 * (((i * 5) % 7) - 3))
    up = i % 2 == 0
    contour = (lambda r: 1 + 0.12 * np.sin(np.pi * r) - 0.05 * r) if up else (lambda r: 1.08 - 0.14 * r)
    if gold:
        y = _voice(base, dur, v0, v1, gender=0.9, breath=0.07, rasp=0.45, seed=100 + i, cons=cons, contour=contour)
    else:
        y = _voice(base, dur, v0, v1, gender=1.25, breath=0.38, rasp=0.0, seed=200 + i, cons=cons, contour=contour, vib=0.015)
    return finish(y, fout=0.012)


def vocal_gesture(i, rng, keeper='gold', kind='laugh'):
    gold = keeper == 'gold'
    g, br, ra = (0.9, 0.07, 0.45) if gold else (1.25, 0.38, 0.0)
    base = 118 if gold else 245
    sd = (300 if gold else 400) + 10 * i + {'laugh': 0, 'hmm': 50, 'gasp': 100, 'farewell': 150}[kind]
    parts = []
    if kind == 'laugh':
        v = ('o', 'a')[i] if gold else ('i', 'e')[i]
        for k in range(4):
            f = base * (1.25 - 0.07 * k) * (1 + 0.05 * i)
            u = _voice(f, 0.075 + 0.01 * (k == 3), v, None, g, br + 0.1, ra, sd + k, cons='h',
                       contour=lambda r: 1 + 0.06 * np.sin(np.pi * r))
            parts.append((0.115 * k + 0.01 * i * k, u * (1 - 0.12 * k), 1.0))
    elif kind == 'hmm':
        dur = 0.5
        if i == 0:
            ctr = lambda r: 1 + 0.18 * np.sin(np.pi * r * 0.9) - 0.04 * r      # "hmm?" rise-fall
        else:
            ctr = lambda r: 1 - 0.06 * np.sin(np.pi * r) + 0.16 * r ** 2      # "hmm..." dip then up
        u = _voice(base * (1 + 0.04 * i), dur, 'm', 'u', g, br * 0.6, ra * 0.7, sd, cons=None, att=0.03, contour=ctr, vib=0.006)
        parts.append((0, lp(u, 1400), 1.0))
    elif kind == 'gasp':
        m = N(0.18)
        tt = ts(m)
        inh = svf_bp(np.random.default_rng(sd).standard_normal(m), (900 + 900 * tt / 0.18) * g, 3.0) * (tt / 0.18) ** 1.5
        parts.append((0, inh, 0.6))
        u = _voice(base * 1.35, 0.12, ('o', 'a')[i], ('o', 'e')[i], g, br, ra, sd, cons='h', contour=lambda r: 1.08 - 0.12 * r)
        parts.append((0.17, u, 1.0))
    elif kind == 'farewell':
        seqv = [('b', 'a', None), ('y', 'i', None), ('b', 'o', 'u')] if i == 0 else [('t', 'a', None), ('n', 'e', None), ('m', 'o', 'u')]
        o = 0.0
        for k, (c, v0, v1) in enumerate(seqv):
            last = k == 2
            dur = 0.2 if last else 0.09
            f = base * (1.12 - 0.04 * k) * (1 + 0.03 * i)
            ctr = (lambda r: 1.05 - 0.2 * r) if last else (lambda r: 1 + 0.06 * np.sin(np.pi * r))
            u = _voice(f, dur, v0, v1, g, br, ra, sd + k, cons=c, contour=ctr, vib=0.01 if last else 0.0)
            parts.append((o, u, 1.0))
            o += len(u) / SR + 0.035
    return finish(seq(parts), 0.8, 0.05)


# ----------------------------------------------------------- integration-contract additions
def alarm_boom(i, rng):
    """AlarmMortar Fuse field resolves 0.9 s after Fire: the burst (Hit with cause 'expanded-fuse')."""
    parts = [(0, thump(140 * (1 + 0.05 * i), 40, 0.5, 0.03, 0.14, 3.0, 0.35, i), 1.0),
             (0, lp(hp(rng.standard_normal(N(0.4)), 200), 4500) * _env_dec(N(0.4), 0.06), 0.6),
             (0, clapish(rng, 0.2, 0.008, 3, 800, 4500, 0.05), 0.45),
             (0.01, ting(nf(76 + i), 0.5, ratio=2.76, idx=1.6, dec=0.18), 0.3),
             (0.02, grains(rng, 0.4, 25, 500, 3500, 0.006, 0.12), 0.35)]
    return finish(crush(seq(parts), 10, 1), 0.5, 0.1, drv=1.3)


def zipper_link(i, rng):
    dur = 0.24
    n = N(dur)
    t = ts(n)
    r = t / dur
    rate = 60 + 220 * r ** 1.3
    clicks = (np.diff(np.floor(np.cumsum(rate) / SR), prepend=0) > 0).astype(float)
    zz = svf_bp(np.convolve(clicks, np.hanning(24), 'same'), 1500 + 2500 * r, 1.5) * (0.4 + 0.6 * r)
    tone = np.sin(phase_of((800 + 50 * i) * 3 ** (r ** 1.5), n)) * r * 0.15
    parts = [(0, zz + tone, 1.0), (0.22, fm_pluck(nf(84 + 2 * i), 0.15, 0.7, ratio=2.0, idx=2.0, dec=0.05), 0.5)]
    return finish(seq(parts, 0.3), 0.3, 0.05)


def hazard_warn(i, rng):
    """Danger: two-tone warning that rises and accelerates, with ticks."""
    parts = []
    o = 0.0
    k = 0
    gap = 0.13
    while o < 0.52:
        f = (700 + 80 * i) * (1.26 if k % 2 else 1.0) * (1 + 0.25 * o / 0.6)
        parts.append((o, blip(f, gap * 0.85, 0.4, 3800, 1.3, gap * 0.5), 0.6))
        parts.append((o, K.tick(0.5, k % 2 == 0), 0.3))
        o += gap
        gap *= 0.86
        k += 1
    n = N(0.6)
    tt = ts(n)
    wob = svf_lp(saw(phase_of((90 + 10 * i) * (1 + 0.3 * tt / 0.6), n)), 600, 2.0) * (tt / 0.6) * (0.6 + 0.4 * np.sin(TAU * (8 + i) * tt))
    parts.append((0, wob, 0.35))
    return finish(seq(parts, 0.6), 0.6, 0.06)


def hazard_hit(i, rng):
    parts = [(0, impact(i, rng, 'metal', 1.8, 0.4), 0.8), (0, thump(120, 38, 0.6, 0.03, 0.15, 2.8, 0.3, i), 1.0),
             (0, ting(nf(55 + 2 * i), 0.7, ratio=2.76, idx=2.0, dec=0.25), 0.35), (0, K.crash(0.5, seed=i, dur=0.7), 0.07)]
    return finish(seq(parts), 0.7, 0.12, drv=1.2)


def mimic_wake(i, rng):
    n = N(0.36)
    t = ts(n)
    creak = svf_bp(rng.standard_normal(n), 400 + 250 * t / 0.36, 8.0) * (np.sin(TAU * (12 + i) * t) ** 4) * 1.5
    chomp = seq([(0, clapish(rng, 0.1, 0.004, 2, 800, 4000, 0.015), 0.7), (0, thump(260, 90, 0.15, 0.01, 0.04, 2.0, 0.2, i), 0.9),
                 (0, K.rim(0.7, 520 + 40 * i), 0.4)])
    gr = K.vox(nf(40 + i), 0.18, 0.8, 'a', 'o', gender=0.7, breath=0.5, seed=i)
    return finish(seq([(0, creak, 0.5), (0.34, chomp, 1.0), (0.36, gr, 0.35)]), 0.6, 0.08)


def ui_negative(i, rng, kind='locked'):
    if kind == 'locked':
        f = 220 + 25 * i
        return finish(seq([(0, blip(f, 0.07, 0.25, 1400, 1.6, 0.035), 0.9), (0.08, blip(f * 0.84, 0.08, 0.25, 1300, 1.6, 0.04), 0.9),
                           (0, K.rim(0.4, 700), 0.25)]), 0.2, 0.04)
    f = nf(62 - 2 * i)
    return finish(seq([(0, blip(f, 0.12, 0.28, 1800, 1.8, 0.06), 0.8), (0.1, blip(f * 0.75, 0.25, 0.28, 1500, 1.8, 0.1, glide=0.94), 0.8),
                       (0.1, thump(110, 45, 0.2, 0.012, 0.05, 1.6, 0.0, i), 0.5), (0.02, glitch(rng, 0.08, 500, 3, 0.85, 6, 2, 0.2), 0.15)]), 0.5, 0.08)


def hero_pulse(i, rng, element='ember'):
    """K11 hero mechanics (LanternStride ember burst / DreamBreath frost wave): rare (4-7 s cooldown), so it can be big."""
    if element == 'ember':
        base = fire_cast(i, rng)
        parts = [(0, base, 0.9), (0, K.impact_boom(1.0, 0.8, seed=i), 0.6), (0, K.gong_hit(nf(43 + 2 * i), 1.0, 0.8), 0.3),
                 (0.05, crackle(rng, 0.8, 50, 1500, 6000, 0.35), 0.35),
                 (0, swoosh(rng, 0.5, 300, 2200, 1.2, peak=0.3, tone=0.4, tone_ratio=0.2), 0.4)]
        return finish(seq(parts), 1.0, 0.25, drv=1.3)
    base = ice_cast(i, rng)
    pad = K.pad(nf(60 + 2 * i), 0.6, 0.6, bright=0.8, voices=5, detune=0.012) + K.pad(nf(67 + 2 * i), 0.6, 0.5, bright=0.8, voices=5)
    parts = [(0, base, 1.0), (0, pad, 0.35), (0.04, sparkle(rng, 0.8, 91, 10, 0.5, dec=0.12, ratio=1.41), 0.3),
             (0, swoosh(rng, 0.7, 1500, 6000, 2.0, peak=0.25), 0.25), (0, sub_drop(100, 40, 0.5, 0.15), 0.5)]
    return finish(room(seq(parts), 0.8, 0.15), 1.0, 0.25)
