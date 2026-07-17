"""Toy-shop sound kit (requirement 27/10/2026: the old effects sounded odd and did not suit the game). Every effect is built from the same few
materials the game is made of: wood, brass bells, music-box tines, paper, cloth, glass and a little air, all warm and rounded, never harsh or
electronic. Recipes are chosen by cue id; each cue's clips are the variants (small random changes of pitch, noise and timing).

Run:  python Tools/Audio/sfx_kit.py            (rewrites the wavs of the covered cues in place and fixes loudness.json)
Not covered (kept as they were): boss, hero, signature, voice, ambience, menu spots, events, the star bells (Tools/Audio/star_bells.py)."""
import json, os, re, wave
import numpy as np

SR = 44100
ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "Assets", "ArcaneBrigade", "Resources", "Audio", "Arcane")
ROOT = os.path.normpath(ROOT)
TARGET_DB = -12.07
rng = np.random.default_rng(271027)


def t_(dur): return np.arange(max(1, int(SR * dur))) / SR


def env(n, attack=.003, decay=8.0):
    t = np.arange(n) / SR
    return np.minimum(1, t / max(1e-4, attack)) * np.exp(-t * decay)


def noise(n): return rng.standard_normal(n)


def filt(x, lo=None, hi=None, order=3):
    spec = np.fft.rfft(x); f = np.fft.rfftfreq(len(x), 1 / SR)
    g = np.ones_like(f)
    if hi: g *= 1 / (1 + (f / hi) ** (2 * order))
    if lo: g *= 1 - 1 / (1 + (f / lo) ** (2 * order))
    return np.fft.irfft(spec * g, len(x))


def tone(freq, dur, decay=6, attack=.003, harm=((1, 1),)):
    t = t_(dur); out = np.zeros_like(t)
    for r, a in harm: out += a * np.sin(2 * np.pi * freq * r * t)
    return out * env(len(t), attack, decay)


def sweep(f0, f1, dur, decay=5, attack=.004, harm=((1, 1),)):
    t = t_(dur); f = f0 * (f1 / f0) ** (t / dur); ph = 2 * np.pi * np.cumsum(f) / SR; out = np.zeros_like(t)
    for r, a in harm: out += a * np.sin(ph * r)
    return out * env(len(t), attack, decay)


def bell(freq, dur=1.0, decay=5, bright=1.0):
    t = t_(dur); out = np.zeros_like(t)
    for ratio, a, d in ((1, 1, 1), (2.0, .5, 1.3), (2.76, .45 * bright, 1.7), (4.07, .25 * bright, 2.2), (5.4, .15 * bright, 2.8)):
        out += a * np.sin(2 * np.pi * freq * ratio * t) * np.exp(-t * decay * d)
    return out * np.minimum(1, t / .002)


def tine(freq, dur=.7, decay=7):
    """A music-box tine: a pure ping with a faint octave."""
    t = t_(dur)
    return (np.sin(2 * np.pi * freq * t) + .25 * np.sin(2 * np.pi * freq * 3 * t) * np.exp(-t * 12)) * env(len(t), .001, decay)


def wood(freq=520, dur=.12, hard=1.0):
    t = t_(dur)
    body = np.sin(2 * np.pi * freq * t) * np.exp(-t * 55) + .5 * np.sin(2 * np.pi * freq * 2.3 * t) * np.exp(-t * 80)
    click = filt(noise(len(t)), 800, 6000) * np.exp(-t * 400) * .6 * hard
    return body + click


def thud(freq=90, dur=.25, hard=1.0):
    t = t_(dur)
    return (sweep(freq * 1.8, freq, dur, 18, .002) + filt(noise(len(t)), None, 600) * np.exp(-t * 40) * .5 * hard)


def whoosh(dur=.3, lo=300, hi=2500, peak=.4, amount=1.0):
    n = int(SR * dur); t = np.arange(n) / SR
    x = filt(noise(n), lo, hi)
    shape = np.sin(np.pi * np.clip(t / dur, 0, 1) ** (1 / (2 * peak))) ** 2
    return x * shape * amount


