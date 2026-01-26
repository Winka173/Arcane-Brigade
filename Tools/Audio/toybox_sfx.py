"""Object-led redesign of every cue, plus all 53 requested additions.

Existing event lengths and animation landing offsets are preserved. Frequent
sounds use small dry resonators; rewards use sampled toy instruments. Synthetic
gibberish voices remain deliberately nonverbal and acquire shop-object timbres.
"""
import json
from pathlib import Path
import re

import numpy as np
from scipy.signal import resample_poly
import soundfile as sf

import build_sfx as old
import sfx_gen as g
import kit as K
from gen_agent_brief import NEW_CUES
from sfx_kit_extra import swoosh, seq, thump, nf, zip_sweep, periodic_noise
from toybox_engine import SR, ROOT, add, circular_filter, edges, filt, fit, object_hit, seed


def prompts():
    text=(ROOT/'Docs/Audio/Music_Prompts_Toybox.md').read_text(encoding='utf-8')
    rows={}
    for block in text.split('\n### ')[1:]:
        head=re.match(r'\d+\. `([^`]+)` - (\d+) variations? - ([\d.]+) s([^\n]*)',block)
        if not head: continue
        body=re.search(r'```\n(.*?)\n```',block,re.S)
        rows[head[1]]={'count':int(head[2]),'seconds':float(head[3]),'loop':'LOOP' in head[4],
                       'description':body[1] if body else ''}
    if len(rows)!=206: raise ValueError(f'Expected 206 Toybox cues, found {len(rows)}')
    return rows


def instrument_overrides(sampler):
    """Replace electronic reward tones in the old choreography with sampled toys."""
    def voice(program):
        def play(f,dur,vel=1.,*args,**kwargs):
            midi=int(np.clip(round(69+12*np.log2(max(f,1)/440)),24,108))
            x=sampler.note(program,midi,min(round(dur,2),1.8),84)
            return fit(x,round((dur+.12)*SR))*vel*2.2
        return play
    K.glock=voice(9); K.vibes=voice(11); K.rhodes=voice(4); K.pluck=voice(10)
    K.organ=voice(82)
    def chord_stab(freqs,dur=.2,vel=1.,kind='brass',*args,**kwargs):
        program={'brass':61,'pluck':3,'guitar':24}.get(kind,82)
        parts=[voice(program)(f,dur,vel)/max(1,len(freqs))**.5 for f in freqs]
        n=max(map(len,parts)); return sum(fit(x,n) for x in parts)
    K.chord_stab=chord_stab


def toy_phrase(sampler,n,notes=(67,72,74,76,76,74,72),program=10,start=0,span=.5,gain=1.):
    y=np.zeros(n)
    for k,midi in enumerate(notes):
        add(y,sampler.note(program,midi,.12,88),start+span*k/max(len(notes)-1,1),gain)
    return y


def ui_sound(sampler,cid,i,n,rng):
    kind=cid.split('.')[-1]
    root=[72,74,76,79][i%4] + (2 if kind=='setting' else 0)
    duration=n/SR
    y=np.zeros(n)
    mechanical=object_hit('wood',min(duration,.055),i)
    add(y,mechanical,0,.45)
    if kind in ('page','receipt'):
        for k in range(3 if kind=='page' else 7):
            add(y,object_hit('paper',.045,i+k),k*duration/10,.3)
        return y
    if kind in ('hover','tab','setting','tooltip'):
        # Tiny key/tine, warm and deliberately free of piercing square-wave chirps.
        add(y,sampler.note(10 if kind=='tooltip' else 3,root+12,.025,73),0,3.5)
    elif kind in ('confirm','resume','select'):
        notes=(root,root+4,root+7) if kind=='resume' else (root,root+7) if kind=='confirm' else (root,)
        y+=toy_phrase(sampler,n,notes,10,0,min(duration*.45,.14),4)
    elif kind in ('back','close','pause','locked','error'):
        y+=toy_phrase(sampler,n,(root+3,root-2),3,0,min(.08,duration*.3),5)
        if kind in ('locked','error'): add(y,object_hit('spring',min(duration,.12),i),0,.2)
    elif kind in ('unlock','achievement'):
        y+=toy_phrase(sampler,n,(67,72,74,76,79),9,.035,min(.36,duration*.45),4)
        add(y,sampler.note(56,72,.2,75),.09,2)
    else:
        y+=toy_phrase(sampler,n,(root,root+4),3,0,min(.08,duration*.2),4)
    return y


