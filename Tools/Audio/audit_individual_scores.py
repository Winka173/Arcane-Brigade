"""Score originality/structure diagnostics and editable MIDI exports.

Measures shared note-interval/rhythm windows without treating different keys as
different writing. These diagnostics do not certify catchiness or replace ears.
"""
import hashlib
import itertools
import json
from pathlib import Path

import mido

from toybox_music import IDS
from score_player import ROOT, SR, STEMS, load_score, compile_score, parse_bar

OUT=ROOT/'PreviewExports/Audio/Toybox'


def fingerprint(notes,size=6):
    intervals=[(notes[i+1][1]-notes[i][1],round(notes[i+1][0]-notes[i][0],3),round(notes[i][2],3)) for i in range(len(notes)-1)]
    return {tuple(intervals[i:i+size]) for i in range(len(intervals)-size+1)}


def export_midi(data,events):
    midi=mido.MidiFile(ticks_per_beat=960,charset='utf-8')
    meta=mido.MidiTrack();midi.tracks.append(meta)
    meta.extend([mido.MetaMessage('track_name',name=data['title']),
                 mido.MetaMessage('set_tempo',tempo=mido.bpm2tempo(data['bpm'])),
                 mido.MetaMessage('time_signature',numerator=data['meter'],denominator=4)])
    channel=0
    for st in STEMS:
        groups={}
        for at,program,pit,vel,gate,pan,drum in events[st]:
            groups.setdefault((program,pan,drum),[]).append((at,pit,vel,gate))
        for (program,pan,drum),notes in groups.items():
            if drum: ch=9
            else:
                ch=channel
                channel+=1
                if channel==9: channel+=1
                if ch>=16: raise ValueError('Too many MIDI channels '+data['id'])
            track=mido.MidiTrack();midi.tracks.append(track)
            track.append(mido.MetaMessage('track_name',name=f'{st}: program {program+1}'))
            track.append(mido.Message('program_change',channel=ch,program=program))
            track.append(mido.Message('control_change',channel=ch,control=10,value=round(64+pan*50)))
            seq=[];ratio=960*data['bpm']/(60*SR)
            for at,pit,vel,gate in notes:
                seq.append((round(at*ratio),1,pit,vel))
                seq.append((round((at+gate)*ratio),0,pit,0))
            previous=0
            for at,on,pit,vel in sorted(seq):
                track.append(mido.Message('note_on' if on else 'note_off',channel=ch,note=pit,velocity=vel,time=at-previous))
                previous=at
    dest=OUT/'MIDI';dest.mkdir(parents=True,exist_ok=True)
    midi.save(str(dest/(data['id']+'.mid')))


def main():
    OUT.mkdir(parents=True,exist_ok=True)
    records={};errors=[]
    for pid in IDS:
        d=load_score(pid);events,objects,n,sections,lead=compile_score(d)
        if len(set(s['name'] for s in sections))!=len(sections): errors.append('Ambiguous section names '+pid)
        export_midi(d,events)
        hook_line=next(line for line in d['form'][0]['lines'] if line.get('primary'))
        phrase=d['phrases'][hook_line['phrase']];hook=[]
        for bar,text in enumerate(phrase):
            hook.extend((bar*d['meter']+t,p,dur) for t,p,dur,v in parse_bar(text,d['meter']))
        ranges=[p for t,p,dur,_ in lead]
        harmony=tuple(tuple(d['harmony'][s['harmony']]) for s in d['form'])
        harmony_hash=hashlib.sha256(repr(harmony).encode()).hexdigest()
        grooves=json.dumps(d['drums'],sort_keys=True)
        records[pid]={'title':d['title'],'identity':d['identity'],'bars':sum(s['bars'] for s in sections),
                      'seconds':n/SR,'bpm':d['bpm'],'meter':d['meter'],'sections':sections,
                      'leadRangeMidi':[min(ranges),max(ranges)],'primaryNoteCount':len(lead),
                      'harmonySequenceHash':harmony_hash,'drumChartHash':hashlib.sha256(grooves.encode()).hexdigest(),
                      '_windows':fingerprint(hook),
                      'scoreSha256':hashlib.sha256(json.dumps(d,sort_keys=True).encode()).hexdigest()}
    pairs=[]
    for a,b in itertools.combinations(IDS,2):
        left=records[a];right=records[b];wa=left['_windows'];wb=right['_windows']
        overlap=len(wa&wb);ratio=overlap/max(1,min(len(wa),len(wb)))
        if left['harmonySequenceHash']==right['harmonySequenceHash']: errors.append('Identical harmony sequence '+a+' / '+b)
        if left['drumChartHash']==right['drumChartHash']: errors.append('Identical drum chart '+a+' / '+b)
        if ratio>.5: errors.append('Large shared interval/rhythm passage '+a+' / '+b)
        pairs.append({'a':a,'b':b,'sharedSevenNoteWindows':overlap,'sharedWindowFraction':ratio})
    for r in records.values(): r.pop('_windows')
    report={'status':'PASS' if not errors else 'FAIL','revision':4,'pieces':records,'pairs':pairs,'failures':errors,
            'interpretation':'No shared harmony sequence/drum chart; hook windows compare intervals and timing under transposition. A low overlap is not proof of perceptual uniqueness or enjoyment.',
            'listeningReview':'Not performed by the agent. Player audition required.'}
    (OUT/'Composition_Audit.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
    lines=['# Fifteen separately authored compositions — revision 4','',
           'Each song has its own JSON score, harmony, bass charts, drum charts, phrases, form and mix. No shared melody/bridge is injected. MIDI files are editable exports; the game uses the four rendered stems.','',
           '| Piece | Identity | Tempo / length |','| --- | --- | --- |']
    for pid,r in records.items():
        lines.append(f'| `{pid}` — {r["title"]} | {r["identity"]} | {r["bpm"]} BPM · {r["meter"]}/4 · {r["bars"]} bars · {r["seconds"]:.2f}s |')
    lines+=['','Technical comparisons are diagnostics, not a listening approval. See Composition_Audit.json for all 105 pairs.']
    (ROOT/'Docs/Audio/Toybox_Individual_Compositions.md').write_text('\n'.join(lines)+'\n',encoding='utf-8')
    print('Composition audit:',report['status'],'105 pairs; max shared hook-window fraction',max(p['sharedWindowFraction'] for p in pairs))
    print('15 MIDI scores exported.');print('\n'.join(errors))
    raise SystemExit(bool(errors))


if __name__=='__main__': main()
