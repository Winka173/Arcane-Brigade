# Ensemble animation and presentation pass

## Implementation sequence

1. Retain existing identity-specific painted combat/evolution sets; add independent surface response and source-preserving species deaths.
2. Author missing fan, parasol and gun mechanisms as transparent painted action clips; retain stable grip scale and real combat timing.
3. Paint all four merchant handover identities; integrate with committed physical-shop transactions, not a disconnected preview.
4. Complete party look-back/farewell choreography for the saved roster, with accessibility and skip behavior.
5. Review the full catalog in native Unity Windows, then measure a dense actual-combat fixture and run story/save regressions.

## What is actually implemented

- Existing individually painted walk, blink, windup, release, recovery and mutation art is preserved. The earlier short backlog description understated how much K16/K21/K22/K25 already supplied; these sets are not replaced by generic faces.
- `EnsembleMotion` adds independently phased, bounded face/mouth/fabric surface response and physical-family gestures. It does not invent eyelids, mouths, arms or anatomy that the source does not have. Attack/hurt/telegraph readability takes priority. The new surface channels are GPU source articulation, **not newly hand-painted facial/cloth layers for every actor**.
- Ordinary enemy deaths retain each enemy's actual painted silhouette and palette. Species/family/identity and evolution determine drift, folding/ratcheting/buoyancy and erosion. Evolution changes erosion seed/intensity without changing rewards or stats. These are source-driven procedural death sequences, **not a claim of a newly painted death sheet for every species**. Existing individual boss finales remain intact.
- All weapon definitions retain their real windup/recovery and motion families. New painted mechanisms cover PaperFan, MoonFeatherFan, ParasolNeedle, InkPistol, ShellCannon, PlasmaTrident and FireExtinguisher: **44 additional painted poses**. Existing hinged scissors, extending camera lens, winding key and mailbox flag remain. Other weapons gain bounded tension/release surface response alongside their existing attack motion; no fake shots, range or damage are added.
- All four merchant identities receive their own painted handover: **24 poses** across ReceiptMaw, EggOracle, GiantGoldMerchant and GiantEggMerchant. The giant vault uses its receipt tongue, not invented arms. Actual purchase/fusion/reroll celebration starts the clip, it settles after 1.05 seconds, and Reduced Motion keeps the original stable source. Prices, merchandise anchors, rarity aura parenting and currency rules are unchanged.
- The farewell overlay uses the real champion, acquired weapons, tiers, rescued guests and roster. Actors look back, bow/flutter/bounce according to their physical identity, then return toward the real dawn door. Compatible bodies use their new painted curiosity/cheer frames. Large parties use lanes and a bounded total stagger rather than placing the entire tail offscreen for several seconds. Continue remains skippable; no new ending, compulsory room, control or persistent stat reward is added.

## Performance and safety

`AnimationDetailBudget` derives two ornament-detail thresholds from real unit/projectile counts. Under pressure it sheds surface decoration, secondary penumbrae/cast shadows on ordinary units and extra affix adornments. Contact shadows remain; champion/boss shadows remain detailed. It never removes real projectiles, collision, damage, enemy decisions, attack tells, shop interaction, loot or enemies.

Enemy actors no longer allocate a complete unused weapon-shadow hierarchy when their weapon grip is hidden. Disabled soft-shadow layers no longer receive all transform/color updates. Heavy ornament shader work is skipped when its channels are zero. Resources and shared identity phases are cached; no combat/shop RNG is consumed.

The guarded stress fixture runs only with isolated K7 diagnostic storage. It adds 420 high-health enemy fixtures in the actual arena simulation, maintains a dense hostile-projectile stream, moves the champion and samples native frame times, managed heap/collections and Unity allocation. Modified definitions are runtime clones, not source assets or player progression. Normal and crowded frame timings are recorded separately in `Performance.json`; a `P95Within60Fps` field is a measured result, not a guaranteed pass or device certification. Hidden-window graphics timing is not a substitute for visible-window frame-pacing or mobile-device testing.

The first measurement is wave 17, not a quiet first-wave baseline. The second adds 420 units to the enemies already alive; `PeakUnits` records the actual total. Projectile injection targets a stream below 320 existing projectiles, but collisions can remove them before a sampled frame. `PeakShots` is the observed count, not a claimed 320-projectile acceptance gate. CPU sampling separately records simulation/render P95 and main-thread managed allocation; these do not include all GPU, audio or feedback work.

Spatial-query ordering now reuses one `Comparison<UnitState>` delegate. `Tools/Animation/SortAllocationProbe.ps1` uses this installed Unity's Mono runtime: 1,000 warmed `List.Sort(IComparer)` calls allocated 64,000 bytes versus zero with a cached `Comparison`. The focused Unity test checks ascending identities, moved/dead membership and heavy-only query equivalence. Allocation assertions run only when a known allocation actually advances Unity's counter. The Windows player's counter returned zero even for a known allocation: `ManagedAllocationCounterAvailable=false` and `ManagedAllocatedBytes=-1` record that limitation, not zero-allocation gameplay. Heavy-body separation filters candidates before sorting while retaining the original sorted membership and movement response. This is not by itself evidence that dense combat meets 60 FPS. Spring-family lower-body motion is applied after base gait scale, so it is no longer overwritten during the same frame.

Integration preserves the Customer's six body/prop-based story attacks rather than layering the October generic encore volley over that final encounter. The optional-Keeper reward fixture restores diagnostic champion HP while waiting for real attacks; this isolates reward settlement from newly increased boss damage without weakening the production boss.

