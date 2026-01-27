# K21: boss encounters, reciprocal contact and collection layout

## Implemented

- All ten bosses use new detailed core art: five transparent atlases, eight distinct drawings per boss. Separate K20 organs still have rest/charged/scar sprites and break consequences. This is an eight-drawing state set, not continuous hand-drawn skeletal animation or native-4K source art.
- Runtime boss body scale is 2.1x the catalog size (2.3x for Cradle/Leviathan), versus the previous 1.35x. Boss HP is 60% higher than the previous formula; contact/weapon damage +18%, movement speed +8%. Existing incoming-player per-hit limits remain.
- Every fourth boss attack uses one of two named signature patterns, alternating over successive cycles. Twenty variants across ten bosses use existing real damage primitives: timed lashes, falling objects, pools, expanding impact waves and actual jumping movement. They are not twenty wholly new VFX systems. Normal organ attacks remain between signatures.
- Signature target is locked during a 1.15-second body windup. Jump attacks interpolate position, lift the body, cast a ground shadow and land at the locked destination; airborne bosses do not deal contact damage. Pause freezes their simulation clock. Rest removes hazards.
- Floating/orbiting organs and lunging lashes use a shared simulation pose clock for projectile origins. Destroyed organs remain unavailable. Body drawing dimensions are normalized across poses and their rotated height is bounded.
- Champion/enemy contact resolves both directions after the contact windup, even if the outgoing collision kills the enemy. Each enemy has an independent outgoing cooldown; faster collision builds can hit again during the enemy's own recovery. Multiple enemies may still hit simultaneously. No added player invulnerability frames.
- Every champion has at least 3 base collision damage before run modifiers. Existing personal-damage relics now also scale collision. Notebook collision rows show actual compiled damage and interval instead of only bonus totals.
- Existing relics already support personal HP, armor, damage, dodge, collision damage/frequency, melee, ranged, projectile count/pierce/split/bounce. No duplicate relic icons or redundant items were added merely to recreate these existing modifiers. Stacks last for the run, not across deaths.
- Collection: smaller local text styles, explicit interior padding, fitted card captions, separate heading/detail bounds, scrolling descriptions, compact pose controls, no disabled gear-try action outside the weapon tab. Animated previews and break/fix controls remain.
- Reroll station is near the left stock slot, not behind the merchant. Reroll begins at 2 gold at each new rest boundary and escalates only within that boundary, including the special/normal shop sequence. Stock price inflation is unchanged. Description panel is lowered below merchandise prices.
- Floor uses mirrored continuous sampling (including grain) so non-seamless painted tile edges do not butt against different edges. This fixes discontinuities, not the visible repetition of a mirrored pattern.
- Removed boss aim lanes/landing ellipse, optional ally-radius guide, exposed-target brackets, spectacle floor boundaries, pre-impact hazard outlines and boomerang/disc decorative shells. Actual projectiles, attack waves, lashes, active shadow pools, player underfoot identity and shop rarity effects remain visible.

## Reference

The community [Bizarre Brigade relic list](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Relics) documents protagonist-specific bonuses and collision-frequency tradeoffs (for example Hard Hat), alongside melee/ranged/collision bonuses. [Dream Seekers](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Dream_Seekers) also distinguishes protagonist collision and ranged/melee identities. Search-index excerpts were accessible; direct full-page requests failed in this environment. This is reference for build categories, not a claim that every listed effect has been copied or exhaustively audited.

Art prompts and source limitations: `Docs/K21_Boss_Art_Prompts.md`. Generated using painting pass from this project's own K20 organ-art reference, not extracted third-party sprites.

## Validation

- `K21BossEncounterBuilder.VerifyAndBuild`: focused boss/contact/build-support checks, K20 anatomy checks, K19 encounter checks, K10 stats, K16 motion, K4 merge, K17 content, K18 experience and K8 merchant checks, then Windows build.
- Native Windows capture flags: `-arcane-k7-capture -arcane-k11 -arcane-k13 -arcane-k16-art -arcane-k17-art -arcane-k17-presentation -arcane-k18-experience -arcane-k20-anatomy -arcane-k21-bosses -capturePath <isolated-directory>`.
- Separate native contact/shop fixture: replace the final two boss flags with `-arcane-k21-contact`. Both fixtures use diagnostic storage, not the player's real profile.
- Historical standalone `K8UIChecks` still asserts Vietnamese unlock text even though the game defaults to English; historical `K32Checks` stops on its obsolete fixed roster requirement. They were attempted and fail those pre-existing assumptions, and are not reported as passing. Focused current tests cover the changed tooltip/reroll behavior instead.
- Audio sources from main are retained; no audio regeneration or new subjective listening approval is claimed. Android/iOS device gates and human gameplay/balance approval remain open.

## Still separate work

Ordinary-enemy redraws with recognizable evolutionary art and changed later-wave movesets have not been implemented in this batch. Do not interpret the boss rework or current stat scaling as completion of that request.
