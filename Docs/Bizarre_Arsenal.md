# Bizarre Arsenal — new weapons, one weapon per hero, starting friends, animation pass
Date: 2026-10-17. Owner request: add every weapon found in Bizarre Brigade research (passes 1 and 2; pass 3 is running and will be appended), give each of the 31 playable characters a different weapon that fits who they are, review each hero's starting companion, and do every item of the animation audit.

Owner decision on art: **wait for new art.** Every new weapon is coded now, but it only enters the game (shop pool, hero loadout, starter) once the image agent has delivered its art. No procedural stand-ins, no borrowed art. `ArsenalBuilder` checks for the files listed in §5 and wires each weapon the moment its art exists.

Rule for all new weapons: **nothing radiates from the wielder's own body.** Every attack is thrown, fired, planted ahead or swung in front.

Sources: research notes in the session scratchpad (`bb/`, `bb2/`): Fandom wikitext, ~60 YouTube transcripts, frames from 54 videos (the "Naming Demon" Gon cycles through the whole weapon pool frame by frame), Bahamut / Gamersky / TapTap / note.com articles. Weapons in Bizarre Brigade show only as icons, so names here are descriptive.

---

## 1. New weapons (24 + 8 from pass 3 + 3 own takes + 6 from pass 4 = 41)

Numbers are tier-1 values; tier 2/3 follow the usual `CompanionTierScaling`. W = weapon damage. "Hero" = the playable character who carries it (§2). All are `ArsenalWeapon` mechanics resolved in `Runtime/Core/ArsenalCombat.cs`.

