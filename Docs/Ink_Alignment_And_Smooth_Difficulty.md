# Painted alignment, animated rarity and smoother difficulty

## Presentation

- `SpriteInkMetricsBuilder.Build` bakes alpha-based painted bounds for 253 stock, stand and weapon sprites. Bounds account for import resolution versus original PNG resolution. Runtime uses a cached table, not texture readback. Rebuild the table after replacing relevant artwork.
- Merchandise aligns its painted bottom-center with the animated pedestal cushion, rather than the transparent sprite rectangle. Rotation, breathing, focus scaling and travel remain separate; the merchandise and its ground contact follow the cushion without inheriting its entrance jump twice.
- Rarity uses presentation time, so it animates while combat simulation rests in the shop. Added a short arrival bloom, staggered rotating ray flares and rising/fading motes. The envelope follows target visibility and size during purchase, retirement and travel. Reduced Motion suppresses moving motes and ray movement.
- Companion weapon sizing is normalized against the painted silhouette. At each body size, the longest painted dimension is constrained to 1.05–1.75 body heights for melee, 0.65–0.95 for ranged. Weapon silhouettes and tier shaping remain distinct; this is a visual correction, not a damage or range buff.
- The cyan player marker is flattened onto the ground below the body's feet and draws behind the body. Removed its overhead placement; retained the player rim highlight.

## Difficulty

The old wave-4 cliff combined a spawn-interval cut, a doubled batch, a tripled population cap and immediately closer spawn edges. Wave 5 increased the batch again.

- One enemy per spawn beat; population pressure now grows smoothly through interval and cap, rather than multiplying both interval and batch at early milestones.
- Baseline wave 3/4/5 intervals: 1.295 / 1.097 / 0.952 seconds. Caps: 39 / 48 / 57, before event/difficulty modifiers. Spawn edges tighten gradually over waves 4–10.
- Event spawn/cap bonuses are blended halfway toward their former strength. Event spawn interval stays at least 0.25 seconds.
- Enemy HP grows more slowly. New body-attack enemy species have 20% less HP than the previous curve. Boss extra bulk starts at x4 instead of x10 and gradually reaches x6.5 at wave 20; boss size and attacks remain intact.
- Late-game HP still grows substantially, with a tested wave-20 baseline x10–x15 band, rather than the earlier x15 minimum. No new wave-3-to-4 HP or spawn-rate discontinuity.

## Validation boundary

- 259 focused ink/weapon/30-wave progression checks, 2,247 existing shop/carpet checks, and the regression suites pass. The former wave-5 dense-population test was replaced with explicit smooth-early/dense-late tests, matching the new requirement.
- Ten-second protected, non-attacking simulation fixtures count 8 / 11 / 31 enemies at waves 3 / 5 / 20. These fixtures verify spawning, not survival balance.
- One deterministic 10-hero campaign seed: five victories and five defeats, no timeouts. Drowsy Seer, Comet Runner and Mirror Keeper now reached waves 15/14/18; Root Oracle and Lone Jelly completed wave 20. Glass Dice lost at wave 12 and Treasure Acorn at wave 20. This is bot evidence, not human difficulty approval or a general win-rate claim.
- Final Windows build: zero errors/warnings, 652532693 bytes. Runtime assembly SHA-256: `EF233D46D1B609E82F9CF4277CC58C562B293D1DE4A484429BA6F48C6EAC1E82`.
- Native captures/check notes and campaign/economy CSVs: `PreviewExports/InkAndBalance`. Existing K17 campaign files are preserved as historical evidence. The simultaneous boss frame remains an explicit presentation fixture, not normal encounter or performance evidence.
- Final 3840×2160 runtime checks: 762 normal passes and 704 Reduced Motion passes, zero failures. Includes cushion alignment, live shop motes, minimum painted weapon size, ground-level player marker and seamless transport.

No audio assets or overall UI layout were changed.
