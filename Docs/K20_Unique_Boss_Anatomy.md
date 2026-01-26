# K20 — unique anatomy for all ten bosses

## Scope

Replaces K19's four shared decorative organs with 30 individually identified boss parts. Each boss has three destructible attachments, unique rest/charged artwork and a matching torn socket. Five original transparent 1254×1254 atlases contain 60 part poses and 20 socket variants; ten socket variants are currently used. These are modular additions to existing animated boss bodies, not ten replacement full-body sheets. No part illustration is assigned to a relic, companion or weapon.

| Boss | Left side | Right side | Centre |
| --- | --- | --- | --- |
| Undelivered King | Stamp Jaw: removes envelope bites | Dead Letter Cannon: reduces letter volleys | Pendulum Crown: removes its pulse and slows the attack cycle |
| Scarlet Index | Quill Talon: removes lashes | Binder Maw: removes falling paper traps | Ink Bladder: removes ink pools |
| Sleep Collector | Rocking Claw: removes lashes and slows walking | Drowsing Bell: removes paired sleep rings | Dream Sack: removes dream pools |
| Wax Admiral | Anchor Claw: removes anchor sweep | Molten Broadside: reduces volleys | Wick Cluster: removes molten pools |
| Last Lantern | Wick Tendril: removes needle lashes | Shutter Eye: reduces light volleys | Fuel Chamber: removes falling fuel traps |
| Marquee Maw | Stage Jaw: removes bite | Spotlight Eyes: reduces aimed shots | Curtain Tendon: removes thrown stage debris |
| Mother Umbra | Rocking Arm: removes lashes; cradle tilts and slows | Nursery Bell: removes paired rings | Shadow Pouch: removes shadow pools |
| Coin Widow | Coin Hook: removes sweep and slows crawling | Coin Spitter: reduces coin volleys | Vault Abdomen: returns stolen coins, stops suction; exposed phase attacks faster |
| The Howling Organ | Bass Mouth: removes triple bass rings | Choir Pipes: reduces choir shots | Bellows Heart: removes its pulse and slows rhythm |
| Suitcase Leviathan | Zipper Jaw: removes bite | Handle Tentacle: removes thrown luggage | Luggage Chamber: removes tongue pools; burst phase attacks faster |

## Gameplay rules

- Input remains movement only. Incoming horizontal damage targets a side organ; sufficiently vertical damage targets the centre (`abs(y) > abs(x) * 1.3`). This is a directional body-zone system, not cursor aiming or separate collision meshes. Zero-direction environmental damage and enemy friendly fire do not damage organs.
- Each side's durability is 18% of full boss HP; centre is 16%. Organ damage also reduces body HP normally. Breaking all three does not itself kill the boss, does not grant permanent stats or invent gold. The boss still has to be defeated.
- Broken organs are excluded from the next attack selection. Break cancels the current windup and grants a short recovery; existing launched projectiles and ground warnings finish normally rather than vanishing. When both sides are gone, the exposed core retains a warned shockwave and reduced core volleys, even after the centre is destroyed. Independent pre-existing half-health arena motifs remain active.
- Each attack selects an intact organ, with that part's windup/release pose. Lash origins use its attachment position. Fan/ring/coin/choir volleys vary by boss; reduced volleys stay at least three shots. Projectiles remain clamped to the established hostile speed ceiling. Attachment-based projectile origins are clamped inside the arena.
- Destroying a locomotion part multiplies actual walking speed by 0.6; the body also gains a static lean. Tempo parts add 0.75 seconds between patterns; ruptured vault/luggage centres subtract 0.35 seconds. Base enemy HP/damage/speed buffs from K19 remain unchanged.
- Widow suction operates only during vault windup, never steals coins already attracting toward the player, stores at most 120 gold and returns the exact stored value on break or death. Pull distance is simulation-delta based, not rendered frame rate based.

## Presentation

Each part has subtle idle motion, a dedicated charged sprite plus recoil/release transform motion, local damage tint/recoil, falling/spinning/fading break debris and a persistent recessed socket. This is two painted poses plus procedural in-between motion, not a claim of frame-by-frame authored sheets for every state. Reduced Motion removes decorative sway/falling/spinning but preserves charged-state changes and sockets. Reduced Flashes suppresses the short hit flash.

Death remnants copy the three organ/socket pieces and scatter/fade them using the existing death-feedback clock, so they freeze with the rest of presentation. Reusing a pooled remnant removes old pieces. Regular enemy deaths do not allocate anatomy arrays or search children for boss debris.

Collection adds BREAK/FIX controls for each of the three parts and explains individual consequences. Idle/walk/attack/hit previews remain isolated from gameplay and do not grant discoveries, currency or stats.

## Validation and artifacts

- `K20BossAnatomyBuilder.Build`: imports/slices the five atlases by alpha components, links the anatomy resource and calls Verify. Re-running preserves sprite identities.
- `K20BossAnatomyBuilder.Verify`: 417 anatomy checks, then K19 325, K18 91 and the existing ink/shop/K17–K10 regression suites; builds Windows. Checks cover all 30 directional breaks, independent durability, attack origins/warnings, consequences, repeat-break prevention, real vault capture/return, reduced core volleys, pause and rest cleanup.
- `PreviewExports/K20/AcceptedFinal` and `AcceptedReduced`: native 3840×2160 screenshots and assertion notes for all ten bosses. Fixtures use selected allies, high per-hit damage and substituted boss templates at a fixed test wave; windup sampling advances the real simulation in fixed steps. They are not normal-run balance or performance evidence.
- Native coverage: 96 assertions in each mode, three unique attachments, actual windup/release, real companion attacks breaking each organ, all three sockets, one boss death/scatter, collection preview and cleanup. The first draft's failed windup sampling was replaced with simulation-timed sampling, not accepted as passing evidence.
- `Campaign.csv` / `Economy.csv`: five victories, five defeats and no timeouts in one fixed-seed ten-hero movement/purchase bot observation, not a human win-rate claim. Manual full-run difficulty/readability approval remains necessary.
- Accepted Windows build: zero errors and warnings, 715785475 bytes. Runtime DLL SHA256 `2AFC72FD06A11959584E24AC35803DEFC6E951F1C3D0DFABC48C8C1FC8A4876C`; resources SHA256 `FE90344D16DA0B00F57420F227EF5AFD9EB681ED715B69D252312143F4CFF589`.

Run `Builds/FullRun/ArcaneBrigade.exe`; share the entire FullRun folder. No audio sources or the other agent's prompt/contract edits are included. Mobile device acceptance and publication remain open.

## Manual review

1. Fight from different sides and vertically; verify each organ's visible removal matches its attack consequence without requiring new buttons.
2. Draw the Widow near loose coins, then destroy her vault: returned value must equal what she took.
3. Break the cradle arm and luggage chamber separately to compare slower tilted movement against the faster exposed phase.
4. Check charged poses and sockets in crowded fights, at arena edges and in Reduced Motion; confirm enemy warning shapes remain readable.
5. In Collection, toggle each part, cycle poses, change boss and leave/reopen; ensure no preview actors or state leak into a run.
