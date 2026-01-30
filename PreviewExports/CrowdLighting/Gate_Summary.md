# Crowd Lighting follow-up — 16 October 2026

## Build

Actual Windows x64 player: `Builds/FullRun/ArcaneBrigade.exe`, Unity 6000.4.8f1. Build succeeded with zero build errors/warnings; reported size 1,560,097,414 bytes. Includes the shared working tree's new MienCho encounters, not just the lighting changes.

Runtime DLL SHA-256: `7C0B0604B16375D26BAD596F3849DE6F48EECB0C1C496038A6BBB2C065FEB068`. Exact player metadata: `Builds/FullRun/BUILD_INFO.txt`.

## Changes verified

Cached shader property IDs; no submissions of defaults immediately overwritten by nightmare lighting; no duplicate body/eye party-light query in one actor display pass; cached companion glow identity color. No change to enemy counts, attack frequency, collision, damage, darkness curve or solo visibility. Twelve added regression assertions check old lighting constants/UVs. See `Docs/Crowd_Lighting_Followup.md` for scope.

Editor gate: FinalQuality 922, roster15 1,021, recruitment 802, hero identity 2,330, expanded performance 6,070, outside-stage 48, October 500, secrets 459, nightmare 224, nightmare visuals 34, farewell 1,214. Separate MienCho checks: 272. Logs: `Library/CrowdLighting-ChecksBuild.log`, `Library/CrowdLighting-MienChoChecks.log`.

## Native verification

- MienCho normal/reduced: 58 PASS and 21 screenshots each; includes automatic opening, solo/six/fifteen lights, actual enemy cues, danger reveal, pause, journal, progress and both farewells. Each profile has eight 1080p images and thirteen 720p images (automatic opening plus the journal/dawn/ending tests).
- Physical shop normal: 440 PASS / 9 images; reduced: 337 PASS / 9 images. Material support, focus, rarity attachment, departure and absence of large shader-error pink blocks checked.
- All captures store pre-launch player file hashes. `MienCho/Capture_Audit.json` and `Shop/Capture_Audit.json` compare every file to this build, require completion/no FAIL assertions and check exact expected dimensions.
- New physical table/phone/mirror encounters: 71 PASS / four images in each profile; `Encounters/Capture_Audit.json` confirms this player. Checks actual approach, remote-examine rejection, correct companion reflection/solo fallback, journal chapter, input consumption, pause, disk-persisted page/encounter acknowledgement and absence of encounters during combat. Three world images are 1080p; `encounter-mirror-720.png` is 720p. These results are separate from the journal-only fixture.

## Two isolated stress runs

1080p DX11 on Ryzen 7 9700X / RTX 5070 Ti, no concurrent editor or player during measurements. Standard fixture: 602 units / 7 shots, P95 8.334–8.335 ms. Dense fixture is deliberately beyond the standard wave: 1,022 units / 365–372 hostile shots.

| Dense run | Median ms | P95 ms | P99 ms | Simulation P95 ms | Render P95 ms |
| --- | ---: | ---: | ---: | ---: | ---: |
| Stress-1 | 14.130 | 18.053 | 22.432 | 7.068 | 7.681 |
| Stress-2 | 13.900 | 17.136 | 19.890 | 6.560 | 7.454 |

Both complete their diagnostic assertions but **both fail the strict 16.667 ms P95 performance target**. No stable-60 claim or demonstrated overall speedup: the results vary around the previous build, and concurrent new content prevents attributing differences solely to the optimisation. Each dense sample observes 47 collections; managed allocation counter is unavailable, not zero. Raw results and player identities: `Stress-1/`, `Stress-2/`.

## Remaining work

Dense-crowd performance still needs profiling; multi-seed balance/human playtesting and physical mobile tests remain open. This pass does not certify minimum-spec hardware, narrative taste, audio quality or universal balance.

The previous FinalQuality-01 ZIP remains unchanged and identifies an older verified player. Any new archive for this follow-up must use its own name and stream-decompressed verification manifest; do not substitute the old archive's checksum or audit.

New archive: `Builds/ArcaneBrigade-Windows-CrowdLighting-01.zip`, 613,062,065 bytes. All 163 entries were stream-decompressed and compared to the source files; the player remained unchanged during packaging. Manifest: `Package_Verification.json`. ZIP SHA-256: `62A7E16513D059F63029EAC8359244CA69A0CEAD57C3DFEDE5709B50DD22CDEB`. The runnable folder remains `Builds/FullRun/`.
