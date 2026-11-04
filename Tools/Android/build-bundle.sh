#!/bin/bash
# Builds the Android player with Unity 6000.6 (the editor that has the Android module) in a mirror of this project, so the
# project itself stays free for its own editor. Usage: Tools/Android/build-bundle.sh [BuildAndroidBundle|BuildAndroid|BuildAndroidEmulator]
# The Play bundle is signed with the upload key from ~/.secrets/arcane-brigade-upload.properties (Editor/MobileBuilder.cs).
set -e
MAIN="$(cd "$(dirname "$0")/../.." && pwd)"
D="${AB_ANDROID_MIRROR:-D:/ABMobile/android}"
UNITY="${AB_UNITY_ANDROID:-C:/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Unity.exe}"
METHOD="${1:-BuildAndroidBundle}"
mkdir -p "$D" D:/ABMobile/tmp D:/ABMobile/gradle
export GRADLE_USER_HOME="D:\ABMobile\gradle" TEMP="D:\ABMobile\tmp" TMP="D:\ABMobile\tmp"
for f in Assets ProjectSettings Packages; do
  MSYS_NO_PATHCONV=1 robocopy "$(cygpath -w "$MAIN/$f")" "$(cygpath -w "$D/$f")" /MIR /R:2 /W:2 /NFL /NDL /NJH /NJS /NP /XD Library Temp Logs __pycache__ > /dev/null || [ $? -lt 8 ]
done
"$UNITY" -batchmode -nographics -quit -projectPath "$D" -buildTarget Android -executeMethod "ArcaneBrigade.Editor.MobileBuilder.$METHOD" -logFile "$D/../android_build.log"
grep -E "MobileBuilder Android|signed with|error CS" "$D/../android_build.log" | head -5
ls -la "$D/Builds/Android/"