def replacement(sampler,c,i,n):
    cid=c['id']; dur=n/SR
    rng=np.random.default_rng(seed(('midnight-toybox-v3',cid,i)))
    if cid.startswith('ui.'):
        return ui_sound(sampler,cid,i,n,rng)
    if cid.startswith('cmb.hit.'):
        return object_hit(cid.split('.')[-1],dur,i)
    if cid.startswith('enemy.die.') and cid!='enemy.die.large':
        material=cid.split('.')[-1]
        y=object_hit(material,dur,i+11,1.3)
        add(y,object_hit('spring',min(.19,dur),i+19),.018,.17)
        return y
    if cid.startswith('cmb.swing.'):
        kind=cid.split('.')[-1]
        x=fit(g.swing(i,rng,kind),n)*.65
        material={'blade':'metal','spear':'wood','hammer':'wood','scissors':'metal','umbrella':'fabric','whip':'spring'}[kind]
        add(x,object_hit(material,min(.09,dur),i),0,.28)
        return x
    if cid=='sig.musicbox':
        return toy_phrase(sampler,n,tuple(m+2*i for m in (67,72,76)),10,0,.24+.008*i,4)
    if cid.startswith('loot.coin.'):
        y=fit(g.coin(i,rng,c['kw']['mode']),n)*.45
        add(y,object_hit('metal',min(dur,.16),i),0,.13)
        if cid.endswith('pickup'): add(y,sampler.note(10,[84,88,91,86,89][i%5],.045,82),0,3)
        return y
    if cid.startswith('sting.'):
        down=cid=='sting.defeat'
        notes=(76,74,72,67,64,60) if down else (67,72,74,76,76,74,72)
        y=toy_phrase(sampler,n,notes,3 if down else 9,0,min(1.1,dur*.5),3)
        add(y,object_hit('wood',.24,i,1.5),0,.5)
        for m in ([48,51,55] if down else [48,52,55,60]):
            add(y,sampler.note(82,m,.65,70),min(.55,dur*.25),1.7)
        return y
    if c['loop']:
        # Keep the requested exact ambience loop length, redesigning the small
        # foreground events with dry object resonators and music-box tines.
        y=fit(c['fn'](i,rng,**c['kw']),n)*.6
        material='paper' if cid=='amb.office' else 'glass' if cid=='amb.dreamshop' else 'wood'
        for k in range(12):
            add(y,object_hit(material,.12,k+13),float(rng.uniform(0,dur)),.045,True)
        return y
    # Existing bespoke animation choreography is retained, but regenerated with
    # the new random stream and sampled instrument palette above.
    kw=dict(c['kw'])
    ii=i
    if cid.startswith('enemy.die.'):
        kw['size']='large'
    y=fit(c['fn'](ii,rng,**kw),n)
    if c['bus']=='Voice':
        # No real speech; gold speaks through a wooden drawer, dream through glass.
        tone='wood' if '.gold.' in cid else 'glass'
        add(y,object_hit(tone,min(.08,dur),i+4),0,.08)
        if '.dream.' in cid:
            add(y,sampler.note(10,84+i%5,.035,65),min(.015,dur*.08),.25)
    elif not c['danger']:
        # Give every remaining effect a tactile attack. Preserve arrivals at
        # 0.98 s / 0.405 s and gather/reveal separation exactly.
        at=.98 if cid.startswith('shop.keeper.enter') else .405 if cid=='up.ally.land' else 0
        mat='paper' if any(k in cid for k in ('carpet','chest','stamp')) else 'glass' if any(k in cid for k in ('dream','chill','ice')) else 'wood'
        add(y,object_hit(mat,min(.12,max(.01,dur-at)),i+27),at,.16)
    return y


