"""SFX revision 7: every gameplay, interface and shop sound rebuilt in the music's own palette.

Real instrument and percussion one-shots rendered from GeneralUser GS (the soundfont the soundtrack is rendered with, via
FluidSynth) are layered with the procedural foley of sfx_lib (air, paper, cloth, grit). Weapons, swings and skills sit lower
than revision 6 (a body an octave or more down, airy tops rolled off). Each clip is levelled to one loudness, and the cue's
gainDb then sets where it sits in the mix (sfx7_wire.py).

Usage: python sfx7.py <out dir> [cue.id ...]
Env:   AB_FLUIDSYNTH (fluidsynth.exe), AB_SF2 (GeneralUser-GS.sf2), AB_BANK (one-shot cache dir)
"""
import hashlib
import os
import subprocess
import sys
import zlib

import mido
import numpy as np
import pyloudnorm
import soundfile as sf

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sfx_lib import (SR, N, tt, noise, lp, hp, bp, reson, env_exp, env_ar, place, pitch, norm, osc, click, thump,  # noqa: E402
                     whoosh, crackle, flutter, scrape, sizzle, wind, rumble, roar, voiced, breath, creak, finish, drip)

from sfx_lib import Sig  # noqa: E402


def _sig(fn):
    """Layers come back as Sig, so layers of different lengths add with zero padding."""
    def wrapped(*a, **k):
        return np.asarray(fn(*a, **k), dtype=float).view(Sig)
    return wrapped


noise, lp, hp, bp, reson, env_exp, env_ar, pitch, osc, click, thump = map(_sig, (noise, lp, hp, bp, reson, env_exp, env_ar, pitch, osc, click, thump))
whoosh, crackle, flutter, scrape, sizzle, wind, rumble, roar, voiced, breath, creak, drip, tt = map(_sig, (whoosh, crackle, flutter, scrape, sizzle, wind, rumble, roar, voiced, breath, creak, drip, tt))

FS = os.environ.get('AB_FLUIDSYNTH', 'fluidsynth')
SF2 = os.environ.get('AB_SF2', 'GeneralUser-GS.sf2')
BANK = os.environ.get('AB_BANK', 'sfx7bank')
LEVEL = -18.0  # every clip's loudness (LUFS) before its cue's gain

# ---------------------------------------------------------------------------------------------- the soundfont one-shots
_bank = {}


def smp(prog, note, vel=100, dur=1.2, drum=False, kit=0):
    """One note of the soundfont, mono, peak 1. Drums: prog is ignored, kit selects the GS drum set."""
    key = ('d%d' % kit if drum else 'p%d' % prog, note, vel, round(dur, 2))
    if key in _bank:
        return _bank[key]
    os.makedirs(BANK, exist_ok=True)
    name = '%s_%d_%d_%03d' % (key[0], note, vel, int(dur * 100))
    wav = os.path.join(BANK, name + '.wav')
    if not os.path.exists(wav):
        mid = mido.MidiFile(ticks_per_beat=480)
        tr = mido.MidiTrack(); mid.tracks.append(tr)
        tr.append(mido.MetaMessage('set_tempo', tempo=500000, time=0))
        ch = 9 if drum else 0
        tr.append(mido.Message('program_change', channel=ch, program=kit if drum else prog, time=0))
        tr.append(mido.Message('note_on', channel=ch, note=note, velocity=vel, time=0))
        ticks = int(dur * 960)
        tr.append(mido.Message('note_off', channel=ch, note=note, velocity=0, time=ticks))
        tr.append(mido.MetaMessage('end_of_track', time=960))
        path = os.path.join(BANK, name + '.mid'); mid.save(path)
        subprocess.run([FS, '-ni', '-q', '-R', '0', '-C', '0', '-g', '0.6', '-r', str(SR), '-O', 'float', '-T', 'wav', '-F', wav, SF2, path],
                       check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    x, _ = sf.read(wav, dtype='float64')
    if x.ndim > 1:
        x = x.mean(axis=1)
    nz = np.flatnonzero(np.abs(x) > 1e-4)
    x = x[nz[0]:] if len(nz) else x
    x = norm(x).view(Sig)
    _bank[key] = x
    return x


def dk(note, vel=100, dur=.8, kit=0):
    return smp(0, note, vel, dur, True, kit)


# GM programs
CELESTA, GLOCK, MUSICBOX, VIBES, MARIMBA, XYLO, TUBULAR, DULCIMER = 8, 9, 10, 11, 12, 13, 14, 15
PIZZ, HARP, TIMPANI, ORCHHIT, CHOIR, STRINGS, TREM = 45, 46, 47, 55, 52, 48, 44
TUBA, TROMBONE, HORN, BRASS, CONTRABASS, CELLO = 58, 57, 60, 61, 43, 42
TINKLE, AGOGO, STEEL, WOODBLK, TAIKO, MELTOM, REVCYM = 112, 113, 114, 115, 116, 117, 119
KALIMBA, CRYSTAL, FRET, BREATHN = 108, 98, 120, 121
# GM drums
KICK, KICK2, STICK, SNARE, CLAP, SNARE2 = 35, 36, 37, 38, 39, 40
TOM_LO, TOM_LM, TOM_M, TOM_HM, TOM_HI = 41, 45, 47, 48, 50
CRASH, RIDE, RIDEBELL, TAMB, SPLASH, COWBELL, CRASH2 = 49, 51, 53, 54, 55, 56, 57
BONGO_HI, BONGO_LO, CONGA_MUTE, CONGA_HI, CONGA_LO = 60, 61, 62, 63, 64
CABASA, MARACAS, CLAVES, WB_HI, WB_LO, TRI_MUTE, TRI = 69, 70, 75, 76, 77, 80, 81
SHAKER, JINGLE, BELLTREE, CASTANET, SURDO_M, SURDO = 82, 83, 84, 85, 86, 87
ORCH_KIT = 48  # GS orchestra kit: concert bass drum and timpani


def hz(m):
    return 440.0 * 2 ** ((m - 69) / 12)


# ---------------------------------------------------------------------------------------------- mixing helpers
def buf(d):
    return np.zeros(N(d)).view(Sig)


def put(out, x, t=0.0, g=1.0, ratio=1.0, d=None):
    """Mix a layer into out at time t (s), resampled by ratio (<1 lower and longer), cut to d seconds."""
    if ratio != 1.0:
        x = pitch(x, ratio)
    if d is not None:
        x = x[:N(d)] * np.linspace(1, 0, min(len(x), N(d))) ** .5 if len(x) > N(d) else x
    return place(out, x, t, g)


def shape(x, d):
    """A layer faded to fit d seconds."""
    x = x[:N(d)]
    f = min(len(x) // 3, N(.04))
    if f > 0:
        x = x.copy(); x[-f:] *= np.linspace(1, 0, f)
    return np.asarray(x, dtype=float).view(Sig)


def air(d, rng, f0, f1, q=1.2, peak=.45, g=1.0):
    return whoosh(d, rng, f0, f1, q, 'pink', peak) * g


def grit(d, rng, f=2200, rate=90, g=.5):
    return crackle(d, rng, rate, (f * .6, f * 1.6), (.2, 1.0), env_exp(d, d * .35)) * g


def tone(m, d, decay=.35, kind=1.0):
    """A soft sine-plus-octave body note (for a low weight under a hit)."""
    t = tt(d)
    return (np.sin(2 * np.pi * hz(m) * t) + .3 * kind * np.sin(4 * np.pi * hz(m) * t)) * env_exp(d, decay, .003)


def J(rng, a=.97, b=1.03):
    return float(rng.uniform(a, b))


# ---------------------------------------------------------------------------------------------- recipes
R = {}


def cue(name, dur, count=3, wet=.1, lpf=11000):
    def deco(fn):
        R[name] = (fn, dur, count, wet, lpf)
        return fn
    return deco


# --- swings (lower, heavier air; a material accent)
def swing(d, rng, f0, f1, body=None, accent=None, ag=.5):
    o = buf(d)
    put(o, air(d * .9, rng, f0 * J(rng), f1 * J(rng), 1.4, .45), 0)
    put(o, air(d * .7, rng, f0 * .5, f1 * .45, 1.0, .5, .7), .01)
    if body is not None:
        put(o, body, d * .25, .5)
    if accent is not None:
        put(o, accent, d * .35, ag)
    return o


@cue('cmb.swing.blade', .26, 4)
def _(rng, d):
    ring = bp(noise(.18, rng), 1700 * J(rng), 9) * env_exp(.18, .05)
    return swing(d, rng, 260, 1200, accent=ring, ag=.35)


@cue('cmb.swing.spear', .24, 4)
def _(rng, d):
    return swing(d, rng, 300, 1400, accent=dk(WB_LO, 70) * .5, ag=.4)


@cue('cmb.swing.hammer', .34, 4)
def _(rng, d):
    return swing(d, rng, 140, 650, body=pitch(dk(TOM_LO, 110), .8 * J(rng)), accent=None)


@cue('cmb.swing.scissors', .2, 4)
def _(rng, d):
    o = swing(d, rng, 380, 1500)
    put(o, dk(CLAVES, 90), .04, .55, .72 * J(rng)); put(o, dk(CLAVES, 80), .1, .4, .62 * J(rng))
    return o


@cue('cmb.swing.umbrella', .3, 4)
def _(rng, d):
    o = swing(d, rng, 180, 800)
    put(o, flutter(.18, rng, 55, (500, 1400)), .05, .7)
    return o


@cue('cmb.swing.whip', .24, 4)
def _(rng, d):
    o = swing(d, rng, 300, 1700)
    put(o, dk(SNARE2, 110), .14, .55, .78 * J(rng)); put(o, hp(noise(.05, rng), 1800) * env_exp(.05, .012), .14, .6)
    return o


@cue('cmb.windup', .34, 3)
def _(rng, d):
    o = buf(d)
    put(o, air(d, rng, 120, 700, 1.3, .9), 0)
    put(o, pitch(smp(TREM, 40, 70, .5), J(rng, .99, 1.01)) * env_ar(N(.45) / SR, .25, .1)[:N(.45)] if False else shape(smp(TREM, 40, 70, .5), d), 0, .25)
    return o


@cue('cmb.miss', .2, 4)
def _(rng, d):
    return air(d, rng, 220, 900, 1.0, .4, .8)


# --- impacts by material: a low body from the kit (pitched down) and the material on top
def hit(d, rng, body, top, bg=1.0, tg=.6, low=None):
    o = buf(d)
    put(o, body, 0, bg, J(rng, .92, 1.0))
    if low is not None:
        put(o, tone(low, d, .07), 0, .45)
    put(o, top, .002, tg)
    return o


@cue('cmb.hit.paper', .22, 4)
def _(rng, d):
    return hit(d, rng, pitch(dk(KICK, 100), .85), grit(.18, rng, 2400, 160, .7), low=40)


@cue('cmb.hit.metal', .4, 6)
def _(rng, d):
    ring = shape(pitch(smp(TUBULAR, 55, 80, .6), J(rng, .96, 1.04)), .35)
    return hit(d, rng, pitch(dk(TOM_LM, 110), .8), ring, tg=.3, low=38)


@cue('cmb.hit.wax', .26, 6)
def _(rng, d):
    sq = lp(noise(.16, rng, 'brown'), 900) * env_exp(.16, .05) * 3
    return hit(d, rng, pitch(dk(KICK2, 105), .8), sq, tg=.6, low=38)


@cue('cmb.hit.fabric', .22, 4)
def _(rng, d):
    return hit(d, rng, pitch(dk(CONGA_LO, 100), .8), flutter(.12, rng, 70, (600, 1800)), tg=.5, low=40)


@cue('cmb.hit.glass', .38, 6)
def _(rng, d):
    sh = shape(pitch(smp(CELESTA, 84 + int(rng.integers(0, 5)), 70, .4), 1.0), .3) * .5 + grit(.25, rng, 5200, 260, .5)
    return hit(d, rng, pitch(dk(KICK, 95), .9), sh, tg=.55, low=41)


@cue('cmb.hit.wood', .22, 6)
def _(rng, d):
    return hit(d, rng, pitch(dk(TOM_M, 105), .78), pitch(dk(WB_LO, 100), .7 * J(rng)), tg=.6, low=40)


def die(d, rng, material):
    o = buf(d)
    put(o, pitch(dk(KICK, 120), .78), 0, .9)
    put(o, tone(33, d, .12), 0, .5)
    put(o, air(d * .8, rng, 200, 900, 1.0, .25, .45), .02)  # the poof
    put(o, material, .03, .6)
    return o


@cue('enemy.die.paper', .5, 4)
def _(rng, d):
    return die(d, rng, grit(.4, rng, 2200, 220, .8))


@cue('enemy.die.metal', .5, 4)
def _(rng, d):
    return die(d, rng, shape(pitch(smp(TUBULAR, 50, 90, .6), J(rng)), .45) * .6 + pitch(dk(COWBELL, 90), .6) * .3)


@cue('enemy.die.wax', .5, 4)
def _(rng, d):
    return die(d, rng, lp(noise(.4, rng, 'brown'), 700) * env_exp(.4, .12) * 3)


@cue('enemy.die.fabric', .5, 4)
def _(rng, d):
    return die(d, rng, flutter(.35, rng, 60, (500, 1500)))


@cue('enemy.die.glass', .55, 4)
def _(rng, d):
    o = die(d, rng, grit(.45, rng, 5000, 300, .7))
    for k in range(3):
        put(o, shape(smp(CELESTA, 79 + 3 * k + int(rng.integers(0, 3)), 60, .4), .35), .03 + .04 * k, .25)
    return o


@cue('enemy.die.wood', .5, 4)
def _(rng, d):
    o = die(d, rng, pitch(dk(WB_LO, 110), .65))
    put(o, pitch(dk(WB_HI, 80), .7), .07, .4)
    return o


@cue('cmb.crit', .75, 3)
def _(rng, d):
    o = buf(d)
    put(o, pitch(smp(ORCHHIT, 48, 110, .6), .84 * J(rng, .99, 1.01)), 0, .8)
    put(o, pitch(dk(KICK, 127), .75), 0, 1.0)
    put(o, shape(pitch(dk(CRASH, 90), .8), .6), .01, .3)
    put(o, tone(31, d, .2), 0, .5)
    return o


# --- weapons
@cue('wpn.bow.draw', .32, 3)
def _(rng, d):
    o = buf(d)
    put(o, creak(d, rng, (14, 26), ((180, 9), (420, 10), (900, 11))), 0, .6)
    put(o, shape(pitch(smp(FRET, 40, 60, .4), .8), d), 0, .3)
    return o


@cue('wpn.bow.release', .32, 5)
def _(rng, d):
    o = buf(d)
    put(o, shape(smp(PIZZ, 31 + int(rng.integers(0, 3)), 115, .4), .3), 0, .8)
    put(o, air(.2, rng, 500, 1800, 1.6, .2, .6), .01)
    put(o, tone(43, .2, .05), 0, .35)
    return o


@cue('wpn.gun.pop', .2, 4)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(BONGO_LO, 115), .62 * J(rng)), 0, .8)
    put(o, pitch(dk(KICK2, 100), .9), 0, .6)
    put(o, air(.14, rng, 400, 1300, 1.2, .15, .5), .005)
    return o


