# K15 Combat Feel and Art Cohesion

## Delivery

- Target-local impact pose holds (16-45 ms); champion/root movement and simulation
  continue at normal speed. Holds have a per-target cooldown. No global time scale.
- Heavy hits, crits, player damage, boss arrival/death have different camera accents,
  bounded below 0.09 world units and rate-limited. Camera tracking excludes accents
  from its smoothing history, preventing drift. Pause freezes the presentation.
- Original 16-frame cut/slam/pierce/orb atlas, fixed cell pivots, per-victim impact
  throttling, bounded/reused pools and muted allied alpha. Actual hostile projectile
  bodies/trails and arena hazard telegraphs are never culled by cosmetic density.
- Warm sprite edge lighting, quiet lower-body ambient shading and restrained paint
  saturation on existing and new units/weapons. Split-body puppets share the original
  UV lighting space. Emerged bosses return to the common painted material.
- Floor macro variation, edge depth/worn perimeter, central light and subtle slow
  haze use the paused presentation clock. No new foreground clutter or map loading.
- Fabric counter-sway, wax foot deformation, motif-dependent boss breathing and
  enraged/recovery acting; staggered friendly wave-clear hops with responsive shadows.
- Wax puddles, fabric folds, shrinking glass and rigid metal/wood death silhouettes.
  Existing authored blink/attack/idle frames and upper/lower rigs remain in use;
  this is not a full redraw or separately authored eye/mouth rig for all species.
- Wave3+ pressure/recovery cadence within existing budgets and safety rules.
  First two introductory waves unchanged. Loot curves left/right into the player,
  converges without overshoot; Reduced Motion now removes chest bob and coin rotation.
- Independent persistent Reduced Shake, Reduced Flashes and Reduced Effects settings.
  Three controls reuse the UI owner's existing settings buttons; no UI restyling or
  audio changes. Reduced Motion remains a superset for movement/camera effects.

## Validation and limits

K15 checks, K14 regression suites, Windows build and 33 authored encounter fixtures
cover early/mid/late waves, melee/ranged/mixed groups, 2/12/20 allies and bosses.
Encounter fixtures use real health, damage, movement and hostile attacks with an
authored starting roster and movement bot; they are not natural shop builds.
Mean target distance is an approach metric, not a safety or win-rate guarantee.

Full campaign routes, build results, native 4K screenshots and observed frame timings
are recorded under PreviewExports/K15. Human fun/readability/art approval and physical
Android/iOS/minimum-spec gates remain open. Do not equate desktop render size with
native4K source artwork; the original raster atlas retains its generated resolution.

Final recorded results:
- 3,817 assertions: 140 K15 plus 3,677 focused K14/K8/K10-K13 regressions.
- 33 encounter fixtures with companion-ID damage attribution, not environment damage.
- 21 campaign routes: 16 victories, 5 defeats, no hidden timeout. Normal routes:
  Lantern 3/3, Postmaster 3/3, Seer 2/3; successful normal runs reach endless hall23.
- Native Windows: 27 standard, 44 horde/boss/transition and 27 independent reduced-
  effects checks passed; no failed assertions/runtime errors. 79 PNGs, including
  different aspect ratios and four-frame visible-impact/accessibility sequences.
- Dense-wave observation: 32 peak enemies, 1,155 samples, median/p95 16.67/17.88 ms
  on RTX5070Ti while three capture players ran concurrently. Not a stress benchmark.
- Windows build: succeeded, zero errors/warnings. Binary hashes recorded separately.

## Integration notes

UI owner: keep the three accessibility setters and save/apply semantics if settings
are redesigned. New options are optional version3 profile fields, default false.
Gameplay-owned lighting uses per-renderer property blocks, not per-unit material
copies. Unity reference: https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/renderer/setpropertyblock

Audio owner: existing hit/crit/kill/boss/loot cues remain wired; no asset replacement.
