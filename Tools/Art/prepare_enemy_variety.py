"""Register painted enemy atlases only; owner-approved layout and faint-alpha cleanup."""
import argparse
import json
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
MANIFEST = ROOT / 'Docs/Enemy_Variety_Art_Manifest.json'
QA = ROOT / 'Docs/Enemy_Variety_Art_Verification.json'


def cuts(projection):
    result = [0]
    for i in range(1, 4):
        nominal = len(projection)*i//4
        radius = len(projection)//22
        options = [x for x in range(max(2, nominal-radius), min(len(projection)-2, nominal+radius))
                   if not projection[x-1:x+2].any()]
        if not options:
            raise ValueError('no transparent gutter at boundary '+str(i))
        result.append(min(options, key=lambda x: abs(x-nominal)))
    return result+[len(projection)]


def prepare(job):
    im = Image.open(job['source'])
    if im.mode != 'RGBA':
        raise ValueError('painted source is not RGBA')
    pixels = np.array(im)
    pixels[pixels[:, :, 3] < 12] = 0
    row_edges = cuts((pixels[:, :, 3] > 0).sum(axis=1))
    cells, boxes = [], []
    for row in range(4):
        strip = pixels[row_edges[row]:row_edges[row+1]]
        columns = cuts((strip[:, :, 3] > 0).sum(axis=0))
        for col in range(4):
            cell = Image.fromarray(strip[:, columns[col]:columns[col+1]])
            box = cell.getchannel('A').getbbox()
            if not box or min(box[0], box[1], cell.width-box[2], cell.height-box[3]) <= 0:
                raise ValueError('empty or edge-clipped source cell '+str(row*4+col))
            cells.append(cell.crop(box)); boxes.append(box)
    # One shared scale preserves the larger mutated anatomy relative to the normal form.
    scale = min(284/max(c.width for c in cells), 286/max(c.height for c in cells))
    output = Image.new('RGBA', (1536, 1536))
    bounds = []
    for i, cell in enumerate(cells):
        cell = cell.resize((max(1, round(cell.width*scale)), max(1, round(cell.height*scale))), Image.Resampling.LANCZOS)
        x, y = (384-cell.width)//2, 336-cell.height
        output.alpha_composite(cell, ((i%4)*384+x, (i//4)*384+y))
        bounds.append([x, y, x+cell.width, y+cell.height])
        if min(x, y, 384-x-cell.width, 384-y-cell.height) < 38:
            raise ValueError('less than10% exported margin')
    a = np.array(output)[:, :, 3]
    if a[0].any() or a[-1].any() or a[:, 0].any() or a[:, -1].any():
        raise ValueError('opaque canvas edge')
    path = ROOT/job['path']; path.parent.mkdir(parents=True, exist_ok=True)
    output.save(path, optimize=True)
    return dict(name=job['name'], path=job['path'], source=job['source'], mode='RGBA',
                size=[1536, 1536], grid=[4, 4], frames=16, bounds=bounds,
                corner_alpha=[0, 0, 0, 0], bytes=path.stat().st_size)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('name', nargs='?')
    parser.add_argument('--review-only', action='store_true')
    args = parser.parse_args()
    manifest = json.loads(MANIFEST.read_text())
    previous = json.loads(QA.read_text()) if QA.exists() else {'completed': [], 'failures': []}
    complete = {j['name']: j for j in previous['completed']}
    failures = {j['name']: j for j in previous['failures']}
    selected = [j for j in manifest['jobs'] if j.get('source') and (not args.name or j['name'] == args.name)]
    if args.review_only:
        selected = []
    if args.name and not selected:
        raise ValueError('no generated source for '+args.name)
    failed = False
    for job in selected:
        try:
            complete[job['name']] = prepare(job)
            failures.pop(job['name'], None)
            print(job['name']+' normalized: RGBA,16 cells, transparent margins')
        except (ValueError, OSError) as error:
            failures[job['name']] = dict(name=job['name'], reason=str(error))
            print(job['name']+' FAILED '+str(error)); failed = True
    ordered = [complete[j['name']] for j in manifest['jobs'] if j['name'] in complete and j['name'] not in failures]
    QA.write_text(json.dumps(dict(tool='painted', processing='Registration/resampling and alpha<12 cleanup only; no code-painted art', completed=ordered, failures=list(failures.values())), indent=2)+'\n')
    (ROOT/'Docs/Enemy_Variety_Art_Progress.txt').write_text(''.join(j['name']+' '+j['path']+' done\n' for j in ordered))
    print(str(len(ordered))+'/22 delivered')
    if args.review_only:
        names = {j['name'] for j in ordered}
        folder = ROOT/'PreviewExports/EnemyVariety'; folder.mkdir(parents=True, exist_ok=True)
        for label, jobs in [('Repaints', manifest['jobs'][:10]), ('NewEnemies', manifest['jobs'][10:18]), ('Elites', manifest['jobs'][18:])]:
            jobs = [j for j in jobs if j['name'] in names]
            if not jobs:
                continue
            board = Image.new('RGB', (1460, len(jobs)*120), (38, 40, 49)); draw = ImageDraw.Draw(board)
            for row, job in enumerate(jobs):
                draw.text((8, row*120+5), job['id'], fill=(215, 208, 239))
                im = Image.open(ROOT/job['path']).convert('RGBA')
                for i in range(16):
                    sprite = im.crop(((i%4)*384, (i//4)*384, (i%4+1)*384, (i//4+1)*384))
                    sprite.thumbnail((80, 80), Image.Resampling.LANCZOS)
                    x = 140+i*80+(20 if i >= 8 else 0)
                    board.paste(sprite, (x, row*120+25), sprite)
            board.save(folder/(label+'.jpg'), quality=94)
    if failed:
        raise SystemExit(1)


if __name__ == '__main__':
    main()