@cue('wpn.launcher', .3, 4)
def _(rng, d):
    o = buf(d)
    put(o, pitch(smp(TAIKO, 36, 115, .4), .82 * J(rng)), 0, .9)
    put(o, air(.26, rng, 220, 900, 1.2, .3, .7), .01)
    return o


@cue('wpn.cast.fire', 1.1, 4)
def _(rng, d):
    o = buf(d)
    put(o, air(d, rng, 150, 900, .9, .35, .9), 0)
    put(o, grit(d * .8, rng, 1800, 120, .55), .05)
    put(o, shape(smp(TIMPANI, 38 + int(rng.integers(0, 3)), 100, .9), d), 0, .6)
    put(o, roar(d * .6, rng, 380) * env_exp(d * .6, .2), .02, .25)
    return o


@cue('wpn.cast.ice', 1.1, 4)
def _(rng, d):
    o = buf(d)
    for k, m in enumerate((62, 69, 74, 78)):
        put(o, shape(smp(CELESTA, m - 12 + int(rng.integers(0, 2)), 70, .8), .8), .03 * k, .3)
    put(o, wind(d, rng, 420, .5, .5), 0, .5)
    put(o, shape(smp(TIMPANI, 38, 80, .8), d), 0, .4)
    return o


@cue('wpn.cast.arcane', 1.1, 4)
def _(rng, d):
    o = buf(d)
    put(o, shape(pitch(smp(REVCYM, 60, 80, 1.0), .8), .7), 0, .35)
    for k, m in enumerate((50, 57, 62)):
        put(o, shape(smp(VIBES, m + int(rng.integers(0, 2)), 85, 1.0), d), .3 + .05 * k, .4)
    put(o, tone(38, d, .4), .3, .35)
    return o


@cue('wpn.boomerang.fly', .32, 3)
def _(rng, d):
    o = buf(d)
    a = air(d, rng, 200, 800, 1.3, .5)
    put(o, a * (.55 + .45 * np.sin(2 * np.pi * 14 * tt(d))), 0)
    return o


@cue('wpn.boomerang.return', .45, 3)
def _(rng, d):
    o = buf(d)
    a = air(d * .8, rng, 260, 900, 1.3, .7)
    put(o, a * (.55 + .45 * np.sin(2 * np.pi * 16 * tt(d * .8))), 0)
    put(o, pitch(dk(WB_LO, 100), .75), d * .78, .6)
    return o


@cue('wpn.pierce', .16, 4)
def _(rng, d):
    o = buf(d)
    put(o, air(d, rng, 600, 2400, 2.2, .3, .7), 0)
    put(o, pitch(dk(STICK, 90), .7), .03, .4)
    return o


@cue('wpn.split', .24, 4)
def _(rng, d):
    o = buf(d)
    for k in range(3):
        put(o, pitch(dk(BONGO_HI, 90), (.65 + .08 * k) * J(rng)), .03 * k, .5)
    put(o, air(.2, rng, 300, 1200, 1.2, .2, .5), 0)
    return o


@cue('wpn.bounce', .18, 4)
def _(rng, d):
    o = buf(d)
    t = tt(d)
    f = 150 * J(rng) * (1 + .6 * np.exp(-t / .03))
    put(o, np.sin(2 * np.pi * np.cumsum(f) / SR) * env_exp(d, .06), 0, .6)
    put(o, pitch(dk(CONGA_MUTE, 95), .75), 0, .4)
    return o


