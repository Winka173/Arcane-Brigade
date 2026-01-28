# Expanded painted animation 01 — Windows

## Artifact and scope

- Player: `Builds/FullRun/ArcaneBrigade.exe`. Distribute the entire FullRun directory, not the executable alone.
- Full Windows archive after this pass: `Builds/ArcaneBrigade-Windows-Animation-01.zip`.
- Archive size: 468,250,581 bytes; SHA-256 `98719EAC21F0F6091F59129F8B50801060D339BEC246445AF867E287C27C3847`. All 163 files were decompressed and SHA-256-compared against FullRun, including BUILD_INFO; `Archive_Audit.json` records each entry.
- Previous playable baseline was archived first as `Builds/ArcaneBrigade-Windows-Full-20261016.zip`; it intentionally does not contain subsequent new animation.
- Unity 6000.4.8f1 Windows x64 build succeeded, 0 player errors / 0 player warnings; reported size 1,198,013,520 bytes. Final release log: `Library/ExpandedPerformance-FinalGate.log`.
- Main runtime `ArcaneBrigade.Runtime.dll` SHA-256: `2F65EF341D439949DC5AF99AA6E45243ED81DE6C2B1B0B33385C61770F5FB6B6`.
- `Assembly-CSharp.dll` SHA-256: `7F12CA4048E5C338401F1121A478D85E2187C53692C4225DF6775731191D9DBB`.
- `resources.assets` SHA-256: `59E8B7684D3EE6C188E91727F0854B5C0FBA3D5A7C2D96AF64A1B05B66536B61`.
- Existing unrelated editor obsolete-GetInstanceID warning in BuildMorphChecks remains; player build warnings above are separate. Unity analytics/licensing connection warnings did not prevent a successful exit 0.

## Imported

Twelve boss atlases (48 poses) and eight four-weapon atlases (128 poses): **20 sheets, 176 new painted poses, 44 actor clips**. Bosses use anticipation/release/recoil and actual pooled collapse. Weapons use their actual painted-action runtime path. New sprite geometry follows connected silhouettes to avoid clipped or neighbouring atlas body parts; original PNGs are retained unchanged. Duplicate legacy weapon subparts hide while a painted action is visible. Reduced Motion keeps the stable original presentation.

Import ledger: `Imported_Coverage.tsv`; progress: `Progress.txt`; source prompts/provenance: `Docs/Expanded_Performance_Provenance.json`; implementation and branch/archive details: `Docs/Expanded_Performance_Pass.md`.

## Editor verification

| Suite | Passed |
| --- | ---: |
| Expanded performance import/selection/geometry | 786 |
| Shop presentation | 575 |
| Champion acting | 1200 |
| Animation polish | 232 |
| Ensemble | 1351 |
| Outside stage | 48 |
| October content | 500 |
| Secret events | 459 |
| Nightmare | 223 |
| Nightmare visuals | 34 |
| Nightmare farewell | 1214 |
| Total | 6622 |

## Native Windows verification

| Review | Standard | Reduced | Native screenshots/profile |
| --- | ---: | ---: | ---: |
| Actual boss/weapon animation paths | 497 PASS / 0 FAIL | 369 PASS / 0 FAIL | 127 |
| Physical shop / owned aura / exit | 163 PASS / 0 FAIL | 163 PASS / 0 FAIL | 8 |
| Story / Customer / both dawn endings | 149 PASS / 0 FAIL | 149 PASS / 0 FAIL | 34 |

Audited 1920x1080 images and complete build-file hashes are in `Release/Capture_Audit.json`, `ReleaseShop/Capture_Audit.json`, `ReleaseStory/Capture_Audit.json`. Selected boss release/collapse and weapon screenshots were inspected directly. Initial review folders are iteration history, not final evidence. They exposed atlas clipping and stray neighbouring fragments; final render geometry addresses those issues.

The final three native audits completed against the final main-runtime hash above. Their player logs contain no captured runtime exceptions, failed assertions or shader/compilation errors. Focused source whitespace checks passed.

## Not complete

The queue has **101**, not the previously mis-added 95, proposed sheets. **81 remain ungenerated**: 16 companion acting, 46 locomotion, 14 normal/evolved enemy reactions and 5 weapon sheets. Existing animation remains active for those actors. Authoring stopped when image service returned a revoked-network-permission error and HTTP 401. No alternate route or unapproved API fallback was used. `ExpandedPerformanceBuilder.RequireComplete` deliberately rejects the unfinished queue.

Neither automated rendering nor successful compilation constitutes human art/game-feel/audio approval, locked-60 extreme-crowd acceptance or Android/iOS device certification. No completion claim is made for those gates.
