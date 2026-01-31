# Smaller, scattered antique lanterns — 16/10/2026

Windows x64 build succeeded with **0 errors / 0 warnings**. Updated player: `Builds/HeroIdentity/ArcaneBrigade.exe`. FullRun was already open and was left running with its previous build.

- Eight lamps now stand 1.6–1.9 world units high, previously 2.7. Each has a seeded size, permanent left/right lean and independent hover phase.
- Seeded placement scatters lamps inside separated map sectors. Locations survive scene recreation and wave/damage progression; another run changes them. Shop lamps use staggered side positions, with the right lamp above the journal buttons.
- Flame bloom, floor projection and shadows follow the smaller tilted art. Light radius stays unchanged. Reduced Motion preserves the composed lean while removing drift/sway. Pause freezes the poses.
- **512 editor assertions pass**, including **321 added layout checks across 64 seeds**. Native Windows: **260 normal + 260 reduced assertions pass**, **5 screenshots per mode**. Captures reject black frames, check actual dimensions, floor shadows, lamp scale, flame/light alignment, pause, seeded reloads, clear shop stations and camera framing.

Fresh captures and assembly fingerprints: [Verification.json](Verification.json), [Capture_Runs.json](Capture_Runs.json). Visual samples: [combat, first seed](Native/scattered-lamps-wave-1.png), [combat, another seed](Native/scattered-lamps-another-run.png), [shop](Native/smaller-asymmetric-shop-lamps.png).

Runtime assembly SHA-256: `9C603AF7375B9B71F13C8467B7EFF13261B677F9C96D50AB764123DF8DAE91D8`.

This is a focused lamp regression, not a complete campaign replay or a frame-time benchmark. Screenshots and position checks cover the named fixtures; they are not human approval of every seed/map angle. Existing room backdrops and composite painted furniture were not separated into additional objects.
