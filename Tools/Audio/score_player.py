"""Interpret individually authored song scores. No default melody, harmony or groove.

Every compositional choice lives in Scores/<piece>.json. This module supplies
notation, event scheduling and rendering only. Notes use scientific pitch;
bar strings include explicit rests and gates instead of quantised note streams.
"""
import hashlib
import json
from pathlib import Path
import re

import numpy as np
import soundfile as sf

from toybox_engine import SR, ROOT, Sampler, add, circular_filter, object_hit, seed
from synth import circular_reverb

STEMS=('base','rhythm','melody','intensity')
SCORE_DIR=Path(__file__).with_name('Scores')
INTERVALS={
 '':[0,4,7], 'm':[0,3,7], '7':[0,4,7,10], 'maj7':[0,4,7,11],
 'm7':[0,3,7,10], '6':[0,4,7,9], 'm6':[0,3,7,9], '9':[0,4,7,10,14],
 'maj9':[0,4,7,11,14], 'm9':[0,3,7,10,14], '13':[0,4,7,10,14,21], 'sus2':[0,2,7],
 'sus4':[0,5,7], '7sus4':[0,5,7,10], 'dim7':[0,3,6,9],
 'm7b5':[0,3,6,10], '7b9':[0,4,7,10,13], '5':[0,7], 'add9':[0,4,7,14],
}
PC={'C':0,'D':2,'E':4,'F':5,'G':7,'A':9,'B':11}


def pitch(token):
    m=re.fullmatch(r'([A-G])([#b]?)(-?\d+)',token)
    if not m: raise ValueError('Invalid note '+token)
    return (int(m[3])+1)*12+PC[m[1]]+{'':0,'#':1,'b':-1}[m[2]]


def chord(token):
    symbol,_,slash=token.partition('/')
    m=re.fullmatch(r'([A-G])([#b]?)(.*)',symbol)
    if not m or m[3] not in INTERVALS: raise ValueError('Invalid chord '+token)
    root=PC[m[1]]+{'':0,'#':1,'b':-1}[m[2]]
    bass=PC[slash[0]]+({'#':1,'b':-1}.get(slash[1:],0)) if slash else root
    return root%12,INTERVALS[m[3]],bass%12


def parse_bar(text,meter):
    pos=0; events=[]
    for tok in text.split():
        m=re.fullmatch(r'([^:]+):([\d.]+)(?:@([\d.]+))?',tok)
        if not m: raise ValueError('Invalid note token '+tok)
        duration=float(m[2]); velocity=int(m[3]) if m[3] else None
        if duration<=0: raise ValueError('Non-positive duration')
        if m[1]!='r': events.append((pos,pitch(m[1]),duration,velocity))
        pos+=duration
    if abs(pos-meter)>1e-6: raise ValueError(f'Bar is {pos} beats, expected {meter}: {text}')
    return events


def load_score(pid):
    data=json.loads((SCORE_DIR/f'{pid}.json').read_text(encoding='utf-8'))
    if data['id']!=pid: raise ValueError('Score identity mismatch')
    return data


