# Shop presentation Windows delivery

## Artifact

- Unity 6000.4.8f1, Windows x64: `Builds/FullRun/ArcaneBrigade.exe`. Distribute the entire FullRun directory.
- Release gate: `Library/ShopPresentation-ReleaseGate.log`, exit 0. Player build succeeded: 0 errors, 0 warnings, 1,076,619,861 reported bytes.
- `ArcaneBrigade.Runtime.dll` SHA-256: `B347082A75C43F5AAC5680051B7710AB321F9B1039D4726383B7555D768A7319`.
- `Assembly-CSharp.dll` SHA-256: `30A99220716F69EBE0DC893A41F4607F4985CFD54C0D429EE6A1092AB815C9AD`.
- `resources.assets` SHA-256: `17A5651876A37C64AC6C81BA983C21B0EAEFE4B1A736FFB687238143F0CCCE09`.

## Delivered

- Separate right-hand continue doorway with a smaller interaction reach; merchandise purchase regions cannot select it.
- Actual stock approach state now drives growth, bounce, outline and brighter rarity. Feet remain pinned to the animated cushion support.
- Sixteen original painted rarity frames, larger coloured light and motes; no segmented target rings. Aura follows stock entrance, motion, reroll and departure.
- Owned companions retain rarity stats but never carry their stock rarity aura into gameplay.
- Visible-ink grounding and contact shadows for fixtures and landmarks; two freestanding cabinet replacements. Architectural curtains/windows remain behind merchants; exit lane stays clear.
- Door animation respects the shop lifecycle instead of overwriting its fade.

## Automated editor checks

| Gate | Passed |
| --- | ---: |
| Shop presentation / anchoring | 575 |
| Champion acting preservation | 1200 |
| Animation polish | 232 |
| Ensemble | 1159 |
| October content | 500 |
| Outside stage | 48 |
| Secret events | 459 |
| Nightmare | 223 |
| Nightmare visuals | 34 |
| Nightmare farewell | 1214 |

## Native evidence

| Review | Standard | Reduced | Screenshots per profile |
| --- | ---: | ---: | ---: |
| Physical shop | 163 PASS, 0 FAIL | 163 PASS, 0 FAIL | 8 |
| Story and ending regression | 149 PASS, 0 FAIL | 149 PASS, 0 FAIL | 34 |

Both release audits completed. Player logs contain no captured exceptions, assertion failures or shader/compilation errors. Focused source diff whitespace checks passed.

Final artifact captures and file hashes: `Release/Capture_Audit.json` and `ReleaseStory/Capture_Audit.json`. Standard and Reduced profiles use native 1920x1080 Windows rendering. Shop capture physically approaches each offer, verifies highlight and support contact, purchases a rare companion, walks to the separate exit and returns to actual combat without leftover shop aura.

Intermediate folders (`Review`, `Delivery`, `FinalReview`, `Accepted`, `Story`) are diagnostic history, not release acceptance. They exposed missing merchandise Inspect wiring, support-transform update ordering and an obsolete six-decoration story expectation. The intentional exit-lane decoration removal changes the base shop count to five; final story tests reflect this layout.

Selected shop screenshots were inspected directly: highlighted merchandise, owned ally after purchase and the separated doorway. This is not player approval, GPU/performance certification or mobile-device testing.

## Scope boundary

See `Docs/Shop_Presentation_Pass.md` for provenance and remaining roster animation work. The manifest contains 101 proposed jobs, correcting the previous arithmetic error of 95. This historical shop delivery does not claim every expanded character or weapon has bespoke newly painted animation. Subsequent animation delivery status is recorded in `../PerformanceExpansion/Gate_Summary.md`.
