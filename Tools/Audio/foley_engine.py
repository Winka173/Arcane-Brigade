"""Original Toybox Foley v5. No sample bank, legacy cue renderer or input audio.

Modal bodies, lossy strings, contact grains, shaped air, bubbles and additive
formant puppets are rendered at 88.2 kHz and bandlimited to 44.1 kHz. Layers are
authored in SfxDesigns.json. Randomness changes performances, not cue identity.
"""
import numpy as np
from scipy.signal import butter, sosfilt, lfilter, resample_poly

from toybox_engine import SR, seed

RATE = SR * 2
MATERIALS = {
    # root Hz, inharmonic modes, decay seconds, contact brightness
    'oak': (365, [1, 2.39, 4.83, 7.17], .052, 2400),
    'hollow': (228, [1, 1.93, 3.08, 5.43], .095, 1900),
    'tin': (720, [1, 1.62, 2.79, 4.38, 6.31], .115, 4700),
    'steel': (1260, [1, 2.756, 5.404, 8.93], .19, 6200),
    'porcelain': (1080, [1, 1.466, 2.486, 4.071, 5.83], .23, 5100),
    'glass': (1740, [1, 1.593, 2.316, 3.76, 5.18], .28, 6700),
    'paper': (280, [1, 2.17, 3.37], .012, 5400),
    'felt': (135, [1, 1.46, 2.31], .031, 950),
    'wax': (190, [1, 1.28, 2.82], .045, 1200),
    'skin': (92, [1, 1.594, 2.136, 2.91, 3.598], .13, 2500),
    'tine': (880, [1, 6.267, 17.55], .30, 4000),
    'bell': (660, [1, 2, 2.4, 3, 4.16, 5.43], .52, 4800),
    'coin': (2450, [1, 1.41, 2.09, 2.63], .072, 6300),
}


def low(x, hz):
    return sosfilt(butter(2, min(hz, RATE*.45), fs=RATE, output='sos'), x)


def band(x, lo, hi):
    return sosfilt(butter(2, [lo, min(hi, RATE*.45)], btype='bandpass', fs=RATE, output='sos'), x)


def envelope(t, attack=.001, release=.02):
    return np.minimum(1, t/max(attack, 1/RATE)) * np.minimum(1, (len(t)/RATE-t)/max(release, 1/RATE))