def compile_score(data):
    meter=data['meter']; beat=60/data['bpm']; events={st:[] for st in STEMS}
    objects=[]; sections=[]; total_bars=0; lead_audit=[]
    rng=np.random.default_rng(seed('individual-score-20261015:'+data['id']))
    role=data['instruments']
    def event(instrument,midi,at,gate,velocity=None,stem=None):
        ins=role[instrument]; st=stem or ins['stem']
        vel=velocity if velocity is not None else ins.get('velocity',80)
        vel=int(np.clip(vel+rng.integers(-3,4),1,127))
        # Up to 2 ms of deterministic timing variation on accompaniment only.
        offset=0 if st=='melody' or ins.get('drum') else rng.uniform(0,.002)
        events[st].append((round((at*beat+offset)*SR),ins['program'],int(midi),vel,
                           max(1,round(gate*beat*SR)),ins.get('pan',0),ins.get('drum',False)))
    for section in data['form']:
        count=section['bars']; sections.append({'name':section['name'],'bar':total_bars,'bars':count})
        harmony=data['harmony'][section['harmony']]
        for local in range(count):
            absolute=total_bars+local; at=absolute*meter
            root,ivs,bass_pc=chord(harmony[local%len(harmony)])
            # Bass is specified by each score as explicit relative chromatic
            # offsets, never inferred from a shared diatonic progression.
            bass_spec=data['bass'][section['bass']]
            bass_bar=bass_spec['patterns'][local%len(bass_spec['patterns'])]
            for offset,semitones,dur,vel in bass_bar:
                if isinstance(semitones,str):
                    # A notated chord member (e.g. chord:1) follows that bar's
                    # authored chord quality, including its minor/major third.
                    kind,index=semitones.split(':')
                    if kind!='chord': raise ValueError('Unknown bass notation '+semitones)
                    semitones=(root-bass_pc)%12+ivs[int(index)]
                note=12*(bass_spec.get('octave',1)+1)+bass_pc+semitones
                event(bass_spec['instrument'],note,at+offset,dur,vel)
            # Voicing and comp attack grids are part of the song's own score.
            for comp_name in section.get('comp',[]):
                spec=data['comp'][comp_name]
                grid=spec['grid'][local%len(spec['grid'])]
                voicing=spec.get('voicing',list(range(min(4,len(ivs)))))
                for offset,gate,vel in grid:
                    for k,index in enumerate(voicing):
                        octaves,index=divmod(index,len(ivs))
                        midi=12*(spec.get('octave',3)+1)+root+ivs[index]+octaves*12
                        # Fold into each instrument's register; explicit score
                        # overrides allow wide orchestral or compact jazz voicings.
                        while midi>spec.get('top',79): midi-=12
                        while midi<spec.get('bottom',48): midi+=12
                        event(spec['instrument'],midi,at+offset+k*spec.get('strum',.007),gate,vel)
            groove=data['drums'][section['drums']]
            for instrument,part in groove.items():
                if instrument=='fills': continue
                drum_ins=role[instrument]
                patterns=part['patterns']; pattern=patterns[local%len(patterns)].replace(' ','')
                steps=meter*4
                if len(pattern)!=steps: raise ValueError(f'{data["id"]} {instrument} pattern {pattern} has {len(pattern)} steps')
                for k,char in enumerate(pattern):
                    if char=='.': continue
                    off=k*.25
                    if k%2: off+=section.get('swing',data.get('swing',0))
                    vel={'X':part.get('velocity',95),'x':part.get('velocity',95)*.72,'o':part.get('velocity',95)*.42}[char]
                    event(instrument,part['note'],at+off,part.get('gate',.1),round(vel))
            # Each track authors its own fills rather than taking a universal tom run.
            fill=groove.get('fills',{})
            if str(local%fill.get('every',999)) in fill.get('bars',{}):
                for name,midi,off,gate,vel in fill['bars'][str(local%fill['every'])]:
                    event(name,midi,at+off,gate,vel)
            for name in section.get('objects',[]):
                spec=data['objects'][name]
                for off,gain in spec['grid'][local%len(spec['grid'])]:
                    objects.append((at+off,spec['material'],spec['duration'],absolute*37+round(off*31),gain,spec.get('pan',0)))
            for line in section.get('lines',[]):
                bars=data['phrases'][line['phrase']]
                if line.get('only') and local not in line['only']: continue
                # A phrase can start with pickups or leave entire accompaniment bars bare.
                bar_index=(local+line.get('phase',0))%len(bars)
                for off,midi,dur,vel in parse_bar(bars[bar_index],meter):
                    midi+=line.get('transpose',0)
                    gate=dur*line.get('gate',.83)
                    event(line['instrument'],midi,at+off,gate,vel or line.get('velocity'),line.get('stem'))
                    if line.get('primary',False): lead_audit.append([round(at+off,5),midi,round(dur,5),section['name']])
        total_bars+=count
    if any(e[2]<0 or e[2]>127 for es in events.values() for e in es): raise ValueError('MIDI pitch out of range')
    n=round(total_bars*meter*beat*SR)
    for es in events.values():
        if not es: raise ValueError('Empty adaptive stem '+data['id'])
        if any(e[0]>=n or e[0]<0 for e in es): raise ValueError('Event outside loop')
    return events,objects,n,sections,lead_audit


def render(sampler,data,out,preview,original,encode):
    events,objects,n,sections,lead=compile_score(data)
    beat=60/data['bpm']; bufs={}
    mix_settings=data['mix']
    for st in STEMS:
        print(f'RENDER {data["id"]}/{st}',flush=True)
        x=sampler.render(events[st],n)
        if st=='rhythm':
            for at,material,duration,var,gain,pan in objects:
                add(x,object_hit(material,duration,var),at*beat,gain,True,pan)
        # Reverb, width and instrument balance are authored per track.
        x=circular_filter(circular_filter(x,30 if st=='base' else 45,'highpass'),mix_settings.get('lowpass',12500))
        wet=mix_settings['reverb'].get(st,0)
        if wet:
            x=circular_reverb(x,wet=wet,seconds=mix_settings.get('room',.75),
                              seed=seed(data['id'])%100000,damp=mix_settings.get('damping',5800))
        x-=x.mean(axis=0)
        rms=np.sqrt(np.mean(x*x))
        if rms<1e-9: raise RuntimeError('Silent '+st)
        x*=10**(mix_settings['rms'][st]/20)/rms
        # Mild saturation at the stem level only; linked mastering follows.
        x=.85*np.tanh(x/.85)
        bufs[st]=x
    mix=sum(bufs.values()); gain=min(1,.8/np.abs(mix).max())
    folder=out/'Music'/data['id']; folder.mkdir(parents=True,exist_ok=True)
    master=preview/'Masters'/data['id']; master.mkdir(parents=True,exist_ok=True)
    for st,x in bufs.items():
        x*=gain
        sf.write(str(master/f'{data["id"]}_{st}.wav'),x,SR,subtype='PCM_24')
        encode(master/f'{data["id"]}_{st}.wav',folder/f'{data["id"]}_{st}.ogg')
    bars=sum(s['bars'] for s in data['form'])
    piece=dict(original,title=data['title'],bpm=data['bpm'],beatsPerBar=data['meter'],bars=bars,
               loopSamples=n,loopSeconds=n/SR,family='toybox_individual_v4')
    score=dict(piece,revision=4,scoreFile='Tools/Audio/Scores/'+data['id']+'.json',
               identity=data['identity'],key=data['key'],sections=sections,
               instruments=data['instruments'],primaryLead=lead,
               compositionSha256=hashlib.sha256(json.dumps(data,sort_keys=True).encode()).hexdigest(),
               sampleEvents={k:len(v) for k,v in events.items()})
    print(f'COMPOSED {data["id"]}: {bars} bars, {n/SR:.2f}s; {data["identity"]}',flush=True)
    return piece,score
