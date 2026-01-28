# Nightmare visual audit

## Scope

Reviewed the current Unity FullRun player at waves 1, 5, 9, 13 and 17, Last Customer arrival/attack and the illustrated ending, against the approved nightmare concept and production rendering code. Fresh native evidence: `PreviewExports/Nightmare/VisualAudit`. Unity Editor verification log: `Library/Nightmare-Visual-Audit.log`. This is a targeted presentation audit, not a complete crowded-combat or mobile performance certification.

## Findings

- Landmark artwork changes with depth, but environmental illumination does not establish a sufficiently distinct warm-to-nightmare progression. The floor remains broadly evenly visible.
- The painted lamp has no localized light spill on the floor or nearby bodies. Sprite rim/shading parameters do not react to nearby sources.
- Ground contact, soft and silhouette shadows exist. Their cast direction is fixed rather than derived from a light source; the issue is integration, not absence of shadows.
- Painterly detailed scenery and the Customer contrast with much simpler recruit bodies and some merchant frames. More detail everywhere would not fix this mismatch: line weight, material language and silhouette need a shared art direction.
- Nightmare landmarks swap art and use small scale oscillations, but lack articulated eye tracking, cloth folds, independent limbs and deliberately timed unsettling gestures.
- In reviewed shop frames the lower tooltip obscures offer prices, and the continue prompt overlaps lower shop content. Atmospheric additions must not conceal transactional UI further.
- The ending establishes a useful warm contrast, but is an illustrated sequence rather than a fully animated departure using the actual party.

## Recommended order (not implemented by this audit)

The subsequent approved implementation is documented in `Nightmare_Implementation.md` and `PreviewExports/Nightmare/Lighting/Gate_Summary.md`. The list below preserves the original audit recommendations rather than implying the later work is still absent.

1. **Readable environmental lighting.** Warm lamp/shop pools, cold window spill, protected player silhouette and restrained emissive relics. Decrease background ambient with depth without dimming hostile shots, essential UI or the whole screen indiscriminately. Use soft irregular light falloff, not visible targeting circles.
2. **One coherent art language.** Keep the compact hand-drawn toy characters. Match scene props and bosses through contour thickness, simplified small-scale detail, common shadow temperature and purposeful material highlights. Prioritize the enlarged Customer body and carrier appendages.
3. **Five authored environmental states.** Warm timber/brass; closing-time blue; damp ink/peeling labels; ribbed shelving and breathing velvet; near-black architecture with surviving amber landmarks. Crossfade states smoothly without moving collision boundaries or introducing loading interruptions.
4. **Shadow storytelling.** First add light-aware casts for major props. Separately author rare delayed or independently moving decorative shadows during safe shop moments. Never delay movement feedback or make enemy collision positions ambiguous.
5. **Articulated unsettling animation.** Eyes turn before heads, drawers inhale before opening, receipts curl, chair upholstery pulses and the carpet tip briefly behaves like a tongue. Use separated sprite parts and intentional anticipation/recovery rather than continuous whole-sprite squash.
6. **Warm recovery anchors.** Rescued toys, the repaired heart and delivered letter preserve warm light and recognizable gestures. Allow brief quiet shop beats between darker segments; later corruption becomes more meaningful through contrast.
7. **Narrative transitions and finale.** Improve grip/contact, passenger occlusion, trailing paper and landing for each carrier. Reuse its visual motifs in the optional boss. Give the Customer a staged entrance, progressive physical collapse and a door-opening dawn reveal; actual saved companions should participate in departure when authored animation is available.
8. **Sparse atmosphere with visual budgets.** Backlit dust, low peripheral mist, drifting receipts and moving shelf shadows. Keep the combat plane clean, cap layered transparency and disable unnecessary motion under Reduced settings. Avoid persistent blur, strobing and foreground fog covering bullets.
9. **UI polish before more decoration.** Preserve fixed price space and tooltip padding, keep exit prompts separate, and unify shop frames with the receipt/price-tag theme. Nightmare flourishes belong on decorative edges, not changing labels, font legibility or hit areas.

## Acceptance targets

- Compare early, middle, late and dawn frames at the same camera/exposure, including crowded combat and both shop types.
- Verify warm sources visibly affect nearby ground and relevant sprite edges, with contact shadows remaining attached through jumps and purchases.
- Confirm enemy projectiles and the player's foot marker remain distinguishable on every floor palette; no new target circles.
- Check pause, Reduced Motion/Effects/Flashes and transitions; measure standalone frame time before claiming acceptable performance. Mobile remains a separate device gate.

Unity is the implementation and visual acceptance environment. Blender is optional support for original carrier rigs, cloth studies or sprite/normal-map baking, not a replacement engine or a requirement to migrate this 2D game to 3D. The current project uses the Built-in Render Pipeline; adding ordinary scene lights alone will not illuminate the existing unlit floor/sprite shaders. A bounded custom lighting pass can be evaluated before considering a broader pipeline migration.
