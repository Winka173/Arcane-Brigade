# Final Quality 01 — Windows integration

16 October 2026. Unity 6000.4.8f1, Windows x64. Actual player built successfully, zero build errors and zero build warnings; Unity reported 1,552,988,886 bytes. This includes the shared working tree at build time, not a claim that uncommitted changes have been pushed to main.

## Delivered content

- 15 additional original sprite sheets / 200 poses: ten legacy heroes with twelve poses each and twenty environment/architecture objects with four poses each. Animation-02's earlier 101 sheets / 1,444 poses / 270 clips are retained separately.
- 198 bespoke English lines covering all 66 companion species and cozy/nightmare/farewell contexts. Existing gibberish audio is reused, not new recorded speech.
- Twelve individual spatial boss encores; retained signatures and projectile rotations, readable timing, distinct escape routes and actual damage fields. Reuses existing organ/ink/tentacle/falling-prop art rather than claiming twelve new VFX families.
- Production unit-ID lookup optimisation and cosmetic-only heavy-crowd LOD. No reduction of enemy count, damage, simulation steps or collision checks.
- Concurrent recruitment pacing, 15-body roster/bond changes, MienCho story and shared-party light integrated. Missing light resources fail safely; the included glow materials were checked in the real player.
- Fixed furniture slicing across nominal sheet rows and stray neighbouring fragments. Inspected the complete architecture and shop renders. Preserved rescue, letter and optional Keeper outcomes in the new ending text.

## Editor checks

`Library/FinalQuality-CompleteSpritesBuild.log`: FinalQuality 910; roster15 1,021; recruitment 802; hero identity 2,330; expanded performance 6,070; outside-stage 48; October 500; secrets 459; nightmare 224; nightmare visuals 34; farewell 1,214. The separate final MienCho check run passes 241 assertions (`Library/FinalQuality-MienChoFinalChecks.log`). These counts test their stated fixtures, not universal gameplay correctness.

## Native Windows checks

Only `Accepted/` captures describe this player. Every run records the player's file hashes before launch; audits compare every file against the current player, check completion/no FAIL lines, and verify native 1920×1080 PNGs, except the deliberate journal/life-page tests `09-journal-scroll-720.png` and `09b-companion-life-and-arc-720.png` at 1280×720 in each MienCho profile. Intermediate folders are retained as investigation logs, not release evidence; superseded PNGs were removed to reclaim space.

| Fixture | Standard PASS | Reduced PASS | PNGs per profile |
| --- | ---: | ---: | ---: |
| Animation/art/12 boss encores | 4,697 | 4,497 | 375 |
| Physical shop/focus/rarity/departure | 433 | 332 | 9 |
| Nightmare progression/transfer/endings | 149 | 149 | 34 |
| MienCho journal/lights/real enemy cues/both farewells | 52 | 52 | 20 |

Total: 876 native images across these eight runs. Each MienCho profile has eight 1080p frames followed by twelve 720p frames: the two journal/life-page tests, real dawn, four CloseShop ending pages and five LeaveLight ending pages. The capture intentionally remains at 720p after testing the journal. These exact filenames are supplied to the audit; unexpected sizes are rejected. Shop checks additionally verify visible renderer shader support and reject large pure-magenta error blocks. Higher assertion counts do not establish visual beauty. Actual screenshots were inspected, including the repaired architecture, shop overview and solo wave-19 darkness; not every image has been manually approved.

MienCho capture must not start the older K7 runner simultaneously. Its initial window-resolution changes finish before the window is moved offscreen. Interrupted intermediate MienCho attempts are not counted as passes.

## Isolated crowd measurement

`Accepted/Stress/Final/Performance.json`, Ryzen 7 9700X / RTX 5070 Ti, 1080p DX11, no concurrent Unity editor or game at launch:

| Fixture | Peak units / hostile shots | Median ms | P95 ms | P99 ms |
| --- | --- | ---: | ---: | ---: |
| Wave-17 production crowd | 602 / 7 | 8.333 | 8.336 | 8.343 |
| Additional 420 enemies and hostile stream | 1,022 / 366 | 13.374 | 17.805 | 22.670 |

The dense fixture **does not pass** a strict 16.667 ms P95 / stable-60 target. The diagnostic itself completes, which is not a performance pass. Dense simulation P95 is 6.634 ms and render P95 7.906 ms; 48 collections were observed. Managed allocation counter is unavailable (-1), not zero allocation. Earlier runs show variability; do not claim a guaranteed improvement or minimum-spec performance.

## Balance and remaining gates

186 single-seed natural production-rule bot runs / 1,288 wave samples: easy has 20 victories, 31 defeats and 42 unresolved; highest difficulty has 47 defeats and 46 unresolved. Of 88 timeouts, 84 occur at the first boss. A timeout is not a loss or win. This flags the boss/purchase-policy bottleneck; it does not justify blanket enemy nerfs or certify economy/build balance. No global balance tuning is claimed in this pass.

Open: targeted multi-seed build/boss evaluation and human playtesting, dense-crowd P95 optimisation, minimum-spec hardware, physical Android and iOS tests. Android/iOS Unity modules and real mobile devices are unavailable; an emulator is not physical acceptance. Audio taste and human art approval remain separate.

## Player and package identity

- Executable: `Builds/FullRun/ArcaneBrigade.exe`; distribute the entire folder, not the EXE alone.
- ZIP: `Builds/ArcaneBrigade-Windows-FinalQuality-01.zip`.
- Package verified: 612,173,845 bytes, 163 files, every file stream-decompressed and SHA-256 compared to its source; player remained unchanged during packaging. Full manifest: `Package_Verification.json`.
- ZIP SHA-256: `EED9D2CDF0475695E9FF9DFEF505FAEE5C18E0C535AE4BD4EAA1200E832BDE17`.
- Runtime DLL SHA-256: `775B3557D27C4B2A6941B11F57535FC14135308CE1DDEA878E6D519A3C99CBAD`.
- `resources.assets` SHA-256: `F9BA328FE3B7C0FD72C897840A0563EA517618300F4C5C5F84C12DDCA4348CD2`.
- Exact per-file identities and PNG hashes: `Accepted/*/Capture_Audit.json`; player metadata: `Builds/FullRun/BUILD_INFO.txt`.
