# Real combat sounds (4/11/2026)

The synthesized combat sounds were replaced by real CC0 / public-domain recordings: sword, spear and hammer swings; metal hits and criticals; bow draw, release and arrow hits; fire, ice and arcane casts; cork-gun pops and launchers; creature hurt, lunge and deaths by material; big monster roars and steps; boss entrance, phase, attack and defeat (real growls pitched down, with a rumble under the entrance); the hero's hurt, dodge and heal. Each file was cut to its loud part, faded and matched to the loudness of the sound it replaced, so the mix balance is unchanged. Per-boss signature sets and the toy weapons (scissors, umbrella, whip) keep their earlier sounds.

Built by the processing script (cue group -> source list); the replaced originals are kept outside the repository. Every source below is CC0 or public domain, verified on the author's page or in the licence file inside the archive.

## Hero signature weapon sounds (4/11/2026)

Each of the 36 heroes' signature weapons attacks with a sound of its own (cue `hero.wpn.<weapon id in lower case>`, files `Sfx/s7_hero_wpn_<weapon>_0..2.wav`) instead of a pitched copy of a shared swing, cast or shot. Each sound layers short cuts of two to four of the recordings below (some used twice, as two notes or two knocks) so it says what the object is and how it moves; the three files of a weapon use different takes or small pitch and timing changes. All are mono 44.1 kHz 16-bit, start within 1 ms (2 ms fade-in), stay under 0.35 s (melee and shots) or 0.6 s (casts, launchers, music), end on a fade, peak at or below -1 dBFS and sit within 0.25 dB of the loudness (loudest 50 ms RMS) of the cue they replace; their rows in `loudness.json` carry the table gain of the clips they replace, so the mix balance is unchanged. A friend wielding the weapon plays the same cue. Measurements: `C:/ABClones/hero_wpn_report.txt` (outside the repository).

