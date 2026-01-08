# K8 — single-player polish and surreal merchants

2026-10-14. This supersedes K7B for the current Windows candidate, not a commercial-quality approval.

## Implemented

- Receipt-eating three-eyed register merchant and cracked-egg oracle: separate original four-state sprite rows, independent personalities, entry squash/confetti, attentive pose, committed purchase/HP/fusion reactions and denial. Short Vietnamese text dialogue on entry/proximity/sale/reroll/fusion; no modal, no extra input, no voiceover claimed. Pause follows simulation time; reduced motion disables entry particles and typewriter.
- Painted oval market rugs and garlands replace the UI-like rectangle on the floor. Rugs have separate1536px-wide source sprites, uniform scale rather than enlarging/stretching small atlas cells; prompt/size in `K8_Rug_Art_Manifest.json`. Portal uses a sign rather than a small duplicated shopkeeper. Ground/contact shadows, companion shop avoidance, stock retirement and staggered arrivals retained. Tooltip sits below goods and above exit; long details scroll rather than cover the arena. Prices/rarity/currency remain physical-shop interactions.
- Three unique hero bodies, seven distinct companion forms and five bespoke boss silhouettes with four source states each. Source PNGs are not native4K; original painting prompts and sizes recorded in art manifests. Layered runtime movement is not dozens of hand-drawn directional/attack/death poses.
- Companion bodies/weapons reduced relative to player, tier stars/weapon shapes, hostile projectiles drawn above friendly VFX, less map-floor contrast, edge landmarks and three stage palettes. Same-target damage numbers aggregate over a short window; damage math is unchanged.
- Weapon anticipation/release/recovery differs by family and is continuous through impact. Reduced-motion, paused dust/death/attack behavior covered by focused checks.
- Boss overtime stops reinforcement spawning, preventing target starvation. Timer does not grant victory; UI shows HẠ TRÙM. Former hall8 seed77200/difficulty3 case now clears, and all15 movement-bot runs finish20halls; this is not human balance approval.
- Bow projectile ricochets and tea impact split: bounded ownership, swept collisions, victim dedupe and secondary cause-depth rules. Wave1 remains contact-only, enemy emitted speed capped, incoming single-hit damage limited without global invulnerability.
- Animated roster portrait grid/locks, Vietnamese player-facing descriptions, drawn inventory/pause icons, next-threshold bond icons and activation feedback. Purchase explanation shows scope, stacking, owned/resulting count and HP safety; recruit weapon remains random.
- Three champion build identities, tier-up ranged multishot/pierce and melee reach/arc/speed, cumulative bond milestones2/4/6, four recipes with four different existing companion outcomes. These recipes do not introduce four new bespoke fused characters.
- Separate menu/shop/combat/boss CC0 compositions at pitch1, eight-voice danger/interface/cosmetic lanes, contextual material hits, ducking and focus/pause ownership. Credits ship with build; audio provenance document records exact primary-source download/license/hash evidence.

## Validation

- `ArcaneBrigade.Editor.K8Builder.Build`: apply, full regression,15-run movement diagnostics and Windows build.
- `ArcaneBrigade.Editor.K8Builder.FinalizeCandidate`: preserve content, rerun regression and rebuild after presentation refinements.
- New checks: presentation12, combat23 + exact boss replay, content91, motion51, UI40, merchant17, audio source114 + playback27; previous K0–K7 suites rerun, including K4 sandbox258 checks. Legacy assertions now check semantics instead of English labels; wave-independent collision fixtures use RunWave0, while K8 verifies actual wave1 guards separately.
- Windows build succeeded with0errors/0warnings. Native framebuffer evidence, reports, music links and motion clips are collected in `PreviewExports/K8/index.html`.
- Native102assertions passed with0failures;33screenshots,30native3840×2160 +3desktop aspects. Actual entry/inspect/committed-sale dialogue, merges, item/deny, HP/egg purchases, reroll and downward wave transition exercised. Five boss frames use real runtime boss instances; their diagnostic player position is staged to frame the boss, not a natural first approach.
- Separate12s no-encoder probes at1/56/100allies show roughly60FPS on RTX5070Ti/Ryzen9700X, p95frame~16.7ms. Raw CSV/metrics are hardware-specific; release GC counter unavailable(NA), not zero-GC, GPU timing includes presentation/driver. No device certification inferred.
- Reproduce native: `Builds/FullRun/ArcaneBrigade.exe -arcane-k7-capture -arcane-k7b -arcane-k8 -capturePath <absolute-directory> -force-d3d11 -screen-fullscreen 0`.
- Reproduce movie: `Tools/Record-K7Window.ps1 -Phase K8 -Mode shop` or `combat`. Direct Unity framebuffer only, silent movie, no desktop/microphone. CFR movie frame count is not performance proof; fixture stock/coins/roster are seeded, not natural economy.
- Final movies: shop38s/2280CFRframes, combat15s/900CFRframes, both1920×1080. Shop movie starts during merchant entrance, showing actual unfolding/confetti and then proximity/sale reactions. Candidate/assets/evidence fingerprints: `PreviewExports/K8/Artifact_SHA256.txt`; local build folder is ignored by Git, source/art/audio/provenance/evidence are tracked.

## Still open / not silently marked complete

- Shared Dream co-op remains deferred as agreed before this pass.
- More protagonists, truly new fusion species, bespoke animation sets per weapon direction, biome-specific gameplay and a commissioned cohesive soundtrack remain content expansion, not claimed delivered by this polish pass.
- The four soundtrack tracks are non-exclusive licensed third-party works. Hearing checks are required for taste, loops and mix; waveform/playback tests cannot approve them.
- Android/iOS hardware, physical controller, Steam/Deck release gates remain open. Human quality approval and exact Bizarre Brigade formula/version parity remain unclaimed.
- Preserve movement-only controls, immortal unlimited allies, no upkeep/sell/lock or permanent stat progression.
