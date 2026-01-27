"""Author the complete v5 Foley source catalogue; no generic missing-cue recipe.

Each entry has its own staging and material layers. Related performances share
physical instruments intentionally (e.g. the same scissors), not rendered clips.
Run only to revise/export SfxDesigns.json; render_foley.py reads that JSON directly.
"""
import json
from pathlib import Path
import soundfile as sf
from toybox_engine import ROOT

A=ROOT/'Assets/ArcaneBrigade/Resources/Audio/Arcane'
DESIGNS={}


def L(kind,at=0,dur=.15,gain=1,**params):
    return dict(kind=kind,at=at,dur=dur,gain=gain,**params)


def B(material,at=0,dur=.2,gain=1,**params):
    return L('body',at,dur,gain,material=material,**params)


def W(at=0,dur=.2,gain=.5,start=500,end=3500,**params):
    return L('air',at,dur,gain,**{'from':start,'to':end},**params)


def G(material='paper',at=0,dur=.2,gain=.3,**params):
    return L('grain',at,dur,gain,material=material,**params)


def V(hz=150,at=0,dur=.3,gain=.4,**params):
    return L('voice',at,dur,gain,hz=hz,**params)


def R(hz=80,at=0,dur=.3,gain=.5,**params):
    return L('rumble',at,dur,gain,hz=hz,**params)


def melody(notes,times,material='tine',dur=.34,gain=.4):
    return [B(material,t,dur,gain,notes=[m]) for m,t in zip(notes,times)]


def chord(notes,at=0,dur=.5,gain=.25,kind='reed'):
    return [L(kind,at,dur,gain,notes=[m],**({'brightness':6} if kind=='reed' else {'material':'porcelain'})) for m in notes]


def put(cid,identity,*layers,room=0):
    if cid in DESIGNS: raise ValueError('Duplicate '+cid)
    DESIGNS[cid]=dict(id=cid,identity=identity,layers=list(layers),room=room)


