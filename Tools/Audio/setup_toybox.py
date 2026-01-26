"""Download pinned, offline-only production dependencies. Nothing is installed globally."""
import hashlib
import json
from pathlib import Path
import urllib.request
import zipfile

CACHE=Path(__file__).resolve().parent/'.cache'
SOURCES={
    'fluidsynth.zip': 'https://github.com/FluidSynth/fluidsynth/releases/download/v2.6.1/fluidsynth-v2.6.1-win10-x64-cpp11.zip',
    'GeneralUser-GS.sf2': 'https://raw.githubusercontent.com/mrbumpy409/GeneralUser-GS/main/GeneralUser-GS.sf2',
    'GeneralUser-LICENSE.txt': 'https://raw.githubusercontent.com/mrbumpy409/GeneralUser-GS/main/documentation/LICENSE.txt',
}

def main():
    CACHE.mkdir(parents=True,exist_ok=True)
    lock_path=Path(__file__).with_name('toybox_dependencies.json')
    lock=json.loads(lock_path.read_text()) if lock_path.exists() else {}
    for name,url in SOURCES.items():
        if name in lock: url=lock[name]['url']
        path=CACHE/name
        if not path.exists(): urllib.request.urlretrieve(url,path)
        digest=hashlib.sha256(path.read_bytes()).hexdigest()
        if name in lock and digest!=lock[name]['sha256']: raise RuntimeError('Dependency checksum mismatch: '+name)
        lock[name]={'url':url,'sha256':digest}
        print(name,digest)
    with zipfile.ZipFile(CACHE/'fluidsynth.zip') as z:
        # Archive contents are pinned by the checksum above and remain in this cache.
        for info in z.infolist():
            dest=(CACHE/'fluidsynth'/info.filename).resolve()
            if not dest.is_relative_to((CACHE/'fluidsynth').resolve()): raise RuntimeError('Unsafe archive path')
        z.extractall(CACHE/'fluidsynth')
    lock_path.write_text(json.dumps(lock,indent=2)+'\n')

if __name__=='__main__': main()
