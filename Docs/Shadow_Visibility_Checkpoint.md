# Ground shadows

Strengthened existing faint shadows rather than adding duplicate actor systems.
GroundShadow now draws a dense contact ellipse, wider soft penumbra and a
directional sprite silhouette, consistently above ground/carpet and below actors.
Footprints use world dimensions, counter parent rotation/scale and soften with
elevation. Pooling and terminal fades affect all three layers.
World-space shadow stages avoid the shear caused by rotated, nonuniformly scaled
actor parents. Stages follow the owner's scene and are hidden/destroyed with the
owner; sandbox previews explicitly include their renderers on the preview layer.

Coverage: champion, companions, enemies/bosses, held weapons, coins/eggs/hearts/
chests, projectiles, shop stock/keepers/fixtures and arena toys use the shared
renderer. Added previously missing shadows to purchased item flights, retiring
stock and death remnants. Unit arrival footprints track the visual landing path
instead of jumping immediately to the simulation destination.

This change does not modify audio work already present in the working tree.
Native screenshots and Windows checks are stored in PreviewExports/Shadows.

Validation: 24 shadow assertions, 2,078 K13 regression assertions, 24 native
shop/merge/weapon checks and 39 native horde/boss/transition checks passed.
Windows build succeeded with zero errors/warnings. Combat and shop screenshots
were visually inspected at native 4K output; this is not mobile certification.
The executable also contains the user's current uncommitted audio assets,
which are neither changed nor staged by this shadow patch.
