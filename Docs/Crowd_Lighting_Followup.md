# Crowd-lighting follow-up — 16 October 2026

Scope: remove repeated presentation work without changing combat counts, collision, damage, danger visibility or the darkness curve. Preserve the concurrently developed MienCho encounters and narrative.

## Changes

- Sprite lighting uses cached shader property IDs instead of resolving strings per renderer per frame.
- When nightmare lighting is active, it submits the final lighting properties directly instead of first submitting defaults that are immediately overwritten. No lighting values changed.
- NightmarePresence consumes the exact visibility already computed for the actor's body. This avoids another party-light scan and ensures eyes/ember and body use one visibility result in the same display pass.
- Companion glow identity color is cached per actor identity; pooled actors refresh it if rebound to a different species.
- Twelve editor assertions check UV, unlit visibility/saturation and shade/rim values for hostile/friendly and prop/body cases. Native checks exercise the actual lit world, solo and fifteen companions, danger cues, pause, journal, encounters and endings.

## Validation

The new FullRun build includes concurrent MienCho encounters at build time. Unity build succeeds with zero errors/warnings. FinalQuality checks now total 922; the other-agent MienCho checks total 272. Runtime SHA-256: `7C0B0604B16375D26BAD596F3849DE6F48EECB0C1C496038A6BBB2C065FEB068`.

Native evidence: MienCho 58 PASS / 21 images per profile; physical shop 440/337 PASS and nine images per profile; new table/phone/mirror encounters 71 PASS / four images per profile. Exact player-file and image audits are recorded in `PreviewExports/CrowdLighting/`. Both normal and reduced profiles complete without FAIL assertions.

Two isolated dense-crowd runs reach 1,022 units and 365–372 hostile shots. P95 is 18.053 / 17.136 ms: neither meets the strict 16.667 ms target. The normal 602-unit fixture stays around 8.335 ms P95 on the development machine. Results vary around the earlier build; no overall speedup is established. Concurrently added encounters also prevent attributing a difference solely to these optimisations. Full timings and limitations: `PreviewExports/CrowdLighting/Gate_Summary.md`.

`Builds/FullRun/BUILD_INFO.txt` identifies this newer executable. The existing `ArcaneBrigade-Windows-FinalQuality-01.zip` is retained unchanged as the previous verified delivery, not relabelled as the new build. Human balance and mobile-device gates remain open.
