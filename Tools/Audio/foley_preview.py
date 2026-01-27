"""Audition every v5 cue in labelled category reels, with exact cue timelines."""
import html
import json
import numpy as np
import soundfile as sf
from toybox_engine import ROOT, SR
from toybox_music import encode_vorbis

A=ROOT/'Assets/ArcaneBrigade/Resources/Audio/Arcane'
P=ROOT/'PreviewExports/Audio/Toybox'

CATEGORIES={
    'materials':'Vật liệu · va chạm và vỡ',
    'weapons':'Vũ khí và đồ vật đặc trưng',
    'combat':'Chiến đấu · cảnh báo và di chuyển',
    'heroes':'Kỹ năng riêng của các hero',
    'bosses':'Boss · giọng riêng và sân khấu',
    'interface':'Giao diện · cửa hàng và phần thưởng',
    'voices':'Hai chủ tiệm · giọng không lời',
    'ambience':'Sáu không gian · ambience và chuyển cảnh',
}


def category(cid):
    if cid.startswith(('cmb.hit.','enemy.die.')): return 'materials'
    if cid.startswith(('wpn.','sig.')): return 'weapons'
    if cid.startswith(('voice.',)): return 'voices'
    if cid.startswith('boss.'): return 'bosses'
    if cid.startswith('hero.'): return 'heroes'
    if cid.startswith('amb.'): return 'ambience'
    if cid.startswith(('cmb.','plr.','chr.','enemy.','fx.','event.')): return 'combat'
    return 'interface'


def build():
    cues=json.loads((A/'cues.json').read_text())['cues']
    designs={d['id']:d for d in json.loads((ROOT/'Tools/Audio/SfxDesigns.json').read_text())['designs']}
    timeline=[];buttons=[]
    for name,title in CATEGORIES.items():
        segments=[];entries=[];cursor=0
        for c in cues:
            if category(c['id'])!=name: continue
            x,_=sf.read(str(A/(c['clips'][0]+'.wav')))
            if c['loop']:
                x=x[:SR*6].copy()
                ramp=np.linspace(0,1,round(.025*SR));x[:len(ramp)]*=ramp;x[-len(ramp):]*=ramp[::-1]
                level=.65
            else: level=10**((c['gainDb']+15)/20)*.55
            entries.append(dict(cue=c['id'],at=round(cursor,4),seconds=len(x)/SR,
                                identity=designs[c['id']]['identity'],variation=0,loopExcerpt=c['loop']))
            segments.extend([x*level,np.zeros(round(SR*.22))]);cursor+=len(x)/SR+.22
        reel=np.concatenate(segments)
        path=P/'Masters'/('foley_'+name+'.wav');path.parent.mkdir(parents=True,exist_ok=True)
        sf.write(str(path),reel,SR,subtype='PCM_24');encode_vorbis(path,P/('foley_'+name+'.ogg'))
        timeline.append(dict(category=name,title=title,file='foley_'+name+'.ogg',seconds=len(reel)/SR,entries=entries))
        buttons.append(f'<button class="track" data-src="foley_{name}.ogg"><b>{html.escape(title)}</b><small>{len(entries)} cue · {len(reel)/SR:.0f}s · biến thể đầu tiên; ambience nghe trích đoạn 6s</small></button>')
    (P/'Foley_Timelines.json').write_text(json.dumps(timeline,ensure_ascii=False,indent=1)+'\n',encoding='utf-8')
    return ''.join(buttons),designs


if __name__=='__main__': build()
