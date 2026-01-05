# K5 content and tuning guide

Candidate dated 2026-10-14 (Asia/Saigon). Original art and tuning; not extracted Bizarre Brigade assets or verified reference parameters. Final acceptance is recorded separately in `K5_Final_Checkpoint.md`.

## Stable content

K4's nine weapon IDs remain valid; seven new IDs make sixteen recruitable variants. Twelve signature concepts comprise Umbrella/Bell/Stamp plus Tea/Tether Bow/Scissors/Camera/Fish/Iron/Hammer/Alarm/Music Box. Key, mailbox, book and rune variants remain in the random purchase pool. No weapon choice before recruitment, ally limit, upkeep, active skill, dash or manual aiming added.

| Concept / stable ID | Actual pattern | Tier-specific rule or silhouette |
| --- | --- | --- |
| Umbrella / UmbrellaLance | Three beats, delayed forward opening and push | Wider opening; never grants bullet blocking |
| Bell / BellFork | Three beats, delayed circular wave | Exactly one weak echo |
| Stamp / DreamStamp | Per-source marks, three-mark burst | One neighboring mark, no burst chain |
| Tea / TeapotPopgun | Projectile impact leaves 1.8s heat, 0.4s ticks | Existing tier damage/body scaling; Coaster changes zone |
| Zipper / CrescentBow | Up to two distinct 25% links within 2 units | Three links; taller bow draw silhouette |
| Scissors / CloudScissors | Two narrow 65% cuts separated by 0.12s | Wider blade silhouette; no omnidirectional sweep |
| Camera / SleepCamera | Cone hits at most three, exposes 2s, +15% allied crit chance | Four targets; taller camera silhouette |
| Fish / TinFish | One outbound collision, harmless homing return after hit or 0.7s | Existing tier scaling; no duplicated return damage |
| Iron / DreamIron | Forward strike then 1.4s hot strip | Wider strip and iron sole |
| Hammer / RubberHammer | Third attack +50% windup/+35% cadence; 55% area strike | Heavy head silhouette; burning target adds 30% steam |
| Alarm / AlarmMortar | Locks floor position; 0.9s warning, single blast | Blast 1.5 → 1.8 radius; no post-launch tracking |
| Music / MusicBox | Soft homing notes; third note extends live mark to at least 0.6s | Box silhouette; Record adds finite third-note echo |

Body art stays separate from floating weapon grip/hands. Attack/impact pose outranks deterministic idle; heavy-third effective windup is shared between simulation and visuals. Tier silhouettes are presentation, not extra hidden stats. Body-material death changes collapse and fragment shape/color; pooled remnants remain bounded at 64.

## Six partial builds

- Harvest: Bell + Receipt already charges cadence from logical coin pickup with shared 0.35s cooldown. Carbon/Stamp can connect coin collection without inventing money.
- Office: Stamp + Ink changes cadence; Stamp + Lens can exploit exposed targets. Faster marks expire sooner; Lens trades attack speed for crit.
- Tea nightmare: Tea + Hammer uses burning targets and third-hit steam. Coaster widens a weaker field; Thermal links live marks to heat with per-target 0.9s cooldown.
- Chorus: Music + Record gives one third-note echo at 35%/70%. Bell's primary wave adds 25%/50% damage to live marked targets; bell echo cannot repeat the bridge. Music prolongs existing marks but does not create them. Record costs attack speed.
- Treasure: Bow + Ribbon creates a four-second chest window (+15% per stack, capped45%) and three healing per stack. It costs damage outside the window. Effective bonus damage/healing receives separate ledger entries, never double credited.
- Toy workshop: Spring + arena props gives one charge per prop every three seconds; Coupon converts prop kills to bounded healing under shared 0.6s cooldown. Attack speed/pickup penalties are real.

Tradeoffs are ordinary visible modifiers; there is no hidden full-set threshold. Six signature build read-model rows show whether minimal components exist; old non-signature stat suggestions remain available.

## Props and enemy composition

All six props use HD original sprites, charge pips, 1.2s warning and5s cooldown. Wave1 introduces bell/cart in expanded mode; waves3/5/7/9 introduce ink/hammer/mirror/music. Placement is seeded with safe clearance; tiny/crowded maps may omit a prop rather than trap the player. This deliberately differs from K4's one-bell introductory fixture, which remains tested when expansion is disabled.

- Stage bell: enemy-only area impact,2.4radius.
- Toy cart: swept four-unit lane; one hit per target per activation.
- Ink bottle: two-second persistent2.5radius slow field; refreshes0.6s chill while inside.
- Hanging hammer:1.8radius enemy impact, with matching warning radius.
- Sleep mirror: one40% projectile copied from a living projectile wielder, range6/lifetime1.5s; physical secondary, cannot copy signature/proc graph.
- Floor music box: nearby allied cooldown reduced0.2s in4radius; never grants a player skill.

Primary hit roots charge once per prop. Delayed scissors/mortar remain primary roots and retain on-kill proc eligibility in the same step. Heat/links/echo/mirror remain secondary and cannot charge or trigger generic procs. Spring is an explicit bounded bridge, not a generic secondary-hit exception.

Door unlocks wave2: direct directional front hits halved, flank unguarded. Magnet wave4: nearby coins pulled briefly, value/wallet unchanged. Mimic wave6: sleeps until hit/proximity, then0.8s wake warning before pursuit/contact. Curtain wave8:1.3s locked-position floor warning, then small capped hit. Early composition emphasizes old contact swarm, middle emphasizes door/magnet, late emphasizes expanded behaviors; wave timing/budget curves and existing modifiers still apply.

Bosses retain old IDs, bullet patterns and one-time K4 prop-layout shift. Half-health motif adds stage curtain pair, office staggered three stamps, or toy four separated impacts. Warnings1.5–1.8s; positions lock and clamp inside arena, do not move/block the player. Each new hazard hit is capped at8% maximum champion HP before armor; simultaneous hits are still allowed.

## Rendering and remaining gates

Field overlays use64-segment circles, tapered alpha fill, narrow directional cuts, zipper lines, hot strip and distinct danger colors. Hazard cues receive priority before decorative fields under the96-view pool. Projectile fish/note glyphs and nine grouped family sound cues are distinct; eight audio voices and per-family90ms aggregation remain bounded. No imported reference music.

Sandbox uses actual auto-combat, independent cloned assets/RNG, no rewards or discovery writes. Every family has an English rule description visible in existing Details and preview, pending K6 Vietnamese terminology/UI redesign. Final font/layout identity is explicitly not claimed here.

K7 must validate natural full-run balance, overlapping threat readability, mobile texture budgets/frame pacing, audio/music audition, animation video and real Android/iOS devices. Unlimited allies are preserved; current diagnostics are not a promise of unlimited stable frame rate. Source images are native1536×1024,1334×1179 and1448×1086, not native4K; Windows screenshots are Unity-rendered3840×2160.
