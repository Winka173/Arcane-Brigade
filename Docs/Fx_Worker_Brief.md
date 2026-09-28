# Brief for effect workers (29/10/2026)

You draw bond effects for Arcane Brigade (Unity 6, built-in render pipeline, 2D top-down toybox game, dark hallway floor, painted storybook
sprites). Read first: `Docs/Fx_Rebuild.md`, `Assets/ArcaneBrigade/Runtime/Presentation/Fx/StormFx.cs` (the quality bar: read it fully),
`FxKit.cs`, `FxBatch.cs`, `FxDirector.cs`, `Resources/Shaders/FxProc.shader`. The owner reads Vietnamese; write reports in Vietnamese.

## Your deliverable
For every rule in your list, one effect class registered in your own file `Runtime/Presentation/Fx/<Family>Fx.cs` (a `public static sealed`-style
`static class <Family>Fx` with `public static void Register(Dictionary<string, Func<FxEffect>> map)`; the director finds it by reflection, class name
must end in `Fx`). The bond rules and numbers are FIXED (`Docs/Bond_Steps_Proposal_2026-10-28.md`, `Runtime/Core/Bonds/BondStepsV6.cs`). The look is
YOURS: you decide every animation. Do not let the older docs dictate looks. Find how each rule is emitted: `Moment(`/`Strike(`/`LineStrike(`/`Zone(`/`StepWeather(`
in BondStepsV6.cs (BondMomentRequest: kind, from, to, radius, delay, duration) and `Emit("rule"...)` in `BondV3Runtime.cs`/`BondV5Runtime.cs` (trait events arrive
as a Strike at the subject's position with radius 1 + 0.25*value). Step 8 weather arrives with duration 999 (draw until the wave ends, fade in) and then
usually separate Strike moments. Some rules may emit both a Weather and a Strike (see how StormFx handles both with one class, switching on `Moment.Kind`).
Zones (kind Zone) last `duration`; draw them for their whole life. Lines have From and To (width in Radius). Register EVERY rule your bonds emit that is
visible (grep for your bond's names), not only the table's.

## Hard rules from the owner (repeated many times; violating any is a defect)
- NEVER reuse an animation. Every rule has a visibly different animation: different silhouette and motion, not a bigger/smaller/recoloured copy of
  another rule, not another bond's. Step 6 must not look like step 2/4.
- LOUD: big, exaggerated, made of many pieces, readable on a phone over hundreds of enemies. A faint or thin effect is a defect. Aim at the footprint
  of the area it really hits (radius in world units; the arena is 24 x 13, hero units are ~1 unit).
- Two kinds of hit only: targeted (source to target) and area (a blast over an area). Summons and weather sit on top.
- Only lightning is a bolt. Other elements fly as their own piece (ember, shard, leaf, note) on an arc.
- Nothing radial bursting out of a friend's own body, no rings on our side, no friendly target zones. Skills never target a single enemy.
- Enemy danger zones are one solid fill (not yours). NO camera shake. Damage numbers are not yours.
- Use light and shaders: additive glow, procedural shapes, layered quads. Style: warm storybook, thick-outline feel, dark moody floor, colours that
  pop against it. Pieces should look hand-made, not generic particles.
- Summons must act out what they do (a summon bond effect = a creature or object that visibly performs its action).

## How to work (important: other workers run in parallel)
- NEVER edit files outside your own: do not touch FxKit/FxBatch/FxDirector/FxProc.shader/other families' files, RulesV3Ink*. If you truly need a shared
  helper, copy it into your own file. New shader shapes: write them in YOUR cginc only (`Resources/Shaders/FxShapes<X>.cginc`, mode numbers: A 20-29, B 30-39,
  C 40-49, D 50-59, E 60-69), draw them with `b.Quad((FxShape)23, ...)`. The cginc is yours exclusively; keep the existing function signature.
- Work in your OVERLAY folder so half-written code never breaks others: write files under `C:/ABWork/<yourname>/` using the same relative path as the
  real project (e.g. `C:/ABWork/A/Assets/ArcaneBrigade/Runtime/Presentation/Fx/WindFx.cs` and `.../Resources/Shaders/FxShapesA.cginc`). Preview with the
  env var: `OVERLAY=C:/ABWork/A bash Tools/BondFx/fx_frames.sh <rule> <Strike|Line|Zone|Weather> [radius] [delay] [length]` run from the project root
  `C:/Users/Winka/OneDrive/Documents/Arcane-Brigade` (about 30-90 s per run; it waits for a free Unity clone, never run Unity any other way, never run
  Unity on the main folder). Frames land in `C:/ABClones/fxprev/<rule>/f000.png...` and a tiled `sheet.png` (4x4, every 2nd frame). READ the frames with the
  Read tool (it shows images) and judge them honestly: scale against the crowd, loudness, silhouette, clipping at quad edges (shapes must fade to zero
  before |u|=1), is it different from every other effect. Use ffmpeg `C:/Users/Winka/.stacher/ffmpeg.exe` to crop/combine frames into one image to
  save tokens (see how it was done for StormFx: pick 4-6 key frames, hstack/vstack). Iterate until it looks good, then it counts; a first draft is not done.
- Use a distinctive length/delay/radius from the real emission in your previews (radius ~2 for sky events, 1-1.5 for traits, lines ~8 long).
- When a file compiles, plays well and passes your own frame review, copy the finished files (the .cs and your .cginc) into the real project at the same
  relative path (copy only, do not leave overlay copies behind). Then run the full check once from the project root:
  `CLONE=c1 bash Tools/BondFx/unity_clone.sh C:/ABClones/checks_<name>.log ArcaneBrigade.Editor.BondStepChecks.Run -quit` (look at the log for "BondStepChecks:" lines;
  it must not fail; pending rules from other workers are expected). If the method name differs, grep for it.
- Do NOT git commit or push anything. The lead commits.
- Token economy: the owner demands maximum token savings. Do not narrate; do not re-read large files you already read; batch previews; crop frames.

## Finish
Reply (Vietnamese, under 250 words): each rule done with a one-line description of its look and how you judged it; rules you could not finish and why;
anything you saw that looks wrong in someone else's effect or the shared code. Do not claim in-game verification: you only saw frame previews.
