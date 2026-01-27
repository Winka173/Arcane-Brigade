"""Render/validate all v5 non-music audio; optionally install without touching music.

python Tools/Audio/render_foley.py --install
Runtime cue parameters, all 582 frame counts and every Unity GUID are retained.
The source catalogue is sufficient on a new checkout; no v3 render cache needed.
"""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
from concurrent.futures import ProcessPoolExecutor, as_completed

import numpy as np
import soundfile as sf

from toybox_engine import ROOT, CACHE, SR

A=ROOT/'Assets/ArcaneBrigade/Resources/Audio/Arcane'
STAGE=CACHE/'foley-v5'
P=ROOT/'PreviewExports/Audio/Toybox'
SOURCE=Path(__file__).with_name('SfxDesigns.json')


def digest(path): return hashlib.sha256(path.read_bytes()).hexdigest()


def worker(design, output=STAGE):
    from foley_engine import render
    rows=[]
    for i,rel in enumerate(design['clips']):
        d=dict(design);d['frames']=design['variationFrames'][i]
        if d['id'].startswith('voice.') and d['id'].endswith('.syl'):
            d['layers']=[dict(d['layers'][0],dur=d['frames']/SR)]
        x=render(d,i,d['danger'])
        dest=output/(rel+'.wav');dest.parent.mkdir(parents=True,exist_ok=True)
        sf.write(str(dest),x,SR,subtype='PCM_16')
        master=P/'Masters/Foley'/(rel+'.wav');master.parent.mkdir(parents=True,exist_ok=True)
        sf.write(str(master),x,SR,subtype='PCM_24')
        rows.append(dict(path=rel+'.wav',cue=d['id'],variation=i,frames=len(x),seconds=len(x)/SR,
                         revision=5,source='Original modal / waveguide / granular / additive formant synthesis',
                         samplePrograms=[],designFile='Tools/Audio/SfxDesigns.json',identity=d['identity'],
                         designSha256=hashlib.sha256(json.dumps(design,sort_keys=True).encode()).hexdigest(),
                         rendererSha256=digest(Path(__file__).with_name('foley_engine.py')),
                         license='Project original synthesis; no sampled instruments or input audio',sha256=digest(dest)))
    return rows


def stage_build(jobs=3, output=STAGE):
    designs=json.loads(SOURCE.read_text(encoding='utf-8'))['designs']
    live=json.loads((A/'cues.json').read_text())['cues']
    if {d['id']:d['clips'] for d in designs}!={c['id']:c['clips'] for c in live}:
        raise ValueError('Design coverage must match every live cue and clip')
    output.mkdir(parents=True,exist_ok=True);P.mkdir(parents=True,exist_ok=True)
    shutil.copyfile(A/'cues.json',output/'cues.json')
    rows=[]
    with ProcessPoolExecutor(max_workers=jobs) as pool:
        pending={pool.submit(worker,d,output):d['id'] for d in designs}
        for task in as_completed(pending):
            rows.extend(task.result());print('FOLEY',pending[task],flush=True)
    rows.sort(key=lambda r:r['path'])
    (output/'sfx_delivery.json').write_text(json.dumps(rows,indent=1)+'\n',encoding='utf-8')
    return rows


def validate_staging(rows, output=STAGE):
    designs={d['id']:d for d in json.loads(SOURCE.read_text())['designs']}
    expected={rel+'.wav' for d in designs.values() for rel in d['clips']}
    if len(rows)!=582 or {r['path'] for r in rows}!=expected:
        raise RuntimeError('Staged Foley coverage is incomplete')
    engine_hash=digest(Path(__file__).with_name('foley_engine.py'))
    for row in rows:
        source_hash=hashlib.sha256(json.dumps(designs[row['cue']],sort_keys=True).encode()).hexdigest()
        if row.get('revision')!=5 or row.get('rendererSha256')!=engine_hash or row['designSha256']!=source_hash or row['sha256']!=digest(output/row['path']):
            raise RuntimeError('Staged Foley no longer matches source/hash: '+row['path'])