def primitive(layer, rng, variation, fixed=False):
    dur=layer['dur']; t=np.arange(max(8,round(dur*RATE)))/RATE
    n=len(t); u=t/dur; kind=layer['kind']
    # Very narrow intonation variation for warnings; contact/timing still vary.
    detune=2**(rng.uniform(-.08 if fixed else -.65,.08 if fixed else .65)/12)
    pitch=layer.get('pitch',1)*detune
    if 'notes' in layer:
        pitch *= 2**((layer['notes'][variation%len(layer['notes'])]-69)/12)*440
    else:
        pitch *= layer.get('hz',1)
    noise=rng.normal(0,1,n)
    if kind=='body':
        root,ratios,decay,bright=MATERIALS[layer['material']]
        freq=pitch if 'notes' in layer or 'hz' in layer else root*pitch
        decay*=layer.get('decay',1)
        y=np.zeros(n)
        strike=rng.uniform(.18,.47)
        for k,r in enumerate(ratios):
            f=freq*r
            if f>16000: continue
            amp=(.85+.15*np.sin(np.pi*(k+1)*strike))/(1+k)**1.48
            y+=amp*np.sin(2*np.pi*f*t)*np.exp(-t/(decay/(1+k*.46)))
        contact=low(noise,bright)*np.exp(-t/(.004 if layer['material'] not in ('paper','felt','wax') else .018))
        y+=contact*layer.get('contact',.22)
        if layer['material']=='bell': y*=.87+.13*np.cos(2*np.pi*6.3*t)
    elif kind=='air':
        start,end=layer.get('from',400),layer.get('to',4200)
        a=band(noise,max(65,start*.5),max(180,start*1.7))
        b=band(noise,max(65,end*.5),max(180,end*1.7))
        sweep=u**layer.get('curve',1)
        y=((1-sweep)*a+sweep*b)*np.sin(np.pi*u)**layer.get('shape',1.5)
        if layer.get('whistle'):
            phase=2*np.pi*(start*t+(end-start)*t*t/(2*dur))
            y+=layer['whistle']*np.sin(phase)*np.sin(np.pi*u)**2
        y*=1+.22*np.sin(2*np.pi*layer.get('flutter',0)*t)
    elif kind=='grain':
        mat=layer.get('material','paper')
        bright={'paper':6500,'felt':1800,'tin':4300,'oak':2900,'wax':1300}[mat]
        rate=layer.get('rate',70)
        phase=2*np.pi*(rate*t+.025*rate*np.sin(2*np.pi*7*t))
        impulses=np.maximum(0,np.sin(phase))**6
        y=band(noise,170,bright)*(.08+impulses)
        y*=np.sin(np.pi*u)**.6
    elif kind=='spring':
        f=layer.get('hz',240)*detune
        phase=2*np.pi*f*(t+.006*(1-np.exp(-t/.006)))
        phase+=layer.get('bend',11)*np.sin(2*np.pi*layer.get('rate',23)*t)*np.exp(-t/.13)
        y=(np.sin(phase)+.22*np.sin(2.031*phase))*np.exp(-t/layer.get('decay',.09))
    elif kind=='string':
        # Lossy waveguide, fractional-frequency rounded only at 88.2k.
        f=layer.get('hz',180)*detune; delay=max(8,round(RATE/f))
        excitation=np.zeros(n); excitation[:min(delay,n)]=noise[:min(delay,n)]
        a=np.zeros(delay+2);a[0]=1
        a[delay]=a[delay+1]=-layer.get('feedback',.978)*.5
        y=lfilter([1],a,excitation)*np.exp(-t/layer.get('decay',.2))*2.2
    elif kind=='bubble':
        y=np.zeros(n)
        for k in range(layer.get('count',9)):
            at=rng.uniform(0,dur*.75); v=np.maximum(0,t-at)
            f=rng.uniform(210,900)*layer.get('pitch',1)
            bubble=np.sin(2*np.pi*f*(v+.018*(1-np.exp(-v/.014))))*np.exp(-v/rng.uniform(.012,.045))
            y+=bubble*(t>=at)*rng.uniform(.15,.6)
        y+=low(noise,1700)*np.sin(np.pi*u)*.12
    elif kind=='voice':
        f=layer.get('hz',150)*detune
        contour=layer.get('contour',[1,1.1,.9])
        f0=f*np.interp(u,np.linspace(0,1,len(contour)),contour)
        f0*=1+.009*np.sin(2*np.pi*5.8*t)+.003*np.sin(2*np.pi*37*t)
        phase=2*np.pi*np.cumsum(f0)/RATE
        vowels={'a':(730,1090,2440),'o':(500,880,2450),'u':(330,780,2200),'e':(530,1840,2480),'i':(270,2290,3010),'m':(260,1050,2150)}
        v=layer.get('vowel','a'); v=v[variation%len(v)]
        formants=np.array(vowels[v])*layer.get('formant',1)
        y=np.zeros(n)
        for h in range(1,min(65,int(12500/max(f*.7,30)))):
            freq=f0*h
            gains=sum(np.exp(-.5*((freq-ff)/(80+j*65))**2)*(1-.22*j) for j,ff in enumerate(formants))
            gains+=.06*np.exp(-freq/1800)
            y+=np.sin(h*phase+.07*np.sin(2*np.pi*19*t))*gains/h**.85
        y+=band(noise,650,5400)*layer.get('breath',.06)
        if layer.get('rasp'): y*=.78+.22*np.sin(phase*.5)
        y*=np.sin(np.pi*u)**layer.get('shape',.75)
    elif kind=='reed':
        f=pitch if 'notes' in layer else layer.get('hz',220)*detune
        phase=2*np.pi*f*t+.018*np.sin(2*np.pi*5.4*t)
        y=sum(np.sin(h*phase)*np.exp(-h/layer.get('brightness',5))/h for h in range(1,24) if h*f<15000)
        y+=low(noise,4500)*.025
        y*=envelope(t,.022,min(.13,dur*.4))*(.84+.16*np.sin(np.pi*u))
    elif kind=='rumble':
        f=layer.get('hz',70)*detune; end=layer.get('to',32)*detune
        phase=2*np.pi*(end*t+(f-end)*.028*(1-np.exp(-t/.028)))
        decay=layer.get('decay',.13)*rng.uniform(.94,1.06)
        y=np.sin(phase)*np.exp(-t/decay)+.15*low(noise,230)*np.exp(-t/.06)
    elif kind=='bed':
        # FFT coloured noise is periodic by construction, unlike a faded room tone.
        spec=np.fft.rfft(noise);freq=np.fft.rfftfreq(n,1/RATE)
        weight=np.sqrt(np.maximum(freq,35))**-1
        weight*=np.exp(-(freq/layer.get('cutoff',2100))**2)
        weight[freq<35]=0; y=np.fft.irfft(spec*weight,n=n)
        y/=max(np.std(y),1e-9)
        y*=.86+.09*np.sin(2*np.pi*2*u)+.05*np.cos(2*np.pi*5*u)
        hum=layer.get('hum',0)
        if hum:
            for h in (1,2,3): y+=.12/h*np.sin(2*np.pi*round(h*hum*dur)*u)
        return y
    else: raise ValueError('Unknown synthesis primitive: '+kind)
    return y*envelope(t,layer.get('attack',.0007),min(.025,dur*.24))


