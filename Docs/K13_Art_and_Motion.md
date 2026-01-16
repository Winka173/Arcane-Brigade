# K13 art and motion delivery

## Assets

Built-in image generation/editing is used; no API/CLI fallback or extracted reference-game art. Final atlases are committed with Unity sprite metadata and used by the eight weapon definitions:

- `Assets/ArcaneBrigade/Art/K13/DreamArms_v2.png`: giant orb staff, extinguisher, three-barrel plasma gun and four-feather fan.
- `Assets/ArcaneBrigade/Art/K13/ToyBlades_v2.png`: twin sabres, clock crescent, closed parasol needle and stamp hammer.
- `Assets/ArcaneBrigade/Art/K13/DreamProjectiles_v1.png`: purple orb, cyan plasma, foam puff and feather dart.

Each is a 2172x724 transparent RGBA source atlas, four horizontal cells; this is not native-4K source art. Runtime evidence is captured at 3840x2160 and smaller desktop surfaces. Unity imports preserve alpha, disable mipmaps, use BC7 on Windows and retain the existing project import convention.

The original ranged draft `DreamArms_v1.png` is retained only as a source study; gameplay uses the padded v2. The discarded melee draft with damaged borders is not imported into the project.

## Final prompt set

Ranged layout edit: preserve the four original painted weapon identities and their brass/teal/plum/coral surfaces, right-facing tips. Exactly four equal-width cells, ample transparent padding on each side, no crossing cell boundaries. True RGBA transparency; no text, grid, floor shadow or debris. Spacing correction only.

Melee cleanup edit: preserve double curved blades, crescent clock boomerang, closed umbrella lance and stamp hammer. Remove all borders, separators, corner debris and background while preserving opaque interiors and crisp ink silhouettes. Four isolated equal-width horizontal cells on true alpha, no text, grid or shadows.

Projectile generation: four equal-width cells in hand-drawn toy-fantasy style: a round purple glass orb with gold star, a right-facing cyan plasma teardrop with cream core, a pale turquoise three-lobed foam puff, and an ivory/violet feather dart. Opaque interiors with dark readable rims, true alpha, ample padding, no trails, text or grid.

## Layered map redraw

`CarnivalStone_v1.png` is the newly painted walkable slate/ceramic floor, with recessed seams, chipped plaster, etched faded ornament and moss. `CarnivalPatina_v1.png` is an independent true-alpha layer of worn carpet pigment, mosaic fragments, stitchwork and dusty roots. The floor shader combines repeated stone at material scale, world-map patina at a broader scale and subtle higher-frequency grain. Phase palettes still transition without loading.

Ground source dimensions are 1536x1024 for stone and 1774x887 for patina. Ground imports use mipmaps/trilinear filtering, mirrored tile sampling and clamped world patina; weapon atlases use the separate non-mipmapped sprite convention.

All thirty raised decorative landmarks/garden props and their decorative shadows are removed from the runtime map. Functional arena toys, combat units, boundary geometry and market fixtures are retained. This is a floor-art redraw, not an alteration of gameplay collisions or enemy spawning.

Map generation prompts: top-down original hand-painted midnight carnival ground, blue-teal slate and dusty plum ceramic, soft recessed relief and muted ochre etching; no perspective, props, actors, text or floor shadows. Separate overlay: sparse broad faded plum/teal carpet paint, erased ochre stitches, small mosaic/moss patches and transparent negative space, all painted flat into the ground, no raised objects or glowing target circles.

## Animation implementation

New weapon sprites use individually authored grip pivots rather than rotating around the center of the artwork. Muzzle offsets derive from the same sprite/pivot/width convention as existing weapons.

All roster units and bosses have their existing four authored state sprites checked for availability. Upper/lower-body puppeting adds independent breathing, blink/gait selection, idle gestures, attack anticipation/release, support gestures and hit recoil. Cadences and amplitudes vary by authored material and stable species identity; rigid metal/wood and elastic fabric/wax do not share one pose response. Boss motif adds a separate telegraph flourish. Existing material-dependent death remnants and painted death frames remain active.

Weapon poses use their actual attack cycle; dual sabres deliver two cuts, hammers have impact dust rather than a generic sword slash, boomerangs visibly spin and return, and painted projectiles retain their own silhouette without oversized generic halo/core circles. Ground shadows and chase dust remain independent of weapon animation.

Shop preview moves the two actual eligible companions into formation, with animated gold threads. It does not mutate saved roster, currency or shop/weapon randomness. Only committed purchase triggers the existing merge reveal. Star badges show the projected result including cascades to tier three. Shop focus/denial beats now use the paused presentation clock.

This is authored sprite states plus procedural animation, not eight individually redrawn frame-by-frame clips for every species. Automated coverage and captures do not establish human art-direction approval.
