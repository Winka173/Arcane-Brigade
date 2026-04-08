"""Build SFX v6: render every design to Resources/Audio/Arcane/Sfx/<name>.wav (44.1 kHz mono 16-bit, with a Unity .meta
matching the library's SFX import settings) and chained MP3 previews.
Usage: python sfx_build.py <Sfx dir> <preview dir> <ffmpeg> [name ...]"""
import hashlib
import os
import subprocess
import sys
import zlib

import numpy as np
import soundfile as sf

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sfx_lib import SR, finish, N  # noqa: E402
from sfx_designs import D  # noqa: E402

META = """fileFormatVersion: 2
guid: {guid}
AudioImporter:
  externalObjects: {{}}
  serializedVersion: 8
  defaultSettings:
    serializedVersion: 2
    loadType: 1
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
  userData: Arcane SFX v6; see Docs/Music_Credits.md
  assetBundleName: 
  assetBundleVariant: 
"""


def render(name):
    fn, dur, wet, peak = D[name]
    rng = np.random.default_rng(zlib.crc32(name.encode()))
    return finish(fn(rng), dur, wet, peak)


def main():
    out, prev, ffmpeg = sys.argv[1:4]
    names = sys.argv[4:] or list(D)
    os.makedirs(prev, exist_ok=True)
    clips = {}
    for nm in names:
        y = render(nm)
        clips[nm] = y
        path = os.path.join(out, nm + '.wav')
        sf.write(path, y, SR, subtype='PCM_16')
        meta = path + '.meta'
        if not os.path.exists(meta):
            with open(meta, 'w', newline='\n') as f:
                f.write(META.format(guid=hashlib.md5(('arcane-sfx-v6/' + nm).encode()).hexdigest()))
        print(f'{nm:42s} {len(y) / SR:.2f}s peak {20 * np.log10(np.abs(y).max()):.1f}', flush=True)
    groups = {}
    for nm in clips:
        key = nm.rsplit('_', 1)[0] if nm.startswith('sfx_') else nm.rsplit('_', 1)[0]
        groups.setdefault(key, []).append(nm)
    order = ['entrance', 'windup', 'hit', 'phase', 'defeat']
    for key, members in groups.items():
        members.sort(key=lambda m: order.index(m.rsplit('_', 1)[1]) if m.startswith('boss_') else m)
        gap = np.zeros(N(0.5 if key.startswith('boss_') else 0.3))
        chain = np.concatenate([np.concatenate([clips[m], gap]) for m in members])
        tmp = os.path.join(prev, key + '.wav')
        sf.write(tmp, chain, SR, subtype='PCM_16')
        subprocess.run([ffmpeg, '-y', '-v', 'error', '-i', tmp, '-c:a', 'libmp3lame', '-b:a', '128k', os.path.join(prev, key + '.mp3')], check=True)
        os.remove(tmp)


if __name__ == '__main__':
    main()
