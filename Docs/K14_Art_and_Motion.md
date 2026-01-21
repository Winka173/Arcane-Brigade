# K14 original art and motion

Runtime sources in Assets/ArcaneBrigade/Art/K14:

- ClockworkArsenal_v3: six isolated weapons, 3 by 2 atlas, original generated
  hand-drawn toy designs with true alpha. Version 1/2 are unused source studies.
- CarnivalCompanions_v1: twelve painted poses, four per character; neutral,
  blink, two directional gait poses. No arms; hands continue using the existing
  separate weapon-attached hand system. This is not a full eight-frame cycle.
- CarnivalProjectiles_v1: separate runtime yo-yo spool and needle/thread; the
  third ribbon shard is reserved artwork. Weapon handles do not fly as projectiles.
  Three-cell sprite atlas, weapon grip pivots authored separately for each prop.

Unity slices the original bitmap assets without upscaling them. Desktop uses BC7,
mobile import settings use ASTC 6x6; screenshots render at 3840 by 2160, which
does not mean the source PNGs are native 4K.

New weapon motion: spring compression/release, yo-yo throw, sewing vibration,
projector steadiness, magnet recoil and ribbon rotation. Tether endpoints follow
the actual projectile/source, and projector light is a translucent cone rather
than an aim/target circle. Existing Reduced Motion disables exaggerated poses.

Existing species gain deterministic peek/nod/tuck/wiggle gestures and material
gait variation. New allies have their own painted blink/gait poses plus upper/
lower puppeting. Boss recovery uses an authored rest pose. No claim is made that
every existing ally has been redrawn or received a bespoke frame-by-frame clip.

Original bitmap prompts requested transparent cutouts, thick dark outlines,
teal/crimson/amber/plum haunted-carnival toy shapes; no franchise art is copied.
Human style/feel approval remains separate from technical tests.
