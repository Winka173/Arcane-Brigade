"""Weight pass for the combat sounds (requirement 10/11/2026: hits and deaths sounded high and without force).

The shipped clips were either all air (metal / blade / glass / cast: 4-10 kHz and nothing below) or all sub (paper / wax /
wood: 50-120 Hz, which a phone speaker cannot play). This rebuilds each clip as three layers: a short click (1.5-4.5 kHz), a
body thump (a sine dropping 170 -> 75 Hz, driven through tanh so its harmonics sit at 150-600 Hz where phones can play them)
and the old clip, rolled off above 5 kHz when it was brittle. Swings and casts keep their air but gain a mid body and lose the
shrill top. The result is limited to -2 dBFS.

It always reads from the originals kept in <backup> (made on the first run), so it can be rerun with new settings.
    python Tools/Audio/punch_pass.py            # rewrites the combat clips in Resources/Audio/Arcane/Sfx
"""
import json, os, shutil, sys
import numpy as np
from scipy.io import wavfile
from scipy.signal import butter, sosfilt

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
ARC = os.path.join(ROOT, 'Assets', 'ArcaneBrigade', 'Resources', 'Audio', 'Arcane')
BACKUP = 'C:/ABWork/audio/orig'
TARGET_PEAK = 10 ** (-2 / 20)

def kind_of(cue_id):
    if cue_id in ('sig.alarm.boom', 'sig.stamp', 'wpn.spring.slam'): return 'sub'
    if cue_id.startswith(('cmb.hit.', 'fx.knockback', 'fx.hazard.hit')) or cue_id == 'cmb.crit': return 'hit'
    if cue_id.startswith('sig.') and cue_id not in ('sig.alarm', 'sig.bell', 'sig.musicbox', 'sig.tea', 'sig.camera'): return 'hit'
    if cue_id == 'enemy.die.large': return 'bigdie'
    if cue_id.startswith('enemy.die.'): return 'die'
    if cue_id.startswith(('cmb.swing.', 'wpn.cast.', 'fx.reaction.steam', 'enemy.lunge', 'enemy.spit.fire', 'wpn.launcher', 'wpn.bow.draw', 'wpn.pierce')): return 'air'
    if cue_id.startswith(('boss.', 'hero.', 'enemy.', 'wpn.', 'fx.', 'sig.', 'plr.')): return 'auto'
    return None

def band(x, sr, lo=None, hi=None, order=2):
    if lo and hi: sos = butter(order, [lo, hi], 'bandpass', fs=sr, output='sos')
    elif hi: sos = butter(order, hi, 'lowpass', fs=sr, output='sos')
    else: sos = butter(order, lo, 'highpass', fs=sr, output='sos')
    return sosfilt(sos, x)

def centroid(x, sr):
    spec = np.abs(np.fft.rfft(x * np.hanning(len(x)))) ** 2
    f = np.fft.rfftfreq(len(x), 1 / sr)
    return float((f * spec).sum() / max(spec.sum(), 1e-12))

def thump(n, sr, length, f0=75.0, f1=170.0, tau=0.02, decay=0.05):
    t = np.arange(min(n, int(length * sr))) / sr
    freq = f0 + (f1 - f0) * np.exp(-t / tau)
    phase = 2 * np.pi * np.cumsum(freq) / sr
    env = np.exp(-t / decay) * (1 - np.exp(-t / 0.0015))
    s = np.tanh(2.4 * np.sin(phase) * env) * env
    return np.pad(s, (0, n - len(s)))

def click(n, sr, seed):
    rng = np.random.default_rng(seed)
    t = np.arange(min(n, int(0.012 * sr))) / sr
    s = band(rng.standard_normal(len(t)), sr, 1500, 4500) * np.exp(-t / 0.003)
    return np.pad(s, (0, n - len(s)))

def envelope(x, sr):
    return np.maximum(band(np.abs(x), sr, hi=40, order=1), 0)

