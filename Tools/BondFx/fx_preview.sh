#!/bin/bash
# Mirrors the main project into C:/ABClones/<clone> and runs a Unity batch method there (main stays free for the editor).
log="$1"; shift; method="$1"; shift
MAIN="C:/Users/Winka/OneDrive/Documents/Arcane-Brigade"
UNITY="C:/Program Files/Unity/Hub/Editor/6000.4.8f1/Editor/Unity.exe"
pick() {
  for c in ${CLONE:-c1 c2}; do
    if mkdir "C:/ABClones/$c.lock" 2>/dev/null; then echo "$c"; return; fi
  done
}
while true; do c=$(pick); [ -n "$c" ] && break; sleep 15; done
trap 'rmdir "C:/ABClones/$c.lock" 2>/dev/null' EXIT
D="C:/ABClones/$c"
for f in Assets ProjectSettings Packages; do
  MSYS_NO_PATHCONV=1 robocopy "$(cygpath -w "$MAIN/$f")" "$(cygpath -w "$D/$f")" /MIR /R:2 /W:2 /NFL /NDL /NJH /NJS /NP /XD Library Temp Logs __pycache__ > /dev/null
done
"$UNITY" -batchmode -projectPath "$D" -executeMethod "$method" "$@" -logFile "$log"
code=$?
echo "exit $code ($c)"; exit $code