def author():
    DESIGNS.clear()
    # Contact and motion are separate: wood does not acquire a glass ring.
    put('cmb.swing.blade','Narrow tin edge passing the microphone',W(dur=.18,start=1900,end=5800,gain=.62),B('steel',.025,.11,.11,pitch=1.4,decay=.26))
    put('cmb.swing.spear','Pointed bamboo thrust, accelerating air',W(dur=.15,start=700,end=4200,gain=.8,curve=2.2),L('string',.008,.1,.1,hz=330,feedback=.91))
    put('cmb.swing.hammer','Broad mallet head displacing air',W(dur=.24,start=170,end=780,gain=1.2,shape=.85),G('felt',.015,.18,.17))
    put('cmb.swing.scissors','Two shearing contacts around a tiny air cut',B('tin',0,.07,.65,pitch=1.7,decay=.22),B('steel',.042,.065,.4,pitch=1.1,decay=.18),W(.014,.09,.32,1700,6300))
    put('cmb.swing.umbrella','Canvas flap and rib flex',G('felt',dur=.21,gain=.75,rate=37),W(dur=.21,gain=.42,start=260,end=1300),L('spring',.018,.14,.08,hz=154))
    put('cmb.swing.whip','Silk whistle resolving into a tip crack',W(dur=.13,start=800,end=5100,gain=.65,whistle=.08),G('paper',.103,.04,.7,rate=330))
    put('cmb.windup','A tightening key ratchets toward the attack',B('tin',dur=.025,gain=.23,pitch=.75,decay=.12,each=[.006,.057,.094,.121,.143]),W(dur=.165,gain=.5,start=300,end=2400,curve=2.8))
    put('cmb.miss','Unloaded fabric swish',W(dur=.125,gain=.7,start=1200,end=800,shape=2.5))
    mats={'paper':'paper','metal':'tin','wax':'wax','fabric':'felt','glass':'glass','wood':'hollow'}
    for material,body in mats.items():
        extra={
            'paper':[G('paper',.005,.18,.66,rate=113)],
            'metal':[B('steel',.003,.22,.16,pitch=.72,decay=.46)],
            'wax':[L('bubble',.004,.18,.32,count=5,pitch=.58)],
            'fabric':[G('felt',.006,.11,.43,rate=48)],
            'glass':[B('porcelain',.016,.22,.21,pitch=1.32),B('glass',.048,.13,.1,pitch=1.71)],
            'wood':[G('oak',.002,.07,.22,rate=160)],
        }[material]
        put('cmb.hit.'+material,'Close '+material+' impact: contact, body, short material tail',B(body,dur=.26,gain=.58),R(115,dur=.1,gain=.26),*extra)
        # Breakup has staggered debris, not a louder copy of the hit.
        put('enemy.die.'+material,'Collapsing '+material+' toy with scattering fragments',B(body,dur=.14,gain=.5,pitch=.76),
            B(body,dur=.09,gain=.23,pitch=1.65,each=[.065,.108,.171,.226],jitter=.005),
            G('paper' if material=='paper' else 'felt' if material=='fabric' else 'wax' if material=='wax' else 'tin' if material in ('metal','glass') else 'oak',.022,.27,.34,rate=92),
            R(156,dur=.16,gain=.22,to=65))
    put('cmb.crit','A split oak crack, compressed body and single reward pin',B('oak',dur=.14,gain=.85,pitch=.67),B('paper',.003,.08,.35),R(158,dur=.21,gain=.55),B('tine',.012,.35,.31,notes=[91,93,95,96,98]))

    # Every ranged weapon uses a recognisable mechanism, with no universal pew.
    put('wpn.bow.draw','Rosined string tension climbing under a wooden bow',G('oak',dur=.19,gain=.5,rate=153),L('spring',0,.195,.2,hz=130,rate=11,bend=4),W(dur=.19,gain=.18,start=350,end=1800))
    put('wpn.bow.release','Unwinding string, bamboo click and narrow thwip',L('string',0,.30,.65,hz=198,feedback=.985),B('oak',0,.07,.21,pitch=1.7),W(0,.12,.42,900,4800))
    put('wpn.gun.pop','Cork leaves a short wooden barrel',B('hollow',dur=.12,gain=.7,pitch=1.6),R(310,dur=.095,gain=.45,to=110),W(.002,.065,.38,600,2300))
    put('wpn.launcher','Spring latch, hollow tube pressure and spent gear',B('tin',dur=.04,gain=.26,pitch=.6),R(104,.012,.27,.65),B('hollow',.009,.17,.65,pitch=.53),B('tin',.115,.13,.21,pitch=.82))
    put('wpn.cast.fire','Bellows ignite with a warm pressure puff and cinders',W(dur=.39,gain=.9,start=180,end=1650,shape=.55),R(138,dur=.22,gain=.65),G('paper',.03,.36,.34,rate=129))
    put('wpn.cast.ice','Cold porcelain facets fan outward',G('tin',dur=.22,gain=.22,rate=263),*melody([86,93,98],[0,.035,.079],'porcelain',.39,.38))
    put('wpn.cast.arcane','Bowed glass winds inward, then three asymmetrical tines',W(dur=.4,gain=.35,start=4200,end=450,flutter=17),L('spring',0,.31,.24,hz=410,bend=5,rate=71),*melody([79,86,90],[.024,.084,.152],dur=.43,gain=.32),room=.3)
    put('wpn.boomerang.fly','Spinning maple propeller with four blade passes',W(dur=.57,gain=.8,start=1900,end=470,flutter=15,shape=.7),L('string',0,.56,.13,hz=120,feedback=.99))
    put('wpn.boomerang.return','Returning wooden propeller caught in a glove',W(dur=.41,gain=.65,start=500,end=2400,flutter=13,curve=1.9),B('felt',.405,.09,.7),B('oak',.405,.07,.3,pitch=1.3))
    put('wpn.pierce','A needle passes through taut card',W(dur=.23,gain=.65,start=4200,end=1700,whistle=.08),G('paper',.015,.09,.18,rate=330))
    put('wpn.split','Three cork chambers branch in a fast fan',B('hollow',dur=.075,gain=.6,pitch=2,each=[0,.033,.068]),W(.015,.13,.22,1500,4700))
    put('wpn.bounce','Taut ruler rebounds against a tin rim',L('spring',0,.24,.65,hz=330,bend=7,rate=29),B('tin',0,.12,.21,pitch=1.46,decay=.4))
    put('wpn.yoyo.throw','Key ratchet releases into a wooden axle whirr',B('tin',dur=.03,gain=.28,pitch=.65,each=[0,.022,.042,.06]),W(.045,.34,.65,550,2500,flutter=32),L('string',.01,.23,.12,hz=240))
    put('wpn.yoyo.return','String zips inward, then a precise palm catch',W(dur=.32,gain=.8,start=2800,end=700,curve=.55),B('oak',.325,.07,.6,pitch=1.25),B('felt',.325,.07,.38))
    put('wpn.magnet.pull','Energised coil gathers rattling scrap',L('reed',0,.42,.18,hz=98,brightness=2),L('spring',.01,.31,.2,hz=148,rate=52,bend=3),B('tin',dur=.08,gain=.25,each=[.07,.14,.205,.253,.29,.32,.338]),W(dur=.4,gain=.4,start=400,end=1700))
    put('wpn.needle.volley','Three plucked needle darts with distinct sewing clicks',*melody([94,98,101],[0,.056,.112],'tine',.13,.45),B('steel',dur=.05,gain=.18,pitch=1.9,decay=.2,each=[0,.056,.112]),W(0,.24,.22,3200,5100))
    put('wpn.spring.slam','Spring compression, then a broad wooden floor impact',L('spring',0,.17,.45,hz=170,bend=19,rate=14),B('oak',.125,.26,.8,pitch=.6),R(94,.125,.26,.5))
    put('wpn.projector.cast','Film sprockets run up beneath a diffuse glass beam',B('oak',dur=.021,gain=.13,pitch=1.8,repeat=19,until=.42),W(dur=.53,gain=.35,start=750,end=3800,flutter=24,shape=.6),B('glass',.04,.5,.18,pitch=.41,decay=1.8),room=.6)
    put('wpn.ribbon.cut','Two silk cuts with offset porcelain plucks',W(0,.12,.7,1200,4600),W(.127,.14,.6,3500,1000),B('porcelain',.011,.14,.17,pitch=1.4,decay=.38),B('porcelain',.14,.13,.14,pitch=1.72,decay=.38))

    # Champion signatures: recognisable shop objects, down to shutter timing.
    put('sig.bell','Large handbell with beating partials',B('bell',dur=1.39,gain=.7,pitch=.62,decay=1.65),B('oak',dur=.09,gain=.1,pitch=1.9),room=.35)
    put('sig.umbrella','Canvas canopy pops over eight steel ribs',G('felt',dur=.27,gain=.9,rate=46),B('tin',.02,.13,.22,pitch=.64),W(0,.28,.65,320,1800))
    put('sig.stamp','Rubber seal compresses onto a stack of receipts',B('hollow',dur=.22,gain=.8,pitch=.66),B('felt',0,.11,.45),G('paper',0,.24,.55,rate=117))
    put('sig.scissors','Two deliberate oversized shear cuts',B('steel',dur=.09,gain=.38,pitch=.7,decay=.3,each=[0,.105]),G('tin',dur=.08,gain=.7,rate=237,each=[0,.105]))
    put('sig.camera','Flash capacitor winds up and mechanical shutter snaps at .2s',W(0,.197,.17,1500,5100,whistle=.3,shape=.6),B('tin',.2,.065,.65,pitch=1.6,decay=.25),G('paper',.2,.09,.25,rate=440))
    put('sig.musicbox','Three hand-pinned music-box teeth, spacious final note',*melody([76,83,88],[0,.12,.29],dur=.5,gain=.5),B('oak',0,.05,.1,pitch=2.2),room=.3)
    put('sig.tea','A ceramic cup empties in a hot splash',L('bubble',0,.28,.65,count=15,pitch=1.2),W(.035,.25,.38,1700,4200),B('porcelain',0,.14,.2,pitch=.82,decay=.4))
    put('sig.fish','Wet felt slap with tiny water droplets',B('felt',dur=.16,gain=.85,pitch=.8),L('bubble',0,.17,.5,count=7,pitch=.7),G('wax',.003,.13,.35,rate=150))
    put('sig.alarm','Twin mechanical alarm bells, alternating mallets',B('steel',dur=.12,gain=.38,pitch=.72,each=[0,.072,.144,.216]),B('tin',dur=.11,gain=.35,pitch=1.35,each=[.036,.108,.18,.252]))
    put('sig.iron','Heavy soleplate presses down and vents steam',B('tin',dur=.14,gain=.4,pitch=.46),B('hollow',dur=.12,gain=.4,pitch=.55),W(.02,.32,.9,2200,5500,shape=.55))
    put('sig.hammer','Oversized hardwood mallet, split grain and floor bounce',B('oak',dur=.31,gain=.8,pitch=.5,decay=1.7),R(115,dur=.38,gain=.7),B('hollow',.17,.18,.22,pitch=.65),G('oak',.006,.19,.3,rate=187))
    put('sig.alarm.boom','Alarm bell is abruptly swallowed by a pressure burst',B('steel',dur=.055,gain=.33,pitch=.75,each=[0,.04,.08]),R(160,.1,.39,.9),G('paper',.1,.27,.5,rate=270),B('tin',.1,.35,.25,pitch=.4))

    put('fx.burn','Tiny crinkled cinder, no tonal tail',G('paper',dur=.115,gain=.9,rate=217),W(0,.1,.22,500,1600))
    put('fx.chill','One damped frost facet',B('porcelain',dur=.2,gain=.6,pitch=1.95,decay=.28),G('tin',dur=.05,gain=.1,rate=310))
    put('fx.knockback','Soft pressure slap pushing a cloth body',W(dur=.19,gain=.65,start=470,end=1100),B('felt',.014,.17,.6,pitch=.65),R(110,.014,.15,.25))
    put('fx.reaction.steam','Pressure release across a wet hot plate',W(dur=.49,gain=.9,start=4100,end=1600,shape=.55),L('bubble',0,.3,.2,count=13,pitch=.55))
    put('fx.reaction.shatter','Frozen porcelain bursts into receding shards',B('glass',dur=.4,gain=.6,pitch=.62),B('porcelain',dur=.12,gain=.2,pitch=1.9,each=[.035,.074,.122,.189,.278,.337]),G('tin',.008,.3,.3,rate=267))
    put('fx.reaction.arcane','Reverse glass suction closes on a resonant knot',W(dur=.24,gain=.6,start=5000,end=700),B('porcelain',.225,.27,.58,pitch=.58),*melody([94,90,83],[.24,.286,.327],dur=.16,gain=.24))
    put('fx.field.zipper','A row of metal teeth zip taut',B('tin',dur=.025,gain=.22,pitch=1.4,repeat=13,until=.25),W(dur=.28,gain=.48,start=900,end=4200,shape=.6))
    put('fx.hazard.warn','Three escalating warning reeds with a tension rattle',L('reed',dur=.11,gain=.5,hz=1046,each=[0,.17,.34],brightness=3),G('tin',0,.57,.13,rate=38),W(0,.59,.2,600,2500))
    put('fx.hazard.hit','Falling mechanism crushes a tin and oak platform',B('oak',dur=.45,gain=.8,pitch=.41),B('tin',.002,.58,.38,pitch=.45,decay=1.7),R(110,dur=.56,gain=.7),G('paper',.04,.5,.34,rate=135))
    put('hero.pulse.ember','Expanding furnace ring with cinder flecks',R(190,dur=.46,gain=.5,to=58),W(dur=.84,gain=.8,start=350,end=2100,shape=.6),G('paper',.045,.85,.32,rate=181),B('bell',0,.9,.16,pitch=.49))
    put('hero.pulse.frost','A slow expanding glass ring with falling frost',B('glass',dur=.96,gain=.45,pitch=.47,decay=2.4),B('porcelain',dur=.18,gain=.16,pitch=2.1,each=[.07,.19,.32,.51,.68]),W(dur=.9,gain=.27,start=4800,end=1700),room=.5)

    put('chr.step','Felt shoe presses into rug, wooden heel contact',B('felt',dur=.08,gain=.7,pitch=1.5),G('felt',dur=.075,gain=.4,rate=93),B('oak',dur=.045,gain=.08,pitch=1.3))
    put('chr.dash','Coat hem sweeps out with a short foot push',G('felt',dur=.24,gain=.38,rate=38),W(dur=.27,gain=.8,start=300,end=2600),B('felt',dur=.07,gain=.25))
    put('chr.jump','Toe push and rising spring lift',B('felt',dur=.075,gain=.45,pitch=1.7),L('spring',0,.18,.32,hz=210,bend=4,rate=14),W(0,.18,.3,450,1600))
    put('chr.land','Two cushioned soles and a floorboard',B('felt',dur=.17,gain=.75),B('hollow',.008,.16,.34,pitch=.7),G('felt',.007,.11,.25,rate=67))
    put('plr.dodge','Air displaced to the side, single small safety tine',W(dur=.25,gain=.8,start=4000,end=650),B('tine',.07,.22,.18,notes=[93,95,98],decay=.35))
    put('plr.hurt','Low chest hit plus two rough alarm reeds; never a reward chime',R(138,dur=.23,gain=.6,to=55),B('felt',dur=.14,gain=.5),L('reed',dur=.06,gain=.45,hz=740,brightness=3,each=[.008,.096]),G('tin',.005,.12,.28,rate=71))
    put('plr.lowhp','Gentle lub-dub with a low breath warning',R(74,dur=.23,gain=.8,to=46),R(69,.18,.25,.52,to=42,variantDelay=[0,.018]),L('reed',.02,.34,.08,hz=349,brightness=1.5))
    put('plr.heal','Warm ceramic third resolves upward through water',*melody([69,73,78],[0,.13,.31],'porcelain',.44,.3),L('bubble',0,.47,.24,count=7,pitch=.7),room=.4)
    put('plr.death','Puppet breath drops with a stalled winding mechanism',V(110,0,.58,.34,vowel='o',contour=[1.6,1,.4],rasp=True),L('spring',0,.65,.26,hz=125,bend=18,rate=8),R(90,.39,.8,.75,to=27),G('oak',.35,.58,.27,rate=25))

    put('enemy.spit.windup','Wet throat pressure interrupted by three reed pulses',L('bubble',0,.28,.38,count=7,pitch=.5),L('reed',dur=.055,gain=.4,hz=880,brightness=3,each=[0,.095,.19]),W(0,.29,.35,400,2800))
    put('enemy.spit.fire','Sharp spit pop and recognisable nasal launch',L('bubble',0,.2,.5,count=5,pitch=1.1),V(230,0,.13,.25,vowel='u',contour=[1.4,1,.65],formant=1.2),W(0,.2,.4,1400,3900))
    put('enemy.lunge','Cloth puppet growl surges into a breath swipe',V(100,0,.27,.35,vowel='ae',contour=[.7,1.35,.9],rasp=True),W(.025,.26,.65,450,1900))
    put('enemy.hurt','Small stuffing grunt, intentionally no ringing metal',V(180,0,.17,.5,vowel='ouea',contour=[1.15,.9,.6],formant=1.25,rasp=True),G('felt',0,.13,.32,rate=45))
    put('enemy.big.step','Padded giant foot with a shelf-rattling aftershock',B('felt',dur=.28,gain=.8,pitch=.55),R(92,dur=.32,gain=.65,to=37),B('hollow',.013,.25,.45,pitch=.46),B('tin',.09,.14,.12,pitch=.6))
    put('enemy.big.roar','Large wooden puppet throat, rising then falling',V(72,0,.88,.65,vowel='ao',formant=.73,contour=[.7,1.1,1.4,.7],rasp=True,breath=.14),G('oak',.05,.73,.24,rate=32),R(73,0,.65,.2))
    put('enemy.die.large','Toy chassis breaks, heavy body settles, scattered fittings',B('hollow',dur=.43,gain=.8,pitch=.39),R(118,dur=.5,gain=.7),G('oak',0,.43,.45,rate=149),B('tin',dur=.12,gain=.2,pitch=.67,each=[.076,.154,.27,.39,.47]))
    put('enemy.mimic.wake','A hinged chest inhales, then its lid bites shut',G('oak',0,.36,.38,rate=64),L('spring',0,.32,.23,hz=148,bend=8,rate=12),B('oak',.37,.2,.85,pitch=.63),B('tin',.37,.14,.29,pitch=.8),V(96,.05,.25,.15,vowel='o'))
    put('enemy.shot.arc','Hollow mortar thoonk followed by an ascending whistle',B('hollow',dur=.18,gain=.72,pitch=.56),R(200,dur=.2,gain=.28,to=72),W(.015,.36,.48,700,2350,whistle=.2))
    put('enemy.shot.spiral','Three intertwined reed zips rotate away',W(0,.3,.46,950,2850,whistle=.3,flutter=27),W(.045,.3,.3,1300,3300,whistle=.24),W(.09,.3,.23,1600,3700,whistle=.2))
    put('enemy.shot.burst','Radial air pops over a double tin warning',B('tin',dur=.12,gain=.42,pitch=.7,each=[0,.095]),W(0,.19,.7,1000,3800),W(.09,.26,.4,3300,1500),B('hollow',0,.19,.35,pitch=1.3))

    # Boss stage cues have long-form punctuation at the existing landing points.
    put('boss.enter','Ratchet drum escalation, stage gong lands at one second',B('skin',dur=.1,gain=.2,hz=175,each=[0,.2,.37,.52,.65,.76,.85,.92]),W(0,1,.35,500,3400),B('bell',1,1.9,.6,pitch=.21,decay=4),R(115,1,1.2,.7),*chord([38,45,50],1,.7,.2),room=.7)
    put('boss.telegraph','Three clipped warning blasts, consistent rhythm',L('reed',dur=.1,gain=.58,hz=1174,brightness=4,each=[0,.145,.29]),B('tin',dur=.045,gain=.15,pitch=1.05,each=[0,.145,.29]))
    put('boss.attack','Broken stage skin, heavy air pressure and tin debris',B('skin',dur=.49,gain=.65,hz=63,decay=2),R(148,dur=.55,gain=.8),G('tin',.005,.43,.45,rate=98),W(0,.53,.6,450,2500))
    put('boss.phase','Clock mechanism slows, then the scene turns at .95s',B('tin',dur=.05,gain=.18,pitch=.7,each=[0,.07,.16,.28,.44,.64,.87]),W(0,.93,.35,2700,240),B('hollow',.95,.7,.8,pitch=.4),B('bell',.95,1.2,.38,pitch=.28,decay=3),*chord([41,44,48],1.02,.7,.15),room=.5)
    put('boss.defeat','Tin tower collapses before an independent rising brass answer',B('hollow',dur=.7,gain=.7,pitch=.33),R(103,dur=.8,gain=.8),G('tin',0,.8,.3,rate=94),*chord([55,62,67],.75,.35,.2),*chord([60,64,69],1.13,.32,.22),*chord([60,64,67,72],1.57,.8,.24),*melody([91,88,84,79,76],[1.78,1.9,2.02,2.14,2.26],dur=.65,gain=.25),room=.7)

    # Currency's tiny tines are tuned; ordinary impacts are not melodic.
    put('loot.coin.drop','One brass disk settles in two contacts',B('coin',dur=.22,gain=.6),B('coin',.088,.12,.25,pitch=1.08,decay=.5))
    put('loot.coin.bounce','A short edge bounce with no extra jingle',B('coin',dur=.12,gain=.6,pitch=1.24,decay=.48),B('oak',0,.035,.1,pitch=2.2))
    put('loot.coin.vacuum','Tiny suction trail ending in a damped tine',W(0,.13,.65,600,3800,shape=1),B('tine',.065,.07,.19,pitch=1.8,decay=.18))
    put('loot.coin.pickup','Six ascending pentatonic reward pins, short muted contact',B('tine',dur=.25,gain=.5,notes=[84,86,88,91,93,96],decay=.53),B('coin',0,.055,.12,decay=.25))
    put('loot.egg','A hollow porcelain bubble with two soft droplets',L('bubble',0,.31,.7,count=3,pitch=1.6),B('porcelain',.025,.29,.18,notes=[79,83,86,88],decay=.6))
    put('loot.heart','A warm third over two cushioned heart pulses',*melody([69,73],[0,.18],'porcelain',.58,.4),R(100,0,.17,.22,to=75),R(100,.16,.18,.17,to=75),room=.35)
    put('loot.chest.appear','A wooden box materialises below a falling glass dust trail',W(0,.35,.3,4100,700),B('hollow',.18,.43,.72,pitch=.63),*melody([96,91,88],[.02,.09,.16],dur=.38,gain=.23))
    put('loot.chest.open','Hinge lifts, latch releases, hand-cranked reward teeth',G('oak',0,.35,.34,rate=57),B('tin',.22,.13,.27,pitch=.6),*melody([76,83,86,91],[.28,.37,.5,.66],dur=.48,gain=.4),*chord([55,62,67],.32,.5,.13),room=.4)
    put('up.merge.gather','Three converging air ribbons wind a taut glass knot',W(0,.53,.45,400,3900,curve=2),W(.05,.48,.28,800,4600,curve=2),L('string',0,.53,.16,hz=293,feedback=.995),B('tine',dur=.08,gain=.13,pitch=1.7,each=[.04,.15,.24,.31,.37,.42,.46]))
    put('up.merge','Three contacts fuse into one wood-and-porcelain body',B('hollow',0,.26,.6,pitch=.7),B('porcelain',0,.65,.45,notes=[81,83,86],decay=1.6),R(150,0,.34,.45),*melody([88,95,100],[.018,.074,.15],dur=.45,gain=.25),room=.4)
    put('up.star2','Two articulated bell pins answered by a small brass chord',*melody([79,86,91],[0,.12,.31],dur=.65,gain=.38),*chord([55,62,67],.34,.7,.2),B('skin',.34,.3,.25,hz=120),room=.5)
    put('up.star3','Three spotlight knocks and a high independent brass flourish',B('oak',dur=.12,gain=.28,each=[0,.1,.2]),*melody([84,88,91,96],[.03,.14,.25,.43],'porcelain',.8,.3),*chord([60,64,67,74],.43,.85,.25),R(110,.43,.4,.3),room=.65)
    put('up.ally.land','An ally falls through air and bounces precisely at .405s',W(0,.402,.3,2500,500),B('hollow',.405,.3,.85,pitch=.64),B('felt',.405,.2,.4),L('spring',.405,.48,.18,hz=270,bend=9,rate=15),B('oak',.63,.18,.15,pitch=.85))
    put('up.bond.t1','Two connected tines lead a close major-sixth reed voicing',*melody([79,86],[0,.14],dur=.5,gain=.35),*chord([60,64,69],.23,.72,.21),room=.45)
    put('up.bond.t2','Layered toy brass with an answered upper chord',*chord([55,60,64,69],.01,.35,.24),*chord([60,64,67,74],.32,.7,.25),*melody([91,96],[.36,.52],dur=.64,gain=.25),room=.55)
    put('up.chain','Four linked ceramic dominoes accelerate',*melody([81,86,90,93],[0,.11,.2,.27],'porcelain',.4,.38),B('oak',dur=.055,gain=.17,pitch=1.6,each=[0,.11,.2,.27]))
    put('up.rarity.blue','An uncommon glass lid and a delicate sixth',B('glass',dur=.76,gain=.25,pitch=.73),*melody([86,93],[.07,.25],dur=.65,gain=.35),room=.35)
    put('up.rarity.purple','A richer bowed jar opens on suspended ceramic tones',W(0,.28,.27,650,2700),*melody([81,88,95],[.18,.33,.49],'porcelain',.83,.3),*chord([57,64,71],.23,.73,.11),room=.6)
    put('up.rarity.gold','Latch tension, brass reveal and a descending golden crown',W(0,.46,.4,400,4100),B('hollow',.45,.3,.5,pitch=.55),R(118,.45,.4,.4),*chord([60,64,67,74],.45,.75,.25),*melody([100,98,95,91],[.55,.67,.8,.94],dur=.8,gain=.3),room=.7)

    put('shop.keeper.enter.gold','A heavy merchant lands at .98s, drawer and coins follow',W(0,.94,.23,1800,320),B('hollow',.98,.48,.9,pitch=.43),R(115,.98,.35,.42),B('coin',dur=.15,gain=.24,each=[1.01,1.11,1.2,1.36,1.53]),G('oak',1.02,.5,.22,rate=34),room=.25)
    put('shop.keeper.enter.dream','A silk portal lands in porcelain at .98s',W(0,.96,.35,3800,750,shape=.65),B('porcelain',.98,1.5,.65,pitch=.48,decay=2.8),B('felt',.98,.18,.4),*melody([96,93,88],[1.08,1.27,1.48],dur=.88,gain=.18),room=.7)
    put('shop.keeper.exit.gold','Coat tails, closing till and departing loose change',G('felt',0,.62,.4,rate=27),W(0,.65,.55,600,2200),B('coin',dur=.12,gain=.24,each=[.1,.2,.34,.51]),B('oak',.52,.22,.3,pitch=.75))
    put('shop.keeper.exit.dream','Silk portal folds inward around a fading jar',W(0,.75,.5,3300,650),B('glass',0,.77,.2,pitch=.53,decay=1.8),*melody([93,88,81],[0,.16,.32],dur=.43,gain=.18),room=.65)
    put('shop.carpet.open','Rolled canvas cascades to a weighted flop at .84s',G('felt',0,.85,.7,rate=34),W(0,.8,.38,700,220),B('felt',.84,.25,.75,pitch=.6),B('oak',.84,.14,.13,pitch=.65))
    put('shop.carpet.roll','Canvas gathers around a spindle that clacks at .7s',G('felt',0,.69,.7,rate=48),W(0,.68,.35,250,1500),B('oak',.7,.09,.6,pitch=.8))
    put('shop.stand.appear','A hollow counter snaps onto felt feet',B('hollow',dur=.36,gain=.75,pitch=.72),B('felt',.014,.19,.45),B('oak',.093,.14,.16,pitch=1.4))
    put('shop.item.pop','A small item springs onto its display pad',L('spring',0,.27,.45,hz=380,bend=4,rate=32),B('felt',0,.11,.4,pitch=1.6),B('porcelain',.025,.26,.14,pitch=1.2,decay=.5))
    put('shop.approach','One muted toy key invites attention',B('tine',dur=.28,gain=.5,notes=[81,83,86,88],decay=.5),B('oak',0,.04,.1,pitch=1.7))
    put('shop.focus','A felt-damped counter key, four contact positions',B('oak',dur=.038,gain=.6,pitch=2.1,decay=.2),B('tine',0,.037,.16,notes=[88,91,93,95],decay=.08))
    put('shop.buy.gold','The till unlocks, brass bell rings and coins land in a tray',B('oak',dur=.14,gain=.5,pitch=.82),B('hollow',.09,.26,.45,pitch=.62),B('bell',.1,.69,.34,pitch=1.33,decay=.6),B('coin',dur=.12,gain=.17,each=[.14,.21,.32,.45]),*chord([64,71],.12,.24,.14))
    put('shop.buy.dream','Glass counter blooms with a breathy nonverbal offering',B('glass',dur=1.4,gain=.3,pitch=.61,decay=2.8),*melody([88,93,98],[.05,.2,.39],'porcelain',.9,.3),V(310,.12,.9,.22,vowel='a',formant=1.3,breath=.17,contour=[.98,1.04,1]),room=.8)
    for cid,material,hz in [('shop.deny.gold','tin',247),('shop.deny.egg','porcelain',293),('shop.deny.hp','felt',196)]:
        put(cid,'Two blocked latch contacts, '+material+' payment refused',B(material,dur=.075,gain=.45,pitch=.7,decay=.25,each=[0,.095]),L('reed',.008,.17,.3,hz=hz,brightness=2.4))
    put('shop.invalid','Single blocked felt key',B('felt',dur=.075,gain=.75,pitch=1.7),L('spring',0,.07,.2,hz=190,bend=2,rate=50))
    put('shop.reroll','Three stock drums spin out of sync, latch into a tine',B('oak',dur=.035,gain=.2,pitch=1.4,each=[0,.03,.075,.13,.2,.29,.4,.53]),G('paper',0,.6,.27,rate=43),B('tine',.59,.24,.45,notes=[91,93,95]))
    put('shop.fusion','Three wooden sockets wind in, reveal a sustained brass chord',B('oak',dur=.06,gain=.19,pitch=1.4,each=[0,.12,.23,.33,.42,.5]),W(0,.58,.4,300,3300),B('porcelain',.59,1.35,.45,pitch=.57,decay=3),R(130,.59,.3,.35),*chord([57,64,69,73],.6,.85,.22),*melody([88,93,100],[.69,.83,1.03],dur=.7,gain=.26),room=.65)
    put('shop.descend','Three padded stair steps dissolve downward',B('hollow',dur=.19,gain=.45,pitch=.65,each=[0,.23,.47]),B('felt',dur=.12,gain=.45,each=[0,.23,.47]),W(0,.95,.48,2100,260))

    # UI follows intent: tiny mechanical navigation; harmonic affirmation only.
    put('ui.hover','Damped ivory key touch, no pitched tail',B('oak',dur=.029,gain=.55,pitch=2.8,decay=.14),B('felt',0,.025,.22,pitch=2.1))
    put('ui.select','Latch click and one short music-box tooth',B('oak',dur=.06,gain=.37,pitch=1.9),B('tine',0,.115,.35,notes=[84,86,88,91],decay=.25))
    put('ui.confirm','Clean rising perfect fifth on ceramic keys',*melody([81,88],[0,.073],'porcelain',.24,.5),B('oak',0,.05,.18,pitch=2))
    put('ui.back','Two muted descending toy-piano teeth',*melody([83,76],[0,.066],dur=.15,gain=.5),B('felt',0,.05,.2,pitch=1.6))
    put('ui.open','A hinged miniature drawer slides and seats',B('oak',0,.07,.45,pitch=.95),G('oak',0,.22,.4,rate=75),W(0,.25,.33,550,1800))
    put('ui.close','Drawer retracts and closes on a soft stop',G('oak',0,.13,.37,rate=96),W(0,.13,.27,1800,400),B('oak',.112,.055,.55,pitch=.82))
    put('ui.page','Receipt stock flicked with a crisp fold',G('paper',dur=.132,gain=.8,rate=109),B('paper',.008,.06,.3,pitch=1.8))
    put('ui.tooltip','Small cork bubble rising through a cloth surface',L('bubble',0,.095,.5,count=1,pitch=1.6),B('felt',0,.045,.35,pitch=2))
    put('ui.tab','One dry typewriter ratchet tooth',B('tin',0,.056,.4,pitch=1.6,decay=.14),B('oak',0,.05,.36,pitch=2.2))
    put('ui.pause','The miniature tape spindle slows down',L('spring',0,.48,.45,hz=164,rate=7,bend=14),W(0,.46,.38,2100,240),B('oak',.34,.07,.2,pitch=.8))
    put('ui.resume','A spool catch releases and air accelerates',B('oak',0,.05,.35,pitch=1.2),W(0,.37,.6,300,2900),L('string',0,.32,.25,hz=260,feedback=.99))
    put('ui.setting','A single felted detent on a wooden dial',B('oak',dur=.028,gain=.6,pitch=2.7,decay=.11),G('felt',0,.025,.2,rate=213))
    put('ui.locked','Two dull latch knocks, no reward harmonics',B('hollow',dur=.062,gain=.6,pitch=1.3,decay=.4,each=[0,.075]))
    put('ui.error','A double reed rasp behind the blocked latch',L('reed',dur=.11,gain=.4,hz=233,brightness=5,each=[0,.14]),G('tin',dur=.06,gain=.32,rate=77,each=[0,.14]))
    put('ui.unlock','A key turns, latch opens, three teeth sing a new door',B('tin',dur=.06,gain=.2,pitch=.8,each=[0,.055,.11]),B('oak',.16,.13,.4,pitch=.9),*melody([76,83,88],[.17,.28,.43],dur=.65,gain=.4),room=.3)
    put('ui.achievement','Toy-band medal flourish with an answering high bell',*melody([79,83,86,91],[0,.115,.23,.42],dur=.63,gain=.32),*chord([55,62,67,71],.42,.7,.21),B('bell',.74,.9,.19,notes=[98]),B('skin',.42,.23,.25,hz=140),room=.6)
    put('ui.receipt','A dot-matrix carriage prints in three bursts then cuts paper',B('oak',dur=.018,gain=.16,pitch=2.3,each=[0,.025,.05,.075,.1,.125,.21,.235,.26,.285,.31,.335,.42,.445,.47,.495]),G('paper',0,.63,.25,rate=54),B('tin',.65,.07,.28,pitch=1.4),B('tine',.66,.23,.21,notes=[93]))
    put('ui.wave.stamp','A heavy rubber wave seal with a broad receipt slap',B('hollow',dur=.28,gain=.7,pitch=.57),B('felt',0,.14,.45),G('paper',0,.3,.5,rate=151))
    put('shop.ticket.sold','Small SOLD seal and a falling price ticket',B('oak',0,.15,.6,pitch=1.15),B('paper',0,.09,.55),G('paper',.08,.3,.33,rate=61))
    put('shop.ticket.sway','Twine creaks under one soft card flutter',L('string',0,.28,.25,hz=182,feedback=.95),G('paper',.014,.26,.42,rate=34))
    put('ui.coach.pop','A friendly rubber speech bubble opens',L('spring',0,.21,.5,hz=410,bend=5,rate=16),L('bubble',0,.18,.4,count=2,pitch=1.6))
    put('ui.merge.hint','Two quiet upward counter tines leave room for buying',*melody([88,93],[0,.105],dur=.28,gain=.45))
    put('ui.encore.ready','A stage switch clacks under a nonverbal crowd swell',B('oak',0,.13,.7,pitch=.8),V(220,.04,.6,.2,vowel='a',formant=1.2,breath=.35,contour=[.7,1.2,1]),V(160,.07,.6,.18,vowel='o',formant=.9,breath=.35),W(0,.76,.3,400,2200),room=.8)
    put('ui.cutin.boss','Two low hostile brass knocks and a held stage gasp',*chord([50,53,57],0,.25,.22),*chord([49,52,56],.28,.45,.24),B('skin',dur=.2,gain=.4,hz=105,each=[0,.28]),V(150,.1,.55,.2,vowel='o',breath=.3,contour=[1.2,1,.8]),room=.5)
    put('ui.cutin.showstopper','Brass victory punctuation and rising crowd breath',*chord([60,64,67,72],.02,.67,.26),V(230,.01,.85,.22,vowel='a',breath=.4,formant=1.2,contour=[.8,1.2,1]),G('paper',0,.7,.24,rate=33),B('bell',.1,1.2,.15,pitch=1.2),room=.8)
    put('ui.cutin.encore','A spotlight latch opens on a six-note ceramic flourish',B('oak',0,.2,.6,pitch=.66),*melody([79,83,86,91,95,98],[.03,.11,.19,.27,.36,.48],'porcelain',.64,.28),*chord([60,67,76],.48,.6,.15),room=.55)
    put('ui.cutin.bond','Applause-like paper palms over joined voices',G('paper',0,.85,.4,rate=27),*chord([60,64,69],.11,.65,.22),*melody([86,91],[.05,.23],dur=.65,gain=.25),room=.6)

    put('menu.spot.hero','A cloth puppet calls a tiny nonverbal greeting',V(215,0,.23,.55,vowel='ea',contour=[.8,1.3,.9],formant=1.15),L('reed',0,.42,.08,hz=110,brightness=1.5))
    put('menu.spot.door','A shop hinge creaks over distant wordless room breath',G('oak',0,.54,.42,rate=38),L('spring',0,.53,.22,hz=108,bend=5,rate=13),V(145,.07,.48,.12,vowel='o',breath=.5,formant=.85),room=.8)
    put('menu.spot.jar','A finger taps a small hollow glass display jar',B('glass',dur=.43,gain=.6,pitch=.64,decay=1.6),B('porcelain',.018,.37,.17,pitch=.79),room=.25)
    put('menu.spot.printer','A short carriage scan, feed roller and paper edge',B('oak',dur=.019,gain=.2,pitch=2.1,each=[0,.027,.054,.081,.108,.18,.207,.234,.261,.288]),G('paper',0,.42,.35,rate=65),B('tin',.32,.1,.22,pitch=1.3))
    put('menu.spot.lamp','Bulb switch and warm buzzing filament',B('tin',dur=.045,gain=.4,pitch=1.5,decay=.14),L('reed',.02,.41,.25,hz=100,brightness=2),G('felt',.02,.4,.12,rate=100))
    put('menu.spot.bell','One small counter bell with a damped contact button',B('bell',dur=.64,gain=.6,pitch=1.7,decay=.75),B('oak',0,.035,.14,pitch=2))

    # Every hero has its own palette and gesture (not one reward phrase reused).
    put('hero.lantern.stride','Warm lantern valve ignites, bronze casing rings',W(0,.59,.65,250,1600),R(145,0,.27,.5),G('paper',.04,.53,.23,rate=173),B('bell',.02,.61,.27,pitch=.76,decay=.9))
    put('hero.parcel.express','Three cardboard parcels leave a metal mail slot',B('paper',dur=.12,gain=.5,each=[0,.09,.18]),B('oak',dur=.08,gain=.3,pitch=1.6,each=[0,.09,.18]),W(.025,.49,.5,550,2900),B('tin',.01,.12,.2,pitch=.72))
    put('hero.dream.breath','Airy puppet yawn freezes into suspended porcelain',V(320,0,.55,.27,vowel='a',formant=1.3,breath=.28,contour=[.9,1.1,.7]),W(0,.77,.42,3000,900),B('porcelain',.09,.68,.25,pitch=1.4,decay=1.5),room=.65)
    put('hero.rooted.oracle','Deep wooden roots stretch under a nasal ward hum',G('oak',0,.76,.5,rate=32),B('hollow',0,.65,.4,pitch=.4,decay=2.3),V(82,0,.73,.24,vowel='m',formant=.78,contour=[1,1.06,1]),room=.3)
    put('hero.solo.maw','A felt jaw clamps before a rotating devouring breath',B('hollow',0,.17,.55,pitch=.64),B('felt',0,.13,.6),W(.025,.5,.75,1400,320,flutter=23),V(102,0,.26,.2,vowel='o',rasp=True))
    put('hero.glass.gambler','Dice bounce twice, glass breaks and one coin escapes',B('oak',dur=.07,gain=.37,pitch=2,each=[0,.04,.075,.104]),B('glass',.13,.41,.55,pitch=.76),B('porcelain',dur=.1,gain=.17,pitch=1.9,each=[.16,.21,.29]),B('coin',.25,.24,.32))
    put('hero.tide.collector','A low water push, droplets and a warm healing third',L('bubble',0,.65,.65,count=25,pitch=.7),W(0,.61,.48,250,1300),*melody([76,80],[.08,.27],'porcelain',.41,.25))
    put('hero.clock.runner','Three clock teeth stretch apart into reverse air',B('tin',dur=.08,gain=.38,pitch=.8,each=[0,.07,.19]),L('spring',0,.4,.2,hz=210,bend=12,rate=5),W(0,.53,.45,3400,350,curve=.6))
    put('hero.mirror.guard','Broad mirror flex and three reflected glass darts',B('glass',0,.52,.42,pitch=.59,decay=1.6),W(0,.46,.4,3300,1400),*melody([98,91,100],[.025,.096,.18],'porcelain',.28,.27))
    put('hero.treasure.tax','A small coin drawer acknowledges a healing payment',B('hollow',0,.13,.4,pitch=.88),B('coin',dur=.13,gain=.28,each=[.03,.11,.19]),*melody([83,88],[.03,.17],dur=.27,gain=.3))
    put('hero.charge.ready','A single damped bright bell pin, no celebration tail',B('bell',0,.29,.5,notes=[93,96],decay=.26),B('oak',0,.04,.13,pitch=1.9))

    put('event.warn','A circus skin roll accelerates into a clear rising whistle',B('skin',dur=.07,gain=.23,hz=180,each=[0,.22,.42,.6,.76,.9,1.02,1.13,1.23,1.32,1.4,1.47,1.53,1.59,1.65,1.71,1.77,1.83]),W(0,1.88,.45,550,2200,whistle=.2,shape=.6),G('tin',0,1.86,.12,rate=36))
    put('event.parade.start','Paper boots march to a tiny two-note horn call',B('skin',dur=.1,gain=.3,hz=190,each=[0,.12,.24,.36,.48]),G('paper',0,.8,.4,rate=55),L('reed',0,.25,.27,notes=[72]),L('reed',.27,.38,.27,notes=[79]),room=.3)
    put('event.trap.start','A gilded pedestal ratchets upward and locks',G('tin',0,.5,.38,rate=47),B('tin',.43,.5,.5,pitch=.46,decay=1.8),B('coin',dur=.2,gain=.22,each=[.49,.61,.74]),W(0,.48,.32,350,1800))
    put('event.eclipse.start','A dark pressure shadow passes a resonating jar',R(80,0,.9,.5,to=43,decay=.3),W(0,.95,.5,2800,250),B('glass',0,.95,.33,pitch=.34,decay=2.6),V(72,0,.8,.13,vowel='u',breath=.3,formant=.75),room=.7)
    put('event.reward','Loose coin shower answered by a wordless little ooh',B('coin',dur=.14,gain=.27,each=[0,.04,.09,.15,.23,.32,.42]),V(270,.04,.47,.25,vowel='o',formant=1.25,contour=[.8,1.2,1]),B('tine',.07,.6,.2,notes=[91,93,96]),room=.3)
    put('event.end','Canvas air folds into one small closing porcelain tone',W(0,.51,.55,3100,500),B('porcelain',.22,.4,.3,notes=[79,81],decay=.6))

    # Ten separately staged object-creatures: these are nonverbal performances.
    put('boss.voice.choirengine','Three mechanical choir pipes engage an iron chassis',V(65,0,1.17,.3,vowel='o',formant=.75,contour=[.8,1,1],rasp=True),V(98,.03,1.1,.2,vowel='a',formant=.9,contour=[.8,1,1]),*chord([38,45,50,53],.06,1,.18),G('tin',0,.7,.22,rate=33),room=.6)
    put('boss.voice.coinwidow','Three descending coin-throat cackles and scuttling disks',V(210,dur=.22,gain=.37,vowel='a',formant=1.08,rasp=True,contour=[1.35,.8],each=[0,.29,.61]),B('coin',dur=.15,gain=.23,each=[.02,.08,.17,.3,.41,.52,.66,.79,.94,1.06]),room=.35)
    put('boss.voice.lastlantern','A roaring bellows takes a single cracked bronze breath',W(0,1.21,.75,250,2100,shape=.6),V(85,0,1.13,.3,vowel='ao',formant=.8,rasp=True,breath=.2,contour=[.6,1.2,.8]),B('bell',.07,1.17,.3,pitch=.38,decay=2.5),G('paper',.03,1.16,.22,rate=179))
    put('boss.voice.marqueemaw','Tin signage snaps twice under a buzzing neon bellow',B('tin',dur=.35,gain=.55,pitch=.32,each=[0,.38]),B('hollow',dur=.27,gain=.65,pitch=.37,each=[0,.38]),L('reed',0,1.18,.17,hz=60,brightness=3),V(76,.08,.98,.4,vowel='ae',formant=.7,rasp=True,contour=[.8,1.1,.6]))
    put('boss.voice.scarletindex','Furious index cards shred through a sharp paper screech',G('paper',0,1.18,.85,rate=117),V(240,.02,.91,.25,vowel='ie',formant=1.4,breath=.25,contour=[.7,1.6,.8]),B('oak',dur=.06,gain=.18,pitch=1.8,repeat=17,until=.94))
    put('boss.voice.sleepcollector','Deep inhaled yawn followed by two cracked lullaby teeth',V(66,0,.98,.55,vowel='ao',formant=.7,breath=.24,contour=[.65,1.15,1.3,.55]),W(0,.94,.27,1600,250),*melody([79,75],[.57,.81],dur=.4,gain=.18),room=.7)
    put('boss.voice.suitcaseleviathan','Two brass latches snap around a whale-like leather groan',B('tin',dur=.2,gain=.36,pitch=.48,each=[0,.16]),B('hollow',.17,.48,.7,pitch=.26),G('felt',0,.98,.25,rate=17),V(49,.04,1.17,.55,vowel='ou',formant=.55,contour=[1.4,1,.7,1.15],rasp=True),room=.5)
    put('boss.voice.umbracradle','Rocking cradle joints and an unvoiced shadow breath',G('oak',dur=.31,gain=.45,rate=29,each=[0,.4,.79]),L('spring',0,1.15,.22,hz=84,bend=6,rate=4),W(0,1.22,.6,3200,500,shape=.7),V(58,0,1.2,.15,vowel='u',formant=.6,breath=.4),room=.9)
    put('boss.voice.undeliveredking','Royal postal horn goes sour before a giant stamp',L('reed',0,.2,.35,notes=[55]),L('reed',.21,.2,.35,notes=[62]),L('reed',.42,.35,.4,notes=[58]),B('hollow',.78,.45,.8,pitch=.38),G('paper',.78,.4,.5,rate=121),R(100,.78,.4,.45),room=.45)
    put('boss.voice.waxadmiral','A low double foghorn exhales through melting wax',L('reed',0,.53,.43,hz=55,brightness=3),L('reed',.55,.65,.37,hz=52,brightness=3),V(55,0,1.2,.23,vowel='o',formant=.65,rasp=True),W(0,1.23,.45,1300,4100,shape=.55),L('bubble',0,1.18,.24,count=17,pitch=.45),room=.5)

    # Short jingles each have their own cadence; no transplanted music-box hook.
    put('sting.waveclear','Syncopated wooden band answers a four-note brass pickup',B('skin',dur=.17,gain=.3,hz=120,each=[0,.24,.36,.72]),*melody([79,83,86,88],[0,.13,.32,.48],dur=.6,gain=.35),*chord([55,62,67],.48,.65,.21),L('string',0,.4,.24,hz=98),room=.5)
    put('sting.victory','Three toy-band chord punches and a high victory crown',B('skin',dur=.14,gain=.3,hz=145,each=[0,.1,.2,.3,.4,.5]),*chord([60,64,67],.48,.4,.22),*chord([65,69,72],.96,.4,.23),*chord([67,71,74],1.44,.4,.23),*chord([60,64,67,72],1.98,1.2,.25),*melody([91,93,95,100],[1.5,1.65,1.8,1.98],dur=1.1,gain=.25),B('bell',1.98,1.6,.15,pitch=1.3,decay=2),room=.75)
    put('sting.defeat','Drooping puppet horn, winding key stalls, final empty tin gong',V(150,0,.7,.27,vowel='o',contour=[1,.7,.45],formant=.85),L('reed',0,.55,.3,notes=[67]),L('reed',.59,.55,.32,notes=[64]),L('reed',1.18,.68,.34,notes=[60]),L('spring',.5,1.35,.18,hz=120,bend=15,rate=5),B('bell',1.87,1.88,.3,pitch=.25,decay=4),R(83,1.87,.8,.3),room=.65)

    # Articulated glottal/formant voices, not recycled syllables or real speech.
    for who,hz,formant,breath in [('gold',142,.92,.065),('dream',335,1.29,.22)]:
        put('voice.'+who+'.syl',who+' puppet: eight clean single vowel gestures',V(hz,0,.1,.6,vowel='aoeuieom',formant=formant,breath=breath,rasp=who=='gold',contour=[.9,1.1,.95]))
        put('voice.'+who+'.laugh',who+' puppet: three clipped glottal chuckles',V(hz*1.05,dur=.115,gain=.55,vowel='a',formant=formant,breath=breath,rasp=who=='gold',contour=[1.15,.83],each=[0,.145,.285]))
        put('voice.'+who+'.hmm',who+' puppet: thoughtful nasal hum with a lifted question',V(hz,0,.49,.65,vowel='m',formant=formant,breath=breath*.3,contour=[.92,.89,1.16]))
        put('voice.'+who+'.gasp',who+' puppet: breath-first open astonishment',W(0,.14,.3,1800,3500),V(hz*1.18,.035,.27,.5,vowel='o',formant=formant,breath=breath*1.3,contour=[.75,1.2,.95]))
        put('voice.'+who+'.farewell',who+' puppet: three differently articulated falling gestures',V(hz*1.12,0,.135,.5,vowel='e',formant=formant,breath=breath),V(hz,.165,.135,.5,vowel='a',formant=formant,breath=breath),V(hz*.81,.33,.175,.45,vowel='u',formant=formant,breath=breath))

    # Separate 24s living rooms: periodic beds, no repeating musical phrase.
    put('amb.stage','Canvas hall: distant wordless crowd, rope and loose lighting rigs',L('bed',0,24,.13,cutoff=1800),G('felt',dur=.7,gain=.1,rate=17,each=[1.3,8.6,17.4,23.8]),V(140,dur=1.4,gain=.05,vowel='oa',breath=.5,formant=.85,each=[3.2,11.5,19.3]),B('tin',dur=.4,gain=.022,pitch=.6,each=[6.8,16.4,22.7]),room=.8)
    put('amb.office','Night desk: quiet fluorescent air, scattered keyboard bursts',L('bed',0,24,.12,cutoff=2500,hum=50),B('oak',dur=.028,gain=.025,pitch=2,each=[2.1,2.24,2.35,2.54,7.1,7.23,7.39,14.3,14.47,14.64,14.8,21.4,21.56,21.7]),G('paper',dur=.55,gain=.045,rate=41,each=[4.6,16.8]),room=.35)
    put('amb.lostshop','Dusty clock shop: two unsynchronised clocks and settling shelves',L('bed',0,24,.11,cutoff=1250),B('oak',dur=.032,gain=.025,pitch=1.8,repeat=48,until=23.5),B('tin',dur=.035,gain=.012,pitch=.85,each=[.37+1.09*k for k in range(22)]),G('oak',dur=.65,gain=.065,rate=21,each=[4.1,12.7,21.8]),B('tine',9.4,.8,.018,hz=1630,decay=2),room=.7)
    put('amb.goldshop','Warm counter: drawer friction, soft coin sorting, low electric air',L('bed',0,24,.12,cutoff=1400,hum=50),B('coin',dur=.14,gain=.037,each=[2.3,2.52,6.7,7.02,13.5,13.74,19.6,19.95]),G('oak',dur=.63,gain=.06,rate=43,each=[5.4,16.9]),B('bell',11.3,.8,.02,pitch=1.4,decay=1.1),room=.45)
    put('amb.dreamshop','Suspended glass room: drifting filtered air and sparse unpitched facets',L('bed',0,24,.12,cutoff=3400),W(dur=5.2,gain=.1,start=1700,end=4200,each=[1.2,9.8,20.7],jitter=.12),B('glass',dur=1.5,gain=.025,hz=1373,decay=3,each=[3.3,14.4,23.5]),B('porcelain',dur=1.3,gain=.018,hz=2197,decay=3,each=[7.1,18.6]),room=1)
    put('amb.toys','Sleeping toy room: winding keys, miniature footsteps and one loose ruler',L('bed',0,24,.11,cutoff=1700),B('tin',dur=.027,gain=.035,pitch=.8,each=[2.2,2.25,2.3,2.35,9.1,9.15,9.2,9.25,18.3,18.35,18.4]),B('felt',dur=.075,gain=.047,pitch=1.6,each=[5.7,5.95,6.2,13.5,13.75,14,21.3,21.55]),L('spring',16.2,.55,.027,hz=155,bend=12,rate=13),room=.55)
    put('amb.transition','A canvas curtain rises through glass air then settles to felt',W(0,.7,.5,300,3600),W(.57,.8,.4,3400,300),G('felt',.65,.7,.25,rate=24),B('glass',.34,.95,.15,pitch=.7,decay=2),room=.5)

    cues=json.loads((A/'cues.json').read_text())['cues']
    expected={c['id'] for c in cues}
    if set(DESIGNS)!=expected: raise ValueError({'missing':sorted(expected-set(DESIGNS)),'extra':sorted(set(DESIGNS)-expected)})
    rows=[]
    for c in cues:
        d=DESIGNS[c['id']]
        frames=[sf.info(str(A/(p+'.wav'))).frames for p in c['clips']]
        d.update(frames=frames[0],variationFrames=frames,loop=c['loop'],clips=c['clips'],danger=c['danger'])
        # Syllable lengths are individual; scale the whole articulated gesture.
        if c['id'].startswith('voice.') and c['id'].endswith('.syl'):
            d['layers'][0]['dur']=frames[0]/44100
        rows.append(d)
    out=Path(__file__).with_name('SfxDesigns.json')
    out.write_text(json.dumps({'revision':5,'renderRate':88200,'outputRate':44100,'designs':rows},indent=1)+'\n',encoding='utf-8')
    print('Authored',len(rows),'cues /',sum(len(d['clips']) for d in rows),'performances')


if __name__=='__main__': author()
