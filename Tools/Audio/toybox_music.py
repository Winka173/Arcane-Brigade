"""Fifteen original, fully arranged Toybox compositions; four independently playable stems.

The seven-note Brigade call/reply appears in every score. Each scene has its own
hook rhythm, harmony, bass line, object percussion and bridge. Sections are the
bar-accurate STING/HOOK/ANSWER/BRIDGE/RETURN from the supplied Toybox document.
"""
import json
from pathlib import Path
import re
import subprocess
import imageio_ffmpeg

import numpy as np
import soundfile as sf

from gen_toybox_prompts import TRACKS
import kit as K
from synth import circular_reverb
from toybox_engine import SR, ROOT, add, circular_filter, object_hit, seed

STEMS=('base','rhythm','melody','intensity')

def encode_vorbis(source,dest):
    result=subprocess.run([imageio_ffmpeg.get_ffmpeg_exe(),'-hide_banner','-loglevel','error','-y',
                           '-i',str(source),'-map_metadata','-1','-c:a','libvorbis','-q:a','8',str(dest)],
                          capture_output=True,text=True,creationflags=getattr(subprocess,'CREATE_NO_WINDOW',0))
    if result.returncode: raise RuntimeError(result.stderr)
SCALES={'major':[0,2,4,5,7,9,11], 'minor':[0,2,3,5,7,8,10], 'harmonic minor':[0,2,3,5,7,8,11]}
TONICS={'C':0,'D':2,'E':4,'F':5,'G':7,'A':9,'Bb':10,'Ab':8}

# Program numbers are zero-based General MIDI. Instrument/pan is kept stable
# across stems, with bass/kick centred and narrow low frequencies.
PALETTES={
 'menu_curtain': (10,9,22,82,32,'pop'),
 'select_dressing': (56,9,22,82,33,'march'),
 'combat_toys': (3,82,56,9,38,'march'),
 'combat_spoons': (78,59,3,9,36,'funk'),
 'combat_clockwork': (80,9,10,78,38,'electro'),
 'combat_carousel': (82,21,3,9,32,'swing'),
 'combat_overtime': (9,3,82,55,38,'break'),
 'combat_lasttrain': (3,78,9,82,38,'train'),
 'boss_ringmaster': (19,30,10,61,39,'rock'),
 'boss_clown': (10,82,3,81,38,'hard'),
 'shop_gold': (3,59,9,10,32,'swing'),
 'shop_layaway': (9,4,3,78,36,'funk'),
 'shop_dream': (10,8,92,52,32,'waltz'),
 'endless_neverclose': (3,81,82,9,38,'house'),
 'endless_encore': (82,61,3,9,34,'rock'),
}

# Each tuple is a distinct two-bar hook in scale degrees, beginning with the
# motif's rising call. Values below zero allow a pickup G below tonic C.
HOOKS={
 'menu_curtain': [-3,0,1,2,4,2,1,0,1,2,4,5,4,2,1,0],
 'select_dressing': [-3,0,1,2,4,4,2,1,2,1,0,-1,0,2,1,0],
 'combat_toys': [-3,0,1,2,2,4,2,1,0,-1,0,1,4,2,1,0],
 'combat_spoons': [-3,0,1,2,4,2,5,4,2,1,0,-3,2,4,1,0],
 'combat_clockwork': [-3,0,1,2,4,3,2,1,2,1,0,-3,0,1,2,4],
 'combat_carousel': [-3,0,1,2,3,4,5,4,3,2,1,0,2,4,1,0],
 'combat_overtime': [-3,0,1,2,4,7,5,4,2,1,0,-1,0,1,2,4],
 'combat_lasttrain': [-3,0,1,2,4,3,2,0,1,2,4,2,1,0,-1,0],
 'boss_ringmaster': [-3,0,1,2,-1,0,4,3,2,1,0,-3,-1,2,1,0],
 'boss_clown': [-3,0,1,2,3,2,-1,0,4,3,2,1,0,-1,1,0],
 'shop_gold': [-3,0,1,2,4,5,4,2,2,1,0,-1,0,2,1,0],
 'shop_layaway': [-3,0,1,2,4,2,1,0,4,5,7,5,4,2,1,0],
 'shop_dream': [-3,0,1,2,4,5,4,2,2,1,0,-1,0,2,1,0],
 'endless_neverclose': [-3,0,1,2,4,7,5,4,2,1,0,2,4,5,4,2],
 'endless_encore': [-3,0,1,2,4,5,7,4,2,1,0,-1,0,4,2,0],
}
RHYTHMS={
 'pop': [0,.5,1,1.5,2.5,3,3.5,3.75],
 'march': [0,.5,1,1.5,2,2.75,3.25,3.5],
 'funk': [0,.75,1.25,1.75,2.5,2.75,3.25,3.75],
 'electro': [0,.5,.75,1.5,2,2.5,3,3.5],
 'swing': [0,2/3,1,5/3,2,8/3,3,11/3],
 'break': [0,.5,.75,1.5,2,2.75,3,3.5],
 'train': [0,.5,1,1.75,2.25,2.5,3,3.5],
 'rock': [0,.5,1,1.5,2.5,3,3.5,3.75],
 'hard': [0,.75,1,1.5,2.25,2.75,3.25,3.5],
 'house': [0,.5,1,1.5,2,2.5,3.25,3.5],
 'waltz': [0,.5,1,1.5,2,2.5],
}


