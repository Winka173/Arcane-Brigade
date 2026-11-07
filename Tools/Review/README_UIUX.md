# Native UI/UX audit without OS input

These scripts review the existing Unity IMGUI game. They do not need a browser, move the OS pointer, generate key events or take input focus. The only Win32 calls in the runner expose its own render window with `SW_SHOWNOACTIVATE`; all pointer/activation/navigation states are in-process fixture state. `prepare` rejects templates containing OS input/focus APIs.

1. Run `python -B Tools/Review/uiux_audit.py prepare`. This saves exact source bytes under ignored `Logs/UIUXAudit`, installs a temporary runner, and adds diagnostic logging hooks. Do not edit hooked product files until restoring.
2. Build with Unity 6000.4.8f1 in batch mode, project path set to this checkout, `-executeMethod ArcaneBrigade.Editor.UIUXAuditBuild.Run`. The isolated player is written to `Builds/UIUXAudit/ArcaneBrigade.exe`.
3. Run `python -B Tools/Review/uiux_audit.py restore` after the build, even if it failed. Restoration checks for intervening edits before restoring every saved file byte-for-byte and removing temporary scripts/metas. Never commit the imported temporary scripts or source-backup JSON.
4. Before a fresh player run, remove only the previous generated `Logs/UIUXAudit/command.json` (a prior `quit` command would stop the new player). For a fresh full capture, also remove the generated `PreviewExports/UIUXAuditSynthetic/steps.json`; keep it only when resuming the same player session.
5. Start the player hidden with `-uiux-audit -audit-bridge "ABSOLUTE_PATH/Logs/UIUXAudit" -capturePath "ABSOLUTE_PATH/PreviewExports/UIUXAuditSynthetic" -screen-fullscreen 0 -screen-width 1280 -screen-height 720`. The runner makes its own window render without activating it. Wait for the menu to finish loading.
6. Run `python -B Tools/Review/run_uiux_audit.py`. The script visits menu, settings, information, hub and arena fixtures and renders each registered control's hover state. It can resume an interrupted script in the same player session using the saved step sequence.
7. `finish_uiux_audit.py` performs targeted in-process activation, focus, tooltip, glossary and stocked/poor shop checks, then quits its own player. Run its menu checks in a fresh player, or use `--shop-only` when the menu checks have already run and the player is still open. No real save is used.
8. Run `python -B Tools/Review/summarize_uiux_audit.py` after captures finish. It writes `Docs/Verification/UIUXAuditMetrics.json`. Pillow is used only to compare screenshot regions, never to generate or alter game artwork.

Use `BRIGADE_AUDIT_BRIDGE` to direct `uiux_audit.send()` to a second isolated player, such as a phone-layout rendering fixture. The game requires `-uiux-audit` to accept commands and uses a unique diagnostic save directory. Never use this runner on the owner's ordinary save.

The virtual `activate` command exercises GUI callbacks without generating mouse/key events. It does not certify OS input, actual touch, controller hardware or drag behaviour. The hover report checks whether the intended target really had `over=true`; unmatched samples are not passes. A pixel difference can come from animation, and a lack of difference can be correct for selected/disabled controls. Review source and screenshots before calling a result a defect.

See `Docs/UIUX_Audit_2026-11-07.md` for findings, scope and limitations. Generated preview PNGs, logs, caches and player builds remain ignored.
