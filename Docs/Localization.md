# English and Vietnamese

The game starts in English, including on a Vietnamese device. In **Settings > Language**, select **English** or **Tiếng Việt**. The choice applies immediately in the main menu and pause menu and is saved with the player profile. Failed saves restore the previous choice. Old profiles and unsupported language codes use English.

Translations are offline in `Assets/ArcaneBrigade/Resources/Localization/vi.json`. There is no runtime translation service or extra network request. English source copy, content assets, item IDs, save IDs and gameplay rules stay unchanged. `LocalizedGUI` applies translations to both measurement and drawing; longest matching authored fragments handle text assembled from names and counters. Unknown copy falls back to English. Rich-text tags stay intact. Speech is translated before its reveal animation; world text uses a font with Vietnamese glyphs.

The catalogue was seeded by machine translation of existing English copy, with reviewed overrides for navigation, settings, gameplay terminology and numerical formula descriptions. It covers UI, descriptions and text dialogue; this change does not add Vietnamese voice recordings or repaint lettering embedded in images. Literary dialogue and future content can receive further editorial review through `Tools/Localization/vi_overrides.json`.

To refresh the authoring catalogue, explicitly run `python -B Tools/Localization/build_vietnamese.py`. This tool sends extracted English strings to Google Translate, caches drafts under ignored `Logs/Localization`, and applies the checked-in overrides last. Review changes before committing. The checked-in JSON is sufficient to build and run the game without this authoring tool or translation access.

Validation: Unity menu **Arcane Brigade > Localization > Check English and Vietnamese**. Batch entry point `ArcaneBrigade.Editor.LocalizationChecks.CheckAndBuild` also builds `Builds/Localization/ArcaneBrigade.exe`, leaving an already running older executable alone. Reports are in `Docs/Verification/LocalizationChecks.txt` and `LocalizationBuild.txt`.

For native screenshots and the smoke run, launch that build with `-arcane-localization-capture -arcane-k7-capture -capturePath PreviewExports/Localization -screen-fullscreen 0 -screen-width 1280 -screen-height 720`. The diagnostic flag isolates all save operations from the player's profile.
