# Hero effect art requests (24/10/2026)

What `Docs/Hero_Effect_Suggestions.md` asks for that code alone cannot paint. The light, steam, water, electricity, rain,
sonar, seams, rims, finales and waiting tells are already drawn in code (`HeroSignatureVisual`, `HeroFinaleVisual`,
`HeroFlairVisual`, `SkillRim`). The items below are physical objects that need painted frames.

Shared rules:
- Same style as the existing `<Hero>_Awake` and `<Hero>_Awake_Held` art. Transparent PNG, one sheet per item, frames
  left to right, equal cells.
- Size each item to its on-screen size. These are small objects; 256 px per cell is enough. Mind the build-size budget.
- Our side's light uses amber, cream, brass, teal and moon tones. Red is only a material, such as wax or velvet. Glowing red
  belongs to enemies.
- No ground pools, no rings round the wielder, no full-screen overlays.

| Hero | Item | Frames | What moves |
| --- | --- | --- | --- |
| Treasure Acorn | Acorn machine gun, crank side view | 4 | The crank turns a quarter per frame; the feed hopper shakes; a shell flips out on frame 3 |
| Postmaster | Sorting gatling barrels | 3 | The barrels recoil out of step (left, middle, right); a puff of ink at each mouth |
| Copper Cauldron | Steam engine piston and valve | 4 | The piston drives in and out; the valve lid lifts on frames 2 and 3 |
| Citrus Marshal | Coupling between two carriages | 3 | Slack, taut, stretched; the coupling sways through curves |
| Mirror Keeper | One hall-of-mirrors pane | 3 | Still; dented as it catches a shot; flashing silver as it returns it |
| Petal Citadel | Wall petals, front and back layers | 2 layers × 3 | Closed, a slit opening to shoot, fully open |
| Juggling Jester | Club pin and ball | 2 + 2 | The pin spinning end over end; the ball squashed on landing |
| Kite Admiral | Fleet kite | 3 | Level, tilted left, tilted right (follows its friend's run) |
| Bell Diver | Brass diving bell | 3 | Still; ramming with its front pressed in; venting bubbles from the rim |
| Bonsai Clocksmith | Gear branch of the bonsai | 2 | A gear in the branch turning half a tooth |
| Supper Mortar | Six-barrel kitchen pot lid | 2 | The lid seated; the lid rattling up with steam under it |
| Hourglass Stag | Hourglass over the head | 3 | Upright, tipping, inverted (sand stays drawn in code) |

When the art arrives, put the sheets in `Assets/ArcaneBrigade/Art/Playtest/HeroEffects/` and name them
`<HeroId>_<Item>.png`. Code will slice them and swap them in for the tokens that stand in for these objects today.
