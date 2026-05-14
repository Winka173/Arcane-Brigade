"""Normalize painted sheets to 256px cells; layout/alpha cleanup only, never draws artwork."""
import json
import argparse
from pathlib import Path
from PIL import Image
import numpy as np

ROOT = Path(__file__).resolve().parents[2]
parser = argparse.ArgumentParser()
parser.add_argument('--manifest', default='Docs/Hero_Effect_Art_Manifest.json')
parser.add_argument('--report', default='Docs/Hero_Effect_Art_Verification.json')
args = parser.parse_args()
manifest_path = ROOT / args.manifest
manifest = json.loads(manifest_path.read_text(encoding='utf-8'))
report = []
prepared = []
for job in manifest['jobs']:
    image = Image.open(job['source'])
    if image.mode != 'RGBA':
        raise ValueError(job['name'] + ': source has no genuine alpha')
    cells, anchors, boxes = [], [], []
    # The generator can space moving mechanisms unevenly. Split in actual transparent gutters before
    # registering them into equal delivery cells; never cut a piston/shell at a nominal grid boundary.
    projection = (np.array(image)[:,:,3] >= 12).sum(axis=0)
    cuts = [0]
    for frame in range(1,job['frames']):
        nominal = image.width * frame // job['frames']
        radius = image.width // job['frames'] // 4
        candidates = [x for x in range(max(2,nominal-radius),min(image.width-2,nominal+radius)) if not projection[x-1:x+2].any()]
        if not candidates: raise ValueError(job['name'] + ': frames overlap; regenerate')
        cuts.append(min(candidates,key=lambda x:abs(x-nominal)))
    cuts.append(image.width)
    for frame in range(job['frames']):
        cell = image.crop((cuts[frame], 0, cuts[frame+1], image.height))
        a = np.array(cell)
        a[a[:, :, 3] < 12] = 0  # owner-authorized removal of faint alpha debris
        cell = Image.fromarray(a)
        box = cell.getchannel('A').getbbox()
        if not box or box[0] <= 0 or box[1] <= 0 or box[2] >= cell.width or box[3] >= cell.height:
            raise ValueError(job['name'] + ': clipped or empty source cell ' + str(frame))
        x0, y0, x1, y1 = box
        anchor = job.get('anchor', 'center')
        if anchor == 'wine':
            mask = (a[:,:,0] > 90) & (a[:,:,1] < 80) & (a[:,:,2] < 100) & (a[:,:,0] > a[:,:,1] * 1.5) & (a[:,:,3] > 128)
            yy, xx = np.where(mask)
            if len(xx) < 50: raise ValueError('Missing fixed wine-red panel: ' + job['name'])
            point = (float(np.median(xx)), float(np.median(yy)))
        elif anchor == 'right_bottom': point = (x1, y1)
        elif anchor == 'left_bottom': point = (x0, y1)
        elif anchor == 'bottom_center':
            yy, xx = np.where(a[max(y0,y1-max(3,(y1-y0)//8)):y1,:,3] > 128)
            point = (float(np.median(xx)), y1)
        elif anchor == 'top_center':
            yy, xx = np.where(a[y0:min(y1,y0+max(3,(y1-y0)//8)),:,3] > 128)
            point = (float(np.median(xx)), y0)
        elif anchor == 'canvas': point = (cell.width/2, cell.height/2)
        else: point = ((x0+x1)/2, (y0+y1)/2)
        cells.append(cell); anchors.append(point)
        boxes.append((x0-point[0], y0-point[1], x1-point[0], y1-point[1]))
    prepared.append((job, cells, anchors, boxes))

for job, cells, anchors, boxes in prepared:
    group = job.get('registration_group')
    all_boxes = [b for j,c,a,bs in prepared if group and j.get('registration_group') == group for b in bs] if group else boxes
    union = (min(b[0] for b in all_boxes), min(b[1] for b in all_boxes), max(b[2] for b in all_boxes), max(b[3] for b in all_boxes))
    scale = min(192/(union[2]-union[0]), 192/(union[3]-union[1]))
    width, height = round((union[2]-union[0])*scale), round((union[3]-union[1])*scale)
    sheet = Image.new('RGBA', (256*job['frames'], 256))
    bounds = []
    for i,(cell,point) in enumerate(zip(cells,anchors)):
        # A common scale and registered anchor preserve the mechanism's motion instead of resizing each pose to fit.
        normalized = cell.transform((width,height), Image.Transform.AFFINE,
            (1/scale,0,point[0]+union[0],0,1/scale,point[1]+union[1]), resample=Image.Resampling.BICUBIC)
        sheet.alpha_composite(normalized, (i*256+(256-width)//2,(256-height)//2))
        box = sheet.crop((i*256,0,(i+1)*256,256)).getchannel('A').getbbox()
        if not box or min(box[0],box[1],256-box[2],256-box[3]) < 30:
            raise ValueError(job['name'] + ': insufficient exported margin')
        bounds.append(list(box))
    out = ROOT / job['path']; out.parent.mkdir(parents=True,exist_ok=True)
    sheet.save(out,optimize=True)
    report.append(dict(name=job['name'],path=job['path'],frames=job['frames'],size=list(sheet.size),mode=sheet.mode,
        corner_alpha=[sheet.getchannel('A').getpixel(p) for p in [(0,0),(sheet.width-1,0),(0,255),(sheet.width-1,255)]],
        cell_bounds=bounds,bytes=out.stat().st_size,anchor=job.get('anchor','center')))
(ROOT/args.report).write_text(json.dumps(dict(tool='painted',normalization='Shared scale, registered anchors, alpha <12 debris removed; no code-painted substitutes',sheets=report),indent=2)+'\n',encoding='utf-8')
print(f'{len(report)} sheets / {sum(j["frames"] for j in report)} frames verified; {sum(j["bytes"] for j in report):,} PNG bytes')