Enemy target selection directly checks the sole champion instead of sorting surrounding enemies which cannot be hostile targets. Strict range boundaries, defeated/entering champions and invalid ranges have focused tests. Heavy-body filtering and this targeting fast path do not change spawn counts, decisions, target eligibility, damage, timing, economy or RNG. K24's historical fixed 1.5-times reach assertion now checks that actual reach still exceeds source reach after the current October balance pass; it no longer conflicts with the deliberately changed melee multiplier and late-wave range adjustment.

The user's established economy rule remains: non-gold currencies do not inflate with later waves. `SpecialStagePriceFactor` stays one; rarity, veteran and evolved-tier premiums remain separate. K26 checks this contract. Restoring wave inflation while integrating another content branch is a regression, not an animation improvement.

## Art provenance and prompts

Built-in image generation only; final selected PNGs live in `Assets/ArcaneBrigade/Art/Ensemble/`. No external runtime file dependency or third-party game asset is introduced. PNG alpha is preserved. Unity sprite import/constant-canvas metadata aligns animation poses without externally editing the bitmaps.

Merchant prompt: preserve the exact reference's grotesque haunted-toyshop identity, palette, detailed ink painting and anatomy. Continuous anticipate/look-down/extend-existing-appendage/cradle-invisible-purchase/release/withdraw/chuckle/settle poses, four columns, transparent separated full-body cutouts. No actual purchased item is baked into the drawing. No extra arms on the giant vault, text, background, shadows, target circles or cropped appendages. Giants use four principal poses; small merchants use eight.

Weapon prompt: preserve the exact source weapon and its mechanical parts; same scale, grip axis, right-facing muzzle and stable composition. Eight load/release/recovery key poses, or four principal poses for the fans/extinguisher. No projectiles, firing effects, hands, characters, labels, background or ground shadow. Bulb/liquid, shell bellows, plasma cylinders, extinguisher lever/hose, fan ribs/feathers and parasol ribs animate according to their mechanism.

Technical framing edits preserve poses and shrink each cutout within its cell, guaranteeing actual transparent gaps. Opaque-background variants and silhouettes touching their neighbors were rejected, not silently treated as valid alpha. Weapon import keeps a constant cell canvas and records the painted ink reference width to avoid per-frame scale pumping.

| Project filename | Reference | Accepted generation |
| --- | --- | --- |
| ReceiptMaw_Handoff_v1.png | `Art/AnimationHD/StrangeMerchants_v2.png`, cash register | `exec-af86f92a-94e4-4377-887c-a9512cbf96d4.png` |
| EggOracle_Handoff_v1.png | Same source, egg oracle; separated-cell correction | `exec-8f3fac85-ed9f-404e-a1e0-9d8b15357503.png` |
| GiantGoldMerchant_Handoff_v1.png | `Art/K11/GiantGoldMerchant_v1.png`; separated-cell correction | `exec-fb68c0c2-127a-4f14-b703-354302f258d2.png` |
| GiantEggMerchant_Handoff_v1.png | `Art/K11/GiantEggMerchant_v1.png`; separated-cell correction | `exec-c5a9d353-8518-4e6d-97cb-7f953a7cefb4.png` |
| PaperFan_Action_v1.png | `Art/K10/PaperFan.png`; separated-cell correction | `exec-6100b75f-98d5-46ed-842f-80a5e97a4cb5.png` |
| MoonFeatherFan_Action_v1.png | `Art/K13/DreamArms_v2.png`, feather fan | `exec-95fcb381-c065-4a06-95a4-7b433c1c5955.png` |
| ParasolNeedle_Action_v1.png | `Art/K13/ToyBlades_v2.png`, parasol | `exec-1ef24cdd-0484-4c2e-900b-796651450e2c.png` |
| InkPistol_Action_v1.png | `Art/K10/InkPistol.png` | `exec-a788f154-72f3-4f47-8fcc-30f6bddc40a7.png` |
| ShellCannon_Action_v1.png | `Art/K10/ShellCannon.png` | `exec-163b90ec-2f14-4bea-9c19-d53e028f86bf.png` |
| PlasmaTrident_Action_v1.png | `Art/K13/DreamArms_v2.png`, plasma pistol | `exec-11b24193-77ed-4cfb-b287-180c753f5b5d.png` |
| FireExtinguisher_Action_v1.png | Same source, extinguisher | `exec-5744195e-6f33-44b7-8920-f6e6d5917b47.png` |

Generation originals are under `ArtSource/`; the game uses project copies only.

## Verification commands

- Editor import/regression/Windows build: `ArcaneBrigade.Editor.EnsembleBuilder.Gate`.
- Full native catalog, real shop purchase and crowd measurement: `Tools/Nightmare/Capture.ps1 -Ensemble -Stress -Player Builds/FullRun/ArcaneBrigade.exe -Output <folder>`; add `-Reduced` for the accessibility profile. Run timing measurements sequentially, not against another concurrent player.
- Separate ordinary Nightmare captures verify story/save/ending compatibility.
- `PreviewExports/Ensemble/Coverage.md` inventories every catalog identity, including the final Customer and optional Outside Keeper, and distinguishes existing painted sets, new art and procedural response.

Catalog boards normalize body height and cap weapon width for their ten-slot layout. They review pose continuity and resource coverage, not gameplay proportions or range balance. Real shop and stress scenes use the production run-compiled actors. Stress health is deliberately diagnostic, so its HUD health numbers are not a normal-player UI acceptance sample.

Final artifact hashes, actual counts, corrected failures and measured timings belong in `PreviewExports/Ensemble/Gate_Summary.md`. Human-perceived quality, every animation being individually hand-painted, and Android/iOS device acceptance must not be inferred from passing automated tests.
