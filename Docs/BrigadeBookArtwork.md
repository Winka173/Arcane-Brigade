# Brigade book button — 2026-11-07

Replaced the flat rectangle-and-bars book glyph in the touch HUD with a painted open journal: walnut leather, curled ivory pages, antique brass fittings, a burgundy bookmark and a small map motif. The existing medallion, hit area, action and placement stay the same. The original code glyph remains only as a missing-resource fallback.

Asset: `Assets/ArcaneBrigade/Resources/UI/Kit/HUD/BrigadeBook.png`. Source: 1254×1254 RGBA with genuine alpha, generated with the built-in image generation tool from the owner's screenshot as a style reference. Imported as a single sprite, mipmaps disabled, clamped, maximum 256 pixels for desktop and phone. No text is embedded in the artwork.

Art brief: a hand-painted storybook fantasy journal, near-frontal, warm light, leather cover, curved parchment, restrained map marks; no frame, crown, background, harsh black bars or legible lettering. The frame already belongs to the UI kit.

Native visual check: `PreviewExports/BrigadeBook/01-book-hud.png`, rendered with the phone HUD. Opening the existing brigade details action is shown in `02-book-opens-brigade.png`. Review instrumentation uses an isolated save and virtual GUI state; it does not control the OS mouse or keyboard. The broader UI/UX findings are reported separately and are not fixed by this art change.
