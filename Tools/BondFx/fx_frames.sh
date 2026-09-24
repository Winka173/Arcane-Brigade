#!/bin/bash
# Renders a bond moment frame by frame (see Editor/FxPreview.cs) and tiles a contact sheet.
# usage: bash fx_frames.sh <rule> <Strike|Line|Zone|Weather> [radius] [delay] [length]
rule=$1; kind=$2; radius=${3:-2}; delay=${4:-.4}; length=${5:-2.6}
D=C:/ABClones/fxprev/$rule
CLONE="${CLONE:-c1 c2}" bash "$(dirname "$0")/fx_preview.sh" C:/ABClones/fxprev_$rule.log ArcaneBrigade.Editor.FxPreview.Run -fxRule $rule -fxKind $kind -fxRadius $radius -fxDelay $delay -fxLength $length -fxOut $D
grep -E "error CS|FxPreview" C:/ABClones/fxprev_$rule.log | head -8
/c/Users/Winka/.stacher/ffmpeg.exe -loglevel error -y -framerate 12 -i $D/f%03d.png -vf "select='not(mod(n,2))',scale=480:-1,tile=4x4" -frames:v 1 $D/sheet.png && echo "sheet: $D/sheet.png"
