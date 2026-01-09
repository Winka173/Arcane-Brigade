# K11.2 — themed recruitment and automatic protagonist mechanics

2026-10-14. Continues K11.1; not the complete K11 language/artifact/balance overhaul.

Historical K11.2 gate results below are superseded for melee, tiers, recruitment distribution and language by `K11_Melee_English_Checkpoint.md`. Current player captures are refreshed from K11.3, not archival K11.2 screenshots.

## Recruitment

| Protagonist | Recruit themes | Species pool | Starter |
| --- | --- | --- | --- |
| Lantern Knight | Hearth, Clock, Orchard, Oddity | 20 | Teapot Pilgrim + Spoon Sabre |
| Postmaster | Archive, Clock, Tidal | 15 | Brass Acorn + Ink Pistol |
| Drowsy Seer | Frost, Astral, Tidal, Oddity | 19 | Hood Wisp + Snow Globe Orb |

These are actual regular-shop filters, not weighting alone. Species unlock waves still apply. All39 species remain covered across the three protagonists; each bond still has4–6 species, not a roster cap. Existing off-theme saved allies/offers and fusion results are retained. Random weapon assignment remains separate from body species. New recruit rolls choose melee/ranged categories equally while both have available unique variants; this is a distribution adjustment, not a claim that DPS balance is finished. Merge/fusion weapon rolls retain their prior rules in this checkpoint.

## Movement-only identities

- Lantern Stride: actual traveled distance charges a pulse at12 units;65% compiled weapon damage to enemies within2.4 units, minimum4-second cooldown. No charge from teleports/save restoration or shop movement.
- Parcel Express:6 collected gold charges an extra automatically aimed volley,70% compiled weapon damage, minimum4-second cooldown. Scavenger passive/shop income do not charge it. If no valid target exists, holds the charge. Does not overwrite the direction of a pending base attack.
- Dream Breath: stand still for2 seconds to chill nearby enemies (1.5seconds;0.6 on bosses) and remove at most3 hostile projectiles within3 units;7-second cooldown. Movement interrupts charge; this is not invulnerability or blanket hit immunity.
- Charge/cooldown reset per combat wave, unlike purchased run stats. Pause and rest do not charge or activate. Each activation records contribution provenance; secondary effects cannot recursively trigger relic procs.
- Only one activation can be charged at once; excess pickup gold does not bank multiple volleys. Stride/Breath hold until an enemy/projectile is in their region instead of firing into an empty arena. Breath charge still resets when moving.
- A thin charge indicator sits below HP, no manual skill button. Selection shows theme icons and mechanical descriptions; glossary explains each named mechanic and theme filtering. Existing starting-stat traits/passives retained.

## Reference boundaries

Indexed [Dream Seekers](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Dream_Seekers) and [Game8](https://game8.co/games/Zenless-Zone-Zero/archives/475381) support protagonist-specific rules and some shop exceptions; they do not establish that every reference protagonist has these Arcane pools. Indexed [Relics](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Relics) confirms diverse stat tradeoffs and conditional effects. Full/mixed-version tables remain incomplete/blocked. The three mechanics/pools above are original Arcane design, not copied formulas or character abilities.

## Validation approach

K11 builder runs legacy compatibility checks with HeroRevision0, then applies HeroRevision1 and runs themed RNG/save, actual movement/pickup/idle mechanics, pause/rest and projectile-limit checks plus15 movement-bot full-run smoke tests. Compatibility gates are not misrepresented as all old fixtures under the new theme mode. Native capture verifies the enabled mode in the actual Windows player; human pacing/art/audio/device gates stay open.

Final model gates:22 transition/motion checks and167 theme/mechanic assertions, with earlier163 K10/65 K9 checks and legacy compatibility gates. Windows build zero errors/warnings. Full-run bot results:14 victories and one Lantern Knight/difficulty2 timeout at hall4 (seed77169), not15 victories. This is not natural-game balance approval; melee approach and difficulty tuning remain open. Native transition fixture now moves through wave2 instead of standing idle and dying with a melee-heavy party; no HP, immunity or enemy-timer override was added.

Named hero contributions use player-facing glossary names, not internal effect IDs. Charge bar dims during cooldown; full charge waits for an actual target/threat. Runtime screenshots and movie are refreshed from the final candidate, not retained from K11.1.

## Independent market choreography

Gold merchants now fall from above at authored size, land with a short squash and leave upward. Dream merchants rise from below a fixed ground plane; a dedicated sprite shader clips the submerged body instead of showing a transparent giant under the map. They sink on departure. This checkpoint concerns shopkeepers, not combat-boss introduction sequences.

Stock launches from the merchant toward each stand on staggered parabolic paths and returns independently on departure. Pedestals rise and settle separately. The carpet retains its texture and uses a shaded32-by32 mesh to curl/unroll and roll back; neither the market root nor the actor entrance uses a tiny-to-full scale multiplier. Shadows track elevation. Presentation-clock timing freezes on pause; reduced-motion mode substitutes fades and a flat carpet. Failed boundary saves restart individual entrances as well as the carpet lifecycle.

Native Windows fixture:55 passing assertions, zero failures,17 verified3840x2160 framebuffer PNGs, including live carpet presence and full-scale market root during arrival. Arrival/departure screenshots and the silent realtime movie demonstrate these motions. Technical gates and image inspection do not imply human animation/art/audio approval.

Still pending: English-default localization, concise highlighted synergy previews, more differentiated artifact content, full melee reach/DPS and difficulty tuning, dedicated enemy attack/environment artwork.
