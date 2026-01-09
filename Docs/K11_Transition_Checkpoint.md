# K11.1 — living arena transition checkpoint

2026-10-14. This checkpoint implements the transition/body-attack portion of K11, not the complete requested overhaul.

## Implemented

- Wave continuation stays in RunArena. Removed the opaque curtain rather than disguising a load. Simulation recompiles inside the same scene; friendly world positions are preserved through purchases and wave changes, no reset-to-origin teleport.
- Regular merchant folds upward with stock and cloth; the next combat begins after departure. Cloth unfurls, merchant and offers arrive in staggered motion on return to rest. Existing voice/entrance cues remain.
- Special merchant appears first when special stock exists, with low-opacity purple moving dream wisps and palette tint. Its downward exit commits `SpecialShopVisited` before the regular merchant arrives; it does not advance the wave. There is no alternate-shop sign/toggle station. Players may leave without buying all stock. The visited stage survives save/resume, and resets when the next shop is settled.
- Merchant/stock clocks use a controller presentation clock, not the rebuilt simulation clock. Combat attack timestamps retain their original simulation time domain.
- Enemy handheld grip is disabled regardless of weapon definition. Ranged shots originate within the enemy body radius, with body charge/squash/recoil. The updated user decision supersedes the earlier request for hostile held weapons. Exploratory weapon art generated before that decision is not shipped.
- Friendly presentation height now uses 95% authored height, capped at0.88 before size modifiers (was72%, cap0.64). Weapon readability increases proportionally, retaining body-relative bounds. This is visual scaling, not a silent hitbox/damage buff.
- Added two new original giant merchant identities: the many-footed bank-vault sea slug and the many-eyed egg-manta oracle. Each has four imported poses (idle/blink/talk/celebrate), layered ground shadows and existing syllable/interaction reactions. The two older identities remain in rotation: four silhouettes total, with large-to-colossal authored heights3.7/4.2/5.25 for gold and4.4/5.2/5.65 for dream shops. Sizes/identity are deterministic by hall, not randomized on each purchase. Offers stay in front and NPC avoidance radius scales with size.
- Sources saved in `Assets/ArcaneBrigade/Art/K11/GiantGoldMerchant_v1.png` and `GiantEggMerchant_v1.png`; full prompts/provenance: `K11_Merchant_Art.json`. Built-in painting,2172×724 strips, true alpha; not native-4K source art. View/import source art and Unity captures separately for QA.
- Shop shadow anchor now derives from sprite pivot, rather than a hardcoded42%-height offset that visibly floated giant feet. Camera Follow no longer snaps during purchases/wave changes; it keeps the existing smooth follow/zoom.
- Added18 small nonblocking painted scenery objects outside the merchant footprint. Palette changes interpolate rather than jump, blending toward a dream tint for the special merchant. Reuses existing authored environment artwork; not claimed as a newly painted map or new prop set.

## Validation

`K11Checks` covers save/restore stage, movement during rest, clearing hostile state, transition source contracts, position preservation and body projectile origin. Native capture must additionally verify real purchase/exit timing, unchanged scene/controller identity, no teleport, actual resolution and natural combat-to-shop arrival. Legacy motion/presentation assertions were updated only for intentional larger allies and removal of the switch sign.

Final Windows technical gate passed:18 K11 focused checks,163 K10 checks,65 K9 checks and the existing K3–K8 regression gate. Build zero errors/warnings. Native37 assertions passed, eleven PNGs verified3840×2160, including actual egg/recruit purchases, both departures, same-scene wave2, natural wave completion and explicit hall2/3 merchant-size showcases. Live hostile grips verified hidden; four large/colossal merchant viewport bounds verified. Initial capture at the wrong desktop client size was rejected; final capture uses the shared native resize helper and checks actual pixels. Native review also caught a clipped giant crown, fixed by camera-aware staging before final capture. This does not certify hitch-free performance or human quality.

Realtime transition video:38seconds,2280 encoded frames,1920×1080/60-fps CFR/timeScale1, Unity framebuffer only. Silent; no desktop/microphone capture. It does not certify audio quality or measured FPS. Frame repetition preserves wall-clock pacing when an encoder readback is missed. All native/video checks use the same final Windows candidate. SHA256 manifest records local executable plus changed source/content/evidence, excluding the manifest itself.

Source/capture gallery: `PreviewExports/K11/index.html`. Local candidate: `Builds/FullRun/ArcaneBrigade.exe`. Failed boundary saves re-open the departed shop instead of leaving it permanently collapsed. K11 capture uses isolated diagnostic storage and never modifies the real player save.

## Still pending in K11

- English-default persisted language setting with complete translated menus/content/glossary/dialogue.
- Protagonist-themed recruit pools and distinct automatic gameplay mechanics.
- New differentiated artifacts and concise highlighted bond/build-link/merge previews.
- Melee/ranged distribution, per-weapon reach, harder fair enemy pressure and measured balance.
- Dedicated new enemy mouth/body attack artwork and fuller arena environmental variety.

This checkpoint does not claim those pending features, exact Bizarre Brigade parity, human visual/audio approval, mobile device approval or guaranteed hitch-free performance on other hardware.

## Reference boundaries

Indexed [Dream Seekers](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Dream_Seekers) and [Companions](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Companions) plus the locally viewed1.5 footage inform differentiation and automatic combat. Full Fandom extraction remains blocked/incomplete; theme pools will be original Arcane designs, not falsely claimed reference-wide rules. New protagonist pools should preserve random weapon assignment, run-only persistence and movement-only input.
