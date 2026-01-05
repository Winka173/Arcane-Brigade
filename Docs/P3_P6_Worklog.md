# P3–P6 implementation worklog

Requested: complete army/build systems, shop/merge/save, quality slice, normal run and Endless. Work is in progress; this file is not an acceptance certificate.

1. Research reference art, weapons, movement and ambiguous shop/merge policies.
2. Build independent roster, bonds/stats/passives and combat modifiers.
3. Implement transactional shop, interest, merge/altar and boundary saves.
4. Add original modular artwork, differentiated motion, enemies/boss patterns and usable UI.
5. Connect full normal-run schedule, Endless and persistent unlock/codex/cosmetics.
6. Run regression/integration/build checks; record remaining quality or reference gaps honestly.

## Reference audit — 2026-10-13

- Inspected actual gameplay and shop screenshots in `.utmp/art-reference/`. Shape language: compact flat silhouettes, tiny faces, deliberately mismatched prop weapons, hands attached to weapons, not articulated arms. Static images do not prove animation timing.
- [Icy Veins](https://www.icy-veins.com/zenless-zone-zero/bizarre-brigade), updated 2025-07-28: timer survival, companions and upgrades; no exact economy formula verified.
- [Theria Games guide](https://theriagames.com/guide/zenless-zone-zero-bizarre-brigade/): between-wave purchases, rerolls, immortal companions, bonds and three same-unit merges. Secondary source, not frame-by-frame verification.
- [Community companion table](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Companions): search excerpt supports three Level One Little Witch -> Level Two; full page fetch failed. Does not establish weapon inheritance or all tier rules.
- [Gameplay video](https://www.youtube.com/watch?v=0RcLBt0125A), published 2024-10-12: subsequently downloaded to local research-only `.utmp/art-reference/bizarre-reference.webm`. Inspected frames at 30, 90, 180, 300, 480, 600, 660, 720, 840 and 960 seconds, plus 720.1–720.6 seconds at 0.1-second intervals. This supersedes the earlier static-only audit. Not a full frame-by-frame timing reconstruction.
- GamerSky page fetched but article mechanics were not exposed as searchable text. Do not cite it as confirmed detailed evidence.

## Policy boundary

Exact interest formula, duplicate handling, weapon retention/reroll on merging, maximum tier and altar exceptions remain NEEDS_REFERENCE. Implementations must distinguish project decisions/tunable experimental policies from verified reference mechanics. Do not import/reference the other game's assets in shipping content or claim its animation was copied frame-for-frame.

## Motion observations and implementation

- Around 12:00–12:00.6, companions reposition independently of the protagonist. Body pose itself tilts/rotates strongly while attacking; not merely a weapon on an inert body. Detached hands remain with prop weapons. Large impact flashes, brief arc-like shapes and damage numerals make hits read quickly. These are observations from this sampled footage, not recovered source curves or exact attack durations.
- Shop frames around 1:30, 3:00 and 14:00 show minimal, strongly differing hats/faces/props, not elaborate armor. A big merchant silhouette frames a compact purchase area. Kept original characters/weapons and own wide-screen shop layout rather than importing reference sprites/UI.
- Added data-driven body attack lean (35-degree slam, 22-degree slash, 10-degree thrust, 8-degree recoil/bell), impact squash, floating actual-damage numerals and separately driven grip motion. Our tuning, not claimed to match original degrees/timing exactly.
- Python/PyAV only decoded video frames for inspection; no reference pixels entered generated atlases. Temporary analysis dependencies yt-dlp/PyAV are under `.utmp/reference-tools`, not Unity dependencies or shipping content.

## Execution status

Functional P3/P4/P6 paths are connected, with a P5 playable candidate and original six-body/six-weapon art. The build/research work does not substitute for the required owner review of feel/art/UI, natural-run pacing, full reference-policy verification or mobile/controller production checks. Detailed scope and final measured results: `Docs/Full_Run_Implementation.md`.
