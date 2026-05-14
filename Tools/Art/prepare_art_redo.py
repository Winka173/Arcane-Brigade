"""Lay out painted artwork only: shared scale, cell registration and authorized alpha cleanup."""
import json
import sys
from pathlib import Path
import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
manifest = json.loads((ROOT / 'Docs/Art_Redo_Manifest.json').read_text(encoding='utf-8'))
if '--refs' in sys.argv:
    # The identity crop is reproducible even though Library is deliberately not versioned.
    for job in manifest['jobs']:
        if not job.get('reference_crop'):
            continue
        target = ROOT / job['refs'][0]
        target.parent.mkdir(parents=True, exist_ok=True)
        Image.open(ROOT / job['identity_reference']).crop(job['reference_crop']).save(target)
report, failures = [], []
qa_path = ROOT / 'Docs/Art_Redo_Verification.json'
previous = {j['name']: j for j in json.loads(qa_path.read_text())['completed']} if qa_path.exists() else {}

def cuts_for(projection, count):
    cuts = [0]
    for i in range(1, count):
        nominal = len(projection) * i // count
        radius = len(projection) // count // 5
        options = [x for x in range(max(2, nominal-radius), min(len(projection)-2, nominal+radius))
                   if not projection[x-1:x+2].any()]
        if not options:
            raise ValueError('no transparent gutter at cell boundary ' + str(i))
        cuts.append(min(options, key=lambda x: abs(x-nominal)))
    return cuts + [len(projection)]

for job in manifest['jobs']:
    if not job.get('source'):
        continue
    cached = previous.get(job['name'])
    if '--full' not in sys.argv and cached and cached.get('source') == job['source'] and (ROOT/job['path']).exists():
        report.append(cached)
        continue
    try:
        im = Image.open(job['source'])
        if im.mode != 'RGBA':
            raise ValueError('source is not RGBA')
        pixels = np.array(im)
        pixels[pixels[:, :, 3] < 12] = 0
        im = Image.fromarray(pixels)
        if job['kind'] == 'weapon':
            box = im.getchannel('A').getbbox()
            if not box or min(box[0], box[1], im.width-box[2], im.height-box[3]) <= 0:
                raise ValueError('clipped or empty weapon')
            sprite = im.crop(box)
            sprite.thumbnail((528, 224), Image.Resampling.LANCZOS)
            sheet = Image.new('RGBA', (700, 300))
            sheet.alpha_composite(sprite, ((700-sprite.width)//2, (300-sprite.height)//2))
            bounds = [list(sheet.getchannel('A').getbbox())]
        else:
            cells, anchors, boxes = [], [], []
            row_count = job.get('rows', 2)
            rows = cuts_for((pixels[:, :, 3] > 0).sum(axis=1), row_count)
            for row in range(row_count):
                strip = pixels[rows[row]:rows[row+1]]
                columns = cuts_for((strip[:, :, 3] > 0).sum(axis=0), 4)
                for col in range(4):
                    a = strip[:, columns[col]:columns[col+1]]
                    cell = Image.fromarray(a)
                    box = cell.getchannel('A').getbbox()
                    if not box or min(box[0], box[1], cell.width-box[2], cell.height-box[3]) <= 0:
                        raise ValueError('clipped or empty character cell ' + str(row*4+col))
                    x0, y0, x1, y1 = box
                    # Bottom sole band registers the same feet baseline while preserving pose proportions.
                    yy, xx = np.where(a[max(y0, y1-max(3, (y1-y0)//8)):y1, :, 3] > 128)
                    anchor = (float(np.median(xx)), y1)
                    cells.append(cell); anchors.append(anchor)
                    boxes.append((x0-anchor[0], y0-anchor[1], x1-anchor[0], 0))
            spread = max(max(abs(b[0]), abs(b[2])) for b in boxes)
            height = max(-b[1] for b in boxes)
            scale = min(94/spread, 190/height)
            sheet = Image.new('RGBA', (1024, 256*row_count)); bounds = []
            for i, (cell, point) in enumerate(zip(cells, anchors)):
                registered = cell.transform((256, 256), Image.Transform.AFFINE,
                    (1/scale, 0, point[0]-128/scale, 0, 1/scale, point[1]-224/scale),
                    resample=Image.Resampling.BICUBIC)
                box = registered.getchannel('A').getbbox()
                if not box or min(box[0], box[1], 256-box[2], 256-box[3]) < 30:
                    raise ValueError('insufficient exported character margin')
                bounds.append(list(box))
                sheet.alpha_composite(registered, ((i%4)*256, (i//4)*256))
        corners = [sheet.getpixel(p)[3] for p in [(0, 0), (sheet.width-1, 0),
                   (0, sheet.height-1), (sheet.width-1, sheet.height-1)]]
        if any(corners):
            raise ValueError('opaque corner')
        out = ROOT/job['path']; out.parent.mkdir(parents=True, exist_ok=True)
        sheet.save(out, optimize=True)
        report.append(dict(name=job['name'], path=job['path'], kind=job['kind'], source=job['source'],
                           mode=sheet.mode, size=list(sheet.size), bounds=bounds,
                           corner_alpha=corners, bytes=out.stat().st_size))
    except (ValueError, OSError) as e:
        failures.append(dict(name=job['name'], reason=str(e)))

qa_path.write_text(json.dumps(dict(tool='painted',
    normalization='Layout/resampling and alpha <12 debris cleanup only; no code-painted substitutes',
    completed=report, failures=failures), indent=2)+'\n', encoding='utf-8')
print(str(len(report))+' delivered, '+str(len(failures))+' need correction, '+
      str(sum(not j.get('source') for j in manifest['jobs']))+' not yet generated')
for failure in failures:
    print(failure['name']+': '+failure['reason'])

if '--review' in sys.argv:
    complete = {j['name'] for j in report}
    groups = [('Heroes', manifest['jobs'][:10]), ('Companions_A', manifest['jobs'][10:32]),
              ('Companions_B', manifest['jobs'][32:54]), ('Weapons', manifest['jobs'][54:77]),
              ('Acting', manifest['jobs'][77:])]
    for label, jobs in groups:
        jobs = [j for j in jobs if j['name'] in complete]
        board = Image.new('RGB', (1716 if label == 'Acting' else 1220, max(1, len(jobs))*160), (25, 29, 38))
        draw = ImageDraw.Draw(board)
        for row, job in enumerate(jobs):
            im = Image.open(ROOT/job['path']).convert('RGBA')
            draw.text((8, row*160+5), job['label'], fill=(237, 214, 166))
            if job['kind'] == 'weapon':
                im.thumbnail((600, 130))
                board.paste(im, (180, row*160+25), im)
            else:
                for i in range(job.get('rows', 2)*4):
                    cell = im.crop(((i%4)*256, (i//4)*256, (i%4+1)*256, (i//4+1)*256))
                    cell = cell.resize((128, 128), Image.Resampling.LANCZOS)
                    board.paste(cell, (180+i*128, row*160+25), cell)
        path = ROOT / ('PreviewExports/ArtRedo/'+label+'.jpg')
        path.parent.mkdir(parents=True, exist_ok=True)
        board.save(path, quality=92)
