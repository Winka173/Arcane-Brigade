# K19 — encounters, evolution and dedicated relic art

## Implemented

- Bosses have independent left/right organ durability, each 20% of maximum boss HP. Damage arriving from the left/right accumulates on that side; these are directional body zones, not separate precision collider targets. An organ break cancels the current telegraph, briefly exposes/staggers the boss and emits the existing boss-phase feedback event once. Left break removes lashes and four volley shots; right removes falling props/pools, reduces shockwave radius and removes two shots. Both broken introduces warned core shockwaves while retaining a reduced volley. Existing delayed hazards remain visible and resolve normally.
- Large enemies/bosses intercept other enemies' projectiles, never their own. Interception consumes the projectile without split/bounce and deals at most 1.5% maximum HP. Small ground enemies yield around large bodies. Hostile lashes, falling props and shockwaves inflict bounded collateral damage once per enemy per field (12% normal / 1.5% boss HP maximum). Puffer deaths enqueue warned blasts; processing is bounded to eight per step and 32 queued, not recursive damage calls. Invulnerable companions remain unaffected; ordinary hostile shots still pass through them without the interceptor item.
- Two-star companions gain painted headwear, mantle and clasp; three-star companions gain a more elaborate mantle, pennant and weapon charm. Accessory choices vary by body material. These are modular additions attached to the existing animated body/weapon, not 42 newly redrawn character sheets. Twelve new transparent illustrations supply four boss organs and eight evolution accessories; none is assigned as a relic icon.
- Collection unit/boss previews render real `UnitVisual` animation to a private render target: idle, walk, attack and hit, with tier 1–3 companion previews. Enter/A cycles poses, T cycles companion tiers. Relics respond to clicks with a small pop and idle breathing. Locked exhibits remain hidden, no progress/stats are granted, leaving the page releases preview actors/textures. Weapon sandbox remains separate. Card labels are clipped/wrapped within their own bounds, with the repetitive Contract prefix removed only on cards.

## Enemy restoration

Relative to K18, for every wave: enemy health multiplier +20%, speed +8%, attack/contact scaling +15%. New body-behavior enemies change from 0.8 to 0.9 HP adjustment (combined health increase 35%). Boss bulk rises from 4–6.5 to 4.6–7.5 (combined HP increase approximately 38%). Density, wave cadence, prices, projectile speed ceiling (2.4), first-wave no-shooter rule, damage-per-hit safety cap and full heal after waves are unchanged. There is no renewed wave-4 spawn cliff.

## Dedicated relic art

All 68 relics now have different sprites. Replaced 58 borrowed/repeated/loot-placeholder illustrations with original item-specific artwork across four transparent atlases; retained the ten existing dedicated signature relic illustrations. Six extra atlas cells are unused reserves, not new gameplay items. No character, enemy or weapon artwork is assigned as an item icon. References are updated in `RunCatalog`, so world shop, collection and inventory share the replacement rather than a collection-only substitution.

`AssetRoleAudit.Run` exports `PreviewExports/K19/Asset_Role_Audit.csv`; the accepted audit is 68 unique entries. `Asset_Role_Before.csv` preserves the initial audit (its old-character heuristic initially undercounted retired character sprites). Focused checks prevent missing/repeated relic icons and known character/weapon art reuse. Source transparent rasters are 1254×1254 for each relic atlas and 1448×1086 for organs/evolution; runtime screenshots are 3840×2160, not a claim of native-4K source art. Slicing follows alpha-connected objects for relics and inspected region bounds for the irregular evolution sheet. All retain original source alpha, with BC7 Windows and ASTC mobile import settings.

## Verification

- `K19EncounterChecks.Verify`: 325 focused checks; K18 91; ink/balance 268; shop/carpet 2247; K17 2474; K16 220; K15 143; K14 57. K12/K11/K10 regression suites also pass; exact counts are in the Unity release log.
- Windows build succeeds, zero errors, zero warnings, 684148167 bytes. Play `Builds/FullRun/ArcaneBrigade.exe`; sharing requires the entire `Builds/FullRun` directory.
- Native 3840×2160 diagnostic runs: `PreviewExports/K19/ReleaseFinal` and `ReleaseReduced`, 15 assertions each, including actual companion attacks breaking both boss organs, exposed-core shockwave, three evolution tiers, no profile mutations and preview cleanup. Includes relic collection screenshots throughout the list. Fixtures use isolated saves, deliberately selected allies and high single-hit damage to demonstrate break transitions; they are not normal-run win-rate or performance measurements.
- Fixed-seed movement/purchase bot campaign: 3 victories, 7 defeats, no timeouts across ten heroes. This is an observation for one seed/policy, not a player win rate or proof of equal hero balance. Compared to the previous 4/10 K18 sample, difficulty restoration is measurable but individual outcomes are not controlled causal estimates. See `Campaign.csv` and `Economy.csv`.
- Runtime DLL SHA256: `247ABA694DBCB6F8478B8A1F12DD23E2A8E05CC64B9AB78FC4A8F7F8AD54BAE1`.
- `resources.assets` SHA256: `193E8CB50F6982C87BE950E1E0B1FF49AF22E3E3246AC278691C2D7A7AAE65BB`.

No audio source files or the other agent's audio prompt/contract edits are included. Mobile device acceptance, subjective audio review and publishing remain outside this checkpoint.

## Manual acceptance

1. Compare each relic silhouette with its name in the world shop and inventory, including uncommon/rare/mythic auras and reroll travel.
2. Fight from alternate boss sides: confirm the visible organ falls off, the matching attack is disabled, core warnings remain readable and the boss does not die simply from two breaks.
3. Pull a shooter behind a large enemy and lure a puffer into a pack; confirm collateral damage is useful without becoming an infinite gold farm.
4. Compare tier silhouettes during crowded combat and collection, including Reduced Motion. Verify movement, attack and hit previews and leaving/reopening the page.
5. Play waves 1–5 and a full late-game run to judge the restored difficulty beyond automated fixtures.
