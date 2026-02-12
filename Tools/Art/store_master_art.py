"""Copy painting pass deliveries into the project and record provenance."""
import hashlib
import json
import re
import shutil
import sys
import msvcrt
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
lock_path = ROOT/'.utmp/ArtProvenance.lock'
lock_path.parent.mkdir(parents=True, exist_ok=True)
manifest_lock = lock_path.open('a+b')
if manifest_lock.tell() == 0:
    manifest_lock.write(b'0'); manifest_lock.flush()
manifest_lock.seek(0)
msvcrt.locking(manifest_lock.fileno(), msvcrt.LK_LOCK, 1)
manifest = ROOT/'Docs/Act_One_Story_Image_Prompts.json'
record = json.loads(manifest.read_text(encoding='utf-8-sig')) if manifest.exists() else dict(
    date='2026-10-17', mode='painting pass; existing game illustration style references; no Blender', assets=[])
incoming = json.loads(Path(sys.argv[1]).read_text(encoding='utf-8-sig'))
for entry in incoming:
    source = entry.get('source')
    if not source:
        match = re.search(r'as (C:\\[^\r\n]+?\.png) by default', entry['output_hint'])
        if not match: raise ValueError('No generated image path for '+entry['name'])
        source = match[1]
    destination = (ROOT/entry['path']).resolve()
    if not destination.is_relative_to(ROOT/'Assets'): raise ValueError('Asset outside project Assets')
    destination.parent.mkdir(parents=True, exist_ok=True)
    if Path(source).resolve() != destination: shutil.copyfile(source, destination)
    with Image.open(destination) as image:
        size = list(image.size)
        alpha = image.getchannel('A').histogram()[0] if image.mode == 'RGBA' else 0
        if entry['transparent'] and alpha < image.width*image.height*.04:
            raise ValueError('Transparent asset has no usable transparent margin: '+entry['name'])
        item = {k: entry[k] for k in ['name','path','prompt','transparent']}
        item.update(source=source, sha256=hashlib.sha256(destination.read_bytes()).hexdigest(),
                    size=size, transparentPixels=alpha)
    record['assets'] = [a for a in record['assets'] if a['path'] != entry['path']]+[item]
    print('Saved',entry['name'],size,'alpha',alpha)
manifest.write_text(json.dumps(record,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print('Manifest assets:',len(record['assets']))
manifest_lock.seek(0)
msvcrt.locking(manifest_lock.fileno(), msvcrt.LK_UNLCK, 1)
manifest_lock.close()
