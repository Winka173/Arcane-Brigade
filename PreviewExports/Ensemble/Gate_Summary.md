# Ensemble animation Windows delivery

Historical artifact: the hashes and captures below describe the Ensemble delivery, not the current FullRun build. See `../ShopPresentation/Gate_Summary.md` for the superseding shop-presentation delivery.

## Artifact

- Unity 6000.4.8f1, Windows x64: `Builds/FullRun/ArcaneBrigade.exe`. Distribute the entire FullRun directory, not just the executable.
- Gate: `Library/Ensemble-ReleaseGate.log`, exit 0. Player build: 0 errors, 0 warnings, 984,061,625 reported bytes.
- `ArcaneBrigade.Runtime.dll` SHA-256: `EAE406A8A0E59F7FF2126152D957D6EA1F9D72641813D05F8EE9088EE9F55BF7`.
- `Assembly-CSharp.dll` SHA-256: `0F0D266178AAEF0A183CC2AA629A496E5DACE2CD4EF95946C6E37A449BEC2886`.
- `resources.assets` SHA-256: `47795CADA7500FAEA6B52D5381715118C791B9AD53C6924641058C3073C07C26`.
- Both runtime assembly names are recorded: the game's main runtime lives in `ArcaneBrigade.Runtime.dll`; hashing only `Assembly-CSharp.dll` would miss it.

## Delivered scope

- 68 additional original painted poses: four merchant identities / 24 handover poses and seven weapon identities / 44 mechanism poses. These extend the earlier 120 champion acting poses; they do not replace every existing combat set.
- Fans, feather fan, parasol ribs, ink pistol, shell cannon, plasma mechanism and extinguisher use event-driven painted action clips. Stable canvas, scale and center pivot prevent handoff jumps or weapon pumping.
- Existing catalog artwork gains eleven physical-family surface/gesture/death responses. Real attacks and hurt take priority. Existing normal/evolved enemy combat sets are retained.
- Saved-party farewell includes identity-specific look-back/bow/flutter behavior and staggered large-party departure. Both real dawn endings, optional Keeper, save boundaries and separate endless unlock remain intact.
- Reduced motion/effects and pause remain supported. Crowding reduces ornament and secondary ordinary-unit shadow layers first; contact shadows, real projectiles and gameplay remain.
- Cached spatial sort delegate, heavy-only separation query and direct enemy-to-champion targeting reduce CPU overhead without removing enemies or altering combat RNG. Focused tests preserve range boundaries, target eligibility and query order/membership.
- Integration repairs: null intrinsic enemy variants; painted action checks against actual attack phase; real physical-shop approach before purchase; merchant center pivot; spring acting applied after gait; invalid duplicated pickup-hide code; duplicate journal field; final Customer excluded from generic October encore volleys; fixed non-gold wave-price factor.
- K24 reward waiting fixture restores diagnostic HP, not production HP. Reach regression now accepts the current intentional melee/late-wave balance while still requiring longer reach than the source weapon.

## Editor verification

5,316 distinct labeled assertions, plus the cooperation regression. Counts: Nightmare 223; Character Acting 360; Ensemble 919; Animation Polish 127; Build Forms 470; Secret Events 459; K26 relics 156; K24 48; lighting 34; farewell 395; Arcane audio 2,078; Toybox routing 47. Nightmare runs twice in the gate and is counted once here.

Ensemble includes full catalog resource/pose checks, reduced behavior, source immutability, heavy query equivalence, range edges and defeated/entering champion eligibility. The standalone installed-Mono sort probe records 64,000 bytes versus zero for 1,000 warmed comparer/cached-comparison calls. This is a library allocation probe, not zero-allocation gameplay.

## Native verification

`Delivery/Capture_Audit.json`: Standard 304 PASS / 90 screenshots; Reduced 304 PASS / 90 screenshots, all 1920x1080. Both completion markers are present; both player logs have zero matched exceptions, failed checks, C# errors, shader errors or failed assertions. The native run covers the actual walk-up purchase/handover, full companions/enemies/evolved enemies/weapons, all four merchant identities, collection controls and the real dense arena simulation. Audit records hashes of all delivered player files, including the main runtime assembly.

`DeliveryStory/Capture_Audit.json`: Standard 149 PASS / 34 screenshots; Reduced 149 PASS / 34 screenshots, all 1920x1080. Both replay-safe ending completion markers are present; both logs have zero matched exceptions, failed checks, C# errors, shader errors or failed assertions. Covers real final encounter, physical farewell, both four-page dawn branches, the fifth promise afterword, save/restore and no duplicate victory credit. Story captures ran concurrently only after the sequential timing captures finished; they are not performance measurements.

Older `AlignmentReview`, `StoryReview`, `Accepted`, `Timing`, `Profile` and `FinalReview` runs refer to intermediate binaries, not this delivery. The historical Character Acting report now explicitly points here instead of claiming its old hashes describe the current FullRun directory.

## Measured performance

Recorded sequentially on RTX 5070 Ti / Ryzen 7 9700X. No concurrent review player during timing. The fixture's first case is already wave 17, not an empty early arena. Additional injected units remain real simulation units; neither enemies nor projectiles are removed to obtain a frame-time result.

| Native case | Peak units | Peak projectiles | Median ms | P95 ms | P99 ms | P95 <= 16.667 ms |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| Standard wave 17 | 602 | 8 | 8.33 | 8.33 | 9.26 | Yes |
| Standard additional 420 enemies | 1022 | 328 | 14.59 | 17.37 | 19.42 | No |
| Reduced wave 17 | 602 | 7 | 8.33 | 8.33 | 8.34 | Yes |
| Reduced additional 420 enemies | 1022 | 306 | 14.59 | 26.53 | 54.39 | No |

Final raw measurements: `Delivery/Final/Performance.json` and `Delivery/Final-Reduced/Performance.json`. Heavy-case simulation/render P95: 6.75/8.09 ms Standard, 12.65/9.95 ms Reduced; these independent percentiles must not be added as a frame decomposition. Main-thread allocation counter is unavailable in this player; see scope limits.

An intermediate build before heavy-only separation filtering recorded 591.81 ms frame P95 and 576.70 ms simulation P95 around 1022 units (`Accepted/Final/Performance.json`). Filtering removed that severe sorting bottleneck. These single diagnostic runs have different real-time trajectories/background conditions; they are not a controlled cross-profile benchmark or a guarantee of exact speedup. The final adversarial stress still fails the strict 60-FPS P95 threshold, particularly Reduced. Do not call this a locked-60 performance acceptance, and do not hide that remaining limit behind the passing functional checks.

## Scope limits

This is not individually redrawn, hand-keyed facial/cloth/death animation for every catalog identity. Coverage and art/procedural distinctions are in `Coverage.md` and `Docs/Ensemble_Animation_Pass.md`. Catalog boards normalize body height/weapon width; real shop/combat scenes review production proportions. Stress HP is diagnostic.

Native captures are off-desktop D3D11 at 1920x1080, not visible-window GPU certification or human animation/audio acceptance. Android/iOS device gates remain deferred. The Windows player's managed per-thread allocation counter does not advance for a known allocation; JSON records `ManagedAllocationCounterAvailable=false` and `ManagedAllocatedBytes=-1`, not zero allocation. Heap/collection measurements remain available.
