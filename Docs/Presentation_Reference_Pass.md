# Presentation reference pass — 2026-10-13

Request: review and rework motion, damage feedback, sword/magic/bow effects, UI, music and map toward Bizarre Brigade. The previous Full Run is a functional prototype, not an accepted presentation match.

**Follow-up:** the owner selected **1.5+**, not the older video alone. New Belle/Boo Fortress footage, tutorial cross-checks and a 57-item source gap audit are recorded in `Bizarre_Brigade_15_Reference_Audit.md`. `Reference_Parity_Backlog.md` replaces a narrow VFX-only work order. Latest map requirement is a bounded arena larger than the viewport with a following camera; the map/camera in the existing build has not yet been updated for it.

## Work order

1. Inspect combat sequences, shopping/menu screens, floor treatment and actual audio from the reference video; separate observation from authored tuning.
2. Give attacks independent anticipation/impact/recovery and identifiable slash, arrow and magic cues; improve hurt/death feedback without changing immunity or controls.
3. Replace the rectangular grid testbed and spreadsheet-like UI with an original nightmare-arcade presentation, preserving the same world bounds and purchase rules.
4. Add distinct SFX and an original music study; do not ship the reference recording or call a synthetic study the final soundtrack.
5. Capture actual Unity frames and motion sequence, compile/regression/player checks; record what remains unlike the reference.

Reference video: https://www.youtube.com/watch?v=0RcLBt0125A. Research-only files remain in `.utmp/art-reference`, never in shipping Assets.

## Implemented follow-up

- Replaced the tiled floor with an original muted painted nightmare floor; collision bounds remain unchanged.
- Added original bow/staff/arrow/orb artwork, animated bow draw hand/string, casting charge, melee arcs, impact bursts, kill puffs, projectile streaks, hit flash and critical numbers. Hands are floating; no arms were added.
- Added a purple arcade UI treatment and contextual world-shop interaction rather than the previous fullscreen purchase grid. See `Docs/World_Shop_Implementation.md` for the separate egg shop and provisional economic rules.
- Added original synthesized combat/shop music studies and differentiated swing/bow/cast/hit/pickup sounds. These are placeholders, not ripped reference music or a finished soundtrack.

## Assets and provenance

original raster assets: `Art/Presentation/NightmareFloor_v1.png` (new image), `BowMagic_v1.png` (new image), `BowMagic_v2.png` (image edit). Paths are relative to `Assets/ArcaneBrigade/`. Prompts are saved in `Docs/Presentation_Map_Prompt.txt`, `Docs/Presentation_Weapons_Prompt.txt`, and `Docs/Presentation_Weapons_Edit_Prompt.txt`. Unity imports and slices the source artwork; reference video/audio is not shipped.

## Remaining quality gaps

Reference motion and shop frames were visually inspected. Reference audio was downloaded for research, but audio playback was unsupported in this tool session: **neither the reference nor the generated music was auditioned**. Waveform validation does not establish musical quality. Bow pose fidelity, merchant character/art, richer hand-drawn animation, special-vendor appearance/drop rules, exclusive special upgrades, sound design/mixing and final soundtrack composition still need work. The original map is more textured than the reference. No mobile-device, Steam, commercial-quality or full-run playtest acceptance is claimed.

Rendered Unity screenshots and a silent frame-sequence diagnostic are exported in `PreviewExports/Presentation/`. The encoded motion study uses 12 fps; it is not a measured real-time performance capture.
