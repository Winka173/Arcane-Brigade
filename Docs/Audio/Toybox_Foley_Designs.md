# Toybox Foley v5 ? danh m?c thi?t k?

206 cue / 582 clip ???c d?ng m?i t? m? h?nh v?t l? v? ngu?n gi?ng kh?ng l?i. Source: `Tools/Audio/SfxDesigns.json`; renderer: `Tools/Audio/foley_engine.py`. Gi? s? sample v? cue routing hi?n c?.

| Cue | Bus | Bi?n th? | Gi?y (bi?n th? ??u) | Thi?t k? |
| --- | --- | ---: | ---: | --- |
| `cmb.swing.blade` | Combat | 6 | 0.200 | Narrow tin edge passing the microphone |
| `cmb.swing.spear` | Combat | 5 | 0.160 | Pointed bamboo thrust, accelerating air |
| `cmb.swing.hammer` | Combat | 4 | 0.250 | Broad mallet head displacing air |
| `cmb.swing.scissors` | Combat | 4 | 0.120 | Two shearing contacts around a tiny air cut |
| `cmb.swing.umbrella` | Combat | 4 | 0.220 | Canvas flap and rib flex |
| `cmb.swing.whip` | Combat | 4 | 0.150 | Silk whistle resolving into a tip crack |
| `cmb.windup` | Combat | 3 | 0.170 | A tightening key ratchets toward the attack |
| `cmb.miss` | Combat | 4 | 0.130 | Unloaded fabric swish |
| `cmb.hit.paper` | Combat | 6 | 0.280 | Close paper impact: contact, body, short material tail |
| `enemy.die.paper` | Combat | 3 | 0.340 | Collapsing paper toy with scattering fragments |
| `cmb.hit.metal` | Combat | 6 | 0.280 | Close metal impact: contact, body, short material tail |
| `enemy.die.metal` | Combat | 3 | 0.340 | Collapsing metal toy with scattering fragments |
| `cmb.hit.wax` | Combat | 6 | 0.280 | Close wax impact: contact, body, short material tail |
| `enemy.die.wax` | Combat | 3 | 0.340 | Collapsing wax toy with scattering fragments |
| `cmb.hit.fabric` | Combat | 6 | 0.280 | Close fabric impact: contact, body, short material tail |
| `enemy.die.fabric` | Combat | 3 | 0.340 | Collapsing fabric toy with scattering fragments |
| `cmb.hit.glass` | Combat | 6 | 0.280 | Close glass impact: contact, body, short material tail |
| `enemy.die.glass` | Combat | 3 | 0.340 | Collapsing glass toy with scattering fragments |
| `cmb.hit.wood` | Combat | 6 | 0.280 | Close wood impact: contact, body, short material tail |
| `enemy.die.wood` | Combat | 3 | 0.340 | Collapsing wood toy with scattering fragments |
| `cmb.crit` | Combat | 5 | 0.454 | A split oak crack, compressed body and single reward pin |
| `wpn.bow.draw` | Combat | 3 | 0.200 | Rosined string tension climbing under a wooden bow |
| `wpn.bow.release` | Combat | 5 | 0.320 | Unwinding string, bamboo click and narrow thwip |
| `wpn.gun.pop` | Combat | 4 | 0.130 | Cork leaves a short wooden barrel |
| `wpn.launcher` | Combat | 3 | 0.320 | Spring latch, hollow tube pressure and spent gear |
| `wpn.cast.fire` | Combat | 4 | 0.450 | Bellows ignite with a warm pressure puff and cinders |
| `wpn.cast.ice` | Combat | 4 | 0.500 | Cold porcelain facets fan outward |
| `wpn.cast.arcane` | Combat | 4 | 0.620 | Bowed glass winds inward, then three asymmetrical tines |
| `wpn.boomerang.fly` | Combat | 3 | 0.600 | Spinning maple propeller with four blade passes |
| `wpn.boomerang.return` | Combat | 3 | 0.500 | Returning wooden propeller caught in a glove |
| `wpn.pierce` | Combat | 4 | 0.240 | A needle passes through taut card |
| `wpn.split` | Combat | 3 | 0.160 | Three cork chambers branch in a fast fan |
| `wpn.bounce` | Combat | 4 | 0.250 | Taut ruler rebounds against a tin rim |
| `sig.bell` | Combat | 4 | 1.400 | Large handbell with beating partials |
| `sig.umbrella` | Combat | 3 | 0.300 | Canvas canopy pops over eight steel ribs |
| `sig.stamp` | Combat | 3 | 0.280 | Rubber seal compresses onto a stack of receipts |
| `sig.scissors` | Combat | 4 | 0.220 | Two deliberate oversized shear cuts |
| `sig.camera` | Combat | 3 | 0.295 | Flash capacitor winds up and mechanical shutter snaps at .2s |
| `sig.musicbox` | Combat | 3 | 0.800 | Three hand-pinned music-box teeth, spacious final note |
| `sig.tea` | Combat | 3 | 0.300 | A ceramic cup empties in a hot splash |
| `sig.fish` | Combat | 3 | 0.180 | Wet felt slap with tiny water droplets |
| `sig.alarm` | Combat | 2 | 0.335 | Twin mechanical alarm bells, alternating mallets |
| `sig.iron` | Combat | 3 | 0.348 | Heavy soleplate presses down and vents steam |
| `sig.hammer` | Combat | 3 | 0.500 | Oversized hardwood mallet, split grain and floor bounce |
| `fx.burn` | Combat | 4 | 0.120 | Tiny crinkled cinder, no tonal tail |
| `fx.chill` | Combat | 4 | 0.220 | One damped frost facet |
| `fx.knockback` | Combat | 4 | 0.220 | Soft pressure slap pushing a cloth body |
| `fx.reaction.steam` | Combat | 2 | 0.500 | Pressure release across a wet hot plate |
| `fx.reaction.shatter` | Combat | 2 | 0.500 | Frozen porcelain bursts into receding shards |
| `fx.reaction.arcane` | Combat | 2 | 0.500 | Reverse glass suction closes on a resonant knot |
| `chr.step` | Foley | 6 | 0.090 | Felt shoe presses into rug, wooden heel contact |
| `chr.dash` | Foley | 4 | 0.280 | Coat hem sweeps out with a short foot push |
| `chr.jump` | Foley | 3 | 0.200 | Toe push and rising spring lift |
| `chr.land` | Foley | 3 | 0.200 | Two cushioned soles and a floorboard |
| `plr.dodge` | Foley | 3 | 0.300 | Air displaced to the side, single small safety tine |
| `plr.hurt` | Combat | 5 | 0.260 | Low chest hit plus two rough alarm reeds; never a reward chime |
| `plr.lowhp` | Combat | 2 | 0.440 | Gentle lub-dub with a low breath warning |
| `plr.heal` | Foley | 3 | 0.800 | Warm ceramic third resolves upward through water |
| `plr.death` | Combat | 2 | 1.290 | Puppet breath drops with a stalled winding mechanism |
| `enemy.spit.windup` | Combat | 4 | 0.300 | Wet throat pressure interrupted by three reed pulses |
| `enemy.spit.fire` | Combat | 5 | 0.220 | Sharp spit pop and recognisable nasal launch |
| `enemy.lunge` | Combat | 4 | 0.300 | Cloth puppet growl surges into a breath swipe |
| `enemy.hurt` | Combat | 6 | 0.184 | Small stuffing grunt, intentionally no ringing metal |
| `enemy.big.step` | Combat | 3 | 0.350 | Padded giant foot with a shelf-rattling aftershock |
| `enemy.big.roar` | Combat | 3 | 0.900 | Large wooden puppet throat, rising then falling |
| `enemy.die.large` | Combat | 3 | 0.600 | Toy chassis breaks, heavy body settles, scattered fittings |
| `boss.enter` | Combat | 2 | 2.950 | Ratchet drum escalation, stage gong lands at one second |
| `boss.telegraph` | Combat | 3 | 0.500 | Three clipped warning blasts, consistent rhythm |
| `boss.attack` | Combat | 3 | 0.600 | Broken stage skin, heavy air pressure and tin debris |
| `boss.phase` | Combat | 2 | 2.200 | Clock mechanism slows, then the scene turns at .95s |
| `boss.defeat` | Combat | 2 | 3.000 | Tin tower collapses before an independent rising brass answer |
| `sig.alarm.boom` | Combat | 3 | 0.500 | Alarm bell is abruptly swallowed by a pressure burst |
| `fx.field.zipper` | Combat | 3 | 0.300 | A row of metal teeth zip taut |
| `fx.hazard.warn` | Combat | 2 | 0.600 | Three escalating warning reeds with a tension rattle |
| `fx.hazard.hit` | Combat | 3 | 0.700 | Falling mechanism crushes a tin and oak platform |
| `enemy.mimic.wake` | Combat | 2 | 0.590 | A hinged chest inhales, then its lid bites shut |
| `hero.pulse.ember` | Combat | 2 | 1.000 | Expanding furnace ring with cinder flecks |
| `hero.pulse.frost` | Combat | 2 | 1.000 | A slow expanding glass ring with falling frost |
| `ui.locked` | UI | 2 | 0.160 | Two dull latch knocks, no reward harmonics |
| `ui.error` | UI | 2 | 0.350 | A double reed rasp behind the blocked latch |
| `loot.coin.drop` | Foley | 4 | 0.250 | One brass disk settles in two contacts |
| `loot.coin.bounce` | Foley | 5 | 0.130 | A short edge bounce with no extra jingle |
| `loot.coin.vacuum` | Foley | 3 | 0.140 | Tiny suction trail ending in a damped tine |
| `loot.coin.pickup` | Foley | 6 | 0.265 | Six ascending pentatonic reward pins, short muted contact |
| `loot.egg` | Foley | 4 | 0.340 | A hollow porcelain bubble with two soft droplets |
| `loot.heart` | Foley | 3 | 0.800 | A warm third over two cushioned heart pulses |
| `loot.chest.appear` | Foley | 2 | 0.800 | A wooden box materialises below a falling glass dust trail |
| `loot.chest.open` | Foley | 3 | 1.200 | Hinge lifts, latch releases, hand-cranked reward teeth |
| `up.merge` | Foley | 3 | 0.800 | Three contacts fuse into one wood-and-porcelain body |
| `up.merge.gather` | Foley | 3 | 0.550 | Three converging air ribbons wind a taut glass knot |
| `up.star2` | Foley | 2 | 1.780 | Two articulated bell pins answered by a small brass chord |
| `up.star3` | Foley | 2 | 1.800 | Three spotlight knocks and a high independent brass flourish |
| `up.ally.land` | Foley | 4 | 1.000 | An ally falls through air and bounces precisely at .405s |
| `up.bond.t1` | Foley | 2 | 1.400 | Two connected tines lead a close major-sixth reed voicing |
| `up.bond.t2` | Foley | 2 | 1.400 | Layered toy brass with an answered upper chord |
| `up.chain` | Foley | 4 | 0.800 | Four linked ceramic dominoes accelerate |
| `up.rarity.blue` | Foley | 2 | 1.000 | An uncommon glass lid and a delicate sixth |
| `up.rarity.purple` | Foley | 2 | 1.400 | A richer bowed jar opens on suspended ceramic tones |
| `up.rarity.gold` | Foley | 2 | 2.000 | Latch tension, brass reveal and a descending golden crown |
| `shop.keeper.enter.gold` | Foley | 2 | 2.580 | A heavy merchant lands at .98s, drawer and coins follow |
| `shop.keeper.enter.dream` | Foley | 2 | 2.600 | A silk portal lands in porcelain at .98s |
| `shop.keeper.exit.gold` | Foley | 2 | 0.800 | Coat tails, closing till and departing loose change |
| `shop.keeper.exit.dream` | Foley | 2 | 0.800 | Silk portal folds inward around a fading jar |
| `shop.carpet.open` | Foley | 2 | 1.100 | Rolled canvas cascades to a weighted flop at .84s |
| `shop.carpet.roll` | Foley | 2 | 0.800 | Canvas gathers around a spindle that clacks at .7s |
| `shop.stand.appear` | Foley | 4 | 0.500 | A hollow counter snaps onto felt feet |
| `shop.item.pop` | Foley | 4 | 0.350 | A small item springs onto its display pad |
| `shop.approach` | UI | 4 | 0.300 | One muted toy key invites attention |
| `shop.focus` | UI | 4 | 0.040 | A felt-damped counter key, four contact positions |
| `shop.buy.gold` | UI | 4 | 0.900 | The till unlocks, brass bell rings and coins land in a tray |
| `shop.buy.dream` | UI | 3 | 1.600 | Glass counter blooms with a breathy nonverbal offering |
| `shop.deny.gold` | UI | 2 | 0.240 | Two blocked latch contacts, tin payment refused |
| `shop.deny.egg` | UI | 2 | 0.200 | Two blocked latch contacts, porcelain payment refused |
| `shop.deny.hp` | UI | 2 | 0.250 | Two blocked latch contacts, felt payment refused |
| `shop.invalid` | UI | 3 | 0.080 | Single blocked felt key |
| `shop.reroll` | UI | 3 | 0.840 | Three stock drums spin out of sync, latch into a tine |
| `shop.fusion` | UI | 2 | 2.000 | Three wooden sockets wind in, reveal a sustained brass chord |
| `shop.descend` | UI | 2 | 1.000 | Three padded stair steps dissolve downward |
| `ui.hover` | UI | 4 | 0.030 | Damped ivory key touch, no pitched tail |
| `ui.select` | UI | 4 | 0.120 | Latch click and one short music-box tooth |
| `ui.confirm` | UI | 3 | 0.315 | Clean rising perfect fifth on ceramic keys |
| `ui.back` | UI | 3 | 0.225 | Two muted descending toy-piano teeth |
| `ui.open` | UI | 2 | 0.270 | A hinged miniature drawer slides and seats |
| `ui.close` | UI | 2 | 0.170 | Drawer retracts and closes on a soft stop |
| `ui.page` | UI | 3 | 0.140 | Receipt stock flicked with a crisp fold |
| `ui.tooltip` | UI | 2 | 0.100 | Small cork bubble rising through a cloth surface |
| `ui.tab` | UI | 3 | 0.060 | One dry typewriter ratchet tooth |
| `ui.pause` | UI | 1 | 0.500 | The miniature tape spindle slows down |
| `ui.resume` | UI | 1 | 0.390 | A spool catch releases and air accelerates |
| `ui.setting` | UI | 3 | 0.030 | A single felted detent on a wooden dial |
| `ui.unlock` | UI | 2 | 1.200 | A key turns, latch opens, three teeth sing a new door |
| `ui.achievement` | UI | 2 | 1.800 | Toy-band medal flourish with an answering high bell |
| `ui.receipt` | UI | 2 | 0.900 | A dot-matrix carriage prints in three bursts then cuts paper |
| `sting.waveclear` | Foley | 2 | 1.900 | Syncopated wooden band answers a four-note brass pickup |
| `sting.victory` | Foley | 1 | 3.600 | Three toy-band chord punches and a high victory crown |
| `sting.defeat` | Foley | 1 | 3.800 | Drooping puppet horn, winding key stalls, final empty tin gong |
| `amb.stage` | Ambience | 1 | 24.000 | Canvas hall: distant wordless crowd, rope and loose lighting rigs |
| `amb.office` | Ambience | 1 | 24.000 | Night desk: quiet fluorescent air, scattered keyboard bursts |
| `amb.lostshop` | Ambience | 1 | 24.000 | Dusty clock shop: two unsynchronised clocks and settling shelves |
| `amb.goldshop` | Ambience | 1 | 24.000 | Warm counter: drawer friction, soft coin sorting, low electric air |
| `amb.dreamshop` | Ambience | 1 | 24.000 | Suspended glass room: drifting filtered air and sparse unpitched facets |
| `amb.toys` | Ambience | 1 | 24.000 | Sleeping toy room: winding keys, miniature footsteps and one loose ruler |
| `amb.transition` | Ambience | 2 | 1.400 | A canvas curtain rises through glass air then settles to felt |
| `voice.gold.syl` | Voice | 8 | 0.097 | gold puppet: eight clean single vowel gestures |
| `voice.gold.laugh` | Voice | 2 | 0.460 | gold puppet: three clipped glottal chuckles |
| `voice.gold.hmm` | Voice | 2 | 0.500 | gold puppet: thoughtful nasal hum with a lifted question |
| `voice.gold.gasp` | Voice | 2 | 0.320 | gold puppet: breath-first open astonishment |
| `voice.gold.farewell` | Voice | 2 | 0.510 | gold puppet: three differently articulated falling gestures |
| `voice.dream.syl` | Voice | 8 | 0.102 | dream puppet: eight clean single vowel gestures |
| `voice.dream.laugh` | Voice | 2 | 0.460 | dream puppet: three clipped glottal chuckles |
| `voice.dream.hmm` | Voice | 2 | 0.500 | dream puppet: thoughtful nasal hum with a lifted question |
| `voice.dream.gasp` | Voice | 2 | 0.320 | dream puppet: breath-first open astonishment |
| `voice.dream.farewell` | Voice | 2 | 0.510 | dream puppet: three differently articulated falling gestures |
| `ui.cutin.boss` | UI | 2 | 1.200 | Two low hostile brass knocks and a held stage gasp |
| `ui.cutin.showstopper` | UI | 2 | 1.400 | Brass victory punctuation and rising crowd breath |
| `ui.cutin.encore` | UI | 2 | 1.200 | A spotlight latch opens on a six-note ceramic flourish |
| `ui.cutin.bond` | UI | 3 | 1.100 | Applause-like paper palms over joined voices |
| `ui.wave.stamp` | UI | 2 | 0.400 | A heavy rubber wave seal with a broad receipt slap |
| `shop.ticket.sold` | UI | 3 | 0.400 | Small SOLD seal and a falling price ticket |
| `shop.ticket.sway` | Foley | 3 | 0.300 | Twine creaks under one soft card flutter |
| `ui.coach.pop` | UI | 3 | 0.225 | A friendly rubber speech bubble opens |
| `menu.spot.hero` | UI | 2 | 0.450 | A cloth puppet calls a tiny nonverbal greeting |
| `menu.spot.door` | UI | 2 | 0.600 | A shop hinge creaks over distant wordless room breath |
| `menu.spot.jar` | UI | 2 | 0.450 | A finger taps a small hollow glass display jar |
| `menu.spot.printer` | UI | 2 | 0.450 | A short carriage scan, feed roller and paper edge |
| `menu.spot.lamp` | UI | 2 | 0.450 | Bulb switch and warm buzzing filament |
| `menu.spot.bell` | UI | 2 | 0.650 | One small counter bell with a damped contact button |
| `wpn.yoyo.throw` | Combat | 3 | 0.400 | Key ratchet releases into a wooden axle whirr |
| `wpn.yoyo.return` | Combat | 3 | 0.400 | String zips inward, then a precise palm catch |
| `wpn.magnet.pull` | Combat | 3 | 0.450 | Energised coil gathers rattling scrap |
| `wpn.needle.volley` | Combat | 4 | 0.275 | Three plucked needle darts with distinct sewing clicks |
| `wpn.spring.slam` | Combat | 3 | 0.400 | Spring compression, then a broad wooden floor impact |
| `wpn.projector.cast` | Combat | 3 | 0.550 | Film sprockets run up beneath a diffuse glass beam |
| `wpn.ribbon.cut` | Combat | 4 | 0.275 | Two silk cuts with offset porcelain plucks |
| `ui.encore.ready` | UI | 2 | 0.800 | A stage switch clacks under a nonverbal crowd swell |
| `hero.lantern.stride` | Combat | 2 | 0.650 | Warm lantern valve ignites, bronze casing rings |
| `hero.parcel.express` | Combat | 2 | 0.550 | Three cardboard parcels leave a metal mail slot |
| `hero.dream.breath` | Combat | 2 | 0.800 | Airy puppet yawn freezes into suspended porcelain |
| `hero.rooted.oracle` | Combat | 2 | 0.800 | Deep wooden roots stretch under a nasal ward hum |
| `hero.solo.maw` | Combat | 3 | 0.550 | A felt jaw clamps before a rotating devouring breath |
| `hero.glass.gambler` | Combat | 2 | 0.550 | Dice bounce twice, glass breaks and one coin escapes |
| `hero.tide.collector` | Combat | 2 | 0.700 | A low water push, droplets and a warm healing third |
| `hero.clock.runner` | Combat | 2 | 0.550 | Three clock teeth stretch apart into reverse air |
| `hero.mirror.guard` | Combat | 2 | 0.550 | Broad mirror flex and three reflected glass darts |
| `hero.treasure.tax` | Foley | 2 | 0.450 | A small coin drawer acknowledges a healing payment |
| `hero.charge.ready` | UI | 2 | 0.300 | A single damped bright bell pin, no celebration tail |
| `event.warn` | Combat | 2 | 1.900 | A circus skin roll accelerates into a clear rising whistle |
| `event.parade.start` | Combat | 2 | 1.000 | Paper boots march to a tiny two-note horn call |
| `event.trap.start` | Combat | 2 | 1.000 | A gilded pedestal ratchets upward and locks |
| `event.eclipse.start` | Combat | 2 | 1.000 | A dark pressure shadow passes a resonating jar |
| `event.reward` | Foley | 3 | 0.700 | Loose coin shower answered by a wordless little ooh |
| `event.end` | Combat | 2 | 0.650 | Canvas air folds into one small closing porcelain tone |
| `boss.voice.choirengine` | Combat | 1 | 1.250 | Three mechanical choir pipes engage an iron chassis |
| `boss.voice.coinwidow` | Combat | 1 | 1.250 | Three descending coin-throat cackles and scuttling disks |
| `boss.voice.lastlantern` | Combat | 1 | 1.250 | A roaring bellows takes a single cracked bronze breath |
| `boss.voice.marqueemaw` | Combat | 1 | 1.250 | Tin signage snaps twice under a buzzing neon bellow |
| `boss.voice.scarletindex` | Combat | 1 | 1.250 | Furious index cards shred through a sharp paper screech |
| `boss.voice.sleepcollector` | Combat | 1 | 1.250 | Deep inhaled yawn followed by two cracked lullaby teeth |
| `boss.voice.suitcaseleviathan` | Combat | 1 | 1.250 | Two brass latches snap around a whale-like leather groan |
| `boss.voice.umbracradle` | Combat | 1 | 1.250 | Rocking cradle joints and an unvoiced shadow breath |
| `boss.voice.undeliveredking` | Combat | 1 | 1.250 | Royal postal horn goes sour before a giant stamp |
| `boss.voice.waxadmiral` | Combat | 1 | 1.250 | A low double foghorn exhales through melting wax |
| `enemy.shot.arc` | Combat | 2 | 0.400 | Hollow mortar thoonk followed by an ascending whistle |
| `enemy.shot.spiral` | Combat | 2 | 0.400 | Three intertwined reed zips rotate away |
| `enemy.shot.burst` | Combat | 2 | 0.400 | Radial air pops over a double tin warning |
| `ui.merge.hint` | UI | 2 | 0.400 | Two quiet upward counter tines leave room for buying |
