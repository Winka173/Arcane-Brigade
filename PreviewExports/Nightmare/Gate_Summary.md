# Nightmare progression and event-story integration

Historical narrative gate. Current final-hall/farewell Windows delivery is in `FinalHall/Gate_Summary.md`; the counts and executable below are the earlier incremental gate, not the latest FullRun player.

2026-10-15. Windows-first incremental delivery, not release certification.

## Rules and persistence

- Unity 6000.4.8f1 focused narrative checks: 221 PASS.
- Secret-event regression with actual narrative commit/resolution assertions: 459 PASS. All 24 room choices remain covered. Losing or merely accepting an optional battle does not fabricate its completed story memory.
- Relic regression: 156 PASS.
- Outside Keeper / merge / cooperation regression: 48 PASS.
- Total distinct focused checks across these suites: 884.
- Latest combined player build succeeded with 0 build errors and 0 build warnings: `Builds/Nightmare/ArcaneBrigade.exe`.
- Editor compilation also reported a deprecated `GetInstanceID` warning in the concurrently edited `BuildMorphChecks.cs`; this is not a player build error and is not hidden by changing that other feature's code.

## Implemented

Five wave-depth layers, original scenery/shadows, 180+ contextual English lines, warm anchors, actual remembered outcomes from the eight existing rooms, final Last Customer battle with six attack sequences and breakable organs, four illustrated ending pages, saved page progress and explicit endless continuation.

Earlier native checks exposed row bleed in the first packed Customer atlas, boss captions overlapping the health bar and clipped scenery. Separate body/organ atlas drawings, caption relocation, landmark placement and aspect-preserving ending layout address those findings. The superseded packed atlas remains only as an original art source.

Final normal/reduced evidence is captured by `Tools/Nightmare/Capture.ps1` into `Final/` and `Reduced/`. Their `Native_Checks.txt` files are authoritative; screenshots alone are not proof that all assertions passed. Earlier `Accepted/` captures predate the final revisions and are not the final gate.

- Final normal native run: 31 PASS, zero failures, 11 nonblank 1920×1080 screenshots.
- Final Reduced Motion/Effects/Flashes native run: 31 PASS, zero failures, 11 nonblank 1920×1080 screenshots.
- Both modes verify actual final-boss settlement, ending pause, rejection of page advancement while paused, scene reload at saved page two and result reveal only after page four.
- Visual inspection checked revised Customer anatomy/body separation, captions above the boss health bar, shop landmarks, and aspect-preserving ending panels. This does not certify the scene as a fully animated cinematic.
- Player runtime assembly SHA256: `0C54477C99C506EA2DB3D310FB58D82BF4BD7FA3978A6CB9D79BA734E4F2198B`.

## Boundaries

The ending uses illustrated panels and restrained motion, not a fully rigged cinematic. Detailed actor-by-actor door exit, literal delayed shadow acting and fully articulated sign-turning remain future authored-animation work. The secret rescue units retain their existing body animation assets.

Diagnostics accelerate/force the final defeat only to verify settlement and presentation; they are not ordinary-run balance results. No audio listening approval, human enjoyment certification, mobile-device test or native-4K source-art claim is made.

No blanket git staging or commit is performed: the working tree also contains active relic, secret-event and build-morph changes from concurrent work.
