# World shop follow-up

**Reference correction, 2026-10-13:** This documents the current prototype, not 1.5 parity. The new audit confirms altar offers can cost eggs **or HP**, with activation influenced by held eggs. The fixed two-hall schedule, egg-only offer pool, flat reroll price and merchant visuals remain gaps. See `Bizarre_Brigade_15_Reference_Audit.md` and `Reference_Parity_Backlog.md`. No runtime correction was implemented during that research turn.

The fullscreen purchase grid has been removed from RunArena. After a hall, combat ends and three gold offers appear as physical stands. Move with WASD/arrows or the touch stick, approach within 1.1 world units, then press Space or the contextual USE button. Walking onto an offer never buys it. Sold stands remain visible and cannot be purchased twice.

Reroll and next-hall entry are physical interaction points too. Escape pauses the rest phase; resuming does not start combat. Rest movement does not advance the wave timer, heal the player, spawn enemies, or run attacks. Companions continue roaming. Purchases rebuild the visible roster from the saved build; no selling, locking, slot cap, or upkeep was added.

The EGG SHOP portal leads to a separate set of three offers using `RunSnapshot.Eggs`, never gold. Visiting is free; returning does not refresh either stock. Gold rerolls do not refresh egg offers. Currency, stock tokens, and sold flags survive boundary saves. Fusion remains a separate physical altar action, not a substitute for the special shop.

## Reference fidelity and provisional rules

Three in-world stands and nearby interaction follow the user's correction and the shop footage at 90/180/600/840 seconds in https://www.youtube.com/watch?v=0RcLBt0125A.

The current special-shop schedule (every two halls), price (two eggs), shared unlocked-item pool, and existing egg reward (one per hall / three for a defeated boss) are **provisional authored rules**, not verified copies of Bizarre Brigade. Tune `SpecialShopEvery` and `SpecialShopPrice` in RunCatalog. Special-enemy egg drops, probabilistic vendor appearance, exclusive special-item tradeoffs, merchant artwork and purchase animations remain reference-fidelity work. Do not present the current special shop as an exact recreation.

## Validation artifacts

`PreviewExports/FullRun_Checks.txt` includes separate-wallet, insufficient-eggs, sold guard, restore, rest movement and rest-pause checks. Rendered-player diagnostic capture additionally checks that interaction fails from the rest entry position, buys a companion and an upgrade at their actual stands, then buys an egg offer while verifying gold is unchanged. Diagnostic saves are isolated from real player saves.

Current screenshots are under `PreviewExports/Presentation/`: `Unity_FullRun_Shop_UI.png` and `Unity_FullRun_Egg_Shop_UI.png`. These are constructed shop fixtures, not evidence of a naturally completed run or tested mobile controls.

Verified 2026-10-13: 96 Full Run checks and 7 presentation checks passed; Windows build succeeded with zero build errors/warnings. Both headless player smoke and rendered spatial-shop/egg-wallet capture exited 0. Final shop/boss screenshots were visually inspected. Legacy six-offer saves retain their first three offer entries when restored into the new three-slot catalog; existing purchases/items/wallets are preserved.
