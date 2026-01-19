"""Procedural SFX / voice / ambience generators for Arcane Brigade.

Each generator: fn(i, rng, **params) -> mono float array. `i` is the variation index so
variations differ in pitch/time/timbre without being renames of each other.
"""
import numpy as np
from synth import *

# ------------------------------------------------------------------ helpers
def seq(parts, total=None):
    """parts = [(offset_s, clip, gain)] -> mono."""
    end = max(int(o * SR) + len(c) for o, c, g in parts)
    out = np.zeros(total and int(total * SR) or end)
    for o, c, g in parts:
        i = int(o * SR)
        e = min(len(out), i + len(c))
        out[i:e] += c[:e - i] * g
    return out


def F(x, n):
    """Pad/truncate x to exactly n samples."""
    return x[:n] if len(x) >= n else np.pad(x, (0, n - len(x)))


def note(m):
    return float(mtof(m))


PENT = [0, 2, 4, 7, 9]


def pent(root, step):
    return root + PENT[step % 5] + 12 * (step // 5)


def finish(x, dur=None, fout=0.015):
    if dur and len(x) > int(dur * SR):
        x = x[:int(dur * SR)]
        fout = max(fout, min(0.25, dur * 0.3))  # truncated ring-outs get a musical decay, not a cut
    return fade(lp(x, 14000), 0.0008, fout)


# ------------------------------------------------------------------- combat
SWING = {  # kind: (f0, f1, dur, q, grain)
    'blade': (900, 4200, 0.24, 2.0, 1.0), 'spear': (1500, 5200, 0.2, 2.6, 0.8),
    'hammer': (260, 1100, 0.34, 1.2, 1.2), 'scissors': (2200, 6000, 0.14, 3.0, 0.6),
    'umbrella': (400, 1600, 0.26, 1.4, 1.1), 'whip': (1800, 7000, 0.16, 3.5, 0.7),
}


def swing(i, rng, kind='blade', light=False):
    f0, f1, dur, q, g = SWING[kind]
    j = 1 + 0.12 * (i - 2) / 2
    dur = dur * (0.75 if light else 1) * (1 + 0.06 * (i % 3))
    n = int(dur * SR)
    t = np.arange(n) / SR
    y = sweep_bp(rng.standard_normal(n), f0 * j, f1 * j, q)
    env = np.sin(np.pi * np.clip(t / dur, 0, 1)) ** 1.6
    y = y * env * (0.55 if light else 0.9)
    if kind == 'umbrella':
        y += 0.3 * lp(rng.standard_normal(n), 700) * np.exp(-t / 0.08)  # cloth flap
    if kind == 'hammer':
        y += 0.25 * np.sin(TAU * (90 + 60 * t / dur) * t) * env
    return finish(y)


def windup(i, rng, kind='blade'):
    dur = 0.18
    n = int(dur * SR)
    t = np.arange(n) / SR
    y = sweep_bp(rng.standard_normal(n), 3000 - 400 * i, 800, 2.0) * (t / dur) * 0.4
    y += 0.1 * np.sin(TAU * (220 + 400 * (t / dur)) * t) * (t / dur)
    return finish(y, fout=0.04)


def thrust(i, rng):
    return swing(i, rng, 'spear')


MAT = {  # material: (body_freq, body_decay, ring_freq, ring_decay, noise_fc, noise_amt)
    'paper': (210, 0.03, 0, 0, 5500, 0.9), 'metal': (330, 0.05, 1480, 0.28, 8000, 0.25),
    'wax': (120, 0.06, 0, 0, 900, 0.45), 'fabric': (150, 0.05, 0, 0, 1800, 0.7),
    'glass': (700, 0.03, 2600, 0.22, 9000, 0.2), 'wood': (240, 0.06, 620, 0.09, 3500, 0.35),
}


def impact(i, rng, material='wood', weight=1.0, dur=0.3):
    bf, bd, rf, rd, nf, na = MAT[material]
    j = 1 + (i - 2.5) * 0.045
    n = int(dur * SR * (0.6 + 0.4 * weight))
    t = np.arange(n) / SR
    body = np.sin(TAU * np.cumsum(bf * j * weight ** -0.4 * (1 + 0.8 * np.exp(-t / 0.015))) / SR) * np.exp(-t / (bd * (0.7 + 0.5 * weight)))
    nz = lp(hp(rng.standard_normal(n), 300), nf) * np.exp(-t / 0.018) * na
    ring = np.zeros(n)
    if rf:
        for r, a in ((1, 1), (2.32, 0.45), (3.71, 0.2)):
            ring += a * np.sin(TAU * rf * j * r * t) * np.exp(-t / (rd / r))
        ring *= 0.22
    y = (body * 0.9 + nz + ring) * np.minimum(1, t / 0.0008)
    return finish(y * 0.9)


def crit_sting(i, rng, material='wood'):
    base = impact(i, rng, material, 1.2)
    f = note(88 + [0, 2, 4, 7, 9][i % 5])
    ping = celesta(f, 0.45, 0.35, rng)
    snap = hp(rng.standard_normal(int(0.03 * SR)), 3000) * np.exp(-np.arange(int(0.03 * SR)) / (0.005 * SR)) * 0.25
    return finish(seq([(0, base, 0.9), (0.004, ping, 0.8), (0, snap, 1)]), 0.5, 0.1)


def miss(i, rng):
    return swing(i, rng, 'blade', light=True) * 0.45


def bow_draw(i, rng):
    dur = 0.2 + 0.012 * i   # matches weapon.Windup (0.15-0.24 s): string draws exactly during the windup pose
    n = int(dur * SR)
    t = np.arange(n) / SR
    creak = bp(rng.standard_normal(n), 300, 1200) * (0.4 + 0.6 * (t / dur)) * 0.4
    tension = pizz(note(48 + i), 0.3, 0.05, rng)
    y = creak * np.sin(np.pi * np.clip(t / dur, 0, 1)) ** 0.7
    y[:len(tension)] += 0
    return finish(y, fout=0.05)


def bow_release(i, rng):
    s = pizz(note(60 - 5 + i * 2), 0.35, 1.0, rng)
    w = swing(i, rng, 'whip', light=True)
    return finish(seq([(0, s, 0.9), (0.01, w, 0.5)]), 0.4, 0.05)


def pop_gun(i, rng, big=False):
    dur = 0.28 if big else 0.18
    n = int(dur * SR)
    t = np.arange(n) / SR
    f = (80 if big else 160) * (1 + 0.05 * i)
    thump = np.sin(TAU * np.cumsum(f * (1 + 2.0 * np.exp(-t / 0.02))) / SR) * np.exp(-t / (0.09 if big else 0.05))
    cork = lp(hp(rng.standard_normal(n), 700), 4500) * np.exp(-t / 0.012) * 0.6
    cork += 0 if big else F(woodblock(520 + 50 * i, 0.5, dur), n) * 0.6
    return finish((thump * 0.9 + cork) * np.minimum(1, t / 0.0006))


def fire_cast(i, rng):
    n = int(0.45 * SR)
    t = np.arange(n) / SR
    y = lp(rng.standard_normal(n), 2400) * (0.75 + 0.25 * np.sin(t * 40)) * np.sin(np.pi * np.clip(t / 0.45, 0, 1)) ** 0.6 * 0.6
    y += sweep_bp(rng.standard_normal(n), 400, 2400 + 200 * i, 1.5) * 0.4 * np.exp(-t / 0.2)
    cr = (rng.random(n) > 0.9992).astype(float)
    y += hp(lp(np.convolve(cr, np.hanning(40), 'same'), 7000), 1500) * 3
    return finish(y, fout=0.08)


def ice_cast(i, rng):
    f = note(96 + PENT[i % 5])
    parts = [(0.0, celesta(f, 0.5, 0.6, rng), 1.0), (0.025, celesta(f * 1.5, 0.4, 0.4, rng), 0.7), (0.05, celesta(f * 2.0, 0.3, 0.25, rng), 0.5)]
    sh = hp(rng.standard_normal(int(0.2 * SR)), 5000) * np.exp(-np.arange(int(0.2 * SR)) / (0.04 * SR)) * 0.2
    parts.append((0, sh, 1))
    return finish(seq(parts), 0.55, 0.08)


def arcane_cast(i, rng):
    root = 64 + (i % 3) * 2
    parts = []
    for k, st in enumerate([0, 2, 4, 5]):
        parts.append((0.055 * k, vibes(note(pent(root, st + 2)), 0.6, 0.6 - 0.1 * k, rng), 1))
    wob = saw_pad(note(root - 12), 0.45, 0.25, detune=0.012, cutoff=1200, a=0.05, r=0.2)
    parts.append((0, wob, 0.5))
    return finish(seq(parts), 0.7, 0.1)


def boomerang_fly(i, rng, loop=False):
    dur = 0.6
    n = int(dur * SR)
    t = np.arange(n) / SR
    f = 330 + 40 * i
    carrier = np.sin(TAU * f * t) + 0.4 * np.sin(TAU * f * 2.01 * t)
    am = 0.55 + 0.45 * np.sin(TAU * 18 * t)
    y = lp(carrier * am, 2200) * 0.35 + sweep_bp(rng.standard_normal(n), 500, 1800, 1.5) * 0.3
    y *= np.sin(np.pi * np.clip(t / dur, 0, 1)) ** 0.5
    return finish(y, fout=0.06)


def boomerang_return(i, rng):
    n = int(0.5 * SR)
    t = np.arange(n) / SR
    f = 260 + 380 * (t / 0.5)
    y = lp((np.sin(TAU * np.cumsum(f) / SR)) * (0.55 + 0.45 * np.sin(TAU * 22 * t)), 2400) * 0.4
    y += sweep_bp(rng.standard_normal(n), 600, 3000, 1.6) * 0.35 * (t / 0.5)
    catch = woodblock(700, 0.7, 0.1)
    return finish(seq([(0, y * np.sin(np.pi * np.clip(t / 0.5, 0, 1)) ** 0.6, 1), (0.38, catch, 0.9)]), 0.55, 0.04)


def pierce(i, rng):
    s = swing(i, rng, 'spear', light=True)
    z = pizz(note(76 + 2 * i), 0.2, 0.5, rng)
    return finish(seq([(0, s, 0.7), (0.05, z, 0.6)]), 0.3)


def split(i, rng):
    parts = [(0.0, woodblock(800, 0.8), 1), (0.045, woodblock(1050, 0.7), 1), (0.09, woodblock(1300 + 40 * i, 0.6), 1)]
    return finish(seq(parts), 0.25)


def bounce(i, rng):
    f = 420 + 55 * i
    return finish(seq([(0, marimba(f, 0.3, 0.8, rng), 1), (0, hp(rng.standard_normal(int(0.02 * SR)), 2000) * 0.1, 1)]), 0.3)


def bell_ring(i, rng, big=True):
    return finish(bell(note(72 - 12 * big + [0, 2, 4][i % 3] + (1 if i > 3 else 0)), 1.4, 0.9, rng), 1.4, 0.2)


def umbrella_pop(i, rng):
    n = int(0.3 * SR)
    t = np.arange(n) / SR
    flap = lp(rng.standard_normal(n), 900 + 200 * i) * np.exp(-t / 0.06)
    pop = np.sin(TAU * np.cumsum(220 * (1 + 1.2 * np.exp(-t / 0.02))) / SR) * np.exp(-t / 0.05)
    return finish(flap * 0.7 + pop * 0.7)


def stamp(i, rng):
    th = np.sin(TAU * np.cumsum(95 * (1 + np.exp(-np.arange(int(0.25 * SR)) / (0.015 * SR)))) / SR) * np.exp(-np.arange(int(0.25 * SR)) / (0.06 * SR))
    pa = lp(hp(rng.standard_normal(int(0.1 * SR)), 1500), 6000) * np.exp(-np.arange(int(0.1 * SR)) / (0.02 * SR)) * 0.5
    rub = woodblock(450 + 30 * i, 0.5, 0.08)
    return finish(seq([(0, th, 1.0), (0.0, pa, 1), (0.03, rub, 0.6)]), 0.3)


def snip(i, rng):
    parts = []
    for k in range(2):
        t = np.arange(int(0.12 * SR)) / SR
        y = (np.sin(TAU * (3300 + 200 * k + 80 * i) * t) + 0.6 * np.sin(TAU * (5200 + 100 * i) * t)) * np.exp(-t / 0.02)
        y += hp(rng.standard_normal(len(t)), 3000) * np.exp(-t / 0.004) * 0.5
        parts.append((0.06 * k, y * 0.5, 1))
    return finish(seq(parts), 0.25)


def camera_flash(i, rng):
    n = int(0.28 * SR)
    t = np.arange(n) / SR
    zip_ = np.sin(TAU * np.cumsum(500 + 5500 * (t / 0.2) ** 2) / SR) * (t < 0.2) * 0.15
    shutter = woodblock(1900 + 90 * i, 0.6, 0.05) + woodblock(1300 + 60 * i, 0.5, 0.05)
    return finish(seq([(0, zip_, 1), (0.2, shutter[:int(0.06 * SR)], 0.8), (0.235, woodblock(1100, 0.4, 0.05), 0.7)]), 0.32)


def music_box_note(i, rng):
    parts = [(0.07 * k, musicbox(note(60 + pent(0, s + i)), 0.6, 0.6 - 0.1 * k, rng), 1) for k, s in enumerate([0, 2, 3])]
    return finish(seq(parts), 0.8, 0.1)


def tea_splash(i, rng):
    n = int(0.3 * SR)
    t = np.arange(n) / SR
    y = bp(rng.standard_normal(n), 500, 3000) * np.exp(-t / 0.07)
    y += np.sin(TAU * np.cumsum(500 + 1500 * np.exp(-t / 0.05)) / SR) * np.exp(-t / 0.03) * 0.3
    return finish(y * 0.8)


def fish_slap(i, rng):
    n = int(0.15 * SR)
    wet = bp(rng.standard_normal(n), 800, 4000) * np.exp(-np.arange(n) / (0.03 * SR)) * 0.5
    return finish(seq([(0, impact(i, rng, 'fabric', 0.8), 1), (0, wet, 1)]), 0.3)


def alarm_ring(i, rng):
    parts = []
    for k in range(6):
        t = np.arange(int(0.045 * SR)) / SR
        parts.append((0.05 * k, (np.sin(TAU * 2000 * t) * np.exp(-t / 0.012)) * 0.25 + woodblock(1800 + 60 * i, 0.4, 0.045) * 0.3, 1))
    return finish(seq(parts), 0.4, 0.04)


def iron_press(i, rng):
    return finish(seq([(0, impact(i, rng, 'metal', 1.3), 1), (0.02, bp(rng.standard_normal(int(0.25 * SR)), 1200, 3500) * np.exp(-np.arange(int(0.25 * SR)) / (0.1 * SR)) * 0.2, 1)]), 0.4)


def hammer_slam(i, rng):
    return finish(seq([(0, impact(i, rng, 'wood', 1.8, 0.45), 1), (0.0, swing(i, rng, 'hammer', light=True), 0.25)]), 0.55)


def burn_tick(i, rng):
    n = int(0.12 * SR)
    t = np.arange(n) / SR
    return finish(hp(lp(rng.standard_normal(n), 3500), 700) * np.exp(-t / 0.025) * 0.7)


def chill_tick(i, rng):
    return finish(celesta(note(100 + PENT[i % 5]), 0.25, 0.4, rng), 0.25)


def knockback(i, rng):
    n = int(0.22 * SR)
    t = np.arange(n) / SR
    y = sweep_bp(rng.standard_normal(n), 1800, 400, 1.5) * np.sin(np.pi * np.clip(t / 0.22, 0, 1)) * 0.6
    y += np.sin(TAU * np.cumsum(140 * (1 - 0.5 * t / 0.22)) / SR) * np.exp(-t / 0.08) * 0.6
    return finish(y)


def reaction(i, rng, kind='steam'):
    n = int(0.5 * SR)
    t = np.arange(n) / SR
    if kind == 'steam':
        y = seq([(0, bp(rng.standard_normal(n), 1500, 4500) * np.exp(-t / 0.15) * 0.5, 1), (0, celesta(note(84 + i), 0.4, 0.3, rng), 1)], 0.5)
    elif kind == 'shatter':
        y = np.zeros(n)
        for k in range(8):
            o = int(rng.random() * 0.12 * SR)
            c = celesta(note(96 + PENT[k % 5] + 12 * (k % 2)), 0.18, 0.25, rng)
            y[o:o + len(c)] += F(c, min(len(c), n - o))
        y += hp(rng.standard_normal(n), 5000) * np.exp(-t / 0.03) * 0.3
    else:  # arcane flare
        y = np.zeros(n)
        for k, s in enumerate([0, 3, 7, 12]):
            c = vibes(note(72 + s + i), 0.45, 0.5, rng)
            o = int(0.04 * k * SR)
            y[o:o + len(c)] += F(c, min(len(c), n - o))
    return finish(y, 0.5, 0.08)


# -------------------------------------------------------------- movement/body
def step(i, rng, mat='felt'):
    n = int(0.09 * SR)
    t = np.arange(n) / SR
    y = lp(rng.standard_normal(n), 600 + 120 * i) * np.exp(-t / 0.02) * 0.8
    y += np.sin(TAU * (90 + 8 * i) * t) * np.exp(-t / 0.03) * 0.5
    return finish(y, fout=0.02)


def dash_whoosh(i, rng):
    n = int(0.3 * SR)
    t = np.arange(n) / SR
    y = sweep_bp(rng.standard_normal(n), 500, 3500, 1.2) * np.sin(np.pi * np.clip(t / 0.3, 0, 1)) ** 1.2 * 0.8
    y += lp(rng.standard_normal(n), 1200) * np.exp(-t / 0.05) * 0.25
    return finish(y)


def jump(i, rng):
    n = int(0.22 * SR)
    t = np.arange(n) / SR
    y = np.sin(TAU * np.cumsum(240 + 380 * (t / 0.22)) / SR) * np.exp(-t / 0.09) * 0.5
    y += F(marimba(note(64 + 2 * i), 0.2, 0.3, rng), n)
    return finish(y)


def land(i, rng):
    n = int(0.2 * SR)
    t = np.arange(n) / SR
    y = np.sin(TAU * np.cumsum(120 * (1 + np.exp(-t / 0.02))) / SR) * np.exp(-t / 0.05)
    y += lp(rng.standard_normal(n), 900) * np.exp(-t / 0.03) * 0.5
    return finish(y)


def hurt(i, rng):
    parts = [(0, impact(i, rng, 'fabric', 1.2), 1.0)]
    cl = clarinet(note(55 - 3 * (i % 3)), 0.25, 0.8, rng=rng)
    cl = cl * np.linspace(1, 0.2, len(cl))
    parts.append((0.01, np.interp(np.arange(len(cl)), np.arange(len(cl)), cl), 0.6))
    return finish(seq(parts), 0.35, 0.08)


def dodge(i, rng):
    return dash_whoosh(i, rng) * 0.8 + 0.0


def heartbeat(i, rng):
    parts = [(0, felt_kick(0.9, 0.2), 1), (0.17, felt_kick(0.6, 0.2), 1)]
    bass = upright(note(33), 0.5, 0.4, rng)
    parts.append((0, bass, 0.5))
    return finish(seq(parts), 0.5, 0.08)


def heal(i, rng):
    parts = [(0.07 * k, celesta(note(pent(72, s)), 0.6, 0.7 - 0.1 * k, rng), 1) for k, s in enumerate([0, 1, 2, 4])]
    return finish(seq(parts), 0.8, 0.12)


def player_death(i, rng):
    parts = []
    c = clarinet(note(58), 1.1, 0.9, vib=1, rng=rng)
    f = np.linspace(1, 0.5, len(c))
    sl = np.sin(TAU * np.cumsum(note(58) * (1 - 0.35 * (np.arange(len(c)) / len(c)) ** 1.5)) / SR)
    c2 = lp(sl + 0.4 * np.sin(2 * TAU * np.cumsum(note(58) * (1 - 0.35 * (np.arange(len(c)) / len(c)) ** 1.5)) / SR), 1800) * adsr(len(c), 0.03, 0.5, 0.3, 0.3) * 0.5
    parts.append((0, c2, 1))
    parts.append((0.0, gong(note(34), 1.4, 0.45), 1))
    parts.append((0.0, impact(i, rng, 'wood', 1.8), 0.6))
    return finish(seq(parts), 1.4, 0.2)


# --------------------------------------------------------------------- enemies
def spit_windup(i, rng):
    n = int(0.3 * SR)
    t = np.arange(n) / SR
    f = 150 + 250 * (t / 0.3) ** 2
    y = lp(np.sin(TAU * np.cumsum(f) / SR) * (0.6 + 0.4 * np.sin(TAU * 25 * t)), 1500) * (t / 0.3) ** 0.8
    y += bp(rng.standard_normal(n), 400, 1400) * (t / 0.3) * 0.25
    return finish(y, fout=0.04)


def spit_fire(i, rng):
    n = int(0.22 * SR)
    t = np.arange(n) / SR
    y = np.sin(TAU * np.cumsum(520 * (1 - 0.6 * t / 0.22)) / SR) * np.exp(-t / 0.07) * 0.8
    y += lp(rng.standard_normal(n), 2200) * np.exp(-t / 0.025) * 0.5
    y += F(woodblock(1000 + 60 * i, 0.4, 0.05), n)
    return finish(y)


def enemy_lunge(i, rng):
    n = int(0.3 * SR)
    t = np.arange(n) / SR
    y = sweep_bp(rng.standard_normal(n), 300, 1600, 1.2) * np.sin(np.pi * np.clip(t / 0.3, 0, 1)) * 0.8
    y += np.sin(TAU * np.cumsum(110 + 120 * t / 0.3) / SR) * 0.3 * np.sin(np.pi * np.clip(t / 0.3, 0, 1))
    return finish(y)


def enemy_hurt(i, rng):
    return impact(i, rng, ['paper', 'wax', 'fabric', 'wood', 'metal', 'glass'][i % 6], 0.9) * 0.9


def enemy_die(i, rng, size='small'):
    mat = ['paper', 'wax', 'fabric', 'wood', 'metal', 'glass'][i % 6]
    big = size == 'large'
    base = impact(i, rng, mat, 1.6 if big else 1.0, 0.5 if big else 0.3)
    puff = lp(rng.standard_normal(int(0.3 * SR)), 1500) * np.exp(-np.arange(int(0.3 * SR)) / (0.08 * SR)) * 0.35
    pop = marimba(note(52 + 3 * i), 0.3, 0.35, rng)
    return finish(seq([(0, base, 1), (0.02, puff, 1), (0.04, pop, 0.8)]), 0.55 if big else 0.4, 0.06)


def big_step(i, rng):
    n = int(0.35 * SR)
    t = np.arange(n) / SR
    y = np.sin(TAU * np.cumsum(60 * (1 + 1.5 * np.exp(-t / 0.03))) / SR) * np.exp(-t / 0.12)
    y += lp(rng.standard_normal(n), 400) * np.exp(-t / 0.06) * 0.6
    return finish(y * 0.9, fout=0.05)


def big_roar(i, rng):
    n = int(0.9 * SR)
    t = np.arange(n) / SR
    f0 = 70 + 10 * i
    f = f0 * (1 + 0.4 * np.sin(np.pi * t / 0.9))
    y = np.zeros(n)
    ph = np.cumsum(f) / SR * TAU
    for h in range(1, 9):
        y += np.sin(ph * h) / h
    y = lp(y, 1300) * (0.6 + 0.4 * np.sin(TAU * 7 * t))
    y *= np.sin(np.pi * np.clip(t / 0.9, 0, 1)) ** 0.7
    c = clarinet(note(34 + i), 0.8, 0.5, rng=rng, breath=0.0)
    y += F(c, n) * 0.4
    return finish(y * 0.8, fout=0.15)


def boss_enter(i, rng):
    parts = [(0, gong(note(31), 2.8, 0.9), 1), (0.0, felt_kick(1.0, 0.3), 1), (0.35, felt_kick(1.0, 0.3), 1), (0.7, felt_kick(1.1, 0.3), 1)]
    sw = saw_pad(note(31), 2.0, 0.5, detune=0.01, cutoff=700, a=0.8, r=0.9)
    parts.append((0.3, sw, 0.8))
    parts.append((1.0, big_roar(i, rng), 0.7))
    return finish(seq(parts), 3.0, 0.4)


def telegraph(i, rng):
    parts = []
    for k in range(6):
        parts.append((0.07 * k, woodblock(900 + 150 * k, 0.5 + 0.08 * k, 0.06), 1))
    parts.append((0, bp(rng.standard_normal(int(0.5 * SR)), 1500, 4000) * np.linspace(0, 0.3, int(0.5 * SR)), 1))
    return finish(seq(parts), 0.5, 0.06)


def boss_attack(i, rng):
    return finish(seq([(0, impact(i, rng, 'metal', 2.0, 0.5), 1), (0, big_step(i, rng), 0.7), (0, swing(i, rng, 'hammer'), 0.6)]), 0.6, 0.1)


def phase_change(i, rng):
    parts = [(0, gong(note(38), 2.4, 0.8), 1)]
    for k, s in enumerate([0, 3, 6, 10]):
        parts.append((0.1 * k, bell(note(60 + s), 1.0, 0.45, rng), 1))
    parts.append((0, sweep_bp(rng.standard_normal(int(1.0 * SR)), 300, 3500, 1.0) * np.linspace(0.1, 0.5, int(1.0 * SR)), 0.8))
    return finish(seq(parts), 2.2, 0.3)


def boss_defeat(i, rng):
    parts = [(0, gong(note(36), 3.0, 1.0), 1), (0, impact(i, rng, 'wood', 2.0, 0.6), 1)]
    for k in range(8):
        parts.append((0.08 * k, marimba(note(pent(60, 8 - k)), 0.5, 0.5, rng), 1))
    parts.append((0.7, celesta(note(96), 1.5, 0.5, rng), 1))
    return finish(seq(parts), 3.0, 0.4)


# ------------------------------------------------------------------------ loot
def coin(i, rng, mode='pickup'):
    f = note(88 + [0, 2, 4, 7, 9, 12][i % 6])
    ring = bell(f, 0.5, 0.55, rng)
    tick = metal_hat(0.8, 0.04, i + 3)
    if mode == 'drop':
        return finish(seq([(0, tick, 0.6), (0.0, ring, 0.55)]), 0.4, 0.06)
    if mode == 'bounce':
        return finish(seq([(0, ring[:int(0.2 * SR)] * np.exp(-np.arange(int(0.2 * SR)) / (0.06 * SR)), 0.5), (0, tick, 0.5)]), 0.2)
    if mode == 'vacuum':
        n = int(0.14 * SR)
        t = np.arange(n) / SR
        return finish(np.sin(TAU * np.cumsum(900 + 1800 * (t / 0.14)) / SR) * np.exp(-t / 0.05) * 0.12 + F(tick, n) * 0.4, 0.14)
    return finish(seq([(0, ring, 0.6), (0.05, bell(f * 1.5, 0.35, 0.4, rng), 0.7), (0, tick, 0.5)]), 0.5, 0.08)


def egg_pickup(i, rng):
    n = int(0.3 * SR)
    t = np.arange(n) / SR
    pop = np.sin(TAU * np.cumsum(300 * (1 + 1.5 * np.exp(-t / 0.03))) / SR) * np.exp(-t / 0.04)
    return finish(seq([(0, pop * 0.7, 1), (0.03, marimba(note(72 + 2 * (i % 3)), 0.3, 0.7, rng), 1), (0.09, marimba(note(79 + 2 * (i % 3)), 0.3, 0.6, rng), 1)]), 0.4, 0.06)


def heart_pickup(i, rng):
    return finish(seq([(0, vibes(note(67), 0.7, 0.7, rng), 1), (0.12, vibes(note(74 + (i % 2) * 2), 0.9, 0.7, rng), 1), (0, felt_kick(0.5, 0.15), 0.5)]), 0.9, 0.15)


def chest_appear(i, rng):
    parts = [(0, jump(i, rng), 0.5), (0.18, impact(i, rng, 'wood', 1.4), 0.9), (0.2, bell(note(84), 0.8, 0.5, rng), 0.7)]
    return finish(seq(parts), 0.8, 0.1)


def chest_open(i, rng):
    n = int(0.35 * SR)
    t = np.arange(n) / SR
    creak = bp(rng.standard_normal(n), 250, 1100) * np.sin(TAU * 11 * t) ** 2 * 0.3 * (t < 0.3)
    parts = [(0, creak, 1), (0.28, woodblock(500, 0.8, 0.1), 1)]
    for k, s in enumerate([0, 2, 4, 7, 9]):
        parts.append((0.33 + 0.06 * k, celesta(note(pent(76, s)), 0.8, 0.55, rng), 1))
    parts.append((0.33, hp(rng.standard_normal(int(0.4 * SR)), 5000) * np.exp(-np.arange(int(0.4 * SR)) / (0.1 * SR)) * 0.1, 1))
    return finish(seq(parts), 1.2, 0.2)


# ------------------------------------------------------------------ upgrades
def merge(i, rng):
    """MergeReveal flash (age 0.54 s): the impact only. The converging build-up is up.merge.gather, the star arpeggio is up.star2/3."""
    parts = [(0, impact(i, rng, 'wood', 1.3), 1.0), (0.0, bell(note(79 + 2 * (i % 3)), 0.9, 0.55, rng), 1),
             (0.02, hp(rng.standard_normal(int(0.3 * SR)), 5000) * np.exp(-np.arange(int(0.3 * SR)) / (0.07 * SR)) * 0.12, 1)]
    return finish(seq(parts), 0.8, 0.15)


def merge_gather(i, rng):
    """MergeReveal 0..0.54 s: materials spiral together (gather 0.32 s), result appears at 0.36 s, flash at 0.54 s."""
    n = int(0.54 * SR)
    t = np.arange(n) / SR
    sw = sweep_bp(rng.standard_normal(n), 300, 4200 + 300 * i, 1.4) * (t / 0.54) ** 1.5 * 0.45
    tone = np.sin(TAU * np.cumsum(220 * (1 + 2.2 * (t / 0.54) ** 2)) / SR) * (t / 0.54) * 0.18
    ticks = [(0.07 * k, woodblock(600 + 90 * k, 0.35, 0.04), 1) for k in range(7)]
    parts = [(0, sw, 1), (0, tone, 1)] + ticks + [(0.36, pizz(note(60 + 2 * i), 0.3, 0.5, rng), 1)]
    return finish(seq(parts, 0.55), 0.55, 0.03)


def star_up(i, rng, stars=2):
    parts = []
    steps = [0, 2, 4, 7, 9, 11][:2 + stars * 2]
    for k, s in enumerate(steps):
        parts.append((0.07 * k, celesta(note(pent(72, k)), 0.9, 0.65, rng), 1))
        parts.append((0.07 * k, vibes(note(pent(60, k)), 0.9, 0.3, rng), 1))
    parts.append((0.07 * len(steps), bell(note(96), 1.6, 0.5, rng), 1))
    return finish(seq(parts), 1.8, 0.3)


def ally_land(i, rng):
    """UnitVisual.BeginArrival: the arc lasts 0.45*0.9 = 0.405 s, touchdown at 0.405 s. Call at BeginArrival."""
    n = int(0.4 * SR)
    t = np.arange(n) / SR
    arc = sweep_bp(rng.standard_normal(n), 2500, 700, 1.0) * np.sin(np.pi * np.clip(t / 0.4, 0, 1)) * 0.25
    parts = [(0, arc, 1), (0.405, impact(i, rng, 'fabric', 1.4), 1.0), (0.41, marimba(note(50 + 2 * (i % 3)), 0.4, 0.6, rng), 1)]
    return finish(seq(parts), 1.0, 0.1)


def bond_tier(i, rng, tier=1):
    root = 60 + 2 * tier
    parts = []
    for k, s in enumerate([0, 4, 7] + ([12] if tier > 1 else [])):
        parts.append((0.05 * k, vibes(note(root + s), 1.2, 0.6, rng), 1))
        parts.append((0.05 * k, pizz(note(root - 12 + s), 0.5, 0.5, rng), 1))
    parts.append((0.0, woodblock(1200, 0.5), 1))
    return finish(seq(parts), 1.4, 0.2)


def chain_trigger(i, rng):
    parts = [(0.045 * k, woodblock(700 + 140 * k, 0.5, 0.05), 1) for k in range(4)]
    parts.append((0.2, celesta(note(91 + PENT[i % 5]), 0.7, 0.6, rng), 1))
    return finish(seq(parts), 0.8, 0.12)


def rarity(i, rng, tier='blue'):
    if tier == 'blue':
        parts = [(0, vibes(note(79), 0.8, 0.7, rng), 1), (0.09, vibes(note(84), 1.0, 0.7, rng), 1)]
        dur = 1.0
    elif tier == 'purple':
        parts = [(0, vibes(note(75), 0.9, 0.6, rng), 1), (0.1, vibes(note(79), 0.9, 0.6, rng), 1), (0.2, vibes(note(82), 1.3, 0.7, rng), 1)]
        sl = np.sin(TAU * np.cumsum(np.linspace(380, 760, int(0.5 * SR))) / SR) * np.exp(-np.arange(int(0.5 * SR)) / (0.2 * SR)) * 0.1
        parts.append((0, sl, 1))
        dur = 1.4
    else:
        parts = [(0.06 * k, celesta(note(pent(76, k)), 1.2, 0.6, rng), 1) for k in range(6)]
        parts.append((0.0, bell(note(84), 1.8, 0.45, rng), 1))
        parts.append((0.36, hp(rng.standard_normal(int(0.6 * SR)), 6000) * np.exp(-np.arange(int(0.6 * SR)) / (0.15 * SR)) * 0.12, 1))
        parts.append((0.0, gong(note(48), 1.5, 0.35), 1))
        dur = 2.0
    return finish(seq(parts), dur, 0.25)


# ----------------------------------------------------------------------- shop
def keeper_enter(i, rng, special=False):
    """WorldShop.Build: merchant arrives after MerchantArrivalDelay 0.18 s over 0.8 s => landing at 0.98 s. Call at Build (t=0).
    Gold giant drops from above (falling tone); the dream merchant rises out of the floor (swelling sweep)."""
    n = int(1.0 * SR)
    t = np.arange(n) / SR
    span = np.clip((t - 0.18) / 0.8, 0, 1)
    if not special:
        drop = np.sin(TAU * np.cumsum(60 + 560 * (1 - span) ** 2 * (t > 0.18)) / SR) * 0.22 * span
        drop += sweep_bp(rng.standard_normal(n), 2200, 400, 1.0) * span * (1 - span * 0.3) * 0.25
    else:
        drop = sweep_bp(rng.standard_normal(n), 250, 1100, 1.0) * np.sin(np.pi * span) * 0.45
        drop += np.sin(TAU * np.cumsum(110 + 150 * span) / SR) * 0.12 * span
    land_t = 0.98
    parts = [(0, drop, 1), (land_t, impact(i, rng, 'wood', 2.0, 0.5), 1), (land_t, upright(note(36 if not special else 34), 0.8, 0.8, rng), 0.8)]
    if not special:
        parts += [(land_t + 0.1, bell(note(79), 1.0, 0.4, rng), 1)]
    else:
        parts += [(land_t + 0.05, vibes(note(70), 1.8, 0.6, rng), 1), (land_t + 0.2, vibes(note(75), 1.8, 0.5, rng), 1)]
    return finish(seq(parts), 2.6, 0.3)


def keeper_exit(i, rng, special=False):
    n = int(0.8 * SR)
    t = np.arange(n) / SR
    y = sweep_bp(rng.standard_normal(n), 1800, 250, 1.2) * np.sin(np.pi * np.clip(t / 0.8, 0, 1)) * 0.6
    y += np.sin(TAU * np.cumsum(400 * (1 - 0.7 * t / 0.8)) / SR) * 0.2 * np.exp(-t / 0.4)
    return finish(y, fout=0.2)


def carpet(i, rng, mode='open'):
    """MarketLifecycle: opening progress 0.9 s from Build; departure starts CarpetDepartureDelay 0.55 s after Depart and lasts 0.75 s."""
    n = int(0.9 * SR)
    t = np.arange(n) / SR
    cloth = lp(rng.standard_normal(n), 900) * (np.sin(TAU * 7 * t) ** 2 * 0.3 + 0.1)
    if mode == 'open':
        cloth = cloth * np.sin(np.pi * np.clip(t / 0.9, 0, 1)) * 1.0
        flop = impact(i, rng, 'fabric', 1.6, 0.4)
        return finish(seq([(0, cloth, 1), (0.84, flop, 0.9)]), 1.1, 0.12)
    n2 = int(0.75 * SR)
    t2 = np.arange(n2) / SR
    cl = lp(rng.standard_normal(n2), 900) * (np.sin(TAU * 8 * t2) ** 2 * 0.3 + 0.1) * np.sin(np.pi * np.clip(t2 / 0.75, 0, 1)) ** 0.6
    pop = woodblock(520 + 30 * i, 0.6, 0.08)
    return finish(seq([(0, cl, 1), (0.7, pop, 0.8)]), 0.85, 0.1)


def stand_appear(i, rng):
    return finish(seq([(0, jump(i, rng), 0.6), (0.14, impact(i, rng, 'wood', 1.0), 0.8), (0.15, marimba(note(pent(64, i)), 0.4, 0.6, rng), 1)]), 0.5, 0.08)


def item_pop(i, rng):
    n = int(0.25 * SR)
    t = np.arange(n) / SR
    pop = np.sin(TAU * np.cumsum(380 * (1 + 1.4 * np.exp(-t / 0.025))) / SR) * np.exp(-t / 0.04) * 0.6
    return finish(seq([(0, pop, 1), (0.03, marimba(note(pent(76, i % 5)), 0.3, 0.7, rng), 1)]), 0.35, 0.05)


def approach(i, rng):
    return finish(vibes(note(pent(84, i % 5)), 0.4, 0.35, rng), 0.3, 0.06)


def focus(i, rng):
    return finish(woodblock(1250 + 70 * i, 0.5, 0.05) + clock_tick(0.4, i % 2 == 0)[:int(0.05 * SR)], 0.06)


def buy_gold(i, rng):
    parts = [(0, bell(note(79 + (i % 3)), 0.9, 0.6, rng), 1), (0.0, metal_hat(1.0, 0.05, i), 0.7)]
    for k in range(4):
        parts.append((0.05 + 0.04 * k, bell(note(88 + k * 2), 0.4, 0.3, rng), 1))
    parts.append((0.0, woodblock(480, 0.7, 0.1), 1))
    return finish(seq(parts), 0.9, 0.12)


def buy_dream(i, rng):
    parts = [(0, vibes(note(72), 1.4, 0.6, rng), 1), (0.09, vibes(note(76), 1.4, 0.55, rng), 1), (0.18, vibes(note(81), 1.8, 0.6, rng), 1)]
    sl = np.sin(TAU * np.cumsum(np.linspace(300, 1200, int(0.6 * SR))) / SR) * np.exp(-np.arange(int(0.6 * SR)) / (0.25 * SR)) * 0.1
    parts.append((0, sl, 1))
    return finish(seq(parts), 1.6, 0.25)


def deny(i, rng, currency='gold'):
    f = {'gold': 1.0, 'egg': 0.84, 'hp': 0.7}[currency] * (1 + 0.05 * i)
    parts = [(0, woodblock(520 * f, 0.9, 0.08), 1), (0.09, woodblock(430 * f, 0.9, 0.08), 1)]
    if currency == 'gold':
        parts.append((0.0, lp(clarinet(note(46), 0.22, 0.6, rng=rng), 1200), 0.6))
    elif currency == 'egg':
        parts.append((0.0, marimba(note(54), 0.3, 0.6, rng), 0.8))
    else:
        parts.append((0.0, felt_kick(0.7, 0.18), 0.8))
    return finish(seq(parts), 0.35, 0.05)


def invalid(i, rng):
    return finish(woodblock(300 + 35 * i, 0.8, 0.08) * 0.8, 0.08)


def reroll(i, rng):
    parts = [(0.04 * k, woodblock(700 + 90 * ((k * 3 + i) % 5), 0.45, 0.05), 1) for k in range(8)]
    parts.append((0.34, pizz(note(60), 0.3, 0.5, rng), 1))
    parts.append((0.3, bell(note(86), 0.7, 0.35, rng), 1))
    return finish(seq(parts), 0.9, 0.12)


def fusion(i, rng):
    parts = [(0, sweep_bp(rng.standard_normal(int(0.6 * SR)), 300, 6000, 1.2) * np.linspace(0.1, 0.5, int(0.6 * SR)), 1),
             (0, saw_pad(note(48), 0.7, 0.5, cutoff=1600, a=0.3, r=0.2), 0.7), (0.6, impact(i, rng, 'metal', 1.6), 0.7), (0.62, bell(note(72), 1.5, 0.6, rng), 1)]
    for k, s in enumerate([0, 4, 7, 11, 14]):
        parts.append((0.65 + 0.05 * k, celesta(note(72 + s), 1.0, 0.5, rng), 1))
    return finish(seq(parts), 2.0, 0.3)


def descend(i, rng):
    n = int(0.9 * SR)
    t = np.arange(n) / SR
    y = np.sin(TAU * np.cumsum(500 * (1 - 0.65 * t / 0.9)) / SR) * 0.3 * np.exp(-t / 0.5)
    y += sweep_bp(rng.standard_normal(n), 2400, 300, 1.2) * np.sin(np.pi * np.clip(t / 0.9, 0, 1)) * 0.4
    parts = [(0, y, 1)] + [(0.1 * k, woodblock(900 - 80 * k, 0.4, 0.05), 1) for k in range(5)]
    return finish(seq(parts), 1.0, 0.2)


# ------------------------------------------------------------------------- UI
def ui(i, rng, kind='select'):
    if kind == 'hover':
        return finish(woodblock(1450 + 65 * i, 0.35, 0.04), 0.04)
    if kind == 'select':
        return finish(seq([(0, woodblock(900, 0.7, 0.07), 1), (0.0, marimba(note(pent(76, i % 5)), 0.25, 0.5, rng), 1)]), 0.25)
    if kind == 'confirm':
        return finish(seq([(0, marimba(note(72), 0.4, 0.7, rng), 1), (0.07, marimba(note(79), 0.5, 0.7, rng), 1), (0.0, woodblock(800, 0.6, 0.06), 1)]), 0.5, 0.06)
    if kind == 'back':
        return finish(seq([(0, marimba(note(74), 0.3, 0.6, rng), 1), (0.07, marimba(note(67), 0.4, 0.55, rng), 1)]), 0.4, 0.06)
    if kind == 'open':
        return finish(seq([(0, sweep_bp(rng.standard_normal(int(0.25 * SR)), 500, 2600, 1.2) * 0.4, 1), (0.1, celesta(note(84), 0.5, 0.5, rng), 1)]), 0.4, 0.08)
    if kind == 'close':
        return finish(seq([(0, sweep_bp(rng.standard_normal(int(0.2 * SR)), 2600, 500, 1.2) * 0.4, 1), (0.08, woodblock(600, 0.5, 0.06), 1)]), 0.3, 0.06)
    if kind == 'page':
        n = int(0.28 * SR)
        t = np.arange(n) / SR
        y = hp(rng.standard_normal(n), 1500) * np.sin(np.pi * np.clip(t / 0.28, 0, 1)) * 0.25 * (0.6 + 0.4 * np.sin(TAU * 30 * t) ** 2)
        return finish(y, fout=0.06)
    if kind == 'tooltip':
        return finish(celesta(note(96), 0.15, 0.3, rng), 0.12)
    if kind == 'tab':
        return finish(woodblock(1000 + 90 * i, 0.5, 0.05), 0.05)
    if kind == 'pause':
        return finish(seq([(0, vibes(note(67), 0.5, 0.5, rng), 1), (0.08, vibes(note(60), 0.7, 0.5, rng), 1)]), 0.7, 0.1)
    if kind == 'resume':
        return finish(seq([(0, vibes(note(60), 0.4, 0.5, rng), 1), (0.08, vibes(note(67), 0.5, 0.5, rng), 1), (0.16, vibes(note(72), 0.7, 0.5, rng), 1)]), 0.7, 0.1)
    if kind == 'setting':
        return finish(woodblock(1000 + 40 * i, 0.45, 0.04), 0.04)
    if kind == 'unlock':
        return finish(seq([(0, woodblock(500, 0.8, 0.1), 1), (0.08, bell(note(76), 1.0, 0.5, rng), 1), (0.16, bell(note(83), 1.2, 0.5, rng), 1)]), 1.2, 0.2)
    if kind == 'achievement':
        parts = [(0.07 * k, celesta(note(pent(72, k)), 1.0, 0.55, rng), 1) for k in range(5)]
        parts += [(0.35, bell(note(88), 1.6, 0.5, rng), 1), (0, pizz(note(48), 0.5, 0.6, rng), 1)]
        return finish(seq(parts), 1.8, 0.3)
    if kind == 'receipt':
        parts = [(0.045 * k, woodblock(1400 + 60 * (k % 3), 0.35, 0.03), 1) for k in range(10)]
        parts.append((0.5, bell(note(81), 0.6, 0.4, rng), 1))
        return finish(seq(parts), 0.9, 0.1)
    raise KeyError(kind)


# --------------------------------------------------------------------- stings
def sting(i, rng, kind='waveclear'):
    if kind == 'waveclear':
        parts = [(0.08 * k, marimba(note(pent(64, s)), 0.6, 0.6, rng), 1) for k, s in enumerate([0, 2, 4, 5])]
        parts += [(0.32, vibes(note(79), 1.4, 0.5, rng), 1), (0, pizz(note(40), 0.5, 0.7, rng), 1)]
        return finish(seq(parts), 1.6, 0.25)
    if kind == 'victory':
        parts = []
        for k, (s, o) in enumerate([(0, 0.0), (4, 0.18), (7, 0.36), (12, 0.54)]):
            parts.append((o, celesta(note(72 + s), 1.6, 0.6, rng), 1))
            parts.append((o, vibes(note(60 + s), 1.6, 0.4, rng), 1))
        for s in (0, 4, 7, 12):
            parts.append((0.7, bell(note(72 + s), 2.4, 0.4, rng), 1))
        parts.append((0.7, gong(note(48), 2.5, 0.5), 1))
        parts.append((0.7, upright(note(36), 1.5, 0.9, rng), 1))
        return finish(seq(parts), 3.6, 0.5)
    if kind == 'defeat':
        parts = []
        for k, s in enumerate([0, -2, -5, -9]):
            parts.append((0.28 * k, clarinet(note(58 + s), 0.6, 0.7, vib=0.5, rng=rng), 1))
        parts.append((0.84, gong(note(34), 3.0, 0.9), 1))
        parts.append((0.84, upright(note(34), 1.5, 0.9, rng), 1))
        return finish(seq(parts), 3.8, 0.6)
    if kind == 'bossdeath':
        return boss_defeat(i, rng)
    raise KeyError(kind)


# ------------------------------------------------------------------ ambience
def ambience(i, rng, kind='stage', seconds=24.0):
    """Seamless loops: noise is built circularly via FFT shaping so the loop point is continuous."""
    n = int(seconds * SR)
    w = np.fft.rfft(rng.standard_normal(n))
    fr = np.fft.rfftfreq(n, 1 / SR)

    def shaped(lo, hi, slope=0.0):
        m = ((fr > lo) & (fr < hi)).astype(float)
        m = m * (np.maximum(fr, 1) / 1000.0) ** slope
        return np.fft.irfft(w * m, n)

    t = np.arange(n) / SR
    k = round(seconds / 6.0)  # integer cycle counts keep modulators loop-safe
    if kind == 'stage':       # night stage: low air + faint hall + distant creaks
        y = shaped(40, 400, -0.5) * 1.2 + shaped(400, 2500, -1.5) * 0.45
        y *= 0.85 + 0.15 * np.sin(TAU * k / seconds * t)
        base = f = None
    elif kind == 'office':    # fluorescent hum + ventilation + paper
        y = shaped(30, 300, -0.3) * 0.9 + shaped(2000, 7000, -1.0) * 0.18
        y += 0.05 * np.sin(TAU * 120 * t) + 0.02 * np.sin(TAU * 240 * t)
        y *= 0.9 + 0.1 * np.sin(TAU * (k + 1) / seconds * t)
    elif kind == 'lostshop':  # dim shop: low room + clock ticks
        y = shaped(40, 600, -0.8) * 1.0 + shaped(1500, 5000, -1.2) * 0.1
        tick = np.zeros(n)
        c = clock_tick(0.5, True)
        for s in range(int(seconds)):
            idx = int(s * SR)
            tick[idx:idx + len(c)] += c[:n - idx] * (1 if s % 2 == 0 else 0.8)
        y = y * 0.5 + tick * 0.6
    elif kind == 'goldshop':  # warm room tone + sparse register clinks
        y = shaped(60, 800, -0.6) * 0.7 + shaped(2000, 6000, -1.0) * 0.05
        for s in (3.1, 9.4, 14.7, 20.2):
            c = bell(note(96 + int(s) % 5), 1.2, 0.08, rng)
            idx = int(s * SR)
            e = min(n, idx + len(c))
            y[idx:e] += c[:e - idx]
    elif kind == 'dreamshop':  # drifting detuned drone + glassy shimmer
        y = np.zeros(n)
        for f, a in ((note(43), 0.5), (note(50) * 1.003, 0.35), (note(55), 0.3), (note(62) * 0.997, 0.2)):
            f = round(f * seconds) / seconds  # whole cycles -> seamless
            y += a * np.sin(TAU * f * t + 0.8 * np.sin(TAU * k / seconds * t))
        y = np.tile(y, 3); y = lp(y, 900)[n:2 * n] * 0.5 + shaped(3000, 9000, -0.5) * 0.04 * (0.5 + 0.5 * np.sin(TAU * (k + 2) / seconds * t))
    elif kind == 'toys':      # attic of toys: wind-up ratchet + room
        y = shaped(50, 500, -0.7) * 0.6
        for s in (2.2, 8.0, 15.5):
            c = hp(rng.standard_normal(int(0.9 * SR)), 1500) * (np.sin(TAU * 22 * np.arange(int(0.9 * SR)) / SR) > 0.6) * np.sin(np.pi * np.arange(int(0.9 * SR)) / (0.9 * SR)) * 0.08
            idx = int(s * SR)
            y[idx:idx + len(c)] += c[:n - idx]
    elif kind == 'transition':
        n2 = int(1.4 * SR)
        tt = np.arange(n2) / SR
        z = sweep_bp(rng.standard_normal(n2), 300, 2800, 1.0) * np.sin(np.pi * np.clip(tt / 1.4, 0, 1)) * 0.6
        return finish(z, fout=0.2)
    else:
        raise KeyError(kind)
    y = y / (np.max(np.abs(y)) + 1e-9) * 0.5
    return y


# ----------------------------------------------------------------------- voice
def syllable(i, rng, keeper='gold'):
    """Gibberish phoneme: glottal pulse train through two formants. Gold = warm/low, Dream = breathy/warbling."""
    gold = keeper == 'gold'
    dur = (0.11 + 0.012 * (i % 4)) if gold else (0.16 + 0.015 * (i % 4))
    n = int(dur * SR)
    t = np.arange(n) / SR
    base = (118 if gold else 190) * (1 + 0.06 * ((i * 5) % 7 - 3))
    contour = 1 + 0.18 * np.sin(np.pi * t / dur) * (1 if i % 2 == 0 else -0.6)
    if not gold:
        contour *= 1 + 0.04 * np.sin(TAU * 6 * t)
    ph = np.cumsum(base * contour) / SR
    pulse = np.zeros(n)
    for h in range(1, 40):
        if base * h > 5000:
            break
        pulse += np.sin(TAU * ph * h) / h ** 1.1
    vowels = [(700, 1200), (400, 2000), (300, 800), (550, 900), (800, 1500), (450, 1100), (350, 1800), (600, 1400)]
    f1, f2 = vowels[i % 8]
    if not gold:
        f1, f2 = f1 * 1.15, f2 * 1.2
    y = bp(pulse, f1 * 0.8, f1 * 1.25, 2) * 1.0 + bp(pulse, f2 * 0.85, f2 * 1.2, 2) * 0.55
    if not gold:
        y += bp(rng.standard_normal(n), 2500, 7000) * 0.08  # breath
    cons = int(0.02 * SR)
    y[:cons] += bp(rng.standard_normal(cons), 900, 3500) * np.linspace(1, 0, cons) * 0.5
    env = np.sin(np.pi * np.clip(t / dur, 0, 1)) ** 1.5
    return finish(lp(y, 4200) * env, fout=0.02)


def vocal_gesture(i, rng, keeper='gold', kind='laugh'):
    parts = []
    if kind == 'laugh':
        for k in range(4):
            parts.append((0.11 * k, syllable(2 + k * 3 + i, rng, keeper) * (1 - 0.15 * k), 1))
    elif kind == 'hmm':
        n = int(0.5 * SR)
        t = np.arange(n) / SR
        f = (120 if keeper == 'gold' else 200) * (1 + (0.1 + 0.07 * i) * np.sin(np.pi * t / 0.5)) * (1 + 0.06 * i)
        y = lp(np.sin(TAU * np.cumsum(f) / SR) + 0.5 * np.sin(2 * TAU * np.cumsum(f) / SR), 700) * np.sin(np.pi * np.clip(t / 0.5, 0, 1))
        parts.append((0, y * 0.6, 1))
    elif kind == 'gasp':
        n = int(0.3 * SR)
        t = np.arange(n) / SR
        y = bp(rng.standard_normal(n), 800, 3500) * (t / 0.3) ** 1.5 * np.exp(-t / 0.2) * 1.2
        parts.append((0, y, 1))
    elif kind == 'farewell':
        for k, ii in enumerate([4, 1, 6]):
            parts.append((0.14 * k, syllable(ii + i, rng, keeper), 1))
    return finish(seq(parts), 0.8, 0.05)


# ----------------------------------------------------------- integration-contract additions
def alarm_boom(i, rng):
    """AlarmMortar Fuse field resolves 0.9 s after Fire: the burst (Hit with cause 'expanded-fuse')."""
    n = int(0.5 * SR)
    t = np.arange(n) / SR
    thump = np.sin(TAU * np.cumsum(70 * (1 + 2.5 * np.exp(-t / 0.04))) / SR) * np.exp(-t / 0.14)
    burst = lp(hp(rng.standard_normal(n), 250), 5000) * np.exp(-t / 0.05) * 0.6
    ring = modal(note(76 + i), 0.5, [(1, 0.5, 0.25), (2.4, 0.3, 0.12), (3.9, 0.15, 0.08)], 0.5, 0.1, rng)
    return finish(seq([(0, thump, 1), (0, burst, 1), (0.01, ring, 0.7)]), 0.5, 0.1)


def zipper_link(i, rng):
    n = int(0.26 * SR)
    t = np.arange(n) / SR
    zip_ = np.sin(TAU * np.cumsum(900 + 2600 * (t / 0.24) ** 1.5) / SR) * (np.sin(TAU * 60 * t) > 0) * 0.15 * (t < 0.24)
    parts = [(0, zip_, 1), (0.22, pizz(note(84 + 2 * i), 0.15, 0.6, rng), 1)]
    return finish(seq(parts, 0.3), 0.3, 0.05)


def hazard_warn(i, rng):
    n = int(0.6 * SR)
    t = np.arange(n) / SR
    wob = np.sin(TAU * np.cumsum((180 + 40 * i) + 120 * (t / 0.6) + 30 * np.sin(TAU * (7 + i) * t)) / SR) * (t / 0.6) * 0.4
    ticks = [(0.1 * k, woodblock(800 + 90 * i + 120 * k, 0.5 + 0.06 * k, 0.05), 1) for k in range(5)]
    return finish(seq([(0, wob, 1)] + ticks, 0.6), 0.6, 0.06)


def hazard_hit(i, rng):
    return finish(seq([(0, impact(i, rng, 'metal', 1.8, 0.4), 1), (0, bell(note(55 + 2 * i), 0.8, 0.5, rng), 1), (0, big_step(i, rng), 0.6)]), 0.7, 0.1)


def mimic_wake(i, rng):
    n = int(0.55 * SR)
    t = np.arange(n) / SR
    creak = bp(rng.standard_normal(n), 250, 1100) * np.sin(TAU * 12 * t) ** 2 * 0.3 * (t < 0.35)
    gasp = bp(rng.standard_normal(int(0.2 * SR)), 800, 3500) * np.exp(-np.arange(int(0.2 * SR)) / (0.08 * SR)) * 0.5
    return finish(seq([(0, creak, 1), (0.34, gasp, 1), (0.36, woodblock(420 + 40 * i, 0.9, 0.1), 1)]), 0.6, 0.08)


def ui_negative(i, rng, kind='locked'):
    if kind == 'locked':
        return finish(seq([(0, woodblock(260 + 30 * i, 0.9, 0.09), 1), (0.07, woodblock(220 + 30 * i, 0.8, 0.09), 1)]), 0.2, 0.04)
    return finish(seq([(0, marimba(note(62 - 2 * i), 0.3, 0.7, rng), 1), (0.09, marimba(note(55 - 2 * i), 0.4, 0.7, rng), 1), (0.09, felt_kick(0.5, 0.15), 0.6)]), 0.5, 0.08)


def hero_pulse(i, rng, element='ember'):
    """K11 hero mechanics (LanternStride ember burst / DreamBreath frost wave): rare (4-7 s cooldown), so it can be big."""
    if element == 'ember':
        base = fire_cast(i, rng)
        body = gong(note(43 + 2 * i), 1.2, 0.45)
        sw = sweep_bp(rng.standard_normal(int(0.5 * SR)), 300, 2200, 1.2) * np.sin(np.pi * np.clip(np.arange(int(0.5 * SR)) / (0.5 * SR), 0, 1)) * 0.3
        return finish(seq([(0, base, 0.9), (0, body, 0.7), (0, sw, 1)]), 1.0, 0.25)
    base = ice_cast(i, rng)
    pad = saw_pad(note(60 + 2 * i), 0.9, 0.25, detune=0.01, cutoff=1800, a=0.15, r=0.5)
    sh = hp(rng.standard_normal(int(0.6 * SR)), 5000) * np.exp(-np.arange(int(0.6 * SR)) / (0.2 * SR)) * 0.12
    return finish(lp(seq([(0, base, 1), (0.0, pad, 1.4), (0.05, sh, 0.6)]), 5000), 1.0, 0.25)