@cue('wpn.yoyo.throw', .32, 3)
def _(rng, d):
    o = buf(d)
    put(o, air(d, rng, 260, 1000, 1.6, .3) * (.6 + .4 * np.sin(2 * np.pi * 22 * tt(d))), 0)
    put(o, pitch(dk(WB_HI, 80), .7), 0, .3)
    return o


@cue('wpn.yoyo.return', .32, 3)
def _(rng, d):
    o = buf(d)
    put(o, air(d, rng, 300, 900, 1.6, .7) * (.6 + .4 * np.sin(2 * np.pi * 26 * tt(d))), 0)
    put(o, pitch(dk(WB_LO, 90), .7), d * .85, .45)
    return o


@cue('wpn.magnet.pull', .5, 3)
def _(rng, d):
    o = buf(d)
    t = tt(d)
    f = np.linspace(70, 140, len(t)) * J(rng)
    hum = np.sin(2 * np.pi * np.cumsum(f) / SR) + .3 * np.sin(4 * np.pi * np.cumsum(f) / SR)
    put(o, hum * env_ar(d, .15, .15) * .5, 0)
    put(o, shape(smp(VIBES, 45, 70, .5), d), .1, .25)
    return o


@cue('wpn.needle.volley', .28, 4)
def _(rng, d):
    o = buf(d)
    for k in range(5):
        put(o, pitch(dk(STICK, 70), (.75 + .05 * rng.random())), .035 * k + rng.uniform(0, .01), .4)
        put(o, air(.06, rng, 900, 2600, 2.5, .2, .3), .035 * k)
    return o


@cue('wpn.spring.slam', .4, 3)
def _(rng, d):
    o = buf(d)
    t = tt(.25)
    f = 120 * (1 + .9 * np.exp(-t / .04)) * (1 + .15 * np.sin(2 * np.pi * 30 * t))
    put(o, np.sin(2 * np.pi * np.cumsum(f) / SR) * env_exp(.25, .09), 0, .45)
    put(o, pitch(dk(KICK, 120), .78), .02, .9)
    put(o, tone(33, d, .12), .02, .4)
    return o


@cue('wpn.projector.cast', .5, 3)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(CASTANET, 90), .7), 0, .4)
    put(o, flutter(.3, rng, 24, (900, 1800)), .02, .4)
    put(o, shape(smp(VIBES, 55, 80, .5), .45), .08, .35)
    return o


@cue('wpn.ribbon.cut', .18, 4)
def _(rng, d):
    o = buf(d)
    put(o, air(d, rng, 500, 2000, 2.0, .3, .6), 0)
    put(o, pitch(dk(CLAVES, 80), .66 * J(rng)), .05, .4)
    return o


# --- signatures and hero skills (fuller, lower)
@cue('sig.bell', 1.4, 3)
def _(rng, d):
    o = buf(d)
    put(o, shape(smp(TUBULAR, 50 + int(rng.integers(0, 3)), 105, 1.4), d), 0, .7)
    put(o, shape(smp(TUBULAR, 62, 70, 1.2), d), .01, .25)
    put(o, tone(38, d, .5), 0, .3)
    return o


@cue('sig.umbrella', .34, 3)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(BONGO_LO, 100), .7), 0, .6)
    put(o, flutter(.28, rng, 60, (500, 1500)), .01, .7)
    put(o, air(.3, rng, 200, 700, 1.0, .3, .5), 0)
    return o


@cue('sig.stamp', .32, 3)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(WB_LO, 120), .62), 0, .8)
    put(o, pitch(dk(KICK, 115), .8), 0, .8)
    put(o, tone(36, d, .08), 0, .4)
    return o


@cue('sig.scissors', .26, 4)
def _(rng, d):
    o = buf(d)
    for k in range(2):
        put(o, pitch(dk(CLAVES, 100), .62 * J(rng)), .02 + .09 * k, .6)
        put(o, scrape(.07, rng, 1600, 2.0), .02 + .09 * k, .35)
    return o


@cue('sig.camera', .34, 3)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(CASTANET, 110), .7), 0, .6)
    put(o, pitch(dk(STICK, 100), .7), .06, .5)
    t = tt(.25)
    put(o, np.sin(2 * np.pi * np.cumsum(np.linspace(900, 1500, len(t))) / SR) * env_ar(.25, .1, .1) * .12, .08)
    return o


@cue('sig.musicbox', .9, 3)
def _(rng, d):
    o = buf(d)
    seq = [(0, 62), (.1, 66), (.2, 69), (.3, 74)] if rng.random() < .5 else [(0, 74), (.1, 69), (.2, 66), (.3, 62)]
    for t0, m in seq:
        put(o, shape(smp(MUSICBOX, m - 12, 90, .6), .55), t0, .5)
    return o


@cue('sig.tea', .36, 3)
def _(rng, d):
    o = buf(d)
    put(o, bp(noise(.3, rng), 900, 2) * env_ar(.3, .1, .15) * .8, 0)
    put(o, shape(smp(GLOCK, 72 + int(rng.integers(0, 3)), 60, .3), .25), .2, .25)
    return o


@cue('sig.fish', .24, 3)
def _(rng, d):
    o = buf(d)
    put(o, lp(noise(.2, rng), 1600) * env_exp(.2, .05) * 1.5, 0)
    put(o, pitch(dk(CONGA_MUTE, 100), .7), 0, .6)
    put(o, drip(rng, 700, .08), .05, .5)
    return o


@cue('sig.alarm', .4, 2)
def _(rng, d):
    o = buf(d)
    for k in range(4):
        put(o, shape(smp(TUBULAR, 57, 80, .2), .12), .08 * k, .35)
    return o


@cue('sig.iron', .4, 3)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(COWBELL, 100), .55), 0, .5)
    put(o, sizzle(.35, rng, 700) * env_exp(.35, .14), .02, .4)
    put(o, tone(40, d, .1), 0, .4)
    return o


@cue('sig.hammer', .55, 3)
def _(rng, d):
    o = buf(d)
    put(o, shape(pitch(smp(TUBULAR, 48, 115, .6), .9), .5), 0, .55)
    put(o, pitch(dk(KICK, 125), .75), 0, 1.0)
    put(o, tone(31, d, .15), 0, .5)
    return o


@cue('sig.alarm.boom', .6, 3)
def _(rng, d):
    o = buf(d)
    put(o, shape(smp(TAIKO, 35, 127, .6), d), 0, .9)
    put(o, shape(smp(TIMPANI, 33, 120, .6), d), 0, .6)
    put(o, lp(noise(d, rng, 'brown'), 400) * env_exp(d, .2) * 3, 0, .6)
    return o


def skill(d, rng, notes, prog=VIBES, low=36, swell=True, top=None):
    """A hero skill: a low body note, a short chord in the instrument, a swell of air."""
    o = buf(d)
    put(o, tone(low, d, d * .4), 0, .45)
    put(o, shape(smp(TIMPANI, low + 12, 95, d), d), 0, .4)
    for k, m in enumerate(notes):
        put(o, shape(smp(prog, m, 85, d), d), .03 * k, .35)
    if swell:
        put(o, air(d, rng, 160, 700, 1.0, .3, .5), 0)
    if top is not None:
        put(o, top, .02, .5)
    return o


@cue('hero.pulse.ember', 1.0, 2)
def _(rng, d):
    return skill(d, rng, (50, 57), HARP, 38, top=grit(.7, rng, 1600, 110, .5))


@cue('hero.pulse.frost', 1.0, 2)
def _(rng, d):
    return skill(d, rng, (57, 64, 69), CELESTA, 33, top=wind(.8, rng, 500, .5, .5) * .5)


@cue('hero.lantern.stride', .65, 2)
def _(rng, d):
    return skill(d, rng, (45, 52, 57), VIBES, 33)


@cue('hero.parcel.express', .55, 2)
def _(rng, d):
    o = skill(d, rng, (55, 59), MARIMBA, 35)
    put(o, pitch(dk(WB_LO, 110), .7), 0, .5)
    return o


@cue('hero.dream.breath', .8, 2)
def _(rng, d):
    return skill(d, rng, (57, 61, 64), CHOIR, 33, top=breath(.7, rng) * .4)


@cue('hero.rooted.oracle', .8, 2)
def _(rng, d):
    return skill(d, rng, (45, 52), MARIMBA, 31, top=creak(.6, rng) * .3)


@cue('hero.solo.maw', .55, 3)
def _(rng, d):
    o = skill(d, rng, (43, 46), BRASS, 31)
    put(o, roar(.4, rng, 300) * env_exp(.4, .15), 0, .3)
    return o


@cue('hero.glass.gambler', .55, 2)
def _(rng, d):
    o = skill(d, rng, (64, 71), CELESTA, 40)
    for k in range(4):
        put(o, pitch(dk(CASTANET, 80), .8), .04 * k, .3)
    return o


@cue('hero.tide.collector', .7, 2)
def _(rng, d):
    o = skill(d, rng, (50, 57), HARP, 33)
    put(o, lp(noise(.6, rng, 'pink'), 1200) * env_ar(.6, .2, .3), 0, .6)
    return o


