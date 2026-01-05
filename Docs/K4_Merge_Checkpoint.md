# K4.3 merge / silhouette / personality checkpoint

**Final Windows integration:** merge checks + full regression PASS; native quầy→save→tier2 merge→reveal finish/skip PASS. Main đã nối profile reduced-motion cho active/pooled visuals. Báo cáo có ưu tiên `K4_Final_Checkpoint.md`; pending handoff dưới đây là lịch sử, không phải hạng mục functional K4 còn thiếu.

Status: Unity suite PASS, including merge checks; native committed-merge completion and skip PASS, as reported by the main agent on 2026-10-13. The reviewed `Merge_Reveal_4K` screenshot shows the actual tier-2 Umbrella result. This verifies the tested K4.3 slice, not final art/audio/device approval or all K4 work. Unity/native execution was performed by main, not this file-scope worker; no Unity launch, commit, push, font replacement, UI redesign or gameplay-rule changes by this worker.

Verification limits: snapshot-helper checks cover detached payloads, rollback/reload/deduplication and synthetic altar/read-only states; they are not native captures of every altar, cascade, rapid-purchase or accessibility path. K7 still owns 32–48px/device readability, all-family silhouettes, weakspot/danger contrast, motion tuning and audio audition/mixing. Font/UI redesign remains K6.

## Transaction boundary

- `WorldShop.Interact` captures the pre-transaction snapshot immediately before `BuyOffer` / `FuseAltar`. It enqueues only after their successful return. `ArenaController.Transaction` returns true only after `RunTransaction.TryCommit` completes persistence, then rebuilds rest presentation.
- The queue additionally requires same run, shop phase, exactly one new receipt counter and that receipt present in committed history. Failed operations/save rollback cannot create a success beat. Receipt ID deduplication survives skip within the component; loading never reconstructs animation from receipt history.
- Detached before/after roster tuples, not localized receipt strings, supply consumed bodies and the actual final weapon/tier. The recruited third body's consumed weapon is not observable in those snapshots: it is represented by a body-shadow only, never guessed or rerolled. Cascades reveal final committed results with consumed prior bodies; intermediate rolled weapons are deliberately not invented.
- Normal and altar beats queue without confirmations. Altar uses its actual two-material delta and separate ring color/orientation/label. Animation is 0.9 seconds, using disposable visual `UnitState`/definition copies, no colliders or simulation registration. Gameplay positions, roster, RNG, currency and bonds are never written by reveal. Existing receipt/Details remain authoritative for bond and transaction deltas.
- Phase exit, read-only inspection/pause/navigation, disable, run change and explicit `Skip()` dispose only presentation. No reload/replay logic, save schema changes, new rolls or material consumption.

## Visuals

- Tier one art is unchanged. Higher tiers alter weapon proportions by family: broader umbrella, tall stamp head/handle, taller bow, and a distinct smaller bell satellite. These remain visible with particles off and add no arms/forearms. Hand and muzzle anchors remain unchanged. Tier changes use `CombatTier`, already populated by `RunSession.BuildUnit`.
- Companion idle beats last 0.65 seconds every 6.4 seconds, with deterministic per-unit phase/sign. Bell rattles, stamp checks its grip, other families test a small pose. Attack, pending attack, hit, death and telegraph suppress personality; no RNG, cooldown, velocity or leash changes.
- Reveal includes a local ring, existing-font tier/actual-weapon label and original two-note synthesized confirmation study. Native 4K tier-2 Umbrella result visibility has been reviewed; all-family/32–48px/device silhouette review and audio audition/mixing remain K7 refinements. Not final art/audio approval.

## Integration contracts and remaining limits

1. Main reports the full Unity suite, including `ArcaneBrigade.Editor.K4MergeChecks.Run()`, PASS. Keep this entry in K4 validation orchestration alongside regression checks. This entry does not build, save assets or write reports. Unity-generated `.cs.meta` files remain outside this worker's edit scope.
2. Current physical purchases/fusions are wired automatically by `WorldShop.Configure`; no prefab or controller edit is needed. Any new direct `ArenaController.BuyOffer`/`FuseAltar` caller bypassing `WorldShop.Interact` must capture before and call `Shop.Reveal.EnqueueCommitted(before, Session.State, successfulReturn, visualAnchor, isAltar)` after success. Do not enqueue from `persist` or replay stored receipts. If centralizing this hook in `ArenaController.Transaction`, remove the WorldShop hook rather than maintain two observers.
3. `WorldShop.Configure` and `Display` synchronize `Shop.Reveal.ReducedMotion` from `owner.Profile.ReducedMotion`, safely defaulting false when profile is absent. Main/menu workers own the profile field and setting; active/pooled `UnitVisual.ReducedMotion` must also follow that setting. Reduced motion keeps tier shape, label and audio but suppresses gathering/idle. Set `Shop.Reveal.ReadOnly = true` for any embedded read-only preview/sandbox. Inspection pause is handled automatically; a standalone preview must not call shop transactions.
4. Backspace at unpaused rest calls `Shop.Reveal.Skip()` only; no gameplay action, mandatory input or modal is introduced. Existing continuation immediately cancels queued presentation when leaving shop. Native capture can inspect public `Shop.Reveal`, read-only `PendingCount` and `CompletedCount`. Completed counts naturally finished beats only, not skips/cancellations, and resets on the first observed transaction of a different run.
5. Death pose copying currently has no satellite destination. If terminal bell satellites must persist into corpse presentation, extend `DeathFeedback` separately; tier weapon proportions already copy through `CopyDeathPose`. No gameplay dependency.
6. Native committed merge completion/skip and the reviewed tier-2 Umbrella 4K result are verified by main. Further native coverage of quick sequential purchases, altar/cascade variants, save failure/load and reduced motion remains distinct from snapshot-helper coverage. K7 reviews displacement, danger readability, hand attachment, combat priority and tier shell/motion/audio refinements across devices; font/UI redesign remains K6.

## Weakspot exposure presentation follow-up

`UnitVisual` reads the shared `UnitState.ExposedUntil` field without mutation. Live exposed enemies get four thin, static mint corner brackets sized from the current sprite bounds/body scale; no bitmap, opaque fill, flash or pulse. Active danger telegraphs suppress the brackets. Expiry/death/rebind disable them, terminal fade also fades them, and per-view material resources are destroyed with the view. The cue is static in reduced motion too. `WeakspotVisible` exposes the actual renderer state and `IsWeakspotVisible(unit, time)` exposes the eligibility rule for native/editor checks. Visual color, scale and danger-readability still require native review.

## Validation record

- Main-reported verification: full Unity suite including merge checks PASS; native committed-merge completion/skip PASS; `Merge_Reveal_4K` reviewed with actual tier-2 Umbrella visible. No additional native cases or artifact counts are claimed here.
- Worker validation: scoped `git diff --check` clean. This worker did not launch Unity under the shared-workspace restriction; main handles the final Unity rerun.
- Checks exercise actual `RunSession.Buy` through `RunTransaction.TryCommit`: save failure restores serialized state, successful merge reports actual rolled result, duplicate enqueue and skip do not replay, reload emits nothing, payload edits do not mutate state, synthetic altar delta supports tier-one results, and tier/idle guards are deterministic.