def paper(dur=.2, bright=1.0):
    n = int(SR * dur); t = np.arange(n) / SR
    x = filt(noise(n), 2000, 9000 * bright)
    flutter = .6 + .4 * np.sin(2 * np.pi * rng.uniform(30, 60) * t)
    return x * flutter * np.exp(-t * (6 / dur)) * np.minimum(1, t / .004)


def clink(freq=2400, dur=.35, decay=12):
    t = t_(dur)
    return sum(a * np.sin(2 * np.pi * freq * r * t) * np.exp(-t * decay * d) for r, a, d in ((1, 1, 1), (1.51, .6, 1.4), (2.31, .4, 2.2), (3.4, .2, 3))) * np.minimum(1, t / .0008)


def glass(freq=3200, dur=.4):
    t = t_(dur)
    return (np.sin(2 * np.pi * freq * t) * np.exp(-t * 14) + .5 * np.sin(2 * np.pi * freq * 1.62 * t) * np.exp(-t * 20)) * np.minimum(1, t / .0006)


def pop(f=700, dur=.12):
    return sweep(f * 2.2, f * .7, dur, 30, .001) + .3 * filt(noise(int(SR * dur)), 400, 3000) * np.exp(-t_(dur) * 70)


def shimmer(dur=.8, base=1800, up=True):
    t = t_(dur); out = np.zeros_like(t)
    for i in range(7):
        f = base * (1 + .25 * i); s = rng.uniform(0, dur * .6)
        e = np.exp(-np.maximum(0, t - s) * 7) * (t >= s)
        out += np.sin(2 * np.pi * f * t) * e * (1 / (1 + i * .3))
    return out


def mix(*arrs):
    out = np.zeros(max(len(a) for a in arrs))
    for a in arrs: out[:len(a)] += a
    return out


def seq(parts, length):
    out = np.zeros(int(SR * length))
    for at, sig, gain in parts:
        i = int(at * SR); e = min(len(out), i + len(sig))
        if i < len(out): out[i:e] += sig[:e - i] * gain
    return out


def jitter(p, amount): return p * (1 + rng.uniform(-amount, amount))


def reverb(x, tail=.6, wet=.2):
    n = int(SR * tail); ir = rng.standard_normal(n) * np.exp(-np.arange(n) / SR * 6); ir[0] = 0
    size = 1 << int(np.ceil(np.log2(len(x) + n)))
    y = np.fft.irfft(np.fft.rfft(x, size) * np.fft.rfft(ir, size), size)[:len(x) + n]
    out = np.zeros(len(x) + n); out[:len(x)] = x; out += wet * y / (np.max(np.abs(y)) + 1e-9) * np.max(np.abs(x))
    return out


MATERIAL = {
    "paper": lambda v: seq([(0, paper(.16, 1.0), 1), (0, thud(jitter(220, .1), .08), .5)], .2),
    "metal": lambda v: seq([(0, clink(jitter(1500, .12), .4, 10), 1), (0, thud(jitter(160, .1), .08), .5)], .45),
    "wax": lambda v: seq([(0, thud(jitter(130, .1), .2, .6), 1), (0, filt(noise(int(SR * .1)), 300, 1500) * env(int(SR * .1), .004, 40), .5)], .25),
    "fabric": lambda v: seq([(0, filt(noise(int(SR * .14)), 150, 1400) * env(int(SR * .14), .006, 30), 1), (0, thud(jitter(100, .1), .12, .5), .6)], .2),
    "glass": lambda v: seq([(0, glass(jitter(3000, .15), .35), 1), (0, wood(jitter(900, .1), .05), .4)], .4),
    "wood": lambda v: seq([(0, wood(jitter(430, .12), .14), 1.1), (.0, thud(jitter(140, .1), .1), .4)], .2),
}


