# K25 Windows gate - 2026-10-15

## Delivered content

- Eight new specialist species, each with eight normal and eight evolved imported poses: 128 body poses, plus a dedicated sixteen-frame smoke/flame atlas. Total ordinary roster is now 28 species.
- Five independent affixes on existing enemies: black smoke, blue flame, red flame, blackfire and twin flame. Real paced attacks, warning/recovery, shared windup cap, pause/rest cleanup and deterministic capped population. No automatic affix health multiplier.
- Coffin summons and procession followers are bounded, short-lived and have no direct loot payout. Actual blackfire damage, twin alternation, leader death, kill-during-windup cancellation and summon expiry are checked.
- Hand/carpet-tongue/keyhole-eye carriers rotate every four waves, using the same anatomy fought on the optional Outside Keeper. Wave stamp waits for landing. A long frame cannot skip the whole transfer. New hostile fields use painted wisps rather than geometric targeting outlines.

## Build and automated gates

- Unity 6000.4.8f1 Windows x64 build succeeded: **0 errors, 0 warnings, 775215495 bytes** reported by Unity. Run `Builds/FullRun/ArcaneBrigade.exe`; share the entire `Builds/FullRun` folder, not the executable alone. Build files remain local/ignored by Git.
- K25 focused checks: **314 passed**. K24 focused checks: 48. K23 star-up/cooperation fixtures pass.
- Regressions pass: K22 evolution 681, K21 encounter/contact 199, K20 anatomy 417, K19 encounters 325, K10 stats 172, K16 motion 220, K17 content 2476, K18 experience 91, K8 merchant 17; K4 merge also passes. An earlier Unity Bee state-file rename failure was retried successfully, without deleting source or weakening checks.
- Native normal scenario: **81 passing checks, zero failures, 50 captures at 3840x2160**. All eight normal/evolved pairs, specialist anticipation/release, real ability outcomes, five overlays on an existing evolved enemy, actual player kills/death dissipation, and both new transfers.
- Native Reduced Motion/Effects/Flashes scenario: **81 passing checks, zero failures, 50 captures**. Reduced transfers are accepted when inactive, rather than incorrectly requiring Unity's deferred Destroy to have completed in the same frame.
- Outside Keeper/swarm/merge regression on the final build: **23 passing checks, zero failures, 18 captures**. Contact/shop regression: **8 passing checks, zero failures, 4 captures**.
- Final complete capture directories: `Library/K25-Acceptance-Final`, `Library/K25-Acceptance-Final-Reduced`, `Library/K25-Acceptance-Final-Outside`, `Library/K25-Acceptance-Final-Contact`. Eight selected captures are tracked in `Accepted/`; test notes are tracked alongside this report.
- Ten champion bot routes at seed 171077: **six victories, four defeats**, reaching waves 14-20. Campaign/economy reports are observations for one policy and seed, not a human balance approval or measured win-rate target. Summon caps and shop quota guards remain active.

## Artifact identity

- Runtime DLL SHA256: `1E4542E559BE58330F9DA33C970B7C95B1C6546D2ACBF968D8836D373C3AB265`.
- Resources SHA256: `8F8F9AA908D5F1EF22090776D47523936A07F73AB208B22737EF64BDE4D9FECC`.
- Built on top of the audio agent's separate main commit `c99d32e` (Toybox soundtrack and sound redesign). This gameplay patch does not regenerate or stage audio. No fresh listening or mix approval is claimed by this agent.

## Scope and limitations

Original source atlases are not native 4K; 4K refers to the Windows render/capture. Animation uses authored sprite poses and procedural motion, not a full skeletal/cloth rig. Eye/tongue carriers use original K24 anatomy; their import keeps the main connected pose and preserves sprite IDs. Lost Lantern Soul uses explicit uneven-row cuts. Earlier lantern-crop, effect-contamination and transfer-timing/deferred-destruction fixture failures are not accepted evidence.

Human art/readability/feel acceptance, multi-seed balancing, Windows frame-time profiling, Android/iOS real-device gates and publishing remain separate. Native high-health fixtures deliberately isolate abilities and do not establish normal-run difficulty or performance. See `Docs/K25_Enemy_Affixes.md` and `Docs/K25_Art_Prompts.md` for behavior, caps and original art prompts.