@cue('hero.clock.runner', .55, 2)
def _(rng, d):
    o = skill(d, rng, (52, 59), MARIMBA, 35, swell=False)
    for k in range(4):
        put(o, pitch(dk(WB_HI, 90), .7 if k % 2 else .78), .1 * k, .4)
    return o


@cue('hero.mirror.guard', .55, 2)
def _(rng, d):
    return skill(d, rng, (59, 66, 71), CELESTA, 35, top=shape(pitch(smp(REVCYM, 60, 70, .5), 1.0), .4) * .3)


@cue('hero.treasure.tax', .45, 2)
def _(rng, d):
    o = buf(d)
    for k in range(3):
        put(o, shape(smp(GLOCK, 69 + 3 * k, 75, .3), .3), .05 * k, .35)
    put(o, pitch(dk(TAMB, 80), .8), 0, .3)
    return o


@cue('hero.charge.ready', .32, 2)
def _(rng, d):
    o = buf(d)
    put(o, shape(smp(VIBES, 62, 80, .3), .3), 0, .4); put(o, shape(smp(VIBES, 69, 75, .3), .25), .07, .35)
    return o


# --- reactions and fields
@cue('fx.burn', .4, 4)
def _(rng, d):
    return grit(d, rng, 1500, 120, .6) + air(d, rng, 150, 500, .9, .4, .4)


@cue('fx.chill', .6, 4)
def _(rng, d):
    o = buf(d)
    put(o, wind(d, rng, 600, .5, .6), 0, .5)
    put(o, shape(smp(CELESTA, 76 + int(rng.integers(0, 4)), 50, .5), .5), .05, .25)
    return o


@cue('fx.knockback', .28, 4)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(KICK2, 110), .78), 0, .8)
    put(o, air(.24, rng, 180, 700, 1.1, .25, .6), .01)
    return o


@cue('fx.reaction.steam', .5, 2)
def _(rng, d):
    return hp(noise(d, rng), 1500) * env_ar(d, .05, .3) * .6 + air(d, rng, 300, 1100, 1.0, .2, .4)


@cue('fx.reaction.shatter', .6, 2)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(KICK, 115), .85), 0, .7)
    put(o, grit(.5, rng, 5000, 320, .7), .01)
    for k in range(5):
        put(o, shape(smp(CELESTA, 77 + 2 * k, 60, .4), .35), .02 + .03 * k, .2)
    return o


@cue('fx.reaction.arcane', 1.0, 2)
def _(rng, d):
    o = buf(d)
    put(o, shape(pitch(smp(REVCYM, 60, 75, 1.0), .85), .6), 0, .35)
    put(o, shape(smp(VIBES, 50, 90, 1.0), d), .45, .4); put(o, shape(smp(VIBES, 57, 85, 1.0), d), .48, .3)
    put(o, tone(38, d, .3), .45, .35)
    return o


@cue('fx.field.zipper', .26, 3)
def _(rng, d):
    return flutter(d, rng, 80, (1200, 2600)) * .7 + air(d, rng, 400, 1300, 1.4, .5, .3)


@cue('fx.hazard.warn', .7, 2)
def _(rng, d):
    o = buf(d)
    put(o, shape(smp(HORN, 50, 95, .3), .3), 0, .5); put(o, shape(smp(HORN, 50, 95, .3), .3), .32, .5)
    put(o, tone(38, .3, .15), 0, .3); put(o, tone(38, .3, .15), .32, .3)
    return o


@cue('fx.hazard.hit', .3, 3)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(KICK, 120), .78), 0, .9)
    put(o, grit(.25, rng, 1800, 140, .5), .01)
    return o


# --- the hero's and friends' bodies
@cue('chr.step', .07, 6, wet=.05)
def _(rng, d):
    return lp(pitch(dk(WB_LO, 50), .55 * J(rng, .95, 1.05)), 1400)[:N(d)] * .7


@cue('chr.dash', .26, 4)
def _(rng, d):
    return air(d, rng, 250, 1100, 1.2, .3)


@cue('chr.jump', .18, 3)
def _(rng, d):
    return flutter(d, rng, 60, (500, 1400)) * .5 + air(d, rng, 300, 1000, 1.2, .3, .5)


@cue('chr.land', .18, 3)
def _(rng, d):
    o = buf(d)
    put(o, lp(pitch(dk(KICK2, 70), .8), 600), 0, .6)
    put(o, flutter(.1, rng, 70, (400, 1200)), .005, .3)
    return o


@cue('plr.dodge', .22, 3)
def _(rng, d):
    return air(d, rng, 350, 1500, 1.4, .3)


@cue('plr.hurt', .38, 5)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(KICK, 120), .8), 0, .9)
    put(o, shape(smp(PIZZ, 40 - int(rng.integers(0, 3)), 110, .3), .25), 0, .6)
    put(o, lp(noise(.12, rng), 2500) * env_exp(.12, .03), 0, .5)
    return o


@cue('plr.lowhp', .55, 2)
def _(rng, d):
    o = buf(d)
    put(o, lp(pitch(dk(KICK, 100), .75), 300), 0, .9); put(o, lp(pitch(dk(KICK, 80), .75), 300), .2, .7)
    return o


@cue('plr.heal', 1.4, 3)
def _(rng, d):
    o = buf(d)
    for k, m in enumerate((62, 66, 69, 74, 78)):
        put(o, shape(smp(HARP, m - 12, 80, 1.0), 1.0), .06 * k, .35)
    put(o, shape(smp(CHOIR, 50, 60, 1.2), d), .1, .2)
    return o


@cue('plr.death', 1.3, 2)
def _(rng, d):
    o = buf(d)
    for k, m in enumerate((50, 46, 43, 38)):
        put(o, shape(smp(TIMPANI, m - 12, 110 - 10 * k, .5), .5), .22 * k, .6)
    put(o, shape(smp(CELLO, 38, 90, 1.2), d), .1, .35)
    return o


# --- enemies (creaturely, from voiced formants, pitched low)
@cue('enemy.spit.windup', .28, 4)
def _(rng, d):
    return voiced(d, 110 * J(rng, .9, 1.1), rng, ((350, 5), (900, 6), (2200, 7)), .4) * env_ar(d, .2, .05) * .7


@cue('enemy.spit.fire', .16, 5)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(BONGO_LO, 100), .6 * J(rng)), 0, .7)
    put(o, drip(rng, 500, .08), .01, .5)
    return o


@cue('enemy.lunge', .24, 4)
def _(rng, d):
    o = buf(d)
    put(o, air(d, rng, 200, 800, 1.2, .3, .7), 0)
    put(o, voiced(.18, 95 * J(rng, .9, 1.1), rng, ((300, 5), (800, 6), (2000, 7)), .5) * env_exp(.18, .08), 0, .5)
    return o


@cue('enemy.hurt', .2, 6)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(CONGA_MUTE, 95), .7 * J(rng)), 0, .6)
    put(o, voiced(.12, 160 * J(rng, .85, 1.15), rng, ((400, 5), (1000, 6), (2400, 7)), .3) * env_exp(.12, .05), 0, .35)
    return o


@cue('enemy.big.step', .34, 3)
def _(rng, d):
    o = buf(d)
    put(o, shape(smp(TAIKO, 33, 110, .4), d), 0, .9)
    put(o, tone(28, d, .15), 0, .5)
    return o


@cue('enemy.big.roar', .9, 3)
def _(rng, d):
    o = buf(d)
    put(o, roar(d, rng, 260 * J(rng, .9, 1.1)) * env_ar(d, .1, .5), 0, .7)
    put(o, shape(smp(TUBA, 34 + int(rng.integers(0, 3)), 110, .8), d), .02, .5)
    return o


@cue('enemy.die.large', .8, 3)
def _(rng, d):
    o = buf(d)
    put(o, shape(smp(TAIKO, 33, 127, .7), d), 0, .9)
    put(o, shape(smp(TIMPANI, 31, 110, .7), d), 0, .5)
    put(o, grit(.6, rng, 1200, 140, .5), .02)
    put(o, air(.7, rng, 150, 600, .9, .2, .4), .01)
    return o


@cue('enemy.mimic.wake', .7, 2)
def _(rng, d):
    o = buf(d)
    put(o, creak(.45, rng, (10, 20), ((150, 9), (380, 10), (800, 11))), 0, .6)
    put(o, pitch(dk(WB_LO, 120), .55), .48, .8); put(o, pitch(dk(KICK, 100), .8), .48, .5)
    return o


def blip(d, rng, f0, f1, g=.5):
    t = tt(d); f = np.linspace(f0, f1, len(t)) * J(rng)
    return np.sin(2 * np.pi * np.cumsum(f) / SR) * env_exp(d, d * .4) * g


@cue('enemy.shot.arc', .12, 3)
def _(rng, d):
    return blip(d, rng, 420, 260) + lp(noise(d, rng), 1200) * env_exp(d, .03) * .4


@cue('enemy.shot.spiral', .12, 3)
def _(rng, d):
    return blip(d, rng, 330, 460) + lp(noise(d, rng), 1200) * env_exp(d, .03) * .4


@cue('enemy.shot.burst', .12, 3)
def _(rng, d):
    return blip(d, rng, 500, 220) + pitch(dk(BONGO_HI, 80), .7)[:N(d)] * .4


# --- loot and upgrades (toy-shop chimes, an octave lower than before)
def chime(d, notes, prog=GLOCK, vel=85, gap=.06, g=.4, oct=-12):
    o = buf(d)
    for k, m in enumerate(notes):
        put(o, shape(smp(prog, m + oct, vel, max(.3, d - gap * k)), d), gap * k, g)
    return o