| Weapon (hero) | Replaces | Layers | Sources (pack: file) |
|---|---|---|---|
| KeyCleaver (LanternKnight) | `cmb.swing.hammer` | heavy blade whoosh + key-ring jingle + low metal clank of the key bit | Freesound 420670: `420670_Strong_Melee_Swing_by_SypherZent.ogg`; OGA 100-cc0-metal-and-wood-sfx: `keys_01.ogg`, `keys_02.ogg`, `keys_03.ogg`; Kenney impact-sounds: `impactMetal_medium_000.ogg`, `impactMetal_medium_002.ogg`, `impactMetal_medium_004.ogg` |
| MailboxLauncher (Postmaster) | `wpn.gun.pop` | mailbox flap clack + launcher thump + letter paper flutter | Kenney rpg-audio: `metalLatch.ogg`, `bookFlip3.ogg`, `bookFlip2.ogg`; OGA 25-cc0-bang-firework-sfx: `shot_01.ogg`; OGA 100-cc0-metal-and-wood-sfx: `metal_hit_01.ogg`; OGA 80-cc0-rpg-sfx: `lock_02.ogg`; OGA 100-cc0-sfx: `paper_01.ogg` |
| MoonseedOrb (DrowsySeer) | `wpn.cast.arcane` | soft seed pop + dreamy shimmer + low airy whoosh | OGA 100-cc0-sfx: `plop_01.ogg`, `plop_02.ogg`; OGA cure-magic: `Cure2.wav`, `Cure3.wav`, `Cure8.wav`; OGA fantasy-sound-effects-tinysized-sfx: `tube-plastic-whoosh-01.wav`, `tube-plastic-whoosh-02.wav`; OGA 3-pop-sounds: `pop3.ogg` |
| BambooVase (RootOracle) | `wpn.launcher` | hollow bamboo tok + bamboo whoosh + ceramic vase clink | Kenney impact-sounds: `impactWood_light_000.ogg`, `impactWood_light_002.ogg`, `impactWood_light_004.ogg`; Freesound 706676: `706676_Whoosh_Bamboo_1_by_Sadiquecat.ogg`; OGA 100-cc0-sfx: `dishes_02.ogg`, `dishes_04.ogg`, `dishes_01.ogg` |
| InkyOctopus (LoneJelly) | `cmb.hit.fabric` | wet ink squirt + tentacle swish + low ink plop | OGA 80-cc0-creture-sfx-2: `slime_02.ogg`, `slime_07.ogg`; OGA swishes-sound-pack: `swish-3.wav`, `swish-5.wav`, `swish-8.wav`; OGA 100-cc0-sfx: `plop_02.ogg`, `plop_01.ogg`; OGA rpg-sound-pack: `NPC/slime/slime6.wav`; OGA fantasy-sound-effects-tinysized-sfx: `water-drop-03.wav` |
| GlitchToybox (GlassDice) | `wpn.projector.cast` | toy-box spring boing with a stuttered glitch start + retro chirp + dice rattle | OGA 100-cc0-sfx: `spring_04.ogg`, `spring_06.ogg`, `spring_09.ogg`; Kenney sci-fi-sounds: `laserRetro_004.ogg`, `laserRetro_001.ogg`, `laserRetro_000.ogg`; OGA fantasy-accessory-sfx-library: `dice-many-shake-01.wav`, `dice-two-roll-01.wav`, `die-one-roll-01.wav` |
| RibbonHarp (TideSinger) | `wpn.cast.arcane` | three plucked tine notes (rising/falling chord) + ribbon cloth swish | OGA rpg-sound-pack: `inventory/metal-small2.wav`; Kenney rpg-audio: `cloth1.ogg`, `cloth2.ogg`; OGA 80-cc0-rpg-sfx: `item_gem_02.ogg`; Kenney impact-sounds: `impactGlass_light_001.ogg`; OGA fantasy-sound-effects-tinysized-sfx: `tube-plastic-whoosh-02.wav` |
| ClockCrescent (CometRunner) | `wpn.boomerang.fly` | spinning crescent whoosh + clock tick-tock + thin blade ring | Freesound 420670: `420670_Strong_Melee_Swing_by_SypherZent.ogg`; OGA 100-cc0-sfx: `switch_01.ogg`, `switch_02.ogg`; OGA 100-cc0-sfx-2: `sfx100v2_switch_01.ogg`, `sfx100v2_switch_02.ogg`; OGA 80-cc0-rpg-sfx: `blade_01.ogg`, `blade_02.ogg`, `blade_03.ogg`; Freesound 420668: `420668_Basic_Melee_Swing_Miss_Whoosh_by_SypherZent.ogg`; OGA rpg-sound-pack: `battle/swing.wav` |
| SnowGlobeOrb (MirrorKeeper) | `wpn.cast.ice` | glass globe chime + icy shimmer + soft snow slosh | OGA 100-cc0-sfx-2: `sfx100v2_glass_04.ogg`; OGA ice-spells: `ice.wav`, `coldsnap.wav`; Kenney impact-sounds: `footstep_snow_000.ogg`, `footstep_snow_002.ogg`, `footstep_snow_004.ogg`; OGA 100-cc0-sfx: `glass_01.ogg`; OGA 80-cc0-rpg-sfx: `item_gem_02.ogg` |
| AcornSling (TreasureAcorn) | `wpn.bow.release` | sling band snap + acorn wood knock + nut rattle | Freesound 536068: `536068_Bow_Release_Hit_by_EminYILDIRIM.ogg`; Kenney impact-sounds: `impactWood_light_001.ogg`, `impactWood_light_003.ogg`, `impactWood_light_004.ogg`; OGA fantasy-accessory-sfx-library: `container-nuts-01.wav` |
| LeafCrossbow (VerdantMatron) | `wpn.bow.release` | crossbow clack + string thwip + leaf rustle | Freesound 384919: `384919_Crossbow_Firing_and_Hitting_Target_by_Ali_6868.ogg`; Freesound 263675: `263675_Bow_release.wav_by_PorkMuncher.ogg`; Kenney impact-sounds: `footstep_grass_000.ogg`, `footstep_grass_001.ogg`, `footstep_grass_002.ogg` |
| FireExtinguisher (CopperCauldron) | `wpn.launcher` | pressurised spray burst (high hiss filtered off) + canister clank + valve click | OGA fantasy-sound-effects-tinysized-sfx: `compressed-air-spray-01.wav`, `compressed-air-spray-02.wav`; Kenney impact-sounds: `impactTin_medium_000.ogg`, `impactTin_medium_002.ogg`, `impactTin_medium_004.ogg`; OGA 100-cc0-sfx-2: `sfx100v2_switch_02.ogg`; OGA 100-cc0-sfx: `switch_02.ogg`; Kenney rpg-audio: `metalClick.ogg` |
| NeedleSinger (ChinaMarionette) | `wpn.needle.volley` | thin needle zip + porcelain tink + short singing glass ring | Freesound 855248: `855248_Real_Bullet_Flyby_Sound_by_qubodup.ogg`; Kenney impact-sounds: `impactGlass_light_000.ogg`, `impactGlass_light_002.ogg`, `impactGlass_light_004.ogg`; OGA 100-cc0-sfx-2: `sfx100v2_glass_04.ogg`, `sfx100v2_glass_06.ogg`; OGA 100-cc0-sfx: `glass_01.ogg` |
| BubbleWand (SoapSkater) | `wpn.cast.arcane` | bubble pops + wand blow + small drop | OGA rpg-sound-pack: `inventory/bubble.wav`, `inventory/bubble2.wav`, `inventory/bubble3.wav`; OGA fantasy-sound-effects-tinysized-sfx: `tube-plastic-whoosh-02.wav`, `water-drop-01.wav`, `tube-plastic-whoosh-01.wav`, `water-drop-02.wav`; OGA 100-cc0-sfx: `plop_01.ogg` |
| StarforgeClaymore (VelvetRegent) | `cmb.swing.hammer` | big low blade whoosh + forge hammer-on-anvil ring + star spark | Freesound 420670: `420670_Strong_Melee_Swing_by_SypherZent.ogg`; OGA 100-cc0-metal-and-wood-sfx: `hammer_02.ogg`, `hammer_03.ogg`, `hammer_04.ogg`; OGA 80-cc0-rpg-sfx: `item_gem_01.ogg`, `item_gem_03.ogg` |
| CorkscrewRapier (ButtonCavalier) | `cmb.swing.spear` | thin thrust swish + corkscrew twist squeak + cork pop | OGA swishes-sound-pack: `swish-9.wav`, `swish-7.wav`, `swish-4.wav`; OGA fantasy-sound-effects-tinysized-sfx: `bottle-glass-cork-twist-01.wav`; OGA fantasy-accessory-sfx-library: `bottle-glass-uncork-01.wav`, `vial-glass-round-cork-twist-05.wav`, `bottle-glass-uncork-02.wav`, `vial-glass-round-cork-twist-04.wav`, `bottle-glass-uncork-04.wav` |
| DrumstickClub (NightMaw) | `cmb.swing.hammer` | wooden stick whoosh + drum thump + stick click | OGA rpg-sound-pack: `battle/swing.wav`, `battle/swing2.wav`; Kenney impact-sounds: `impactSoft_heavy_000.ogg`, `impactWood_light_000.ogg`, `impactSoft_heavy_002.ogg`, `impactWood_light_002.ogg`, `impactSoft_heavy_004.ogg`, `impactWood_light_003.ogg`; OGA swishes-sound-pack: `swish-7.wav` |
| GumballGatling (ThornTreasurer) | `wpn.gun.pop` | crank click + rubbery gumball pop + gumball rattle | OGA 100-cc0-sfx-2: `sfx100v2_switch_01.ogg`; OGA 3-pop-sounds: `pop1.ogg`, `pop3.ogg`; OGA fantasy-accessory-sfx-library: `dice-two-roll-01.wav`, `die-one-roll-01.wav`, `container-nuts-01.wav`; OGA 100-cc0-sfx: `switch_01.ogg`, `plop_01.ogg`; OGA 80-cc0-rpg-sfx: `lock_02.ogg` |
| SignpostSlammer (CitrusMarshal) | `cmb.swing.hammer` | heavy board whoosh + wooden plank thwack + metal post rattle | OGA swishes-sound-pack: `swish-7.wav`, `swish-9.wav`; Kenney impact-sounds: `impactPlank_medium_000.ogg`, `impactPlank_medium_001.ogg`, `impactPlank_medium_003.ogg`; OGA 100-cc0-metal-and-wood-sfx: `metal_hit_03.ogg`; OGA 75-cc0-breaking-falling-hit-sfx: `bfh1_metal_hit_02.ogg`, `bfh1_metal_hit_05.ogg`; Freesound 420670: `420670_Strong_Melee_Swing_by_SypherZent.ogg` |
| DreamStamp (IvorySovereign) | `sig.stamp` | rubber stamp slap on paper + soft low thunk + faint dream sparkle | Kenney rpg-audio: `bookPlace1.ogg`, `bookPlace2.ogg`, `bookPlace3.ogg`; Kenney impact-sounds: `impactSoft_medium_001.ogg`, `impactSoft_medium_002.ogg`, `impactSoft_medium_003.ogg`; OGA 80-cc0-rpg-sfx: `item_gem_03.ogg` |
| RoseBouquet (PetalCitadel) | `cmb.swing.whip` | bouquet swing whoosh + wrapping-paper rustle + petal flutter | Freesound 420670: `420670_Strong_Melee_Swing_by_SypherZent.ogg`; OGA 100-cc0-sfx: `paper_01.ogg`, `paper_02.ogg`, `paper_04.ogg`; Kenney rpg-audio: `bookFlip3.ogg`, `bookFlip2.ogg`; Freesound 420668: `420668_Basic_Melee_Swing_Miss_Whoosh_by_SypherZent.ogg`; OGA swishes-sound-pack: `swish-6.wav`; OGA fantasy-weapons-and-apparel-sfx-library: `arrow-feathers-03.wav` |
| ConfettiBlunderbuss (PatchworkHost) | `wpn.gun.pop` | party-popper bang + wide low boom + confetti paper flutter | OGA 25-cc0-bang-firework-sfx: `shot_02.ogg`, `shot_03.ogg`; Kenney impact-sounds: `impactPunch_heavy_000.ogg`, `impactPunch_heavy_003.ogg`, `impactPunch_heavy_004.ogg`; OGA 100-cc0-sfx: `paper_04.ogg`, `shot_02.ogg`; Kenney rpg-audio: `bookFlip2.ogg`; OGA various-paper-sound-effects: `Paper Sound - 3.wav` |
| SleepCamera (ArchiveCurator) | `sig.camera` | shutter click-clack + film wind + sleepy flash chime | OGA 100-cc0-sfx-2: `sfx100v2_switch_01.ogg`, `sfx100v2_switch_02.ogg`; OGA fantasy-accessory-sfx-library: `keyhole-lockbox-turn-01.wav`, `keyhole-lockbox-turn-03.wav`, `keyhole-lockbox-turn-05.wav`; OGA cure-magic: `Cure4.wav`, `Cure2.wav`, `Cure8.wav`; OGA 100-cc0-sfx: `switch_01.ogg`, `switch_02.ogg`; Kenney rpg-audio: `metalLatch.ogg` |
| DetonatorBox (PennyBroker) | `wpn.cast.fire` | plunger push clack + fuse fizz + small dull pop | OGA 80-cc0-rpg-sfx: `lock_03.ogg`, `lock_01.ogg`; OGA fantasy-sound-effects-tinysized-sfx: `lighter-light-01.wav`, `match-light-01.wav`; OGA dull-explosion: `explosion_dull.flac`; Kenney rpg-audio: `metalLatch.ogg`; OGA flare-ignition: `ignition.flac` |
| PlasmaTrident (StormFencer) | `wpn.launcher` | trident thrust whoosh + electric discharge crackle + prong ring | OGA swishes-sound-pack: `swish-9.wav`, `swish-4.wav`; OGA fantasy-sound-effects-tinysized-sfx: `paralyzer-discharge-01.wav`, `paralyzer-discharge-02.wav`; Kenney impact-sounds: `impactMetal_light_000.ogg`, `impactMetal_light_003.ogg`, `impactMetal_light_001.ogg`; Freesound 706676: `706676_Whoosh_Bamboo_1_by_Sadiquecat.ogg` |
| MusicBox (TinVirtuoso) | `sig.musicbox` | wind-up click + four music-box tine notes (a different little tune per variation) | OGA fantasy-accessory-sfx-library: `keyhole-lockbox-turn-02.wav`; OGA rpg-sound-pack: `inventory/metal-small2.wav`; OGA 100-cc0-sfx: `switch_02.ogg`; OGA 80-cc0-rpg-sfx: `item_gem_02.ogg`, `lock_02.ogg`; Kenney impact-sounds: `impactGlass_light_002.ogg` |
| MoonScythe (MoonProwler) | `cmb.swing.blade` | long curved scythe whoosh + blade shing + faint moonlight shimmer | Freesound 706676: `706676_Whoosh_Bamboo_1_by_Sadiquecat.ogg`; OGA rpg-sound-pack: `battle/sword-unsheathe2.wav`, `battle/sword-unsheathe5.wav`; OGA 80-cc0-rpg-sfx: `item_gem_04.ogg`, `blade_02.ogg`; OGA cure-magic: `Cure5.wav`; OGA magic-spell-sfx: `magical_3.ogg` |
| TeapotPopgun (SupperMortar) | `sig.tea` | teapot lid pop + porcelain clink + steam puff | OGA fantasy-accessory-sfx-library: `bottle-clay-uncork-01.wav`, `bottle-clay-uncork-02.wav`, `bottle-clay-uncork-03.wav`; OGA 100-cc0-sfx: `dishes_02.ogg`, `dishes_03.ogg`, `dishes_04.ogg`; OGA 100-cc0-sfx-2: `sfx100v2_air_03.ogg`, `sfx100v2_air_02.ogg`; OGA fantasy-sound-effects-tinysized-sfx: `compressed-air-spray-01.wav` |
| LanternStaff (LanternWeaver) | `wpn.cast.fire` | staff swing + lantern flame whoosh + lantern handle clink | OGA swishes-sound-pack: `swish-3.wav`, `swish-5.wav`, `swish-8.wav`; OGA catching-fire: `flame_0.ogg`; OGA rpg-sound-pack: `inventory/metal-small3.wav`, `inventory/metal-small1.wav`; OGA 80-cc0-rpg-sfx: `spell_fire_07.ogg`, `spell_fire_06.ogg`; Kenney impact-sounds: `impactMetal_light_004.ogg` |
| AlarmMortar (HourglassStag) | `sig.alarm` | mortar thump + rattling alarm-clock bell + tick | OGA 25-cc0-bang-firework-sfx: `cannon_04.ogg`, `cannon_05.ogg`, `shot_01.ogg`; OGA 100-cc0-sfx: `bell_02.ogg`, `switch_01.ogg`, `bell_01.ogg`; Kenney impact-sounds: `impactBell_heavy_003.ogg` |
| SquirtTrio (RainArchivist) | `wpn.split` | three quick water squirts + a drip | OGA 80-cc0-creture-sfx-2: `slime_07.ogg`; OGA rpg-sound-pack: `NPC/slime/slime3.wav`, `NPC/slime/slime5.wav`; OGA fantasy-sound-effects-tinysized-sfx: `water-drop-01.wav`, `water-drop-02.wav`, `water-drop-03.wav` |
| KiteSpool (KiteAdmiral) | `wpn.boomerang.fly` | spool string ratchet + kite cloth flap + wind | OGA fantasy-accessory-sfx-library: `keyhole-lockbox-turn-01.wav`, `keyhole-lockbox-turn-04.wav`, `keyhole-lockbox-turn-06.wav`; Kenney rpg-audio: `cloth1.ogg`, `cloth2.ogg`, `cloth3.ogg`; OGA air-whoosh: `whoosh2_0.wav` |
| ScoopLauncher (SundaeDuchess) | `wpn.launcher` | ice-cream scoop scrape + spring launch + soft scoop plop | OGA fantasy-sound-effects-tinysized-sfx: `metal-fork-scrape-02.wav`, `metal-knife-scrape-03.wav`, `handsaw-knife-scrape-01.wav`; OGA 100-cc0-metal-and-wood-sfx: `metal_spring_02.ogg`, `metal_spring_01.ogg`; OGA 100-cc0-sfx: `plop_02.ogg`, `plop_01.ogg`, `spring_03.ogg`; OGA 80-cc0-creture-sfx-2: `slime_01.ogg` |
| CuckooCannon (BonsaiClocksmith) | `wpn.launcher` | cannon thump + two-note cu-ckoo call (toy squeak pitched down a major third) + clock gear click | OGA 25-cc0-bang-firework-sfx: `cannon_02.ogg`, `cannon_03.ogg`, `cannon_05.ogg`; OGA squeak-toy-sounds: `squeak toy - squeak2.wav`, `squeak toy - squeak1.wav`, `squeak toy - squeak5.wav`; OGA 100-cc0-sfx: `switch_02.ogg`; OGA 100-cc0-sfx-2: `sfx100v2_switch_02.ogg`; Kenney rpg-audio: `metalClick.ogg` |
| JugglingPins (JugglingJester) | `wpn.bounce` | pin spin swish + two hollow wooden pin knocks with a soft body thump each (toss and catch) | OGA swishes-sound-pack: `swish-1.wav`, `swish-2.wav`, `swish-10.wav`; Kenney impact-sounds: `impactWood_light_000.ogg`, `impactSoft_medium_001.ogg`, `impactWood_light_002.ogg`, `impactSoft_medium_002.ogg`, `impactWood_light_004.ogg`, `impactSoft_medium_003.ogg` |
| SonarHarpoon (BellDiver) | `wpn.pierce` | harpoon launch thwack + sonar ping with one faint echo + bubbles | Freesound 536068: `536068_Bow_Release_Hit_by_EminYILDIRIM.ogg`; OGA 100-cc0-sfx-2: `sfx100v2_glass_04.ogg`; OGA rpg-sound-pack: `inventory/bubble2.wav`, `inventory/bubble.wav`, `inventory/bubble3.wav`; OGA thwack-sounds: `thwack-03.wav`, `thwack-02.wav`; OGA 100-cc0-sfx: `glass_01.ogg`; OGA 80-cc0-rpg-sfx: `item_gem_02.ogg` |


