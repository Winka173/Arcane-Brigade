#!/bin/bash
cd "$(dirname "$0")"; HOUSES=${HOUSES:+-houses $HOUSES}
[ -z "$SKIPBUILD" ] && CLONE=c1 bash unity_clone.sh C:/ABClones/buildink.log ArcaneBrigade.Editor.FullRunBuilder.BuildWindows -quit > C:/ABClones/buildink.out 2>&1
grep -E "error CS" C:/ABClones/buildink.log | head -5
rm -rf /c/ABClones/capvid; mkdir -p /c/ABClones/capvid
# the player runs with a hidden window at below-normal priority so it never takes the owner's focus
cd /c/ABClones/c1/Builds/FullRun && FX_WINDOW=Minimized powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(cygpath -w /c/Users/Winka/OneDrive/Documents/Arcane-Brigade/Tools/BondFx/hidden_unity.ps1)" "$(cygpath -w /c/ABClones/c1/Builds/FullRun/ArcaneBrigade.exe)" -screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -arcane-rulesink-capture $HOUSES -capturePath "C:/ABClones/capvid" -videoPath "C:/ABClones/capvid/RulesEffects.mp4" -ffmpegPath "C:/Users/Winka/.stacher/ffmpeg.exe" -screen-width 1920 -screen-height 1080 -logFile C:/ABClones/capvid/log.txt > /dev/null 2>&1
echo "rc=$?"; grep -v "^ERROR" /c/ABClones/capvid/RulesInk.txt | tail -3; grep -c "^ERROR" /c/ABClones/capvid/RulesInk.txt; ls -la /c/ABClones/capvid/*.mp4