def replacement_audit(rows, previous):
    failures=[];changed=0;variations=[]
    designs=json.loads(SOURCE.read_text())['designs']
    for d in designs:
        arrays=[]
        for i,rel in enumerate(d['clips']):
            new=STAGE/(rel+'.wav');old=A/(rel+'.wav')
            if sf.info(str(new)).frames!=sf.info(str(old)).frames: failures.append('Duration changed: '+rel)
            if digest(new)!=previous[rel+'.wav']: changed+=1
            elif digest(new)!=digest(old): failures.append('Unexpected previous snapshot: '+rel)
            x,_=sf.read(str(new));arrays.append(x)
        # Correlated renders catch seed-only changes whose waveform barely moved.
        if len(arrays)>1:
            correlations=[]
            for i,x in enumerate(arrays):
                for y in arrays[i+1:]:
                    n=min(len(x),len(y));denom=np.linalg.norm(x[:n])*np.linalg.norm(y[:n])
                    value=abs(float(np.dot(x[:n],y[:n]))/max(denom,1e-12));correlations.append(value)
                    if value>.995: failures.append('Nearly identical variants: '+d['id'])
            variations.append(dict(cue=d['id'],maximumAbsoluteWaveformCorrelation=max(correlations)))
    installed_revision=json.loads((ROOT/'Docs/Audio/Toybox_Asset_Manifest.json').read_text(encoding='utf-8')).get('foleyRevision')
    if changed!=582 and installed_revision!=5:
        failures.append(f'Expected every previous Toybox clip replaced, got {changed}/582')
    return dict(status='PASS' if not failures else 'FAIL',revision=5,changedPreviousToyboxClips=changed,
                framesPreserved=582,designCount=len(designs),designFileSha256=digest(SOURCE),
                failures=failures,variations=variations,
                listeningReview='Not performed. Waveform correlation cannot establish perceptual quality.')


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--install',action='store_true')
    parser.add_argument('--install-only',action='store_true')
    parser.add_argument('--jobs',type=int,default=3)
    args=parser.parse_args()
    # This invocation snapshot also makes an ordinary rebuild verifiable.
    snapshot={f.relative_to(A).as_posix():digest(f) for f in A.rglob('*') if f.is_file()}
    session=CACHE/'v5_before.json'
    if not session.exists(): session.write_text(json.dumps(snapshot,indent=1)+'\n')
    rows=json.loads((STAGE/'sfx_delivery.json').read_text()) if args.install_only else stage_build(max(1,args.jobs))
    validate_staging(rows)
    # On repeat installs, compare to the first v5 snapshot when it is available.
    previous={p.replace('\\','/'):h for p,h in json.loads(session.read_text()).items()}
    audit=replacement_audit(rows,previous)
    (P/'Foley_Audit.json').write_text(json.dumps(audit,indent=2)+'\n')
    if audit['failures']: raise RuntimeError('\n'.join(audit['failures']))
    from verify_toybox import verify
    baseline=json.loads(Path(__file__).with_name('toybox_baseline.json').read_text())
    report=verify(STAGE,baseline,A)
    report['foleyRevision']=5;report['changedPreviousToyboxClips']=audit['changedPreviousToyboxClips']
    (P/'QA_Report.json').write_text(json.dumps(report,indent=2)+'\n')
    if report['failures']: raise RuntimeError('\n'.join(report['failures']))
    if args.install or args.install_only:
        # Check all protected assets immediately before writing; don't replace
        # cue configuration, Unity import settings or the independently made music.
        protected={p:h for p,h in snapshot.items() if not p.endswith('.wav')}
        if any(digest(A/p)!=h for p,h in snapshot.items()):
            raise RuntimeError('Live audio changed during render; rerun against the new state')
        for row in rows: shutil.copyfile(STAGE/row['path'],A/row['path'])
        if any(digest(A/p)!=h for p,h in protected.items()): raise RuntimeError('Protected audio asset changed')
        manifest=ROOT/'Docs/Audio/Toybox_Asset_Manifest.json'
        old=json.loads(manifest.read_text(encoding='utf-8'))
        old['assets']=[r for r in old['assets'] if r.get('piece')]+rows
        old['foleyRevision']=5
        manifest.write_text(json.dumps(old,indent=1)+'\n',encoding='utf-8')
        audit.update(installed=True,protectedFilesVerified=len(protected),
                     protectedMusicFiles=sum(p.startswith('Music/') for p in protected),
                     metadataFilesVerified=sum(p.endswith('.meta') for p in protected))
        (P/'Foley_Audit.json').write_text(json.dumps(audit,indent=2)+'\n')
        print('INSTALLED all 582 new Foley/voice/ambience clips; music, cue config and GUIDs preserved',flush=True)


if __name__=='__main__': main()
