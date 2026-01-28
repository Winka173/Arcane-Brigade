# K25 - specialist enemies, affix overlays and carrier variety

## Shipped scope

Eight new enemy species join the existing twenty. Each has original normal/evolved art and an actual body-driven special ability; none holds an ally weapon. The separate five-affix system can attach to existing species, including their evolved forms, without replacing their silhouette or changing shared definitions. This closes the species/overlay backlog listed in K24.

| Species | First wave | Evolved wave | Special behavior |
| --- | ---: | ---: | --- |
| Smoke Wraith | 6 | 12 | Sideways smoke feint and fading afterimage; evolved shadow lashes from the old position. No invulnerability. |
| Blue-Flame Fiend | 7 | 12 | Three slow curved ghost flames; evolved four-shot fan. Curves do not track the player's current position. |
| Red-Flame Berserker | 8 | 13 | Locks an aim, winds up and charges with a narrow body strike; evolved leaves embers at departure. |
| Twin-Flame Furnace | 10 | 15 | Alternates curved blue volleys and red rushes; evolved rush adds a landing pulse. |
| Ash Swallower | 9 | 14 | Eats up to three nearby real death remnants to enlarge its volley; evolved spits a hot pool too. Remnants expire after seven seconds, globally bounded to 24. |
| Lost Lantern Soul | 10 | 15 | Visible soul orbits its shell; shots originate at the orbit. Alternates volleys and shell jumps with landing pulse. One shared health pool, not two separately targetable enemies. |
| Smoking Coffin | 11 | 16 | Lid windup summons two short-lived creatures, three when evolved. Below half health, switches to charging and stops summoning. |
| Ember Procession | 12 | 17 | Creates two followers once. Followers form up and later flank. Killing the leader breaks formation and causes a short hesitation. |

Evolution adds an anatomical stage and the listed attack variation, plus 10% body height. It does not add another hidden health/speed multiplier on top of the existing wave scaling. The spawn system limits live primary specialists to three; additional swarm bodies remain the original small contact enemies.

## Affixes on existing enemies

- **Black smoke:** lateral feint, temporary dark afterimage, no invisibility immunity.
- **Blue flame:** three slow curved ghost flames at 75% of the source weapon's damage each.
- **Red flame:** telegraphed locked-target charge, narrow strike and extended recovery.
- **Blackfire:** one burst below 40% health, preceded by 1.2 seconds of anticipation. Not an unannounced death bomb.
- **Twin flame:** alternates blue volleys and red rushes. Attached flame palette changes with the cycle.

Affixes start at wave five. Per primary spawn chance rises from 8% by one percentage point per wave, capped at 24%. Conditional distribution: smoke 35%, blue 25%, red 25%, blackfire 10%, twin 5%. At most four affixed enemies are alive at once. Assignment is a deterministic hash of run seed, wave and unit ID, independent of the shop random stream. Bosses, swarm extras and summons are excluded. No blanket stat increase is attached to an affix.

Specialists start casting after a paced initial delay. Specials have roughly 6.5-7.7-second start-to-start cooldowns; affixes 9-11 seconds. Their casts share the existing evolution cap of four concurrent windups. A creature does not start a second special during basic attack, contact windup, evolution cast, recovery or travel. Targets lock before release. Killing during anticipation cancels the unreleased attack.

## Summons and economy

Summons have a short entrance, reduced current health, a twelve-second lifetime and a global twelve-live cap, also constrained by the arena enemy budget. They cannot recursively summon, gain affixes or run evolution attacks. They drop no direct coins, eggs or hearts, and do not contribute to the every-sixth-swarm-coin counter. Existing player on-kill items can still react to an actual summoned-enemy kill; this is not a blanket removal of every indirect economy interaction. Lifetime expiry creates only a cosmetic death event, not a scored kill or loot drop.

The prior K24 swarm payout and shop price rules are retained. Ten champion bot routes at seed 171077 now produce six victories and four defeats, waves 14-20. This is one fixed-policy seed, not a win-rate target or proof of human balance. See `PreviewExports/K25/Campaign.csv` and `Economy.csv`.

## Presentation and travel

Painted smoke/flames attach to the animated body, with different phase offsets, cast emphasis and hit response. Death disperses them, with a twelve-effect global cap. Lost Lantern Soul's orbit has its own ground shadow; all bodies retain the shared ground-shadow system. Painted hostile shots keep their own material silhouette rather than the generic projectile halo. Specialist/affix lashes, pulses and pools use separate painted wisps instead of geometric red field outlines or filled targeting discs. Their damage shapes and timing remain unchanged. Reduced Motion/Effects/Flashes retain readable warnings and shorten transfer motion.

Map transfers rotate every four waves: hand at wave 5, carpet-tongue at 9, keyhole-eye at 13, hand again at 17. These reuse the same original anatomy later fought on the Outside Keeper. Each carrier has distinct approach, authored poses and pickup/throw trajectory; no scene loading. They are sprite/transform animation, not a physically simulated cloth roll or fully enclosing 3D wrap. Optional boss entry always uses the hand. The large wave stamp waits until transfer completion so it no longer hides the carrier animation. Transfer presentation advances by at most 50 ms per frame, preventing a long stall from skipping the whole pickup/throw. Native capture waits for actual transfer progress rather than assuming wall-clock time equals animation time.

The lantern source uses explicit uneven-row crop boundaries. The K24 anatomy atlas classifies alpha components into uneven row bands and uses main-component sprite bounds rather than merging every detached fragment into a pose. Both preserve sprite IDs. These are rectangle imports, not per-pixel repainting, masking or upscaling.

## Validation and handoff

Use `ArcaneBrigade.Editor.K25EnemyAffixBuilder.Build`, `Verify`, `Regressions`, `Campaign`, and `VerifyAndBuild` in Unity batchmode. Native capture uses the existing isolated K7/K18 flags plus `-arcane-k25-affixes`; reduced capture additionally needs `-arcane-k17-reduced`. Save data is isolated from normal player profiles.

Exact Windows build, native checks and accepted screenshot paths are recorded in `PreviewExports/K25/Gate_Summary.md`. No automated fixture substitutes for human art/animation acceptance or real-device performance testing. Android/iOS device gates and publishing remain open. Audio belongs to the other agent and is not regenerated, balanced or staged here.
