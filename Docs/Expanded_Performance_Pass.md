# Expanded painted performance pass

## Delivery order and scope

1. Rebuilt the existing working tree with Unity before changing animation and archived the complete Windows folder to `Builds/ArcaneBrigade-Windows-Full-20261016.zip` (428,693,513 bytes; SHA-256 `0E7B9CC8716579B1B6208302D800A6723AB6585BBE353ED8433DA3AF02E9DE69`). This is the pre-expansion playable baseline, not a promise that later animation changes are included.
2. `git fetch origin` confirmed local main and origin/main both at `34458e6`, with no ahead/behind commits. The expanded October content and offline audio integration are uncommitted working-tree changes and are included in this build. No shared changes were reset or committed. The audio branch still has three commits not in main's ancestry (`29e042c`, `0e77a23`, `0ed73a6`); its offline handoff and subsequent game-specific integration supersede source hunks. This is not a claim that those Git commits were merged. Current audio library checks passed 2,078 assertions in `Library/Windows-FullDelivery-Audio.log`; this does not constitute a new listening approval.
3. Began painting and integrating the animation queue. Its actual total is **101 sheets**, not 95: 16 companion acting + 46 locomotion + 14 enemy reactions + 12 bosses + 13 weapons.

## Imported assets

- Twelve individual boss atlases, four newly painted poses each: anticipation, attack release, hurt/recoil and collapsed defeat. Includes ten ordinary bosses, Last Customer and Outside Keeper. Each keeps its existing design and mechanics. No balance, boss progression, endings or rarity rules are changed.
- Eight weapon atlases, four weapons per sheet, four mechanism poses per weapon: **32 weapons / 128 poses**. Existing seven Ensemble weapon clips are retained. Total new imported art: **20 sheets / 176 poses / 44 actor clips**.
- Original PNGs live in `Assets/ArcaneBrigade/Art/PerformanceExpansion/`. Built-in image generation with inspected local reference boards; genuine alpha preserved. Source filenames and prompt set: `Expanded_Performance_Provenance.json`.
- Runtime lookup is per identity, kind and evolution. Boss reaction art overrides actual windup/release/hit windows, not every idle frame. Reduced Motion retains the stable original animation. Existing champion acting and October locomotion remain active.
- Actual pooled boss finales use the new recoil/collapse poses, with stable reference scale. Existing material fragments, anatomical scatter, payout and audio remain intact.
- Collapse keeps the same reference scale whether the boss dies from an original idle pose or from a newly painted recoil; it does not apply the reference-height correction twice. Native review exercises both capture paths across the boss roster.
- Weapon clips feed the existing `WeaponPaintedAction` path, not a preview-only component. Ensemble regeneration preserves extra imported clips. Character acting regeneration already preserves expanded entries.
- Painted weapon action suppresses legacy articulated subparts during its active window, preventing doubled blades/lenses. The existing mechanism returns outside that window and in Reduced Motion.
- Import uses connected-component bounds instead of blind equal-cell clipping. Original PNG pixels are not rewritten. Per-sprite render outlines isolate the main connected silhouette so a pose extending across a nominal grid line does not bring in a neighbour's severed appendage. This was found by inspecting actual boss-collapse screenshots, despite initial frame-selection tests passing.

## Validation

- `ArcaneBrigade.Editor.ExpandedPerformanceBuilder.Gate` imports available sheets, writes `Imported_Coverage.tsv`, checks actual runtime selection and runs shop/acting/animation/October/nightmare regressions before building Windows.
- `RequireComplete` explicitly fails while a source sheet is missing; importing available work cannot accidentally report the whole queue finished.
- Dedicated native diagnostic: `Tools/Nightmare/Capture.ps1 -Expanded -Player Builds/FullRun/ArcaneBrigade.exe -Output <root>/Final`; add `-Reduced` for the second profile. Uses actual UnitVisual windup/release/recoil, DeathFeedback/BossFinaleVisual and WeaponPaintedAction paths.
- Native evidence and final artifact hashes: `PreviewExports/PerformanceExpansion/Gate_Summary.md`. Diagnostic stage shots are not proof that every battle composition has been artistically approved.
- Initial `Review`, `ContactReview` and `OutlineReview` folders are iteration history; only the named final release audit is evidence for the final executable. Early fixed-cell captures exposed clipped boss bodies; component bounds restored the full shape and render outlines removed unrelated neighbouring pixels.

## Explicit remaining work

**81 sheets are not created:** 16 companion acting, 46 locomotion, 14 normal/evolved enemy reactions, 5 weapon sheets. Existing animation remains the fallback for these actors. Image generation stopped after `application network permission was revoked` and HTTP 401 authentication errors on weapons-08/09. No network-policy bypass, alternate credential or unapproved CLI/API fallback was attempted. Resume asset authoring only when the authorised image service is available.

Extreme-crowd locked-60 performance, human play/art/audio acceptance and mobile-device gates remain separate and unaccepted. Do not infer completion of those from Windows compilation or diagnostic captures.
