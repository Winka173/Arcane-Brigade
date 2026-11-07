# English / Vietnamese verification — 2026-11-07

- Unity 6000.4.8f1: 28 localization checks passed. The report covers English defaults, older saves, persistence, failed-save rollback, switching back, rich text and glossary tokens, Unicode font coverage and unchanged gameplay assets.
- 7,754 offline entries. Navigation, settings, introductory help, key gameplay terms and formula descriptions have authored overrides. The remaining catalogue was seeded from machine translation; literary dialogue is not claimed to have received a full editorial pass.
- Windows build: `Builds/Localization/ArcaneBrigade.exe`. See `LocalizationBuild.txt` for the final build result.
- Native player smoke run: isolated diagnostic profile, English Settings, Vietnamese Settings, landing menu, hero selection, help book, switching back to English, persisting Vietnamese and entering combat. See `PreviewExports/Localization/Result.txt`.
- Inspected 1280×720 native captures: Vietnamese language choices and diacritics render correctly; hero names, descriptions and help text display translated copy. The longer dice hero name was shortened to fit its portrait card.
- Voice recordings and words embedded in existing pictures/video remain in their original language. Unknown future copy falls back to English.
- Existing asset-import changes, prior preview exports and unrelated local files were left out of the task commit. Unity caches, logs and player binaries remain ignored.
