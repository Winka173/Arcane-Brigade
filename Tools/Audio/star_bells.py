"""Synthesises one bell cue per star level (half, 2, 3, 4, 5) plus the five-star "one of a kind" shimmer: FM/inharmonic bell
partials, arpeggios that grow richer with each star, a pad and a reverb tail. 44.1 kHz mono 16-bit like the other cues."""
import json, os, re, uuid, wave
import numpy as np

SR = 44100
ROOT = r"C:\Users\Winka\OneDrive\Documents\Arcane-Brigade\Assets\ArcaneBrigade\Resources\Audio\Arcane"
SFX = os.path.join(ROOT, "Sfx")
rng = np.random.default_rng(2710)


def note(n):  # midi
    return 440.0 * 2 ** ((n - 69) / 12)


def bell(freq, dur, amp=1.0, bright=1.0, decay=3.0):
    t = np.arange(int(SR * dur)) / SR
    out = np.zeros_like(t)
    # inharmonic partials of a struck bell
    for ratio, a, d in ((1.0, 1.0, 1.0), (2.0, .55, 1.25), (2.76, .5 * bright, 1.6), (4.07, .3 * bright, 2.0), (5.4, .2 * bright, 2.6), (6.8, .12 * bright, 3.3)):
        out += a * np.sin(2 * np.pi * freq * ratio * t) * np.exp(-t * decay * d)
    # a short FM strike for the attack
    out += .6 * np.sin(2 * np.pi * freq * t + 2.2 * np.sin(2 * np.pi * freq * 3.5 * t)) * np.exp(-t * 28)
    env = np.minimum(1, t / .004)
    return amp * out * env


def pad(freqs, dur, amp=.25, attack=.6):
    t = np.arange(int(SR * dur)) / SR
    out = np.zeros_like(t)
    for f in freqs:
        for det in (-.004, 0, .004):
            out += np.sin(2 * np.pi * f * (1 + det) * t + rng.uniform(0, 6.28))
    env = np.minimum(1, t / attack) * np.exp(-np.maximum(0, t - attack) * 1.1)
    return amp * out / (len(freqs) * 3) * env


def place(mix, sig, at):
    i = int(at * SR)
    end = min(len(mix), i + len(sig))
    mix[i:end] += sig[:end - i]


def reverb(x, tail=2.2, wet=.35):
    n = int(SR * tail)
    ir = rng.standard_normal(n) * np.exp(-np.arange(n) / SR * 3.2)
    ir[0] = 0
    # cheap fft convolution
    size = 1 << int(np.ceil(np.log2(len(x) + n)))
    y = np.fft.irfft(np.fft.rfft(x, size) * np.fft.rfft(ir, size), size)[:len(x) + n]
    y /= np.max(np.abs(y)) + 1e-9
    out = np.zeros(len(x) + n)
    out[:len(x)] += x
    out += wet * y * np.max(np.abs(x))
    return out


def finish(x, peak=.8):
    x = x - np.mean(x)
    x = x / (np.max(np.abs(x)) + 1e-9) * peak
    fade = int(SR * .08)
    x[-fade:] *= np.linspace(1, 0, fade)
    return x


def half():
    m = np.zeros(int(SR * 1.8))
    place(m, bell(note(84), 1.6, 1, .8, 3.2), 0)
    place(m, bell(note(91), 1.2, .35, .6, 4), .09)
    return finish(reverb(m, 1.2, .3))


def star2():
    m = np.zeros(int(SR * 2.4))
    place(m, bell(note(79), 1.8, .9, .9, 3), 0)
    place(m, bell(note(86), 2.0, 1, 1, 2.8), .13)
    place(m, bell(note(91), 1.4, .4, .7, 3.5), .26)
    return finish(reverb(m, 1.6, .35))


def star3():
    m = np.zeros(int(SR * 3.4))
    place(m, bell(note(60), 3.0, .7, .5, 1.6), 0)  # a warm low bell under the climb
    for i, n in enumerate((72, 76, 79, 84, 88)):
        place(m, bell(note(n), 2.2, .85, 1, 2.6), .11 * i)
    place(m, pad([note(72), note(76), note(79)], 3.0, .22, .5), .3)
    return finish(reverb(m, 2.0, .4))


def star4():
    m = np.zeros(int(SR * 5.2))
    place(m, bell(note(50), 4.2, .8, .5, 1.1), 0)
    place(m, pad([note(62), note(66), note(69), note(73), note(76)], 4.4, .35, .9), .1)
    for i, n in enumerate((74, 78, 81, 85, 88, 93)):  # D major add9 reaching up
        place(m, bell(note(n), 2.8, .8, 1.1, 2.2), .2 + .14 * i)
    for k in range(10):  # shimmer: little high bells scattered after the climb
        place(m, bell(note(rng.choice((93, 97, 100, 105))), 1.2, .22, 1.3, 3.5), 1.2 + rng.uniform(0, 2.4))
    return finish(reverb(m, 3.0, .45))


