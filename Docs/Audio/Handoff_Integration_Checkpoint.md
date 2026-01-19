# Offline audio handoff integration

Applied the 79-file AudioHandoff package onto main after aac039b, preserving
K13 gameplay, economy, roster and enemy scaling. The 42 supplied WAV files
(24 new, 18 retimed) and their 24 new metadata files are installed in Resources.
The original package and duplicate prompts are disposable only after validation.

## Implementation

- Source-driven combat mapping now distinguishes boss patterns, item procs,
  hero pulses, secondary hits, large enemies and volley versus impact.
- Appended simulation audio events for return/pierce/split/bounce, substantial
  knockback, loot drop/attraction, contact windup, chill, boss phase and mimic wake.
- Batched combat/sandbox routing, hit deduplication, off-screen culling, family
  cooldowns and companion attenuation; no simulation RNG consumed by new hooks.
- Gold/dream/HP purchase and denial semantics; hall exit is not a purchase.
  Altar fusion does not double-play the transaction merge. Bond and rarity cues.
- Shop carpet/keeper/stock choreography, focus and gesture hooks; ally arrival
  and tier-two/three reveal cues. Schedules freeze with gameplay, survive pause,
  are bounded/coalesced and obsolete market cues are cancelled.
- Champion footsteps, pursuit-entry dash, large-enemy roar/steps, UI state-change
  hooks for open/close/back/tab/page/tooltip/pause/resume/settings/unlocks/receipts.
- Updated contract generator resolves actual source line numbers; 141 source
  hooks, zero P entries, 12 deliberate reserved cues. Source hooks do not imply
  every sound is admitted: density/Reduced Audio gates intentionally discard some.

## Deliberate reservations

Redundant chr.jump/chr.land (ally arrival already has its complete lift/landing
clip), unused swing variants, enemy.hurt/sig.fish, shatter without a separate
mechanic, and theme/transition ambience without a theme-routing API. No fake
gameplay or redundant sound is introduced to consume these clips.

## Quality gates

- Final Windows build: succeeded, zero errors/warnings, 485,503,160 bytes.
- 1,697 library assertions, 114 legacy source assertions, 26 focused combat
  checks and 2,078 K13 regressions passed.
- Playback: 54 assertions passed, including interleaved hit deduplication, actual authored clips, scheduled
  pause/cancellation and four-stem select/dream-shop playback.
- Native Windows: 24 shop/merge/weapon checks and 39 horde/boss/transition
  checks passed, no runtime errors. 27 + 17 screenshots are in the report folder.
- Waveform validation: all 471 cue clips and 60 stems passed without warnings.
- Separate legacy difficulty-three scripted boss bot failed at hall 7; recorded
  in Legacy_Boss_Bot.txt, not hidden or treated as a passing gate. Audio integration
  uses RunFocused rather than changing game balance to satisfy this old bot.
- Wave-five fixture: 32 peak enemies, median/p95 16.67/16.68 ms on RTX 5070 Ti.
  This is not mobile/minimum-spec/max-roster performance certification.

Results are recorded in PreviewExports/AudioHandoff. Waveform/Unity playback
checks are technical verification, not an auditory review. The user's approval
of the earlier music preview is retained; newly supplied cues and new combinations
have not been listened to by this agent. Device/release gates remain excluded.
