# K16 crafted characters and build identity

## Delivered

- Five priority characters redrawn in coherent, simplified ink/cel style; 60
  separate poses with idle/blink, walk, anticipation, release and recovery states.
  Shared reference scale and ground pivots; existing portraits/weapon mounting,
  no-arms design and Reduced Motion remain. Full roster redesign is not claimed.
- Melee approaches from the near side with an anchored personal slot, not a
  continuously rotating destination. Reanchors after large displacement; holds
  its firing position during close windup. Range acquisition includes enemy radius.
- Windup retains its valid target; dead targets can be reacquired without resetting
  impact/cooldown. Aim turns at up to 300 degrees/s with short bounded motion lead.
  Pursuit still clamps to the champion leash and separation/return rules remain.
- Yo-yo pulls/slows ordinary victims; tier2/3 also slows up to 2/3 enemies along
  the tether corridor. Bosses are not pulled. Original one-hit-per-victim return
  ownership is preserved; no added recursive projectile spawning.
- Three sewing needles: tier2 stitches one neighbor, tier3 stitches two, each
  link deals 18% of projectile damage with secondary cause attribution, brief slow
  and actual animated inter-enemy thread. Fragments cannot trigger stitching.
- Projector tier2/3 gently gathers ordinary foes at its locked spotlight center
  while slowing them briefly. Knockback resistance applies; bosses never move.
- Magnet cap now scales 4/5/6 by tier; spring hammer keeps primary-hit attribution
  for relic interactions and the real critical flag. Ribbon's second cut follows
  the wielder's current position instead of being abandoned behind a moving ally.
- Short actual mechanic descriptions exposed through existing description code.
  No audio changes, new active controls, permanent buffs, recruiting-rule changes
  or redesigned UI styling.

## Validation

Windows candidate, build evidence in PreviewExports/K16. 4,033 assertions: 220 K16
plus K15 and focused K8/K10-K14 regression suites. Assertions include full authored
state selection, anchored melee slot, dead-target reacquisition, bounded aim,
tier-gated sewing/projector effects, secondary stitch attribution and heavy crits.
648 controlled weapon samples and 33 live authored-roster encounter measurements
cover melee/ranged/mixed and early/mid/late tiers. They are not player win rates.

Native checks: 63 art-gallery, 27 weapon/market/feel, 44 horde/boss/transition and
27 independent reduced-effects checks pass (161 total).
Art gallery uses explicit paused clocks/positions and no HUD overlay;
weapon gallery uses durable stationary targets. Horde fixture uses natural enemies
and attacks, with a saved tier2 party and movement-only circle driver.
Screenshots and frame intervals are evidence, not human art/fun approval or mobile
certification. Windows build succeeded with zero errors/warnings.

Dense-wave observation: 35 peak enemies, 1,202 measured frames, median/p95
16.67/16.68 ms on RTX5070Ti with concurrent capture/diagnostic processes. Not a
maximum army or minimum-spec performance certification.

Final 21 campaign routes after the melee anchoring fix: 16 victories, 5 defeats,
zero timeouts. Normal Lantern/Postmaster/Seer each complete 3/3 routes and reach
three endless halls to hall23. Harder-route failures are retained in the CSV;
this is a fixed-seed movement/purchase bot, not a player win-rate estimate.
Minimum-spec/load tests, bespoke mouth/eye layer rigs for the whole roster and
physical mobile gates remain open.