def star5():
    m = np.zeros(int(SR * 8.0))
    place(m, bell(note(48), 6.0, .9, .5, .8), 0)
    place(m, pad([note(60), note(64), note(67), note(71), note(74), note(79)], 6.5, .45, 1.4), .1)
    for i, n in enumerate((72, 76, 79, 83, 84, 88, 91, 95, 96)):  # a long rising run in C major 9
        place(m, bell(note(n), 3.4, .85, 1.2, 1.9), .25 + .16 * i)
    for k in range(26):  # a rain of high sparks over the whole glory
        place(m, bell(note(rng.choice((96, 100, 103, 105, 108))), 1.4, .2, 1.4, 3.2), 1.4 + rng.uniform(0, 4.4))
    place(m, pad([note(72), note(76), note(79), note(83), note(86)], 4.5, .35, .5), 3.2)  # the second swell
    place(m, bell(note(96), 4.0, .9, 1, 1.4), 3.3)
    return finish(reverb(m, 4.0, .5))


def one_of_a_kind():
    m = np.zeros(int(SR * 3.6))
    for i, n in enumerate((91, 96, 100, 103, 108)):
        place(m, bell(note(n), 2.2, .8, 1.3, 2.4), .1 * i)
    place(m, pad([note(84), note(88), note(91), note(95)], 3.0, .25, .3), 0)
    return finish(reverb(m, 2.4, .45), .75)


def star_note(n):
    def make():
        m = np.zeros(int(SR * 1.6))
        place(m, bell(note(n), 1.5, 1, .9, 3.4), 0)
        return finish(reverb(m, .9, .25), .8)
    return make


NOTES = [("up.note.%d" % (i + 1), "up_note%d" % (i + 1), star_note(n), -11, 85) for i, n in enumerate((72, 76, 79, 84, 88))]

CUES = NOTES + [("up.bell.half", "up_bell_half", half, -9, 130), ("up.bell.2", "up_bell2", star2, -9, 100), ("up.bell.3", "up_bell3", star3, -9, 90),
        ("up.bell.4", "up_bell4", star4, -9, 80), ("up.bell.5", "up_bell5", star5, -8, 70), ("up.bell.5b", "up_bell5b", one_of_a_kind, -9, 75)]


def write(name, x):
    path = os.path.join(SFX, name + "_00.wav")
    with wave.open(path, "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes((np.clip(x, -1, 1) * 32767).astype("<i2").tobytes())
    return path


def main():
    meta_src = open(os.path.join(SFX, "up_star3_00.wav.meta"), encoding="utf-8", newline="").read()
    cues_path = os.path.join(ROOT, "cues.json"); loud_path = os.path.join(ROOT, "loudness.json")
    cues_raw = open(cues_path, encoding="utf-8", newline="").read(); loud_raw = open(loud_path, encoding="utf-8", newline="").read()
    cues = json.loads(cues_raw); loud = json.loads(loud_raw)
    template = next(c for c in cues["cues"] if c["id"] == "up.star3")
    print("cues keys", list(cues.keys()), "loud keys", list(loud.keys()))
    known = {c["id"] for c in cues["cues"]}
    loud_add = []
    for cid, name, fn, gain, prio in CUES:
        if cid in known and os.path.exists(os.path.join(SFX, name + "_00.wav")):
            continue  # already made: keep the existing sound as it is
        x = fn(); p = write(name, x)
        rms = 20 * np.log10(np.sqrt(np.mean(x ** 2)) + 1e-9)
        mp = p + ".meta"
        if not os.path.exists(mp):
            open(mp, "w", encoding="utf-8", newline="").write(re.sub(r"guid: [0-9a-f]{32}", "guid: " + uuid.uuid4().hex, meta_src, count=1))
        if cid not in known:
            c = dict(template); c["id"] = cid; c["gainDb"] = gain; c["priority"] = prio; c["cooldown"] = .2; c["maxVoices"] = 2
            c["clips"] = ["Sfx/" + name + "_00"]
            cues["cues"].append(c)
        loud_add.append((name + "_00", round(float(rms), 2), round(float(-12.07 - rms), 2)))
        print(cid, name, round(len(x) / SR, 2), "s rms", round(float(rms), 1))
    nl = "\r\n" if "\r\n" in cues_raw else "\n"
    open(cues_path, "w", encoding="utf-8", newline="").write(json.dumps(cues, indent=1, ensure_ascii=False).replace("\n", nl) + (nl if cues_raw.endswith("\n") else ""))
    NL = chr(13) + chr(10)
    LF = chr(10)
    nl2 = NL if NL in loud_raw else LF
    body = loud_raw.rstrip()
    idx = body.rindex("]")
    head = body[:idx].rstrip()
    add = ""
    for c, r, g in loud_add:
        if '"' + c + '"' in loud_raw:
            continue
        add += "," + nl2 + "        {" + nl2 + '            "clip": "' + c + '",' + nl2 + '            "rmsDb": ' + str(r) + "," + nl2 + '            "gainDb": ' + str(g) + nl2 + "        }"
    open(loud_path, "w", encoding="utf-8", newline="").write(head + add + nl2 + "    ]" + nl2 + "}" + (nl2 if loud_raw.endswith(LF) else ""))


if __name__ == "__main__":
    main()