# Sound sources (CC0 / public domain only)

Downloaded 2026-11-04 to `D:/ABMobile/sfx_src/`. Every pack below was checked on its own asset page (the licence field of the OpenGameArt page, the licence banner on the Kenney page, the licence on each Freesound sound page) and, where present, against the licence/readme file inside the archive.

SHA256 is of the file as downloaded (archive or single audio file).

## Kenney (kenney.nl)

Download URLs are signed, short-lived links issued by `https://kenney.nl/download-link/<id>` from the asset page; the stable form is given. Licence verified twice: the asset page says "Creative Commons CC0", and License.txt inside each zip says "License: (Creative Commons Zero, CC0) http://creativecommons.org/publicdomain/zero/1.0/".

| Pack | Page | Download | Author | Licence | SHA256 | Size |
|---|---|---|---|---|---|---|
| rpg-audio | https://kenney.nl/assets/rpg-audio | https://kenney.nl/download/6173736574732f7270672d617564696f/<signature> (`kenney_rpg-audio.zip`) | Kenney Vleugels | CC0 1.0 (page + License.txt) | `6dbeaf8544da958d8f2adcb4a4a4b76c1ade34a05f8ab9edccd327da7375f38b` | 942 KB |
| impact-sounds | https://kenney.nl/assets/impact-sounds | https://kenney.nl/download/6173736574732f696d706163742d736f756e6473/<signature> (`kenney_impact-sounds.zip`) | Kenney Vleugels | CC0 1.0 (page + License.txt) | `029d734af1582474edf3a694d1b0cebc97c1c152f2f39fa34d4c2bafc5de77f8` | 782 KB |
| sci-fi-sounds | https://kenney.nl/assets/sci-fi-sounds | https://kenney.nl/download/6173736574732f7363692d66692d736f756e6473/<signature> (`kenney_sci-fi-sounds.zip`) | Kenney Vleugels | CC0 1.0 (page + License.txt) | `119340f351a5098ad814f78719438c0da355a9ce8a4c8a3af6a8d48aa3d49e04` | 5737 KB |

