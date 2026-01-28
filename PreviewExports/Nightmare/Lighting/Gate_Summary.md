# Nightmare visual delivery

## Implemented

- Four bounded local light sources: warm lamp, cool window, protected player light and shop warmth. The player source follows the visual carrier path, not just the simulation's grounded position. Floor ambient progressively falls across five depth levels and recovers at dawn; opaque dark-screen overlays are not used.
- Floor grain, ink-like veins and peripheral rib treatments strengthen with depth. Existing painted floor families remain; this pass does not claim five newly painted maps.
- Original clean cartoon redraws: sixteen landmarks, eight Customer body poses and eight matching anatomy drawings. Source filenames: `NightmareProps_v2.png`, `LastCustomerBody_v3.png`, `LastCustomerOrgans_v2.png` in `Assets/ArcaneBrigade/Art/Nightmare/`. Irregular atlas rows are imported by connected components to avoid cut feet, borrowed neighbouring sprites and broken silhouettes.
- Local light affects sprite rim/shadow tint. Ground casts blend source directions; landmark shadows draw behind the prop rather than over its feet. Sparse independent decorative-shadow acting occurs only in safe shop moments.
- Eight hinged landmark puppets, localized corrupted-eye tracking, breathing curtain motion and remembered warm heart/bird details. Full-source UV shading avoids false bright seams at sprite cuts.
- At most eighteen small drifting receipt motes, behind combat actors; Reduced Motion/Effects disable them. Pause freezes the lighting/atmosphere clock. No target rings, projectile dimming, blur or strobing were added.
- Carrier contact/landing dust is world-anchored; passengers tilt through grip/release and reset on completion or destruction. All three carrier families retain their existing authored source frames and sound cues.
- Customer-specific staged arrival, cracked-mask collapse, retracting anatomy and liberated paper-bird fragments. The isolated diagnostic now queues lethal damage through fixed-step combat so the actual kill-event consumer presents death effects.
- Dawn procession uses the saved champion, permanent recruits, their assigned weapons and tiers, plus rescued-friend cameos. Figures are clipped to the illustrated scene and remain opaque until the doorway. Reduced Motion retains stationary figures. Existing save/page/pause/skip rules remain.
- Shop price tickets reserve the lower tooltip band; deny feedback stays above prices. Continue controls and narrative whispers no longer share the stock-price area.

## Verification

Unity 6000.4.8f1. Rebuild entry point: `ArcaneBrigade.Editor.NightmareVisualChecks.Gate`, which imports the revised art, runs the integration checks and builds `Builds/FullRun/ArcaneBrigade.exe` without regenerating the rest of the catalog.

- Focused visual checks: 30 assertions, including depth ambient bounds, light falloff, Reduced departure, delayed visibility fade, landmark composition, tooltip safe bounds and active revised artwork.
- Integration suite: build forms 470, nightmare 221, secrets 459, relics 156, Outside Keeper 48, audio library 2,078, Toybox routes 47; cooperation regression also runs.
- Native reproduction: `Tools/Nightmare/Capture.ps1 -Player Builds/FullRun/ArcaneBrigade.exe -Output PreviewExports/Nightmare/Lighting/Final`, then the same command with `-Reduced -Output PreviewExports/Nightmare/Lighting/Final-Reduced`.
- Capture auditing: `Tools/Nightmare/AuditVisuals.ps1`; the adjacent `Capture_Audit.json` records runtime assembly SHA-256, native pass/fail counts, screenshot dimensions and individual hashes.
- Native scenarios include eight shop depths/checkpoints, actual focused purchase UI, all three seamless carrier transitions, Customer entrance/attack, three death beats, pause/resume, saved ending reload and the actual party's departure. Diagnostic storage is isolated from normal player saves.

The final runtime totals and player build result are recorded in `Delivery_Result.md` after the native run completes. Earlier candidate directories are not the accepted evidence.

## Scope boundaries

This is layered, articulated 2D sprite presentation on the Built-in pipeline, not a URP migration, physical volumetric-light simulation or fully hand-rigged cinematic. Earlier boss/merchant/carrier art outside this nightmare redraw remains in use. The ending's underlying story panels are still illustrations. Source images are not native 4K. Automated integration and screenshots do not establish human art/audio approval, full-campaign balance or mobile-device performance.

Art generation requirements and provenance: `Docs/Nightmare_Visual_Art_Prompts.md`. Implementation notes: `Docs/Nightmare_Implementation.md`.
