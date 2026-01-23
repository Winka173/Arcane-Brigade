# K16 original character art

Five redesigned originals: Lantern Knight, Coil Kettle, Thimble Moth, Reel Owl,
Wax Admiral. Each has twelve genuinely different drawings: two idle/blink,
four walking, two anticipation, two release/follow-through, two recovery.
Bold dark ink, simpler cel shading, fewer tiny texture details, shared teal/gold/
purple/ivory palette. No arms, held weapons remain separate runtime objects.

Source: built-in image generation, 2026-10-15, existing project-owned artwork as
reference; no third-party game sprites. All five selected sheets are 1448x1086.
ThimbleMoth_v2 and WaxAdmiral_v2 replace crowded first drafts. Source resolution
is preserved; these are not native4K sheets. Unity screenshots render at 3840x2160.
First drafts are retained for provenance, but only the selected version is assigned
to AuthoredMotionSet. Original generated outputs remain in the new art folder.

The importer finds transparent gutters near nominal cells, trims alpha bounding
boxes with two-pixel padding and refuses opaque cuts. Stable per-name sprite IDs;
bottom-center pivots with a shared idle reference scale keep the feet grounded and
avoid normalizing a crouched pose back to standing height. BC7 desktop, ASTC6x6
Android/iPhone import settings; real-device QA remains open.

Authored full-body clips bypass the older upper/lower sliced puppet to preserve
drawn face/cloak/coil/flame motion; weapon, hit accents, shadows and lights remain.
Boss anticipation/release/recovery follows real volley timing. Reduced Motion
uses the steady idle portrait. Ordinary legacy characters retain existing puppets.

Scope is five priority characters, not a claim that all 42 companions or every boss
has been redrawn. Native gallery is an isolated posed fixture, not natural combat;
normal weapon/horde captures separately exercise runtime animation during attacks.
