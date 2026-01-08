# Dream Seekers reference and bonus-stat UI

2026-10-14. Revisited [Dream Seekers](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Dream_Seekers), indexed excerpts and the existing1.5 footage. Direct page remains blocked; page describes growth, scoped character rules and overrides, but some entries/version changes are incomplete. We do not claim exhaustive21-character parity or import persistent profile candy growth. User decided stats last for the run only.

## Original Arcane starting traits

Existing K8 identity rules retained, plus:

| Protagonist | Added benefit | Tradeoff |
| --- | --- | --- |
| Hiệp Sĩ Đèn | Player collision damage +2; companion bounce chance +8 points% | Enemy HP +8% |
| Trưởng Bưu Cục | Income +5; player split +1 branch | Enemy count/spawn pressure +8% |
| Tiên Tri Ngái Ngủ | Player dodge +6 points%; player pierce +1 | Enemy movement speed +5% |

These feed real compile/economy/collision, not cosmetic text. Character selection shows scope, icon, bonus values and tradeoffs. New sprite identities remain original object-creatures with readable silhouettes/no arms, rather than imported reference protagonists.

## Clickable explanations and drawn icons

- Code-native `StatGlyph` draws vector-style health/shield/attack/projectile/branch/bounce/coin/star sigils; no emoji or alphabet-placeholder icons. Existing/new bond icons are original raster artwork.
- `GlossaryUI` recognizes all48 stat labels and terms such as split/pierce/bounce, scope, merge/tier, altar/reroll/stack, protagonist/companion, passive, invulnerability, HP/currencies/endless.
- Keywords display an icon and underlined interactive caption. Click/tap in roster bonus descriptions, shop details, inventory source text or bond explanations opens a contextual definition. Definitions include actual scope, mechanics, caps and distinctions, not only translations.
- Overlay blocks underlying mouse/touch clicks in its footprint to prevent accidental purchases; close control clears it. It is informational, not an extra combat skill. Controller row-selection still exposes normal details; controller-specific glossary navigation is not claimed certified.
- Definition text is laid out with its icons and measured for scrolling, rather than clipping because a rich icon makes the row wider. The popup auto-dismisses after30 seconds and is cleared when leaving details/menu pages.

Human UI/art approval and physical touch/controller tests remain open. Native programmatic open-route capture is not claimed as hardware input certification.
