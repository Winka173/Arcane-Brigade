# Effect rebuild (29/10/2026)

The bond effects are being redrawn from scratch, per the owner's decision, because patching the old ones kept making them
worse. The chain lightning (Storm step 2 hops, `RulesV3Ink.OnBondDamage`) is kept: the owner likes it.

## What replaced what
- Deleted: `RulesV3InkDrawn.cs`, `RulesStepLooks.cs` (shared look table), the painted-flipbook and weather-tint paths in `RulesV3InkSteps.cs`.
- New, in `Runtime/Presentation/Fx/`: `FxBatch` (one draw call per layer), `FxDirector` (registry rule -> effect), `FxKit` (bolts, glints, smoke),
  one file per bond family (`StormFx.cs` first). Shapes are procedural, in `Resources/Shaders/FxProc.shader`.
- A bond moment whose rule has no effect yet draws nothing. `BondStepChecks` lists those rules; set `RebuildDone = true` there when all are drawn.

## Judge an effect in seconds
`bash Tools/BondFx/fx_frames.sh <rule> <Strike|Line|Zone|Weather> [radius] [delay] [length]` renders it frame by frame onto the real floor
with a crowd of enemies (about 25 s once Unity is warm) into `C:/ABClones/fxprev/<rule>/` and tiles `sheet.png`.

## Rules every effect follows (owner)
Never reuse or recolour another rule's animation; loud and made of many pieces; no rings or bursts on our side, no friendly target zones;
only lightning is a bolt; no camera shake; enemy danger zones stay one solid fill.

## Done
| Rule | Look |
|---|---|
| `storm-thunderbolt` (6) | dark cloud and crackling arcs gather, white flash, pillar of three forked bolts, shock rim, fissures spread under the crowd, sparks and smoke |
| `storm-thunderstorm` (8) weather | dark arena, cloud ceiling, slanted rain, sheet lightning |
| `storm-thunderstorm` (8) strike | ball lightning: a spinning plasma orb sinks from the clouds, pulses three times throwing arcs, collapses to a point |
