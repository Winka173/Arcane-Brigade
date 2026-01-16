"""Objective checks only (NOT a listening review): peak/clipping, DC, silence, duration, near-duplicates,
brightness, loop-seam continuity, manifest/file consistency. Exit 1 on hard failures."""
import glob, hashlib, json, os, sys, wave
import numpy as np
ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
A = os.path.join(ROOT, 'Assets', 'ArcaneBrigade', 'Resources', 'Audio', 'Arcane')

def load(p):
    with wave.open(p) as w:
        x = np.frombuffer(w.readframes(w.getnframes()), '<i2').astype(np.float32) / 32768
        return x.reshape(-1, w.getnchannels()), w.getframerate()

def centroid(x, sr):
    m = np.abs(np.fft.rfft(x * np.hanning(len(x)))); f = np.fft.rfftfreq(len(x), 1 / sr)
    return float((m * f).sum() / (m.sum() + 1e-9))

fails, warns, rows = [], [], []
man = json.load(open(os.path.join(A, 'cues.json')))
seen = {}
for c in man['cues']:
    for rel in c['clips']:
        p = os.path.join(A, rel + '.wav')
        if not os.path.exists(p): fails.append('missing ' + rel); continue
        x, sr = load(p); m = x.mean(1)
        pk = float(np.abs(x).max()); dur = len(x) / sr
        h = hashlib.sha1(x.tobytes()).hexdigest()
        if h in seen: fails.append('duplicate content %s == %s' % (rel, seen[h]))
        seen[h] = rel
        if pk >= 0.999: fails.append('clipping ' + rel)
        if abs(m.mean()) > 0.01: warns.append('DC offset %s %.3f' % (rel, m.mean()))
        if np.sqrt((m ** 2).mean()) < 0.003: fails.append('near-silent ' + rel)
        if abs(m[0]) > 0.02 or abs(m[-1]) > 0.02:
            if not c['loop']: warns.append('edge click risk ' + rel)
        cen = centroid(m, sr)
        if cen > 6500 and c['bus'] in ('Combat', 'Foley'): warns.append('very bright (%.0f Hz centroid) %s' % (cen, rel))
        if c['loop']:
            d = abs(float(m[0]) - float(m[-1])); step = float(np.abs(np.diff(m)).mean())
            if d > 6 * step + 0.01: fails.append('loop seam jump %.3f %s' % (d, rel))
        if dur > 4.5 and not c['loop']: warns.append('long one-shot %.1fs %s' % (dur, rel))
        rows.append((rel, dur, pk))
print('clips', len(rows), 'cues', len(man['cues']), 'total seconds %.0f' % sum(r[1] for r in rows))
print('bytes', sum(os.path.getsize(os.path.join(A, r[0] + '.wav')) for r in rows))

# ---- music: decode each stem with ffmpeg, check identical length == manifest, peak, sum headroom, loop-seam continuity
import subprocess
mp = os.path.join(A, 'Music', '..', 'Music', 'music.json')
if os.path.exists(mp):
    for pc in json.load(open(mp))['pieces']:
        dec = []
        for st in pc['stems']:
            raw = subprocess.run(['ffmpeg', '-v', 'error', '-i', os.path.join(A, st + '.ogg'), '-f', 's16le', '-ac', '2', '-ar', '44100', '-'], capture_output=True).stdout
            x = np.frombuffer(raw, '<i2').astype(np.float32).reshape(-1, 2) / 32768
            dec.append(x)
            if abs(len(x) - pc['loopSamples']) > 1: fails.append('%s length %d != manifest %d' % (st, len(x), pc['loopSamples']))
            seam = float(np.abs(x[0] - x[-1]).max()); step = float(np.abs(np.diff(x, axis=0)).mean()) + 1e-6
            if seam > 40 * step + 0.02: fails.append('music loop seam %.3f (mean step %.4f) %s' % (seam, step, st))
            if np.abs(x).max() >= 0.99: fails.append('music clipping ' + st)
        n = min(len(d) for d in dec); mix = sum(d[:n] for d in dec)
        print('music %-22s %5.1fs stems=%d  sum peak %.2f  sum rms %.1f dB' % (pc['id'], pc['loopSeconds'], len(dec), np.abs(mix).max(), 20 * np.log10(np.sqrt((mix ** 2).mean()) + 1e-9)))
        if np.abs(mix).max() > 1.2: warns.append('stem sum peak %.2f may need runtime headroom %s' % (np.abs(mix).max(), pc['id']))
for w in warns: print('WARN', w)
for f in fails: print('FAIL', f)
sys.exit(1 if fails else 0)
