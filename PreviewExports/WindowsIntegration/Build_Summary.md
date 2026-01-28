# Windows integration rebuild

Historical integration record. The current FullRun executable is superseded by the nightmare visual delivery described in `PreviewExports/Nightmare/Lighting/Gate_Summary.md`; its current assembly hash and captures are recorded there rather than in this earlier report.

Rebuilt the current saved working tree with Unity 6000.4.8f1, including the twelve build-form families, secret rooms, adapted relics, nightmare progression, Last Customer ending and current Toybox audio. Existing content assets were not regenerated or reset.

- Executable: `Builds/FullRun/ArcaneBrigade.exe`. Distribute the entire `Builds/FullRun` directory, not the executable alone.
- Player build: succeeded, zero errors and zero warnings; reported size 850,605,633 bytes.
- Editor compilation emitted an existing obsolete `GetInstanceID` warning in `BuildMorphChecks.cs`; the player build warning count above is separate.
- Focused checks: build forms 470, nightmare 221, secret events 459, relics 156, Outside Keeper 48, audio library 2,078, Toybox routes 47. All passed, as did the cooperation regression.
- Native checks on this FullRun executable: nightmare 31 PASS / 0 FAIL / 11 screenshots; build forms 99 PASS / 0 FAIL / 25 screenshots. Captures are under `Nightmare/` and `BuildMorphs/` alongside this report, using isolated diagnostic saves.
- Build log: `Library/Windows-Integration-Rebuild.log`.
- Runtime assembly SHA-256: `EB56B9CAAA17ABF21A2235DD58BA9E099001705A891FF1CC02AD95958EB19EE2`.

Reproduce with `ArcaneBrigade.Editor.WindowsIntegrationBuilder.Build`. Native captures validate controlled integration, not human audio approval, full-run balance or mobile performance. This rebuild does not add a new lighting system.
