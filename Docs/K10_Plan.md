# K10 — full run stats, depth and expanded roster

User decisions: stats persist throughout the run and endless waves, not across new runs. Movement-only, no upkeep. Add 32 distinct friendly species and 24 distinct weapons with original raster art; no recolor-only counting. Eight bonds with 4–6 species each, without limiting owned units. Stress-test roster counts are not bond species counts.

1. Re-audit reference footage, Dream Seekers, Companions, Relics, Nightmares, achievements and 1.5 changes; record observed versus secondary or unverified mechanics.
2. Add missing core, bullet, body, enemy and economy modifiers through actual simulation, save/readmodel and shops. Preserve additive enum compatibility and deterministic RNG; bounded split descendants, consistent collision/visual size.
3. Replace tinted disks with layered contact/cast shadows for grounded bodies, shops, props, pickups, toys and airborne weapons/projectiles; align sorting and ground anchors.
4. Generate/import 32 original four-frame character strips and 24 original weapon sprites; authored traits, unlocks, prices and identities, 4–6 species per bond; broaden weapon behaviors, not just icons.
5. Refine hit/slash/projectile color/scale feedback, verify readability, regressions, balance, native Windows build and art gallery; push complete source/assets/evidence. Human quality/device approval stays open.

Art generation: painting pass, individual prompts per species/weapon, prompts described by K10_Content_Manifest.json plus the shared art direction. Reference images remain outside shipped assets. Pixel resolution is reported honestly, not claimed native 4K.

## Implementation status

Steps 1–4 completed: scoped reference audit, 48 stat categories, real simulation/economy wiring, run persistence, original content and layered shadows. Dream Seeker-inspired original starting traits, drawn glossary icons and clickable explanations included.

Step 5 completed for the Windows technical gate: 163 focused checks, legacy regression, zero build errors/warnings; native 118 assertions and 37 PNGs (34 confirmed 3840×2160, three desktop aspect frames). Gameplay/shop videos and gallery/checklist packaged. Visual inspection caught clipped glossary text and transformed icon strokes; fixed token styling and replaced rotated GUI lines with cached antialiased code-native glyph textures. Scoped summaries distinguish protagonist and companion bonuses and include protagonist sources. Human art/audio approval and mobile device gates remain open.
