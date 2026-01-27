# K22: ordinary enemy art and evolution

## Scope

All twenty ordinary enemy species now have a normal and a recognizably related mutated form: forty forms, 320 authored body drawings, plus sixteen dedicated body-fragment projectile drawings. Existing ten-boss K21 art and movesets are retained. These are frame-based idle/blink, walk, anticipation, release and recoil sets, not skeletal rigs or continuous hand-drawn animation.

Art is original generated work using the project's own monster reference. The sources are not native 4K. Import uses separate ground-aligned sprites and normalized per-pose display height; existing contact shadows remain. Prompts and provenance are in `Docs/K22_Enemy_Art_Prompts.md`.

## Later-wave identities

| Species | First mutated wave | Added body attack |
| --- | ---: | --- |
| HushMite | 12 | Split-eye pounce to a locked destination and landing pulse |
| ClockBat | 9 | Torn-wing sidestep with three curved shots |
| BristleImp | 10 | Antler rush and narrow physical lash |
| RuneWisp | 11 | Six slow curved spiral shots |
| InkMoth | 12 | Molts two short-lived ink pools |
| SirenSeal | 12 | Two staggered expanding impact waves |
| DoorBearer | 10 | Splintered door-body forward slam |
| MoonMagnet | 11 | Pulls loose coins and spits four weaving metal fragments |
| SleepMimic | 11 | Seam-ripper lunge and landing impact |
| CurtainWisp | 6 | Three staggered falling strips of living cloth |
| FurnaceToad | 12 | Five slow accelerating furnace globs |
| WardrobeCrab | 7 | Two crossed cabinet-jaw lashes |
| HopperKeg | 11 | Two actual jumps with separate landing pulses |
| ChargerRam | 7 | Backstep feint followed by a locked rush |
| CoinLeech | 10 | Steals loose coins to fuel a fragment fan; stolen coins return on death |
| BlastPuffer | 6 | Large expanding pulse followed by a smaller echo |
| SporeBishop | 12 | Three spaced temporary spore pools |
| BroomSkater | 12 | Three narrow hooked-comb rakes |
| RibbonHydra | 11 | Five weaving strands from its evolved body |
| BurrowBell | 8 | Burrows to a locked destination, then erupts |

The deterministic species schedule spreads introductions across waves 6–12, not the already sensitive wave 3-to-4 transition. Form selection happens when compiling fresh per-run enemy clones, not by changing shared catalog assets and not as an animated mid-life transformation. Existing base attacks remain between mutation casts.

Evolution itself does not multiply HP, damage or speed: existing wave scaling remains unchanged. It adds anatomy, a moveset, 12% visual body height and 8% body radius. No shop/economy changes or permanent progression were added in this batch.

## Readability and combat safety

- A body anticipation of 0.85 seconds precedes mutation release. At most four mutation windups can begin concurrently. Mutation cooldowns are at least 7.5 seconds.
- Leap/charge destinations are locked rather than continuously tracking the player. Two-leg moves show a separate travel interval. Burrowing bodies do not deal underground contact damage.
- New hazards use existing timed lash, falling-object, pool and impact-wave primitives, with the existing 48-field budget. This is not twenty separate custom VFX systems. Projectile count retains its existing 4096 cap; these are bounds, not measured frame-rate guarantees.
- Dedicated enemy fragments replace borrowed item/weapon art in these new volleys and non-boss thrown-body effects. Their painted silhouettes are not covered by generic halo/core discs. Enemies do not display held weapons.
- No target rings or pre-impact floor outlines were added. Body anticipation and actual visible attack geometry convey the attack.
- Pause freezes the simulation; shop/rest clears pending hazards. Wave one remains contact-only. Base attack, contact and mutation timing are kept separate to avoid simultaneous unrelated casts.

## Validation entry points

- Unity: `ArcaneBrigade.Editor.K22EnemyEvolutionBuilder.Build` imports the atlases and creates the library/motion assets.
- `VerifyAndBuild` runs K22 focused checks, K21 checks and existing focused regressions, then builds `Builds/FullRun/ArcaneBrigade.exe`.
- Native Windows: add `-arcane-k22-evolution` to the K18 experience capture flags documented in K21. The diagnostic fixture uses isolated save storage, pairs both forms of each species, tests anticipation/release, and captures their actual Windows presentation at 3840x2160. Diagnostic preview body height is enlarged to at least 1.1 world units to inspect the drawings; this does not change gameplay catalog height.
- `Campaign` runs the existing automated campaign diagnostics with the new forms enabled. Bot reports are observations, not human difficulty approval.

Exact results and artifact hashes belong in `PreviewExports/K22/Gate_Summary.md`. Human play/balance/art acceptance, device testing and performance profiling remain separate gates. Audio sources from main are retained; no subjective audio re-approval is claimed.

The ten single-seed campaign routes (seed 171077) produced four victories and six defeats, reaching waves 11–20. New-recruit quotas stayed within the existing three-per-wave ceiling. A fixed movement/purchase policy is not a representative player population; these results do not establish a win-rate target or justify a further automatic difficulty buff.
