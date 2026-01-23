# K17 review implementation

## Gates

1. Economy: every companion purchase counts against three purchases per wave across both shops. Rerolls after quota contain items only. Increasing prices and rerolls must remove late-game infinite shopping without upkeep.
2. Combat: varied hostile trajectories; stronger, larger legacy bosses; eight original enemies and five original bosses with readable body attacks and authored animation.
3. Heroes: ten distinct playable characters, including stationary collector and solo fighter; no extra player input.
4. Presentation: stronger rarity auras and companion rarity, player visibility, clear merchant stock placement, genuine carpet reveal, varied stands, collection motion and concise copy.
5. Seamless map transport, original map variants, native capture, simulation regression and Windows build.

## Reference boundary

https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Companions and https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Relics could not be retrieved during this review. Three purchases is our user requirement, not a verified quotation from the reference. Comparison guide: https://game8.co/games/Zenless-Zone-Zero/archives/475381 . Reference upkeep is explicitly excluded. New visual designs must be original.

## Balance decisions

Gold prices: wave 1 x1, wave 10 x3.916, wave 20 x10.196. Rerolls use the same stage factor and increase every refresh. The quota counts purchased bodies, not species; fusion does not refund a purchase.

Legacy bosses have a 2.5-times stronger base health coefficient before the new wave curve, and 35% larger bodies. Ordinary enemy health reaches approximately x19.4 at wave 20 before difficulty/build modifiers. Speed is bounded below x2.8; x20 movement would cause unavoidable collisions. Damage increases independently and existing per-hit safety remains.

## Implementation

- Eight enemies: Hopper Keg jumps; Charging Ram locks and charges; Coin Leech steals loose coins and returns them on death; Blast Puffer emits an expanding shockwave; Spore Bishop plants pools; Broom Skater sweeps delayed impact spots; Ribbon Hydra fires curved streams; Burrow Bell moves underground and erupts.
- Five bosses: Marquee Maw bites/throws; Mother Umbra lashes and plants shadows; Coin Widow attracts coins, volleys and shockwaves; Howling Organ emits paired waves; Suitcase Leviathan throws luggage and lashes. Existing bosses receive body attacks every third volley, stronger health, larger silhouettes and longer death effects.
- Ten heroes total. Seven new selectable roles reuse distinct existing friendly species art with fixed weapons; their mechanics, starting traits and shop themes are new. Root Oracle is stationary only in combat and collects across the arena; Lone Jelly cannot recruit and scales its personal attacks with waves. Both have automatic defensive pulses, not extra player inputs.
- Companion rarity provides +18/36/54% attack before existing special-recruit bonuses. Purple/gold stocks have colored rays and stronger light. Three pedestal styles have four authored poses each.
- Every four cleared waves, an original paper manta transports the brigade while floor materials dissolve. The same Unity scene remains loaded. Pause freezes transport; Reduced Motion uses a short stationary transition.
- Merchant stock is positioned below the actual merchant footprint; carpets unroll toward the player. Normal shops no longer show four unavailable fusion recipes; the dream market shows at most one recipe when its materials and currency are available.
- Player has an aqua rim and small overhead beacon. Directional loot boxes are removed. Collection selection cycles real character drawings and breathes other exhibits. Shop modifier copy uses words rather than icon-only shorthand.
- Music scene gains are lower; no audio clips or compositions are replaced in this review.

## Acceptance boundary

See K17_Checkpoint.md for actual editor/native/campaign results. Windows is the target; mobile remains an open device gate. Bot play and deterministic captures do not establish human fun, audio balance or art approval. Source sheets are not native 4K; Unity captures render at 3840 by 2160. Numeric economy changes are our own balance choices, not claimed exact Bizarre Brigade pricing.
