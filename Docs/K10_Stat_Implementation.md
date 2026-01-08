# K10 — stat implementation and run persistence

All acquired item stacks stay in the run snapshot across halls and endless. New run clears stacks/roster build. Companion traits derive from the current roster, so merging/altar changes support sources; they are not copied twice into save. Profile does not grant stat growth.

## Added modifier categories (30)

| Category | Actual consumer |
| --- | --- |
| Score multiplier | Snapshot multiplier settled/bought, RunMeta.Score, receipt |
| Outgoing collision damage/frequency | Automatic contact damage by friendly units, independent cooldown, cause ledger |
| Flat melee/ranged damage | Added to base weapon damage before existing damage multipliers |
| Attack range | Weapon reach and projectile travel lifetime |
| Dodge | Separate combat RNG roll per incoming hit,0–60%, no generic i-frame |
| Income | Hall income and endless entry credit, bounded resource counter |
| Bullet pierce | Swept hit victim tracking,0–64 |
| Bullet split | First impact creates1–6 configured branches; 35% damage,70% visual/collision size for stat-generated children; no recursive split |
| Bullet bounce | Chance0–100% to seek a next unhit enemy,8 random redirects; authored family bounces remain distinct |
| Bullet size | Both rendered width and swept collision radius ×0.25–4 |
| Bullet speed/distance | Velocity and lifetime adjusted without counting speed as free range |
| Knockback | Automatic weapon push distance; no added manual input |
| Player/companion size | Actual render size and unit collision radius; scaled companion size not silently swallowed by legacy render cap |
| Enemy count/health/speed | Wave budget/spawn interval and enemy compile; stronger late health curve |
| Heart healing | Heart recovery amount, not all healing procs |
| Chest quality | Chest coin value factor; treasure detection contributes25% of its bonus |
| Recruit/relic discount | Separate gold offer prices; not reroll price or blood discount |
| Shop rarity | Item weights; Luck adds25% of its bonus to rarity weighting in K10 |
| Health/damage/speed/armor/regen corrections | Extra multiplier on corresponding compile; regen correction affects continuous regen, not all proc heals |

There are18 prior ItemEffect values and30 appended ones, preserving serialized enum indexes. UI names, shop descriptions and source readmodel updated.30 original contract relic definitions make every category purchasable; companion traits additionally cover these categories. Enemy-strength contracts reward score so they are not only unexplained penalties.

## Bounds / honest differences

- Split branch cap6, projectile size cap4×,96 emitted projectiles per shot and4096 live projectiles are technical safeguards; no cap imposed on owned allies or item stacks.
- Negative armor now increases damage received up to2× before the existing per-hit safety ceiling. Incoming hits can coincide; one hit is still bounded by14HP or25% maximum HP, whichever is lower.
- Late HP adds factor `1+0.055×max(0,wave−10)` on top of existing wave/difficulty scaling. Speed/weapon damage already scale; capped incoming damage and slow hostile bullets remain for readability.
- Flat negative damage cannot heal enemies; unspecified reference cap/formulas are not invented. No generic stat transfers, item-by-item conditionals, every reference protagonist or reference asset clones are claimed.
- The K9 comparison document is a historical baseline. K10 implements previously missing categories, but coefficients/scopes/technical bounds are Arcane design, not exact reference parity.

## Original content

32 species ×4 imported sprite frames +24 weapon sprites +4 dedicated bond icons.39 total species,40 total weapon definitions. Eight bonds contain5/5/5/4/5/5/5/5 species; owned roster remains unlimited. See manifest and gallery for identities/prompts. Original existing species stay addressable in old saves; tags are rebalanced by current content rather than duplicating identity.

## Depth and effects

Consistent soft contact and directional silhouette shadows for units, floating weapons, shop actors/goods, projectiles, pickups, landmarks and arena toys. Light/emissive particles are not treated as opaque shadow casters. Elevation softens shadows. This is layered2D projection, not physical ray tracing. Center-pivot body placement fixed instead of merely painting a shadow behind a misplaced sprite. Painted attacks tinted by element; existing hit/death/material cues, windup/recovery and dust preserved.
