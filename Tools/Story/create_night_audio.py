"""Deterministic original PCM ambience and Foley. No external recordings or voices."""
import math,random,wave,array
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/ArcaneBrigade/Resources/MienCho/Audio';OUT.mkdir(parents=True,exist_ok=True)
RATE=22050
def write(name,seconds,kind):
    rng=random.Random(161026+kind);samples=array.array('h');low=0
    total=int(seconds*RATE)
    for i in range(total):
        t=i/RATE;raw=rng.uniform(-1,1);low=low*.985+raw*.015
        if kind==0:
            value=(low*.8+math.sin(t*math.tau*53)*.018+math.sin(t*math.tau*79)*.012)*(.55+.2*math.sin(t*math.tau/seconds))
            value*=math.sin(math.pi*t/seconds)**.25
        elif kind==1:
            value=raw*math.exp(-t*8)*.075+sum(math.sin(t*math.tau*f)*math.exp(-t*(4+j))*.045 for j,f in enumerate((1173,1831,2789,3569)))
            value*=min(1,t*120)
        elif kind==2:
            value=(raw*.08+low*1.2)*math.exp(-t*19)*min(1,t*90)
        else:
            base={3:55,4:65.406,5:48.999}[kind]
            swell=.55+.25*math.sin(t*math.tau/seconds)
            value=(math.sin(t*math.tau*base)*.018+math.sin(t*math.tau*base*1.498)*.012+low*.32)*swell
            # An unresolved, widely spaced bell; no imitation of a hostile combat cue.
            bell=t%4;value+=math.sin(bell*math.tau*(base*8))*math.exp(-bell*2.1)*.026
            value*=math.sin(math.pi*t/seconds)**.2
        samples.append(int(max(-.8,min(.8,value))*32767))
    with wave.open(str(OUT/(name+'.wav')),'wb') as f:f.setnchannels(1);f.setsampwidth(2);f.setframerate(RATE);f.writeframes(samples.tobytes())
write('DarkWind',12,0);write('LanternBreak',1.2,1);write('ExtraFootstep',.4,2)
for room in range(1,4):write('Room'+str(room),12,room+2)
print('Six original PCM ambience/Foley clips saved:',OUT)
