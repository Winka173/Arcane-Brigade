# K15 original art and motion

ImpactFamilies_v1.png: generated original transparent raster, 1536x1024, 4x4
fixed-cell atlas. Cut, slam, pierce and orb each have four sequential frames.
Prompt: crisp ink silhouettes, ivory/gold/teal/lavender, isolated effects with
clear margins, no opaque backdrop, no copied game sprites or target rings.
Made on 2026-10-15 and inspected locally.
Original source retained at ArtSource.
Project copy: Assets/ArcaneBrigade/Art/K15/ImpactFamilies_v1.png.

Stable sprite IDs survive importer reruns. All frames keep equal cell size and
center pivots instead of independent tight crops that would cause size/pivot jitter.
BC7 desktop; ASTC6x6 mobile import configuration, physical-device validation open.
Raster is not native4K; Windows runtime captures render at 3840x2160.

Shared UnitFlash paint lighting supports per-renderer UV bounds. BodyPuppet keeps
the full-frame lighting coordinates so its upper/lower cut does not create a new
shading seam. Hand/weapon positioning and the no-arms visual language are preserved.
The existing full roster is not replaced with lookalikes.

Animation additions are runtime art direction: local hit accents, material gait,
boss motif/phase acting, celebration hops, material-specific death deformation,
shadow response and curved loot flow. Existing four-pose faces retain their authored
blinks. Separately authored expression clips for every unit are future art work;
this delivery does not claim a new mouth/eye layer for all 42 companions.
