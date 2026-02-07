# Combat VFX art requests — enemy projectiles, allied weapon effects, explosions
Date: 2026-10-17. Audit + image prompts. No code was changed for this document; all `file:line` references are to the working tree on this date (several of these files are being edited in parallel, so lines can drift by a few).
## How to read this
- **Scale.** Combat camera `orthographicSize` = 3.9 (Resources/RunCatalog.asset `CombatViewHeight`), so 1 world unit ≈ 138.5 px at 1080p, ≈ 277 px at 4K. A typical shot (0.22 u) is about 30 px wide on a 1080p screen. Every canvas below is 4–8× that, so the art downsamples cleanly.
- **Direction.** Every directional projectile is drawn pointing **right (+X)**. `ArenaController.cs:662` rotates projectile sprites to `atan2(velocity)`, so +X art needs no extra offset. Round/spinning ones say so.
- **Colour rule.** Hostile = ember red / soot black / burgundy. Friendly = brass, amber, cream, deep teal. This matches `ProjectileVisual.cs:94` ("one danger hue for every hostile shot") and `UI/ArcadeSkin.cs:41`.
- **One image per file.** The image agent outputs one isolated object per prompt, so atlases are not requested; animation frames are separate files (`_f0`, `_f1`…) made by editing the approved f0, exactly as the shared prompt's last paragraph says. Each frame line in a request is the **Subject** for that edit; use the same full prompt block with that Subject.
- **Folder & import.** New folder `Assets/ArcaneBrigade/Art/CombatVFX/{Hostile,Allied,Juice}/`, file names `Name[_fN]_v1.png` (same `_v1` convention as `Art/MechanicEffects/Tokens/`). Import like `Editor/HeroMechanicArtImporter.cs`: Sprite (Single), PPU 256, pivot as listed, alpha is transparency, mipmaps on, clamp, max size 1024 (2048 for 1536-wide canvases), CompressedHQ.
- **Priority.** P1 = do first (hostile readability or the most common allied weapons), P2 = next, P3 = nice to have.

## 1. Audit — enemy projectiles and hostile effects (17 distinct visuals)
How enemy shots are drawn today: every enemy projectile goes through `Core/ArenaController.cs:646-668` (sprite or the white `Disc.png` fallback, scaled, rotated to velocity) and `Presentation/ProjectileVisual.cs:48-132` (hostile = sorting 2400, body tinted 65 % toward pink 1/.24/.42 unless it is a BodyFragments/EnemyAffixEffects sprite, a pink halo disc at 2.15×, a cream core, a 4-point pink streak). Hostile fields go through `Presentation/ExpandedVisual.cs` (LineRenderers + a soft mesh disc). Enemy shot motions (`HostileShot`: Straight, Weave, Hook, Drift, Accelerate — `Core/HostileBallistics.cs`) only change the path; no visual differs by motion.
| # | Visual | Current art | Spawned at | What it looks like / problem | Verdict |
|---|---|---|---|---|---|
| E1 | Generic enemy bolt | Art/Checkpoint/Disc.png (64 px white disc) via ArenaController.cs:650 | Core/CombatSimulation.cs:816 (EmitProjectile; ClockBat, FurnaceToad, RuneWisp, SirenSeal, RibbonHydra via CarnivalEnemies.cs:55, Secret Paper Bird, checkpoint JellyArbalist/HoodWisp) | 0.22 u ≈ 30 px. Pink-tinted dot (ProjectileVisual.cs:68) + pink halo 2.15× + cream core + pink streak. Procedural, not painted; reads only as 'pink glow dot'. | REDRAW → H1 (+ species H2–H6) |
| E2 | Boss volley | Disc.png (no sprite set) | Core/CombatSimulation.cs:674 | 0.34 u ≈ 47 px. Same glowing dot, colour 1/.38/.3. No boss identity. | REDRAW → H7 |
| E3 | October encore volley | ModularArcade_Atlas.png#Projectile (flat teal teardrop) | Core/OctoberBossCombat.cs:81 | 0.2 u ≈ 28 px. Flat-vector teardrop tinted pink; wrong style family. | REDRAW → H7 (shared) |
| E4 | Mutation volleys | Art/K22/BodyFragments_v1.png (16) | Core/EnemyMutationCombat.cs:82 (via EnemyEvolutionLibrary.Fragment) | 0.27/0.38 u ≈ 37/52 px. Painted, hostile (red eyes) but drawn diagonally with no travel direction; rotated by velocity they tumble randomly; gritty dark-fantasy texture is a notch away from the storybook style; 20 % pink tint. | REDRAW → H8 (keep sheet for FallingProp) |
| E5 | Affix / special volleys (blue & red flame) | Art/K25/EnemyAffixEffects_v1.png rows 1–2 | Core/EnemyVariantCombat.cs:157 | 0.29 u ≈ 40 px. Upright flames rotated by velocity → fly sideways; frame chosen once at spawn, never animates; blue set reads as a friendly frost bolt. | REDRAW → H9 |
| E6 | Secret bosses firing allied weapons | CarnivalProjectiles_v1 #01 needle / #02 crescent, disc | Core/SecretEventCombat.cs:131 | Allied art tinted 65 % pink — looks like a friendly shot gone wrong. | REDRAW → H10a–c (fallback H1) |
| E7 | Enemy muzzle flash | PaintedEffects_v1.png Magic (teal/cream starburst) | Presentation/PaintedEffects.cs:21,48 | 0.48 u. Allied teal colours on enemy fire — no faction check. | REDRAW → H11 |
| E8 | Hero-hurt impact | ImpactFamilies_v1.png Orb / Pierce | Presentation/PaintedEffects.cs:38 | Purple flower / gold streak when the hero is hit; reads as a friendly proc. | REDRAW → H12 |
| E9 | Tentacle hazard | LineRenderer, no sprite | Presentation/ExpandedVisual.cs:90-101,123 | Flat red-pink band with sine wobble. | REDRAW → H13 |
| E10 | Shockwave hazard | LineRenderer circle, width 0.44 | Presentation/ExpandedVisual.cs:77,113 | Flat red-pink circle line. | REDRAW → H14 |
| E11 | Shadow pool hazard | Mesh disc, 14 % alpha wash | Presentation/ExpandedVisual.cs:127-148 | Barely visible pink haze on the dark floors. | REDRAW → H15 |
| E12 | Curtain / Office / Toy hazards | nothing | Core/ExpandedEnemies.cs:33,47,54,59-60 | INVISIBLE: damage with no visual (ExpandedVisual.cs:123,127 skip them). | REDRAW (new) → H16–H18 |
| E13 | Falling prop | BodyFragments / boss organ `Charged` / CarnivalStageAssets stands | Presentation/ExpandedVisual.cs:43-66 | Painted objects on a lob arc with a shadow, warm-red tint. Works. | KEEP |
| E14 | Painted affix field wisps | EnemyAffixEffects_v1 (all 4 rows) | Presentation/EnemyVariantFieldVisual.cs:25 | Upright painted flames on the floor for affixed enemies' fields. Correct orientation, painted. | KEEP |
| E15 | Secret-boss mirror Strike | Forms_v1 painted crescent (friendly) | Core/SecretEventCombat.cs:133 → ExpandedVisual.cs:158-169 | Strike is not in CarnivalHazards.IsHostile, so a boss's attack is drawn with the friendly painted form at 78 % alpha. | IMPROVE IN CODE: treat Strike from an enemy source as hostile (use H14/H13 or the hostile muzzle) |
| E16 | Enemy magnet coin pull (Link) | Forms_v1 painted form (friendly) | Core/ExpandedEnemies.cs:24 → ExpandedVisual.cs:158 | Non-damaging, but an enemy field drawn in friendly colours. | IMPROVE IN CODE: skip or tint dark for enemy sources |
| E17 | Hostile warnings before impact | nothing (BossTelegraph.cs emptied in 84f7837b) | Presentation/ExpandedVisual.cs:123,127 | Hazards appear only when they already hurt. Probably deliberate (boss body/anatomy windups carry the tell). | OPTIONAL → H19 |

