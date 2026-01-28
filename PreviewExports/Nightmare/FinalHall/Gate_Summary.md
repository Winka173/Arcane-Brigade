# Wave 20 final hall / farewell Windows delivery

Historical increment: the current FullRun player now includes the subsequent animation polish. See `../../AnimationPolish/Gate_Summary.md` for current artifact hashes. The evidence below remains specific to the earlier final-hall build.

## Verified artifact

- Unity 6000.4.8f1 Windows x64: `Builds/FullRun/ArcaneBrigade.exe`. Share the entire FullRun folder, not just the executable.
- Final Unity gate: `Library/Nightmare-GrandStage-FinalGate.log`, successful exit 0. Player build succeeded, **0 errors / 0 warnings**, reported size 866,922,613 bytes.
- One existing Editor deprecation warning remains in the concurrently edited `BuildMorphChecks.cs` (`GetInstanceID`); the zero-warning statement refers to the player build report, not every Editor log line.
- Runtime assembly SHA-256: `1EEBF90DE38184FB37297CE57B80E821FE07A585E3521BB2FD792C9E1BB02E59`.
- `resources.assets` SHA-256: `1EC9659517755CD5FDB30D66B0263B78C30D80BDD5182EED07F2DC325E07979B`.
- `Capture_Audit.json` hashes individual frames and the player files, including asset payloads. Assembly equality alone does not prove an art revision: the final body change modifies imported assets, not runtime code.

## Checks

Distinct Editor assertions: **3,910 PASS**, plus the cooperation regression. Breakdown: nightmare 223; build forms 470; secret events 459; relics 156; Outside Stage 48; visuals 34; farewell 395; Arcane audio 2,078; Toybox routing 47. Import invokes the nightmare suite twice; that duplicate is not double-counted.

- Standard native player: **149 PASS / 0 FAIL**, **34** actual 1920x1080 frames.
- Reduced Motion/Effects/Flashes native player: **149 PASS / 0 FAIL**, **34** actual 1920x1080 frames.
- Both complete the marker `PASS Replayed diagnostic fork cannot double-credit a win` after both ending branches, including save/reload of the extra afterword. Neither final Player.log reports an exception, failed assertion, C# or shader error.

Native coverage includes depth transitions, four actual architecture sprites, low fog density, pause clocks, three existing carrier families, wave-19 hush, wave-20 entrance/stamp/contract-break presentation, a physical farewell choice, four-page close-shop ending, actual saved-party departure, and five-page lamp ending with profile-memory checks. This is controlled diagnostic coverage, not an unaided complete campaign playthrough or controller-device certification.

## Visual result and boundaries

The final hall now has an original layered toyshop proscenium, asymmetric cupboard ribs, velvet, lantern sill, parallax/sway/reveal and bounded oblique light shafts. Earlier late-wave shops hint at the same architecture at reduced opacity. Dawn dissolves it. The floor remains the existing three painted families with depth-dependent shaders, not a newly claimed set of hand-painted maps.

The Customer uses two new four-pose body atlases, maintaining the established eight-pose ordering and motions, with larger source cells and details. The first attack generation was rejected for clipped extremities and corrected before import. Original v3 sources remain available. Four-pose atlases are not a claim of native-4K sprites or fully rigged cloth/finger animation.

Reviewed actual final frames include Customer stamp and cracked-mask recoil, the late shop, physical farewell room and illustrated promise afterword; the earlier visual gate also reviewed carrier/departure framing. Architecture renders beneath combat actors; fog does not introduce targets, damage or collisions. Reduced presentation is tested separately.

Both endings reach genuine dawn. The optional promise is a narrative continuation hook, not an implemented Chapter 2. Secret rooms and the Outside Keeper remain optional. No extra permanent combat progression, new audio library or audio-listening approval is claimed. Mobile-device performance and human campaign/art acceptance remain open gates.

Earlier `Farewell/` and `GrandStage/` captures are intermediate evidence, not this final v4 body-art delivery. Initial development compiler/shader failures were corrected before the accepted gates; only the final log named above is the release check for this increment.