@cue('loot.coin.drop', .3, 4)
def _(rng, d):
    o = buf(d)
    put(o, shape(smp(GLOCK, 81 + int(rng.integers(0, 3)), 70, .3), .25), 0, .35)
    put(o, pitch(dk(TRI_MUTE, 70), .8), 0, .3)
    return o


@cue('loot.coin.bounce', .2, 5)
def _(rng, d):
    return shape(pitch(dk(TRI_MUTE, 60), .9 * J(rng)), d) * .5


@cue('loot.coin.vacuum', .4, 3)
def _(rng, d):
    return air(d, rng, 600, 2400, 1.8, .8, .5)


@cue('loot.coin.pickup', .35, 4)
def _(rng, d):
    base = 74 + int(rng.integers(0, 3))
    return chime(d, (base, base + 5), GLOCK, 80, .045, .4)


@cue('loot.egg', .6, 3)
def _(rng, d):
    return chime(d, (66, 71, 74), MARIMBA, 85, .06, .45, 0)


@cue('loot.heart', 1.2, 3)
def _(rng, d):
    o = chime(d, (62, 66, 69, 74), HARP, 85, .07, .4, 0)
    put(o, shape(smp(CHOIR, 62, 60, 1.0), d), .05, .2)
    return o


@cue('loot.chest.appear', 1.4, 2)
def _(rng, d):
    o = buf(d)
    put(o, shape(pitch(smp(REVCYM, 60, 75, 1.0), .9), .8), 0, .3)
    put(o, shape(smp(TIMPANI, 38, 100, .8), .8), .7, .6)
    put(o, chime(.7, (62, 69, 74), CELESTA, 80, .05, .4, 0), .72)
    return o


@cue('loot.chest.open', 1.8, 3)
def _(rng, d):
    o = buf(d)
    put(o, creak(.4, rng, (12, 22), ((200, 9), (480, 10), (1000, 11))), 0, .5)
    put(o, pitch(dk(WB_LO, 110), .6), .38, .6)
    put(o, chime(1.3, (62, 66, 69, 74, 78), CELESTA, 85, .06, .35, 0), .42)
    put(o, shape(smp(STRINGS, 50, 60, 1.2), 1.2), .42, .2)
    return o


@cue('up.merge', .8, 3)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(KICK, 100), .85), 0, .5)
    put(o, chime(.75, (57, 62, 66, 69), VIBES, 85, .05, .4, 0), 0)
    return o


@cue('up.merge.gather', .55, 3)
def _(rng, d):
    return air(d, rng, 300, 1500, 1.5, .9, .7) + shape(pitch(smp(REVCYM, 60, 60, .6), 1.0), d) * .3


@cue('up.star2', 1.7, 2)
def _(rng, d):
    return chime(d, (62, 69, 74, 78), CELESTA, 90, .08, .45, 0) + shape(smp(TUBULAR, 50, 80, 1.6), d) * .25


@cue('up.star3', 1.8, 2)
def _(rng, d):
    o = chime(d, (62, 66, 69, 74, 78, 81), CELESTA, 95, .07, .45, 0)
    put(o, shape(smp(TUBULAR, 50, 95, 1.7), d), 0, .3); put(o, shape(smp(CHOIR, 62, 70, 1.6), d), .1, .2)
    return o


@cue('up.ally.land', .9, 4)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(KICK2, 100), .8), 0, .6)
    put(o, flutter(.2, rng, 60, (500, 1300)), 0, .3)
    put(o, chime(.8, (62, 69), MARIMBA, 80, .07, .4, 0), .03)
    return o


@cue('up.bond.t1', 1.4, 2)
def _(rng, d):
    return chime(d, (57, 62, 66, 69), HARP, 90, .07, .4, 0) + shape(smp(STRINGS, 50, 70, 1.3), d) * .25


@cue('up.bond.t2', 1.4, 2)
def _(rng, d):
    o = chime(d, (57, 62, 66, 69, 74), HARP, 95, .06, .4, 0)
    put(o, shape(smp(BRASS, 50, 80, 1.2), d), .05, .25)
    return o


@cue('up.chain', .8, 4)
def _(rng, d):
    o = buf(d)
    for k in range(3):
        put(o, pitch(dk(TRI_MUTE, 70), .75 + .1 * k), .05 * k, .3)
    put(o, chime(.6, (66, 73), VIBES, 80, .06, .35, 0), .12)
    return o


@cue('up.rarity.blue', 1.0, 2)
def _(rng, d):
    return chime(d, (62, 69, 74), CELESTA, 85, .06, .45, 0)


@cue('up.rarity.purple', 1.4, 2)
def _(rng, d):
    return chime(d, (61, 66, 70, 73), VIBES, 90, .07, .4, 0) + shape(pitch(smp(REVCYM, 60, 60, .8), 1), .6) * .2


@cue('up.rarity.gold', 1.9, 2)
def _(rng, d):
    o = chime(d, (62, 66, 69, 74, 78, 81), GLOCK, 95, .06, .4, -12)
    put(o, shape(smp(BRASS, 62, 90, 1.5), d), .1, .3); put(o, shape(smp(TUBULAR, 50, 90, 1.6), d), .1, .25)
    return o


# --- the shop
@cue('shop.keeper.enter.gold', 1.3, 2)
def _(rng, d):
    o = chime(d, (62, 66, 69, 74), MARIMBA, 85, .08, .4, 0)
    put(o, shape(smp(JINGLE, 0, 80, .6), .5) if False else pitch(dk(JINGLE, 70), .9), 0, .3)
    return o


@cue('shop.keeper.enter.dream', 1.3, 2)
def _(rng, d):
    return chime(d, (61, 64, 68, 73), CELESTA, 80, .1, .4, 0) + shape(smp(CHOIR, 61, 55, 1.2), d) * .2


@cue('shop.keeper.exit.gold', 1.3, 2)
def _(rng, d):
    return chime(d, (74, 69, 66, 62), MARIMBA, 80, .08, .4, 0)


@cue('shop.keeper.exit.dream', 1.3, 2)
def _(rng, d):
    return chime(d, (73, 68, 64, 61), CELESTA, 75, .1, .4, 0)


@cue('shop.carpet.open', .55, 2)
def _(rng, d):
    return flutter(d, rng, 45, (400, 1200)) * .7 + air(d, rng, 200, 800, 1.0, .4, .5)


@cue('shop.carpet.roll', .55, 2)
def _(rng, d):
    return lp(noise(d, rng, 'pink'), 900) * env_ar(d, .1, .3) * .8 + pitch(dk(CONGA_LO, 70), .8)[:N(d)] * .3


@cue('shop.stand.appear', .6, 4)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(WB_LO, 100), .7 * J(rng)), 0, .5)
    put(o, chime(.5, (69, 74), GLOCK, 70, .05, .3), .03)
    return o


@cue('shop.item.pop', .12, 4)
def _(rng, d):
    return pitch(dk(BONGO_HI, 90), .8 * J(rng))[:N(d)] * .6


@cue('shop.approach', .06, 4, wet=.05)
def _(rng, d):
    return pitch(dk(WB_HI, 50), .8)[:N(d)] * .5


@cue('shop.focus', .06, 4, wet=.05)
def _(rng, d):
    return shape(smp(MARIMBA, 74, 50, .1), d) * .5


@cue('shop.buy.gold', 1.5, 4)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(TAMB, 90), .9), 0, .3)
    put(o, chime(1.4, (62, 66, 69, 74, 78), GLOCK, 90, .045, .38), 0)
    put(o, pitch(dk(KICK, 90), .85), 0, .4)
    return o


@cue('shop.buy.dream', 1.8, 3)
def _(rng, d):
    o = chime(d, (61, 66, 69, 73, 78), CELESTA, 85, .06, .4, 0)
    put(o, shape(smp(CHOIR, 61, 60, 1.5), d), .05, .2)
    return o


def deny(d, rng, prog=MARIMBA):
    o = buf(d)
    put(o, shape(smp(prog, 50, 85, .25), .2), 0, .45); put(o, shape(smp(prog, 47, 85, .25), .2), .11, .45)
    put(o, pitch(dk(WB_LO, 70), .7), 0, .3)
    return o


@cue('shop.deny.gold', .35, 2)
def _(rng, d):
    return deny(d, rng)


@cue('shop.deny.egg', .35, 2)
def _(rng, d):
    return deny(d, rng, VIBES)


@cue('shop.deny.hp', .35, 2)
def _(rng, d):
    return deny(d, rng, PIZZ)


@cue('shop.invalid', .35, 3)
def _(rng, d):
    return deny(d, rng, MARIMBA) * .8


@cue('shop.reroll', .45, 3)
def _(rng, d):
    o = buf(d)
    for k in range(6):
        put(o, pitch(dk(CASTANET, 70), .8 + .03 * k), .04 * k, .3)
    put(o, chime(.25, (69,), GLOCK, 75, .05, .35), .26)
    return o


