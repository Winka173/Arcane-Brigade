# K10 Windows candidate — review checklist

2026-10-14. Source reference boundaries: `K10_Research.md`, implementation formulas: `K10_Stat_Implementation.md`, protagonist/UI additions: `K10_Dream_Seekers_UI.md`.

## Delivered

- 48 modifier categories (18 retained, 30 appended), actual simulation/economy wiring, 30 purchasable original contracts. Stats survive waves/endless within the run and reset on a new run.
- Three existing protagonists retain their identities and gain original starting-stat benefits/tradeoffs inspired by Dream Seeker differentiation. No profile stat growth or borrowed character sprites.
- 32 new four-frame friendly species, 24 new weapon sprites/definitions and four dedicated bond icons: 60 original source PNGs. Total 39 species and 40 weapon definitions. Eight bonds have 4–6 species; owned roster is unlimited.
- Layered contact/directional shadows, corrected body-to-ground anchors, projectile size shared by rendering/collision, element-tinted painted impacts/slashes. Shadows are art-directed 2D projections.
- Drawn code-native stat sigils and clickable underlined vocabulary in roster descriptions, inventory and shop details. Definitions explain scope, stack/reset rules, mechanics and actual caps. Popup blocks purchases underneath.
- Fixed a native-only visual defect: glossary glyph strokes escaped scroll clipping and per-token wrapping clipped text. Cached antialiased glyph textures and nonwrapping token styles replace that path. Scoped stat summaries distinguish protagonist versus companion bonuses and list protagonist sources.

## Evidence

- `PreviewExports/K10/Editor_Gates.txt`, `Focused_Checks.txt`, `Windows_Build.txt`: focused checks, legacy regression and Windows build outcome.
- Final editor gate: 163 K10 focused assertions plus legacy K3–K9 checks; Windows build succeeded with zero errors/warnings. Native fixture: 118 passing assertions and 37 screenshots (34 intended 4K frames, three desktop aspect frames). Surface dimensions must be read from the actual PNG/CSV, not assumed from launch arguments.
- Final recapture confirmed all 34 non-aspect PNG headers at 3840×2160 and all four measurement surfaces at 3840×2160. On RTX5070Ti / Ryzen9700X: 59.9–60.0fps at roster1/39/56/100; CPU p95 0.910/2.452/2.897/6.946ms, peak memory up to373.2MiB. Release GC counters unavailable; no universal performance or mobile claim.
- `PreviewExports/K10/Player/`: native menu, glossary, combat, shop, merge, purchase, rarity, bosses and downward-exit screenshots, assertions and measured frame CSVs.
- `PreviewExports/K10/Movement_Bot_Balance.csv`: 15 scripted movement runs, all victory. This is a regression smoke test, not proof of natural-game balance or human satisfaction.
- `PreviewExports/K10/index.html`: source art plus runtime review gallery. Framebuffer resolution 3840×2160; source PNGs are not native 4K. Desktop aspect resizes are not mobile device tests.
- Gameplay/shop videos record only the game framebuffer at 1920×1080, 60-fps CFR and timeScale1. Videos are silent: they do not certify music/SFX quality or prove rendering throughput.
- Encoded gameplay: 15 seconds / 900 frames; shop: 38 seconds / 2280 frames. Encoder repeats missing wall-clock slots rather than speeding up simulation. A native pass whose window changed to 1920×1009 midway was rejected as 4K evidence and recaptured; passing behavioral assertions alone do not certify render size.
- `PreviewExports/K10/Artifact_SHA256.txt`: candidate source/content/evidence fingerprint, including the local executable hash. Local player is `Builds/FullRun/ArcaneBrigade.exe`; generated build directory remains outside source control.

## Playtest checklist

1. Start each protagonist; scroll their benefits/tradeoffs. Click highlighted terms and verify definitions, closing and no accidental underlying action.
2. Recruit different species/weapons, inspect stat scopes and individual support effects. Buy/merge between waves: no bonus reset, no duplicate old tier sources, no free healing from changing maximum HP.
3. Buy pierce/split/bounce/size contracts. Check trajectories, readable friendly/hostile colors, bounded fragments and hitbox/visual agreement.
4. Approach all shop options; buy recruit/item, deny unaffordable purchase, merge, reroll and pay eggs/blood. Check shadows, bounce, gather/payoff, merchant voice and downward transition.
5. Stand still and move: idle/blink/step, ally patrol/rest and dust should remain visible. Check large party combat and rare-item lighting without losing hostile-projectile readability.
6. Continue endless: roster/items/stats retained, later enemies stronger. End run and start again: no permanent stat carryover.
7. Listen to the real player audio; evaluate pacing, mix and fatigue separately from silent recordings. Report readability/style cohesion and animation feel; generated images and automated tests cannot approve taste.

## Open gates

Human art/animation/music/SFX approval, physical Android/iOS and controller glossary input remain open. Full public reference pages are blocked/incomplete; no claim of exhaustive 1.5 item/protagonist/enemy parity. New species have authored four-frame strips plus procedural motion, not full bespoke skeletal animation sets. Weapon definitions share existing motion/family systems; not 24 newly invented combat engines. No ray-traced lighting or native-4K source-art claim.
