"""Offline sampled instruments and original object acoustics for Midnight Toybox Band.

FluidSynth and GeneralUser GS are production tools only; no native library is shipped
with the player. Run setup_toybox.py once. All composition/render seeds are fixed.
"""
import ctypes as ct
import hashlib
import os
from pathlib import Path

import numpy as np
from scipy.signal import butter, sosfilt, sosfiltfilt

SR = 44100
ROOT = Path(__file__).resolve().parents[2]
CACHE = Path(__file__).resolve().parent / '.cache'


def seed(text):
    return int.from_bytes(hashlib.sha256(str(text).encode()).digest()[:8], 'little')


def filt(x, cutoff, kind='lowpass'):
    return sosfilt(butter(2, cutoff, btype=kind, fs=SR, output='sos'), x, axis=0)


def circular_filter(x, cutoff, kind='lowpass'):
    # Warm the IIR with the end of the loop; no cold filter state at the seam.
    k = min(8192, len(x))
    return filt(np.concatenate((x[-k:], x)), cutoff, kind)[k:]


def edges(x, attack=.0008, release=.02):
    y = np.array(x, dtype=np.float64, copy=True)
    a, r = min(len(y), round(attack * SR)), min(len(y), round(release * SR))
    shape = (slice(None),) + (None,) * (y.ndim - 1)
    if a: y[:a] *= np.linspace(0, 1, a)[shape]
    if r: y[-r:] *= np.linspace(1, 0, r)[shape]
    return y


def fit(x, n):
    out = np.zeros((n,) + x.shape[1:], dtype=np.float64)
    out[:min(n, len(x))] = x[:n]
    return out


def add(buf, x, seconds=0, gain=1., wrap=False, pan=0.):
    at = round(seconds * SR)
    if buf.ndim == 2 and x.ndim == 1:
        x = x[:, None] * np.array([np.cos((pan+1)*np.pi/4), np.sin((pan+1)*np.pi/4)])
    if wrap:
        # clips are shorter than a loop, and split placement is faster than add.at
        at %= len(buf)
        first = min(len(x), len(buf)-at)
        buf[at:at+first] += x[:first] * gain
        if first < len(x): buf[:len(x)-first] += x[first:] * gain
    elif at < len(buf):
        count = min(len(x), len(buf)-at)
        buf[at:at+count] += x[:count] * gain


def object_hit(material='wood', dur=.25, variation=0, weight=1.):
    """Impact-excited modal resonators, with separate body and contact texture."""
    rng = np.random.default_rng(seed((material, variation, dur, weight)))
    t = np.arange(round(SR*dur)) / SR
    n = len(t)
    f, ratios, decay, texture = {
        'wood': (510, [1, 1.57, 2.67, 4.11], .045, .18),
        'metal': (1180, [1, 1.487, 2.135, 3.76, 5.21], .12, .12),
        'glass': (1640, [1, 1.365, 2.318, 3.61], .14, .09),
        'paper': (380, [1, 1.91, 3.18], .014, .65),
        'fabric': (165, [1, 1.46, 2.19], .035, .15),
        'wax': (210, [1, 1.63, 2.91], .04, .3),
        'spring': (330, [1, 2.02, 3.14, 4.31], .1, .05),
    }[material]
    f *= 2 ** (rng.uniform(-.65,.65)/12) * weight**-.24
    result = np.zeros(n)
    for k, ratio in enumerate(ratios):
        ff = f*ratio
        if ff > 13000: continue
        phase = 2*np.pi*ff*t
        if material == 'spring': phase += 9*np.sin(2*np.pi*18*t)*np.exp(-t/.1)
        result += np.sin(phase)*np.exp(-t/(decay/(1+k*.52)))/(1+k*.85)
    bodyf = (125 if material not in ('metal','glass') else 230)*weight**-.3
    phase = 2*np.pi*(bodyf*t + 70*.008*(1-np.exp(-t/.008)))
    result += 1.1*np.sin(phase)*np.exp(-t/.034)
    noise = rng.normal(0,1,n)
    tex = filt(filt(noise, 180, 'highpass'), 7500 if material in ('paper','glass') else 3800)
    if material == 'paper': tex *= .5+.5*np.sin(2*np.pi*93*t + 2*np.sin(2*np.pi*17*t))**2
    result += texture*tex*np.exp(-t/(.038 if material=='paper' else .012))
    return edges(result, .0007, min(.025,dur/3))


