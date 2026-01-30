# Animation 02 / Dream Levitation — Windows

## Current build

`Builds/FullRun/ArcaneBrigade.exe`, Unity 6000.4.8f1, Windows x64. Distribute the entire FullRun folder. Player build succeeded with **0 errors, 0 warnings**, reported size 1,538,881,221 bytes. Final build log: `Library/Levitation-PaintedGate.log`; complete integration log: `Library/Levitation-CompleteGate3.log`. The final incremental build reran expanded animation checks, hero identities and levitation/art checks after fixing diagnostic-only cue/time handling and adding painted suspension mist.

Runtime SHA-256 `B1E262AE92E58EB1B320957F5D5B26872DDA193EAE8BCCA1F6C62349D25D4911`; Assembly-CSharp `EFD7638781558C17D4D2C194999782AD7362CFBEE5024604BB9C579F82C2A0E7`; resources.assets `46FDA37EECEEA1108942EFE96D33FCEB368EC2A3E6D728451A133F6D1ACAC231`.

Full archive: `Builds/ArcaneBrigade-Windows-Animation-02.zip`, **599,426,932 bytes**, SHA-256 `958BB570D8B5B43024DAD9FA5A00AA7C4385309E8A84A43FF0753E4C9E80B28F`. All **163 files**, including BUILD_INFO, were stream-decompressed and SHA-256-compared against FullRun; `Archive_Audit.json` records every entry. No separate full extraction was needed.

## Completed animation queue

| Kind | Sheets | Actor clips | Newly painted poses |
| --- | ---: | ---: | ---: |
| Companion acting | 16 | 61 | 244 |
| Hero/companion locomotion | 46 | 91 | 728 |
| Normal/evolved enemy reactions | 14 | 56 | 224 |
| Boss reactions/collapse | 12 | 12 | 48 |
| Weapon mechanisms | 13 | 50 | 200 |
| Total expansion | **101** | **270** | **1444** |

Counts exclude existing compatible authored clips retained for other identities. Catalog coverage explicitly verifies all 31 heroes/66 companions have acting and locomotion, all 28 enemy species have normal/evolved reaction entries, all 12 bosses have reactions and all weapons resolve a painted mechanism. The additional mist atlas has 8 frames and is separate from this 101-sheet queue. Original PNGs retain their pixels/alpha; sprite meshes isolate main connected silhouettes. Locomotion-31 was corrected after actual import exposed touching neighbouring poses.

## Editor gates

Expanded import/selection/geometry **6070**; hero identities **2330**; levitation **23234** including original mist frames; shop **575**; champion acting **1200**; animation polish **232**; Ensemble **1459**; October **500**; secret events **459**; nightmare **223**; nightmare visuals **34**; farewell **1214**. Complete-gate legacy October signature fixture explicitly disables new hero identities only for its old Lantern Weaver/Rain Archivist test; the new identities are checked normally by their separate suite. No gameplay identity is disabled in the player.

## Real Windows render checks

| Suite | Standard pass | Reduced pass | Frames per profile |
| --- | ---: | ---: | ---: |
| Expanded animation | 4524 | 4324 | 319 |
| Physical shop / levitation | 187 | 187 | 9 |
| Nightmare / transitions / endings | 149 | 149 | 34 |

All six native runs finished without failed checks. Hash/dimension audits: `Capture_Audit.json`, `Story/Capture_Audit.json` and `../PerformanceExpansion/Complete/Capture_Audit.json`. These audit actual player files and 1920×1080 native screenshots, not fabricated mockups. An earlier expanded diagnostic used an invalid preview cue, then a negative test clock; both fixture mistakes were fixed and the final full runs above passed.

Representative inspected screenshots: [floating shop](Final/shop-overview.png), [later hover phase](Final/suspended-shop-later.png), [wave 17](Story/Final/depth-17.png), [wave 1](Story/Final/depth-1.png), [enemy normal/evolved release](../PerformanceExpansion/Complete/Final/enemies-4-1.png), [companion locomotion/release](../PerformanceExpansion/Complete/Final/locomotion-7-6.png).

The current shared working tree includes the other agent's hero-identity and description/shop-feedback work. No new Git commit/merge/push is claimed. Locked-60 extreme-crowd performance, subjective human art/balance/audio approval and real Android/iOS device tests remain separate gates. Some existing environment layers still overlap by design; floating presentation is not a full redraw of all scenery.
