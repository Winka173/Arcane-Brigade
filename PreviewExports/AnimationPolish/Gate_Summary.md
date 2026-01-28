# Animation polish Windows delivery

Historical delivery: the current FullRun player is superseded by `PreviewExports/CharacterActing/Gate_Summary.md`. Hashes and assertion counts below describe the earlier animation-polish artifact, not the current executable.

## Artifact

- Player: `Builds/FullRun/ArcaneBrigade.exe`, Unity 6000.4.8f1 Windows x64. Distribute the whole folder.
- Final Editor gate: `Library/AnimationPolish-FinalGate.log`, successful exit 0. Player build: **0 errors / 0 warnings**, reported size **874,872,509 bytes**.
- Existing Editor deprecation warnings in `BuildMorphChecks.cs` (`GetInstanceID`) are not counted as player build warnings and were not hidden by editing that other feature.
- Runtime assembly SHA-256: `DB31B42A3651F0EB44516C9A1829B7AD43E8336D6F7040F4DC9C95931293DDB5`.
- `resources.assets` SHA-256: `127973378C8383E929F8FBED689B8E2145C39D3FC7818373F2DDDA5113F1C716`.
- `Delivery/Capture_Audit.json` and `StoryDelivery/Capture_Audit.json` include per-frame and full player-file hashes. They refer to this build, not the earlier candidate builds.

## Tests and native evidence

**4,030 distinct Editor assertions PASS**, plus the cooperation regression. Breakdown: animation polish 120; nightmare 223; build forms 470; secrets 459; relics 156; Outside Stage 48; visuals 34; farewell 395; Arcane audio 2,078; Toybox routing 47. Duplicate nightmare import invocation is counted once.

| Native suite | Standard | Reduced Motion/Effects/Flashes | Frames per profile |
| --- | --- | --- | --- |
| Animation review (`Delivery/`) | 36 PASS, 0 FAIL | 36 PASS, 0 FAIL | 18 at 1920x1080 |
| Actual story/combat regression (`StoryDelivery/`) | 149 PASS, 0 FAIL | 149 PASS, 0 FAIL | 34 at 1920x1080 |

The animation review exercises a real shop, merchant rig, pause clock and real champion start/stop movement before switching to a controlled board of ten champions, thirteen weapon motion families, painted effects/chest and enemy/boss rosters. It checks hinged scissor parts, reduced fallback, repeated-clock stability, pool stress/reuse and native frame dimensions. The actor board is not a natural combat encounter. Its title/font are diagnostic labels, not a new UI style.

The independent story suite covers actual nightmare depths, carrier transfers, final Customer entrance/death, shop focus, pause and both ending branches with save/scene reload. No final Player.log reports an exception, failed assertion, C# error or shader error. Direct3D presentation warnings may still appear in hidden-window diagnostics; this is not a claim of warning-free native graphics logs or certified frame pacing.

Actual inspected frames include the articulated merchant shop, weapon windup board, painted material impacts/new chest and late-act boss board. These frames establish rendering, not human-perceived smoothness or audiovisual approval. Individual motion timings also have focused assertions. No gameplay video/listening approval is claimed.

## Corrections during development

- The default K7 capture runner initially competed with the new runner. It now excludes the animation-specific flag.
- Hard-coded outer-window resizing could turn 1920x1080 into 1918x1072. The launcher now moves the window without resizing its client area. A subsequent runtime resize attempt could stall offscreen and was removed; the final launcher preserves the requested surface.
- Uniform slicing clipped painted particle tips, including a framing revision. Final import uses connected components and complete source margins rather than pretending the generated atlas is exactly uniform.
- The coarse original chest was replaced by an original four-pose painted chest rather than disguising it with particles.

`Candidate/`, `Review/`, `Accepted/`, `FinalReview/` and `StoryRegression/` are development/intermediate evidence, not the accepted final artifact. Final accepted directories are **Delivery** and **StoryDelivery**.

## Scope and remaining work

See `Docs/Animation_Polish_Pass.md` for the nine-group implementation ledger and original-art provenance. This increment adds presentation systems and targeted original art without changing damage, range, enemy counts, prices, shop RNG or narrative eligibility.

It does **not** finish every bespoke drawing/rig in the large wishlist: unique eyelid/mouth/cloth clips for every actor, mechanical rigs for every gun/fan/umbrella/music-box, every merchant's hand-to-object grabbing choreography, every evolved enemy's unique death, or individual farewell choreography for every possible party still need source-specific authoring. Safe existing frames/procedural acting are the fallback. Do not report these as completed hand-drawn clips.

Mobile device performance, long campaign balance, crowded-endless frame pacing and human animation/art acceptance remain open. The new painted accent pool is bounded at 48 (lower under crowd pressure); the chest reveal pool is four. Cosmetic effects yield to Reduced Effects, High Readability and Reduced Motion without deleting hostile shots.
