#!/bin/bash
cd "$(dirname "$0")"; HOUSES=${HOUSES:+-houses $HOUSES}
CLONE=c1 bash unity_clone.sh C:/ABClones/buildink.log ArcaneBrigade.Editor.FullRunBuilder.BuildWindows -quit > C:/ABClones/buildink.out 2>&1
grep -E "error CS" C:/ABClones/buildink.log | head -5
rm -rf /c/ABClones/capvid; mkdir -p /c/ABClones/capvid
cd /c/ABClones/c1/Builds/FullRun && timeout 2000 ./ArcaneBrigade.exe -arcane-rulesink-capture $HOUSES -capturePath "C:/ABClones/capvid" -videoPath "C:/ABClones/capvid/RulesEffects.mp4" -ffmpegPath "C:/Users/Winka/.stacher/ffmpeg.exe" -screen-width 1920 -screen-height 1080 -logFile C:/ABClones/capvid/log.txt > /dev/null 2>&1
echo "rc=$?"; grep -v "^ERROR" /c/ABClones/capvid/RulesInk.txt | tail -3; grep -c "^ERROR" /c/ABClones/capvid/RulesInk.txt; ls -la /c/ABClones/capvid/*.mp4