def process(x, sr, kind, seed):
    n = len(x)
    peak0 = np.abs(x).max() or 1.0
    x = x / peak0
    cent = centroid(x, sr)
    on = int(np.argmax(np.abs(x) > 0.15))
    if kind == 'auto':
        low = (np.abs(np.fft.rfft(x * np.hanning(n))) ** 2)[np.fft.rfftfreq(n, 1 / sr) < 150].sum() / max((np.abs(np.fft.rfft(x * np.hanning(n))) ** 2).sum(), 1e-12)
        if cent > 2500: kind = 'air'
        elif cent < 150 and low > 0.85: kind = 'sub'
        else: return None            # already has a body: left as it is
    if kind == 'air':
        keep = band(x, sr, hi=3600) * 0.8
        body = band(np.random.default_rng(seed).standard_normal(n), sr, 260, 900) * envelope(x, sr) * 2.2
        out = keep + body * 0.8
    elif kind == 'sub':
        low = band(x, sr, hi=400)
        out = x + 0.55 * (np.tanh(3.0 * low) - low) + 0.25 * click(n, sr, seed)
    else:
        big = kind == 'bigdie'
        if cent > 2500: keep = band(x, sr, hi=3200) * 0.6
        elif cent < 400:
            # a sub-only clip: the phone cannot play it, so keep only what is above 110 Hz and give the rest its harmonics
            low = band(x, sr, hi=300)
            keep = band(x, sr, lo=110) * 0.5 + 0.9 * (np.tanh(4.0 * low) - low)
        else: keep = band(x, sr, hi=5000) * 0.85
        th = np.roll(thump(n, sr, 0.22 if big else 0.14, f0=95, f1=220, decay=0.08 if big else 0.05), on)
        mid = np.roll(thump(n, sr, 0.1, f0=180, f1=300, tau=0.015, decay=0.035), on)
        out = keep + th * (0.8 if big else 0.55) + mid * 0.6 + np.roll(click(n, sr, seed), on) * 0.5
        out = band(out, sr, lo=45)
    peak = np.abs(out).max() or 1.0
    out = np.tanh(out / peak * 1.25) / np.tanh(1.25)   # soft limiter: the peaks squash, the body stays
    return out / (np.abs(out).max() or 1.0) * TARGET_PEAK

def load(path):
    sr, x = wavfile.read(path)
    dtype = x.dtype
    x = x.astype(np.float64)
    if dtype == np.int16: x /= 32768.0
    elif dtype == np.int32: x /= 2147483648.0
    return sr, x

def render(x, sr, kind, seed, pitch=1.0):
    from scipy.signal import resample_poly
    chans = [x] if x.ndim == 1 else [x[:, c] for c in range(x.shape[1])]
    if pitch != 1.0:
        num = int(round(pitch * 100)); chans = [resample_poly(c, 100, num) for c in chans]   # faster = higher and shorter
    outs = [process(c, sr, kind, seed) for c in chans]
    if any(o is None for o in outs): return None
    return np.stack(outs, axis=1) if len(outs) > 1 else outs[0]

def save(path, sr, y):
    wavfile.write(path, sr, (np.clip(y, -1, 1) * 32767).astype(np.int16))

def backup(clip):
    path = os.path.join(ARC, clip + '.wav'); original = os.path.join(BACKUP, clip + '.wav')
    if os.path.exists(path) and not os.path.exists(original):
        os.makedirs(os.path.dirname(original), exist_ok=True); shutil.copy2(path, original)
    return path, original

def new_meta(source_wav, new_wav):
    import uuid
    text = open(source_wav + '.meta', encoding='utf-8').read()
    import re
    text = re.sub(r'guid: \w+', 'guid: ' + uuid.uuid4().hex, text, count=1)
    open(new_wav + '.meta', 'w', encoding='utf-8', newline='').write(text)

def main():
    import re
    cue_path = os.path.join(ARC, 'cues.json')
    raw = open(cue_path, encoding='utf-8', newline='').read()
    m = re.match(r'\{(\r?\n)( +)', raw); nl, ind = m.group(1), len(m.group(2))
    doc = json.loads(raw)
    done = kept = added = 0
    for cue in doc['cues']:
        kind = kind_of(cue['id'])
        if not kind: continue
        for index, clip in enumerate(cue['clips']):
            path, original = backup(clip)
            if not os.path.exists(original): continue
            sr, x = load(original)
            y = render(x, sr, kind, index * 31 + 7)
            if y is None: shutil.copy2(original, path); kept += 1; continue
            save(path, sr, y); done += 1
        # more takes, so a swarm of the same blow never repeats the same four sounds (6-8 per cue, a little different in pitch)
        if kind in ('hit', 'die', 'bigdie', 'air') and len(cue['clips']) < 8:
            base = [c for c in cue['clips'] if '_v' not in os.path.basename(c)]
            for v in (1, 2):
                name = base[0] + '_v' + str(v)
                if name in cue['clips']: continue
                _, source = backup(base[(v * 2) % len(base)])
                sr, x = load(source)
                y = render(x, sr, kind, 1000 + v * 17, 1.07 if v == 1 else 0.94)
                if y is None: continue
                out = os.path.join(ARC, name + '.wav'); save(out, sr, y)
                new_meta(os.path.join(ARC, base[0] + '.wav'), out)
                cue['clips'].append(name); added += 1
    open(cue_path, 'w', encoding='utf-8', newline='').write(json.dumps(doc, indent=ind, ensure_ascii=False).replace('\n', nl) + (nl if raw.endswith('\n') else ''))
    print('clips rewritten', done, 'left as they were', kept, 'new takes', added)

if __name__ == '__main__':
    main()