## OpenGameArt.org

Licence verified from the "License(s)" field on each page (only items showing CC0). Folder: `D:/ABMobile/sfx_src/oga_<slug>/`.

### 10-impactshield-blocks
- Page: https://opengameart.org/content/10-impactshield-blocks
- Author: StarNinjas
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/impact_-_starninjas.zip  
  SHA256 `4592764795344ae010561b0191b059a0e7ed8c2d2e6c0e96171debaa346cf311` (125 KB)

### 100-cc0-metal-and-wood-sfx
- Page: https://opengameart.org/content/100-cc0-metal-and-wood-sfx
- Author: rubberduck
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/100-CC0-wood-metal-SFX.zip  
  SHA256 `be6eba63b03409ac0c77787a956b1503a7c186403d04aef9725c52644a4b7878` (1943 KB)

### 100-cc0-sfx
- Page: https://opengameart.org/content/100-cc0-sfx
- Author: rubberduck
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/100-CC0-SFX_0.zip  
  SHA256 `a5c135878c132f1c59cca54e60061c296cd0ac27ad031ca2c41b8cd5cab3c706` (2853 KB)

### 100-cc0-sfx-2
- Page: https://opengameart.org/content/100-cc0-sfx-2
- Author: rubberduck
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/sfx_100_v2.zip  
  SHA256 `0fc61b4494e2e893c0c015ced4877b3f689c7d84a48cb61daecd7ddb52db797b` (2312 KB)

### 15-vocal-male-strainhurtpainjump-sounds
- Page: https://opengameart.org/content/15-vocal-male-strainhurtpainjump-sounds
- Author: qubodup
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/slightscreams.7z  
  SHA256 `3220b234efed17305bef05fe82c5d2e2b38302ae02b0d188021af9b045499a1e` (1410 KB)

### 150-combat-sound-effects
- Page: https://opengameart.org/content/150-combat-sound-effects
- Author: untamedpixels
- Licence: CC0 (OGA licence field); LICENSES.txt inside: all 150 WAVs CC0 1.0, compiled from CC0 sources (CoMiGo, Kenney, Kronbits, lentikula itch.io packs) with per-file source IDs
- Download: https://opengameart.org/sites/default/files/150gameeffectsounds.zip  
  SHA256 `11891df177ed5f74bcf6798e149733258e5550dc1a4951af323bbf66be30cd47` (37376 KB)

### 16-monster-growls
- Page: https://opengameart.org/content/16-monster-growls
- Author: StarNinjas
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/monster_-_starninjas.zip  
  SHA256 `7542446ebc69892e4267d8b3abd3167193505833692da9624c01c8da392d9065` (239 KB)

### 20-sword-sound-effects-attacks-and-clashes
- Page: https://opengameart.org/content/20-sword-sound-effects-attacks-and-clashes
- Author: StarNinjas
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/sword_-_starninjas_1.zip  
  SHA256 `87a1fe1cfeee5f282fe06a7a9c25ab9ebf24b7674fe19171b7f2762dffb628c1` (146 KB)
- Download: https://opengameart.org/sites/default/files/sword_clash_-_starninjas_0.zip  
  SHA256 `f363c80ea1627548d336370750651b0d2d092978148ecfb3642717c2efb54b6b` (141 KB)

### 25-cc0-bang-firework-sfx
- Page: https://opengameart.org/content/25-cc0-bang-firework-sfx
- Author: rubberduck
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/25-CC0-bang-sfx.zip  
  SHA256 `c0c9ecc11e2dc0d190f0cced755569858234b8528286bc83becb6314c663407f` (1357 KB)

### 3-dark-magic-spells
- Page: https://opengameart.org/content/3-dark-magic-spells
- Author: qubodup
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/dark_magic.7z  
  SHA256 `9527c2a952763a554ae79aaccd5a696eafe8be5df5eb7e79fd61ddcc406e2450` (804 KB)

### 3-melee-sounds
- Page: https://opengameart.org/content/3-melee-sounds
- Author: remaxim
- Licence: CC0 (OGA licence field); melee.txt inside: "public domain, Gemeindegut, cc0-licensed or WTFPL-licensed"
- Download: https://opengameart.org/sites/default/files/melee%20sounds.zip  
  SHA256 `7825110b47d599b7ccdb492939806e18726ca36d1e76f06468ebbec14fc63393` (241 KB)

### 3-pop-sounds
- Page: https://opengameart.org/content/3-pop-sounds
- Author: wubitog
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/3pops.zip  
  SHA256 `b54c299913b6502e5d07ba62bcc6726f13498ee848d49f25fc4390e0b6103a82` (88 KB)

### 5-hit-sounds-dying
- Page: https://opengameart.org/content/5-hit-sounds-dying
- Author: TinyWorlds
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/5Hit_Sounds.zip  
  SHA256 `fb345dac7eb9d8898a7aa48ac17f5852f9365e151821a74f6e156cfb86bb96fa` (118 KB)

### 75-cc0-breaking-falling-hit-sfx
- Page: https://opengameart.org/content/75-cc0-breaking-falling-hit-sfx
- Author: rubberduck
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/sfx_breaking_and_falling.zip  
  SHA256 `e6ee04d91c5f4d30cfda1260d2c9d1faf96fda36319287215fbd07bcb1a80451` (1586 KB)

### 80-cc0-creature-sfx
- Page: https://opengameart.org/content/80-cc0-creature-sfx
- Author: rubberduck
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/80-CC0-creature-SFX_0.zip  
  SHA256 `5d4002eea42f278e76543bb36e8ddd2cb978588a13271984191d8228f5d26768` (1841 KB)

### 80-cc0-creture-sfx-2
- Page: https://opengameart.org/content/80-cc0-creture-sfx-2
- Author: rubberduck
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/80-CC0-creature-sfx-2.zip  
  SHA256 `6d251aeaaaa9c2fffed8fc2f64bab3104725ceabf9259a8441994e7a20cfe22f` (1845 KB)