@cue('shop.fusion', 2.6, 2)
def _(rng, d):
    o = buf(d)
    put(o, shape(pitch(smp(REVCYM, 60, 85, 1.2), .9), 1.1), 0, .4)
    put(o, shape(smp(TIMPANI, 38, 115, 1.2), 1.2), 1.0, .7)
    put(o, chime(1.5, (57, 62, 66, 69, 74, 78), CELESTA, 90, .05, .4, 0), 1.02)
    put(o, shape(smp(BRASS, 50, 90, 1.4), 1.4), 1.02, .3)
    return o


@cue('shop.descend', .7, 2)
def _(rng, d):
    return chime(d, (74, 69, 66, 62, 57), HARP, 80, .08, .4, 0)


# --- interface (soft wood and tines; never shrill)
@cue('ui.hover', .06, 4, wet=.04)
def _(rng, d):
    return shape(smp(MARIMBA, 79, 40, .1), d) * .4


@cue('ui.select', .3, 4)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(WB_HI, 80), .78), 0, .4)
    put(o, shape(smp(MARIMBA, 74 + int(rng.integers(0, 2)) * 2, 80, .3), .28), 0, .4)
    return o


@cue('ui.confirm', 1.2, 3)
def _(rng, d):
    return chime(d, (62, 66, 69, 74), MARIMBA, 90, .05, .42, 0)


@cue('ui.back', .6, 3)
def _(rng, d):
    return chime(d, (69, 62), MARIMBA, 80, .07, .4, 0)


@cue('ui.open', .5, 2)
def _(rng, d):
    o = buf(d)
    put(o, flutter(.2, rng, 50, (600, 1500)), 0, .35)
    put(o, chime(.45, (66, 71), MARIMBA, 75, .06, .35, 0), .02)
    return o


@cue('ui.close', .45, 2)
def _(rng, d):
    o = buf(d)
    put(o, flutter(.18, rng, 50, (500, 1300)), 0, .35)
    put(o, chime(.4, (71, 66), MARIMBA, 70, .06, .35, 0), .02)
    return o


@cue('ui.page', .45, 3)
def _(rng, d):
    return flutter(d, rng, 40, (700, 2000)) * .6 + air(d, rng, 500, 1500, 1.0, .3, .3)


@cue('ui.tooltip', .06, 2, wet=.04)
def _(rng, d):
    return shape(smp(KALIMBA, 74, 50, .1), d) * .4


@cue('ui.tab', .3, 3)
def _(rng, d):
    return pitch(dk(WB_HI, 80), .8)[:N(d)] * .4 + shape(smp(MARIMBA, 69, 70, .3), d) * .35


@cue('ui.pause', 1.4, 1)
def _(rng, d):
    return chime(d, (74, 69, 66, 62), CELESTA, 75, .09, .4, 0)


@cue('ui.resume', 1.4, 1)
def _(rng, d):
    return chime(d, (62, 66, 69, 74), CELESTA, 75, .09, .4, 0)


@cue('ui.setting', .3, 3)
def _(rng, d):
    return pitch(dk(STICK, 70), .8)[:N(d)] * .4 + shape(smp(MARIMBA, 71, 60, .3), d) * .3


@cue('ui.unlock', 1.6, 2)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(CASTANET, 90), .7), 0, .4); put(o, pitch(dk(WB_LO, 90), .7), .08, .4)
    put(o, chime(1.4, (62, 66, 69, 74, 78), CELESTA, 85, .06, .4, 0), .14)
    return o


@cue('ui.achievement', 2.1, 2)
def _(rng, d):
    o = chime(d, (62, 66, 69, 74, 78, 81), GLOCK, 90, .07, .38)
    put(o, shape(smp(BRASS, 62, 85, 1.6), d), .05, .3); put(o, shape(smp(TIMPANI, 38, 100, 1.0), 1.0), 0, .4)
    return o


@cue('ui.receipt', .9, 2)
def _(rng, d):
    o = buf(d)
    for k in range(7):
        put(o, pitch(dk(CASTANET, 50), .9), .05 * k, .25)
    put(o, chime(.4, (74,), GLOCK, 75, .05, .35), .4)
    return o


@cue('ui.locked', .3, 2)
def _(rng, d):
    return deny(d, rng, MARIMBA) * .7


@cue('ui.error', .35, 2)
def _(rng, d):
    return deny(d, rng, PIZZ) * .8


@cue('ui.cutin.boss', 2.2, 2)
def _(rng, d):
    o = buf(d)
    put(o, shape(smp(TAIKO, 33, 127, 1.2), 1.2), 0, .9)
    put(o, shape(smp(BRASS, 38, 110, 1.6), 1.8), .02, .5); put(o, shape(smp(TUBA, 26, 110, 1.6), 1.8), .02, .5)
    put(o, shape(pitch(dk(CRASH, 100), .8), 1.5), .02, .3)
    return o


@cue('ui.cutin.showstopper', 2.2, 2)
def _(rng, d):
    o = buf(d)
    put(o, pitch(smp(ORCHHIT, 55, 110, .6), .9), 0, .7)
    put(o, chime(1.8, (62, 66, 69, 74), BRASS, 95, .1, .35, 0), .1)
    return o


@cue('ui.cutin.encore', 2.2, 2)
def _(rng, d):
    o = chime(d, (62, 69, 74, 78), VIBES, 95, .08, .4, 0)
    put(o, pitch(dk(SPLASH, 90), .9), 0, .3)
    return o


@cue('ui.cutin.bond', 2.2, 3)
def _(rng, d):
    o = chime(d, (57, 62, 66, 69, 74), HARP, 95, .07, .4, 0)
    put(o, shape(smp(STRINGS, 50, 75, 1.8), d), .05, .25)
    return o


@cue('ui.wave.stamp', .32, 2)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(WB_LO, 120), .62), 0, .7); put(o, pitch(dk(KICK, 110), .8), 0, .7)
    return o


@cue('shop.ticket.sold', .4, 3)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(WB_LO, 110), .65), 0, .6)
    put(o, chime(.3, (74,), GLOCK, 70, .05, .3), .05)
    return o


@cue('shop.ticket.sway', .25, 3, wet=.05)
def _(rng, d):
    return flutter(d, rng, 30, (600, 1500)) * .4


@cue('ui.coach.pop', .12, 3)
def _(rng, d):
    return pitch(dk(BONGO_HI, 70), .85)[:N(d)] * .5


def spot(d, notes, prog):
    return chime(d, notes, prog, 75, .06, .38, 0)


@cue('menu.spot.hero', .45, 2)
def _(rng, d):
    return spot(d, (62, 69), VIBES)


@cue('menu.spot.door', .6, 2)
def _(rng, d):
    return creak(.45, rng, (12, 22), ((180, 9), (420, 10), (900, 11))) * .4 + spot(d, (57, 62), MARIMBA)


@cue('menu.spot.jar', .45, 2)
def _(rng, d):
    return spot(d, (69, 74), GLOCK)


@cue('menu.spot.printer', .45, 2)
def _(rng, d):
    o = buf(d)
    for k in range(5):
        put(o, pitch(dk(CASTANET, 60), .85), .05 * k, .25)
    return o


@cue('menu.spot.lamp', .45, 2)
def _(rng, d):
    return spot(d, (66, 73), CELESTA)


@cue('menu.spot.bell', .65, 2)
def _(rng, d):
    return shape(smp(TUBULAR, 62, 80, .6), d) * .5


@cue('ui.encore.ready', 1.4, 2)
def _(rng, d):
    return chime(d, (62, 66, 69, 74), VIBES, 90, .06, .4, 0)


@cue('ui.merge.hint', .8, 2)
def _(rng, d):
    return chime(d, (66, 71), KALIMBA, 75, .1, .4, 0)


# --- events and stings
@cue('event.warn', 1.9, 2)
def _(rng, d):
    o = buf(d)
    for k in range(3):
        put(o, shape(smp(HORN, 50, 100, .4), .45), .5 * k, .5)
        put(o, shape(smp(TIMPANI, 38, 100, .4), .45), .5 * k, .4)
    return o


def event_start(d, rng, notes, prog):
    o = buf(d)
    put(o, shape(smp(TIMPANI, 38, 105, .6), .6), 0, .6)
    put(o, chime(d, notes, prog, 90, .08, .4, 0), .05)
    return o


@cue('event.parade.start', 1.0, 2)
def _(rng, d):
    return event_start(d, rng, (62, 66, 69), BRASS)


@cue('event.trap.start', 1.0, 2)
def _(rng, d):
    return event_start(d, rng, (62, 65, 68), PIZZ)


@cue('event.eclipse.start', 1.0, 2)
def _(rng, d):
    return event_start(d, rng, (57, 60, 64), CHOIR)


@cue('event.reward', .7, 3)
def _(rng, d):
    return chime(d, (62, 69, 74), GLOCK, 85, .06, .4)


@cue('event.end', .65, 2)
def _(rng, d):
    return chime(d, (69, 66, 62), MARIMBA, 85, .07, .4, 0)


@cue('sting.waveclear', 2.0, 2)
def _(rng, d):
    o = chime(d, (62, 66, 69, 74, 78), GLOCK, 95, .07, .38)
    put(o, shape(smp(BRASS, 62, 90, 1.4), 1.6), .3, .35); put(o, shape(smp(TIMPANI, 38, 100, .8), .8), .3, .4)
    return o