def new_sound(sampler,cid,i,n):
    """Explicit scene/weapon/hero recipes, no generic fallback for mandatory cues."""
    rng=np.random.default_rng(seed(('toybox-new',cid,i)))
    dur=n/SR; y=np.zeros(n)
    def hit(mat='wood',at=0,gain=.6,weight=1.,length=.25):
        add(y,object_hit(mat,min(length,dur),i+round(at*100),weight),at,gain)
    def note(program,midi,at=0,gate=.15,gain=3):
        add(y,sampler.note(program,midi,gate,88),at,gain)
    def sw(at=0,span=None,up=True,gain=.35):
        length=span or dur*.75
        x=swoosh(rng,length,400 if up else 3500,4000 if up else 500,1.3,peak=.65,tone=.08)
        add(y,x,at,gain)
    def phrase(notes=(67,72,74,76),program=10,at=.02,span=None,gain=3):
        y[:]+=toy_phrase(sampler,n,notes,program,at,span if span is not None else min(.3,dur*.45),gain)
    def rattle(mat='metal',count=6,span=None,gain=.25):
        for k in range(count): hit(mat,k*(span or dur*.55)/count,gain*(1-k/(count*1.5)),length=.1)
    if cid.startswith('ui.cutin.') or cid=='ui.encore.ready':
        kind=cid.split('.')[-1]
        hit('wood',weight=1.7,gain=.8)
        if kind=='boss':
            phrase((50,53,57,50),61,.02,.35,3.5); rattle('metal',12,.35,.15)
        elif kind=='bond':
            phrase((67,72,74,76+i*2,79),9,.02,.28,3.5); rattle('wood',7,.35,.2)
        else:
            phrase((67,72,74,76,79,84),56,.03,.42,3); rattle('wood',10,.45,.19)
        sw(.03,dur*.6,gain=.15)
    elif cid in ('ui.wave.stamp','shop.ticket.sold'):
        hit('wood',gain=.8,weight=1.8); hit('paper',.003,.9)
        if cid.endswith('sold'): rattle('paper',4,dur*.6,.15)
    elif cid=='shop.ticket.sway':
        sw(0,dur*.8,gain=.2); hit('paper',.01,.2); hit('spring',.015,.06)
    elif cid=='ui.coach.pop':
        hit('spring',gain=.7,length=.16); note(10,84+i,gate=.04,gain=1.7)
    elif cid.startswith('menu.spot.'):
        kind=cid.split('.')[-1]
        if kind=='hero':
            add(y,g.syllable(i,rng,'gold'),0,.45); note(82,60,0,.15,1)
        elif kind=='door':
            hit('wood',gain=.4,length=.4); sw(.015,dur*.7,gain=.3)
        elif kind=='jar':
            hit('glass',gain=.5,length=.3); phrase((84,88,91),9,span=.13,gain=1.8)
        elif kind=='printer': rattle('wood',13,dur*.65,.3); rattle('paper',7,dur*.7,.3)
        elif kind=='lamp': hit('wood',gain=.3,length=.035); note(89,60,0,.18,1.2)
        elif kind=='bell': note(9,88+i,gate=.1,gain=6); hit('metal',gain=.2,length=.2)
    elif cid=='wpn.yoyo.throw': rattle('spring',7,dur*.4,.25); sw(0,dur*.9,gain=.6)
    elif cid=='wpn.yoyo.return': sw(0,dur*.8,False,.5); hit('wood',dur*.78,.75,length=.06)
    elif cid=='wpn.magnet.pull':
        note(89,45,0,dur*.5,3); rattle('metal',8,dur*.7,.3); sw(0,dur*.8,gain=.4)
    elif cid=='wpn.needle.volley':
        for k in range(3): note(10,88+k*2+i%2,k*.045,.025,3); hit('metal',k*.045,.08,length=.04)
    elif cid=='wpn.spring.slam': hit('spring',gain=.4,length=.18); hit('wood',.1,.9,2); sw(0,.13,gain=.15)
    elif cid=='wpn.projector.cast': rattle('wood',15,dur*.8,.16); note(92,72,0,.2,2); sw(0,dur*.8,gain=.3)
    elif cid=='wpn.ribbon.cut':
        sw(0,dur*.45,gain=.4); sw(dur*.45,dur*.45,False,.35); note(10,79+i,0,.05,2)
    elif cid.startswith('hero.'):
        kind=cid.split('.')[1]
        if kind=='lantern':
            add(y,g.fire_cast(i,rng),0,.6); note(9,72+i,0,.12,4)
        elif kind=='parcel': rattle('paper',4,dur*.4,.7); hit('wood',gain=.5); sw(0,dur*.7,gain=.3)
        elif kind=='dream':
            sw(0,dur*.8,False,.25); phrase((84,88,91,96),10,gain=3); note(52,72,0,dur*.5,1)
        elif kind=='rooted': hit('wood',gain=.5,weight=2,length=.5); note(89,48,0,.3,3)
        elif kind=='solo': hit('wood',gain=.8,weight=2); hit('fabric',.02,.6); sw(0,dur*.9,gain=.5)
        elif kind=='glass': rattle('wood',5,.14,.4); hit('glass',.12,.7); note(9,91,.14,.12,4)
        elif kind=='tide':
            add(y,g.tea_splash(i,rng),0,.6); phrase((72,76,79,84),10,gain=3)
        elif kind=='clock': rattle('wood',7,dur*.8,.3); sw(0,dur*.8,False,.35)
        elif kind=='mirror': hit('glass',gain=.5); phrase((91,86,93),9,span=.15,gain=2.5)
        elif kind=='treasure': hit('wood',gain=.4); rattle('metal',4,.2,.25); phrase((84,88,91),10,gain=2)
        elif kind=='charge': phrase((84,91),9,span=.07,gain=3); hit('wood',gain=.2,length=.04)
        else: raise ValueError(cid)
    elif cid.startswith('event.'):
        kind=cid.split('.')[1]
        if kind=='warn':
            for k in range(26):
                at=dur*(1-(1-k/28)**1.6)
                hit('wood',at,.08+.4*k/26,length=.045)
            sw(0,dur*.98,gain=.45)
        elif kind=='parade': rattle('paper',10,dur*.6,.35); phrase((67,72,74,76),56,gain=3)
        elif kind=='trap': hit('metal',gain=.6,weight=2); phrase((84,88,91),9,gain=3)
        elif kind=='eclipse': note(89,43,0,.4,4); phrase((83,78,72),11,span=.3,gain=2); sw(gain=.4)
        elif kind=='reward': rattle('metal',9,dur*.6,.35); phrase((67,72,74,76),9,gain=3)
        elif kind=='end': sw(0,dur*.8,False,.4); phrase((79,76,72),10,span=.15,gain=2)
        else: raise ValueError(cid)
    elif cid.startswith('boss.voice.'):
        kind=cid.split('.')[-1]
        if kind=='choirengine':
            for m in (38,45,50,53): note(19,m,0,.65,3)
            rattle('metal',10,.3,.2)
        elif kind=='coinwidow':
            rattle('metal',18,dur*.75,.4); add(y,g.vocal_gesture(0,rng,'gold','laugh'),.05,.5)
        elif kind=='lastlantern': add(y,g.fire_cast(i,rng),0,.9); note(14,50,0,.5,3)
        elif kind=='marqueemaw': hit('wood',gain=.8,weight=3); hit('metal',.12,.6); note(81,38,0,.6,3)
        elif kind=='scarletindex': rattle('paper',21,dur*.8,.5); sw(0,dur*.8,gain=.35)
        elif kind=='sleepcollector':
            add(y,g.vocal_gesture(0,rng,'gold','hmm'),0,.7); phrase((79,76,74,70),10,.15,.5,3)
        elif kind=='suitcaseleviathan': hit('wood',gain=.8,weight=3); hit('metal',.04,.4); note(58,36,.06,.65,4)
        elif kind=='umbracradle': hit('wood',gain=.4,weight=2,length=.6); sw(0,dur*.9,False,.3); note(92,45,0,.65,4)
        elif kind=='undeliveredking': phrase((48,55,60,58),60,0,.45,4); hit('paper',.52,.7); hit('wood',.52,.6)
        elif kind=='waxadmiral': note(58,36,0,.7,5); add(y,g.fire_cast(i,rng),.05,.35)
        else: raise ValueError(cid)
    elif cid.startswith('enemy.shot.'):
        kind=cid.split('.')[-1]
        if kind=='arc':
            hit('wood',gain=.8,weight=1.4); sw(0,dur*.95,gain=.45)
        elif kind=='spiral':
            for k in range(3): add(y,zip_sweep((700+k*200)*(1+.07*i),(2200+k*300)*(1+.05*i),.2),k*(.06+.004*i),.17)
        elif kind=='burst':
            hit('spring',gain=.65); rattle('wood',5,dur*.6,.3); sw(0,dur*.7,gain=.4)
        else: raise ValueError(cid)
    elif cid=='ui.merge.hint': phrase((84+2*i,88+2*i),10,span=.11+.008*i,gain=3.5)
    else: raise ValueError('No authored Toybox recipe for '+cid)
    return y


