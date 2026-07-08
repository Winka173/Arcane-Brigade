"""Render the storybook soundtrack: every Part -> MIDI -> FluidSynth (GeneralUser GS) -> folded loop -> per-part EQ / level /
pan -> stem bus + circular convolution room -> scene-mix loudness -> 4 sample-aligned OGG Vorbis stems per piece.

Usage: python render.py [--out <Music dir>] [--preview <dir>] [--json] [piece_id ...]
Env:   AB_FLUIDSYNTH (fluidsynth.exe), AB_SF2 (GeneralUser-GS.sf2), AB_FFMPEG (ffmpeg.exe), AB_WORK (scratch dir), AB_TRACKS (tracks | tracks2)
"""
import argparse
import json
import os
import subprocess
import sys
from multiprocessing import Pool

import numpy as np
import pyloudnorm
import soundfile as sf
from scipy import signal
from scipy.ndimage import maximum_filter1d, minimum_filter1d, uniform_filter1d

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from compose import write_part  # noqa: E402
import importlib  # noqa: E402

# AB_TRACKS picks the piece list: 'tracks' (the first sixteen, default) or 'tracks2' (the second set)
tracks = importlib.import_module(os.environ.get('AB_TRACKS', 'tracks'))

SR = 44100
HERE = os.path.dirname(os.path.abspath(__file__))
FS = os.environ.get('AB_FLUIDSYNTH', 'fluidsynth')
SF2 = os.environ.get('AB_SF2', 'GeneralUser-GS.sf2')
FFMPEG = os.environ.get('AB_FFMPEG', 'ffmpeg')
WORK = os.environ.get('AB_WORK', os.path.join(HERE, 'work'))
STEMS = ['base', 'rhythm', 'melody', 'intensity']
TARGET_LUFS = -18.0
TAIL_SECONDS = 6.0
# stem levels of the scene mix that is normalised to TARGET_LUFS (mirrors Runtime/Presentation/MusicArrangement.cs)
SCENE_MIX = {'menu': [1, 0.85, 1, 0], 'hub': [1, 0.85, 1, 0], 'select': [1, 0.7, 1, 0], 'shop_gold': [1, 0.9, 1, 0.75], 'shop_dream': [1, 0.8, 1, 0.7]}
COMBAT_MIX = [1, 1, 0, 0]


def scene_mix(song):
    return SCENE_MIX.get(song.scenes[0], COMBAT_MIX)


def fold(x, n):
    y = x[:n].copy()
    if len(y) < n:
        y = np.vstack([y, np.zeros((n - len(y), 2))])
    tail = x[n:]
    for k in range(0, len(tail), n):
        chunk = tail[k:k + n]
        y[:len(chunk)] += chunk
    return y


def circ_sos(x, sos):
    pad = min(len(x), SR * 2)
    y = signal.sosfilt(sos, np.vstack([x[-pad:], x]), axis=0)
    return y[pad:]


def make_ir(seed, rt60, length=3.0, predelay=0.02):
    rng = np.random.default_rng(seed)
    n = int(length * SR)
    t = np.arange(n) / SR
    ir = np.zeros((n, 2))
    bands = [(None, 400, rt60 * 1.15), (400, 2500, rt60), (2500, 9000, rt60 * 0.6)]
    for lo, hi, rt in bands:
        noise = rng.standard_normal((n, 2))
        if lo is None:
            sos = signal.butter(2, hi, 'lowpass', fs=SR, output='sos')
        else:
            sos = signal.butter(2, [lo, hi], 'bandpass', fs=SR, output='sos')
        ir += signal.sosfilt(sos, noise, axis=0) * np.exp(-6.91 * t / rt)[:, None]
    for d, g in ((0.011, 0.5), (0.017, 0.4), (0.023, 0.33), (0.031, 0.28), (0.043, 0.2)):
        k = int(d * SR)
        ir[k, 0] += g * (1 if rng.random() > 0.5 else -1)
        ir[k + int(0.003 * SR), 1] += g * (1 if rng.random() > 0.5 else -1)
    ir[: int(0.006 * SR)] *= np.linspace(0, 1, int(0.006 * SR))[:, None]
    ir = np.vstack([np.zeros((int(predelay * SR), 2)), ir])
    ir /= np.sqrt((ir ** 2).sum(axis=0).mean())
    return ir


