# Balance audit — After round 2 (2026-10-19 13:26)
1 VU = +1% team damage. Rarity stat targets C/U/R/M/P = 12/19/28/40/55 VU, tolerance ±30%.

## Stat-only item budget by rarity (stat + utility items; mechanic and score items listed in Items.csv)

| Rarity | n | min | median | max | target | within ±30% |
|---|---:|---:|---:|---:|---:|---:|
| Common | 36 | 6 | 10.5 | 14 | 12 | 28/36 |
| Uncommon | 40 | 9.6 | 15.84 | 24.1 | 19 | 30/40 |
| Rare | 16 | 14.25 | 23.75 | 31 | 28 | 14/16 |
| Mythic | 3 | 33.98 | 41.53 | 46.9 | 40 | 3/3 |

## Flagged items (7)

- CurioLiarMask (Mythic, Damage/T 1): 100 VU vs 40 → STAT_PART_OVER
- CurioUrchinBall (Rare, Armor/C 0.6; MoveSpeed/C 0.1): 50.2 VU vs 28 → STAT_PART_OVER
- CurioSplitKernel (Rare, Heal/C 3): 45 VU vs 28 → STAT_PART_OVER
- k10-armorcorrection (Uncommon, ArmorCorrection/C 0.4; Armor/C 0.07): 9.6 VU vs 19 → UNDER
- k10-bulletbounce (Uncommon, BulletBounce/T 0.65): 9.75 VU vs 19 → UNDER
- k10-bulletdistance (Uncommon, BulletDistance/T 0.6; AttackRange/T 0.14): 10 VU vs 19 → UNDER
- echo-postage (Mythic, Projectiles/T 2; AttackSpeed/T -0.3): 50 VU vs 40 → STAT_PART_OVER

## Max HP gifts and HP costs

- k10-collisiondamage Common: +0 max HP, −5 max HP, net 10.65 VU
- k10-attackrange Common: +0 max HP, −5 max HP, net 11.25 VU
- k10-income Common: +0 max HP, −5 max HP, altar price 8 current HP, net 8.75 VU
- k10-bulletpierce Common: +0 max HP, −8 max HP, net 14 VU
- k10-bulletsize Common: +0 max HP, −2 max HP, net 6.5 VU
- k10-enemycount Common: +0 max HP, −5 max HP, net -6.75 VU
- k10-enemyspeed Common: +0 max HP, −5 max HP, altar price 8 current HP, net -8.75 VU
- k10-hearthealing Common: +0 max HP, −2 max HP, net 6.5 VU
- k10-relicdiscount Common: +0 max HP, −5 max HP, net 6.65 VU
- k10-damagecorrection Common: +0 max HP, −5 max HP, net 10.25 VU
- CurioCoralBroth Common: +0 max HP, −6 max HP, net 10.5 VU
- k10-scoremultiplier Uncommon: +0 max HP, −6 max HP, altar price 13 current HP, net -4.5 VU
- k10-meleedamage Uncommon: +0 max HP, −6 max HP, net 16.5 VU
- k10-rangeddamage Uncommon: +0 max HP, −6 max HP, altar price 13 current HP, net 16.5 VU
- k10-dodge Uncommon: +0 max HP, −6 max HP, net 14.7 VU
- k10-bulletsplit Uncommon: +0 max HP, −8 max HP, net 18 VU
- k10-bulletbounce Uncommon: +0 max HP, −0 max HP, altar price 13 current HP, net 9.75 VU
- k10-bulletdistance Uncommon: +0 max HP, −0 max HP, altar price 13 current HP, net 10 VU
- k10-companionsize Uncommon: +0 max HP, −0 max HP, altar price 13 current HP, net 13.75 VU
- k10-enemyhealth Uncommon: +0 max HP, −6 max HP, net -9.5 VU
- k10-recruitdiscount Uncommon: +0 max HP, −0 max HP, altar price 13 current HP, net 9.6 VU
- k10-armorcorrection Uncommon: +0 max HP, −0 max HP, altar price 13 current HP, net 9.6 VU
- K26Defense3 Uncommon: +20 max HP, −0 max HP, net 22.2 VU
- CurioDaylightBonnet Uncommon: +24 max HP, −0 max HP, net 18 VU
- CurioCourageJuice Uncommon: +0 max HP, −10 max HP, net 15.5 VU
- guardian-canopy Rare: +0 max HP, −0 max HP, altar price 21 current HP, net 23.25 VU
- twin-seal Rare: +0 max HP, −0 max HP, altar price 18 current HP, net 30 VU
- long-atlas Rare: +0 max HP, −0 max HP, altar price 18 current HP, net 20.4 VU
- altar-ember Rare: +0 max HP, −0 max HP, altar price 18 current HP, net 25.9 VU
- OctoberRelic3 Rare: +25 max HP, −0 max HP, net 23.75 VU
- CurioOilyLozenge Rare: +20 max HP, −0 max HP, net 15 VU
- CurioPetalTonic Rare: +55 max HP, −0 max HP, net 23.25 VU
- starlight-quilt Mythic: +50 max HP, −0 max HP, net 37.5 VU
- echo-postage Mythic: +0 max HP, −0 max HP, altar price 32 current HP, net 50 VU
- tarnished-crown Mythic: +0 max HP, −0 max HP, altar price 27 current HP, net 33 VU
- CurioWishWick Mythic: +0 max HP, −12 max HP, net -21.5 VU
- CurioLiarMask Mythic: +0 max HP, −0 max HP, altar price 27 current HP, net 100 VU
- CurioFeastBucket Mythic: +40 max HP, −0 max HP, net 21 VU
- CurioSixfoldRoot Mythic: +0 max HP, −0 max HP, altar price 27 current HP, net 0 VU

