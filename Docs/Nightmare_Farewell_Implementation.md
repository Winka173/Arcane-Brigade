# Nightmare farewell and final-hall presentation

## Scope

Wave 20 is the final campaign encounter. The Customer attempts ownership of the dream; defeating him breaks that contract, not the existence of everyone inside. The surviving man is not the Outside Keeper. Two physical choices both lead to a genuine dawn. The lamp branch adds an illustrated promise afterword only; no Chapter 2 gameplay is claimed.

Saved `NightmareProgress.NarrativeRevision` separates new branch selection from legacy dawn saves. Branch and page restore together, choice commits once, save failure rolls back, invalid early promises are rejected, and endless remains unavailable until the selected epilogue is complete. Profile flags remember the ending without granting stats. Replaying a diagnostic fork cannot credit a second win.

The 395-assertion farewell suite covers both choices for all ten champions, invalid/legacy state, page restore, RNG/currency invariance and separate endless continuation. Native diagnostics additionally exercise controller/save/scene boundaries, paused choice/page input, both profile flags, actual party departure and the extra afterword.

## Environmental direction

- Low mist and moving peripheral ink shadows are world-space shader effects below gameplay sprites, not a full-screen black/blur filter.
- Four bounded local lights preserve a small area of warmth. The final Customer receives a separate amber-red source and two soft architectural light shafts.
- Four original architecture sprites gradually emerge through late waves. Wave 20 uses the strongest composition, with a slower header/curtain entrance, parallax and restrained sway. No new collision or attack fields are added.
- A missing chair leg with the old shadow, an eye looking away, slight unseen furniture movement and the reserved hero silhouette suggest the shop remembers. None changes offer prices or disguises actual hostiles.
- A four-second wave-19 shop hush softens the existing music bus, voices and decorative motes. Shopping, movement and timers remain usable. The hush starts on entry to Shop, not when combat wave 19 began.
- Resolved mirror/tailor/theatre rooms and rescued toys retain their comforting anchors. Dawn removes the oppressive architecture and restores warm light.
- Reduced Motion freezes drift and sway. Reduced Effects lowers fog, removes shadow banks and weakens shafts. High Readability reduces fog further. Pause freezes presentation.

## Original raster provenance

All assets below were made, copied into `Assets/ArcaneBrigade/Art/Nightmare/`, and imported directly in Unity. No third-party game art, external raster processing, upscaled-native-4K claim or new audio composition is involved.

Generation directory: `ArtSource/`.

| Project asset | Accepted generation | Prompt requirements |
| --- | --- | --- |
| `OneLightLeftOn_v1.png` | `exec-4d6a03ab-c030-44f7-819f-4d5d8da83eb7.png` | Original hand-painted haunted-toyshop afterword: dark wooden counter, one brass lamp, blank receipt and key, slit of genuine morning, unexplained shadow beneath cupboard. Navy/burgundy/brass; no monster reveal, text or human figure. Opaque illustration, not another crop of the four-page atlas. |
| `ManAfterTheReceipt_v1.png` | `exec-9e4db2d5-83e8-494b-a317-48f28c2f8277.png` | Four isolated transparent idle poses of the exhausted adult man after the contract breaks: pale human face, plain charcoal/burgundy coat, blank receipt; no boss mask, hat, weapons or drawer torso. Subtle breathing rather than combat animation. |
| `NightmareGrandStage_v1.png` | `exec-46256b6f-fa9c-4475-81be-deb87bc23ccb.png` | Four isolated transparent modular perimeter pieces: keyhole-eye proscenium, asymmetric tall ribbed cupboard pillars and lower sill with lanterns/receipts. Crisp painted ink, navy wood, oxblood velvet, aged brass and ivory; complete silhouettes, no words, targeting circles or characters; open gameplay centre. |
| `LastCustomerIdle_v4.png` | `exec-083d8c81-7e2c-4c29-99fc-a4b24717cca3.png` | Edit the project's `LastCustomerBody_v3.png` into a square four-pose 2x2 transparent sheet: two idle/breathing and two advancing poses. Preserve the faceless ivory mask, brass ear circles, charcoal/burgundy coat, four chest drawers, jabot, gloves, stamp and receipt. Crisp detail and broad folds, no background/glow/text. Use larger source cells instead of eight small figures. |
| `LastCustomerActions_v4.png` | `exec-d46944a5-35f2-4f49-883b-959035f972d8.png` | Match the new idle sheet in four transparent full-body poses: raised-stamp windup, stamp slam, stretched burgundy ribbon, cracked-mask recoil. Original attempt `exec-4d9c8052-7592-43bb-bb06-321534b9df4f.png` was rejected for clipped stamp/coat. Accepted edit retains poses but scales each down with generous cell padding and restores complete tips. No blood or corpse; the contract breaks, the man survives. |

The architecture generator returned an irregular arrangement rather than a strict equal grid. Explicit normalized import regions follow the actual drawings; do not replace them with a uniform 2x2 crop. Original alpha is retained. NPCs and architecture are layered 2D sprites, not fully rigged cinematic figures.

The two new body sheets preserve the existing eight-frame ordering and authored motions, including frame 7's cracked-mask death treatment. Unity finds each figure's main connected component, retaining protruding receipt/coat bounds across nominal cell edges without importing a neighbouring figure. This is a source-detail revision, not a new damage/balance change or a claim of 4K-native poses.

## Reproduction

Run Unity batch method `ArcaneBrigade.Editor.NightmareVisualChecks.Gate` to import art, execute integration checks and build `Builds/FullRun/ArcaneBrigade.exe`.

Run `Tools/Nightmare/Capture.ps1 -Player Builds/FullRun/ArcaneBrigade.exe -Output PreviewExports/Nightmare/FinalHall/Final`, then repeat with `-Reduced` and output `Final-Reduced`.

Audit with `Tools/Nightmare/AuditVisuals.ps1 -Root PreviewExports/Nightmare/FinalHall -Completion 'PASS Replayed diagnostic fork cannot double-credit a win'`. The completion marker is deliberately after both branches, not after the first result screen. Delivery counts/hash are recorded alongside those captures, not inferred from older evidence.

This verifies targeted Windows paths and native frames, not a full human campaign playthrough, listening approval, all-device performance or finished sequel content.