## 2. Audit — allied weapons and effects
Allied projectiles use `WeaponDefinition.ProjectileSprite`; when that is empty or `Art/Checkpoint/Disc.png`, the shot is a tinted white dot with a halo (`ProjectileVisual.cs:76-91`). That is the case for **16 of the 30 allied projectile weapons** — the single biggest allied gap. Melee/impact/field art is mostly painted and good.
| Weapon / effect | Current art | Judgement | Verdict |
|---|---|---|---|
| Acorn Sling | Disc (cyan dot, no halo: Frost) | No identity at all; plain dot. | REDRAW → A4 |
| Bell Fork | BowMagic_v2#RuneOrb (flat vector) | Off-style flat gem; a bell weapon firing a gem. | REDRAW → A3 |
| Bubble Wand | Disc (magenta + halo) | Glow dot. | REDRAW → A5 |
| Button Bow | Disc (gold) | Glow dot. | REDRAW → A6 |
| Clockwork Yo-Yo | CarnivalProjectiles_v1#00 yo-yo | Painted, inked, readable. | KEEP |
| Crescent Boomerang (Orbit Moth) | K10/CrescentBoomerang.png (the weapon itself) | Painted, spins (Fish family) so pivot 0.18 doesn't matter much. | KEEP |
| Crescent Bow | BowMagic_v2#ArrowBolt (flat vector, coral-red head) | Off-style; red head near hostile colour. | REDRAW → A1 |
| Dart Compass | K10/DartCompass.png (whole held wand) | Throws the handle too; pivot 0.18 offsets it from the hitbox. | REDRAW → A17 (low) |
| Dream Orb Staff, Moonseed Orb | DreamProjectiles_v1#0 violet orb | Painted, inked, reads well; bloom motif fine. | KEEP |
| Dream Projector | CarnivalProjectiles_v1#02 crescent | Generic crescent with red stripe; the beam field is painted. | REDRAW → A22 (low) |
| Fire Extinguisher | DreamProjectiles_v1#2 bubble cluster | Painted, fine. | KEEP |
| Ink Pistol | Disc (cyan) | Glow dot on a very common weapon (5 companions). | REDRAW → A7 |
| Lantern Staff | Disc (Ember orange-red + halo) | Reads like an enemy shot. | REDRAW → A8 |
| Leaf Crossbow | Disc (Ember orange-red) | Glow dot, hostile-ish colour. | REDRAW → A9 |
| Mailbox Launcher | none → disc (white) | Glow dot. | REDRAW → A10 |
| Moon Feather Fan | DreamProjectiles_v1#3 feather | Painted, directional, good. | KEEP |
| Music Box | Disc (gold) + LineRenderer note glyph | Thin line glyph unreadable at size. | REDRAW → A11 |
| Needle Singer | CarnivalProjectiles_v1#01 needle | Painted, directional. | KEEP |
| Plasma Trident | DreamProjectiles_v1#1 teal comet | Painted, directional. | KEEP |
| Ribbon Harp | Disc (magenta) + note glyph | As Music Box. | REDRAW → A11 |
| Rune Staff | BowMagic_v2#RuneOrb | Flat vector, off-style. | REDRAW → A2 |
| Satellite Disc | K10/CrescentBoomerang.png | Painted; spins. Shares the boomerang art. | KEEP |
| Scrap Magnet | CarnivalProjectiles_v1#02 crescent | Unrelated shape; field visual is painted. | REDRAW → A21 (low) |
| Shell Cannon | Disc (Ember orange-red) | Glow dot, hostile-ish. | REDRAW → A12 |
| Snow Globe Orb | Disc (cyan, no halo) | Plain dot (4 companions). | REDRAW → A13 |
| Star Kazoo | Disc (gold) | Glow dot. | REDRAW → A14 |
| Teapot Popgun | none → disc | Glow dot; Tea field itself is painted. | REDRAW → A15 |
| Tin Fish | Disc (mint) | Plain dot; fish motif is dead code. | REDRAW → A16 |
| Clock Crescent (melee that throws) | ToyBlades_v2#1 crescent blade | Painted, spins. | KEEP |
| Companion response / Curio Retort | ModularArcade_Atlas#Projectile (flat teal teardrop) | Flat vector, off-style. | REDRAW → A18 |
| Melee swing (Slash ×4) | AnimationHD/PaintedEffects_v1 row 2 | Soft golden crescent without ink contour — a little 'anime FX' but warm, readable at 190–280 px. | KEEP |
| Thrust weapons (rapier, lances, trident, parasol) | same Slash crescent | A lunge drawn as a sideways swing. | IMPROVE IN CODE now (use Pierce), REDRAW → A19 |
| Scissors (Cloud Scissors, Twin Sabres) | Slash + ArcadeEffects mesh double slash | Readable cross-snip. | KEEP |
| Slam / hammer fire (Dust ×4) | PaintedEffects_v1 row 1 | Cream dust, painted. | KEEP |
| Projectile muzzle (Magic ×4) | PaintedEffects_v1 row 3 | Teal/cream starburst — good for allies (needs faction check, see H11). | KEEP |
| Enemy death puff (Death ×4) | PaintedEffects_v1 row 4 | Navy ink + ember sparks, good. | KEEP |
| Hit impacts Cut / Slam / Pierce / Orb | K15/ImpactFamilies_v1 (16) | Painted, family-coded, readable at 83–140 px. | KEEP |
| Build-morph forms (Tea, Iron, Photo, Strike, Echo, Fuse, Link…) | MechanicEffects/Forms_v1 (16) | Rich painted circles; best-in-class effect art in the project. | KEEP |
| Tether trails (yo-yo, sewing, link) | Forms_v1 #13 golden spear squashed | Reads as a gold band, not a thread. | IMPROVE IN CODE / optional A20 |
| Signature cues (Umbrella, Bell, Stamp) | Forms_v1 via PaintedEffects cue branch | Painted. | KEEP |
| Alarm Mortar detonation | ExplosionFeedback procedural | Being reworked. | → Juice J1–J5 |
| ArcadeEffects mesh shapes | procedural mesh fallback | Only plays when PaintedEffects declines; fine as fallback. | KEEP |
| ProjectileVisual halo / core / streak | Disc | Will double-glow over new painted projectiles. | IMPROVE IN CODE (authored test, line 77) |
| Hero-identity missiles & tokens (31 heroes) | MechanicInk procedural tokens | Covered by Docs/Hero_Mechanic_Art_Requests.md. | KEEP here (see that doc) |

Totals: 44 allied rows judged — **19 KEEP, 3 IMPROVE IN CODE (2 of them with an optional redraw), 21 REDRAW**, 1 handed to the juice section.

### Code-only improvements found during the audit (no art needed)
- `Presentation/PaintedEffects.cs:21,48` — the projectile muzzle flash has no faction check, so enemies fire with the friendly teal starburst. Skip it for `Faction.Enemy` until H11 exists.
- `Presentation/PaintedEffects.cs:38` — hits on the hero use the attacker's impact family (Orb = purple flower for contact). Use Cut or Slam tinted dark until H12 exists.
- `Presentation/PaintedEffects.cs:45` — Thrust-motion melee uses the swing crescent. Use `impacts.Pierce` for `WeaponMotion.Thrust` until A19 exists.
- `Presentation/ExpandedVisual.cs:123,127` — Curtain, OfficeHazard and ToyHazard draw nothing. Interim: let them use the ShadowPool fill (one-line change) until H16–H18 exist.
- `Presentation/ExpandedVisual.cs:158` — enemy-sourced Strike/Link fields are drawn with friendly painted forms (`CarnivalHazards.IsHostile` excludes them). Check `field.Source.Faction == Faction.Enemy`.
- `Presentation/ProjectileVisual.cs:110` — `motif.enabled = family == MusicBox` makes the Fish motif branch dead; Tin Fish is a bare mint dot.
- `Presentation/ProjectileVisual.cs:68,77-78` — when painted hostile art lands, the 65 % pink tint and the pink halo must be turned off for it (extend `bodyFragment`/`authored` to the CombatVFX textures), or the new art will be washed pink.
- `Core/ArenaController.cs:658` — enemy projectiles are first sized to 0.2 u, then `ProjectileVisual.cs:81-82` re-scales to `VisualWidth` using `sprite.bounds` (includes the 10–12 % transparent padding). Register new sprites in `SpriteInkMetrics` (as PaintedMechanicView does) or trim padding on import so 0.22 u means 0.22 u of ink.
- Allied Ember element colour `ArcadeEffects.ElementColor` = 1/.34/.12 (orange-red) tints Lantern Staff, Leaf Crossbow, Shell Cannon shots almost like enemy shots. Consider an amber 1/.62/.22 for allied projectile tint only.
- `K10/DartCompass.png`, `K10/CrescentBoomerang.png` as projectiles have pivot x = 0.18 (held-weapon pivot), so the drawn shot sits ahead of its hitbox; give projectile use a centred sprite.

