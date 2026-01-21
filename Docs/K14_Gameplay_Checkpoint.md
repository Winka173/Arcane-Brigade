# K14 gameplay candidate

## Delivered

- Tactical companion targeting accounts for assigned allies and committed
  overkill, keeps a short hysteresis bias and distributes claims. The per-tick
  cache avoids rescanning the entire army for every enemy candidate. Melee uses
  separated stable engagement slots; ranged companions seek firing distance.
  Existing leash, resting/patrol, immortality and movement-only controls remain.
- Three new themed recruits: Coil Kettle (hearth, healing/knockback versus speed),
  Thimble Moth (archive, attack speed versus damage), Reel Owl (astral dream
  recruit, reach versus attack speed). Four painted poses each, no arms.
  The new dream recruit is theme-locked; old dream-pool eligibility is preserved.
- Six new weapons with actual mechanics: returning homing yo-yo/tether, capped
  magnetic pull, three piercing/slowing sewing needles, charged spring splash,
  lingering dream projector and two delayed ribbon cuts. Three melee/three ranged.
  Special-only projector and separate spool/needle sprites. Existing stat and
  star scaling apply; returning melee projectiles now scale visually with stars.
- Species peek/nod/tuck/wiggle and material gait variation, independently posed
  upper/lower body, new weapon charge/release/recovery poses, Reduced Motion.
  This is not a claim of redrawing all existing characters into bespoke clips.
- Wave-five packs share an arrival anchor, occasional shield-front/support-back
  composition and contact flankers. Wave one still excludes ranged attacks.
- Bosses retain their distinct patterns/telegraphs, rest after each volley and
  expose a longer counter window every third volley, receiving 15% more damage.
  Below half HP the pattern interval shortens; bullets remain slow and visible.

## Balance methodology

Weapon_Balance.csv contains 648 controlled samples: 54 weapons, tiers 1-3,
single/eight neutral targets, stationary/moving. These isolate delivery and
crowd damage; they are not real boss fights or player win rates. Recruit_Balance
records all 42 recruits at tiers 1-3, authored currency/price, scoped effects and
passives. Actual shop prices retain stage/rarity/discount modifiers.

Initial measurements reduced spring hammer damage 29 -> 24, magnet 13 -> 10
with a four-target limit, and adjusted needles 4.5 -> 5. No indiscriminate DPS
buff was applied to control/weakness/support weapons merely for low single-target
numbers. Weapon_First_Pass is historical and predates the neutral-enemy correction;
do not treat it as an identical-fixture causal comparison.

Campaign_First_Pass records the first 21 full routes: 19 victories/2 defeats,
suggesting the improved AI made the higher difficulties too forgiving for that
policy. Final tuning adds a difficulty-only quadratic durability factor
1 + 0.07 * difficulty^2; normal difficulty stats and the per-hit safety cap remain.
Movement_Bot_Balance.csv records the final repeated routes using the same seeds
and movement/purchase policy. Victories include three endless halls to wave 23.
Automated results do not substitute for human playtesting or fun/feel approval.

## Integration

UI/audio owned by the other agent are not changed/staged. K14_UI_Content_Contract
provides names and short summaries through WeaponDefinition.GameplaySummary.
Existing Motion/Family sound mappings remain usable; bespoke new weapon audio
can be added independently. Windows candidate first; physical mobile gates open.

Verification reports and native screenshots are in PreviewExports/K14.

## Validation

- 3,677 unique assertions across K14, focused combat, K10-K13 and melee suites,
  including spring-hammer impact knockback and full-resistance regression checks.
- 648 weapon delivery measurements; 126 recruit/tier support rows.
- Final campaign: 15 victories, 5 defeats, 1 controlled timeout over 21 routes;
  normal difficulty wins: Lantern 3/3, Postmaster 3/3, Seer 2/3. Hardest Seer
  route reached the harness boss-overtime limit at wave 4; it is not hidden as a win.
- Native Windows: 22 market/new-weapon/animation checks and 39 dense-wave/boss/
  no-loading transition checks, no runtime errors. Screenshots render native 4K.
- Dense-wave fixture: 32 peak enemies, median/p95 16.65/16.68 ms on RTX 5070 Ti;
  not a maximum-roster/mobile/minimum-spec certification.
- Final build success/errors/warnings and binary SHA256 are recorded beside the
  diagnostics. Source art and screenshots inspected; subjective human feel remains
  open. The measurement fixtures use stationary durable targets, not ordinary runs.
