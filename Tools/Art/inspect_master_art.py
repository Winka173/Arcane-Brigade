"""Read-only asset/provenance audit and review contact sheets; never rewrites source art."""
import hashlib
import json
import sys
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'PreviewExports/ArtMaster'
OUT.mkdir(parents=True, exist_ok=True)
jobs = json.loads((ROOT / 'Docs/Art_Master_Worklist.json').read_text(encoding='utf-8-sig'))
manifest = json.loads((ROOT / 'Docs/Act_One_Story_Image_Prompts.json').read_text(encoding='utf-8-sig'))
records = {a['path']: a for a in manifest['assets']}
failures, warnings, images = [], [], []
for job in jobs:
    name, path = job['name'], ROOT / job['path']
    if not path.is_file():
        failures.append('Missing ' + job['path']); continue
    record = records.get(job['path'])
    if not record or hashlib.sha256(path.read_bytes()).hexdigest() != record['sha256']:
        failures.append('Missing or stale provenance ' + job['path'])
    with Image.open(path) as image:
        if job.get('transparent'):
            if image.mode != 'RGBA' or image.getchannel('A').histogram()[0] < image.width * image.height * .04:
                failures.append('No transparent margin ' + name)
            else:
                alpha = image.getchannel('A')
                if max(alpha.crop((0, 0, image.width, 1)).getextrema()[1], alpha.crop((0, image.height-1, image.width, image.height)).getextrema()[1], alpha.crop((0, 0, 1, image.height)).getextrema()[1], alpha.crop((image.width-1, 0, image.width, image.height)).getextrema()[1]) > 64:
                    warnings.append('Ink touches outside border: ' + name)
                    if name in ('Hob_Atlas', 'Tally_Atlas'):
                        failures.append('Companion atlas still clips the outside edge: ' + name)
        if name.endswith('_Atlas'):
            cols, rows = 4, 4 if name.startswith(('ChairPorter', 'DoorThat', 'TheBedside', 'LastCustomer')) else 1
            for row in range(rows):
                for col in range(cols):
                    cell = image.crop((col*image.width//cols, row*image.height//rows, (col+1)*image.width//cols, (row+1)*image.height//rows)).convert('RGBA')
                    ink = cell.getchannel('A').point(lambda a: 255 if a >= 32 else 0).getbbox()
                    if ink is None: failures.append(f'Empty atlas cell {name}:{row*cols+col}')
                    elif ink[0] < 2 or ink[1] < 2 or ink[2] >= cell.width-1 or ink[3] >= cell.height-1:
                        warnings.append(f'Atlas border needs visual review {name}:{row*cols+col}')
                        if name in ('Hob_Atlas', 'Tally_Atlas'):
                            failures.append(f'Companion pose still crosses its cell edge {name}:{row*cols+col}')
        images.append((name, path, image.size))
report = dict(expected=len(jobs), present=len(images), manifest=len(records), failures=failures, visualReview=warnings,
              dimensions={n: list(s) for n, _, s in images})
pending_path = OUT / 'Pending_Deliveries.json'
pending = json.loads(pending_path.read_text(encoding='utf-8-sig')) if pending_path.exists() else {}
known_missing = {'Missing '+p for p in pending.get('missing', [])}
report.update(complete=not failures and not pending.get('repairs'),
              availablePassed=all(f in known_missing for f in failures),
              pendingRepairs=pending.get('repairs', []))
(OUT / 'Asset_Audit.json').write_text(json.dumps(report, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
for start in range(0, len(images), 30):
    page = Image.new('RGB', (1440, 1200), '#252a30'); draw = ImageDraw.Draw(page)
    for index, (name, path, _) in enumerate(images[start:start+30]):
        with Image.open(path) as source:
            preview = source.convert('RGBA'); preview.thumbnail((216, 198))
            x, y = index % 6 * 240, index // 6 * 240
            page.paste(preview, (x+(240-preview.width)//2, y+(205-preview.height)//2), preview)
            draw.text((x+8, y+211), name, fill='white')
    page.save(OUT / f'Contact_{start//30:02}.jpg', quality=92)
print(json.dumps(dict(expected=len(jobs), present=len(images), failures=len(failures), review=len(warnings))))
print('\n'.join(failures[:8] + warnings[:8]))
if failures and '--allow-partial' not in sys.argv and not ('--available' in sys.argv and report['availablePassed']): sys.exit(1)
