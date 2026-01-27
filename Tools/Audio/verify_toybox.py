"""Decoded-asset QA, including true peak, every runtime stem mix and exact replacements.

This is technical verification, not a claim of subjective listening approval.
"""
import hashlib
import json
from pathlib import Path
import sys

import numpy as np
from scipy.signal import resample_poly
import soundfile as sf
try:
    import pyloudnorm as pyln
except ImportError:
    pyln=None

from toybox_engine import SR, ROOT


def db(v): return float(20*np.log10(max(float(v),1e-12)))


def verify(folder,baseline,music_folder=None):
    music_folder=music_folder or folder
    failures=[]; warnings=[]; rows=[]; seen={}
    cues=json.loads((folder/'cues.json').read_text())['cues']
    pieces=json.loads((music_folder/'Music/music.json').read_text())['pieces']
    old=baseline['assets']; changed=0
    if len(cues)!=206: failures.append('Expected 206 cue definitions')
    if sum(len(c['clips']) for c in cues)!=582: failures.append('Expected 582 SFX/voice/ambience files')
    if len(pieces)!=15: failures.append('Expected 15 music pieces')
    ids=[c['id'] for c in cues]
    if len(ids)!=len(set(ids)): failures.append('Duplicate cue ids')
    for c in cues:
        for rel in c['clips']:
            path=folder/(rel+'.wav')
            if not path.exists(): failures.append('Missing '+rel); continue
            info=sf.info(str(path)); x,sr=sf.read(str(path),dtype='float32')
            if sr!=SR or x.ndim!=1 or info.subtype!='PCM_16': failures.append('Format '+rel)
            if rel in old:
                if len(x)!=old[rel]['frames']: failures.append('Changed animation length '+rel)
                changed+=hashlib.sha256(path.read_bytes()).hexdigest()!=old[rel]['sha256']
            pcmhash=hashlib.sha256(x.tobytes()).hexdigest()
            if pcmhash in seen: failures.append(f'Duplicate {rel} / {seen[pcmhash]}')
            seen[pcmhash]=rel
            peak=db(np.max(np.abs(x))); rms=db(np.sqrt(np.mean(x*x)))
            if peak>=-.08: failures.append('SFX clipping '+rel)
            if abs(peak-(-7 if c['loop'] else -3))>.08: failures.append('Peak target '+rel)
            if abs(float(np.mean(x)))>.001: failures.append('DC '+rel)
            if not np.isfinite(x).all() or rms<-55: failures.append('Invalid/silent '+rel)
            true_peak=db(np.abs(resample_poly(x,4,1)).max())
            if true_peak>=0: failures.append('SFX intersample clipping '+rel)
            jump=abs(float(x[0]-x[-1]))
            if c['loop']:
                if jump>max(.015,8*float(np.mean(np.abs(np.diff(x))))): failures.append('Loop seam '+rel)
            elif abs(float(x[0]))>.0001 or abs(float(x[-1]))>.0001: failures.append('One-shot click '+rel)
            if c['id'].startswith('shop.keeper.enter') or c['id']=='up.ally.land':
                landing=.405 if c['id']=='up.ally.land' else .98
                window=x[round((landing-.015)*SR):round((landing+.075)*SR)]
                if np.max(np.abs(window))<.2: failures.append('Missing choreographed landing '+rel)
            rows.append({'file':rel+'.wav','frames':len(x),'peakDb':peak,'truePeakDbTP':true_peak,'rmsDb':rms,'loopSeam':jump})
    if changed!=471: failures.append(f'Only {changed}/471 existing clips changed')
    mixrows=[]
    # Worst case for dense combat, sparse combat, menu/select and danger duck.
    variants={'all':[1,1,1,1],'sparse':[1,.7,1,0],'menu':[1,.85,1,0],
              'select':[1,.7,1,0],'shop':[1,.9,1,.75],'dream':[1,.8,1,.7],
              'boss':[1,1,1,.8],'danger':[.8,1,.35,.15]}
    for p in pieces:
        arrays=[]
        expected=round(p['bars']*p['beatsPerBar']*60/p['bpm']*SR)
        if p['loopSamples']!=expected: failures.append('Tempo/grid '+p['id'])
        for rel in p['stems']:
            path=music_folder/(rel+'.ogg')
            if not path.exists(): failures.append('Missing '+rel); continue
            x,sr=sf.read(str(path),dtype='float32',always_2d=True)
            if sr!=SR or x.shape!=(expected,2): failures.append('Stem alignment '+rel); continue
            arrays.append(x)
            peak=db(np.max(np.abs(x))); rms=db(np.sqrt(np.mean(x*x)))
            if peak>=-.08: failures.append('Stem clipping '+rel)
            if not np.isfinite(x).all() or rms<-55: failures.append('Silent/invalid stem '+rel)
            if abs(x.mean())>.001: failures.append('Stem DC '+rel)
            jump=float(np.max(np.abs(x[0]-x[-1])))
            step=float(np.mean(np.abs(np.diff(x,axis=0))))
            local=np.concatenate((x[-512:],x[:512]))
            local_step=float(np.mean(np.abs(np.diff(local,axis=0))))
            if jump>max(.01,3*local_step): failures.append('Music seam '+rel)
            rows.append({'file':rel+'.ogg','frames':len(x),'peakDb':peak,'rmsDb':rms,'loopSeam':jump})
        if len(arrays)!=4: continue
        for label,levels in variants.items():
            mix=sum(x*v for x,v in zip(arrays,levels))
            # Oversampled true peak: check in blocks with overlap to avoid artificial
            # resampler edges and keep memory bounded for long tracks.
            tp=0
            for at in range(0,len(mix),SR*5):
                start=max(0,at-64); stop=min(len(mix),at+SR*5+64)
                up=resample_poly(mix[start:stop],4,1,axis=0)
                lo=(at-start)*4; hi=min(at+SR*5,len(mix))-start
                tp=max(tp,float(np.max(np.abs(up[lo:hi*4]))))
            if tp>10**(-1/20): failures.append(f'True peak above -1 dBTP: {p["id"]} {label}: {db(tp):.2f}')
            row={'piece':p['id'],'mix':label,'truePeakDbTP':db(tp),'rmsDb':db(np.sqrt(np.mean(mix*mix)))}
            if label=='all' and pyln is not None:
                row['lufsIntegrated']=float(pyln.Meter(SR).integrated_loudness(mix))
            mixrows.append(row)
        print('QA',p['id'],flush=True)
    return {'status':'PASS' if not failures else 'FAIL','cueCount':len(cues),'clipCount':sum(len(c['clips']) for c in cues),
            'musicPieces':len(pieces),'musicStems':sum(len(p['stems']) for p in pieces),
            'replacedExistingClips':changed,'failures':failures,'warnings':warnings,
            'listeningReview':'Not performed. Technical QA cannot certify subjective quality.',
            'assets':rows,'mixes':mixrows}


if __name__=='__main__':
    folder=Path(sys.argv[1]) if len(sys.argv)>1 else ROOT/'Assets/ArcaneBrigade/Resources/Audio/Arcane'
    baseline=json.loads(Path(__file__).with_name('toybox_baseline.json').read_text())
    result=verify(folder,baseline)
    out=ROOT/'PreviewExports/Audio/Toybox/QA_Report.json'
    out.parent.mkdir(parents=True,exist_ok=True); out.write_text(json.dumps(result,indent=2)+'\n')
    print(json.dumps({k:v for k,v in result.items() if k not in ('assets','mixes')},indent=2))
    raise SystemExit(1 if result['failures'] else 0)
