"""Integrate painting art: connected-ink slicing only; no new raster drawing.

Pass the generated Keeper sheet and stat sheet paths as two arguments.
"""
from pathlib import Path
import hashlib
import json
import re
import sys
import uuid
import numpy as np
from PIL import Image
from scipy.ndimage import label, find_objects, distance_transform_edt

ROOT = Path(__file__).resolve().parents[2]
DEST = ROOT / 'Assets/ArcaneBrigade/Resources/MienCho/ActOne'
TEMPLATE = (DEST / 'Prop_Ledger.png.meta').read_text()


def save_sprite(name, original, mask):
    rgba = original.copy()
    rgba[:, :, 3] = np.where(mask, rgba[:, :, 3], 0)
    image = Image.fromarray(rgba)
    image = image.crop(image.getbbox())
    # Transparent import padding, never a painted halo or a shadow.
    padded = Image.new('RGBA', (image.width + 24, image.height + 24))
    padded.paste(image, (12, 12))
    target = DEST / (name + '.png')
    padded.save(target)
    meta = target.with_suffix('.png.meta')
    if not meta.exists():
        text = re.sub(r'guid: [0-9a-f]+', 'guid: ' + uuid.uuid4().hex, TEMPLATE, count=1)
        text = re.sub(r'spriteID: [0-9a-f]+', 'spriteID: ' + uuid.uuid4().hex, text)
        meta.write_text(text)
    return {'name': name, 'file': str(target.relative_to(ROOT)).replace('\\', '/'), 'sha256': hashlib.sha256(target.read_bytes()).hexdigest()}


def components(path):
    pixels = np.array(Image.open(path).convert('RGBA'))
    labels, _ = label(pixels[:, :, 3] > 32)
    # Assign faint antialiased edges to their nearest solid ink component.
    indices = distance_transform_edt(labels == 0, return_distances=False, return_indices=True)
    extended = labels[tuple(indices)]
    return pixels, labels, extended, find_objects(labels)


def main():
    keeper, stats = map(Path, sys.argv[1:3])
    records = []
    pixels, solid, ink, boxes = components(keeper)
    mapping = {1:'Keeper_Whole_Body', 2:'Keeper_Kneel', 3:'Keeper_Silhouette', 4:'Pickup_PrismShard',
               6:'Keeper_Hand_Intact', 7:'Keeper_Hand_Broken', 5:'Keeper_Carpet_Intact', 8:'Keeper_Carpet_Broken',
               10:'Keeper_Eye_Open', 11:'Keeper_Eye_Closed', 13:'Prop_OrdinaryStool', 12:'Prop_StoryTelephone'}
    # Include any detached ink nearest to each object's authored centre.
    centres = [(label_id, np.array([(boxes[label_id-1][1].start + boxes[label_id-1][1].stop)/2,
                                  (boxes[label_id-1][0].start + boxes[label_id-1][0].stop)/2])) for label_id in mapping]
    groups = np.zeros(len(boxes)+1, dtype=int)
    for i, box in enumerate(boxes):
        if box is None: continue
        point = np.array([(box[1].start+box[1].stop)/2, (box[0].start+box[0].stop)/2])
        groups[i+1] = i+1 if i+1 in mapping else min(centres, key=lambda pair: np.sum((pair[1]-point)**2))[0]
    for n, name in mapping.items(): records.append(save_sprite(name, pixels, groups[ink] == n))
    pixels, solid, ink, boxes = components(stats)
    source = (ROOT/'Assets/ArcaneBrigade/Runtime/Data/RunCatalog.cs').read_text()
    effects = re.search(r'enum ItemEffect\s*\{([^}]+)', source).group(1).split(',')
    centres = [(c, r) for r in (120,295,475,650,817,978,1144) for c in (100,275,450,625,800,975,1150)]
    groups = np.zeros(len(boxes)+1, dtype=int)
    for i, box in enumerate(boxes):
        if box is None: continue
        point = np.array([(box[1].start+box[1].stop)/2, (box[0].start+box[0].stop)/2])
        groups[i+1] = 1 + min(range(49), key=lambda j: np.sum((np.array(centres[j])-point)**2))
    for i, effect in enumerate(effects): records.append(save_sprite('Stat_'+effect.strip(), pixels, groups[ink] == i+1))
    inputs = [keeper, stats]
    if len(sys.argv) > 3:
        evidence = Path(sys.argv[3]); inputs.append(evidence)
        pixels, solid, ink, boxes = components(evidence)
        h, w = pixels.shape[:2]
        centres = [np.array([(c+.5)*w/4, (r+.5)*h/2]) for r in range(2) for c in range(4)]
        groups = np.zeros(len(boxes)+1, dtype=int)
        for i, box in enumerate(boxes):
            if box is None: continue
            point = np.array([(box[1].start+box[1].stop)/2, (box[0].start+box[0].stop)/2])
            groups[i+1] = 1 + min(range(8), key=lambda j: np.sum((centres[j]-point)**2))
        names = ['Prop_HeightMarks','Prop_ExtraFootprints','Prop_SewnReply','Prop_InsideRelease','Prop_WetUmbrella','Prop_WaitingNote','Prop_VisitorRaincoat','Prop_BurntContract']
        for i, name in enumerate(names): records.append(save_sprite(name, pixels, groups[ink] == i+1))
    if len(sys.argv) > 4:
        shell = Path(sys.argv[4]); inputs.append(shell)
        pixels = np.array(Image.open(shell).convert('RGBA'))
        records.append(save_sprite('Keeper_BodyShell', pixels, pixels[:, :, 3] > 0))
    manifest = {'date':'2026-10-17', 'tool':'painted', 'operation':'New raster art; connected-ink slicing and transparent padding for Unity integration',
                'sources':[{'file':p.name,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in inputs], 'assets':records}
    (ROOT/'Docs/Theme_Consistency_Art_Provenance.json').write_text(json.dumps(manifest,indent=2)+'\n')
    print('Integrated',len(records),'painted sprites with Unity import metadata.')


if __name__ == '__main__': main()