## Companion support modifiers (tier 1; tiers x2/x4)

| Rarity | n | min | median | max |
|---|---:|---:|---:|---:|
| Common | 31 | -5.75 | 0 | 11.25 |
| Uncommon | 27 | -3.75 | 0 | 1.89 |
| Rare | 17 | -0.75 | 0 | 6 |
| Mythic | 11 | -0.75 | 0 | 1.98 |
| Rainbow | 8 | -5.75 | 0 | 0.53 |

## Bonds (VU of the reward at 2/4/6 members)

VU per member at first threshold: min 0.56 (rain-script), median 2.16, max 6 (rune).

## Heroes

Base HP min/median/max 60/105/225. See Heroes.csv for DPS and kit modifiers.

## Enemy HP multiplier (compiled / species base)

| Difficulty | w1 | w5 | w10 | w15 | w20 | max wave-to-wave step | ratio to previous difficulty at w20 |
|---|---:|---:|---:|---:|---:|---:|---:|
| 0 Peaceful | 0.95 | 1.45 | 3.24 | 8.6 | 21.7 | 1.26 | - |
| 1 Wandering | 1.08 | 1.88 | 4.63 | 12.28 | 31 | 1.26 | 1.43 |
| 2 Restless | 1.21 | 2.31 | 6.01 | 15.96 | 40.3 | 1.26 | 1.3 |
| 3 Nightmare | 1.41 | 2.95 | 8.1 | 21.49 | 54.25 | 1.26 | 1.35 |
| 4 Sleepless | 1.65 | 3.74 | 10.64 | 28.24 | 71.3 | 1.28 | 1.31 |
| 5 Beyond the Lamp | 1.93 | 4.68 | 13.65 | 36.22 | 91.45 | 1.31 | 1.28 |

Economy constants: starting gold 35, wave income 15 + wave, interest gold/10 capped at 10, reroll (2 + 2n) x max(1, stage factor / 2) since 19/10/2026 (was 2 + 2n), gold offer = price x 0.85 x (1 + 0.4 rarity) x stage factor, altar offer = altar price x (1 + 0.25 rarity). See Economy_Static.csv and the Sim outputs.