def recipe(cid, v):
    """The sound of one cue (variant v) as a float array."""
    k = cid.split(".")
    g, rest = k[0], ".".join(k[1:])
    r = rest
    # ---------------------------------------------------------------- UI
    if g == "ui":
        if r == "hover": return wood(jitter(900, .08), .05) * .5
        if r in ("select", "tab", "setting"): return seq([(0, wood(jitter(620, .06), .09), 1), (.02, tine(jitter(1760, .02), .3, 10), .5)], .35)
        if r == "confirm": return reverb(seq([(0, tine(jitter(1318.5, .01), .6, 7), 1), (.09, tine(jitter(1975.5, .01), .8, 6), .9)], .9), .5, .2)
        if r == "back": return seq([(0, tine(1568, .4, 9), .8), (.08, tine(1046.5, .6, 8), 1)], .7)
        if r in ("open", "page"): return seq([(0, paper(.22, .9), .9), (.05, tine(jitter(1568, .03), .4, 10), .35)], .5)
        if r == "close": return seq([(0, paper(.18, .7), .8), (.04, tine(jitter(1046, .03), .4, 10), .3)], .45)
        if r == "tooltip": return wood(jitter(1100, .05), .04) * .35
        if r in ("pause", "resume"): return reverb(seq([(0, bell(jitter(659 if r == "pause" else 880, .01), .9, 5), 1)], 1.0), .6, .25)
        if r == "unlock": return reverb(seq([(0, tine(1046.5, .6, 7), 1), (.07, tine(1318.5, .6, 7), .9), (.14, tine(1568, .9, 6), .9), (.0, shimmer(.8), .12)], 1.1), .6, .25)
        if r == "achievement": return reverb(seq([(i * .1, bell(f, .9, 5, .8), .9) for i, f in enumerate((784, 988, 1175, 1568))] + [(.1, shimmer(1.0), .1)], 1.4), .8, .3)
        if r == "receipt": return seq([(0, paper(.25), .8), (.12, bell(jitter(1760, .02), .8, 6, .6), .8)], .9)
        if r.startswith("cutin"): return reverb(seq([(0, whoosh(.35, 200, 1800, .6), .6), (.25, bell(jitter(523 if "boss" in r else 784, .01), 1.2, 4), 1)], 1.5), .8, .3)
        if r == "wave.stamp": return seq([(0, thud(120, .18), 1), (0, wood(300, .1), .8), (.0, paper(.1), .4)], .3)
        if r == "coach.pop": return pop(jitter(900, .1), .1) * .9
        if r == "encore.ready": return reverb(seq([(0, tine(1568, .5, 8), 1), (.1, tine(2093, .7, 7), .9)], .9), .5, .2)
        if r == "merge.hint": return seq([(0, tine(jitter(1760, .02), .5, 9), .7), (.12, tine(jitter(2093, .02), .6, 8), .6)], .8)
        if r == "locked": return seq([(0, wood(jitter(250, .05), .14), 1), (.09, wood(jitter(210, .05), .14), .8)], .3)
        if r == "error": return seq([(0, wood(jitter(300, .04), .14), 1), (.1, wood(jitter(230, .04), .16), 1)], .35)
    # ---------------------------------------------------------------- shop
    if g == "shop":
        if r.startswith("keeper.enter") or r.startswith("keeper.exit"):
            up = "enter" in r
            fs = (2349, 2637, 2794, 3136) if up else (3136, 2794, 2637, 2349)
            return reverb(seq([(.04 * i + rng.uniform(0, .015), bell(jitter(f, .02), .6, 9, .5), .6) for i, f in enumerate(fs)], .9), .4, .2)
        if r in ("carpet.open", "carpet.roll"): return whoosh(.55, 120, 900, .5, 1.0) * .8
        if r == "stand.appear": return seq([(0, pop(520, .1), 1), (.05, shimmer(.5, 2200), .12)], .6)
        if r == "item.pop": return pop(jitter(760, .1), .1)
        if r in ("approach", "focus"): return wood(jitter(1000, .06), .04) * .4
        if r == "buy.gold": return reverb(seq([(0, clink(jitter(2400, .05), .35, 10), .8), (.07, clink(jitter(2900, .05), .35, 10), .7), (.14, bell(jitter(1568, .01), .9, 5, .6), .9)], 1.1), .5, .2)
        if r == "buy.dream": return reverb(seq([(0, tine(1175, .8, 6), 1), (.08, tine(1568, .9, 6), .9), (.0, shimmer(.9, 1500), .12)], 1.2), .7, .3)
        if r.startswith("deny") or r == "invalid": return seq([(0, wood(jitter(260, .05), .15), 1), (.08, filt(thud(120, .2), None, 700), .7)], .35)
        if r == "reroll": return seq([(i * .045 + rng.uniform(0, .02), wood(jitter(rng.uniform(500, 900), .1), .06), .7) for i in range(6)], .45)
        if r == "fusion": return reverb(seq([(0, shimmer(1.2, 1200), .5), (.1, sweep(300, 1400, .9, 2.5, .1), .3), (.9, bell(1318, 1.2, 4), .9)], 2.0), .8, .3)
        if r == "descend": return seq([(i * .11, thud(jitter(140 - i * 8, .05), .12), .8 - i * .08) for i in range(5)], .7)
        if r == "ticket.sold": return seq([(0, thud(140, .15), .9), (0, wood(380, .08), .7), (.06, paper(.2, .9), .8)], .4)
        if r == "ticket.sway": return paper(.25, .6) * .4
    # ---------------------------------------------------------------- loot
    if g == "loot":
        if r == "coin.drop": return clink(jitter(2500, .08), .3, 14) * .8
        if r == "coin.bounce": return clink(jitter(3000, .08), .2, 20) * .5
        if r == "coin.vacuum": return seq([(0, whoosh(.3, 500, 3500, .8), .5), (.05, shimmer(.3, 2200), .1)], .4)
        if r == "coin.pickup": return seq([(0, clink(jitter(3100, .04), .22, 16), .9), (.03, tine(jitter(2349, .03), .3, 12), .5)], .35)
        if r == "egg": return seq([(0, tine(jitter(988, .02), .6, 7), .9), (.07, tine(jitter(1319, .02), .5, 8), .6), (0, wood(500, .06), .4)], .7)
        if r == "heart": return reverb(seq([(0, bell(jitter(659, .01), .8, 6, .4), .9), (.1, bell(jitter(880, .01), .8, 6, .4), .8)], 1.0), .4, .2)
        if r == "chest.appear": return reverb(seq([(0, shimmer(.8, 1800), .35), (.1, bell(1175, .9, 5), .8)], 1.1), .5, .2)
        if r == "chest.open": return reverb(seq([(0, sweep(180, 260, .35, 4, .05, ((1, 1), (2, .4))) * .4, 1), (.3, shimmer(.8, 1600), .3), (.35, bell(1568, 1.0, 5), .9)], 1.4), .6, .25)
    # ---------------------------------------------------------------- combat
    if g == "cmb":
        if r.startswith("swing"):
            kind = r.split(".")[1]
            lo, hi, dur = {"blade": (700, 5000, .22), "spear": (500, 3500, .2), "hammer": (150, 1500, .3), "scissors": (1200, 6000, .16), "umbrella": (300, 2200, .26), "whip": (900, 7000, .18)}.get(kind, (400, 3000, .22))
            s = whoosh(dur, jitter(lo, .1), jitter(hi, .1), .5)
            if kind == "scissors": s = mix(s, .5 * clink(3500, .12, 30))
            return s * .9
        if r == "windup": return sweep(jitter(260, .05), jitter(520, .05), .3, 3, .05, ((1, 1), (2, .3))) * .4
        if r == "miss": return whoosh(.18, 300, 1800, .5) * .45
        if r.startswith("hit."): return MATERIAL.get(r.split(".")[1], MATERIAL["wood"])(v)
        if r == "crit": return seq([(0, MATERIAL["metal"](v), 1), (.02, bell(jitter(1760, .02), .6, 7, .8), .8)], .7)
    # ---------------------------------------------------------------- enemies
    if g == "enemy":
        if r.startswith("die.") and r != "die.large":
            m = MATERIAL.get(r.split(".")[1], MATERIAL["wood"])(v)
            return seq([(0, m, 1), (.04, sweep(jitter(500, .1), 160, .25, 12, .003) * .4, 1)], .45)
        if r == "die.large": return seq([(0, thud(60, .6), 1), (.05, sweep(260, 70, .5, 5) * .5, 1), (0, filt(noise(int(SR * .5)), 120, 1500) * env(int(SR * .5), .01, 8), .4)], .8)
        if r == "hurt": return seq([(0, wood(jitter(340, .1), .08), .9), (.02, sweep(jitter(420, .08), 250, .12, 18) * .5, 1)], .2)
        if r == "lunge": return whoosh(.22, 250, 1600, .4) * .8
        if r.startswith("spit"): return pop(jitter(500, .1), .14) * .9 if "windup" not in r else sweep(300, 600, .25, 4, .05) * .4
        if r == "big.step": return thud(jitter(70, .08), .3) * 1.0
        if r == "big.roar": return seq([(0, sweep(140, 90, .7, 3, .05, ((1, 1), (2, .5), (3, .3))) * .5, 1), (0, filt(noise(int(SR * .7)), 100, 1200) * env(int(SR * .7), .05, 4), .35)], .9)
        if r == "mimic.wake": return seq([(0, wood(260, .12), 1), (.12, wood(310, .12), .9), (.3, sweep(200, 420, .3, 5) * .4, 1)], .7)
        if r.startswith("shot"): return pop(jitter(820, .08), .1) * .7
    # ---------------------------------------------------------------- player and character
    if g == "plr":
        if r == "dodge": return whoosh(.2, 400, 3500, .4) * .7
        if r == "hurt": return seq([(0, thud(jitter(110, .06), .22), 1), (.02, sweep(380, 220, .22, 9) * .5, 1)], .35)
        if r == "lowhp": return seq([(0, thud(70, .18), 1), (.22, thud(65, .18), .8)], .5)
        if r == "heal": return reverb(seq([(i * .08, bell(f, .7, 7, .4), .8) for i, f in enumerate((659, 784, 988))], 1.0), .5, .2)
        if r == "death": return seq([(0, sweep(500, 110, 1.0, 2.5, .01, ((1, 1), (2, .3))) * .5, 1), (.05, thud(60, .6), .8)], 1.2)
    if g == "chr":
        if r == "step": return wood(jitter(380, .12), .05) * .5
        if r == "dash": return whoosh(.25, 300, 3000, .35) * .8
        if r == "jump": return sweep(260, 620, .16, 14, .004) * .5
        if r == "land": return thud(jitter(100, .08), .16) * .8
    # ---------------------------------------------------------------- weapons
    if g == "wpn":
        if r == "bow.draw": return filt(noise(int(SR * .3)), 200, 1500) * np.linspace(.2, 1, int(SR * .3)) * .5
        if r == "bow.release": return seq([(0, tone(jitter(220, .05), .25, 14, .001, ((1, 1), (2, .5), (3, .3))), .7), (0, whoosh(.1, 800, 4000), .5)], .3)
        if r in ("gun.pop", "launcher"): return mix(pop(jitter(430 if r == "launcher" else 650, .08), .15), thud(110, .15) * .5 if r == "launcher" else np.zeros(1))
        if r.startswith("cast."):
            f = {"fire": 440, "ice": 880, "arcane": 660}.get(r.split(".")[1], 600)
            return reverb(seq([(0, sweep(f * .8, f * 1.6, .35, 4, .03) * .5, 1), (.1, shimmer(.5, f * 3), .12)], .8), .4, .2)
        if r.startswith("boomerang"): return whoosh(.45, 300, 2400, .5) * (.7 if "fly" in r else .55)
        if r == "pierce": return whoosh(.12, 1500, 7000, .3) * .6
        if r == "split": return seq([(0, pop(800, .08), .7), (.05, pop(1000, .08), .6)], .2)
        if r == "bounce": return sweep(200, 520, .14, 12, .003) * .6
        if r.startswith("yoyo"): return whoosh(.3, 500, 3000, .5) * .5
        if r == "magnet.pull": return sweep(180, 420, .5, 3, .1, ((1, 1), (2, .3))) * .4
        if r == "needle.volley": return seq([(i * .03, wood(jitter(1500, .1), .03), .6) for i in range(5)], .25)
        if r == "spring.slam": return seq([(0, sweep(150, 450, .12, 14) * .6, 1), (.1, thud(90, .2), 1)], .35)
        if r == "projector.cast": return seq([(0, wood(1200, .04), .8), (.03, shimmer(.4, 1400), .2)], .5)
        if r == "ribbon.cut": return seq([(0, clink(4000, .1, 40), .8), (0, whoosh(.1, 2000, 8000), .4)], .15)
    # ---------------------------------------------------------------- effects and stings
    if g == "fx":
        if r == "burn": return filt(noise(int(SR * .4)), 800, 6000) * env(int(SR * .4), .01, 6) * (.6 + .4 * np.sin(2 * np.pi * 25 * t_(.4)))
        if r == "chill": return shimmer(.6, 2600) * .5
        if r == "knockback": return seq([(0, thud(100, .18), 1), (0, whoosh(.18, 300, 2000), .5)], .25)
        if r == "reaction.steam": return filt(noise(int(SR * .5)), 1500, 8000) * env(int(SR * .5), .02, 5) * .6
        if r == "reaction.shatter": return seq([(0, glass(3300, .5), 1), (.02, glass(2400, .4), .7), (.04, filt(noise(int(SR * .2)), 3000, 9000) * env(int(SR * .2), .001, 25), .5)], .6)
        if r == "reaction.arcane": return reverb(shimmer(.7, 1500) * .6, .4, .2)
        if r == "field.zipper": return filt(noise(int(SR * .25)), 1500, 7000) * (np.sin(2 * np.pi * 70 * t_(.25)) > 0) * .5
        if r == "hazard.warn": return seq([(0, tine(784, .35, 8), .8), (.18, tine(659, .45, 8), .8)], .7)
        if r == "hazard.hit": return thud(95, .25) * .9
    if g == "sting":
        if r == "waveclear": return reverb(seq([(i * .12, tine(f, .9, 5), .9) for i, f in enumerate((784, 988, 1175))], 1.3), .7, .3)
        if r == "victory": return reverb(seq([(i * .16, bell(f, 1.4, 4, .8), .9) for i, f in enumerate((523, 659, 784, 1046, 1318))], 2.4), .9, .35)
        if r == "defeat": return reverb(seq([(i * .3, bell(f, 1.6, 3.5, .4), .9) for i, f in enumerate((440, 392, 330, 262))], 2.6), .9, .35)
    return None


