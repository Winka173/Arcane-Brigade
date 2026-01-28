# Deliberate dream levitation — 16 October 2026

## Direction

The new request supersedes the earlier attempt to make all furniture look grounded. Environmental furniture now visibly floats above a separate ground shadow. Each object has a different phase, bounded sideways drift and a slow irregular lean rather than synchronized up/down motion. Thin curling dream threads connect the empty air beneath objects without targeting circles or flashing beams. Greater nightmare depth increases the suspension height.

## Coverage

- All eight nightmare landmarks: chairs, cabinets, lamp, curtains/window pieces and story keepsakes. Their articulated puppet pieces inherit the floating root.
- Both shopkeepers, merchandise pedestals and reroll fixtures. Merchandise stays supported by the moving pedestal; rarity lighting continues following the actual merchandise.
- Rolled shop carpet: the entire cloth/cylinder presentation floats and leans together while preserving the left-to-right roll choreography.
- Continue doorway: suspended visual, unchanged physical interaction position. Merchant/stock interaction anchors do not drift.
- Secret-room invitations and artifact displays, Keeper invitation and prismatic rewards.
- Chests, eggs and hearts while waiting to be collected; attraction continues using the existing simulation. Coins retain their existing bounce/spin/attraction rather than adding three renderers per coin.
- Grand-stage perimeter architecture and market pennants. Ending props float, but the final man remains grounded.

Heroes, companions, enemies, projectiles, collision geometry, floor textures, combat attack ranges and event damage footprints are not turned into floating furniture. This is a presentation change, not a movement or balance change.

## Accessibility and validation

PresentationClock drives the effect, so pause freezes its phase. Reduced Motion preserves a constant visible air gap but removes drift/tilt. Reduced Effects keeps only one dream thread per object. No new flashes or target rings are introduced. Pooled loot hides the suspension on release/attraction; destroyed props release their materials and existing external shadow objects.

`DreamLevitationChecks.Run` exercises bounded movement, phase variation and stable reduced-motion poses. Shop native diagnostics additionally check an actual merchant air gap, drift versus reduced motion, support alignment, rarity attachment, reachable purchasing and continue, and no stock aura leaking into combat. Latest verification status and screenshots belong in `PreviewExports/Levitation/`; do not substitute the historical GroundingReview/Animation-01 captures for this pass.