def limiter_gain(x, ceiling, window_ms=4.0):
    """Look-ahead peak limiter gain curve (circular, so loops stay seamless)."""
    w = max(3, int(window_ms * SR / 1000))
    env = maximum_filter1d(np.abs(x).max(axis=1), size=w, mode='wrap')
    g = np.minimum(1.0, ceiling / np.maximum(env, 1e-9))
    g = minimum_filter1d(g, size=2 * w, mode='wrap')
    return uniform_filter1d(g, size=w, mode='wrap')[:, None]


PART_CREST_DB = 20.0
CEILING = 0.89  # -1 dBFS on the sum of all four stems
MAX_BUS_GR_DB = 4.0


def circ_conv(x, ir):
    n = len(x)
    h = np.zeros((n, 2))
    m = min(n, len(ir))
    h[:m] = ir[:m]
    return np.fft.irfft(np.fft.rfft(x, axis=0) * np.fft.rfft(h, axis=0), n=n, axis=0)


def pan(x, p):
    if not p:
        return x
    gl = np.cos((p + 1) * np.pi / 4) * np.sqrt(2)
    gr = np.sin((p + 1) * np.pi / 4) * np.sqrt(2)
    y = x.copy()
    y[:, 0] *= min(1.0, gl)
    y[:, 1] *= min(1.0, gr)
    if p < 0:
        y[:, 0] += x[:, 1] * (1 - gr) * 0.5
    elif p > 0:
        y[:, 1] += x[:, 0] * (1 - gl) * 0.5
    return y


