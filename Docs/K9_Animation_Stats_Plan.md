# K9 — living UI, companion tradeoffs and stat comparison

2026-10-14. Scope follows the complete animation audit and a fresh Bizarre Brigade1.5+ stat comparison. Preserve movement-only controls, no upkeep/permanent stat progression, uncapped immortal companions and random recruit weapons.

## Steps

1. Verify reference stat labels/definitions from footage and distinguish core stats, weapon properties, conditional relic effects and deliberately omitted systems.
2. Introduce per-companion scoped positive/negative modifiers with explicit tier scaling, read-only sources and transactional purchase/merge previews.
3. Reserve top-right for animated bond icons; move/hide controls by platform. Add dialogue cadence audio and synchronized merchant motion.
4. Add contextual animation across menu, roster, entry, combat/statuses, loot, shop/merge, map, details/results and settings; retain reduced motion/audio, pause and danger readability.
5. Regression, native Windows evidence, balance diagnostics, build, comparison table and open-gate report. Human art/audio approval is never inferred from automated checks.

Only runtime-authored animation is promised by code changes; do not describe four-frame sources as dozens of new hand-drawn poses. No reference artwork/audio is copied into assets.

## Outcome

Steps 1–5 completed for this Windows candidate: 33 focused checks, legacy regression, 107 native assertions, screenshots and two silent framebuffer movies. Known companion traits are previewed before purchase; random weapon outcomes are not presented as predetermined. Detailed scope/gaps remain in `K9_Animation_Coverage.md`, `Bizarre_Brigade_15_Stat_Comparison.md` and `K9_Final_Checkpoint.md`. Human quality and device gates remain open.
