# K23/K24: payoff, optional encounter and swarm direction

## Implemented scope

- Star-up reveal: 1.9-second staged gathering, compression, oversized reveal, landing and staggered stars. Receipt/queue deduplication remains. No empowered first attack or extra stat award.
- Three allied cooperation recipes: fans spread another ally's fire; hammers fracture another ally's orb; a real resisted pull can set up a short melee sweep window. Secondary fragments do not recursively cooperate. Fan/hammer direct projectile damage is redistributed, not tripled. Owner damage and helper assists use separate ledger entries.
- Boss death presentation lasts 2.4 seconds and uses material/motif collapse, detached organs, smoke/embers and visual-only coin spray. Actual income is unchanged by that visual spray. Boss-to-shop delay waits for the spectacle; the player may move during the safe hold. Floor hue eases toward a calmer palette. Existing hostile projectiles fade visually rather than being allowed to damage during victory.
- Existing four-wave map-transfer cadence now uses an original giant porcelain hand with the cracked bell. It carries and tosses the party; companion positions are translated with the champion at arrival. No scene load is added by the hand animation.
- After three transfers (normally entering wave 13), a separate hand invitation becomes available in the shop. Hold Space/controller A near it for 0.85 seconds to accept. The ordinary exit declines without a currency penalty. This is one opportunity per run.
- The Keeper Outside the Stage is a separate optional encounter, excluded from the ten ordinary boss rotations. Three health phases unlock six timed patterns using the carrying hand, living carpet tongue and keyhole eye. These use existing falling-prop/lash/pool/impact-wave damage primitives, not six new VFX engines. Body anticipation is 1.35 seconds; target positions are locked and broken organs lose their pattern.
- Defeat ends the run. Victory returns three saved prismatic choices; choose one free reward, with no reroll or ordinary-shop purchases during selection. Solo protagonists receive only relic choices. Normal stock, wave number and shop economy are preserved; the optional victory does not grant another ordinary-wave income payment. Restarting/loading the same reward state cannot claim twice.
- Appended rarity ordinals: Crimson (red, above Mythic/gold) and Rainbow (prismatic, above Crimson). Two unique Crimson relics can appear from wave 10 at weight one and higher base price. Existing special recruit EclipseEgg is promoted to Crimson, unlocked from wave 10 and priced at least 80 before inflation; its rarity damage scale rises accordingly. Four prismatic relics and two prismatic allies are reward-only. Current relic effects are strong combinations of existing modifiers, not a promise of four entirely new signature systems.

## Weapon and swarm changes

- Melee range multiplier increases from 1.35 to 1.85, and art-width multiplier from 1.55 to 2.15. Existing melee damage multiplier remains 1.5; no additional unrequested damage spike. Visible companion weapon limits expand to match rather than silently clamping the bigger art back down.
- Ranged held weapons become 45% wider before the companion display bound, without increasing projectile damage or physical hitboxes. Companion melee ink bounds fit 1.55–2.5 body heights; ranged fit 0.95–1.4. Weapons and projectiles remain separate visual quantities.
- Expanded-content waves spawn packs rather than one enemy per tick. Wave one remains one-at-a-time contact-only. Wave two can spawn pairs. From wave three, a 12-second cycle has a six-second crest, two-second taper and four-second breathing interval. Pack size ramps gradually to twelve, not all at wave four.
- Additional pack bodies are small contact/melee species with 45% of their compiled health, not extra ranged or heavy elites. Primary pack leaders retain the regular roster variety. The existing safety radius still rejects spawns too close to the player.
- Every sixth extra minion kill drops the ordinary coin amount; the others do not mint additional coins or roll egg/heart loot. Regular leaders retain ordinary drops. This makes larger packs satisfying without multiplying income by pack size. Wave-scaled prices receive a gradual extra 2% per wave after wave three, capped at 30%.
- Initial live-enemy budget is raised gradually and capped at 240 before existing item/wave modifiers; this is not an unconditional 240 final cap or a verified mobile performance budget.

## Art and validation limits

Built-in painting created `Art/K24/OutsideStage_v1.png` and `OutsideRewards_v1.png`; sixteen drawings each. Hand/core/tongue/eye share a coherent original material palette. This is not native-4K art or continuous skeletal animation. Exact prompts are in `Docs/K24_Art_Prompts.md`.

Focused checks, build and native Windows evidence are recorded in `PreviewExports/K24/`. Human acceptance, long-run balance, mobile devices and profiling are separate from automated checks. Audio is actively edited by another agent; those worktree files are neither regenerated nor staged as part of this gameplay change.

## Historical backlog at the K24 checkpoint

Follow-up status: K25 now implements all eight species, the five affix overlays, and carpet-tongue/keyhole-eye map-transfer variants. See `Docs/K25_Enemy_Affixes.md` and its current validation report. The paragraphs below describe the earlier K24 checkpoint only.

The eight previously approved special enemy concepts and the black-smoke/blue-fire/red-fire/blackfire/dual-fire overlay system remain separate planned work. Carpet-tongue and eye-based *map transfers* also remain an extension: this batch ships the hand transfer, with tongue and eye already present as encounter anatomy. Do not describe these backlog features as complete.

Approved species backlog: Smoke Wraith (Ám Khói), Blue-Flame Fiend (Hỏa Ma Lam), Red-Flame Berserker (Cuồng Hỏa Đỏ), Twin-Flame Furnace (Lò Song Hỏa), Ash Swallower (Kẻ Nuốt Tro), Lost Lantern Soul (Hồn Đèn Lạc), Smoking Coffin (Quan Tài Hun Khói), and Ember Procession (Đám Rước Tàn Hỏa). The separate overlay system must retain the underlying species/evolution silhouette and add a paced ability, not just recolor the enemy.

## Automated economy observation

Ten deterministic champion routes at seed 171077 produced seven victories and three defeats, reaching waves 14–20. Successful routes recorded roughly 11,000–13,000 kills rather than the earlier roughly 2,000; this is a bot observation, not a performance or human-fun gate. Late-shop examples still spend most available gold (LanternKnight wave 18: 418 before, 7 after; TreasureAcorn wave 19: 776 before, 83 after). Final victory wallets include the last combat without another shopping round and should not be mistaken for disposable money at every shop. See `Campaign.csv`/`Economy.csv`; more seeds and human play remain necessary.