### 80-cc0-rpg-sfx
- Page: https://opengameart.org/content/80-cc0-rpg-sfx
- Author: rubberduck
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/80-CC0-RPG-SFX_0.zip  
  SHA256 `1c2f06ff4e8563b5b8b745b23cf213c1474142a69bb82bd8f5e10d9b3f7a7bbd` (1801 KB)

### air-whoosh
- Page: https://opengameart.org/content/air-whoosh
- Author: pyranostudios
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/whoosh2_0.wav  
  SHA256 `d3fbff5bb78b289af16b2dfa29083f07ef83b0cbaff4514c48dfda16d082b468` (903 KB)

### battle-sound-effects
- Page: https://opengameart.org/content/battle-sound-effects
- Author: (see page; credited in page Author field)
- Licence: CC-BY 3.0, CC-BY-SA 3.0, GPL 3.0, GPL 2.0, CC0 (OGA licence field); Page lists CC-BY 3.0 / CC-BY-SA 3.0 / GPL 3.0 / GPL 2.0 / CC0 as alternatives; we use it under the CC0 option
- Download: https://opengameart.org/sites/default/files/battle_sound_effects_0.zip  
  SHA256 `44e3d26b2378d2eb3a4f28b4c5cbc71908ad13c7389038348dbe9b8cde4f4c05` (215 KB)

### bear-growls
- Page: https://opengameart.org/content/bear-growls
- Author: AntumDeluge
- Licence: CC0 (OGA licence field); README.txt inside: CC0; source U.S. Fish & Wildlife Service (US federal public domain)
- Download: https://opengameart.org/sites/default/files/bear.zip  
  SHA256 `7fbcc09b292019926e00a460831267fc55ae658e3265bb3c1d4e806920699723` (112 KB)

### big-scary-troll-sounds
- Page: https://opengameart.org/content/big-scary-troll-sounds
- Author: Darsycho
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/troll-roars_0.ogg  
  SHA256 `7dc09cc3c3e424b66ac564ac83ac89ee91885c964780694d13f2c4957ac813c7` (123 KB)
- Download: https://opengameart.org/sites/default/files/troll-idle-noises_0.ogg  
  SHA256 `67d1e35bca545c9c17618333265deb90ed506d14e5b45ea730ef012695bd682a` (284 KB)
- Download: https://opengameart.org/sites/default/files/scrapped-troll-sounds.ogg  
  SHA256 `90f2111887c0ad1569f1e91da8044aa15f605a3e30f7abb8f4475adb848a1f4e` (114 KB)

### cannon-fire
- Page: https://opengameart.org/content/cannon-fire
- Author: Thimras
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/cannon_fire_0.ogg  
  SHA256 `2b21ffa41e5c3be4eecdfbd3490544f5aa8b6d1581787265863e13de1a8e599d` (175 KB)

### catching-fire
- Page: https://opengameart.org/content/catching-fire
- Author: themightyglider
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/flame_0.ogg  
  SHA256 `c1eaafba3746374c68c30dc8b7ec895dac0e1e66064522f03340dc9b2da4a2cd` (8 KB)

### cc0-deep-monster-roar
- Page: https://opengameart.org/content/cc0-deep-monster-roar
- Author: trazzz123
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/monster_roar.wav  
  SHA256 `040d2841619c732098c30a638731317d13d7647e5ce1a75a3246219ce4b162ff` (1244 KB)

### chunky-explosion
- Page: https://opengameart.org/content/chunky-explosion
- Author: Joth
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/Chunky%20Explosion.mp3  
  SHA256 `4de53bffac187e59d3f5b43096df2f8fcb4160076b65b71a67074947950e025f` (113 KB)

### cure-magic
- Page: https://opengameart.org/content/cure-magic
- Author: Someoneman
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/curemagic.zip  
  SHA256 `5a0d60a13444d719d05bb496aceac3d749a925259297f1828a51563ee2e465d8` (1573 KB)

### dull-explosion
- Page: https://opengameart.org/content/dull-explosion
- Author: qubodup
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/explosion_dull.flac  
  SHA256 `a463ff6b4cc37321f646ddbe9ff326aef244dea33d3951bc2a833a27cc611bf9` (58 KB)

### earth-element-magic-spell
- Page: https://opengameart.org/content/earth-element-magic-spell
- Author: qubodup
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/Earth%20Element%20Magic%20Spell_0.flac  
  SHA256 `1248f39e7850dc3645e3e2185a38e7a499177649bcec9c793f58aba838281e1e` (706 KB)
- Download: https://opengameart.org/sites/default/files/Earth%20Element%20Magic%20Spell_3.ogg  
  SHA256 `8cd4f45a8e0ddeee0b9f9f576a560e4cd25b5533b555ee9968c554c2d0d3494f` (26 KB)

### earthquake-and-open-sesame
- Page: https://opengameart.org/content/earthquake-and-open-sesame
- Author: mieki256
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/shake.flac  
  SHA256 `344a5703ac9ee0b7cf28e007f522d6ec24426447c4ff3cae8e57dec48eda1ba9` (473 KB)

### fantasy-accessory-sfx-library
- Page: https://opengameart.org/content/fantasy-accessory-sfx-library
- Author: Vehicle
- Licence: CC0 (OGA licence field); readme.txt inside: "Distributed under Creative Commons Zero"
- Download: https://opengameart.org/sites/default/files/accessory.zip  
  SHA256 `1d1166b2469be771d8894cd77ffcd6d500bcb676e2b6cb1c9cecacf97e957217` (14220 KB)

### fantasy-sound-effects-tinysized-sfx
- Page: https://opengameart.org/content/fantasy-sound-effects-tinysized-sfx
- Author: Vehicle
- Licence: CC0 (OGA licence field); readme.txt inside: "Distributed under Creative Commons Zero"
- Download: https://opengameart.org/sites/default/files/tinysized.zip  
  SHA256 `7f00e2b1dae15e68db0e1d6096767f03755e9e7c247128afc3e935e63774d7db` (12924 KB)

### fantasy-weapons-and-apparel-sfx-library
- Page: https://opengameart.org/content/fantasy-weapons-and-apparel-sfx-library
- Author: Vehicle
- Licence: CC0 (OGA licence field); readme.txt inside: "Distributed under Creative Commons Zero"
- Download: https://opengameart.org/sites/default/files/weapons-apparel.zip  
  SHA256 `81cc6317f21b68f8e7f2b23a5a0f5f33c301c39ee41347307eec02201be989e5` (12312 KB)

### female-hurt-grunts-groans
- Page: https://opengameart.org/content/female-hurt-grunts-groans
- Author: Nocturnal_Vanguard
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/female_hurt_grunts_groans_1.ogg  
  SHA256 `80a9fb09203bb8feb065ba15197609b118cbe167c9a2264670a31ee22c384adb` (184 KB)

### fire-crackling
- Page: https://opengameart.org/content/fire-crackling
- Author: AntumDeluge
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/fire-1_0.ogg  
  SHA256 `f922929805bc6afdf0a35727fa3998033db544c1b6f62ed3aeab94341ed6ddeb` (22 KB)
- Download: https://opengameart.org/sites/default/files/fire-1.wav  
  SHA256 `26702de8a195bcbafeae72034861d875a1b7917168a25106f4e1957ecdddf7e5` (247 KB)

### fire-staff-sound-effects
- Page: https://opengameart.org/content/fire-staff-sound-effects
- Author: LEGIT Audio
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/00_-_legit_audio_-_legit_fir_fire_staff_20-audio_0.mp3  
  SHA256 `d122b52ba5ad7fd58f4a3c9e512d09a5ab1b0714ea23d2ec8c45a18d7c27d042` (50 KB)
