# Final quality pass — 16 October 2026

Requested scope: finish the remaining dialogue, legacy hero/environment art, differentiated boss moves, balance evaluation and Windows crowd optimisation. Preserve movement-only controls, no targeting circles, immortal companions, current hero identities and the new deliberate levitation direction. Do not overwrite the other agent's shared changes.

## Execution order

1. Measure the existing Windows crowd fixture before tuning.
2. Author species-specific companion dialogue and legacy hero/environment artwork.
3. Replace generic boss encore rotation with identity-specific spatial choreography; preserve readable windup and existing signature attacks.
4. Run seeded shop/build progression diagnostics across heroes/approaches and difficulty/depth checkpoints; tune only measured systemic failures.
5. Profile and optimise measured hotspots without lowering enemy counts or silently dropping damage.
6. Rebuild Windows, run native presentation/story/crowd checks and package evidence. Record device gates and human acceptance separately.

## Acceptance boundaries

Automated movement and seeded builds do not certify human enjoyment or all build combinations. A development-machine performance result does not certify minimum-spec hardware. Android/iOS physical-device acceptance requires connected devices and platform build support; do not mark it complete on Windows. Subjective art/audio approval remains the user's decision.

## Implemented

- 15 original sheets / 200 additional painted poses: 10 legacy heroes with 12 poses each, 16 landmarks and four finale architecture objects with four poses each. Imported into the actual unit, acting and scenery libraries, not preview-only art. Connected-component bounds preserve complete objects even when tall art crosses nominal grid rows; outlines discard neighbouring-cell fragments while retaining sizeable detached furniture details. Architecture uses its complete main outline, not clipped pieces from neighbouring rows. Full prompts and source paths: `Final_Quality_Art_Provenance.json`.
- 198 distinct English companion lines, three per species across cozy/nightmare/farewell contexts. Existing pacing and merchant priority remain intact. This is text/acting integration, not 198 recorded voice performances.
- Twelve boss-specific spatial encores, with different positions, ordering, escape lanes and timing. Existing signatures and six projectile rotations remain; the generic bullet component is reduced during the new spatial encores. Existing organ/tentacle/ink/falling-prop presentation is reused, not twelve new VFX families. Committed hazards retain windup and clear on shop entry; no targeting circles added.
- Unit-ID dictionary replaces repeated full-roster source lookups in projectile and combat-event paths. Heavy-crowd presentation uses whole painted enemy poses and staggered quiet-enemy cosmetic updates; world movement, attacks, collisions, enemy counts and hostile damage are not skipped. Hero and boss detail stays intact.
- Rebuilt with concurrent recruitment/15-body roster changes and the other agent's SharedLightField/MienCho source present. The obsolete ambient-light test was updated to assert the exact new progressive curve (.92, .7125, .505, .2975, .09), rather than incorrectly enforcing the earlier .44 floor. This is integration coverage, not full acceptance of the other agent's new narrative UI.
- Native inspection caught a pink glow rectangle while the concurrent glow material was not yet imported. The final build includes the required materials; missing/unsupported glow resources now suppress the optional decoration instead of displaying an error quad, and a failed light-field setup no longer activates enemy dimming. Shop captures audit visible material support and count large pure-magenta pixel blocks. Each capture records the exact player file hashes before launch; acceptance rejects a different or subsequently rebuilt player.
- Preserved rescue, sent-letter and optional-Keeper outcomes in the new ending text. The revised creature terminology does not erase prior choices, and an unvisited Keeper is not described as defeated. Both positive and unchosen branches have a regression check.

## Balance evidence, not certification

`PreviewExports/FinalQuality/Natural_Progression.csv` contains 186 production-rule bot runs (31 heroes x three approaches x two difficulties), 1,288 wave samples and one fixed seed. No gold or immortality is injected. Easy difficulty: 20 victories, 31 defeats, 42 unresolved. Highest difficulty: 47 defeats, 46 unresolved. A 90-second diagnostic limit leaves 88 unresolved fights, 84 at the first boss (wave 4). Unresolved is neither a defeat nor a victory.

This flags a first-boss/purchase-policy bottleneck that needs targeted comparison and human playtesting before a global HP or economy change. No blanket stat adjustment is justified by this single movement/purchase policy; none is claimed here. The runs precede the concurrent SharedLightField/MienCho changes. More seeds, specialised builds and human sessions remain necessary.

## Open acceptance gates

- Crowd stress must be evaluated by P95/P99, not median alone. Earlier isolated runs improved median but did not consistently meet the strict 16.667 ms P95 target at 1,022 units / roughly 340 hostile shots. Contended runs are not comparable. Allocation counter is unavailable on this player; -1 does not mean zero allocation.
- Android/iOS modules and physical devices are unavailable. A connected Android emulator is not a physical-device pass; no Mac/Xcode/iPhone gate is claimed.
- Human art, nightmare readability, pacing, difficulty and audio approval are separate from automated render checks. Windows development hardware is not minimum-spec certification.

Status: content integration and Windows native presentation/story checks complete for Final Quality 01. Final results: 876 native images, all eight presentation profiles complete without FAIL assertions; strict dense-crowd P95 remains open (17.805 ms at 1,022 units / 366 hostile shots). Multi-seed balance/human/device gates are explicitly not complete. Current player/package identity and all evidence: `PreviewExports/FinalQuality/Gate_Summary.md`. Previous Animation-02 evidence remains historical, not evidence for this executable.