def master(x,n,loop=False):
    y=fit(x,n)
    if loop:
        y=circular_filter(circular_filter(y,30,'highpass'),13000)
    else:
        y=edges(filt(filt(y,30,'highpass'),13000),.0008,min(.035,n/SR*.18))
    y-=y.mean()
    # Reapply edges after DC removal so quantisation cannot introduce an edge click.
    if not loop: y=edges(y,.0005,min(.018,n/SR*.12))
    peak=np.abs(y).max()
    if peak<1e-8: raise ValueError('Silent render')
    y*=10**((-7 if loop else -3)/20)/peak
    return y


def build(sampler,baseline,out):
    requested=prompts()
    instrument_overrides(sampler)
    recipes={c['id']:c for c in old.CUES}
    existing={c['id']:c for c in baseline['cues']}
    new_specs={cid:(count,bus,length,desc) for cid,count,bus,length,desc in NEW_CUES}
    cues=[]; rows=[]
    for cid,prompt in requested.items():
        if cid in existing:
            cue=dict(existing[cid]); paths=cue['clips']
            if len(paths)!=prompt['count']: raise ValueError('Variation mismatch '+cid)
            if cue['loop']!=prompt['loop']: raise ValueError('Loop flag mismatch '+cid)
        else:
            count,bus,length,desc=new_specs[cid]
            lengths=[float(s) for s in re.findall(r'[\d.]+',length)]
            duration=sum(lengths)/len(lengths)
            danger=cid in ('ui.cutin.boss','event.warn') or cid.startswith('enemy.shot.')
            cue=dict(id=cid,bus=bus,priority=45 if danger else 100 if bus=='UI' else 140,
                     gainDb=-12 if danger else -17 if bus=='Combat' else -15,
                     cooldown=.3 if danger else .09,maxVoices=1 if danger else 2,
                     pitchJitter=0 if danger or bus=='UI' else .02,loop=False,danger=danger,
                     clips=[f'Sfx/{cid.replace(".","_")}_{i:02d}' for i in range(count)])
            # Small discoverability details sit below purchases and danger.
            if cid=='shop.ticket.sway': cue.update(gainDb=-28,priority=200,cooldown=.4)
            if cid in ('ui.coach.pop','hero.charge.ready','ui.merge.hint'):
                cue.update(gainDb=-22,cooldown=.8)
            if cid.startswith('menu.spot.'): cue.update(gainDb=-19,cooldown=.25)
            paths=cue['clips']
        for i,rel in enumerate(paths):
            sampler.used_programs.clear()
            n=baseline['assets'][rel]['frames'] if cid in existing else round(duration*SR)
            x=replacement(sampler,recipes[cid],i,n) if cid in existing else new_sound(sampler,cid,i,n)
            x=master(x,n,cue['loop'])
            path=out/(rel+'.wav'); path.parent.mkdir(parents=True,exist_ok=True)
            sf.write(str(path),x,SR,subtype='PCM_16')
            rows.append({'path':rel+'.wav','cue':cid,'variation':i,'frames':n,'seconds':n/SR,
                         'source':'original synthesis + GeneralUser GS sampled accents' if sampler.used_programs else 'original synthesis',
                         'samplePrograms':sorted(sampler.used_programs),
                         'prompt':prompt['description'],
                         'hook':'existing mapping/reservation retained' if cid in existing else
                                'reserved: Encore meter not implemented' if cid=='ui.encore.ready' else
                                'Toybox runtime hooks; see Toybox_Delivery.md'})
        cues.append(cue)
        print('SFX',cid,len(paths),flush=True)
    (out/'cues.json').write_text(json.dumps({'version':1,'cues':cues},indent=1)+'\n')
    return rows