def render_part(song, part, wdir, n):
    mid = os.path.join(wdir, part.name + '.mid')
    wav = os.path.join(wdir, part.name + '.wav')
    tail_beats = TAIL_SECONDS * song.qbpm / 60
    write_part(song, part, mid, tail_beats)
    subprocess.run([FS, '-ni', '-q', '-R', '0', '-C', '0', '-g', '0.5', '-r', str(SR), '-O', 'float', '-T', 'wav', '-F', wav, SF2, mid],
                   check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    x, sr = sf.read(wav, always_2d=True)
    assert sr == SR
    return fold(x, n)


def loud(x):
    try:
        return pyloudnorm.Meter(SR).integrated_loudness(x)
    except Exception:
        return float('-inf')


def build(song, out_dir, preview_dir):
    wdir = os.path.join(WORK, song.id)
    os.makedirs(wdir, exist_ok=True)
    n = int(round(song.loop_seconds * SR))
    stems = {k: np.zeros((n, 2)) for k in STEMS}
    sends = {k: np.zeros((n, 2)) for k in STEMS}
    tone = signal.butter(1, 13000, 'lowpass', fs=SR, output='sos')
    report = []
    for part in song.parts:
        if not part.notes:
            continue
        x = render_part(song, part, wdir, n)
        x = circ_sos(x, tone)
        if part.hp:
            x = circ_sos(x, signal.butter(2, part.hp, 'highpass', fs=SR, output='sos'))
        if part.lp:
            x = circ_sos(x, signal.butter(2, part.lp, 'lowpass', fs=SR, output='sos'))
        lv = loud(x)
        if np.isfinite(lv):
            x *= 10 ** ((part.level - lv) / 20)
            x *= limiter_gain(x, 10 ** ((part.level + PART_CREST_DB) / 20))
        x = pan(x, part.pan)
        stems[part.stem] += x
        sends[part.stem] += x * part.send
        report.append((part.stem, part.name, round(lv, 1), round(20 * np.log10(np.abs(x).max() + 1e-9) - part.level, 1)))
    rt = 2.1 if song.qbpm <= 84 else 1.7 if not song.scenes[0].startswith(('combat', 'endless')) else 1.35
    ir = make_ir(song.seed, rt)
    for k in STEMS:
        stems[k] = stems[k] + circ_conv(sends[k], ir) * 0.9
        stems[k] = circ_sos(stems[k], signal.butter(2, 28, 'highpass', fs=SR, output='sos'))
    w = scene_mix(song)
    mix = sum(stems[k] * w[i] for i, k in enumerate(STEMS))
    g = 10 ** ((TARGET_LUFS - loud(mix)) / 20)
    full = sum(stems.values()) * g
    peak = np.abs(full).max()
    if peak > CEILING * 10 ** (MAX_BUS_GR_DB / 20):
        g *= CEILING * 10 ** (MAX_BUS_GR_DB / 20) / peak
    lim = limiter_gain(sum(stems.values()) * g, CEILING)
    for k in STEMS:
        stems[k] *= g * lim
    full = sum(stems.values())
    combat = stems['base'] + stems['rhythm']
    scene = sum(stems[k] * w[i] for i, k in enumerate(STEMS))
    info = dict(id=song.id, title=song.title, key=song.key, bpm=song.json_bpm, beatsPerBar=song.json_bpb, bars=song.bars,
                loopSeconds=n / SR, loopSamples=n, scene_lufs=round(loud(scene), 2), full_lufs=round(loud(full), 2),
                combat_lufs=round(loud(combat), 2), full_peak_db=round(20 * np.log10(np.abs(full).max()), 2),
                instruments=sorted({p.name for p in song.parts}), desc=song.desc, parts=report)
    if out_dir:
        d = os.path.join(out_dir, song.id)
        os.makedirs(d, exist_ok=True)
        for k in STEMS:
            tmp = os.path.join(wdir, f'{song.id}_{k}.wav')
            sf.write(tmp, stems[k].astype(np.float32), SR, subtype='FLOAT')
            subprocess.run([FFMPEG, '-y', '-v', 'error', '-i', tmp, '-c:a', 'libvorbis', '-q:a', '6', os.path.join(d, f'{song.id}_{k}.ogg')], check=True)
    if preview_dir:
        os.makedirs(preview_dir, exist_ok=True)
        for tag, sig in (('full', full), ('combat', combat)):
            tmp = os.path.join(wdir, f'prev_{tag}.wav')
            sf.write(tmp, np.clip(sig, -1, 1).astype(np.float32), SR, subtype='FLOAT')
            subprocess.run([FFMPEG, '-y', '-v', 'error', '-i', tmp, '-c:a', 'libmp3lame', '-b:a', '128k',
                            os.path.join(preview_dir, f'{song.id}_{tag}.mp3')], check=True)
    return info


def _job(args):
    fn, out_dir, preview_dir = args
    return build(getattr(tracks, fn)(), out_dir, preview_dir)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--out')
    ap.add_argument('--preview')
    ap.add_argument('--report')
    ap.add_argument('-j', type=int, default=6)
    ap.add_argument('ids', nargs='*')
    a = ap.parse_args()
    names = [f.__name__ for f in tracks.ALL if not a.ids or f.__name__ in a.ids]
    with Pool(min(a.j, len(names))) as pool:
        infos = pool.map(_job, [(nm, a.out, a.preview) for nm in names])
    for i in infos:
        print(f"{i['id']:20s} {i['key']:24s} bpm {i['bpm']:>4} x{i['beatsPerBar']} bars {i['bars']} {i['loopSeconds']:.1f}s "
              f"scene {i['scene_lufs']} full {i['full_lufs']} combat {i['combat_lufs']} peak {i['full_peak_db']}")
    if a.report:
        with open(a.report, 'w', encoding='utf-8') as f:
            json.dump(infos, f, indent=1)


if __name__ == '__main__':
    main()
