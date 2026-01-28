# Character acting Windows delivery

Historical checkpoint: `Builds/FullRun` has subsequently been rebuilt for the Ensemble animation pass. The hashes and captures below describe the earlier Character Acting artifact, not the current executable. See `PreviewExports/Ensemble/Gate_Summary.md` for the newer delivery and its scope.

## Accepted artifact

- Player: `Builds/FullRun/ArcaneBrigade.exe`; Unity 6000.4.8f1, Windows x64. Keep/distribute the entire FullRun folder.
- Final gate: `Library/CharacterActing-ReleaseGate.log`, successful exit 0. Player build: **0 errors, 0 warnings**, reported size **937,999,956 bytes**.
- Runtime `Assembly-CSharp.dll` SHA-256: `F0380C33430708C872D5A2DE857E782C31148BF2D2E238615837479A722AF1A1`.
- `resources.assets` SHA-256: `37405F6DAE357A8B9D85783539E9BC4F1B7CE09639E6131668A12C12279A8EA0`.
- `Accepted/Capture_Audit.json` and `AcceptedStory/Capture_Audit.json` record all player-file hashes and screenshot hashes for this artifact. Earlier Review, Delivery and StoryDelivery directories are intermediate evidence, not this release.

## Implemented follow-up

- Ten identity-preserving transparent acting sheets, **120 original additional painted poses**, integrated into real curiosity, hurt, celebration and defeat presentation. Existing combat/walk clips remain; Lantern Knight's twelve authored combat poses were not replaced.
- Five companion aliases only where the body identity genuinely matches. No generic face replacements for unrelated species.
- Source-specific mechanical articulation for camera lens, music-box winding key and mailbox flag, alongside existing separate scissor blades. Regional shader masking preserves the rest of the source weapon. Reduced Motion restores the intact weapon.
- Distinct gold/dream merchant anticipation, flourish, refusal and settling, using actual event ages. This is source articulation, not newly hand-painted grabbing hands.
- Collection LOOK/CHEER/FLINCH/TIRED controls, two-row layout with separate star controls and description padding, variable keyboard/controller pose cycling, isolated overrides and cleanup.
- Correct defeat-frame pixels-per-unit normalization and foot-pivot continuity. Visual review rejected the earlier excessive material flattening; painted defeat poses now retain readable shape and expression.
- Party rest-attention is also available during the real farewell choice, without changing narrative state or input.

## Verification

**4,397 distinct Editor assertions PASS**, plus the cooperation regression. Breakdown: character acting 360; animation polish 127; nightmare 223; build forms 470; secrets 459; relics 156; Outside Stage 48; visuals 34; farewell 395; Arcane audio 2,078; Toybox routing 47. Duplicate nightmare import invocation is counted once. No new listening approval is claimed.

| Native runtime suite | Standard | Reduced Motion/Effects/Flashes | Screenshots per profile |
| --- | --- | --- | --- |
| Animation and collection, `Accepted/` | 119 PASS / 0 FAIL | 119 PASS / 0 FAIL | 31 at 1920x1080 |
| Story/combat/ending, `AcceptedStory/` | 149 PASS / 0 FAIL | 149 PASS / 0 FAIL | 34 at 1920x1080 |

Animation coverage includes real shop/pause/start-stop, all ten champions and both samples of each new acting cue, actual defeat remnants and their world-height/readability bounds, weapon mechanisms and reduced fallback, effects pool stress/reset, enemy/boss acts, actual collection layout, read-only profile checks and scene cleanup. The identity board is a controlled fixture, not a naturally completed campaign. Its labels are diagnostic text, not new gameplay UI.

Independent story coverage includes nightmare depths, seamless carrier transfers, final Customer entrance/death, physical farewell, both complete endings, save/scene reload and no duplicate win credit. Every final Player.log was checked: no exception, assertion failure, C# error, shader error or diagnostic FAIL. Hidden-window Direct3D presentation warnings are not treated as frame-pacing certification.

Inspected runtime images include curiosity, celebration, tired acting, source-specific weapon windup/release, Reduced Motion fallback, actual collection controls, both defeat stages, the final Customer attack and the physical farewell room. Earlier flattening was corrected before these accepted captures were regenerated. All 130 accepted screenshots were written during the final player runs; old screenshots were not relabelled as new evidence.

## Scope boundary

Direction, art sources, per-identity prompt constraints and provenance: `Docs/Character_Acting_Pass.md`. The broader presentation ledger remains `Docs/Animation_Polish_Pass.md`.

This delivers the defined champion acting set and targeted mechanisms, not bespoke painted clips for every companion, enemy evolution, merchant grabbing action and weapon in the catalog. Existing authored/procedural animation remains the fallback for those sources. No gameplay balance, shop RNG, audio assets or story eligibility changed in this follow-up. Mobile device testing, crowded-endless performance and human-perceived art/motion acceptance remain open.
