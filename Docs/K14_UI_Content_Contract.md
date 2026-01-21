# K14 content contract for the UI owner

No UI/audio files are modified by K14. Read WeaponDefinition.GameplaySummary
for a short authored explanation; do not display the numeric enum as a label.

| Weapon ID | Label | Role |
| --- | --- | --- |
| ClockworkYoYo | Clockwork Yo-Yo | Melee-class homing throw, tether, return |
| ScrapMagnet | Scrap Magnet | Ranged-class cone, pulls/damages up to four enemies; no boss pull |
| NeedleSinger | Needle Singer | Three piercing needles; brief slow, staggered needle motion |
| SpringKnocker | Spring Knocker | Long spring compression, release, splash and push |
| DreamProjector | Dream Projector | Special-only lingering spotlight pool |
| RibbonWheel | Ribbon Wheel | Two delayed wide cuts, rotating recovery |

CoilKettle, ThimbleMoth and ReelOwl have DisplayName, concise Description,
four real authored poses and scoped bonus/tradeoff modifiers. Themes are hearth,
archive and astral respectively. ReelOwl is a dream recruit; normal themed pools
retain existing eligibility and recruitment quotas. Star scaling remains active.

Weapon names automatically fall back to authored IDs if the existing UI dictionary
has no override. Add the above labels through the UI owner's normal localization
path; gameplay does not depend on a UI translation. Existing audio Motion/Family
routes are reused; no new audio assets or new UI panels are added by this patch.
