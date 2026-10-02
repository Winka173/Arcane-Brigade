# Brief: animation features 1, 2, 3, 4, 8, 9, 10 (29/10/2026)

The owner approved these from a suggestion list. Read `Docs/Fx_Worker_Brief.md` first (hard rules, tools, overlay workflow, token economy, Vietnamese report)
and `Docs/Fx_Rebuild.md`. The effect system is `Runtime/Presentation/Fx/` (FxBatch, FxKit, FxDirector, NumberOrnament) with shapes in
`Resources/Shaders/FxProc.shader` (built-in shapes: Glow, Flame, Beam, Vortex, Shock, Sparkle, Smoke, Rays, Cracks, Disc, Shade, Link, Dust). Element of a bond,
rule or unit: `RulesV3Ink.LookFor(key)` (ElementLook), colour `RulesV3Ink.LookColour(look)`. Damage numbers: `CombatFeedback`. Bond HUD: `UI/BondHudFx.cs`.

Other workers are editing at the same time: the bond family files `Fx/*Fx.cs` (Storm, Wind, Ember, ... Minor) and `FxShapesA-D.cginc` belong to them;
do NOT edit those. Edit only the files your task names; for any new shader shape use your own new cginc included from a NEW shader of yours, or draw with
the built-in shapes. Keep hook edits in shared files to a few lines. Before copying into the project, compile in the overlay with
`OVERLAY=C:/ABWork/<you> CLONE="c1 c2" bash Tools/BondFx/unity_clone.sh C:/ABClones/<you>.log ArcaneBrigade.Editor.RulesV3AllChecks.Run -quit` and grep
the log for `error CS` and `RulesV3AllChecks: PASS`. Never commit. Presentation only: never change damage, timing, targeting or any rule number.

Hard rules (owner) apply to all of this: never reuse an animation, loud but not blinding (nothing covers a big part of the field), no rings or bursts on
our side, no friendly target zones, no camera shake, enemy danger zones only for bosses.

## Second list, approved 29/10/2026 (items 2, 3, 4, 8, 12)
- F1 (2) Companion cast tell: ~0.3 s before a companion uses a skill, its weapon glows in its bond's element / a wind-up pose (UnitVisual.PlayCast area), never a ring or burst on our side.
- F1 (3) Element projectiles: a companion's ordinary shots carry a trail of its main bond's element (ember trail, lightning tail, droplets, leaves, notes...), replacing the generic shot glow in RulesV3InkSteps.DrawShotGlow; cheap for hundreds of shots.
- F2 (4) Floor reacts to step-8 weather: puddles under rain, frost under blizzard, scorch and ash under fire weather, spores under plague... persistent while the weather lasts, fading after; under units, never hiding enemies.
- F2 (8) Dynamic light: big effects throw their element's light on the floor and on nearby enemies like a real lamp (additive floor light + a tint on enemy sprites in range), short and bounded.
- F2 (12) Smart effect budget: when the field is crowded or frame time is high, merge or skip small effects (step 2/4 traits, ornaments, enemy states) first and always keep step 6/8 moments readable; a single budget object others can query.