class Sampler:
    """Sample-exact offline FluidSynth event renderer using the C ABI."""
    def __init__(self):
        dlls = list((CACHE/'fluidsynth').rglob('libfluidsynth-3.dll'))
        if not dlls: raise RuntimeError('Run python Tools/Audio/setup_toybox.py first')
        self.dll_dir = os.add_dll_directory(str(dlls[0].parent))
        self.lib = ct.CDLL(str(dlls[0]))
        def fn(name, result, *args):
            f=getattr(self.lib,name); f.restype=result; f.argtypes=list(args); return f
        ptr, integer, string = ct.c_void_p, ct.c_int, ct.c_char_p
        self.settings_new=fn('new_fluid_settings',ptr)
        self.num=fn('fluid_settings_setnum',integer,ptr,string,ct.c_double)
        self.integer=fn('fluid_settings_setint',integer,ptr,string,integer)
        self.new=fn('new_fluid_synth',ptr,ptr)
        self.load=fn('fluid_synth_sfload',integer,ptr,string,integer)
        self.select=fn('fluid_synth_program_select',integer,ptr,integer,integer,integer,integer)
        self.cc=fn('fluid_synth_cc',integer,ptr,integer,integer,integer)
        self.on=fn('fluid_synth_noteon',integer,ptr,integer,integer,integer)
        self.off=fn('fluid_synth_noteoff',integer,ptr,integer,integer)
        self.write=fn('fluid_synth_write_float',integer,ptr,integer,ptr,integer,integer,ptr,integer,integer)
        self.delete=fn('delete_fluid_synth',None,ptr)
        self.settings_delete=fn('delete_fluid_settings',None,ptr)
        self.settings=self.settings_new()
        for key,value in [(b'synth.sample-rate',SR),(b'synth.gain',.65)]: self.num(self.settings,key,value)
        for key,value in [(b'synth.reverb.active',0),(b'synth.chorus.active',0),(b'synth.polyphony',256),(b'synth.cpu-cores',1)]:
            self.integer(self.settings,key,value)
        self.synth=self.new(self.settings)
        self.sfid=self.load(self.synth,os.fsencode(CACHE/'GeneralUser-GS.sf2'),1)
        if self.sfid<0: raise RuntimeError('Could not load GeneralUser GS')
        self.lib.fluid_synth_set_interp_method.argtypes=[ptr,integer,integer]
        self.lib.fluid_synth_set_interp_method(self.synth,-1,7)
        self.notes={}
        self.used_programs=set()

    def samples(self, n):
        x=np.zeros((n,2),dtype=np.float32)
        if n and self.write(self.synth,n,x.ctypes.data,0,2,x.ctypes.data,1,2)<0:
            raise RuntimeError('FluidSynth render failed')
        return x

    def clear(self):
        for channel in range(16): self.cc(self.synth,channel,120,0)

    def note(self, program, pitch, dur=.18, velocity=90):
        """For short SFX accents; cache dry isolated notes, never mix-normalise notes."""
        self.used_programs.add(program)
        key=(program,pitch,round(dur,3),velocity)
        if key not in self.notes:
            self.clear()
            self.select(self.synth,0,self.sfid,0,program)
            self.cc(self.synth,0,10,64)
            self.on(self.synth,0,pitch,velocity)
            held=self.samples(round(SR*dur))
            self.off(self.synth,0,pitch)
            release=self.samples(round(SR*.8))
            self.notes[key]=edges(np.concatenate((held,release)).mean(1),.0008,.025)
        return self.notes[key]

    def render(self, events, n):
        # Event=(sample, program, pitch, velocity, gate_samples, pan, drum).
        # Repeat an entire cycle to establish release tails from the preceding bar.
        self.clear()
        channels={}
        schedule=[]
        for at,program,pitch,velocity,gate,pan,drum in events:
            key=(program,pan,drum)
            if key not in channels:
                channel=9 if drum else next(c for c in range(16) if c!=9 and c not in channels.values())
                channels[key]=channel
                if self.select(self.synth,channel,self.sfid,128 if drum else 0,program)<0:
                    raise RuntimeError(f'Missing instrument {key}')
                self.cc(self.synth,channel,10,round(64+pan*50))
                self.cc(self.synth,channel,91,0); self.cc(self.synth,channel,93,0)
            channel=channels[key]
            for cycle in (0,1):
                start=at+cycle*n
                schedule.extend([(start,1,channel,pitch,velocity),(start+gate,0,channel,pitch,0)])
        schedule.sort()
        out=np.zeros((2*n,2),np.float32)
        cursor=0
        for at,on,channel,pitch,velocity in schedule:
            if at >= len(out): break
            if at>cursor: out[cursor:at]=self.samples(at-cursor); cursor=at
            if on: self.on(self.synth,channel,pitch,velocity)
            else: self.off(self.synth,channel,pitch)
        if cursor<len(out): out[cursor:]=self.samples(len(out)-cursor)
        return out[n:].astype(np.float64)

    def close(self):
        self.delete(self.synth); self.settings_delete(self.settings); self.dll_dir.close()