- Download: https://opengameart.org/sites/default/files/legit_audio_fire_staff_-_free_0.zip  
  SHA256 `8c0668fdde03de52b9b201e672244992ae739438937d4bef6908d1618a86b020` (28556 KB)

### flare-ignition
- Page: https://opengameart.org/content/flare-ignition
- Author: qubodup
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/ignition.flac  
  SHA256 `bd8281bcee65bc705755fd36bdf40d04fadddd474f52fd481233523e178b4ea1` (231 KB)

### freeze-spell-0
- Page: https://opengameart.org/content/freeze-spell-0
- Author: artisticdude
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/freeze.wav  
  SHA256 `dc16da845f18f88885db4584205a4ad9b84a8da6e377b79d4511b366cc8631f1` (599 KB)

### goblins-sound-pack
- Page: https://opengameart.org/content/goblins-sound-pack
- Author: artisticdude
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/goblins_0.zip  
  SHA256 `480e0ff1df5eda83a32f2781b4b5bd65606e8f43e0ca52175c1a9e08e2bc7d40` (1327 KB)

### grunts-male-death-and-pain
- Page: https://opengameart.org/content/grunts-male-death-and-pain
- Author: thebardofblasphemy
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/death%20pain%20grunts.wav  
  SHA256 `3f00fba827f20ff9ccea1bc07d2c21ab8d3a36e81b48a6005cf8459f02779586` (4277 KB)

### hungry-ghoul
- Page: https://opengameart.org/content/hungry-ghoul
- Author: Darsycho
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/hungry-ghoul_0.ogg  
  SHA256 `efe8b72a072b38927a4faea0a5f200d1b4534a9430bae9ec4bccf317a7efad6c` (347 KB)

### hurt-sound-effects
- Page: https://opengameart.org/content/hurt-sound-effects
- Author: EZduzziteh
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/hurt_01_0.mp3  
  SHA256 `45f19e4f637ff0e8936186977aa7cb131f67a95ca68660686476a47da224322e` (6 KB)
- Download: https://opengameart.org/sites/default/files/hurt_05.mp3  
  SHA256 `a70d46a0a20c1e231fdfd4e8154248be9a5e0ff38e84fa08eaf7f25dea749e93` (8 KB)
- Download: https://opengameart.org/sites/default/files/hurt_03.mp3  
  SHA256 `9950c1dbbf3f2c75952510f1f2a2ab6f36586ba7e62f4f1e74ad64e82db2aa52` (3 KB)
- Download: https://opengameart.org/sites/default/files/hurt_04.mp3  
  SHA256 `a6249c89f037553c71529ece4eb443bd3cc06ab33b833c5b3d47121da284d7d8` (4 KB)
- Download: https://opengameart.org/sites/default/files/hurt_06.mp3  
  SHA256 `01bc209cec8f344cad5f7c77eaad0f8a00a6d64c649b514c7ff423d02aec5085` (3 KB)
- Download: https://opengameart.org/sites/default/files/hurt_02.mp3  
  SHA256 `7c4cd404830bf3cd5f10ff3340266f573922399d06bfb4573daf9093c5b69833` (4 KB)

### ice-breakingshattering
- Page: https://opengameart.org/content/ice-breakingshattering
- Author: IgnasD
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/IceShatters_0.zip  
  SHA256 `b84ea396fac1af183dd2cb205a01edaa94b493ffb36965eafe59912e41cf9eb7` (73 KB)

### ice-spells
- Page: https://opengameart.org/content/ice-spells
- Author: bart
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/icespells.zip  
  SHA256 `45f02a204fc7cc896a17487080376c521ac2e564fe3da4d24cffa0086f8d6a54` (263 KB)

### magic-spell-sfx
- Page: https://opengameart.org/content/magic-spell-sfx
- Author: JaggedStone
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/magical_1_0.ogg  
  SHA256 `119f9061a566c72c1c58444b03f8685b833079ef268dda02fe742eb34d3bc68f` (57 KB)
- Download: https://opengameart.org/sites/default/files/magical_2.ogg  
  SHA256 `84a70f63f3f21e5ba4a48285b760a663d422ad3feddba2f3cbadd94b76cdc52e` (38 KB)
- Download: https://opengameart.org/sites/default/files/magical_3.ogg  
  SHA256 `9fac648a1de869cf9ca2f4b3f1cd47f1766a3ee9d2fdec45831234401d7b2392` (37 KB)
- Download: https://opengameart.org/sites/default/files/magical_4.ogg  
  SHA256 `b7862d109b4013dfb2a389a10f8917203beb0efc458444bc5a1b8ada605cc118` (77 KB)
- Download: https://opengameart.org/sites/default/files/magical_5.ogg  
  SHA256 `69aaaaed729cacb518005a5d24074196db3c5fffc0ed2cd01ad864fbed4a62be` (36 KB)
- Download: https://opengameart.org/sites/default/files/magical_6_0.ogg  
  SHA256 `0a18ee3460cf3fd79c3771bbf94c20c26d34b65ab60d0b0e15694ffcad815955` (36 KB)
- Download: https://opengameart.org/sites/default/files/magical_7_0.ogg  
  SHA256 `2817da4f5df69ff993baffd6594a2f2a069f85ee2a02019f97db2246e350a76b` (74 KB)

### magic-words-healing-sound-effect
- Page: https://opengameart.org/content/magic-words-healing-sound-effect
- Author: Spring Spring
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/magic_words.wav  
  SHA256 `6e14f3a011d5f37f5e45b5ef49efef0edd7687dcc2550ff033e040a34add23be` (1327 KB)
- Download: https://opengameart.org/sites/default/files/health_restore.wav  
  SHA256 `c254c71a4e09af6cfe0825f16ac466c745e694cbaf30487f0a1ec7186598f3dd` (1139 KB)

### mech-stomp-step-sound
- Page: https://opengameart.org/content/mech-stomp-step-sound
- Author: hc
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/stomp.flac  
  SHA256 `c9114c31ab07a371db12bfb22190d3a8e08424813319b67b6040df1f23fa9bba` (11 KB)

### monster-or-beast-sounds
- Page: https://opengameart.org/content/monster-or-beast-sounds
- Author: pauliuw
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/monsters_or_beasts.7z  
  SHA256 `b032a255a22e32e08b31c9e7adc560b899d24661da12af5320346e5aa4c059ec` (4783 KB)

### monster-snarls
- Page: https://opengameart.org/content/monster-snarls
- Author: Darsycho
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/monster-snarls_0.ogg  
  SHA256 `7401193d0cc5eb193379cf326c8868d6823a284ebc32ffaa298c1a94d2e25994` (191 KB)
- Download: https://opengameart.org/sites/default/files/monster-snarls-2_0.ogg  
  SHA256 `dbc7b5e621892b5dbc27ee25230b57daaddbc83daadfb20131988b7367f967e1` (73 KB)
- Download: https://opengameart.org/sites/default/files/monsters-snarls-3_0.ogg  
  SHA256 `5917d2697b8688b2c59e592d448d473ae0eeaed83802541ebd19bc87f97fe8a7` (199 KB)
- Download: https://opengameart.org/sites/default/files/monster-snarl-attack.ogg  
  SHA256 `34bf1ceef265725abf00ab3263bbbb492792e126bb1619addd4a5d3ad123347f` (181 KB)
- Download: https://opengameart.org/sites/default/files/monster-snarl-5.ogg  
  SHA256 `aeaf9ab0f25cb8b035a3257913206afd8ba33d47876106291d38dfac7e5c07aa` (196 KB)

