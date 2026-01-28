# Build forms

Current presentation (October pass): evolved companions grow in body size only. Tier accessories and body-mounted form accessories are disabled by the latest art direction. The twelve combat choreographies and equipment progression below remain active; the accessory descriptions in the historical table describe the earlier presentation.

Twelve equipment-driven transformations, each with three visible stages and its own attack choreography. No additional combat input. Champion and permanent recruit weapon identities, random weapon assignments and save schema remain unchanged.

## Progression

The assigned weapon selects one form family. At least one compatible relic is required. Craft sources:

- One point for each **different** owned compatible relic, using its positive modifiers and matching champion/companion scope. Extra copies grant stats but no extra craft. Authored signature connectors also qualify for their associated family.
- One point for each **different supporting weapon** carried by another permanent party member, maximum two. Another copy of the carrier's own weapon does not count. Temporary secret-room friends do not supply partner points.
- One/two points for recruit tier two/three. Champion progression works through relics and supporting weapons, without an evolved recruit requirement.

Stages unlock at 2 / 4 / 6 points: Awakened / Transformed / Masterwork. Every family has at least four compatible relic identities for an evolved carrier. Deposited, lent and rewritten relics lose their former contribution while their original effects are suspended. Progress is derived from saved equipment at each compile, including immediately after a rest purchase. No hidden persistent stat tree or new currency.

| Family | Typical carriers | Visible identity | Attack choreography |
| --- | --- | --- | --- |
| Living Stitchwork | Sewing, scissors | Spool, needles, teal thread charms | Pins, nearest-neighbour stitches, closed web at later stages |
| Paper Furnace | Ember weapons, iron | Lantern engine, folded amber fins | Travelling ember vents; masterwork forks beyond the target |
| Clockwork Foundry | Hammer, spring hammer, alarm | Winding engine and gears | Alternating side impacts, then perpendicular delayed beams |
| Midnight Tea Service | Tea | Porcelain cup and saucer shoulders | Three slowing pours, satellite cups, outer saucer ring |
| Pocket Constellation | Remaining ranged weapons | Indigo orrery and ivory stars | Inward star rays, increasingly many directions, enclosing constellation |
| Origami Tempest | Fans, umbrella, ribbon wheel | Jade folded wings and crane charms | Outward crescent cuts with a resisted push; wider masterwork fan |
| Silver Understudy | Camera, projector, echo blade, remaining melee | Silver mirror fan and fragments | Offset narrow strikes from alternating reflections |
| Toybox Orchestra | Bell, music box | Gold pipe organ, bell/note charms | Successive concentric distance bands and a final answering wave |
| Polarity Workshop | Magnet, yo-yo | Magnet coil rig | Resisted draw, delayed discharge, crossed current lanes |
| Moonlit Regatta | Fish, satellite disc | Fish-tail pennants and fish charms | Outward lane, displaced returning wake, crossing wave |
| Crystal Music Garden | Frost weapons | Crystal shoulders and six-point motifs | Unfolding slowing spokes, then outer fracture ring |
| Impossible Ledger | Stamp | Receipt mantle and wax seal | Closing rectangular seal, cross strokes, larger final seal |

Explicit mechanics take precedence over elemental fallback: a music box remains Chorus even when its element is Arcane. This preserves weapon identity. The Details > Links tab lists every family, carriers, current points, exact sources, eligible weapons and relic affinities. Champion form is also shown in Stats, and recruit form is in that recruit's explanation. Shop item tooltips show CRAFT +1 or FORM UP for affected current carriers; duplicate relics do not advertise extra craft.

## Combat and presentation

Every third resolved basic attack can release a form, with a 2.4-second minimum interval. That attack retains 72% basic damage. Its choreography receives a **single raw damage budget** of 80% / 100% / 120% of that attack at stages one/two/three, divided across all segments. Each segment divides its allocation across at most 3 / 4 / 5 nearest eligible victims; root attribution is retained. More bodies and more lines do not multiply the budget. A normal three-hit sequence therefore gains at most roughly 17% / 24% / 31% raw damage before misses and weapon-specific basic payoffs. This is a tuning starting point, not a player enjoyment claim.

Patterns are fixed to their committed world positions. Lines, circular fields, annuli, crescents and square seals use matching collision geometry. Secondary hits have no weapon payload and depth one; they cannot release another form or re-enter weapon recipes. Pulls and short slows respect ordinary enemy resistance and never affect bosses. If capacity is reached, the ordinary attack remains intact and the form charge is retained. Pending choreography expires, freezes during pause and clears on rest/restart. The contribution ledger measures the actual damage under `build-<family>` with the carrier as owner.

Stage one equips a small body clasp and weapon charm. Stage two adds the family's painted back attachment; stage three adds orbiting motifs. The existing generic evolved mantle is replaced by the specific form costume while a form is active. Weapon proportions change slightly by family. Character animation, hitbox and movement rules remain intact. Atlas pivots are compensated using visible ink bounds, including across animation frames and pooled bindings.

Friendly choreography draws below hostile bullets and danger presentation, uses outlined strokes, and shares existing effect-density limits. Reduced motion stops orbit/sway while retaining equipment identity and hit timing. Reduced effects/high readability reduce friendly visual weight. No full-screen flashes or global camera effects are introduced.

Each family maps to a matching **existing original Toybox** sound, with lower gain for allied casts and the existing voice limits/cooldowns. This feature does not generate new music or use the excluded studio files.

## Artwork and validation

Original painting pass artwork: `Art/BuildMorphs/BackAttachments_v3.png` and `Charms_v2.png`. The importer selects twelve isolated backpacks and twelve matching charms. The first crowded atlas was rejected; final source alpha is retained without external raster editing. Full prompts and generated source locations: [Build_Morph_Art_Prompt.txt](Build_Morph_Art_Prompt.txt).

Unity menu: **Arcane Brigade > Build Forms > Import artwork / Verify**. Batch entry points: `ArcaneBrigade.Editor.BuildMorphBuilder.CheckContent` (import + focused checks) and `.Gate` (also cooperation, secret-event, nightmare and Toybox audio regressions, then Windows player).

Native diagnostic player: `Tools/BuildMorphs/Capture.ps1`, optionally `-Reduced -Output PreviewExports/BuildMorphs/Reduced`. It uses isolated diagnostic save storage, renders the production character renderer at all 36 family/stage combinations, then loads the actual arena with each equipment-compiled masterwork and waits for its automatic attack. Gallery characters intentionally share one body for silhouette comparison. Combat enemies use high diagnostic HP; screenshots are visual/integration evidence, not a balance playtest. It never reads or overwrites a normal player profile.
