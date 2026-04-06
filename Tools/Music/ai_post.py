"""A/B helper: split the music sketch tool candidates into the game's four stems with Demucs (htdemucs_6s) and write loudness-matched previews.

base = bass + other, rhythm = drums + guitar (plucked), melody = piano + vocals (lead-ish leftovers), intensity = silence.
Usage: python ai_post.py <demucs_python> <ffmpeg> <ai_raw_dir> <out_dir> <preview_dir>
"""
import glob
import json
import os
import subprocess
import sys

import numpy as np
import pyloudnorm
import soundfile as sf

PY, FFMPEG, RAW, OUT, PREV = sys.argv[1:6]
SR = 44100
SCENE = {'menu_curtain': [1, 0.85, 1, 0], 'combat_toys': [1, 1, 0, 0], 'boss_clown': [1, 1, 0, 0]}


def loud(x):
    return pyloudnorm.Meter(SR).integrated_loudness(x)


def mp3(x, path):
    tmp = path + '.wav'
    sf.write(tmp, np.clip(x, -1, 1).astype(np.float32), SR, subtype='FLOAT')
    subprocess.run([FFMPEG, '-y', '-v', 'error', '-i', tmp, '-c:a', 'libmp3lame', '-b:a', '128k', path], check=True)
    os.remove(tmp)


def main():
    os.makedirs(OUT, exist_ok=True)
    os.makedirs(PREV, exist_ok=True)
    report = []
    for d in sorted(glob.glob(os.path.join(RAW, '*__*'))):
        piece, model = os.path.basename(d).split('__')
        wavs = sorted(glob.glob(os.path.join(d, '**', '*.wav'), recursive=True))
        for ci, wav in enumerate(wavs):
            tag = f'{piece}__{model}__{ci}'
            sep = os.path.join(OUT, 'demucs')
            conv = os.path.join(OUT, 'conv', tag + '.wav')
            os.makedirs(os.path.dirname(conv), exist_ok=True)
            subprocess.run([FFMPEG, '-y', '-v', 'error', '-i', wav, '-ar', str(SR), '-ac', '2', '-c:a', 'pcm_f32le', conv], check=True)
            wav = conv
            subprocess.run([PY, '-m', 'demucs', '-n', 'htdemucs_6s', '-o', sep, '--filename', tag + '/{stem}.{ext}', wav], check=True,
                           stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
            st = {}
            for name in ('drums', 'bass', 'other', 'vocals', 'guitar', 'piano'):
                x, sr = sf.read(os.path.join(sep, 'htdemucs_6s', tag, name + '.wav'), always_2d=True)
                if sr != SR:
                    raise SystemExit(f'unexpected rate {sr}')
                st[name] = x
            full, _ = sf.read(wav, always_2d=True)
            n = min(len(full), *(len(v) for v in st.values()))
            st = {k: v[:n] for k, v in st.items()}
            full = full[:n]
            stems = {'base': st['bass'] + st['other'], 'rhythm': st['drums'] + st['guitar'], 'melody': st['piano'] + st['vocals'],
                     'intensity': np.zeros((n, 2))}
            e_full = (full ** 2).sum()
            share = {k: round(float((v ** 2).sum() / e_full), 3) for k, v in st.items()}
            resid = full - sum(st.values())
            w = SCENE[piece]
            scene = sum(stems[k] * w[i] for i, k in enumerate(['base', 'rhythm', 'melody', 'intensity']))
            g = 10 ** ((-18 - loud(scene)) / 20)
            gf = 10 ** ((-18 - loud(full)) / 20)
            name = f'{piece}_ai' + ('' if ci == 0 else f'_alt{ci}') + ('' if 'turbo' in model else '_' + model.replace('acestep-v15-', ''))
            mp3(full * gf, os.path.join(PREV, name + '.mp3'))
            if piece.startswith(('combat', 'boss')):
                mp3((stems['base'] + stems['rhythm']) * g, os.path.join(PREV, name + '_combatmix.mp3'))
            report.append(dict(file=name, source=wav, seconds=round(n / SR, 1), full_lufs=round(loud(full), 1),
                               energy_share=share, residual_db=round(10 * np.log10((resid ** 2).sum() / e_full + 1e-12), 1)))
            print(name, report[-1]['seconds'], 's', share, flush=True)
    with open(os.path.join(OUT, 'ai_report.json'), 'w') as f:
        json.dump(report, f, indent=1)


if __name__ == '__main__':
    main()