### monster-sound-effects-2
- Page: https://opengameart.org/content/monster-sound-effects-2
- Author: Ogrebane
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/monster_sfx_pack_2.zip  
  SHA256 `8d9831e5596446ebafb8e6a958e757f07cbd385f9e417b4d9704d63a05a2cd63` (1981 KB)

### monster-sound-effects-pack
- Page: https://opengameart.org/content/monster-sound-effects-pack
- Author: Ogrebane
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/monster_sfx_pack.zip  
  SHA256 `f39c5ae356b1fc01284057ee48e35ec1a52fcc84bfa959b77d0a696adc8459e4` (1394 KB)

### monster-sound-pack-volume-1
- Page: https://opengameart.org/content/monster-sound-pack-volume-1
- Author: Ogrebane
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/monster-sounds-volume-2.zip  
  SHA256 `f23b5691ac502cf03b58a353cb31f53e3cb93aeb65e654efc8482fd1829d7ef6` (1976 KB)

### rpg-sound-pack
- Page: https://opengameart.org/content/rpg-sound-pack
- Author: artisticdude
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/rpg_sound_pack.zip  
  SHA256 `f80754a9c04854e30c9da89d0186540ad097309d98b41387b6e83029e2601421` (12173 KB)

### rumble-fx
- Page: https://opengameart.org/content/rumble-fx
- Author: cinameng
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/rumble.wav  
  SHA256 `c20cd36c670fe8291bc38271f47135599c00f8d88bc25af48abf2c5e58774e4a` (4933 KB)

### silent-beast-growls4
- Page: https://opengameart.org/content/silent-beast-growls4
- Author: pauliuw
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/silent_beast_growl.mp3  
  SHA256 `1bc5f5bb6e2b6d9db2a18eeb7d1d794a9445151be0d43b0aa09d900a3fd955e2` (107 KB)
- Download: https://opengameart.org/sites/default/files/silent_beast_growl_1.mp3  
  SHA256 `adcb4ac105e2e4be373092f4dd27e030b22d20dfea65e84cdd0d4e75f798ed55` (43 KB)
- Download: https://opengameart.org/sites/default/files/silent_beast_growl_2.mp3  
  SHA256 `ef62c1647a2c2d7b80390e45932b8772006f31a491b91584fa0cbcfa7618aff0` (76 KB)
- Download: https://opengameart.org/sites/default/files/silent_beast_growl_3.mp3  
  SHA256 `4cbde8827f5cccdc435bbb90c2d1110695b21820d11273eb40755e9d80755159` (53 KB)

### squeak-toy-sounds
- Page: https://opengameart.org/content/squeak-toy-sounds
- Author: bart
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/squeak_toy.zip  
  SHA256 `3ba6419b05ebf8e0b7dd4395b3c10f26811c037e357143f10a159acdb2dabfb8` (877 KB)

### squeaky-rat
- Page: https://opengameart.org/content/squeaky-rat
- Author: qubodup
- Licence: CC0 (OGA licence field); notes.txt inside: released under CC0, derived from CC0 Freesound sources
- Download: https://opengameart.org/sites/default/files/qubodupSqueakyRat.7z  
  SHA256 `966649ba0e136ec2d473623e15d11710b7fcc8f73a30897758453e8d43bb75a7` (90 KB)

### swishes-sound-pack
- Page: https://opengameart.org/content/swishes-sound-pack
- Author: artisticdude
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/swishes.zip  
  SHA256 `7980215241b739a787dcf26f660ce510bb237900968789395a86911619795693` (376 KB)

### teleport-spell
- Page: https://opengameart.org/content/teleport-spell
- Author: Ogrebane
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/teleport.wav  
  SHA256 `23fe938e7e82931227196bf6b2978ad9154f627af26623acf52958d12125f3b8` (279 KB)

### thwack-sounds
- Page: https://opengameart.org/content/thwack-sounds
- Author: AntumDeluge
- Licence: CC0 (OGA licence field); LICENSE.txt inside: full CC0 1.0 Universal legal code
- Download: https://opengameart.org/sites/default/files/thwack-1.0.zip  
  SHA256 `8edb002735a01369ef4d6259573be4885f9fe32c9a9efa89dfbbb14067c8c8e5` (369 KB)

### toy-double-barrel-shotgun-sounds
- Page: https://opengameart.org/content/toy-double-barrel-shotgun-sounds
- Author: JumboSizedFish
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/toy-double-barrel-shotgun-equip.wav  
  SHA256 `55773a930282a1822c7d097e20f2b652f1070f0634c93058e392d3b30214de15` (58 KB)
- Download: https://opengameart.org/sites/default/files/toy-double-barrel-shotgun-cocked.wav  
  SHA256 `8e227d26dfff14b8f58294b929414c1d2db9ac2182ddd1a60d4560dc7d967204` (72 KB)
- Download: https://opengameart.org/sites/default/files/toy-double-barrel-shotgun-uncocked.wav  
  SHA256 `383c0a1f4662ece4d9606872086b29e903b055c93d13e6aa1ee09c5880bf7f5d` (86 KB)
- Download: https://opengameart.org/sites/default/files/toy-double-barrel-shotgun-reloading.wav  
  SHA256 `bbe9f686d13acf333c10e7af72218464c9fd63b911032f952082504dc795f865` (276 KB)
- Download: https://opengameart.org/sites/default/files/toy-double-barrel-shotgun-left-trigger.wav  
  SHA256 `d9569daa9dd7c36fde8c5c701743dcae23901f7b48b5e92e25728a549768a6c3` (48 KB)
- Download: https://opengameart.org/sites/default/files/toy-double-barrel-shotgun-right-trigger.wav  
  SHA256 `592554f5a7f7e27e3a2bd1a94fbf029741fcb7fcc4fe552b05b097b3a17cdbcd` (68 KB)
- Download: https://opengameart.org/sites/default/files/toy-double-barrel-shotgun-dual-trigger.wav  
  SHA256 `dc4bac1e6c5bd35e60063cc384ab921b120dac6b6c2cf388edb4b8db2771dd8a` (72 KB)

### tree-creaking
- Page: https://opengameart.org/content/tree-creaking
- Author: AntumDeluge
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/tree_creak_0.ogg  
  SHA256 `465356e0e32e71a5c29fda529a0ac23af7791c62e3bd862e8cb2961eb104e1b1` (69 KB)
- Download: https://opengameart.org/sites/default/files/tree_creak.flac  
  SHA256 `9bab7c7f68850136c43fb91216c7c1760ac2bda22b2a31659905c559a464d422` (226 KB)

### various-paper-sound-effects
- Page: https://opengameart.org/content/various-paper-sound-effects
- Author: Luckius
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/sounds_6.zip  
  SHA256 `aa1889414310c7cfdd8dc291c19d4e4fc21ec987e9238b0d961fb72449c263a4` (951 KB)
- Download: https://opengameart.org/sites/default/files/paper_crushed_-_1.mp3  
  SHA256 `f43b208fada1e29e086dc3b9df8d1208dea7d5b0a0496a755d793c2f620b7de2` (27 KB)
- Download: https://opengameart.org/sites/default/files/paper_crushed_-_2.mp3  
  SHA256 `0c1e7b561932d69abf569833f831e6095f85520faa884d495f7c1a465a3a1c0b` (19 KB)
- Download: https://opengameart.org/sites/default/files/paper_ripped_-_1.mp3  
  SHA256 `e282eb21412fb78d6f31ba88fae52a94b23cc9e75e5531b59a1e0cef4358d728` (11 KB)
- Download: https://opengameart.org/sites/default/files/paper_sound_-_1.mp3  
  SHA256 `f62e6cab41a9c0a8c67b5752c95e80e6adc681110d579ada982627909be1773d` (12 KB)