| # | Id | From Bizarre Brigade | Kind | Mechanic | Look (wind-up → shot → impact) | Hero |
|---|---|---|---|---|---|---|
| 1 | CherryFuseBomb | black round bomb with lit red fuse (Gon cycle; ILWS 575–680) | lob | Lobs a bomb onto the target's spot (0.55 s flight, shadow on the floor). It bounces once, fuse fizzes 0.8 s, then bursts r 1.1 for 160% W. Tier 2: two bombs; tier 3: the burst throws 3 bomblets (r 0.6, 40% W). | Arm swings back; glossy cherry-black bomb with a candy-stripe fuse arcs over; on the floor its fuse spits sparks and a red rim blinks faster; bursts as a cotton-cloud puff with black-gold confetti and a hand-lettered POP. | — |
| 2 | PopperSticks | red dynamite / firecracker sticks thrown, landing, exploding (gZL 526; 8eR 56–662) | lob, mines | Throws 3 red paper sticks in a forward fan (2.2–3.0 ahead). Each lies as a mine: bursts r 0.75 for 70% W when an enemy touches it or after 2 s; a burst sets off any stick within 1.2 (chain). Max 9 sticks per wielder. | Three sticks tumble end over end; on the floor fuses fizz; each bursts into a yellow-orange puff with ribbon streamers, rattling "pop-pop-pop" along the chain. | — |
| 3 | DreamRacket | tennis racket (Gon cycle; "young lady with a racket") | projectile | Serves a felt ball that bounces from enemy to enemy, up to 4 hits (range 2.6 between hops), 85% W each, −10% per hop. | Ball tossed up, racket swings; lime felt ball with motion streaks; each hit pops a THWOCK with felt fluff. | — |
| 4 | BambooVase | bamboo in a glass vase (Gon cycle; 6DNyC 14826) | thrown, then a line | Throws the vase at the target; it shatters, then 4 bamboo shoots spring up one after another in a line continuing the throw (0.55 apart, 0.12 s apart), each hitting r 0.45 for 55% W and tossing ordinary enemies up (short stun 0.25 s). | Vase tumbles; glass shards glint; shoots burst up like a pop-up book, leaves unfurling, then sink back. | Root Oracle |
| 5 | RoseBouquet | rose bouquet in white paper (Gon cycle) | thrown, trail | Throws a bouquet that flies 4.5; it sheds a ribbon of petals behind it that lasts 2 s; enemies in the ribbon take 18% W every 0.3 s and are slowed 25%. Impact 60% W. | Bouquet spins, paper wrapping flutters; paper-cut petals drift down along its path; a red bow unties on impact. | Petal Citadel |
| 6 | HomeRunBat | baseball bat (Gon cycle) | melee | Swings at the nearest enemy (range 1.5): 120% W and the ordinary enemy is batted away 3.2 as a spinning projectile that hits everything it passes for 80% W. Bosses only take the hit. | Bat wound back; ball-cap star trail; batted enemy spins with a star streak; HOME RUN lettering on the first knock. | — |
| 7 | CandyDrill | red-white striped drill/lance thrown and piercing (wed3 794; C3vG) | projectile | Throws a drill that pierces up to 4 foes over 5.0, then screws into the floor for 1 s, grinding r 0.6 for 20% W every 0.2 s. | Striped candy drill spinning; candy shavings fly; a spiral crack opens where it bores. | — |
| 8 | InkyOctopus | pink toy octopus (6rPz 197) | summon | Throws a plush octopus that clings to the target for 3 s: 25% W every 0.4 s, and its ink pool (r 0.7) slows others 30%. Then it hops back. One octopus per wielder; re-throw after it returns. | Octopus wobbles through the air; clings with curling arms; ink splotch spreads under it; hops home with a squeak. | Lone Jelly |
| 9 | GrabberClaw | spring grabber claw / pirate hook (VpIh 6487) | projectile | Fires a spring claw (range 4) that grabs the first ordinary enemy, reels it back to 1.0 in front of the wielder over 0.3 s, then flings it sideways 1.6 into others (60% W to each it hits). 100% W to the grabbed one. | Coiled spring stretches; claw snaps shut with a spark; the reeled enemy slides with dust; the fling leaves a whoosh arc. | — |
| 10 | StarforgeClaymore | big golden claymore / sword in the stone (Card King's starter; ranked #1) | melee | Very slow (cooldown 1.6) 200° sweep in front, range 2.2, 190% W. From wind-up to the end of the swing the blade's arc destroys hostile shots it touches (not the wielder's body). | Blade glows along engraved runes during the wind-up; a folded gold-leaf crescent sweeps; blocked shots burst into gold sparks. | Velvet Regent |
| 11 | DragonLanternGlaive | Guan Dao / dragon crescent blade | melee | 3-beat combo: sweep (120°), sweep back, then a forward thrust line 3.0 long (pierces, 140% W). | A paper-lantern dragon head biting a crescent blade; sweeps trail a scaled ribbon; the thrust breathes a lantern-flame streak and drops tiny lantern embers. | — |
| 12 | TwinMoonStaff | double-headed sword | projectile | Throws the two-bladed staff spinning: it flies 3.2, hovers 0.5 s at the end grinding (r 0.6, 30% W every 0.15 s), then returns hitting again on the way back. | Sun-gold and moon-silver blades twist a two-colour helix; the hover throws moth-shaped sparks. | — |
| 13 | PlushShark | giant shark, thrown (6DNyC 7137; 9QK 180) | projectile | Throws a plush shark at the farthest enemy in 5.5: it chomps the first three enemies it passes (110% W each), then swims back along the floor. | A stitched plush shark; its fin cuts the floor into a water ripple; each chomp shows felt teeth and a splash. | — |
| 14 | DrumstickClub | fried-chicken / meat club | melee | Slams the floor in front (1.3 ahead): r 0.9, 130% W, then a short shock line 2.2 forward (3 bumps, 50% W). Every third slam scatters 4 crumb shards. | Giant toy drumstick with a plastic bone; golden batter crumbs spray; a steamy puff on the slam. | Night Maw |
| 15 | PaperShuriken | shuriken (thrown, multi-hit) | projectile | Throws 3 origami shuriken in a fan; each ricochets to 2 more enemies (range 2.4). | Multi-colour folded paper stars; on hit a star opens into a paper flower, refolds and flies on. | — |
| 16 | SquirtTrio | three-way water gun (TV Head's; "very strong") | projectile | Fires 3 water jets in a 20° fan; each bounces off walls twice and soaks (−15% enemy speed 1.5 s). | Clear toy squirt gun with water sloshing inside; jets carry a faint rainbow; splashes star-shaped droplets and a puddle that evaporates. | Rain Archivist |
| 17 | ConfettiBlunderbuss | shotgun | projectile | 7 wrapped-candy pellets in a 30° cone, range 3.2, 26% W each; the wielder recoils 0.2 back. | Toy bell-mouth blunderbuss; muzzle blows a cartoon cloud; pellets are wrapped candies; hits burst confetti; candy wrappers flutter down. | Patchwork Host |
| 18 | PlungerPopper | toilet plunger | projectile | Fires a rubber suction cup on an elastic (range 4.2): it sticks, pulls the enemy 1.0 toward the wielder over 0.4 s, then POPs it off: 110% W and pushes it 0.8 away. | Elastic stretches and wobbles; the cup squelches on; POP! lettering and soap bubbles on release. | — |
| 19 | SignpostSlammer | road sign | melee | Swings a sign (range 1.6, 100% W). Every third hit plants the sign at the target: a 1.6×0.9 "no entry" strip for 3 s where enemies are slowed 40%. | A toy "Dreamland Crossing" sign that flips as it swings; planting draws glowing road stripes and a running arrow. | Citrus Marshal |
| 20 | DetonatorBox | detonator switch | planted | Each attack plants a toy bomb under the target (max 3). The third attack presses the plunger: every bomb bursts r 0.9 for 120% W, in order along the drawn wires. | A wooden plunger box; painted wires run to each bomb; countdown digits hop; bursts are candy-floss mushroom clouds raining stars. | Penny Broker |
| 21 | FirecrackerPinwheel | firecracker drop (Boo Fortress) | planted trap | Plants a paper pinwheel 1.6 ahead toward the target (max 2). When an enemy comes within 1.0 it spins 0.6 s throwing 6 sparks outward (25% W each) and then bursts r 0.8 for 90% W. | Red-gold paper pinwheel; whirs faster and faster spitting sparks; bursts into red paper scraps and BANG lettering. | — |
| 22 | GumballGatling | machine gun (Billy) | projectile | Rapid gumballs (cooldown 0.32, 38% W). Consecutive shots heat the barrel: each shot −4% cooldown, down to −40%; 1.2 s without a target cools it. | A gumball machine with a crank; the glass globe empties as it fires; multicoloured gumballs; the barrel blushes orange when hot. | Thorn Treasurer |
| 23 | BalloonBuddy | balloon (support) | projectile | Releases an animal balloon that floats slowly (2.2/s) toward the densest enemy group within 5; it pops on contact or after 3 s: r 1.0, 60% W, push 0.6, slow 30% 1.5 s. | Twisted balloon animal bobbing on a string; pops with a spinning deflate, falling ribbon and an air ripple. | — |
| 24 | GlitchToybox | "Naming Demon" weapon glitch | special | Each attack borrows a random weapon from a fixed list of 12 simple pool weapons (seeded, deterministic) and performs that weapon's own attack at 90% of its damage. The next pick is shown above the box. | A jack-in-the-box; each swap flickers TV static and a slot-machine roll of tiny weapon icons. | Lady Loaded Dice |

### Research pass 3 (8 more)
Pass 3 read the Naming Demon's complete 32-item cycle frame by frame in three videos (same order every time) and the Potato relic's six orbiting weapons. New items beyond passes 1–2:

| # | Id | From Bizarre Brigade | Kind | Mechanic | Look |
|---|---|---|---|---|---|
| 25 | SaucerBlaster | blue UFO gun firing cyan saucers that trail rings | projectile | A saucer (70% W) pierces up to 4 enemies over 6.0. | Toy saucer with a glowing dome; thin cyan ring wakes and bubbles behind it. |
| 26 | FanfareHorn | golden horn blowing a fan of golden sound fins | projectile fan | 3 sound fins in a 40° fan travel 3.5, piercing; each enemy struck once per blast (45% W). | Brass horn; fins widen as they travel and fade. |
| 27 | WindupWrench | white wrench | melee | Swing 100% W. Every third hit winds the enemy up: 1 s later it spins dizzy (stopped 0.6 s) and knocks its neighbours (r 0.9, 50% W). | Wrench with a wind-up key; the key turns in the enemy's back, dizzy stars. |
| 28 | CorkRifle | wooden hunting rifle | projectile | Slow (1.55 s), long (8.0) cork shot through the whole line, 170% W. | Cork on a string; a POP puff at the muzzle. |
| 29 | CatcherMitt | baseball mitt | projectile | Catches an enemy shot in front (within 1.4) every 0.6 s, holds 3. Each attack: a fastball (130%) then every caught shot thrown back (80%). | THWAP catch burst; stitched baseball fastballs. |
| 30 | FizzBottle | green glass bottle | lob | The bottle bursts into a fizzing puddle (r 0.8, 2.5 s): 15% W every 0.25 s and slow. | Soda bottle tumbling; green puddle bubbling and flattening. |
| 31 | LeekBaton | spring onion / leek | melee | Wide quick swing (150°, 80% W): pushes 0.4 and slows 0.8 s. | Fresh leek streaks. |
| 32 | FlySwatter | red fly swatter | melee | Flat swat (110% W) dazes 0.6 s; every third swat deals double to enemies below 30% health. | Star-mesh swat print and dust. |

Also clarified by pass 3 (already covered): the slingshot (AcornSling), the red round uchiwa fan (PaperFan), the black and gold tridents (ForkTrident/PlasmaTrident), the hunting rifle's shotgun cousin (ConfettiBlunderbuss), the cyan heart-gem staff (staffs), the curved white glaive (MoonScythe), the V boomerang (CrescentBoomerang), the thrown shark (PlushShark). Still unidentified at 40–60 px: a thin wooden stick, a silver spiked fist, a yellow block with a red button (likely the detonator).

### Our own takes on the three unidentified items (3 more, 35 total)
| # | Id | Item in the footage | Kind | Mechanic | Look |
|---|---|---|---|---|---|
| 33 | DowsingRod | the thin wooden stick | projectile | A twig thrown at the weakest enemy in reach (120% W); it bends toward it and exposes it 2 s. | Forked twig with a cyan bead; a dowsing ripple on the mark. |
| 34 | StarKnuckle | the silver spiked fist | melee | Quick jabs (80%); every fourth is a star uppercut (150%) that dazes 0.5 s and throws 3 star shards forward. | Silver knuckle with star studs; punch-star impacts. |
| 35 | BuzzerBlock | the yellow block with a red button | planted | Places a prank buzzer 1.8 ahead (up to 2). The enemy that steps on it and 3 neighbours in a chain are shocked (60%) and stopped 0.3 s. | Yellow buzzer jumping when pressed; a zig-zag toy lightning chain. |

Rejected as duplicates of what we already have: boomerang (CrescentBoomerang), slingshot (AcornSling), thrown fish (TinFish), cleaver (ToastCleaver/KeyCleaver), flail/spiked club (TeabagFlail), fan (PaperFan), fire extinguisher, glow stick (AfterimageSabre), axe (KeyCleaver), machete (TwinSabres), trident, spear, bow, magic staffs, water-bubble wand.

### Research pass 4 (6 more, 41 total)
Pass 4 scanned 40 new videos (30 YouTube, 10 Bilibili; English, Japanese, Korean, Thai, Vietnamese, Spanish, Chinese), 33 of them with shop screens: difficulty-5 runs, endless to wave 99, co-op, two Potato-relic runs, and one Bilibili video (av114227373153171) that ranks **every companion weapon** with burned-in captions, the most useful source of the pass. Companion weapons are random per the wiki, so the companion/weapon pairs seen are observations, not fixed. The Naming Demon did not appear in any new video.

| # | Id | From Bizarre Brigade | Kind | Mechanic | Look |
|---|---|---|---|---|---|
| 36 | PawMitten | beast-claw glove (兽爪): very fast, tiny reach, "weakest weapon" | melee | Very quick short swipes (90% W, cone 0.55, 0.42 s). Every fifth swipe rakes three claw lines 0.5 further (50% W each). | Blue-grey paw mitten with felt pads and blunt claws; cream claw-swipe streaks. |
| 37 | HearthBrick | a brick held by Red Hood and the caveman, thrown | lob | Lobs a brick at the target (0.5 s flight); it thuds down for 130% W (r 0.7), dazes 0.7 s and shoves. Tier 2+: it skips on 1.2 once more for 60% of that. | Terracotta hearth brick with a chalk star, tumbling; low brick-dust puff. |
| 38 | VerdictGavel | the Judge's gavel (裁决官): small area, luck and wage add damage, rare | melee | Bangs down on the enemy in front (r 0.65). +25% per point of luck (up to 2) and up to +50% for coins gathered this hall (200). | Dark wooden gavel; a big golden gavel head stamps down with a cream ring. |
| 39 | TalismanIngot | the Hopping Zombie's charm-sealed gold (蹦蹦僵尸): small blast, 3× a plain shot, bursts with pauses | projectile | Fast throws (0.5 s); each ingot hits, then pops in a small blast (r 0.6) for the same again. After every fourth throw a 1.2 s pause. | Gold ingot with a pasted cream paper charm (teal swirl, no readable text); gold-leaf and paper burst. |
| 40 | KunaiFan | three fanned black kunai with red-wrapped grips (Orange Hood, orange robot) | projectile | The three kunai leave one after another (0.12 s apart) down the same line, 60% W each, each through one enemy. | Black kunai with wine cloth and tassels. |
| 41 | BareKnuckles | no weapon, just fists (猛?王): fast, very low damage, crits explode nearby | melee | Very fast short punches (0.38 s). A critical punch also pops the air just ahead (r 0.8, 120% W). | Cream cloth hand-wraps; a cream-and-teal air-pop ring and jab star. |

Possible but not added: the Little Witch's crystal ball (it may only be the existing purple orb staff's shots), and the Camo Shroom's black-and-red "pruning shears" (most likely the kunai fan). Also clarified by pass 4 (already covered): the air hammer, plunger, signpost (360° sweep), the cyan double-ended spear, Guan Dao, golden trident, thrown shark, claymore, pirate hook, the white and black staffs, bamboo vase, sound-ring gun, hunting rifle, pink octopus, bomb, cannon (splits 1 into 4), detonator, rose bouquet, horn, triple water pistol, striped drill.

## 2. One weapon per playable character (31, all different)

| Hero | Who they are | Was | Now | Why |
|---|---|---|---|---|
| Lantern Knight | keyblade pioneer, lights the way | KeyCleaver | KeyCleaver | already his |
| Postmaster | delivers addressed parcels | MailboxLauncher | MailboxLauncher | already his |
| Drowsy Seer | a dream left waiting behind | BellFork | MoonseedOrb | seeds of sleep that bloom where they land |
| The Root Oracle | rooted, a living garden | RuneStaff | **BambooVase** | a vase that roots a line of bamboo |
| The Lone Jelly | a body of memories and organs | TwinSabres | **InkyOctopus** | a soft creature that clings and inks |
| Lady Loaded Dice | gambler, visible next face | PlasmaTrident | **GlitchToybox** | every throw is a roll of the dice |
| The Tide Singer | sings tides out of three ingredients | BubbleWand | RibbonHarp | the singer's instrument |
| The Comet Courier | runs, the path ignites | ClockCrescent | ClockCrescent | already hers |
| The Mirror Keeper | borrows shots back | SnowGlobeOrb | SnowGlobeOrb | glass that holds a world |
| The Treasure Clerk | bounty ledger, acorns | AcornSling | AcornSling | already his |
| Verdant Matron | a nursery of sprouts | SnowGlobeOrb | LeafCrossbow | leaves from her own garden |
| Copper Cauldron | pressure cooker, steam jets | RubberHammer | FireExtinguisher | a jet of pressure |
| China Marionette | repaired doll, porcelain needles | DreamStamp | NeedleSinger | needle and thread |
| Soap Skater | bubble calligraphy | TinFish | BubbleWand | bubbles |
| Velvet Regent | three commands, the Card King | BellFork | **StarforgeClaymore** | the king's sword in the stone |
| Button Cavalier | courteous riposte | UmbrellaLance | CorkscrewRapier | a fencer's blade |
| Night Maw | a hungry mouth fed by kills | SpoonSabre | **DrumstickClub** | food as a weapon |
| Thorn Treasurer | collateral crown, saved gold | DreamStamp | **GumballGatling** | pays coins in, sprays gumballs out |
| Citrus Marshal | leads a moving convoy | RubberHammer | **SignpostSlammer** | directs traffic, plants a road sign |
| Ivory Sovereign | royal decrees and seals | DreamStamp | DreamStamp | the seal of the decree |
| Petal Citadel | four petals, a garden citadel | SnowGlobeOrb | **RoseBouquet** | petals |
| Patchwork Host | a stage host with a featured friend | InkPistol | **ConfettiBlunderbuss** | showman's confetti |
| Archive Curator | records specimens | SleepCamera | SleepCamera | takes the record |
| Penny Broker | forward orders, delivered later | InkPistol | **DetonatorBox** | plants now, collects later |
| Storm Fencer | thunder sheath, lightning | UmbrellaLance | PlasmaTrident | a lightning trident |
| Tin Virtuoso | conducts the brigade's score | InkPistol | MusicBox | music |
| Moon Prowler | a shadow that flanks | SpoonSabre | MoonScythe | a moon blade from behind |
| Supper Mortar | serves supper in courses | TeapotPopgun | TeapotPopgun | already hers |
| Lantern Weaver | knots a living network | DreamOrbStaff | LanternStaff | a lantern on a staff |
| Hourglass Stag | time debt, collected later | BellFork | AlarmMortar | an alarm that goes off later |
| Rain Archivist | the weather book, frost rain | SleepCamera | **SquirtTrio** | rain in a toy |

Bold = a new weapon; that hero keeps the old weapon until its art lands (`ArsenalBuilder` switches it then). All others switch now.

## 3. Starting companions (one each, all different, weapons all different)

Before: Teapot Pilgrim started for 9 heroes, Jelly Swordsman for 11, Mailbox Imp for 5, always holding SpoonSabre or InkPistol. Now each hero starts with a friend from its own recruit themes (so `K11HeroRules.Eligible` still holds), never a story companion (Hob/Latch/Postscript/Tally), carrying a weapon that **complements** the hero (a melee hero gets a ranged friend and the other way round). No two heroes share a starting friend or its weapon.

| Hero (weapon) | Starting friend | Friend's weapon | Note |
|---|---|---|---|
| Lantern Knight (melee) | Kettle Bat | CrescentBow | archer covers the knight |
| Postmaster (ranged) | Bookmark Golem | BookPaddle | archive guard up close |
| Drowsy Seer (ranged) | Ice Cream Owl | ForkTrident | frost friend in front |
| The Root Oracle (ranged, rooted) | Orbit Moth | DreamIron | moves while she cannot |
| The Lone Jelly (summon) | Jelly Swordsman | TwinSabres | his own kind |
| Lady Loaded Dice (random) | Dice Crab | PaperFan | dice |
| The Tide Singer (ranged) | Anchor Snail | RubberHammer | anchors the shore |
| The Comet Courier (melee) | Comet Kite | ShellCannon | a kite trailing her |
| The Mirror Keeper (ranged) | Mirror Newt | UmbrellaHook | mirror kin |
| The Treasure Clerk (ranged) | Brass Acorn | SpoonSabre | acorn kin |
| Verdant Matron (ranged) | Little Sprout | TapeWhip | her own sprout, a vine whip |
| Copper Cauldron (spray) | Coil Kettle | SpringKnocker | kettle kin |
| China Marionette (ranged) | Ribbon Spider | RibbonWheel | strings of a marionette |
| Soap Skater (ranged) | Sail Squid | UmbrellaLance | tidal skater friend |
| Velvet Regent (melee) | Button Beetle | ButtonBow | royal button archer |
| Button Cavalier (melee) | Tin Tick | InkPistol | covering fire |
| Night Maw (melee) | Mask Pepper | DartCompass | spicy, from range |
| Thorn Treasurer (ranged) | Seed Hedgehog | LollipopLance | thorny hedgehog in front |
| Citrus Marshal (melee) | Citrus Toad | StarKazoo | citrus band |
| Ivory Sovereign (melee) | Crayon Gremlin | RuneStaff | court scribe |
| Petal Citadel (ranged) | Pear Moth | PizzaWheel | garden guard |
| Patchwork Host (ranged) | Sock Cyclops | CloudScissors | patchwork tailor |
| Archive Curator (ranged) | Spindle Wren | ClockworkYoYo | archive guild |
| Penny Broker (planted) | Mailbox Imp | StampHammer | delivery stamp |
| Storm Fencer (ranged) | Spring Duck | AfterimageSabre | a second blade |
| Tin Virtuoso (ranged) | Staple Mole | MaracaHammer | percussion |
| Moon Prowler (melee) | Coral Teacup | MoonFeatherFan | moonlight from range |
| Supper Mortar (ranged) | Bread Crab | ToastCleaver | the bread course |
| Lantern Weaver (ranged) | Violet Remnant | TeabagFlail | a knot on a string |
| Hourglass Stag (ranged) | Teacup Cricket | ParasolNeedle | lantern-guild guard |
| Rain Archivist (ranged) | Ribbon Otter | TinFish | otter and fish |

## 4. Animation pass (everything from the audit)

| # | Item | Status | Done in code | Art requested (image agent) |
|---|---|---|---|---|
| A1 | Characters have few frames (idle 2, walk 4, attack 2+2+2; enemies and roster bosses 4) | done (code), art queued | Frame blending in `UnitVisual.BlendFrames`: when the body changes pose the previous pose lies over it and dissolves in 85 ms, lit like the body; off under Reduced Motion and during a boss entrance. The existing gait bob, transition squash, landing and attack squash stay. | Idle 4 and walk 8 frames for the 31 heroes (§5.2) |
| A2 | Hero identity visuals: thin rings and lines under the hero | done | `HeroIdentityVisual.DrawGauge`: a pip gauge is its row of tokens only (no thin ring under the hero); a continuous gauge keeps a fainter track; the ready glow is smaller and softer. | — |
| A3 | Hostile "Curtain" ring, large, often round the hero | done | `CombatFieldPainting`: during its warning a staged hazard gathers from 75% to full size while its true edge is shown; the edge brightens and beats faster as the blow comes; after the blow it fades to 20% instead of 50%. | H16 (already in `Combat_VFX_Art_Requests.md`) |
| A4 | Shop rarity bloom only 4 frames | done earlier | Linear cross-fade, breath and sway between frames; 12-frame `RarityBloom_v2` imported automatically when it exists. | RarityBloom_v2 (already requested) |
| A5 | Carrier 3 frames; Keeper silhouette placeholder | waiting | The Keeper parts were repainted by the Act I art delivery. Carrier bob/cloth code waits: `StageTransfer.cs` is being edited by another session. | Carrier 6-frame walk |
| A6 | Coins/pickups; ImpactSpark flat disc | done (code), art queued | Coins already spun, bounced and glinted; their rim now darkens edge-on, and painted `CoinSpin` frames replace the squash when they exist. ImpactSpark draws the painted `AllySpark` (our hits) / `HeroHurt` (hero struck) frames instead of a flat disc. | CoinSpin 6 frames |
| A7 | UI cards static; journal page turn; merchant gestures | partly done | Shop inspect cards pop in (92% → overshoot → 100%) and fade up in 0.22 s. Merchant gestures wait: `ShopLayout.cs` is being edited by another session. | Merchant gesture poses (3) |
| A8 | Death pop, boss rising from darkness, weapon cue moments | done earlier | — | — |

## 5. Art requests
Machine-readable list: `Docs/Bizarre_Arsenal_Art_Worklist.json` (same format as `Art_Master_Worklist.json`; generated by `Tools/Art/bizarre_arsenal_art.py`).

**Effects are painted ink in code, not image requests (17/10/2026).** Like the hero mechanics, every effect around the weapons (bursts, clouds, swipes, the gavel's verdict, pools, cords, spirals, wakes, dizzy stars, zaps) is drawn by `Runtime/Presentation/ArsenalInk.cs`: inked cloud silhouettes with a shaded underside and lit top, organic splashes, broken inked rims, brush-stroke sweeps and MechanicInk tokens. A thrown copy of the weapon itself (brick, ingot, bottle, twin-moon staff, the gavel coming down) reuses its held painting. The painted `HomeRunText` lettering rides over the ink burst when present. The image list therefore asks only for held weapons and painted objects: `Docs/Bizarre_Arsenal_Art_Worklist.json` (114 jobs: 41 held, 42 object frames, 25 rarity-frame parts, 6 coin frames). The 372 hero idle/walk frames moved to `Docs/Bizarre_Hero_Animation_Worklist.json` (optional; nothing imports them yet). Review sheets: `-arcane-arsenal-capture -capturePath DIR [-arsenal-live-only] [-arsenal-weapons Id,Id]`.

The tables below list the original requests; pieces that are effects are now ink. The current object list per weapon is `ArsenalSpecs.Pieces`.

### 5.1 Weapons (folder `Assets/ArcaneBrigade/Art/Arsenal/`)
Per weapon: `<Id>_Held_v1.png` — the weapon alone, lying horizontally, **pointing right (+X), grip at the left ~20%**, 1024×512 canvas, transparent. Plus the pieces below (all transparent, one object per file; frames `_f0.._fN` are edits of the approved `_f0`; directional shots point right):

| Id | Extra pieces |
|---|---|
| CherryFuseBomb | `CherryBomb_f0..f2` (fuse burning), `CottonPop_f0..f3` (burst) |
| PopperSticks | `PopperStick_f0..f1` (fuse), `PopperBurst_f0..f2` |
| DreamRacket | `FeltBall` |
| BambooVase | `BambooVaseShot` (tumbling vase), `BambooShoot_f0..f2` (springing up), `GlassShards` |
| RoseBouquet | `BouquetShot`, `PaperPetal_f0..f2` (three petal shapes) |
| HomeRunBat | `HomeRunText` |
| CandyDrill | `CandyDrillShot`, `SpiralCrack` |
| InkyOctopus | `PlushOctopus_f0..f3` (fly, cling ×2, hop), `InkPool` |
| GrabberClaw | `ClawHead_f0..f1` (open/shut), `ClawSpring` (tileable coil, horizontal) |
| StarforgeClaymore | `GoldLeafArc` (crescent sweep) |
| DragonLanternGlaive | `DragonRibbon` (tileable scaled ribbon), `LanternEmber` |
| TwinMoonStaff | `TwinMoonSpin_f0..f3` |
| PlushShark | `PlushSharkShot_f0..f1`, `FloorRipple` |
| DrumstickClub | `BatterCrumbs`, `SteamPuff_f0..f2` |
| PaperShuriken | `ShurikenShot_f0..f1` (spin), `PaperFlower` |
| SquirtTrio | `WaterJet` (shot), `StarDroplets`, `Puddle` |
| ConfettiBlunderbuss | `CandyPellet`, `CartoonCloud_f0..f2`, `ConfettiBurst` |
| PlungerPopper | `PlungerCup`, `ElasticBand` (tileable), `PopText` |
| SignpostSlammer | `PlantedSign_f0..f1`, `RoadStripes` (tileable) |
| DetonatorBox | `ToyBomb`, `DetonatorWire` (tileable), `CandyFlossCloud_f0..f3` |
| FirecrackerPinwheel | `Pinwheel_f0..f3` (spinning), `PaperScraps`, `BangText` |
| GumballGatling | `Gumball_f0..f3` (four colours) |
| BalloonBuddy | `BalloonAnimal_f0..f1` (bob), `BalloonPop_f0..f2` |
| GlitchToybox | `JackBox_f0..f2` (lid shut / popping / open), `TvStatic` |
| PawMitten | `PawSwipe` |
| HearthBrick | `BrickShot_f0..f1` (tumble), `BrickDust_f0..f2` |
| VerdictGavel | `GavelStamp_f0..f2` (raised / striking / lifting) |
| TalismanIngot | `IngotShot_f0..f1` (spin), `IngotBlast_f0..f2` |
| KunaiFan | `KunaiShot` |
| BareKnuckles | `KnucklePop_f0..f2`, `FistJab` |

### 5.2 Animation art
- `Art/Acting/<HeroId>_Idle_f0..f3_v1.png`, `<HeroId>_Walk_f0..f7_v1.png` for the 31 heroes (edit of the hero's existing pose; same scale, feet on the same line).
- `Art/Arsenal/CoinSpin_f0..f5_v1.png`; `Art/Acting/Carrier_Walk_f0..f5_v1.png`; `Art/ShopPresentation/Merchant_Gesture_f0..f2_v1.png`.
- H16 (curtain hazard) and RarityBloom_v2 are already requested in their own documents.

## 6. Implementation notes
- `WeaponDefinition.Arsenal` (new enum `ArsenalWeapon`) selects the mechanic; resolved before the Carnival/Expanded paths in `ResolveExpandedAttack`.
- Delayed and planted pieces (bombs, sticks, pinwheels, signs, shoots, octopus, balloon) are `ArsenalProp` records in the simulation, cleared at rest, deterministic, capped per wielder.
- Presentation: `ArsenalVisual` draws props and special shots from `Art/Arsenal` sprites; hit cues reuse the painted `ArcadeEffects` moments with weapon motes.
- Gate: a weapon is added to `RunCatalog.Weapons` (and to a hero/starter) only when `<Id>_Held_v1.png` and its §5.1 pieces exist. `ArsenalChecks` runs the mechanics in fixtures without art.