def parse_tracks():
    text=(ROOT/'Docs/Audio/Music_Prompts_Toybox.md').read_text(encoding='utf-8')
    parsed=[]
    for pid,title,scene,palette,key,bpm,bpb,bars,sections in TRACKS:
        block=re.search(r'## \d+\. '+re.escape(pid)+r' .*?```\n(.*?)```',text,re.S)
        if not block or f'{key}, {bpm} BPM, {bpb}/4, {bars} bars' not in block[1]:
            raise ValueError('Prompt/config mismatch: '+pid)
        parsed.append((pid,title,key,bpm,bpb,bars))
    return parsed


def build_score(pid,key,bpm,bpb,bars):
    tonic, mode=key.split(' ',1)
    root=TONICS[tonic]
    scale=SCALES[mode]
    lead,double,answer,colour,bass,style=PALETTES[pid]
    beat=60/bpm
    n=round(bars*bpb*beat*SR)
    rng=np.random.default_rng(seed('toybox-score:'+pid))
    events={st:[] for st in STEMS}
    objects=[]
    cuts=[0,1,5,9,13,16] if bars==16 else [0,2,10,18,26,32]
    def degree(d,octave=5):
        octoff,index=divmod(d,7)
        return 12*(octave+1+octoff)+root+scale[index]
    def note(st,p,di,at,gate=.35,vel=85,pan=0,octave=5,midi=False,drum=False):
        pitch=di if midi else degree(di,octave)
        velocity=max(1,min(127,round(vel+rng.integers(-4,5))))
        events[st].append((round(at*beat*SR),p,int(pitch),velocity,max(1,round(gate*beat*SR)),pan,drum))
    def percussion(at,pitch,vel=85,gate=.12):
        note('rhythm',0,pitch,at,gate,vel,0,drum=True,midi=True)
    def chord(st,di,at,gate,vel=50,program=82):
        for k,d in enumerate([di,di+2,di+4]):
            note(st,program,d,at+k*.012,gate,vel,(-.25,0,.25)[k],3)

    progressions=([0,5,3,4,0,3,1,4] if mode=='major' else [0,5,2,6,3,0,1,4])
    bridge_prog=([3,0,5,4,1,3,4,4] if mode=='major' else [5,3,0,4,5,6,3,4])
    hook=HOOKS[pid]
    for bar in range(bars):
        section=next(i for i in range(5) if cuts[i]<=bar<cuts[i+1])
        local=bar-cuts[section]
        at=bar*bpb
        di=(bridge_prog if section==3 else progressions)[(bar//2)%8]
        # A voiced harmonic floor: both bass and chords are musical by themselves.
        chord('base',di,at,bpb-.12,42,program=89 if style=='waltz' else 4 if pid.startswith('shop_') else 82)
        if style=='waltz': bass_pattern=[(0,di,1.8),(1,di+4,.7),(2,di+4,.7)]
        elif style in ('swing','funk'):
            bass_pattern=[(0,di,.65),(.75,di+7,.3),(1.5,di+4,.4),(2,di+2,.65),(2.75,di+4,.35),(3.5,di+6,.3)]
        elif style in ('break','train'):
            bass_pattern=[(0,di,.8),(1.25,di,.5),(2,di+7,.75),(3,di+4,.45),(3.5,di,.35)]
        else: bass_pattern=[(0,di,.7),(1,di+7,.45),(1.75,di+4,.4),(2.5,di,.65),(3.5,di+7,.3)]
        for off,d,gate in bass_pattern:
            note('base',bass,d,at+off,gate,87,0,1)

        # Always-running rhythm: bridge changes instrumentation, never drops drums.
        if style=='waltz':
            for off,pitch,vel in [(0,36,75),(1,37,60),(2,37,66)]: percussion(at+off,pitch,vel)
            for off in np.arange(0,3,.5): percussion(at+off,42,40)
        else:
            kicks=([0,1.5,2.5,3.25] if style in ('break','train') else
                   [0,1.75,2.5] if style=='funk' else [0,1,2,3])
            for off in kicks: percussion(at+off,36,105 if not pid.startswith(('menu','shop')) else 86)
            for off in [1,3]:
                percussion(at+off,38 if style not in ('hard','house') else 39,98)
            for off in ([.75,2.75] if style=='march' else [2.75,3.75] if style in ('break','train') else []):
                percussion(at+off,38,35,.06)
            for i,off in enumerate(np.arange(0,4,.5)):
                if style=='swing' and i%2: off+=1/6
                percussion(at+off,46 if i==7 and bar%2 else 42,48 if i%2 else 36,.13 if i==7 else .06)
        if bar%4==3:
            for k in range(4): percussion(at+bpb-1+k*.25,[38,48,45,41][k],57+8*k,.12)
        # Kitchenware, wind-up keys, clock ticks and typewriter mechanisms.
        material='metal' if pid=='combat_spoons' else 'glass' if style=='waltz' else 'wood'
        for k in range(bpb*2):
            objects.append((at+k*.5,material,.042+.008*(k%2),bar*17+k,.023 if k%2 else .015,(-.4 if k%2 else .4)))
        if bar%4==0: objects.append((at,'spring',.3,bar,.04,.25))
        # Percussive toy-piano chords keep the pulse when the melody is ducked.
        if style!='waltz':
            for off in ([.5,1.5,2.5,3.5] if style in ('house','electro','swing') else [1.5,3]):
                chord('rhythm',di,at+off,.18,49,3)

        if section==0:
            # Seven-note call and answer, spread across the one/two-bar sting.
            if local==0:
                for k,d in enumerate([-3,0,1,2,2,1,0]):
                    note('melody',lead,d,at+k*(bpb*min(cuts[1],2)/8),.35,102,-.1)
                chord('rhythm',0,at,.45,72,61)
        elif section in (1,4):
            rhythm=RHYTHMS[style]
            for k,off in enumerate(rhythm):
                h=(local%2)*len(rhythm)+k
                d=hook[h%len(hook)]
                if section==4 and local>=2 and pid in ('combat_toys','combat_overtime','endless_encore'): d+=7
                gate=min(.5,(rhythm[k+1]-off)*.8) if k+1<len(rhythm) else .25
                if k==len(rhythm)-1 and local%2: gate=.45
                note('melody',lead,d,at+off,gate,96 if section==1 else 103,-.1)
                if (section==4 or local%4>=2) and k%2==0:
                    note('melody',double,d,at+off+.012,gate*.9,63,.22)
            if local%2==1:
                for k,d in enumerate([2,1,0]): note('intensity',colour,d+7,at+bpb-1+k*.25,.18,68,.3)
        elif section==2:
            # Two-bar dialogue, complete call and falling reply stay audible in melody.
            ds=[-3,0,1,2] if local%2==0 else [2,1,0,-3]
            for k,d in enumerate(ds):
                off=k*(bpb/4)
                note('melody',lead if local%2==0 else answer,d,at+off,.5,97, -.12 if local%2==0 else .14)
            if local%2==1:
                for k,d in enumerate([di+4,di+2,di]): note('intensity',double,d+7,at+k*.5,.24,67,.25)
        else:
            # Separate eight-bar bridge, with longer soaring phrases and a new lead.
            bridge=[4,5,7,6,4,2,3,4,5,4,2,1,2,4,1,0]
            offsets=[0,.75,1.5,2.5] if bpb==4 else [0,.75,1.5,2.25]
            for k,off in enumerate(offsets):
                d=bridge[(local%4)*4+k]
                note('melody',answer,d,at+off,.62 if k<3 else .4,99,.06)
                if local>=4: note('melody',lead,d-7,at+off,.4,51,-.2)
        # Intensity adds energy rather than hiding essential hook or percussion.
        if bar%2==0:
            chord('intensity',di,at,.32,62,61 if style in ('rock','march','funk') else colour)
        for k in range(bpb*2):
            d=di+[0,2,4,7][(k+bar)%4]
            note('intensity',9 if colour!=9 else 10,d+7,at+k*.5,.18,49, .35 if k%2 else -.35)
    return events,objects,n,cuts


def render_piece(sampler, config, out, preview, original):
    pid,title,key,bpm,bpb,bars=config
    events,objects,n,cuts=build_score(pid,key,bpm,bpb,bars)
    beat=60/bpm
    bufs={}
    for st in STEMS:
        print(f'RENDER {pid}/{st}',flush=True)
        x=sampler.render(events[st],n)
        if st=='rhythm':
            for at,material,dur,var,gain,pan in objects:
                add(x,object_hit(material,dur,var),at*beat,gain,True,pan)
            # Modern kick gives the acoustic kit a stable, punchy low end.
            for at,program,pitch,velocity,gate,pan,drum in events[st]:
                if drum and pitch==36: add(x,K.kick(velocity/127,'punch'),at/SR,.035,True)
        x=circular_filter(circular_filter(x,30,'highpass'),13500)
        if st!='rhythm':
            x=circular_reverb(x,wet=.11 if pid!='shop_dream' else .21,seconds=.8 if pid!='shop_dream' else 1.35,seed=seed(pid)%10000,damp=6500)
        # Remove residual DC without changing phase or adding a loop fade.
        x-=x.mean(axis=0)
        rms=np.sqrt(np.mean(x*x))
        target={'base':-25.5,'rhythm':-24,'melody':-21.5,'intensity':-29.5}[st]
        x*=10**(target/20)/max(rms,1e-12)
        # Gentle soft-knee saturation tames individual drum/hammer attacks.
        x=.62*np.tanh(x/.62)
        bufs[st]=x
    mix=sum(bufs.values())
    # One shared gain preserves stem balance and provides worst-case sum headroom.
    rms=np.sqrt(np.mean(mix*mix))
    gain=min(10**(-17/20)/rms,.79/max(np.abs(mix).max(),1e-12))
    folder=out/'Music'/pid; folder.mkdir(parents=True,exist_ok=True)
    master_dir=preview/'Masters'/pid; master_dir.mkdir(parents=True,exist_ok=True)
    for st,x in bufs.items():
        x*=gain
        sf.write(str(master_dir/f'{pid}_{st}.wav'),x,SR,subtype='PCM_24')
        # libsndfile's default Vorbis quality is lower than the contract. Explicit q8.
        encode_vorbis(master_dir/f'{pid}_{st}.wav',folder/f'{pid}_{st}.ogg')
    mix=sum(bufs.values())
    # Player-facing stereo preview is the scene's normal mix, no intensity in menu/select.
    levels=[1,.85,1,0] if pid=='menu_curtain' else [1,.7,1,0] if pid=='select_dressing' else [1,1,1,.75]
    normal=sum(bufs[s]*v for s,v in zip(STEMS,levels))
    preview.mkdir(parents=True,exist_ok=True)
    sf.write(str(preview/'Masters'/f'{pid}_mix.wav'),normal,SR,subtype='PCM_24')
    encode_vorbis(preview/'Masters'/f'{pid}_mix.wav',preview/f'{pid}.ogg')
    piece=dict(original,id=pid,title=title,bpm=bpm,beatsPerBar=bpb,bars=bars,
               loopSamples=n,loopSeconds=n/SR,family='midnight_toybox',
               stems=[f'Music/{pid}/{pid}_{st}' for st in STEMS])
    score={**piece,'key':key,'sections':dict(zip(['sting','hook','answer','bridge','return'],cuts[:-1])),
           'motifDegrees':[-3,0,1,2,2,1,0],'sampleEvents':{k:len(v) for k,v in events.items()},
           'sumPeakDb':float(20*np.log10(np.abs(mix).max())), 'sumRmsDb':float(20*np.log10(np.sqrt(np.mean(mix*mix))))}
    print(f'MUSIC {pid}: {n/SR:.3f}s, {score["sumRmsDb"]:.1f} dB RMS, {score["sumPeakDb"]:.1f} dB peak',flush=True)
    return piece,score