## 3. Request table — hostile (redraw every enemy projectile)
| ID | Pri | Asset | Target file(s) | Canvas | Frames / layout | Pivot / direction | On screen | Replaces (code) |
|---|---|---|---|---|---|---|---|---|
| H1 | P1 | Generic hostile bolt (every enemy ranged shot without its own art) | `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/HostileBolt_f0_v1.png, _f1_v1.png` | 256×256 | 2 frames, separate files (flicker loop, 12 fps) | centre (0.5, 0.5); tip points +X | 0.22 u ≈ 30 px @1080p / 61 px @4K | Art/Checkpoint/Disc.png tinted pink when an enemy projectile has no sprite — Core/ArenaController.cs:650 (disc fallback), drawn by Presentation/ProjectileVisual.cs:66-97 (pink tint, halo, core, pink streak). Users: ClockBat, FurnaceToad, RuneWisp, SirenSeal (Data/Run/*Attack.asset, ProjectileSprite empty), RibbonHydra volley (Core/CarnivalEnemies.cs:55), Secret Paper Bird (TeapotPopgun has no sprite), checkpoint JellyArbalist/HoodWisp. |
| H2 | P2 | ClockBat — clock-hand dart (Weave) | `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/ShotH2_ClockBat_v1.png` | 256×256 | 1 frame | centre; tip +X | 0.22 u ≈ 30 px @1080p / 61 px @4K | The generic disc (same path as H1) for ClockBat (Data/Run/ClockBatAttack.asset). |
| H3 | P2 | FurnaceToad — spat cinder glob (Accelerate) | `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/ShotH3_FurnaceToad_v1.png` | 256×256 | 1 frame | centre; tip +X | 0.22 u ≈ 30 px @1080p / 61 px @4K | The generic disc (same path as H1) for FurnaceToad (shares ClockBatAttack.asset — needs its own WeaponDefinition or a per-unit override). |
| H4 | P2 | RuneWisp — cursed rune shard (Hook) | `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/ShotH4_RuneWisp_v1.png` | 256×256 | 1 frame | centre; tip +X | 0.22 u ≈ 30 px @1080p / 61 px @4K | The generic disc (same path as H1) for RuneWisp (Data/Run/RuneWispAttack.asset). |
| H5 | P2 | SirenSeal — sour siren note (Drift) | `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/ShotH5_SirenSeal_v1.png` | 256×256 | 1 frame | centre; tip +X | 0.22 u ≈ 30 px @1080p / 61 px @4K | The generic disc (same path as H1) for SirenSeal (Data/Run/SirenSealAttack.asset). |
| H6 | P2 | RibbonHydra — barbed ribbon dart | `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/ShotH6_RibbonHydra_v1.png` | 256×256 | 1 frame | centre; tip +X | 0.22 u ≈ 30 px @1080p / 61 px @4K | The generic disc (same path as H1) for RibbonHydra volley (Core/CarnivalEnemies.cs:55, RibbonHydraAttack.asset). |
| H7 | P1 | Boss volley orb (all boss bullet patterns + October encore volley) | `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/BossOrb_f0_v1.png, _f1_v1.png` | 384×384 | 2 frames (pulse loop, 10 fps) | centre; flames trail −X | 0.34 u ≈ 47 px @1080p / 94 px @4K (October encore: 0.2 u) | White disc for boss volleys — Core/CombatSimulation.cs:674 (no Sprite, VisualWidth 0.34, colour 1/.38/.3) and the flat teal teardrop `catalog.Projectile` (Art/Checkpoint/ModularArcade_Atlas.png#Projectile) used by Core/OctoberBossCombat.cs:81. |
| H8 | P1 | Mutation volley shots — 8 families × 2 frames (replaces BodyFragments in flight) | `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/Mutation_{family 0-7}_{a|b}_v1.png (16 files)` | 256×256 each | 8 families × 2 frames (a = base, b = tumble/flicker edit) | centre; point +X (family 5 is round and spins) | 0.27 u ≈ 37 px @1080p / 75 px @4K (FurnaceFan 0.38 u) | Art/K22/BodyFragments_v1.png (16 diagonal fragments, no travel direction, gritty texture, tinted 20 % pink) via EnemyEvolutionLibrary.Fragment — Core/EnemyMutationCombat.cs:82. Keep BodyFragments for FallingProp (Presentation/ExpandedVisual.cs:47-48), where tumbling diagonal pieces are correct. |
| H9 | P1 | Affix flame bolts — blue-flame and red-flame (4 frames each) | `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/BlueFlameBolt_f0..f3_v1.png, RedFlameBolt_f0..f3_v1.png` | 256×256 each | 2 sets × 4 frames (flicker loop, 10 fps) | centre; head +X | 0.29 u ≈ 40 px @1080p / 80 px @4K | Art/K25/EnemyAffixEffects_v1.png rows 1–2 (upright flames) used as bullets — Core/EnemyVariantCombat.cs:157. Upright flames get rotated by velocity (Core/ArenaController.cs:662), so they fly sideways; the blue set reads as a friendly frost bolt. Keep the old sheet for the floor wisps in Presentation/EnemyVariantFieldVisual.cs:25 (upright is right there). |
| H10a | P3 | Secret Dream Lamb — scorched film shard | `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/Secret_Dream_v1.png` | 256×256 | 1 frame | centre; tip +X | 0.3 u ≈ 42 px @1080p / 83 px @4K | Art/K14/CarnivalProjectiles_v1.png#02 (teal/red crescent) tinted 65 % pink — enemy boss SecretDreamLamb (DreamProjector weapon) fires an allied weapon through EmitProjectile (Core/SecretEventCombat.cs:131 → CombatSimulation.cs:816). |
| H10b | P3 | Secret Paper Bird — scalding tea bolt | `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/Secret_Paper_v1.png` | 256×256 | 1 frame | centre; tip +X | 0.3 u ≈ 42 px @1080p / 83 px @4K | white disc (no sprite) tinted pink — enemy boss SecretPaperBird (TeapotPopgun weapon) fires an allied weapon through EmitProjectile (Core/SecretEventCombat.cs:131 → CombatSimulation.cs:816). |
| H10c | P3 | Secret Patch Bear — rusted cursed needle | `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/Secret_Patch_v1.png` | 256×256 | 1 frame | centre; tip +X | 0.3 u ≈ 42 px @1080p / 83 px @4K | Art/K14/CarnivalProjectiles_v1.png#01 (silver needle) tinted pink — enemy boss SecretPatchBear (NeedleSinger weapon) fires an allied weapon through EmitProjectile (Core/SecretEventCombat.cs:131 → CombatSimulation.cs:816). |
| H11 | P1 | Hostile muzzle spit (enemy fire flash) | `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/HostileMuzzle_f0..f3_v1.png` | 256×256 | 4 frames, plays once in 0.18 s | (0.2, 0.5) — burst opens to +X | 0.48 u ≈ 66 px @1080p / 133 px @4K | Allied teal/cream starburst `PaintedAnimationLibrary.Magic` (Art/AnimationHD/PaintedEffects_v1.png row 3) that also plays for every enemy projectile Fire event — Presentation/PaintedEffects.cs:21 and :48 (no faction check; the Fire event at Core/CombatSimulation.cs:775 carries the enemy weapon). |
| H12 | P1 | Hero-hurt impact (enemy hit lands on the hero) | `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/HeroHurt_f0..f3_v1.png` | 384×256 | 4 frames, once in 0.19 s | centre | 0.6 u ≈ 83 px @1080p / 166 px @4K (crit 1.0 u) | `ImpactAnimationLibrary.For(enemyWeapon)` — Presentation/PaintedEffects.cs:38. Enemy contact weapons fall through to `Orb` (purple petals + mint core, Art/K15/ImpactFamilies_v1.png row 4) and enemy shots to `Pierce` (gold streak), so the hero getting hurt looks like a friendly flower blooming. |
| H13 | P1 | Tentacle lash (Tentacle hazard) | `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/Tentacle_f0_v1.png, _f1_v1.png` | 1536×256 | 2 frames (writhe, 8 fps) | (0.0, 0.5) — root at left edge, tip +X | stretched along the lash: 2–6 u long × 0.36–1.2 u wide | A plain red-pink LineRenderer (width = 2×radius, sine wobble) — Presentation/ExpandedVisual.cs:90-101, enabled at :123. Used by bosses, LastCustomer, OutsideStage, mutations (TendrilSweep etc.) and affix RedCharge. |
| H14 | P1 | Hostile shock ring (Shockwave hazard) | `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/ShockRing_v1.png` | 1024×1024 | 1 frame (code scales from 0 to radius and fades) | centre | diameter 0.6–12 u (radius grows to 0.3–6 u) | A red-pink LineRenderer circle, width 0.44 — Presentation/ExpandedVisual.cs:77 and :113 (radius from CarnivalHazards.ShockRadius). |
| H15 | P1 | Shadow pool (ShadowPool hazard) | `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/ShadowPool_f0..f2_v1.png` | 1024×640 | 3 frames (bubble loop, 6 fps) | centre | 2× radius: 1.1–2.5 u wide | A soft red-pink mesh disc at 14 % alpha — Presentation/ExpandedVisual.cs:127-148 (nearly invisible on the dark floors). |
| H16 | P1 | Curtain hazard (currently INVISIBLE) | `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/Curtain_f0_v1.png, _f1_v1.png` | 512×512 | 2 frames (f0 incoming/warning, f1 active) | (0.5, 0.15) — base on the floor | ≈ 2× radius: 2–2.4 u wide | Nothing. Used by CurtainWisp behaviour (Core/ExpandedEnemies.cs:33) and Stage-motif bosses (:59-60). CarnivalHazards.IsHostile includes it, but Presentation/ExpandedVisual.cs:123 only enables the line for Tentacle/Shockwave and :127 the fill for ShadowPool, so this hazard deals damage with no visual at all (except when the source carries an affix, :150). |
| H17 | P1 | OfficeHazard hazard (currently INVISIBLE) | `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/OfficeHazard_f0_v1.png, _f1_v1.png` | 512×512 | 2 frames (f0 incoming/warning, f1 active) | (0.5, 0.15) — base on the floor | ≈ 2× radius: 2–2.4 u wide | Nothing. Used by Office-motif bosses at half health (Core/ExpandedEnemies.cs:47). CarnivalHazards.IsHostile includes it, but Presentation/ExpandedVisual.cs:123 only enables the line for Tentacle/Shockwave and :127 the fill for ShadowPool, so this hazard deals damage with no visual at all (except when the source carries an affix, :150). |
| H18 | P1 | ToyHazard hazard (currently INVISIBLE) | `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/ToyHazard_f0_v1.png, _f1_v1.png` | 512×512 | 2 frames (f0 incoming/warning, f1 active) | (0.5, 0.15) — base on the floor | ≈ 2× radius: 2–2.4 u wide | Nothing. Used by Toys-motif bosses at half health (Core/ExpandedEnemies.cs:54). CarnivalHazards.IsHostile includes it, but Presentation/ExpandedVisual.cs:123 only enables the line for Tentacle/Shockwave and :127 the fill for ShadowPool, so this hazard deals damage with no visual at all (except when the source carries an affix, :150). |
| H19 | P3 | (Optional) floor danger telegraphs: circle rim + lane arrow | `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/TelegraphCircle_v1.png, TelegraphLane_v1.png` | 1024×1024 (circle), 1024×256 (lane) | 1 frame each | centre; lane arrow +X | field radius / lane length | Nothing since commit 84f7837b emptied Presentation/BossTelegraph.cs; hostile fields draw nothing during their warning window (ExpandedVisual.cs:123,127). Only request this if the owner wants floor warnings back — the removal looks deliberate. |

## 4. Request table — allied
| ID | Pri | Asset | Target file(s) | Canvas | Frames / layout | Pivot / direction | On screen | Replaces (code) |
|---|---|---|---|---|---|---|---|---|
| A1 | P1 | Crescent arrow (Crescent Bow) | `Assets/ArcaneBrigade/Art/CombatVFX/Allied/ArrowBolt_v2.png` | 512×192 | 1 frame | centre; tip +X | 0.5 u ≈ 69 px @1080p / 138 px @4K | Art/Presentation/BowMagic_v2.png#ArrowBolt — flat vector style with a coral-red head (off-style and close to hostile red). Data/Run/CrescentBow.asset. |
| A2 | P1 | Rune orb (Rune Staff) | `Assets/ArcaneBrigade/Art/CombatVFX/Allied/RuneOrb_v2.png` | 256×256 | 1 frame | centre; spins | 0.3 u ≈ 42 px @1080p / 83 px @4K | Art/Presentation/BowMagic_v2.png#RuneOrb — flat vector purple gem, off-style. Data/Run/RuneStaff.asset (also BellFork today). |
| A3 | P2 | Ringing bell chime (Bell Fork) | `Assets/ArcaneBrigade/Art/CombatVFX/Allied/BellChime_v1.png` | 256×256 | 1 frame | centre; tip +X | 0.3 u ≈ 42 px @1080p / 83 px @4K | RuneOrb (same as A2) — Data/Run/BellFork.asset; a bell weapon firing a gem. |
| A4 | P1 | Frost acorn pellet (Acorn Sling) | `Assets/ArcaneBrigade/Art/CombatVFX/Allied/AcornPellet_v1.png` | 256×256 | 1 frame | centre; tip +X | 0.22 u ≈ 30 px @1080p / 61 px @4K | Art/Checkpoint/Disc.png (64 px white dot) tinted by ProjectileColor, plus ProjectileVisual halo/core/streak (cyan). Data/Run/AcornSling.asset. |
| A5 | P1 | Iridescent bubble (Bubble Wand) | `Assets/ArcaneBrigade/Art/CombatVFX/Allied/Bubble_v1.png` | 256×256 | 1 frame | centre; spins | 0.22 u ≈ 30 px @1080p / 61 px @4K | Art/Checkpoint/Disc.png (64 px white dot) tinted by ProjectileColor, plus ProjectileVisual halo/core/streak (magenta + halo). Data/Run/BubbleWand.asset. |
| A6 | P1 | Button dart (Button Bow) | `Assets/ArcaneBrigade/Art/CombatVFX/Allied/ButtonDart_v1.png` | 256×256 | 1 frame | centre; tip +X | 0.22 u ≈ 30 px @1080p / 61 px @4K | Art/Checkpoint/Disc.png (64 px white dot) tinted by ProjectileColor, plus ProjectileVisual halo/core/streak (gold). Data/Run/ButtonBow.asset. |
| A7 | P1 | Teal ink bullet (Ink Pistol) | `Assets/ArcaneBrigade/Art/CombatVFX/Allied/InkBullet_v1.png` | 256×256 | 1 frame | centre; tip +X | 0.22 u ≈ 30 px @1080p / 61 px @4K | Art/Checkpoint/Disc.png (64 px white dot) tinted by ProjectileColor, plus ProjectileVisual halo/core/streak (cyan). Data/Run/InkPistol.asset (5 companions — very common). |
| A8 | P1 | Lantern-moth ember (Lantern Staff) | `Assets/ArcaneBrigade/Art/CombatVFX/Allied/LanternEmber_v1.png` | 256×256 | 1 frame | centre; tip +X | 0.22 u ≈ 30 px @1080p / 61 px @4K | Art/Checkpoint/Disc.png (64 px white dot) tinted by ProjectileColor, plus ProjectileVisual halo/core/streak (Ember orange-red 1/.34/.12 + halo — reads like an enemy shot). Data/Run/LanternStaff.asset. |
| A9 | P2 | Amber leaf bolt (Leaf Crossbow) | `Assets/ArcaneBrigade/Art/CombatVFX/Allied/LeafBolt_v1.png` | 256×256 | 1 frame | centre; tip +X | 0.22 u ≈ 30 px @1080p / 61 px @4K | Art/Checkpoint/Disc.png (64 px white dot) tinted by ProjectileColor, plus ProjectileVisual halo/core/streak (Ember orange-red). Data/Run/LeafCrossbow.asset. |
| A10 | P2 | Flying letter (Mailbox Launcher) | `Assets/ArcaneBrigade/Art/CombatVFX/Allied/Letter_v1.png` | 256×256 | 1 frame | centre; tip +X | 0.22 u ≈ 30 px @1080p / 61 px @4K | No ProjectileSprite → white disc. Data/Run/MailboxLauncher.asset. (Could share with the hero token Tokens/Envelope_v1.png from Docs/Hero_Mechanic_Art_Requests.md.) |
| A11 | P1 | Brass music note (Music Box) + ribbon note (Ribbon Harp) | `Assets/ArcaneBrigade/Art/CombatVFX/Allied/MusicNote_v1.png, RibbonNote_v1.png` | 256×256 | 1 frame | centre; tip +X | 0.22 u ≈ 30 px @1080p / 61 px @4K | Art/Checkpoint/Disc.png (64 px white dot) tinted by ProjectileColor, plus ProjectileVisual halo/core/streak (gold / magenta) plus a thin LineRenderer note glyph (Presentation/ProjectileVisual.cs:110-118). Data/Run/MusicBox.asset, RibbonHarp.asset. |
| A12 | P2 | Conch shell shot (Shell Cannon) | `Assets/ArcaneBrigade/Art/CombatVFX/Allied/ShellShot_v1.png` | 256×256 | 1 frame | centre; tip +X | 0.34 u ≈ 47 px @1080p / 94 px @4K | Art/Checkpoint/Disc.png (64 px white dot) tinted by ProjectileColor, plus ProjectileVisual halo/core/streak (Ember orange-red, ×3 per volley). Data/Run/ShellCannon.asset. |
| A13 | P2 | Snow-globe shard (Snow Globe Orb) | `Assets/ArcaneBrigade/Art/CombatVFX/Allied/GlobeShard_v1.png` | 256×256 | 1 frame | centre; tip +X | 0.22 u ≈ 30 px @1080p / 61 px @4K | Art/Checkpoint/Disc.png (64 px white dot) tinted by ProjectileColor, plus ProjectileVisual halo/core/streak (cyan, no halo — plain dot). Data/Run/SnowGlobeOrb.asset (4 companions). |
| A14 | P2 | Brass star (Star Kazoo) | `Assets/ArcaneBrigade/Art/CombatVFX/Allied/Star_v1.png` | 256×256 | 1 frame | centre; spins | 0.22 u ≈ 30 px @1080p / 61 px @4K | Art/Checkpoint/Disc.png (64 px white dot) tinted by ProjectileColor, plus ProjectileVisual halo/core/streak (gold). Data/Run/StarKazoo.asset. |
| A15 | P2 | Tea droplet with sugar cube (Teapot Popgun) | `Assets/ArcaneBrigade/Art/CombatVFX/Allied/TeaDrop_v1.png` | 256×256 | 1 frame | centre; tip +X | 0.22 u ≈ 30 px @1080p / 61 px @4K | No ProjectileSprite → disc tinted 1/.7/.4 + halo. Data/Run/TeapotPopgun.asset (3 companions; also splits on impact). |
| A16 | P2 | Tin sardine (Tin Fish) | `Assets/ArcaneBrigade/Art/CombatVFX/Allied/TinFish_v1.png` | 256×256 | 1 frame | centre; spins | 0.22 u ≈ 30 px @1080p / 61 px @4K | Art/Checkpoint/Disc.png (64 px white dot) tinted by ProjectileColor, plus ProjectileVisual halo/core/streak (mint). Fish family spins it (ProjectileVisual.cs:73-74) and turns off the halo (:79), so it is a plain mint dot. The fish line-motif is dead code (motif only enabled for MusicBox, :110). Data/Run/TinFish.asset. |
| A17 | P3 | Compass dart (Dart Compass) | `Assets/ArcaneBrigade/Art/CombatVFX/Allied/CompassDart_v1.png` | 256×256 | 1 frame | centre; tip +X | 0.22 u ≈ 30 px @1080p / 61 px @4K | Art/K10/DartCompass.png — the whole held weapon (handle included) is thrown, pivot 0.18 so it draws off-centre. Data/Run/DartCompass.asset. |
| A18 | P2 | Generic friendly spark (companion response, curio retort) | `Assets/ArcaneBrigade/Art/CombatVFX/Allied/AllySpark_v1.png` | 256×256 | 1 frame | centre; tip +X | 0.2 u ≈ 28 px @1080p / 55 px @4K | `catalog.Projectile` = Art/Checkpoint/ModularArcade_Atlas.png#Projectile (flat vector teal teardrop) — Core/OctoberCombat.cs:116, Core/RelicCombat.cs:137. |
| A19 | P1 | Thrust lunge streak (rapiers, lances, tridents) | `Assets/ArcaneBrigade/Art/CombatVFX/Allied/Thrust_f0..f3_v1.png` | 768×256 | 4 frames, once in 0.2 s | (0.1, 0.5) — starts at the weapon, extends +X | 2 u ≈ 277 px @1080p / 554 px @4K | PaintedAnimationLibrary.Slash crescent (Art/AnimationHD/PaintedEffects_v1.png row 2) — Presentation/PaintedEffects.cs:45 uses the swing crescent for every non-slam melee, including Thrust weapons CorkscrewRapier, ForkTrident, LollipopLance, ParasolNeedle, UmbrellaLance. |
| A20 | P3 | Teal thread strand (yo-yo tether, sewing link) | `Assets/ArcaneBrigade/Art/CombatVFX/Allied/Thread_v1.png` | 1024×96 | 1 frame | centre | 3 u ≈ 416 px @1080p / 831 px @4K | MechanicEffectArtwork.Thrust (golden spear, Art/MechanicEffects/Forms_v1.png #13) squashed into a band — Presentation/PaintedMechanicView.cs:42. |
| A21 | P3 | Horseshoe magnet bolt (Scrap Magnet) | `Assets/ArcaneBrigade/Art/CombatVFX/Allied/MagnetBolt_v1.png` | 256×256 | 1 frame | centre; tip +X | 0.16 u ≈ 22 px @1080p / 44 px @4K | Art/K14/CarnivalProjectiles_v1.png#02 — the teal/red crescent shared by four unrelated weapons; its red stripe flirts with the hostile colour. Data/Run/ScrapMagnet.asset. |
| A22 | P3 | Glowing film frame (Dream Projector) | `Assets/ArcaneBrigade/Art/CombatVFX/Allied/FilmFrame_v1.png` | 256×256 | 1 frame | centre; tip +X | 0.16 u ≈ 22 px @1080p / 44 px @4K | Same crescent as A21. Data/Run/DreamProjector.asset. |

## 5. Juice — explosion / impact frames
`Presentation/ExplosionFeedback.cs` (new, untracked) builds four procedural sprites at runtime — glow, ring, chip, smudge (lines 35-39) — and plays them on Detonation (fuse/echo/hero area blasts), BossLanded, boss deaths and crit/overkill kills. These requests give it painted frames in the same style; sizes follow its radii (0.55–4.4 u).

| ID | Pri | Asset | Target file(s) | Canvas | Frames / layout | Pivot / direction | On screen | Replaces (code) |
|---|---|---|---|---|---|---|---|---|
| J1 | P1 | Blast flash — friendly / neutral | `Assets/ArcaneBrigade/Art/CombatVFX/Juice/BlastFlash_f0..f3_v1.png` | 1024×1024 | 4 frames, once in ~0.12 s | centre | diameter 2×radius: 1.6–8.8 u | Procedural radial glow `ExplosionGlow` — Presentation/ExplosionFeedback.cs (new, untracked, being written by another job):35. |
| J2 | P1 | Blast flash — hostile | `Assets/ArcaneBrigade/Art/CombatVFX/Juice/HostileBlast_f0..f3_v1.png` | 1024×1024 | 4 frames, once | centre | same as J1 | Same glow tinted 0.92/0.3/0.22 for Faction.Enemy — Presentation/ExplosionFeedback.cs (new, untracked, being written by another job):92. |
| J3 | P1 | Shock ring | `Assets/ArcaneBrigade/Art/CombatVFX/Juice/BlastRing_v1.png` | 1024×1024 | 1 frame (code expands and fades) | centre | diameter up to 2.3×radius | Procedural `ExplosionRing` band + ink rim — Presentation/ExplosionFeedback.cs (new, untracked, being written by another job):37 (Ring and Rim renderers). |
| J4 | P2 | Debris chips (6 variants) | `Assets/ArcaneBrigade/Art/CombatVFX/Juice/Chip_{0-5}_v1.png` | 128×128 each | 6 single sprites | centre | 0.12–0.4 u | Procedural blob `ExplosionChip` coloured ink/tint/cream — Presentation/ExplosionFeedback.cs (new, untracked, being written by another job):38. |
| J5 | P2 | Ink smudge puff | `Assets/ArcaneBrigade/Art/CombatVFX/Juice/InkPuff_f0..f3_v1.png` | 512×512 | 4 frames over the puff lifetime | centre | 0.5–3 u | Procedural wobble disc `ExplosionSmudge` tinted dark plum — Presentation/ExplosionFeedback.cs (new, untracked, being written by another job):39. |

## 6. Prompts — hostile

### H1 — Generic hostile bolt (every enemy ranged shot without its own art) (P1)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/HostileBolt_f0_v1.png, _f1_v1.png` · 256×256 · 2 frames, separate files (flicker loop, 12 fps) · pivot centre (0.5, 0.5); tip points +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a small nightmare ember bolt: a teardrop-shaped glob of burning ember with its round head on the right, wrapped in a cracked soot-black inked shell; a hot cream-yellow seam glows through the cracks, one tiny narrowed slit eye sits on the leading bulge so it feels alive and spiteful, and three short ragged soot wisps trail to the left. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

Frames / variants (same prompt, Subject replaced by the line):

- f1: same bolt, the three trailing soot wisps flicker into a different ragged shape and the inner seam is slightly brighter; head position and size unchanged.

_Integration:_ Add `Sprite[] HostileBolt` to the new CombatVfxArtwork asset. In Core/CombatSimulation.cs EmitProjectile (~:816) set `Sprite = weapon.ProjectileSprite ?? (unit.Faction==Faction.Enemy ? CombatVfxArtwork.Hostile(HostileShotKind.Generic) : null)`; animate by swapping frames in ProjectileVisual.Display (12 fps, offset by state.Id). In ProjectileVisual mark the CombatVFX textures as `authored` (line 77) so halo/core are off, and for authored hostile art use tint lerp 0 instead of 0.65 (line 68). Keep the streak but make it soot-to-ember (line 95).

### H2 — ClockBat — clock-hand dart (Weave) (P2)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/ShotH2_ClockBat_v1.png` · 256×256 · 1 frame · pivot centre; tip +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a bent rusty black-iron clock minute-hand flung like a dart, its ornate arrow tip pointing right, glowing ember-red cracks running along the hand, and a tiny torn scrap of bat-wing membrane fluttering at its tail end. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ Assign as `ProjectileSprite` on the enemy's WeaponDefinition asset (no code needed), keep `ProjectileVisualWidth` 0.22–0.26. Needs the same ProjectileVisual changes as H1 (authored texture → no halo, no 65 % pink tint).

### H3 — FurnaceToad — spat cinder glob (Accelerate) (P2)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/ShotH3_FurnaceToad_v1.png` · 256×256 · 1 frame · pivot centre; tip +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a spat cinder glob: a lumpy ball of molten red slag with black crust plates, hot orange core showing through the cracks, its rounder heavy end on the right and two molten droplets dripping back to the left. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ Assign as `ProjectileSprite` on the enemy's WeaponDefinition asset (no code needed), keep `ProjectileVisualWidth` 0.22–0.26. Needs the same ProjectileVisual changes as H1 (authored texture → no halo, no 65 % pink tint).

### H4 — RuneWisp — cursed rune shard (Hook) (P2)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/ShotH4_RuneWisp_v1.png` · 256×256 · 1 frame · pivot centre; tip +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a cursed rune shard: a jagged splinter of dark slate pointed to the right, one ember-red sigil carved into it and glowing, and a curl of crimson smoke hooking away behind it to the left. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ Assign as `ProjectileSprite` on the enemy's WeaponDefinition asset (no code needed), keep `ProjectileVisualWidth` 0.22–0.26. Needs the same ProjectileVisual changes as H1 (authored texture → no halo, no 65 % pink tint).

### H5 — SirenSeal — sour siren note (Drift) (P2)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/ShotH5_SirenSeal_v1.png` · 256×256 · 1 frame · pivot centre; tip +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a sour siren note: a crooked black-ink eighth note lying on its side with its glowing ember-red note-head on the right and its flag streaming left, two small ember-red sound-wave arcs trailing behind it. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ Assign as `ProjectileSprite` on the enemy's WeaponDefinition asset (no code needed), keep `ProjectileVisualWidth` 0.22–0.26. Needs the same ProjectileVisual changes as H1 (authored texture → no halo, no 65 % pink tint).

### H6 — RibbonHydra — barbed ribbon dart (P2)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/ShotH6_RibbonHydra_v1.png` · 256×256 · 1 frame · pivot centre; tip +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a barbed ribbon dart: a short strip of torn crimson velvet ribbon whose front end twists tightly into a black-iron barb pointing right, frayed ribbon tail fluttering to the left, ember glow along the twist. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ Assign as `ProjectileSprite` on the enemy's WeaponDefinition asset (no code needed), keep `ProjectileVisualWidth` 0.22–0.26. Needs the same ProjectileVisual changes as H1 (authored texture → no halo, no 65 % pink tint).

### H7 — Boss volley orb (all boss bullet patterns + October encore volley) (P1)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/BossOrb_f0_v1.png, _f1_v1.png` · 384×384 · 2 frames (pulse loop, 10 fps) · pivot centre; flames trail −X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a heavy nightmare orb: a round ball of churning burgundy-black smoke with a molten ember-red rim and a small hot cream core, an antique ornamental ring of blackened brass around its equator like a cursed bauble, and short soot flames streaming back to the left. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

Frames / variants (same prompt, Subject replaced by the line):

- f1: same orb, core swollen about 10 % brighter, trailing soot flames in a different flicker shape; outline and ring unchanged.

_Integration:_ Set `Sprite = CombatVfxArtwork.BossOrb[0]` in the boss volley initializer (CombatSimulation.cs:674) and replace `catalog.Projectile` in OctoberBossCombat.cs:81; frame swap in ProjectileVisual. Mark authored (no halo/pink tint).

### H8 — Mutation volley shots — 8 families × 2 frames (replaces BodyFragments in flight) (P1)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/Mutation_{family 0-7}_{a|b}_v1.png (16 files)` · 256×256 each · 8 families × 2 frames (a = base, b = tumble/flicker edit) · pivot centre; point +X (family 5 is round and spins)

**Family 0 — Soot spirit pellet** (`Mutation_0_a_v1.png`; used by SootPounce, RuneSpiral (and Standard behaviour))

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a soot spirit pellet: a tight swirl of black soot with two ember-red eyes glaring to the right and smoky wisps streaming to the left. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

- `Mutation_0_b_v1.png` (edit of a): same object tumbled about 20° and its trailing wisps/sparks in a different flicker; size and silhouette centre unchanged.

**Family 1 — Molten rock chunk** (`Mutation_1_a_v1.png`; used by FurnaceFan, KegRebound, Hopper behaviour)

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a molten rock chunk: a faceted cracked crimson stone with glowing orange seams, its heavier point leading to the right and a short flame tail behind it. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

- `Mutation_1_b_v1.png` (edit of a): same object tumbled about 20° and its trailing wisps/sparks in a different flicker; size and silhouette centre unchanged.

**Family 2 — Torn curtain ribbon** (`Mutation_2_a_v1.png`; used by MothMolting, CurtainCollapse, HydraWeave)

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a torn crimson curtain-ribbon with a bone-white stitched seam, twisting like a tadpole with its knotted head on the right and frayed tail to the left. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

- `Mutation_2_b_v1.png` (edit of a): same object tumbled about 20° and its trailing wisps/sparks in a different flicker; size and silhouette centre unchanged.

**Family 3 — Spore pod** (`Mutation_3_a_v1.png`; used by SporeGarden, PufferEcho, Spore behaviour)

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a spore pod: a bone-white puffball shell full of holes with one glowing red eye at its front (right), dark burgundy spores puffing out behind it. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

- `Mutation_3_b_v1.png` (edit of a): same object tumbled about 20° and its trailing wisps/sparks in a different flicker; size and silhouette centre unchanged.

**Family 4 — Splinter shard** (`Mutation_4_a_v1.png`; used by DoorSlam, CabinetScissor, CombRake, Sweeper behaviour)

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a splintered shard of ivory bone and dark wood, sharp ivory point on the right, splintered dark wood at the back, ember glow in the cracks. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

- `Mutation_4_b_v1.png` (edit of a): same object tumbled about 20° and its trailing wisps/sparks in a different flicker; size and silhouette centre unchanged.

**Family 5 — Cursed coin-gear** (`Mutation_5_a_v1.png`; used by MagnetSalvo, CoinBarrage)

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a cursed coin-gear: a small tarnished, blackened bronze cog with an ember-red slit eye in its hub and three sparks flicking off its teeth. It is a projectile seen from the game's elevated top-down camera; it spins in flight, so it is roughly round and reads the same at any rotation; one bold simple shape that still reads at 30 pixels wide. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

- `Mutation_5_b_v1.png` (edit of a): same object tumbled about 20° and its trailing wisps/sparks in a different flicker; size and silhouette centre unchanged.

**Family 6 — Cracked bell** (`Mutation_6_a_v1.png`; used by BellEruption, SirenEcho, Burrower behaviour)

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a small cracked black-bronze bell flung mouth-first to the right, ember-red light leaking from its crack and mouth, clapper swinging out behind. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

- `Mutation_6_b_v1.png` (edit of a): same object tumbled about 20° and its trailing wisps/sparks in a different flicker; size and silhouette centre unchanged.

**Family 7 — Ivory claw** (`Mutation_7_a_v1.png`; used by every other mutation)

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a curved ivory claw like a hooked horn, sharp point to the right, dark crimson root at the back with a wisp of soot. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

- `Mutation_7_b_v1.png` (edit of a): same object tumbled about 20° and its trailing wisps/sparks in a different flicker; size and silhouette centre unchanged.


_Integration:_ Add `Sprite[] MutationShots` (16, index = family*2+variant, same order as EnemyEvolutionLibrary.Fragment's family switch in Data/EnemyEvolutionLibrary.cs:52-66). Add `EnemyEvolutionLibrary.Shot(unit, variant)` that returns the new sprite and use it only in MutationVolley (EnemyMutationCombat.cs:82). Swap a/b at ~8 fps in ProjectileVisual. Family 5 (coin-gear): add it to the spinning list so it rotates instead of facing velocity.

### H9 — Affix flame bolts — blue-flame and red-flame (4 frames each) (P1)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/BlueFlameBolt_f0..f3_v1.png, RedFlameBolt_f0..f3_v1.png` · 256×256 each · 2 sets × 4 frames (flicker loop, 10 fps) · pivot centre; head +X

**Blue set** (`BlueFlameBolt_f0_v1.png`)

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a cold spirit-flame bolt shaped like a comet: a white-blue flame heart at the head on the right, but wrapped in a thick ember-red outer rim and a soot-black ink contour so it still reads as an enemy shot; tongues of blue-and-crimson flame stream back to the left. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```
- `BlueFlameBolt_f1_v1.png` — flame tongues shift upward-left into a new flicker, head unchanged.
- `BlueFlameBolt_f2_v1.png` — tongues stretch longest, one ember breaks off at the tail.
- `BlueFlameBolt_f3_v1.png` — tongues pull short and curl downward-left, head slightly brighter.

**Red set** (`RedFlameBolt_f0_v1.png`)

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a red-flame bolt shaped like a comet: a crimson-orange fireball with a cream-hot head on the right, soot-black licks of flame and two stray embers trailing to the left. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```
- `RedFlameBolt_f1_v1.png` — flame tongues shift upward-left into a new flicker, head unchanged.
- `RedFlameBolt_f2_v1.png` — tongues stretch longest, one ember breaks off at the tail.
- `RedFlameBolt_f3_v1.png` — tongues pull short and curl downward-left, head slightly brighter.


_Integration:_ Add `Sprite[] FlameBolts` (8: blue f0-3, red f0-3). In VariantVolley (EnemyVariantCombat.cs:157) pick the set by `blue`; animate in ProjectileVisual (the old code baked a frame at spawn time via index*0.11 and never advanced it).

### H10a — Secret Dream Lamb — scorched film shard (P3)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/Secret_Dream_v1.png` · 256×256 · 1 frame · pivot centre; tip +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a scorched film-strip shard: a curled scrap of black celluloid with sprocket holes, its burning ember-red edge leading on the right and grey-black char curling off to the left. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ In EmitProjectile, when `unit.Faction == Faction.Enemy && weapon.ProjectileSprite` belongs to an allied atlas, substitute the hostile sprite (a small lookup by weapon Id in CombatVfxArtwork). Generic fallback: H1.

### H10b — Secret Paper Bird — scalding tea bolt (P3)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/Secret_Paper_v1.png` · 256×256 · 1 frame · pivot centre; tip +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a scalding tea bolt: a glob of near-black boiling tea with a red-hot rim and a broken porcelain chip riding at its front on the right, two angry steam curls trailing left. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ In EmitProjectile, when `unit.Faction == Faction.Enemy && weapon.ProjectileSprite` belongs to an allied atlas, substitute the hostile sprite (a small lookup by weapon Id in CombatVfxArtwork). Generic fallback: H1.

### H10c — Secret Patch Bear — rusted cursed needle (P3)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/Secret_Patch_v1.png` · 256×256 · 1 frame · pivot centre; tip +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a rusted cursed needle: a long black-iron sewing needle pointing right with an ember glow at its tip, a short frayed crimson thread whipping behind it. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ In EmitProjectile, when `unit.Faction == Faction.Enemy && weapon.ProjectileSprite` belongs to an allied atlas, substitute the hostile sprite (a small lookup by weapon Id in CombatVfxArtwork). Generic fallback: H1.

### H11 — Hostile muzzle spit (enemy fire flash) (P1)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/HostileMuzzle_f0..f3_v1.png` · 256×256 · 4 frames, plays once in 0.18 s · pivot (0.2, 0.5) — burst opens to +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a hostile spit-burst: a small splash of ember-red fire and soot-black ink flaring open toward the right as if something was just spat out, three ragged ink droplets flying ahead of it. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny. The burst's origin is at the left; it opens to the right.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

Frames / variants (same prompt, Subject replaced by the line):

- f0: tight closed bud of ember and ink at the left.
- f1: fully open splash (the base image).
- f2: splash breaks into four ragged droplets drifting right.
- f3: only a few small soot specks and one ember remain, fading.

_Integration:_ Add `Sprite[] HostileMuzzle`. In PaintedEffects.Play: `else if (magic && value.Faction == Faction.Enemy) frames = art.HostileMuzzle;` before the allied magic branch (line 48), rotate by value.Direction like slashes (line 76).

### H12 — Hero-hurt impact (enemy hit lands on the hero) (P1)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/HeroHurt_f0..f3_v1.png` · 384×256 · 4 frames, once in 0.19 s · pivot centre

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: an ink-and-ember hit splash: a burst of soot-black ink splatter with jagged ember-red spark shards radiating out from a small cream-hot centre, one cracked shard flying off at the upper right. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

Frames / variants (same prompt, Subject replaced by the line):

- f0: small hot centre with the first ink spray.
- f1: full splash (base image).
- f2: splatter breaks into separate droplets and shards moving outward.
- f3: a few soot droplets and one ember shard fading.

_Integration:_ In PaintedEffects.Play hit branch: `frames = value.Faction == Faction.Champion ? art.HeroHurt : impacts.For(value.Weapon);` (line 38).

### H13 — Tentacle lash (Tentacle hazard) (P1)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/Tentacle_f0_v1.png, _f1_v1.png` · 1536×256 · 2 frames (writhe, 8 fps) · pivot (0.0, 0.5) — root at left edge, tip +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: one long nightmare lash-tentacle lying straight from left to right: its thick root on the left, tapering to a hooked tip on the right; burgundy-black velvet skin with a soot ink contour, a row of glowing ember-red suckers along its lower edge, small thorn barbs along its top edge. Seen from the game's elevated top-down camera, long axis horizontal, about 6:1. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

Frames / variants (same prompt, Subject replaced by the line):

- f1: same tentacle with a gentle S-curve writhe (middle bowed up a little, tip curled down), root and tip ends at the same x positions.

_Integration:_ Draw with a SpriteRenderer per field instead of the LineRenderer: position at field.Start, rotate to (field.Position−field.Start), scale x = length / ink width, y = 2×radius / ink height (same placement math as Presentation/PaintedMechanicView.cs Place()). Keep the LineRenderer as fallback when the art is missing.

### H14 — Hostile shock ring (Shockwave hazard) (P1)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/ShockRing_v1.png` · 1024×1024 · 1 frame (code scales from 0 to radius and fades) · pivot centre

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a hostile shock ring: a closed ring of torn ember-red brush strokes with a soot-black ink outer contour, small cracked floor chips and sparks kicked up along it, completely empty transparent centre; the ring band is about 8 % of the diameter. It lies flat on the arena floor and is seen from the game's elevated camera, so its outline is a wide ellipse about twice as wide as tall; it is the object itself, not a shadow or a floor. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ One SpriteRenderer per shockwave: scale = 2×ShockRadius / ink width (x) and ×0.5 on y to sit on the floor ellipse used elsewhere; alpha from the existing `color.a`. Register in SpriteInkMetrics so the ring edge equals the real damage radius.

### H15 — Shadow pool (ShadowPool hazard) (P1)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/ShadowPool_f0..f2_v1.png` · 1024×640 · 3 frames (bubble loop, 6 fps) · pivot centre

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a pool of living nightmare ink: glossy soot-black ink with deep burgundy depths, a glowing ember-red rim around its edge, two small bubbles and one grasping hand-shaped ripple rising from the surface. It lies flat on the arena floor and is seen from the game's elevated camera, so its outline is a wide ellipse about twice as wide as tall; it is the object itself, not a shadow or a floor. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

Frames / variants (same prompt, Subject replaced by the line):

- f1: bubbles moved and the hand-ripple sunk halfway; rim unchanged.
- f2: one bubble popping with a small ink splash, a new bubble starting on the other side.

_Integration:_ Replace the fill mesh for ShadowPool with a SpriteRenderer at field.Position, width = 2×radius (ink bounds), sorting just above the floor (current fill order 61).

### H16 — Curtain hazard (currently INVISIBLE) (P1)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/Curtain_f0_v1.png, _f1_v1.png` · 512×512 · 2 frames (f0 incoming/warning, f1 active) · pivot (0.5, 0.15) — base on the floor

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a heavy crimson-black theatre curtain swag tumbling down, a tarnished brass-black tassel rope wrapped around it, its frayed bottom hem glowing ember-red. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

Frames / variants (same prompt, Subject replaced by the line):

- f1 (landed): the same curtain slumped into a crumpled heap with the hem splayed out, ember glow along the folds.

_Integration:_ In ExpandedVisual, for this kind show a SpriteRenderer at field.Position: during warning (time < field.At) drop f0 from above (reuse the FallingProp arc, ExpandedVisual.cs:61-66) with its GroundShadow; at field.At switch to f1 for the active 0.3 s, then fade. Width = 2×field.Radius.

### H17 — OfficeHazard hazard (currently INVISIBLE) (P1)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/OfficeHazard_f0_v1.png, _f1_v1.png` · 512×512 · 2 frames (f0 incoming/warning, f1 active) · pivot (0.5, 0.15) — base on the floor

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a toppling tower of black ledgers and an iron filing drawer bound with red tape, loose papers with scorched red edges flying off the top. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

Frames / variants (same prompt, Subject replaced by the line):

- f1 (landed): the ledgers slammed flat in a splayed pile, papers scattered around, ember glow in the scorched edges.

_Integration:_ In ExpandedVisual, for this kind show a SpriteRenderer at field.Position: during warning (time < field.At) drop f0 from above (reuse the FallingProp arc, ExpandedVisual.cs:61-66) with its GroundShadow; at field.At switch to f1 for the active 0.3 s, then fade. Width = 2×field.Radius.

### H18 — ToyHazard hazard (currently INVISIBLE) (P1)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/ToyHazard_f0_v1.png, _f1_v1.png` · 512×512 · 2 frames (f0 incoming/warning, f1 active) · pivot (0.5, 0.15) — base on the floor

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a nightmare jack-in-the-box: a black-lacquered box painted with ember-red diamonds, lid shut and rattling, a grinning soot-faced clown peeking through the crack. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

Frames / variants (same prompt, Subject replaced by the line):

- f1 (sprung): the lid thrown open and the grinning clown head shot up on a coiled black spring, mouth wide.

_Integration:_ In ExpandedVisual, for this kind show a SpriteRenderer at field.Position: during warning (time < field.At) drop f0 from above (reuse the FallingProp arc, ExpandedVisual.cs:61-66) with its GroundShadow; at field.At switch to f1 for the active 0.3 s, then fade. Width = 2×field.Radius.

### H19 — (Optional) floor danger telegraphs: circle rim + lane arrow (P3)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Hostile/TelegraphCircle_v1.png, TelegraphLane_v1.png` · 1024×1024 (circle), 1024×256 (lane) · 1 frame each · pivot centre; lane arrow +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a dashed warning ring painted on the floor: crooked ember-red ink dashes with tiny soot thorn ticks pointing inward, transparent inside. It lies flat on the arena floor and is seen from the game's elevated camera, so its outline is a wide ellipse about twice as wide as tall; it is the object itself, not a shadow or a floor. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

Frames / variants (same prompt, Subject replaced by the line):

- Lane variant (separate file TelegraphLane_v1.png, same prompt with this Subject): a long dashed ember-red ink lane lying flat from left to right with three chevron arrow marks pointing right, soot-black edging, transparent inside.

_Integration:_ Draw during `time < field.At` in ExpandedVisual with pulsing alpha (reuse line 74's warning alpha).

## 7. Prompts — allied

### A1 — Crescent arrow (Crescent Bow) (P1)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Allied/ArrowBolt_v2.png` · 512×192 · 1 frame · pivot centre; tip +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a slim moon-crescent arrow: a cream birch shaft, a brass crescent-moon arrowhead pointing right, deep-teal fletching bound with two tiny brass rings. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. FRIENDLY COLOUR CODE: warm brass, amber and cream with deep-teal accents; no ember red as a main colour (red is reserved for enemies).

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ Assign to CrescentBow.ProjectileSprite. Name the sprite `ArrowBolt` (or update the name check in Presentation/ProjectileVisual.cs:75) so it keeps the thin bow streak.

### A2 — Rune orb (Rune Staff) (P1)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Allied/RuneOrb_v2.png` · 256×256 · 1 frame · pivot centre; spins

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a small rune orb: a round deep-teal and violet glass marble with a cream rune glyph glowing inside and a thin brass ring orbiting it at a tilt. It is a projectile seen from the game's elevated top-down camera; it spins in flight, so it is roughly round and reads the same at any rotation; one bold simple shape that still reads at 30 pixels wide. FRIENDLY COLOUR CODE: warm brass, amber and cream with deep-teal accents; no ember red as a main colour (red is reserved for enemies).

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ Assign as `ProjectileSprite` on the weapon asset (Data/Run/<Weapon>.asset), keep `ProjectileVisualWidth`. Add the CombatVFX textures to ProjectileVisual's `authored` test (line 77) so the generic halo/core stop drawing over painted art.

### A3 — Ringing bell chime (Bell Fork) (P2)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Allied/BellChime_v1.png` · 256×256 · 1 frame · pivot centre; tip +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a tiny flying brass hand-bell, its mouth facing right, a small cream clapper inside, and two curved cream sound-wave arcs spreading from its mouth. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. FRIENDLY COLOUR CODE: warm brass, amber and cream with deep-teal accents; no ember red as a main colour (red is reserved for enemies).

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ Assign as `ProjectileSprite` on the weapon asset (Data/Run/<Weapon>.asset), keep `ProjectileVisualWidth`. Add the CombatVFX textures to ProjectileVisual's `authored` test (line 77) so the generic halo/core stop drawing over painted art.

### A4 — Frost acorn pellet (Acorn Sling) (P1)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Allied/AcornPellet_v1.png` · 256×256 · 1 frame · pivot centre; tip +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a frost-glazed acorn pellet: a brass-capped acorn coated in pale ice-teal frost, a tiny icicle point at the front on the right. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. FRIENDLY COLOUR CODE: warm brass, amber and cream with deep-teal accents; no ember red as a main colour (red is reserved for enemies).

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ Assign as `ProjectileSprite` on the weapon asset (Data/Run/<Weapon>.asset), keep `ProjectileVisualWidth`. Add the CombatVFX textures to ProjectileVisual's `authored` test (line 77) so the generic halo/core stop drawing over painted art.

### A5 — Iridescent bubble (Bubble Wand) (P1)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Allied/Bubble_v1.png` · 256×256 · 1 frame · pivot centre; spins

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a soap bubble with a violet-and-teal iridescent film, a cream window highlight and a tiny brass star floating inside. It is a projectile seen from the game's elevated top-down camera; it spins in flight, so it is roughly round and reads the same at any rotation; one bold simple shape that still reads at 30 pixels wide. FRIENDLY COLOUR CODE: warm brass, amber and cream with deep-teal accents; no ember red as a main colour (red is reserved for enemies).

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ Assign as `ProjectileSprite` on the weapon asset (Data/Run/<Weapon>.asset), keep `ProjectileVisualWidth`. Add the CombatVFX textures to ProjectileVisual's `authored` test (line 77) so the generic halo/core stop drawing over painted art.

### A6 — Button dart (Button Bow) (P1)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Allied/ButtonDart_v1.png` · 256×256 · 1 frame · pivot centre; tip +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a button-headed dart: a short cream wooden shaft whose head is a big round four-hole brass sewing button facing right, deep-teal thread fletching at the back. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. FRIENDLY COLOUR CODE: warm brass, amber and cream with deep-teal accents; no ember red as a main colour (red is reserved for enemies).

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ Assign as `ProjectileSprite` on the weapon asset (Data/Run/<Weapon>.asset), keep `ProjectileVisualWidth`. Add the CombatVFX textures to ProjectileVisual's `authored` test (line 77) so the generic halo/core stop drawing over painted art.

### A7 — Teal ink bullet (Ink Pistol) (P1)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Allied/InkBullet_v1.png` · 256×256 · 1 frame · pivot centre; tip +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a teal ink droplet bullet: a teardrop of deep-teal ink with its round head on the right, a cream highlight and a small brass nib cap on the nose. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. FRIENDLY COLOUR CODE: warm brass, amber and cream with deep-teal accents; no ember red as a main colour (red is reserved for enemies).

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ Assign as `ProjectileSprite` on the weapon asset (Data/Run/<Weapon>.asset), keep `ProjectileVisualWidth`. Add the CombatVFX textures to ProjectileVisual's `authored` test (line 77) so the generic halo/core stop drawing over painted art.

### A8 — Lantern-moth ember (Lantern Staff) (P1)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Allied/LanternEmber_v1.png` · 256×256 · 1 frame · pivot centre; tip +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a lantern-moth ember: a tiny amber flame cupped in a little brass lantern cage with two cream paper moth wings, flying to the right. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. FRIENDLY COLOUR CODE: warm brass, amber and cream with deep-teal accents; no ember red as a main colour (red is reserved for enemies).

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ Assign as `ProjectileSprite` on the weapon asset (Data/Run/<Weapon>.asset), keep `ProjectileVisualWidth`. Add the CombatVFX textures to ProjectileVisual's `authored` test (line 77) so the generic halo/core stop drawing over painted art.

### A9 — Amber leaf bolt (Leaf Crossbow) (P2)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Allied/LeafBolt_v1.png` · 256×256 · 1 frame · pivot centre; tip +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: an autumn leaf bolt: a curled amber leaf wrapped around a brass crossbow-bolt tip pointing right, a teal stem flicking at the back. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. FRIENDLY COLOUR CODE: warm brass, amber and cream with deep-teal accents; no ember red as a main colour (red is reserved for enemies).

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ Assign as `ProjectileSprite` on the weapon asset (Data/Run/<Weapon>.asset), keep `ProjectileVisualWidth`. Add the CombatVFX textures to ProjectileVisual's `authored` test (line 77) so the generic halo/core stop drawing over painted art.

### A10 — Flying letter (Mailbox Launcher) (P2)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Allied/Letter_v1.png` · 256×256 · 1 frame · pivot centre; tip +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a sealed cream envelope flying corner-first to the right, a deep-teal wax seal on its flap, brass postage stamp corner. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. FRIENDLY COLOUR CODE: warm brass, amber and cream with deep-teal accents; no ember red as a main colour (red is reserved for enemies).

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ Assign as `ProjectileSprite` on the weapon asset (Data/Run/<Weapon>.asset), keep `ProjectileVisualWidth`. Add the CombatVFX textures to ProjectileVisual's `authored` test (line 77) so the generic halo/core stop drawing over painted art.

### A11 — Brass music note (Music Box) + ribbon note (Ribbon Harp) (P1)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Allied/MusicNote_v1.png, RibbonNote_v1.png` · 256×256 · 1 frame · pivot centre; tip +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a brass eighth note lying on its side, polished note-head on the right with a cream highlight, its curled flag streaming left. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. FRIENDLY COLOUR CODE: warm brass, amber and cream with deep-teal accents; no ember red as a main colour (red is reserved for enemies).

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

Frames / variants (same prompt, Subject replaced by the line):

- RibbonNote_v1.png (separate file, same prompt with this Subject): a violet silk ribbon tied into the shape of an eighth note lying on its side, note-head knot on the right, ribbon ends fluttering left, a brass bead at the knot.

_Integration:_ Assign to MusicBox / RibbonHarp ProjectileSprite; then disable the LineRenderer motif for authored sprites (ProjectileVisual.cs:110).

### A12 — Conch shell shot (Shell Cannon) (P2)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Allied/ShellShot_v1.png` · 256×256 · 1 frame · pivot centre; tip +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a spiral conch-shell cannonball: a cream and amber spiral shell with its point leading to the right and a thin brass band around its mouth. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. FRIENDLY COLOUR CODE: warm brass, amber and cream with deep-teal accents; no ember red as a main colour (red is reserved for enemies).

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ Assign as `ProjectileSprite` on the weapon asset (Data/Run/<Weapon>.asset), keep `ProjectileVisualWidth`. Add the CombatVFX textures to ProjectileVisual's `authored` test (line 77) so the generic halo/core stop drawing over painted art.

### A13 — Snow-globe shard (Snow Globe Orb) (P2)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Allied/GlobeShard_v1.png` · 256×256 · 1 frame · pivot centre; tip +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a snow-globe shard: a curved fragment of glass with swirling snowflakes inside, an ice-teal glinting edge and a piece of brass rim, sharp point to the right. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. FRIENDLY COLOUR CODE: warm brass, amber and cream with deep-teal accents; no ember red as a main colour (red is reserved for enemies).

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ Assign as `ProjectileSprite` on the weapon asset (Data/Run/<Weapon>.asset), keep `ProjectileVisualWidth`. Add the CombatVFX textures to ProjectileVisual's `authored` test (line 77) so the generic halo/core stop drawing over painted art.

### A14 — Brass star (Star Kazoo) (P2)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Allied/Star_v1.png` · 256×256 · 1 frame · pivot centre; spins

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a slightly crooked five-point brass star with a cream face and a small deep-teal gem at its centre. It is a projectile seen from the game's elevated top-down camera; it spins in flight, so it is roughly round and reads the same at any rotation; one bold simple shape that still reads at 30 pixels wide. FRIENDLY COLOUR CODE: warm brass, amber and cream with deep-teal accents; no ember red as a main colour (red is reserved for enemies).

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ Assign as `ProjectileSprite` on the weapon asset (Data/Run/<Weapon>.asset), keep `ProjectileVisualWidth`. Add the CombatVFX textures to ProjectileVisual's `authored` test (line 77) so the generic halo/core stop drawing over painted art.

### A15 — Tea droplet with sugar cube (Teapot Popgun) (P2)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Allied/TeaDrop_v1.png` · 256×256 · 1 frame · pivot centre; tip +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a hot-tea droplet: an amber tea teardrop with a cream sugar cube riding at its front on the right and a curl of cream steam trailing left. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. FRIENDLY COLOUR CODE: warm brass, amber and cream with deep-teal accents; no ember red as a main colour (red is reserved for enemies).

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ Assign as `ProjectileSprite` on the weapon asset (Data/Run/<Weapon>.asset), keep `ProjectileVisualWidth`. Add the CombatVFX textures to ProjectileVisual's `authored` test (line 77) so the generic halo/core stop drawing over painted art.

### A16 — Tin sardine (Tin Fish) (P2)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Allied/TinFish_v1.png` · 256×256 · 1 frame · pivot centre; spins

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a little tin sardine: a polished pewter fish with brass rivets, a deep-teal fin and a wind-up key on its side. It is a projectile seen from the game's elevated top-down camera; it spins in flight, so it is roughly round and reads the same at any rotation; one bold simple shape that still reads at 30 pixels wide. FRIENDLY COLOUR CODE: warm brass, amber and cream with deep-teal accents; no ember red as a main colour (red is reserved for enemies).

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ Assign as `ProjectileSprite` on the weapon asset (Data/Run/<Weapon>.asset), keep `ProjectileVisualWidth`. Add the CombatVFX textures to ProjectileVisual's `authored` test (line 77) so the generic halo/core stop drawing over painted art.

### A17 — Compass dart (Dart Compass) (P3)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Allied/CompassDart_v1.png` · 256×256 · 1 frame · pivot centre; tip +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a compass-needle dart: a small brass compass-rose star with a long deep-teal needle pointing right and a cream tail feather. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. FRIENDLY COLOUR CODE: warm brass, amber and cream with deep-teal accents; no ember red as a main colour (red is reserved for enemies).

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ Assign as `ProjectileSprite` on the weapon asset (Data/Run/<Weapon>.asset), keep `ProjectileVisualWidth`. Add the CombatVFX textures to ProjectileVisual's `authored` test (line 77) so the generic halo/core stop drawing over painted art.

### A18 — Generic friendly spark (companion response, curio retort) (P2)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Allied/AllySpark_v1.png` · 256×256 · 1 frame · pivot centre; tip +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a small friendly spark bolt: a cream-white teardrop of light with a brass-gold rim and two deep-teal glints, head on the right. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. FRIENDLY COLOUR CODE: warm brass, amber and cream with deep-teal accents; no ember red as a main colour (red is reserved for enemies).

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ Replace `catalog.Projectile` in OctoberCombat.cs:116 and RelicCombat.cs:137 with the new sprite (do not change CheckpointCatalog.Projectile itself — hero-identity shots use it as a marker in HeroIdentityVisual.Missile).

### A19 — Thrust lunge streak (rapiers, lances, tridents) (P1)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Allied/Thrust_f0..f3_v1.png` · 768×256 · 4 frames, once in 0.2 s · pivot (0.1, 0.5) — starts at the weapon, extends +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a straight lunge streak: a long tapering stroke of cream and brass light pointing right, a fine deep-teal edge line along it, a small four-point star glint at the tip, a few speed ticks at the back. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. FRIENDLY COLOUR CODE: warm brass, amber and cream with deep-teal accents; no ember red as a main colour (red is reserved for enemies).

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

Frames / variants (same prompt, Subject replaced by the line):

- f0: short bright stub at the left third.
- f1: full-length streak (base image).
- f2: streak thinning, glint flaring at the tip.
- f3: only faint speed ticks and the fading glint.

_Integration:_ Add `Sprite[] Thrust` to the new library; in PaintedEffects.Play slash branch: `frames = slam ? library.Dust : value.Weapon.Motion == WeaponMotion.Thrust ? art.Thrust : library.Slash;` (line 45). Interim fix without art: use ImpactAnimationLibrary.Pierce for Thrust.

### A20 — Teal thread strand (yo-yo tether, sewing link) (P3)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Allied/Thread_v1.png` · 1024×96 · 1 frame · pivot centre

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: one taut length of deep-teal silk thread lying straight from left to right with three small brass beads strung along it and a cream sheen on top. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. FRIENDLY COLOUR CODE: warm brass, amber and cream with deep-teal accents; no ember red as a main colour (red is reserved for enemies).

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ Use instead of `art.Thrust` for Link/YoYoTether/SewingLink in PaintedMechanicView.cs:42.

### A21 — Horseshoe magnet bolt (Scrap Magnet) (P3)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Allied/MagnetBolt_v1.png` · 256×256 · 1 frame · pivot centre; tip +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a tiny horseshoe magnet bolt: a deep-teal horseshoe magnet with brass poles facing right, a few brass iron-filings trailing behind it. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. FRIENDLY COLOUR CODE: warm brass, amber and cream with deep-teal accents; no ember red as a main colour (red is reserved for enemies).

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ Assign as `ProjectileSprite` on the weapon asset (Data/Run/<Weapon>.asset), keep `ProjectileVisualWidth`. Add the CombatVFX textures to ProjectileVisual's `authored` test (line 77) so the generic halo/core stop drawing over painted art.

### A22 — Glowing film frame (Dream Projector) (P3)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Allied/FilmFrame_v1.png` · 256×256 · 1 frame · pivot centre; tip +X

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a single glowing film frame: a small square of cream light set in a brass frame with sprocket holes, tilted slightly as it flies right. It is a projectile seen from the game's elevated top-down camera, lying flat and pointing to the RIGHT (+X): the leading tip touches the right side of the silhouette, the tail and trailing wisps go to the left. Long axis horizontal, roughly 2:1 wide-to-tall, one bold simple shape that still reads at 30 pixels wide. FRIENDLY COLOUR CODE: warm brass, amber and cream with deep-teal accents; no ember red as a main colour (red is reserved for enemies).

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ Assign as `ProjectileSprite` on the weapon asset (Data/Run/<Weapon>.asset), keep `ProjectileVisualWidth`. Add the CombatVFX textures to ProjectileVisual's `authored` test (line 77) so the generic halo/core stop drawing over painted art.

## 8. Prompts — juice

### J1 — Blast flash — friendly / neutral (P1)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Juice/BlastFlash_f0..f3_v1.png` · 1024×1024 · 4 frames, once in ~0.12 s · pivot centre

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a storybook explosion flash: a rounded cartoon burst of cream-white and warm amber light with a crooked star-shaped dark-ink outline, small brass sparks and paper flecks thrown out from the edge. FRIENDLY COLOUR CODE: warm brass, amber and cream with deep-teal accents; no ember red as a main colour (red is reserved for enemies).

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

Frames / variants (same prompt, Subject replaced by the line):

- f0: small, very bright cream bud at the centre.
- f1: full burst (base image).
- f2: burst splitting into rounded lobes, centre going hollow.
- f3: a thin broken ring of amber embers and flecks, centre empty.

_Integration:_ Load as `Sprite[] BlastFlash`; in ExplosionFeedback.Draw pick the frame from flashT (×4) instead of scaling one glow; keep the tint lerp but reduce it to 0.3 so the painted colours survive.

### J2 — Blast flash — hostile (P1)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Juice/HostileBlast_f0..f3_v1.png` · 1024×1024 · 4 frames, once · pivot centre

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a nightmare explosion flash: a ragged burst of ember-red and orange fire with a soot-black jagged ink outline, black ink droplets and burgundy shreds thrown out from the edge, a small cream-hot core. HOSTILE COLOUR CODE: dominant ember red and hot orange-red, soot-black ink and dark burgundy shadow; a small hot cream-yellow core is allowed; no teal, mint, sky blue or bright brass gold as a main colour (in this game those mean 'friendly'). It must read as dangerous at a glance even when tiny.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

Frames / variants (same prompt, Subject replaced by the line):

- f0: small hot core.
- f1: full burst (base image).
- f2: burst tearing into ragged soot-edged tongues.
- f3: smouldering broken ring of embers and soot specks.

_Integration:_ Use when the Detonation event's faction is Enemy (ExplosionFeedback.Tint already branches on it).

### J3 — Shock ring (P1)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Juice/BlastRing_v1.png` · 1024×1024 · 1 frame (code expands and fades) · pivot centre

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a painted shock ring: one closed ring of cream and amber brush strokes with a confident dark-ink outer contour, short speed ticks on its outer edge, completely transparent centre; band about 7 % of the diameter. It lies flat on the arena floor and is seen from the game's elevated camera, so its outline is a wide ellipse about twice as wide as tall; it is the object itself, not a shadow or a floor.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

_Integration:_ Use for both Ring and Rim (drop the separate rim, the ink edge is painted in); tint as today. For hostile blasts, H14 can be reused.

### J4 — Debris chips (6 variants) (P2)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Juice/Chip_{0-5}_v1.png` · 128×128 each · 6 single sprites · pivot centre

**Chip 0** (`Chip_0_v1.png`)

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a torn scrap of cream storybook paper with one inked line on it, tumbling debris thrown out of a storybook explosion, small and chunky with a bold dark ink contour; reads at 20 pixels.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

**Chip 1** (`Chip_1_v1.png`)

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a splinter of dark varnished wood, tumbling debris thrown out of a storybook explosion, small and chunky with a bold dark ink contour; reads at 20 pixels.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

**Chip 2** (`Chip_2_v1.png`)

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a broken brass gear tooth, tumbling debris thrown out of a storybook explosion, small and chunky with a bold dark ink contour; reads at 20 pixels.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

**Chip 3** (`Chip_3_v1.png`)

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a fat droplet of dark ink, tumbling debris thrown out of a storybook explosion, small and chunky with a bold dark ink contour; reads at 20 pixels.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

**Chip 4** (`Chip_4_v1.png`)

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a chip of white-and-blue porcelain, tumbling debris thrown out of a storybook explosion, small and chunky with a bold dark ink contour; reads at 20 pixels.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

**Chip 5** (`Chip_5_v1.png`)

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a small tuft of burgundy velvet thread, tumbling debris thrown out of a storybook explosion, small and chunky with a bold dark ink contour; reads at 20 pixels.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```


_Integration:_ Load `Sprite[] Chips`; assign `blast.Chips[i].sprite = Chips[(seed+i)%6]` in Spawn and stop recolouring them (the variants carry their own colours).

### J5 — Ink smudge puff (P2)

Target: `Assets/ArcaneBrigade/Art/CombatVFX/Juice/InkPuff_f0..f3_v1.png` · 512×512 · 4 frames over the puff lifetime · pivot centre

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a settling puff of dark plum ink-smoke shaped like a lobed cartoon cloud, dark ink contour, soft cream-lilac highlights along its top lobes.

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

Frames / variants (same prompt, Subject replaced by the line):

- f0: small tight puff.
- f1: full cloud (base image).
- f2: lobes separating and thinning.
- f3: three small wisps fading.

_Integration:_ Load `Sprite[] InkPuff`; frame from puffT; keep the existing alpha curve.

## 9. Integration notes (wiring plan)
- **Data.** Add `Runtime/Data/CombatVfxArtwork.cs` (ScriptableObject, `Resources/CombatVfxArtwork.asset`, static `Current` like `MechanicEffectArtwork.Current`) with arrays: `HostileBolt[2]`, `HostileSpecies` (by enemy Id), `BossOrb[2]`, `MutationShots[16]`, `FlameBolts[8]`, `HostileSecret` (by weapon Id), `HostileMuzzle[4]`, `HeroHurt[4]`, `Tentacle[2]`, `ShockRing`, `ShadowPool[3]`, `Curtain[2]`, `Office[2]`, `Toy[2]`, `Telegraph[2]`, `Thrust[4]`, `Thread`, `BlastFlash[4]`, `HostileBlast[4]`, `BlastRing`, `Chips[6]`, `InkPuff[4]`. Every accessor returns null when the slot is empty so the current procedural visual stays as fallback.
- **Importer.** Add `Editor/CombatVfxArtImporter.cs` modelled on `Editor/HeroMechanicArtImporter.cs`: scan `Art/CombatVFX/**`, set importer settings (PPU 256, pivot per table), fill the asset by file name, register each sprite's alpha bounds in `Resources/SpriteInkMetrics.asset`, and for allied weapon sprites (A1–A17, A21–A22) write `WeaponDefinition.ProjectileSprite` on the matching `Data/Run/*.asset`. Run with `-executeMethod ArcaneBrigade.Editor.CombatVfxArtImporter.Import`.
- **ProjectileVisual.** (a) treat any texture under CombatVFX as `authored` (line 77) → no halo/core; (b) for hostile authored art use tint lerp 0 (line 68); (c) add frame animation: if the state's sprite belongs to a multi-frame set, swap `body.sprite` by `time*fps + state.Id`; (d) disable the line motif when the MusicBox/RibbonHarp sprite is painted (line 110); (e) spin list for round sprites (A2, A5, A14, A16, H8 family 5).
- **Spawn sites.** H1: `CombatSimulation.cs:816` fallback for enemies. H2–H6: data only (`ProjectileSprite` on enemy weapon assets; FurnaceToad needs its own WeaponDefinition since it shares ClockBatAttack). H7: `CombatSimulation.cs:674`, `OctoberBossCombat.cs:81`. H8: `EnemyMutationCombat.cs:82`. H9: `EnemyVariantCombat.cs:157`. H10: substitution in EmitProjectile for enemy units. A18: `OctoberCombat.cs:116`, `RelicCombat.cs:137`.
- **PaintedEffects.** H11 at line 48 (enemy fire), H12 at line 38 (target is Champion), A19 at line 45 (Thrust).
- **ExpandedVisual.** H13 replaces the Tentacle LineRenderer (90-101), H14 the Shockwave ring (77, 113), H15 the ShadowPool fill (127-148), H16–H18 add sprites for Curtain/OfficeHazard/ToyHazard (new branch beside the FallingProp prop at 43-66), H19 optional warnings during `time < field.At`. Keep LineRenderers as fallback. Use one pooled SpriteRenderer per slot like `props`.
- **PaintedMechanicView.** A20 replaces `art?.Thrust` at line 42 for Link/YoYoTether/SewingLink.
- **ExplosionFeedback.** J1/J2 replace `glow` frames (35) — choose by faction in `Spawn`; J3 replaces `ring` (37) for Ring+Rim; J4 replaces `chip` (38) per chip index; J5 replaces `puff` (39) by puffT. Keep the procedural `Make()` path as fallback when the asset is missing. Coordinate with whoever is writing that file.
- **Verification.** After import, capture: one ClockBat/RuneWisp swarm at wave 10+, one boss volley, a mutation volley, a BlueFlameFiend volley, LastCustomer tentacles/shockwave/shadow pool, a CurtainWisp curtain, and the 16 allied weapons in `UI/WeaponSandbox.cs` — compare at 1080p 100 % zoom that hostile shots read red/black and allied shots warm/teal.
