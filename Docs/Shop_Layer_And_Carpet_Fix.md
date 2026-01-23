# Shop layers, rarity attachment and rolling carpet

## Changes

- Merchandise mounts above the pedestal using its sprite pivot. Pedestal and merchandise sort from the same station anchor; merchandise always draws on top, including during airborne entrance and departure.
- Rarity lights follow the actual merchandise renderer bounds in late presentation order, inherit its visibility/alpha, and remain behind its body. Purchased relic flights, recruited allies and retiring reroll stock also carry their own attached lights. Destroyed stock removes its lights.
- An eligible companion occupies the first slot before two companion purchases in that wave. After two purchases, that slot has a 15% offer gate; after three it is item-only. Normal and special markets share the purchase quota. Solo heroes remain item-only; unavailable/exhausted companion pools cannot manufacture an offer. Large-roster new-species weighting and merge-copy preference remain separate from this slot gate.
- The illustrated carpet uses uniform scale again, preserving its original aspect ratio. Its position covers the stock row without vertical stretching. No source-resolution increase or native-4K-art claim.
- Entrance unfolds left to right over 1.25 seconds. A textured cylindrical roll travels across the cloth, has raised geometry, lit/dark faces, and loses thickness as it unwinds. The flat cloth behind it retains the original texture; it is not an opacity-only reveal. Departure winds it back toward the left without fading the entire cloth. Reduced Motion retains the non-rolling alternative.

## Verification

- `ArcaneBrigade.Editor.ShopLayerChecks.Verify`: 2,247 focused assertions, including 800 shop rerolls, quota/solo rules and physical carpet geometry. The two-purchase fixture produced 30 companion offers in 200 rolls.
- Existing K17/K16/K15/K14/K12/market/K11/K13/K10 regression suites pass.
- Final Windows build: succeeded, zero build errors/warnings. Play `Builds/FullRun/ArcaneBrigade.exe`; distribute the entire folder.
- Final runtime assembly SHA-256: `EBFE13853AF5688E0D346B7F2B711A905808555718689F7EAEFEDCD605800837`.
- Final native player checks: 225 passes, zero failures; Reduced Motion: 207 passes, zero failures. These cover merchandise/pedestal ordering, body clearance, aura following and hiding, bought item/ally motion, reroll retirement, undistorted carpet and seamless stage transport.
- Native presentation evidence is stored in `PreviewExports/ShopLayerFix`. Captures are actual 3840×2160 player frames, not source-art resolution claims. The five-boss frame is an explicit visual diagnostic fixture, not ordinary encounter or balance acceptance.

No overall UI redesign or audio-asset replacement is included.
