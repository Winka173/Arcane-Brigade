## 7. Attack versus impact

Fire is a single swing/shot/cast cue per attack or volley. Hit uses victim material, critical and secondary-cause mapping; never reuse the fire cue for impact. Split fragment hits use x0.55, bounce/ricochet x0.8. New projectile events represent actual transformations, not per-frame motion. Boomerang return fires only at false -> true Returning.

## 8. Corrected shop semantics

Gold purchases use shop.buy.gold, Eggs/Health use shop.buy.dream after successful persistence. Failed purchases select gold/egg/hp denial; actual save errors use ui.error. Reroll maps legacy 21 to shop.reroll. Hall exit uses shop.descend, dream-to-gold handoff does not play a purchase sound. Altar fusion is emitted by its committed reveal rather than double-playing the merge at transaction time.

## 9. Validation and remaining scope

See `Docs/Audio/Handoff_Integration_Checkpoint.md` and `PreviewExports/AudioHandoff` for actual Unity results. Source-verified W entries are not claims of human auditory approval for every cue or transition.

Reserved clips remain deliberate: redundant jump/land, unused swing variants, enemy.hurt, sig.fish, shatter without a separate mechanic and theme/transition ambience without a theme-routing API. No forced playback is added just to consume every asset.

## 10. Legacy fallback

All legacy numbered cues 0-38 are mapped to library IDs, including reroll 21. Legacy Kenney/procedural clips, four licensed K8 music tracks and procedural merchant syllable remain fallback for missing/corrupt manifests/clips or the library test disable switch. Successful normal imports use the new library for routed events. New explicit ID-only hooks have no meaningful legacy equivalent.

## 11. Asset provenance and future additions

Offline handoff adds ten cues and retimes seven existing cues; original procedural composition, no third-party samples. WAV and OGG use Git LFS. Keep stable Unity metadata. Add future cues in build_sfx.py/sfx_gen.py, generate only changed cue IDs, verify audio and regenerate this contract. Boss projectile patterns currently share boss.attack; dedicated pattern clips, theme music and intro/outro stems are future content, not missing hooks in this delivery.