@cue('sting.victory', 3.3, 1)
def _(rng, d):
    o = buf(d)
    for k, m in enumerate((62, 66, 69, 74)):
        put(o, shape(smp(BRASS, m, 105, 1.0 if k < 3 else 2.4), 1.0 if k < 3 else 2.5), .22 * k, .4)
    put(o, chime(2.4, (74, 78, 81, 86), GLOCK, 90, .08, .3), .7)
    put(o, shape(smp(TIMPANI, 38, 115, 1.5), 1.5), .66, .5)
    put(o, shape(pitch(dk(CRASH, 100), .9), 2.0), .66, .25)
    return o


@cue('sting.defeat', 3.4, 1)
def _(rng, d):
    o = buf(d)
    for k, m in enumerate((62, 58, 57, 50)):
        put(o, shape(smp(CELLO, m - 12, 95, 1.0 if k < 3 else 2.4), 1.0 if k < 3 else 2.5), .4 * k, .45)
    put(o, shape(smp(CELESTA, 62, 60, 2.0), 2.0), 1.2, .25)
    return o



# --- map mechanics (requirement 1/11/2026: every map's mechanic is heard) and the elite's arrival
def bong(m, d, g=.6):
    return shape(smp(TUBULAR, m, 110, d), d) * g


@cue('enemy.elite.arrive', 1.1, 2)
def _(rng, d):
    o = buf(d)
    put(o, shape(smp(HORN, 41, 115, .6), .7), 0, .7); put(o, shape(smp(TUBA, 29, 120, .6), .7), 0, .6)
    put(o, shape(smp(TAIKO, 33, 127, .6), d), .32, 1.0); put(o, tone(28, d, .25), .32, .6)
    put(o, grit(.5, rng, 900, 120, .5), .34)
    return o


# Waiting Room: the numbered doors, the tickets, the wall clock
@cue('map.room.call', .9, 2)
def _(rng, d):
    o = buf(d)
    put(o, shape(smp(TUBULAR, 62, 100, .8), d), 0, .5); put(o, shape(smp(TUBULAR, 57, 95, .8), d), .22, .5)
    for k in range(3): put(o, pitch(dk(WB_LO, 110), .6), .5 + .1 * k, .5)
    return o


@cue('map.room.door', .7, 2)
def _(rng, d):
    o = buf(d)
    put(o, creak(.5, rng, (10, 20), ((150, 9), (380, 10), (800, 11))), 0, .7)
    put(o, pitch(dk(KICK, 120), .7), .45, .9); put(o, tone(30, .3, .12), .45, .5)
    return o


@cue('map.room.ticket', .45, 3)
def _(rng, d):
    o = buf(d)
    put(o, flutter(.15, rng, 60, (800, 2200)), 0, .4)
    put(o, chime(.4, (69, 74), GLOCK, 85, .05, .4), .04)
    return o


@cue('map.room.clockglow', 1.0, 2)
def _(rng, d):
    o = buf(d)
    for k in range(4): put(o, pitch(dk(WB_HI if k % 2 else WB_LO, 90), .75), .22 * k, .5)
    return o


@cue('map.room.clockstrike', 2.0, 2)
def _(rng, d):
    o = buf(d)
    put(o, bong(45, d, .8), 0); put(o, bong(57, d, .4), .01); put(o, tone(33, d, .6), 0, .6)
    put(o, shape(pitch(dk(CRASH, 100), .7), 1.5), 0, .25)
    return o


# Post Office: the chute, the caught letters, the gust, the paper planes, the parcel
@cue('map.post.chute', .8, 2)
def _(rng, d):
    o = buf(d)
    put(o, creak(.3, rng, (16, 30), ((300, 9), (700, 10), (1500, 11))), 0, .5)
    put(o, flutter(.6, rng, 70, (900, 2400)), .15, .6)
    return o


@cue('map.post.catch', .5, 4)
def _(rng, d):
    o = buf(d)
    put(o, flutter(.12, rng, 70, (900, 2400)), 0, .4)
    put(o, chime(.45, (66 + int(rng.integers(0, 3)) * 2, 73), CELESTA, 85, .05, .45, 0), .02)
    return o


@cue('map.post.delivered', 1.4, 2)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(WB_LO, 120), .6), 0, .8); put(o, pitch(dk(KICK, 110), .8), 0, .6)
    put(o, chime(1.3, (62, 66, 69, 74, 78), GLOCK, 95, .06, .4), .08)
    return o


@cue('map.post.gustwarn', .9, 2)
def _(rng, d):
    return wind(d, rng, 420, .6, .9) * .8 + shape(smp(TREM, 50, 70, .8), d) * .2


@cue('map.post.gust', 1.6, 2)
def _(rng, d):
    o = buf(d)
    put(o, air(d, rng, 160, 900, .8, .3, 1.0), 0); put(o, flutter(1.2, rng, 90, (700, 2200)), .1, .5)
    return o


@cue('map.post.planes', 1.0, 2)
def _(rng, d):
    o = buf(d)
    for k in range(3): put(o, air(.4, rng, 500, 2000, 2.0, .6, .6), .18 * k)
    put(o, chime(.6, (69, 74, 78), GLOCK, 80, .06, .3), .05)
    return o


@cue('map.post.planehit', .5, 4)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(KICK, 115), .8), 0, .9); put(o, flutter(.3, rng, 90, (700, 2200)), 0, .6)
    put(o, grit(.3, rng, 2000, 200, .5), .01)
    return o


@cue('map.post.parcel', .7, 2)
def _(rng, d):
    o = buf(d)
    put(o, air(.35, rng, 250, 900, 1.2, .9, .5), 0)
    put(o, pitch(dk(TOM_LO, 120), .7), .34, .9); put(o, pitch(dk(WB_LO, 110), .6), .36, .5)
    return o


# Glasshouse: sun and rain, the cracking panes, the seed turrets, a bloom picked
@cue('map.glass.sun', 1.4, 2)
def _(rng, d):
    return chime(d, (62, 66, 69, 74), HARP, 90, .09, .4, 0) + shape(smp(STRINGS, 62, 70, 1.3), d) * .25


@cue('map.glass.rain', 1.6, 2)
def _(rng, d):
    o = buf(d)
    put(o, lp(noise(d, rng, 'pink'), 3500) * env_ar(d, .4, .6) * .8, 0)
    for k in range(10): put(o, drip(rng, 800 + 600 * rng.random(), .06), rng.uniform(0, d - .1), .25)
    return o


@cue('map.glass.crack', .8, 2)
def _(rng, d):
    o = buf(d)
    for k in range(4): put(o, pitch(dk(STICK, 90), .8 + .05 * k), .12 * k, .35)
    put(o, shape(smp(CELESTA, 86, 50, .5), .5), .1, .2)
    return o


@cue('map.glass.shatter', 1.0, 3)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(KICK, 120), .8), 0, .8); put(o, grit(.8, rng, 5200, 340, .9), .01)
    for k in range(6): put(o, shape(smp(CELESTA, 79 + 2 * k, 70, .5), .45), .02 + .04 * k, .25)
    return o


@cue('map.glass.seed', .3, 4)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(CONGA_HI, 100), .7), 0, .7); put(o, air(.2, rng, 500, 1600, 1.5, .2, .5), .01)
    return o


@cue('map.glass.bloom', .9, 3)
def _(rng, d):
    return chime(d, (66, 71, 74, 78), VIBES, 85, .05, .4, 0) + flutter(.2, rng, 50, (600, 1400)) * .3


# Lamp Hall: a lamp snuffed, a flare, the carried lantern, oil picked up
@cue('map.lamp.snuff', .8, 2)
def _(rng, d):
    o = buf(d)
    put(o, breath(.5, rng) * .8, 0); put(o, shape(smp(VIBES, 50, 80, .6), .6), .25, .35)
    return o


@cue('map.lamp.flare', 1.0, 2)
def _(rng, d):
    o = buf(d)
    put(o, air(.4, rng, 200, 1600, 1.0, .2, .9), 0); put(o, grit(.6, rng, 1600, 160, .6), .05)
    put(o, shape(pitch(dk(CRASH, 110), .8), .8), .02, .35); put(o, tone(33, d, .3), 0, .5)
    return o


@cue('map.lamp.lantern', 1.0, 2)
def _(rng, d):
    return chime(d, (57, 62, 66, 69), CELESTA, 90, .07, .4, 0) + air(.5, rng, 200, 900, 1.0, .3, .4)


@cue('map.lamp.oil', .6, 3)
def _(rng, d):
    o = buf(d)
    put(o, drip(rng, 500, .1), 0, .6); put(o, chime(.5, (69, 74), MARIMBA, 85, .05, .4, 0), .05)
    return o


# Theatre: the spotlight's colour, the encore, the drum, the curtain, the stage cannon
@cue('map.theatre.spot', .8, 2)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(STICK, 110), .7), 0, .5); put(o, hp(noise(.4, rng), 2500) * env_ar(.4, .02, .3) * .3, .02)
    put(o, chime(.6, (62, 69), VIBES, 80, .06, .35, 0), .05)
    return o


@cue('map.theatre.encore', 2.0, 2)
def _(rng, d):
    o = buf(d)
    put(o, shape(smp(BRASS, 62, 110, 1.6), 1.8), 0, .5); put(o, shape(smp(BRASS, 66, 105, 1.6), 1.8), .08, .4)
    put(o, shape(pitch(dk(CRASH, 110), .9), 1.6), 0, .3)
    for k in range(12): put(o, pitch(dk(CLAP, 80 + int(rng.integers(0, 30))), J(rng, .9, 1.1)), .3 + rng.uniform(0, 1.2), .18)
    return o


