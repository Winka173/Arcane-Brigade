# Audio integration on K13

The user approved the listening preview on 2026-10-14 and requested merging
`feat/nifty-cray` into the current main branch, retaining K13 gameplay
and artwork. Music rotation and authored gain settings remain unchanged.

The remote branch contains generators and manifests, but not the other agent's
unpublished LFS audio commit. Regenerate its 447 cue clips and 60 music stems
locally using `Tools/Audio/README.md`; commit the generated clips through LFS.

Integration additionally routes the roster screen to `select` music, respects
the library disable switch in cue playback, and excludes Python bytecode caches.
The legacy playback regression exposed a nonzero music tail when the music
volume is set to zero. The adaptive bus now honors mute exactly while preserving
smooth transitions for nonzero volume settings.

Run Unity batch mode with `ArcaneBrigade.Editor.AudioIntegrationChecks.Build`
to validate the audio library, legacy sources, K13 regressions and Windows build.
Reports are copied to `PreviewExports/AudioIntegration`. Python waveform checks
are technical validation, not a new listening review.

Final Windows validation: 447 cue clips and 60 stems across 15 pieces;
waveform checks passed without warnings or failures; 1,585 library assertions,
114 legacy source assertions and 2,078 K13 regression assertions passed.
The legacy playback suite passed 27 assertions after the mute fix.
The final build succeeded with zero errors and zero warnings (485,368,888 bytes).
Binary fingerprints are in `PreviewExports/AudioIntegration/Artifact_SHA256.txt`.

This merge does not claim all optional cue hooks listed in
`Audio_Cue_Contract.md` are implemented. The existing combat/UI aliases and
merchant entrance integration are retained; further hook coverage is separate
work. Android/iOS device validation and release remain open.
