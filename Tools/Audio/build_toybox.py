"""Build/verify/install the complete Toybox replacement without touching source MP3s.

python Tools/Audio/setup_toybox.py
python Tools/Audio/build_toybox.py [--music ID ...] [--sfx] [--install]

Default builds everything into .cache/toybox/; --install validates the complete
staging tree before copying only manifest-referenced audio into Unity. Existing
.meta GUIDs are retained. Masters/previews live under PreviewExports/Audio/Toybox.
"""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import sys
from concurrent.futures import ProcessPoolExecutor, as_completed

import soundfile as sf

from toybox_engine import ROOT, CACHE, Sampler

A=ROOT/'Assets/ArcaneBrigade/Resources/Audio/Arcane'
STAGE=CACHE/'toybox'
PREVIEW=ROOT/'PreviewExports/Audio/Toybox'
BASELINE=Path(__file__).with_name('toybox_baseline.json')


def digest(path): return hashlib.sha256(path.read_bytes()).hexdigest()


def music_worker(config, original):
    from toybox_music import render_piece
    sampler=Sampler()
    try: return render_piece(sampler,config,STAGE,PREVIEW,original)
    finally: sampler.close()


def baseline():
    if BASELINE.exists(): return json.loads(BASELINE.read_text())
    cues=json.loads((A/'cues.json').read_text())['cues']
    music=json.loads((A/'Music/music.json').read_text())['pieces']
    if len(cues)!=153: raise RuntimeError('Baseline must be captured before replacement')
    assets={}
    for cue in cues:
        for rel in cue['clips']:
            path=A/(rel+'.wav'); info=sf.info(str(path))
            assets[rel]={'frames':info.frames,'sha256':digest(path)}
    result={'cues':cues,'music':music,'assets':assets}
    BASELINE.write_text(json.dumps(result,indent=1)+'\n')
    return result


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--music',nargs='+',help='Only rebuild selected music ids')
    parser.add_argument('--sfx',action='store_true',help='Only rebuild SFX/voices/ambience')
    parser.add_argument('--install',action='store_true',help='Validate and install the complete staged library')
    parser.add_argument('--install-only',action='store_true')
    parser.add_argument('--jobs',type=int,default=3,help='Independent music render processes')
    args=parser.parse_args()
    before=baseline()
    STAGE.mkdir(parents=True,exist_ok=True); (STAGE/'Music').mkdir(exist_ok=True)
    PREVIEW.mkdir(parents=True,exist_ok=True)
    if not args.install_only:
        sampler=Sampler()
        try:
            if not args.music:
                from toybox_sfx import build
                rows=build(sampler,before,STAGE)
                (STAGE/'sfx_delivery.json').write_text(json.dumps(rows,indent=1)+'\n')
            if not args.sfx:
                # SFX overrides must not leak into the modern drum-synthesis kit.
                import importlib,kit
                importlib.reload(kit)
                from toybox_music import parse_tracks,render_piece
                pieces={p['id']:p for p in before['music']}
                scores={}
                if (STAGE/'scores.json').exists(): scores=json.loads((STAGE/'scores.json').read_text())
                pending=[]
                for config in parse_tracks():
                    pid=config[0]
                    if args.music and pid not in args.music:
                        if pid in scores: pieces[pid]={k:scores[pid][k] for k in pieces[pid]}
                        continue
                    pending.append(config)
                with ProcessPoolExecutor(max_workers=max(1,args.jobs)) as pool:
                    jobs=[pool.submit(music_worker,config,pieces[config[0]]) for config in pending]
                    for job in as_completed(jobs):
                        piece,score=job.result(); pid=piece['id']
                        pieces[pid],scores[pid]=piece,score
                        (STAGE/'scores.json').write_text(json.dumps(scores,indent=1)+'\n')
                (STAGE/'Music/music.json').write_text(json.dumps({'version':1,'pieces':list(pieces.values())},indent=1)+'\n')
        finally: sampler.close()
    if args.install or args.install_only:
        import toybox_master
        toybox_master.main()
        from verify_toybox import verify
        report=verify(STAGE,before)
        (PREVIEW/'QA_Report.json').write_text(json.dumps(report,indent=2)+'\n')
        if report['failures']:
            print('\n'.join(report['failures'])); raise SystemExit('Staging failed QA; Unity assets were not replaced')
        cues=json.loads((STAGE/'cues.json').read_text())
        music=json.loads((STAGE/'Music/music.json').read_text())
        files=['cues.json','Music/music.json']
        files += [rel+'.wav' for c in cues['cues'] for rel in c['clips']]
        files += [rel+'.ogg' for p in music['pieces'] for rel in p['stems']]
        for rel in files:
            dest=A/rel; dest.parent.mkdir(parents=True,exist_ok=True)
            shutil.copyfile(STAGE/rel,dest)
        import write_meta
        write_meta.main()
        delivery=json.loads((STAGE/'sfx_delivery.json').read_text())
        for pc in music['pieces']:
            for st in pc['stems']:
                delivery.append({'path':st+'.ogg','piece':pc['id'],'source':'original composition; GeneralUser GS + original object percussion',
                                 'frames':pc['loopSamples'],'seconds':pc['loopSeconds']})
        for row in delivery:
            row['sha256']=digest(A/row['path'])
            row['license']='Project original synthesis/composition'
            if row.get('piece') or row.get('samplePrograms'):
                row['license']+='; GeneralUser GS License v2.0 for sampled instruments (see full license and provenance caveat)'
        (ROOT/'Docs/Audio/Toybox_Asset_Manifest.json').write_text(json.dumps({'collection':'Midnight Toybox Band','assets':delivery},indent=1)+'\n',encoding='utf-8')
        (ROOT/'Docs/Audio/Toybox_Scores.json').write_text((STAGE/'scores.json').read_text())
        print(f'INSTALLED {len(files)-2} audio files. QA: {len(report["failures"])} failures.',flush=True)

if __name__=='__main__': main()