def write(path, x):
    x = x - np.mean(x)
    x = x / (np.max(np.abs(x)) + 1e-9) * .9
    fade = min(len(x) // 4, int(SR * .01)); x[-fade:] *= np.linspace(1, 0, fade)
    with wave.open(path, "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes((np.clip(x, -1, 1) * 32767).astype("<i2").tobytes())
    return 20 * np.log10(np.sqrt(np.mean(x ** 2)) + 1e-9)


def main():
    cues = json.load(open(os.path.join(ROOT, "cues.json"), encoding="utf-8"))["cues"]
    loud_path = os.path.join(ROOT, "loudness.json"); raw = open(loud_path, encoding="utf-8", newline="").read(); done = skipped = 0
    for c in cues:
        out0 = recipe(c["id"], 0)
        if out0 is None: skipped += 1; continue
        for v, clip in enumerate(c["clips"]):
            x = recipe(c["id"], v)
            path = os.path.join(ROOT, clip + ".wav")
            if not os.path.exists(path): continue
            rms = write(path, x); gain = round(float(TARGET_DB - rms), 2)
            name = os.path.basename(clip)
            raw = re.sub(r'("clip": "' + re.escape(name) + r'",\s*"rmsDb": )[-0-9.]+(,\s*"gainDb": )[-0-9.]+', lambda m: m.group(1) + str(round(float(rms), 2)) + m.group(2) + str(gain), raw)
            done += 1
    open(loud_path, "w", encoding="utf-8", newline="").write(raw)
    print("rewrote", done, "clips; cues kept as they were:", skipped)


if __name__ == "__main__":
    main()