- Download: https://opengameart.org/sites/default/files/paper_sound_-_2.mp3  
  SHA256 `6e81a68cfe8283f7e0ebfae637f15c1a2de4b9e4a306d0da2338663c6d5d7196` (12 KB)
- Download: https://opengameart.org/sites/default/files/paper_sound_-_3.mp3  
  SHA256 `20dae9f0684f732015e74a01bda408fe516a9b7a1214d187ef8c956eb5c64a3d` (14 KB)
- Download: https://opengameart.org/sites/default/files/paper_sound_-_4.mp3  
  SHA256 `69f65b053e6b96b1ffe065637f20e5dce990de3ce600fe6fb837d73243f76d41` (52 KB)

### zombies-sound-pack
- Page: https://opengameart.org/content/zombies-sound-pack
- Author: artisticdude
- Licence: CC0 (OGA licence field)
- Download: https://opengameart.org/sites/default/files/zombies.zip  
  SHA256 `8096ceeb4dac35d9eef9bfe0cc6649778d1eebf141b208d394ac338b9e5be314` (4557 KB)

## Freesound.org (single sounds, CC0 only)

Each sound page shows "Creative Commons 0" with a link to creativecommons.org/publicdomain/zero/1.0/; checked per sound before download. Original-quality downloads need a Freesound login, so these are the public high-quality OGG previews (~192 kbps Vorbis), which carry the same CC0 dedication. Folder: `D:/ABMobile/sfx_src/freesound_cc0/<category>/`.

| ID | Title (author) | Page | Download | SHA256 |
|---|---|---|---|---|
| 179996 | Arrow Release and Hit.wav by calvarychurchatlanta | https://freesound.org/people/calvarychurchatlanta/sounds/179996/ | https://cdn.freesound.org/previews/179/179996_3033176-hq.ogg | `aa7d1d88881c82f60d739aa622dbd4cb5d5a4a308a50ae00536f3808d5da8090` |
| 205938 | Arrow Impact by Twisted_Euphoria | https://freesound.org/people/Twisted_Euphoria/sounds/205938/ | https://cdn.freesound.org/previews/205/205938_3842302-hq.ogg | `0a2d5b78d4fe7ee755134cd82862695df62dde69cb7ce5cc708382f7d0701173` |
| 263675 | Bow_release.wav by PorkMuncher | https://freesound.org/people/PorkMuncher/sounds/263675/ | https://cdn.freesound.org/previews/263/263675_4946670-hq.ogg | `da3e4df20952ee582b1f87c4cf1903a867df32dddd8a2b20505988a08399e86d` |
| 384919 | Crossbow Firing and Hitting Target by Ali_6868 | https://freesound.org/people/Ali_6868/sounds/384919/ | https://cdn.freesound.org/previews/384/384919_984733-hq.ogg | `422baafc02fbd13292b7ccec686b207260cbb4059725319ebb8a33741f941b24` |
| 394004 | Arrows Fly By.mp3 by DigPro120 | https://freesound.org/people/DigPro120/sounds/394004/ | https://cdn.freesound.org/previews/394/394004_7414526-hq.ogg | `11083f0be9b1dd4213fc37f207e53f3a152eb0d492b38b6ba0dcc1b35b544872` |
| 394180 | Arrow Loose and Flyby by saturdaysoundguy | https://freesound.org/people/saturdaysoundguy/sounds/394180/ | https://cdn.freesound.org/previews/394/394180_7280678-hq.ogg | `437def673bc215297b14989cd74e86f90f84305a468613ab1d7d5f81088a4d23` |
| 490556 | Bow Drawn by Paveroux | https://freesound.org/people/Paveroux/sounds/490556/ | https://cdn.freesound.org/previews/490/490556_992525-hq.ogg | `9492219fa2d3e365803be6b4a2cc56dc080f559d7e66a3f2fcfe22eab75f344e` |
| 521552 | Arrow Impact by omerbhatti34 | https://freesound.org/people/omerbhatti34/sounds/521552/ | https://cdn.freesound.org/previews/521/521552_11543986-hq.ogg | `3c43c0bb2ce80085b3dfdb88985cc6e562c3ba952c76ad3c591776ece6e143bb` |
| 536068 | Bow Release Hit by EminYILDIRIM | https://freesound.org/people/EminYILDIRIM/sounds/536068/ | https://cdn.freesound.org/previews/536/536068_10912485-hq.ogg | `b519198b2b6159608bb3492572923aa89a5d20aa603818d7ff55de9573bf714f` |
| 789389 | Arrow Flyby by modusmogulus | https://freesound.org/people/modusmogulus/sounds/789389/ | https://cdn.freesound.org/previews/789/789389_15956618-hq.ogg | `febe1507710062c941762bd998307059ec40e313a5d4fd446936a23ada42e8ff` |
| 855248 | Real Bullet Flyby Sound by qubodup | https://freesound.org/people/qubodup/sounds/855248/ | https://cdn.freesound.org/previews/855/855248_71257-hq.ogg | `e52798b8774eec822eb76483e98577e2606a79de43f6814a2478f6a84e37f885` |
| 420668 | Basic Melee Swing / Miss / Whoosh by SypherZent | https://freesound.org/people/SypherZent/sounds/420668/ | https://cdn.freesound.org/previews/420/420668_6544098-hq.ogg | `031b85dab648ed864aa72190ae70887a6ac5636cd927f20d70af0f902be4c32a` |
| 420670 | Strong Melee Swing by SypherZent | https://freesound.org/people/SypherZent/sounds/420670/ | https://cdn.freesound.org/previews/420/420670_6544098-hq.ogg | `853cfcd87825fbfbdab657319935828224649e147696f27b835f561b8516bd15` |
| 464839 | Anime Sound Effect - Piercing impact / Stabbing by Breviceps | https://freesound.org/people/Breviceps/sounds/464839/ | https://cdn.freesound.org/previews/464/464839_9159316-hq.ogg | `bb3247b56c98459a72593c7b5be50ff51b381589771946914ac6cd54f8244e22` |
| 706676 | Whoosh Bamboo 1 by Sadiquecat | https://freesound.org/people/Sadiquecat/sounds/706676/ | https://cdn.freesound.org/previews/706/706676_5287430-hq.ogg | `5d69380f40c77655351eb8d192c5f585a0d15b522f13a122a4e860084b923ad3` |

## Considered and rejected

- OGA "Medieval sound effects - Weapon impacts / Weapon textures" (StillNorth Media; Ben Jaszczak & Brian Nelson; ~330 MB): OGA marks CC0, but the authors' own terms (quoted on the page, site now dead) are "use freely, no royalties, no credit" - not a CC0 dedication verifiable on the author's page. Not downloaded. Contains real longbow draw/shoot, arrow pass-bys, spear swing; owner may decide.
- OGA "40 wet towel club/pound/hit/attack sounds" (qubodup): page says CC0 but the info file inside says MIT/ZLIB/BSD (requires keeping a notice). Deleted.
- OGA "Gorilla Sounds" (AntumDeluge): readme credits mediacollege.com recordings, whose terms are not CC0. Deleted.
- OGA independent.nu ljudbank packs (35 wooden cracks, 37 hits/punches, 5 break/crunch), "Glass Break", "Spell sounds", "Osare Minotaur": uploaded by someone other than the author or multi-licence with unclear origin; not downloaded.
- BigSoundBank.com (CC0, has real archery #0549 and "arrow on a target" #0548): the site refuses scripted downloads (HTTP 403); download by hand in a browser if wanted.
- Battle Sound Effects: included under its CC0 option only.
