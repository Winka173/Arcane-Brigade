"""Linked, circular transient control applied equally to all four stems.

Preserves stereo image, bar/sample alignment and adaptive balance. Offline
lookahead is circular so neither gain nor reverb has a reset at the loop seam.
Source masters are 24-bit; a hash ledger makes mastering idempotent.
"""
import hashlib
import json
from pathlib import Path

import numpy as np
from scipy.ndimage import maximum_filter1d, uniform_filter1d
import soundfile as sf

from toybox_engine import SR, ROOT, CACHE
from toybox_music import STEMS, encode_vorbis

P=ROOT/'PreviewExports/Audio/Toybox'


def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()


def repair_boundary(x):
    """A 3 ms stereo Hermite splice across sample zero, with no silence/length change.

    Sampled voices can have different sub-block phase at the two render cycles.
    Only this tiny circular neighbourhood is interpolated; the bar grid, attack
    positions and all other samples are retained. Tangents are bounded to avoid
    high-frequency endpoint slopes producing a polynomial overshoot.
    """
    h=round(SR*.0015)
    indices=np.concatenate((np.arange(len(x)-h,len(x)),np.arange(h)))
    left=x[-h-1]; right=x[h]
    width=len(indices)+1
    m0=(x[-h-1]-x[-h-2])*width
    m1=(x[h+1]-x[h])*width
    limit=2*np.abs(right-left)+.08
    m0=np.clip(m0,-limit,limit); m1=np.clip(m1,-limit,limit)
    t=(np.arange(1,width)/width)[:,None]
    x[indices]=(2*t**3-3*t**2+1)*left+(t**3-2*t**2+t)*m0+(-2*t**3+3*t**2)*right+(t**3-t**2)*m1
    return x


def main():
    stage=CACHE/'toybox'
    pieces=json.loads((stage/'Music/music.json').read_text())['pieces']
    ledgerpath=stage/'mastering.json'
    ledger=json.loads(ledgerpath.read_text()) if ledgerpath.exists() else {}
    scores=json.loads((stage/'scores.json').read_text())
    for piece in pieces:
        pid=piece['id']; folder=P/'Masters'/pid
        paths=[folder/f'{pid}_{st}.wav' for st in STEMS]
        hashes=[sha(p) for p in paths]
        if pid in ledger and ledger[pid].get('version')==2 and ledger[pid]['outputHashes']==hashes:
            print('MASTER unchanged',pid,flush=True); continue
        bufs=[repair_boundary(sf.read(str(p),always_2d=True)[0]) for p in paths]
        mix=sum(bufs)
        pre_rms=np.sqrt(np.mean(mix*mix))
        scale=10**(-17.25/20)/pre_rms
        # A short 16 ms peak hold followed by a 4 ms smoothing window provides
        # lookahead and smooth release without changing the beat or note timings.
        peak=np.max(np.abs(mix*scale),axis=1)
        envelope=maximum_filter1d(peak,size=round(SR*.016)|1,mode='wrap')
        envelope=uniform_filter1d(envelope,size=round(SR*.004)|1,mode='wrap')
        gain=np.minimum(1.,.72/np.maximum(envelope,1e-12))
        mastered=[x*scale*gain[:,None] for x in bufs]
        mix=sum(mastered)
        for st,path,x in zip(STEMS,paths,mastered):
            sf.write(str(path),x,SR,subtype='PCM_24')
            encode_vorbis(path,stage/'Music'/pid/f'{pid}_{st}.ogg')
        levels=[1,.85,1,0] if pid=='menu_curtain' else [1,.7,1,0] if pid=='select_dressing' else [1,1,1,.75]
        normal=sum(x*v for x,v in zip(mastered,levels))
        sf.write(str(P/'Masters'/f'{pid}_mix.wav'),normal,SR,subtype='PCM_24')
        encode_vorbis(P/'Masters'/f'{pid}_mix.wav',P/f'{pid}.ogg')
        result={'version':2,'boundaryRepairMilliseconds':3,'inputHashes':hashes,'outputHashes':[sha(p) for p in paths],
                'maxReductionDb':float(-20*np.log10(gain.min())),
                'sumPeakDb':float(20*np.log10(np.abs(mix).max())),
                'sumRmsDb':float(20*np.log10(np.sqrt(np.mean(mix*mix))))}
        ledger[pid]=result
        scores[pid].update({k:result[k] for k in ('sumPeakDb','sumRmsDb')})
        ledgerpath.write_text(json.dumps(ledger,indent=2)+'\n')
        (stage/'scores.json').write_text(json.dumps(scores,indent=1)+'\n')
        print(f'MASTER {pid}: RMS {result["sumRmsDb"]:.2f} dB, peak {result["sumPeakDb"]:.2f} dB',flush=True)
    (P/'Mastering.json').write_text(json.dumps(ledger,indent=2)+'\n')


if __name__=='__main__': main()
