# Secret rooms ? delivery gate

- Unity 6000.4.8f1: 404 focused secret-event assertions passed, including all 24 choices, real encounter wins, defeat, save round-trips, expiry, item suspension, duplicate-claim guards, solo alternatives and Keeper coexistence.
- Existing Outside Keeper / merge / cooperation verification passed; K24 focused count: 48.
- Audio import/routing: 2,078 library assertions + 47 Toybox routes passed.
- Windows player build: succeeded, zero errors and zero warnings. Local executable: `Builds/Secrets/ArcaneBrigade.exe`.
- Normal presentation: 84 native assertions passed; 20 nonblank actual 1920?1080 screenshots in `Accepted/`.
- Reduced presentation: 84 native assertions passed; 20 nonblank actual 1920?1080 screenshots in `Reduced/`.
- PNG transparency and generated source dimensions are recorded in `Art_Audit.json`; screenshot pixel/hash checks are in `Capture_Audit.json`.

Visual review checked the secret-room card layout, original portal/relic art, portal placement inside the viewport, and the reflection encounter. Doors were moved away from the screen edge and the combat room title moved below the action. Source art is genuinely transparent. Failed earlier black captures remain excluded from the delivered evidence.

Reproduce with `SecretEventBuilder.Build`, `SecretEventBuilder.Gate` and `Tools/Secrets/Capture.ps1` (add `-Reduced` for the second mode). Full feature rules and source entry points: `Docs/Secret_Events.md`. Built-in painting prompts and saved asset paths: `Docs/Secret_Event_Art_Prompts.json`.

These gates validate implemented rules, persistence, import, rendering and controlled combat flow. They do not certify human enjoyment, full-campaign balance, audio listening acceptance or mobile-device performance. Rescue variants retain existing authored companion art/animations.

The escalating nightmare and Last Customer ending are a proposal in `Docs/Nightmare_Progression_Concept.md`, reserved for the other agent; this delivery implements the eight secret rooms.
