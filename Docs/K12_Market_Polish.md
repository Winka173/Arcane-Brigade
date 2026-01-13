# K12 market and animation polish

## Scope

This checkpoint extends the K12 core, without touching the separate music/audio agent's assets or implementation. Windows is the acceptance platform; physical Android/iOS gates remain open.

## Economy

- Gold stock base price is multiplied by `1 + 0.07 * (wave - 1)`, rounded up, then receives the existing scoped discount and final minimum-one rounding.
- Special-market stock uses the gentler `1 + 0.035 * (wave - 1)` multiplier. Eggs and current-HP offers both participate; HP purchases retain the existing nonlethal transaction guard.
- Wave-one prices retain the previous calculation. Rarity premiums and Luck-weighted rarity selection are unchanged. Existing displayed offers retain their prices on resume; inflation applies to newly generated offers, not purchases of already displayed stock.
- These are Arcane Brigade tuning decisions, not a verified Bizarre Brigade price formula.

## Choreography

- Carpet unfurls first; merchant enters after 0.18 seconds; stock follows at 0.70/0.82/0.94 seconds with launch arcs and damped landing overshoot. Interaction waits until 1.55 seconds.
- Departure retrieves stock with staggered timing, then withdraws the merchant and rolls the carpet. The doorway and reroll fixture descend at their own positions rather than flying to the merchant.
- Special-to-gold handoff waits 1.60 seconds before regular shop arrival. Next-wave combat waits 1.35 seconds for departure; both retain the loaded gameplay scene, without a loading overlay.
- Pennants, floor-contact shadows and directional arrows fade with the carpet; no stranded floor shadows. A world-space ribbon leads into the lower doorway. Purple wisps preserve special-market identity.
- Merchant landing has a ground ripple and timed fragments. Bosses have protected visible fall/rise entrances and a landing event; normal enemies cannot cause contact damage during their windup.
- Reduced-motion settings suppress displacement/oscillation; timing remains tied to the paused presentation clock.

## Related polish in this checkpoint

- Centered boss-health values, portrait/HP/wallet alignment and fixed shop-price anchoring.
- Larger breathing companion stock versus floating relic stock, with distinct ALLY/RELIC badges.
- Four original dream-garden props, dimmer floor dressing, species-specific motion cadence, material-dependent death fragments and tier chest crests.
- Appended chest-opening reach stat and three run relics with actual chest/kill proc effects. Separate from treasure-detection probability and coin pickup range.

## Evidence and limits

Focused checks: `PreviewExports/K12/Core_Checks.txt`, `Polish_Checks.txt`, `Market_Polish_Checks.txt`; Windows build report: `PreviewExports/K12/Windows_Build.txt`.

Native 3840x2160 fixtures and notes live in `PreviewExports/K12/Player`. The capture exercises naturally spawned wave-five hordes, boss wave-four/eight entrances, staggered shop arrival, special-to-gold departure and same-scene next-wave combat. Explicit saved fixtures are not proof of a natural full-run victory. Frame samples are local diagnostic evidence, not mobile/stress certification or human art/audio approval.

Final Windows build: zero errors and zero warnings. 1,862 focused assertions pass (121 core, 1,223 polish, 496 market/economy, 22 transitions). Native executable exits zero with 39 passing checks and 17 framebuffer captures. Wave-five fixture reached 48 live enemies; 1,319 local frame samples had 16.67ms median and 16.68ms p95 on RTX 5070 Ti. Artifact fingerprints are in `PreviewExports/K12/Market_Artifact_SHA256.txt`; the earlier K11 manifest is historical.

The final legacy movement/shopping bot lost all 15 runs at waves 4–6. This is a balance warning, not a passing gate: the increased horde rate, bosses and economy need playable-route/human tuning. The legacy bot buys only two gold-stock batches per wave, ignores special stock and does not prioritize safe stat tradeoffs. Do not infer that the full run is balanced or unwinnable from its results, or reuse the earlier pre-batch victory counts. The actual final CSV is retained alongside the captures.

Raster sources are original generated assets, not extracted Bizarre Brigade sprites or native-4K source textures. Species cadence/puppeting is not a replacement for individually redrawn animation sheets for every character.

K13's requested expanded weapon collection, merge approach preview and distinct slower hero starting speeds are separate follow-up scope; do not mark those complete from this market checkpoint.

Reference audit: [Bizarre Brigade overview](https://www.icy-veins.com/zenless-zone-zero/bizarre-brigade), [relic catalog](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Relics), [companion catalog](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Companions). These support the inspiration/content audit, not exact animation timings or our inflation coefficients.
