# Animation polish implementation ledger

## Direction and sequence

1. Add deterministic presentation-only locomotion inertia, independent rest gestures, weapon settling and material-specific body response. Preserve simulation timing, collisions, damage, range, projectile count and economy.
2. Add original painted material impacts, projectile event accents, loot/chest reveal, dust and light response. Bound pools and event rates; distinguish hostile projectiles from cosmetic particles.
3. Improve shop acting, merchandise weight, weapon articulation and fusion personality without moving prices or separating rarity effects from merchandise.
4. Add boss phase acting, architectural response, environmental disturbance and dawn release. Never add targeting circles or counterfeit attacks.
5. Exercise collection poses, accessibility, pause, pooling and Windows native captures. Record implemented work and outstanding bespoke-art work explicitly.

## Acceptance principles

- Procedural articulation is not labelled hand-drawn animation. Additional pose art is required where the source has no usable limb, face or mechanical part.
- Deterministic local phases do not consume combat/shop RNG or write narrative/gameplay saves.
- The animation hierarchy is actor > hostile shot > interaction > cosmetics. Under crowd pressure reduce ambient/ally decoration first.
- Reduced Motion removes ornamental displacement, Reduced Effects reduces particle work, Reduced Flashes avoids bright pulsation. Pausing freezes gameplay presentation; menu/collection may continue their own preview clock.
- No new controls, fake pickup values, artificial projectile splits, stat effects or unreleased Chapter 2.

## Initial audit

Existing source already covers basic idle/blink, four-frame walk, authored attack phases, pursuit dust, rarity aura, shop focus, rolling-carpet mesh, purchase/fusion, material death treatments, collection pose buttons, boss organs and nightmare layers. This pass improves those paths rather than claiming they were absent. Many unit bodies remain four-frame or hinged sprites; not every actor has bespoke animation clips.

Windows verification and scope boundaries are recorded in `PreviewExports/AnimationPolish/Gate_Summary.md`. Android/iOS device gates remain open.

## Implemented presentation paths

| Group | Existing foundation retained | This pass |
| --- | --- | --- |
| Champion | Blink, walk, upper/lower puppet, hit flash, feet beacon | Bounded acceleration inertia, braking/turn follow-through, independently phased idle acting, ten mechanic-specific gesture accents, health-change/recovery response, subtle low-health posture |
| Companions | Independent patrol/rest, pursuit, landing, real tier evolution | Rest attention toward champion, weapon follow-through, material stiffness, tier-reveal flourish by material; none changes targeting or pursuit rules |
| Weapons | Thirteen attack motion families, bow draw, signatures | Weighted recovery settling, bowstring vibration, bow flex and camera compression shader, actual separately hinged scissors blades with reduced fallback; original source weapon remains usable by death-pose copying |
| Hits and projectiles | Slash/cast frames, crit, collision and material death | Original four-frame painted impact families, real pierce/bounce/return/split accents, bounded sampled-path trails and moving orb core, local spell-light response |
| Enemies | Authored windup/release/recovery, evolved variants, jump/body lift, affixes | Charger anticipation, loaded thief posture, hopper midair acting and material-weight response without moving collision bodies |
| Bosses | Individual entrances, organs/scars, attack motifs, staged deaths | Health-act body cadence, organ secondary motion, final-hall columns opening by act, impact-driven architecture and lamp response |
| Shop | Focus spring, purchase/failure, actual rolling carpet, inventory staging, rarity tracking | Two-part merchant portrait acting, attention/speech/denial/cheer on separate upper/lower parts, merchandise focus settle; prices/economy unchanged |
| Map | Layered floor, props, nightmares, soft shadows, dawn | Player mist wake, impact clearing in low fog, nearby shelf dust from heavy events, short local elemental illumination with readability/accessibility guards |
| Loot/UI/collection | Coin spin/glint/bounce/attract, wallet/system-icon animation, collection pose controls | Original four-pose painted chest and actual opening reveal, resting chest anticipation, clearer collection render target, immediate pose-switch trigger reset, boss PHASE preview control |

These are improvements to existing paths, not a claim that every earlier suggestion had no implementation or now has a bespoke drawn clip. No fake extra projectiles, currency, damage, animation-driven movement locks, target circles or chapter unlocks were added.

## Art provenance

Original painting pass assets copied into `Assets/ArcaneBrigade/Art/Nightmare/`:

- `TactileImpacts_v1.png`: generation `exec-ac99539f-afed-4081-9c79-5d68889f4502.png`. Prompt: a transparent 4x4 atlas of four-frame paper/fabric, brass/glass, wax/wood and dust impacts; hand-painted navy/burgundy/brass toyshop style, no text, circles or background. Rejected as runtime source because uniform cell slicing clipped particle tips.
- `TactileImpacts_v2.png`: accepted framing edit `exec-5ea5c27e-fa78-498d-a268-e99dd2733813.png`. Preserve the same sixteen drawings, shrink clusters into equal padded cells, keep all particle tips and temporal progression. The output still has slightly irregular cluster extents; Unity connected-component bounds preserve the complete groups instead of cutting on nominal grid edges. This is not external raster editing.
- `LivingChest_v1.png`: generation `exec-b824c8aa-ed6e-4245-8260-a5fddbd835fb.png`. Prompt: four transparent horizontal poses of one original navy lacquered wooden toyshop chest with brass star keyhole and burgundy lining, closed/latch anticipation/mid-open/fully open, consistent base, broad clean ink shapes and detailed wood/metal; no words, weapon, character, ground or targeting halo. Replaces the coarse closed chest icon in the run catalog and supplies the opening animation. No loot probabilities change.

All generations live under `ArtSource/`. Runtime does not depend on that external directory. Shader flex, cropped merchant/scissor parts and springs are procedural articulation of existing sprites, not newly hand-drawn faces or limbs.

## Deliberate limits / remaining bespoke art

Follow-up: `Docs/Character_Acting_Pass.md` delivers 120 identity-specific champion acting poses, compatible companion aliases, additional mechanical camera/music-box/mailbox rigs, event-timed merchant choreography and expanded collection controls. Lantern Knight already had twelve authored combat poses; those were preserved. Current delivery evidence is `PreviewExports/CharacterActing/Gate_Summary.md`; the original AnimationPolish delivery is historical once the follow-up player is rebuilt.

The broad presentation layer is implemented, but the following cannot honestly be labelled complete bespoke animation: independent hand-painted eyelid/mouth/cloth rigs for every character; annotated mechanical cutouts for every gun lock, umbrella rib, fan and music-box lid; unique shopkeeper hand-to-item grabbing clips for every body design; individual evolution/death clips for every species; and unique narrative look-back/choreography for every possible party. Existing pose/art and generic safe articulation remain the fallback for these. They need source-specific art and rig authoring, not arbitrary cuts across a sprite or more wobble. This pass does not silently overwrite every actor's identity with generic generated frames.

## Verification

`ArcaneBrigade.Editor.AnimationPolishBuilder.Gate` imports the art, checks animation bounds, pose safety, repeated-clock stability, pooling reset, every champion and every weapon-motion enum, then runs the existing Windows integration gates. Native animation review uses `Tools/Nightmare/Capture.ps1 -Animation`; the ordinary Nightmare capture remains a separate story/combat regression. Both support Reduced Motion/Effects/Flashes.

The native atlas/actor board is a controlled visual fixture, not a natural campaign or proof of human-perceived smoothness. Real shop movement and pause are also captured. Windows scene/save/ending regressions run separately. Final counts, artifact hashes and inspected frames belong in `PreviewExports/AnimationPolish/Gate_Summary.md`.
