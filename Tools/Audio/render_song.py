"""Render ONE independently authored song; keep per-song results until final assembly.

python Tools/Audio/render_song.py menu_curtain
python Tools/Audio/render_song.py --assemble
"""
import json
from pathlib import Path
import sys
import hashlib
from concurrent.futures import ProcessPoolExecutor

from toybox_engine import CACHE, ROOT, Sampler
from toybox_music import IDS, render_piece
from score_player import load_score,compile_score

STAGE=CACHE/'toybox'
PREVIEW=ROOT/'PreviewExports/Audio/Toybox'


def render_one(pid):
    records=STAGE/'individual';records.mkdir(exist_ok=True)
    score=load_score(pid);compile_score(score)
    old=next(p for p in json.loads((ROOT/'Assets/ArcaneBrigade/Resources/Audio/Arcane/Music/music.json').read_text())['pieces'] if p['id']==pid)
    sampler=Sampler()
    try: piece,report=render_piece(sampler,(pid,),STAGE,PREVIEW,old)
    finally: sampler.close()
    (records/(pid+'.json')).write_text(json.dumps({'piece':piece,'score':report},indent=1)+'\n')


def main():
    if len(sys.argv)!=2: raise SystemExit(__doc__)
    pid=sys.argv[1]
    records=STAGE/'individual';records.mkdir(exist_ok=True)
    if pid=='--remaining':
        pending=[]
        for piece_id in IDS:
            source=load_score(piece_id)
            digest=hashlib.sha256(json.dumps(source,sort_keys=True).encode()).hexdigest()
            record=records/(piece_id+'.json')
            if record.exists() and json.loads(record.read_text())['score']['compositionSha256']==digest: continue
            compile_score(source);pending.append(piece_id)
        with ProcessPoolExecutor(max_workers=3) as pool: list(pool.map(render_one,pending))
        return
    if pid=='--assemble':
        pieces=[];scores={}
        for piece_id in IDS:
            data=json.loads((records/(piece_id+'.json')).read_text())
            assert data['score']['revision']==4
            source=load_score(piece_id)
            digest=hashlib.sha256(json.dumps(source,sort_keys=True).encode()).hexdigest()
            if data['score']['compositionSha256']!=digest: raise RuntimeError('Re-render changed score: '+piece_id)
            pieces.append(data['piece']);scores[piece_id]=data['score']
        (STAGE/'Music/music.json').write_text(json.dumps({'version':1,'pieces':pieces},indent=1)+'\n')
        (STAGE/'scores.json').write_text(json.dumps(scores,indent=1)+'\n')
        print('ASSEMBLED fifteen individual compositions',flush=True)
        return
    if pid not in IDS: raise SystemExit('Unknown piece '+pid)
    render_one(pid)


if __name__=='__main__': main()