def render(design, variation, danger=False):
    n=design['frames']*2; out=np.zeros(n)
    rng=np.random.default_rng(seed(('toybox-foley-v5',design['id'],variation)))
    for layer in design['layers']:
        lay=dict(layer)
        if 'each' in lay:
            atlist=lay['each']
        elif 'repeat' in lay:
            atlist=np.linspace(lay.get('at',0),lay.get('until',design['frames']/SR*.7),lay['repeat'])
        else: atlist=[lay.get('at',0)]
        for at in atlist:
            if lay.get('variantDelay'): at+=lay['variantDelay'][variation%len(lay['variantDelay'])]
            if lay.get('jitter'): at+=rng.uniform(-lay['jitter'],lay['jitter'])
            at=max(0,round(at*RATE)); x=primitive(lay,rng,variation,danger)*lay.get('gain',1)
            if design['loop']:
                at%=n; first=min(len(x),n-at);out[at:at+first]+=x[:first]
                if first<len(x): out[:len(x)-first]+=x[first:]
            elif at<n: out[at:at+min(len(x),n-at)]+=x[:n-at]
    if design.get('room'):
        dry=out.copy()
        for dt,gain in ((.011,.12),(.023,.08),(.041,.045)):
            k=round(dt*RATE)
            if design['loop']: out+=np.roll(dry,k)*gain*design['room']
            else: out[k:]+=dry[:-k]*gain*design['room']
    if design['loop']:
        # Circular processing, preserve a living noise floor through the seam.
        warm=np.concatenate((out[-8192:],out))
        out=low(warm,11900)[8192:]
    else: out=low(out,11900)
    out=resample_poly(out,1,2,padtype='wrap' if design['loop'] else 'constant')[:design['frames']]
    # Remove subsonic/DC; warm loop filters with the preceding loop end.
    sos=butter(2,35,btype='highpass',fs=SR,output='sos')
    out=sosfilt(sos,np.concatenate((out[-4096:],out)))[4096:] if design['loop'] else sosfilt(sos,out)
    out-=out.mean()
    out=np.tanh(out*1.12)
    out-=out.mean()
    if not design['loop']:
        a=min(round(.0008*SR),len(out)//4);r=min(round(.022*SR),len(out)//4)
        out[:a]*=np.linspace(0,1,a);out[-r:]*=np.linspace(1,0,r)
        # Fading a 30ms mechanical key can reintroduce measurable DC. Correct
        # through a zero-ended window so both click-free endpoints stay exact.
        window=np.sin(np.linspace(0,np.pi,len(out)))**2
        out-=window*(out.sum()/window.sum())
    out*=10**((-7 if design['loop'] else -3)/20)/max(np.max(np.abs(out)),1e-12)
    return out