@cue('map.theatre.drum', 1.0, 2)
def _(rng, d):
    o = buf(d)
    for k in range(8): put(o, pitch(dk(SNARE, 90 + 4 * k), .85), .07 * k, .35)
    put(o, shape(smp(TIMPANI, 38, 120, .5), .5), .56, .8)
    return o


@cue('map.theatre.curtainwarn', .9, 2)
def _(rng, d):
    o = buf(d)
    put(o, creak(.8, rng, (8, 16), ((120, 9), (300, 10), (700, 11))), 0, .6)
    put(o, shape(smp(HORN, 45, 95, .5), .5), 0, .4)
    return o


@cue('map.theatre.curtain', .8, 2)
def _(rng, d):
    o = buf(d)
    put(o, air(.4, rng, 120, 600, .8, .7, .9), 0); put(o, flutter(.35, rng, 40, (300, 900)), .05, .6)
    put(o, shape(smp(TAIKO, 33, 127, .5), .5), .35, 1.0); put(o, tone(28, .4, .15), .35, .6)
    return o


@cue('map.theatre.cannon', 1.0, 2)
def _(rng, d):
    o = buf(d)
    put(o, shape(smp(TAIKO, 31, 127, .7), d), 0, 1.0); put(o, lp(noise(d, rng, 'brown'), 500) * env_exp(d, .25) * 3, 0, .7)
    put(o, shape(pitch(dk(CRASH, 120), .8), .9), .01, .35)
    for k in range(5): put(o, shape(smp(GLOCK, 74 + 3 * k, 70, .3), .3), .2 + .05 * k, .2)  # confetti
    return o


@cue('map.theatre.coin', .4, 3)
def _(rng, d):
    return chime(d, (79,), GLOCK, 75, .05, .35) + pitch(dk(TRI_MUTE, 70), .85)[:N(d)] * .3


# Archive Vault: pages caught, a chapter, the drifting fog, the ink pot, the stamp, a bookworm's spit
@cue('map.vault.page', .5, 4)
def _(rng, d):
    o = buf(d)
    put(o, flutter(.2, rng, 50, (700, 2000)), 0, .5); put(o, chime(.45, (64, 71), CELESTA, 80, .05, .4, 0), .03)
    return o


@cue('map.vault.chapter', 1.6, 2)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(WB_LO, 120), .55), 0, .8)
    put(o, chime(1.4, (57, 62, 64, 69, 74), CELESTA, 90, .07, .4, 0), .1); put(o, shape(smp(CHOIR, 57, 70, 1.4), 1.4), .1, .25)
    return o


@cue('map.vault.fog', 1.4, 2)
def _(rng, d):
    return wind(d, rng, 300, .5, .4) * .7 + shape(smp(CHOIR, 50, 55, 1.3), d) * .2


@cue('map.vault.potwarn', .9, 2)
def _(rng, d):
    o = buf(d)
    for k in range(3): put(o, shape(pitch(smp(MARIMBA, 45, 100, .2), 1.0), .2), .25 * k, .5)
    put(o, creak(.4, rng, (12, 22)), .1, .3)
    return o


@cue('map.vault.spill', 1.0, 2)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(TOM_LO, 120), .65), 0, .9)
    put(o, lp(noise(.8, rng, 'brown'), 700) * env_ar(.8, .05, .5) * 3, .02, .7)
    for k in range(5): put(o, drip(rng, 400 + 200 * k, .08), .15 + .1 * k, .3)
    return o


@cue('map.vault.stampwarn', .8, 2)
def _(rng, d):
    o = buf(d)
    put(o, creak(.6, rng, (14, 24), ((200, 9), (500, 10))), 0, .5); put(o, shape(smp(HORN, 43, 95, .4), .4), .1, .35)
    return o


@cue('map.vault.stamp', .7, 3)
def _(rng, d):
    o = buf(d)
    put(o, pitch(dk(WB_LO, 127), .5), 0, 1.0); put(o, pitch(dk(KICK, 127), .7), 0, 1.0)
    put(o, tone(28, d, .15), 0, .7); put(o, grit(.3, rng, 1200, 100, .4), .01)
    return o


@cue('map.vault.spit', .4, 3)
def _(rng, d):
    o = buf(d)
    put(o, voiced(.15, 180, rng, ((400, 5), (1000, 6), (2400, 7)), .4) * env_exp(.15, .06), 0, .5)
    put(o, flutter(.2, rng, 60, (800, 2000)), .05, .4)
    return o


# ---------------------------------------------------------------------------------------------- render
_meter = pyloudnorm.Meter(SR)


def loudness(y):
    pad = np.concatenate([y, np.zeros(max(0, N(.5) - len(y)))])
    try:
        return _meter.integrated_loudness(pad)
    except Exception:
        return -70.0


def level(y, target=LEVEL, ceiling=-1.0):
    """Levelled to target LUFS, then a soft limit keeps the peak under the ceiling."""
    l = loudness(y)
    if np.isfinite(l) and l > -70:
        y = y * 10 ** ((target - l) / 20)
    lim = 10 ** (ceiling / 20)
    pk = np.abs(y).max()
    if pk > lim:
        y = np.tanh(y / lim * .9) * lim / np.tanh(.9)
    return y


LOWER = ('cmb.swing', 'cmb.miss', 'cmb.windup', 'wpn.', 'sig.', 'hero.', 'cmb.hit', 'cmb.crit')


def render(name, k):
    fn, dur, count, wet, lpf = R[name]
    rng = np.random.default_rng(zlib.crc32(('%s/%d' % (name, k)).encode()))
    y = np.asarray(fn(rng, dur), dtype=float)
    if name.startswith(LOWER):
        # the attack families sit lower still: about three semitones down and the airy top rolled off
        y = np.asarray(pitch(y, .84), dtype=float)
        lpf = min(lpf, 6000)
    if name.startswith(PUNCH):
        y = punch(y, rng, name)
    y = finish(y, dur, wet, -3.0, 45, lpf, .03)
    return level(y, LEVEL + (3 if name.startswith(PUNCH) else 0))


# requirement 1/11/2026 ("no force"): every blow gets a hard front: a short click on the attack, a sub thump under it, and a
# little saturation so the body stays loud after the peak
PUNCH = ('cmb.swing', 'cmb.hit', 'cmb.crit', 'wpn.', 'sig.', 'hero.', 'enemy.die', 'fx.knockback', 'fx.hazard.hit', 'fx.reaction',
         'plr.hurt', 'enemy.big', 'enemy.elite', 'map.post.planehit', 'map.vault.stamp', 'map.theatre.cannon', 'map.theatre.curtain', 'map.glass.shatter', 'map.room.clockstrike')


def punch(y, rng, name):
    y = np.asarray(y, dtype=float)
    y = y / max(np.abs(y).max(), 1e-9)
    swing = name.startswith(('cmb.swing', 'wpn.boomerang', 'wpn.yoyo', 'wpn.ribbon', 'wpn.pierce'))
    at = 0 if not swing else int(np.argmax(np.abs(y)) * .9)  # a swing's blow lands at its loudest moment, not at its start
    out = np.zeros(max(len(y), at + N(.2)))
    out[:len(y)] += y
    t = np.arange(N(.16)) / SR
    sub = np.sin(2 * np.pi * np.cumsum(52 * (1 + 1.2 * np.exp(-t / .018))) / SR) * np.exp(-t / .055)
    out[at:at + len(sub)] += sub * .7
    clk = np.asarray(hp(noise(.006, rng), 1800), dtype=float) * np.exp(-np.arange(N(.006)) / SR / .0015)
    out[at:at + len(clk)] += clk / max(np.abs(clk).max(), 1e-9) * .45
    return np.tanh(out * 2.2) / np.tanh(2.2)


def clip_name(name, k):
    return 's7_' + name.replace('.', '_') + '_%d' % k


META = """fileFormatVersion: 2
guid: {guid}
AudioImporter:
  externalObjects: {{}}
  serializedVersion: 8
  defaultSettings:
    serializedVersion: 2
    loadType: 0
    sampleRateSetting: 0
    sampleRateOverride: 44100
    compressionFormat: 1
    quality: 0.8
    conversionMode: 0
    preloadAudioData: 1
  platformSettingOverrides: {{}}
  forceToMono: 0
  normalize: 0
  loadInBackground: 0
  ambisonic: 0
  3D: 0
  userData: Arcane SFX revision 7; see Docs/Audio/Provenance.md
  assetBundleName:
  assetBundleVariant:
"""


def main():
    out = sys.argv[1]
    names = sys.argv[2:] or list(R)
    os.makedirs(out, exist_ok=True)
    for nm in names:
        for k in range(R[nm][2]):
            y = render(nm, k)
            path = os.path.join(out, clip_name(nm, k) + '.wav')
            sf.write(path, y, SR, subtype='PCM_16')
            meta = path + '.meta'
            if not os.path.exists(meta):
                with open(meta, 'w', newline='\n') as f:
                    f.write(META.format(guid=hashlib.md5(('arcane-sfx-r7/' + clip_name(nm, k)).encode()).hexdigest()))
        print('%-28s %d x %.2fs' % (nm, R[nm][2], R[nm][1]), flush=True)


if __name__ == '__main__':
    main()
