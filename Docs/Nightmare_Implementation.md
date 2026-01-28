# The Shop That Would Not Close

Original campaign narrative, implemented as a wave-driven layer over the living toyshop. No sanity currency, permanent metaprogression stats, fake prices, control distortion, forced blur or targeting circles are added.

## Campaign and saves

`NightmareProgress` is optional data on the existing run snapshot. It stores bounded depth, accepted secret decisions, resolved outcomes, atmosphere-line cursor and ending page. Old snapshots without it remain readable. Depth is determined by wave, not time spent surviving or shopping.

| Waves | Layer | Implemented presentation |
| --- | --- | --- |
| 1–4 | The shop is open | Warm window, empty chair, familiar lamp, ordinary shelf. |
| 5–8 | After the last bell | Hat waiting on the chair, night window, increasingly uncertain speech. |
| 9–12 | Objects remember | Watchful window, ribbed shelf, purchases and decisions recalled in dialogue. |
| 13–16 | The shop remembers | Occupied chair, watchful velvet, common-owner clues for hand/tongue/eye. |
| 17–20 | The night refuses to end | Last-Customer foreshadowing, darkened architecture contrasted with warm familiar landmarks. |

Landmarks are illustrated background objects with attached shadows and restrained breathing/sway; combat silhouettes, player identity marker and hostile shots are not tinted or dimmed. Reduced Motion freezes landmark movement and illustration panning. Story layers do not increase difficulty for slow players.

Normal shops can show one brief atmospheric caption per wave, after the merchant has finished speaking. No extra persistent HUD section is added. The existing merchant mouth/voice system speaks the new lines with its punctuation-aware gibberish pacing.

The authored library contains 80 normal-merchant lines, 40 dream-merchant lines, 20 atmospheric lines, 24 choice memories, 16 room introductions, plus ending and confrontation dialogue. These are original English lines, not copied source-game dialogue. Normal/dream purchase, denial, blood purchase, reroll, fusion and departure remain distinguishable.

## Eight rooms are part of the story

The secret-event implementation and its economy remain intact. Entering or leaving a room is not equivalent to completing it: accepted choices and resolved outcomes are tracked separately. Optional battle memories resolve only when the selected reward is successfully claimed. Transaction/save rollback restores narrative memory with the rest of the snapshot.

| Existing event | Story connection | Remembered detail |
| --- | --- | --- |
| Lost-Property Vault | Living things are not missing inventory. | The rescued friend, the carried letter, or gently taking the wooden heart. A rescued toy adds a warm heart landmark and crosses first in the ending. |
| Impossible Tailor | A thing can change without having its identity erased. | Returning thread, a second strike, or protective shelter is named in later merchant speech. |
| Understudy's Mirror | A copy is not ownership of a person. | Defeating the reflection, borrowing its shadow, or choosing to leave peacefully. |
| Contraband Express | Some things escape the inventory entirely. | The ticket's destination, unlisted thread, or standing up to the collector. |
| Thirteenth-Hour Bank | A promise is different from a chain. | Borrowed time, repayable debt, or entrusting an item without surrendering its identity. No invented ending debt is charged. |
| Sleeping Garden | Safety and rest are not merchandise. | Being cared for, settling a promise, or meeting a temporary dream friend. |
| Empty Theatre | The audience does not get to choose the player's ending. | Facing the paper crowd, leading actor, or duet. The letter-triggered follow-up remains supported. |
| Dead-Letter Office | An address proves the possibility of somewhere outside. | Entrusted parcel, letter to a specific ally, or invitation beyond the stage. Sending the invitation adds a paper-bird landmark and a shopkeeper ending line. |

Narrative room introductions change at late depth without replacing price, danger or confirmation text. Resolved choices can be recalled during ordinary/dream shop conversations. No room, rescue, letter or random door is required to finish the main story.
Resolving the mirror preserves a clear familiar window; resolving the tailor preserves the repaired shelf; resolving the theatre keeps its curtain from turning into an eye. These are visual anchors, not hidden combat buffs.

## The Last Customer

The final normal hall replaces its rotating boss with The Last Customer; this is the finale of the twenty-wave route, not an extra random encounter or endless tax. The final boss is a separate story resource, not part of the ordinary ten-boss rotation or the optional Outside Keeper.

Six attack sequences use the existing real damage/field system: final stamp and impact wave; staggered binding ribbons; thrown parcels; crossed receipt strokes; a stamping procession with a body leap; closing shadow and ribbon lanes. Three breakable organs weaken the associated patterns. If an organ is broken, the boss retains a smaller readable fallback instead of stopping entirely. No generic enemy bullet volley is substituted for these attacks.

The optional Keeper remains distinct: the hand/tongue/eye belong to the dreaming shop, while the Customer tries to purchase the whole dream. Defeating the Keeper changes its final gesture, not eligibility for the main ending.

## Ending

Customer victory first opens a physical farewell room with an unmasked, living man and two choices: **Close the shop** or **Leave one light on**. Both reach real dawn. The champion can approach and confirm with Space/A, or click either choice. Rooted champions can select left/right without moving. The irrelevant combat HUD is hidden. Choosing then opens four illustrated, gently animated pages before the ordinary run result:

