# Secret rooms in the living toyshop

Eight optional rooms are implemented by `SecretEventRun`, `SecretEventCombat` and `SecretEventRoom`. Their doors are physical props at the left of the shop, separate from its ordinary offers and the Keeper invitation. Approach a prop and hold E/Space/controller A, or press DISCOVER. Choosing a card alone does not spend anything: a second agreement action confirms it. All combat choices state that defeat ends the run.

## Discovery and pacing

Shops after waves 3, 7, 11, 15 and 19 offer two doors chosen deterministically from the run seed and current wave. The choices use their own hash, preserving the shop/weapon RNG streams. Each ordinary visit consumes one of two discoveries per run, even when the player leaves without a deal. At most one room can be visited in the same shop. Previously visited kinds do not appear again.

Posting outside the stage can reserve an unvisited Empty Theatre in a later shop. This saved follow-up can exceed the two ordinary visits, but still cannot repeat the same event. Optional rooms consume no normal wave. The unchanged stock, sold flags, gold/egg currency and wave remain waiting on return. Normal and dream shop interactions are blocked while inside a room. The Keeper invitation cannot overlap a secret room, and secret portals are hidden during Keeper reward selection.

## The eight rooms

| Door | Decisions and outcome |
| --- | --- |
| Lost-Property Vault | Select one of two rescues before fighting the cabinet warden, or take the Wooden Heart peacefully. A rescued toy is remembered by the Dead-Letter Office. Solo protagonists receive a corresponding relic. |
| Impossible Tailor | Select an owned relic. Replace its original modifiers/mechanics with Recall (returning ranged shots, +15% ranged weapon damage, no pierce), Aftershock (30% delayed melee strike, 20% longer melee cycle), or Shelter (+18 maximum HP, 15% less incoming damage, team damage -8%). Compatibility is checked; a relic can be stitched once. |
| Understudy's Mirror | Fight the hostile reflection for the Glass Understudy relic; lend a relic for a temporary shadow; or leave after recovering 20% HP. The reflection repeats champion basic attacks after a locked-direction 0.65-second anticipation. Its health is authored for the encounter; it does not copy every allied/item ability. |
| Contraband Express | Pay the shown gold price for a ticket; trade one eligible relic copy for the thread; or fight the collector for a crown. Exactly one transaction per visit. The train is a merchant-room door, not an escort or movement trial. |
| Thirteenth-Hour Bank | Borrow the Hourglass Advance for three normal victories; take a gold advance repaid from 25% of base income plus interest over two victories; or deposit an eligible relic for two victories to improve its positive stat modifiers by 25%. |
| Sleeping Garden | Receive a full heal and a brooch; settle the active secret agreement early; or gain a temporary dream friend. The solo alternative is a champion-damage boon. No hidden curse or currency fee. |
| Empty Theatre | Choose a 28-second paper swarm, a heavy leading actor, or an actor and ranged partner whose attacks alternate. Defeat ends the run. Rewards are previewed before commitment. |
| Dead-Letter Office | Post a relic for improvement; trade a relic copy for +20% weapon damage on a selected permanent ally identity; or send a letter outside the stage to receive a key and reserve a later theatre invitation. A rescued toy adds a one-time thank-you of 15 gold. The Keeper invitation acknowledges the letter. |

## Agreements, effects and persistence

Only one temporary secret agreement can be active. A victory in a normal wave advances it once; secret battles and the Outside Keeper do not advance it. The final normal victory also advances its term, without charging nonexistent final-shop income. Kill-loot income is excluded from bank repayment. Borrowed relics expire, temporary allies are compiled without modifying the saved permanent roster, and deposited/lent relic effects sleep until returned. A deposited stack remains owned; all effects from that item identity are suspended while away, including its proc/signature/mechanic contribution. Improving a relic scales its positive stat modifiers only, not proc power or previously earned permanent gains.

Stitching replaces effects from that item identity, rather than consuming another copy each time. Deposits and stitches are mutually exclusive. The ally-letter bonus belongs to one unit/weapon identity; merging into a different identity does not move it. Existing item caps and combat caps remain effective.

The run snapshot stores room kind/stage, selected targets, battle reward, visit mask, queued follow-up, agreements, alterations and a bounded journal. The build receipt journal shows current agreements, remaining normal victories and event history. Altered or absent relics are labelled in the relic ledger. The existing schema remains readable when the optional secret data is absent.

Entry, agreement, reward and exit save before presentation is rebuilt. Persistence failure restores the previous snapshot. Immediate rewards clear their claim token; battle rewards have an explicit one-time claim. Leaving an unclaimed battle reward is blocked. Battle settlement preserves ordinary shop stock and does not pay normal income or add a normal boss discovery. Health spent in an optional room remains spent. Optional defeat ends the run.

## Art, sound and accessibility

`Art/Secrets/Portals_v1.png` contains eight illustrated doors with closed/open states; `Relics_v1.png` contains sixteen original artifact illustrations, eight used for reward relics. Built-in image generation created both transparent atlases; full prompts are in `Secret_Event_Art_Prompts.json`. The cabinet guardian uses the illustrated drawer cabinet. Rescue variants deliberately retain existing authored companion bodies, weapons and animation sets; they are not a newly painted bear/bird/lamb animation atlas.

Presentation uses the game's ink panels, cream ticket buttons, display type, focus markers, existing ground and prop lighting. Original Toybox cues identify the eight entrances; existing combat, merchant and reward cues handle their actions. No new sound-library generation is required. Reduced Motion removes door sway and focus motion; text choices and danger anticipation remain available. Mouse/touch buttons, numbered keys and controller selection/hold work alongside ordinary shop controls.

## Validation and reproducibility

Run Unity batch mode with `ArcaneBrigade.Editor.SecretEventBuilder.Build` to import art and install definitions. `Verify` runs the focused rules and actual simulation checks. `Gate` runs these checks, the audio import/routing suite and a Windows player build. `Tools/Secrets/Capture.ps1` launches an isolated diagnostic player with a renderable window positioned outside the desktop; `-Reduced` records the reduced-effects variant. Normal player profiles are not used.

Artifacts live in `PreviewExports/Secrets`: focused checks, native checks, accepted screenshots and the gate summary. Combat diagnostics accelerate damage to exercise settlements; they are not ordinary-run balance evidence. Human playtesting and mobile device performance remain separate acceptance work. The existing K24 merge/cooperation/Outside Keeper verification also passes with the current reviewed swarm-payout tuning.

The approved campaign arc now integrates these rooms through `NightmareProgress`, contextual introductions, remembered outcomes, warm scenery states and ending variations. See `Nightmare_Implementation.md` for the combined implementation and its cinematic acceptance boundaries. The room economy and discovery limits described here are unchanged.
