# K8 audio-only audit remediation

2026-10-14. Scope: four Presentation audio files, new K8Audio editor tooling,
new Resources audio filenames and this provenance document only. K7 documentation
describes the historical pitch-reuse implementation; this document supersedes it
for runtime music. Existing source assets, builders, game/data/UI are unchanged.

## Source and license

All downloads came from links on primary author asset pages, verified to list CC0:
- Menu: [(Basically not) Fusion Jazz](https://opengameart.org/content/basically-not-fusion-jazz), Julie Damsgaard / Spring Spring / Spring Enterprises. Existing verified source copied without modification.
- Shop: [Coffee House Bump](https://opengameart.org/content/coffee-house-bump), OatCog. Original WAV downloaded successfully.
- Combat: [Beat 'Em Up Jazz Club](https://opengameart.org/content/beat-em-up-jazz-club), Spring Spring. Original OGG downloaded successfully; chip-jazz, not a live jazz band.
- Boss: [Boss Battle Theme](https://opengameart.org/content/boss-battle-theme), Cleyton Kauffman. Original OGG variation downloaded successfully; rock/orchestral contrast.

Exact download URLs, filenames, license links, SHA256 and credits ship in
`Assets/ArcaneBrigade/Resources/Audio/Licensed/K8-Audio-Credits.txt`.
Original bytes are retained, with streaming stereo import. No executable downloaded.
CC0 is verified from each uploader's page, not inferred from a collection/search tag.
Kenney's three existing original license files were inspected; sampled SFX stay CC0.
No Bizarre Brigade audio is used. No exclusive authorship or human approval claimed.

## Playback changes

- Four genuinely different compositions at pitch 1; menu now plays by default.
- Victory/defeat return to the menu theme. Missing music logs a warning and stays
  silent, rather than disguising the combat track as another phase.
- Latest enabled music and cue component owns playback. Disabling a sandbox/scene
  restores the preceding enabled interface owner. Only active/fading music plays.
- Phase changes crossfade with unscaled time. Master listener, MusicVolume,
  SfxVolume and danger ducking remain governed by PresentationSettings.
- Manual pause lowers music to 28%; reduced audio lowers it to 75%. Focus/background
  pause preserves music position, stops transient SFX, and blocks new cues. Existing
  listener pause is respected without changing shared focus or UI code.
- Eight SFX voices: two reserved danger, two interface, four cosmetic (two reduced).
  Cosmetic playback has a global 35ms / reduced 85ms gate in addition to per-cue
  cooldown. Occupied cosmetic voices are not stolen. Danger has independent clocks,
  a 60ms global gate and 180ms hurt / 400ms warning cooldown, unchanged reduced gain.
- Cosmetic reduced gain is 65%; danger ducks cosmetics to 55% and music through
  MusicDuck. Interface voices remain independent; navigation has a 100ms gate.
- Hit and death use explicit Paper/Metal/Wax/Fabric/Glass/Wood palettes. Chest
  opening uses confirmation, not the glass/projectile cue. Procedural missing-source
  Foley also differentiates all six materials rather than sharing one impact tone.
- Runtime borrows Resources clips, destroys only generated fallbacks, and configures
  idempotently. Legacy builder Cue API still returns an owned copy.

## Focused verification

Existing local FFmpeg decoded all four source files successfully (no new tools):

| Phase | Duration | Rate / channels | Mean dBFS | Peak dBFS | Runtime trim |
| --- | --- | --- | --- | --- | --- |
| Menu | 246.39s | 48kHz stereo | -16.9 | 0.0 | 0.18 |
| Shop | 156.00s | 44.1kHz stereo PCM16 | -20.2 | -2.6 | 0.26 |
| Combat | 76.80s | 44.1kHz stereo | -9.7 | 0.0 | 0.08 |
| Boss | 114.34s | 44.1kHz stereo | -14.4 | -0.0 | 0.13 |

Static trims bring mean levels near -32dBFS at music bus 1, avoiding a loud combat
jump without altering originals. RMS matching is not perceptual loudness approval.
Source peaks near full scale are reported honestly; runtime attenuation provides
headroom, not proof that the original masters never clip.

Run `ArcaneBrigade.Editor.K8AudioChecks.Run` for hashes, imports, licensed borrowing,
event mappings and fallback sample checks. Run `K8AudioChecks.RunPlayback` (batch
without `-quit`) for actual play-mode ownership, pause, focus, density and danger
tests. Automated checks do not approve taste, final in-game mix, loop seams or
cross-genre fit. Seamless loops are not claimed for every author source.

`ArcaneBrigade.Editor.K8AudioBuilder.Apply()` idempotently configures only the four
new track importers and runs source checks. Existing `FullRunBuilder` copies every
licensed audio `*.txt` into `Builds/FullRun/ThirdPartyNotices`, including the new
`K8-Audio-Credits.txt`; no shared build generator changes are needed. Alternate
builds may call `K8AudioBuilder.CopyCredits(buildDirectory)` explicitly.

Validation completed: Unity 6000.4.8f1 `K8AudioBuilder.Apply` exited 0 with 113
source checks; `K8AudioChecks.RunPlayback` exited 0 with 27 live playback checks.
Reports: `Logs/K8Audio-Source-Checks.txt`, `Logs/K8Audio-Playback-Checks.txt`;
Unity logs: `Logs/K8Audio-Apply.log`, `Logs/K8Audio-Playback.log`.
Focused `git diff --check` passed. No final game build or human sound approval claimed.

Integration follow-up: legacy K7 audio assertion inspected literal allocator code
from the old two-danger/six-cosmetic layout. K8 intentionally uses two-danger,
two-interface, four-cosmetic (two reduced). The legacy audio-only assertion now
checks the actual shared allocator against all 256 occupancy masks, both density
modes and both reserved cursors, including priority ordering and full saturation.
No cosmetic or interface request can allocate danger voices 0-1. The same check
is included in K8 source checks (now 114). This follow-up has not yet been rerun
in Unity; the main integration owner requested no parallel Unity launch.