1. **The Receipt Ends Here** — the protagonist is no longer an entry on the bill; a resolved event can be recalled.
2. **The Shopkeeper** — closing the shop means accepting goodbye, not erasing memory. A sent letter receives its own line.
3. **Morning, Together** — rescued friends cross first; defeating the optional Keeper makes its hand deliberately hold the doorway steady.
4. **Some Things Remain** — a lantern, repaired heart and folded bird remain in a quiet real-world morning.

Each page can be advanced by mouse/touch button, Space, Enter or controller A after a short accidental-input guard. Page progress saves. Pause freezes presentation and offers Resume. Results and explicit endless continuation unlock only after the ending completes. Legacy victories do not acquire a fabricated Customer defeat on load.
An additional native diagnostic reloads the scene midway through the ending and checks that page two resumes; it also checks that pausing cannot advance a page. Narrative choice masks and ending page/stage consistency are validated on restore.

Music uses the existing soundtrack: deeper layers gradually thin melody/rhythm stems without pitch distortion, and dawn restores the clear palette. The latter ending pages reprise the existing menu theme. No audio library is regenerated and no listening approval is claimed.

## Art and acceptance boundaries

The lamp choice adds a fifth illustrated afterword after the complete ordinary dawn, not instead of it. Three existing soft cue hits accompany the knocks. `NightmareLampPromise` / `NightmareShopClosed` are credited only after the selected ending finishes. They grant no stats and launch no unimplemented chapter. Revisioned saves keep a legacy ending already in progress from being rewound to a new choice. Narrative branch/page validation rejects forged early promises; run-save failure rolls back choice state.

`NightmareHaunting` adds bounded low world-space mist and peripheral ink shadows below actors. Reduced Effects lowers density, High Readability lowers it further, Reduced Motion freezes drift, and pause freezes presentation. Wave 19's first four shop seconds briefly quiet music and decorative chatter without locking controls or shopping. A reserved silhouette, altered chair leg and glance-away eye add unease without false prices or simulated enemies.

`NightmareGrandStage` composes four original perimeter architecture pieces with parallax, restrained sway and a staged reveal around the final Customer. Strength grows from wave 8 to the maximal wave-20 hall. Warm oblique light shafts and Customer-local light distinguish the last fight from the ordinary floor. The shop uses a quieter translucent version; secret rooms keep their own staging; dawn dissolves the oppressive architecture. Nothing adds targeting circles, collisions or invisible damage fields. Art is described in `Nightmare_Farewell_Implementation.md`.

The current visual pass uses original painting pass edits: `Art/Nightmare/NightmareProps_v2.png` (16 drawings), `LastCustomerIdle_v4.png` and `LastCustomerActions_v4.png` (four poses each with more source area per pose), `LastCustomerOrgans_v2.png` (eight anatomy/attack drawings) and the existing `NightmareEnding_v1.png` (four story panels). Earlier body atlases remain superseded source material. Revision provenance and prompt requirements are in `Nightmare_Visual_Art_Prompts.md` and `Nightmare_Farewell_Implementation.md`. Sprite sources retain alpha; ending illustrations are intentionally opaque. Irregular scenery, body and organ atlases use connected-component bounds rather than cutting protrusions at a uniform cell edge.

The ending now combines the narrative illustrations with an animated 2D departure using the saved champion, permanent roster, assigned weapons, recruit tiers and rescued-friend cameos. Figures walk toward a warm doorway and fade only as they cross it; Reduced Motion keeps them stationary. It is layered sprite storytelling, not a fully rigged cinematic or a replacement of every illustrated background figure with the party. Existing page saves, pause and manual advance remain authoritative.

`NightmareLighting` supplies four bounded local sources, gradual depth ambient, warm dawn recovery and light-aware soft shadow direction through the current Built-in shaders. Characters retain readable brightness; hostile projectile and UI rendering are not globally darkened. Floor grain/ink/rib treatments strengthen with depth, and the lamp visibly spills onto the ground. The main floor textures remain the existing three painted floor families, not five newly painted maps.

Landmarks use independently hinged upper/lower sprite parts; the corrupted window/curtain iris has a localized shader look toward the player. Rare independent decorative-shadow gestures occur only in the shop. Receipt motes are bounded and disabled under Reduced Motion/Effects. Carrier contact dust, passenger tilt and landing recovery extend existing transfer animation. The Customer has a specific entrance and cracked-mask death treatment. The native diagnostic now queues lethal damage into the real fixed-step combat path so kill effects cannot be lost before the event consumer runs.

These are procedural sprite articulation and shader treatments, not a claim of individually hand-drawn eyelid, finger or cloth rigs for every prop and merchant. Campaign-scale performance, human art approval and mobile device acceptance remain separate gates.

Tests and native evidence are reported in `PreviewExports/Nightmare/Gate_Summary.md`. These are not a substitute for human campaign balance, audio listening or mobile device acceptance.
