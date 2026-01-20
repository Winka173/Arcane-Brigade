"""Renders listening previews into PreviewExports/Audio (soundfile/libsndfile Vorbis) (offline mixes, NOT in-game capture).
 - one mix per composition: intensity stem fades in across the loop, so adaptive layering is audible
 - sfx_reel: every cue's first variation in manifest order, 0.3 s apart, with a spoken-free label list in sfx_reel.txt
Run after build_music.py / build_sfx.py."""
import json, os, sys, wave
import numpy as np
import soundfile as sf
sys.path.insert(0, os.path.dirname(__file__))
from synth import SR
from build_music import write_ogg
ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
A = os.path.join(ROOT, 'Assets', 'ArcaneBrigade', 'Resources', 'Audio', 'Arcane')
OUT = os.path.join(ROOT, 'PreviewExports', 'Audio'); os.makedirs(OUT, exist_ok=True)

def ogg(x, name):
    x = np.asarray(x, dtype=np.float64)
    write_ogg(os.path.join(OUT, name), x if x.ndim == 2 else np.stack([x, x], 1))

def dec(p):
    return sf.read(p, dtype='float32', always_2d=True)[0]

playlist = []
for pc in json.load(open(os.path.join(A, 'Music', 'music.json')))['pieces']:
    base, rhy, mel, inten = [dec(os.path.join(A, s + '.ogg')) for s in pc['stems']]
    n = len(base); ramp = np.clip((np.arange(n) / n - 0.35) / 0.4, 0, 1)[:, None]
    mix = (base + rhy * 0.9 + mel + inten * ramp) * 0.55
    mix = np.tile(mix, (1, 1))
    name = 'music_%s.ogg' % pc['id']
    ogg(mix / max(1, np.abs(mix).max() / 0.89), name)
    playlist.append((name, '%s - %s (%d BPM, %.0fs, scene %s; intensity layer enters at 35%%)' % (pc['id'], pc['title'], pc['bpm'], pc['loopSeconds'], ','.join(pc['scenes']))))

man = json.load(open(os.path.join(A, 'cues.json')))['cues']
gap = np.zeros(int(0.3 * SR)); parts = []; labels = []; t = 0.0
for c in man:
    if c['loop']: continue
    with wave.open(os.path.join(A, c['clips'][0] + '.wav')) as w:
        x = np.frombuffer(w.readframes(w.getnframes()), '<i2').astype(np.float32) / 32768
    x = x * (10 ** (c['gainDb'] / 20)) * 4.0   # approximate in-game relative loudness (bus trim ignored)
    labels.append('%7.1fs  %s  [%s]' % (t, c['id'], c['bus'])); t += (len(x) + len(gap)) / SR
    parts += [x, gap]
reel = np.concatenate(parts); reel = reel / max(1, np.abs(reel).max() / 0.89)
ogg(reel, 'sfx_reel.ogg')
open(os.path.join(OUT, 'sfx_reel.txt'), 'w').write('\n'.join(labels) + '\n')
playlist.append(('sfx_reel.ogg', 'Every cue (first variation) in manifest order; see sfx_reel.txt for timestamps'))
open(os.path.join(OUT, 'playlist.m3u'), 'w').write('#EXTM3U\n' + ''.join('#EXTINF:-1,%s\n%s\n' % (d, n) for n, d in playlist))
print('previews:', len(playlist))
