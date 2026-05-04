"""Arrange painted artwork in isolated cells; never paint replacement artwork.

Owner approved re-centering, transparent padding and stray-alpha cleanup.
Native canvas dimensions are retained. Run once on original generated sheets.
"""
import argparse
import json
from pathlib import Path

import numpy as np
from PIL import Image
from scipy import ndimage


def arrange(path):
    original = Image.open(path).convert('RGBA')
    pixels = np.array(original)
    labels, _ = ndimage.label(pixels[:, :, 3] > 8)
    counts = np.bincount(labels.ravel())
    counts[0] = 0
    cols, rows = (2, 1) if path.stem == 'Shop_ExitDoor' else (4, 4)
    ids = np.argsort(counts)[::-1][:cols * rows]
    if min(counts[ids]) < 1000:
        raise ValueError(f'{path}: insufficient separate full-body sprites')
    objects = ndimage.find_objects(labels)
    entries = []
    for value in ids:
        sy, sx = objects[value - 1]
        entries.append((value, (sx.start + sx.stop) / 2, (sy.start + sy.stop) / 2))
    entries.sort(key=lambda item: item[2])
    entries = [item for y in range(rows) for item in sorted(entries[y*cols:(y+1)*cols], key=lambda item: item[1])]
    sprites = []
    for value, _, _ in entries:
        # Keep painted silhouette and nearby antialiasing, discard distant specks.
        mask = ndimage.binary_dilation(labels == value, iterations=2)
        cutout = pixels.copy()
        cutout[:, :, 3] = np.where(mask & (pixels[:, :, 3] > 4), pixels[:, :, 3], 0)
        cutout[cutout[:, :, 3] == 0, :3] = 0
        sprite = Image.fromarray(cutout)
        sprites.append(sprite.crop(sprite.getbbox()))
    w, h = original.size
    cellw, cellh = w // cols, h // rows
    scale = min(cellw * .76 / max(im.width for im in sprites), cellh * .76 / max(im.height for im in sprites), 1.0)
    output = Image.new('RGBA', original.size)
    for index, sprite in enumerate(sprites):
        sprite = sprite.resize((max(1, round(sprite.width * scale)), max(1, round(sprite.height * scale))), Image.Resampling.LANCZOS)
        r, c = divmod(index, cols)
        left, top = round(c*w/cols), round(r*h/rows)
        right, bottom = round((c+1)*w/cols), round((r+1)*h/rows)
        x = round((left + right - sprite.width)/2)
        y = bottom - round((bottom-top)*.12) - sprite.height
        output.alpha_composite(sprite, (x, y))
    a = np.array(output)
    a[a[:, :, 3] <= 4] = 0
    output = Image.fromarray(a)
    output.save(path)
    return inspect(path, cols, rows)


def inspect(path, cols, rows):
    im = Image.open(path)
    alpha = im.getchannel('A')
    w, h = im.size
    margins = []
    for r in range(rows):
        for c in range(cols):
            cell = alpha.crop((round(c*w/cols), round(r*h/rows), round((c+1)*w/cols), round((r+1)*h/rows)))
            box = cell.getbbox()
            if box is None:
                raise ValueError(f'{path}: empty cell {r},{c}')
            margins.append(min(box[0]/cell.width, box[1]/cell.height, (cell.width-box[2])/cell.width, (cell.height-box[3])/cell.height))
    corners = [alpha.getpixel(p) for p in ((0,0),(w-1,0),(0,h-1),(w-1,h-1))]
    if min(margins) < .10 or any(corners):
        raise ValueError(f'{path}: margin or alpha check failed')
    return dict(file=str(path).replace('\\','/'), width=w, height=h, mode=im.mode, grid=[cols,rows], occupied_cells=cols*rows, corner_alpha=corners, minimum_cell_margin=round(min(margins),4))


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('directory', type=Path)
    parser.add_argument('--report', type=Path, required=True)
    args = parser.parse_args()
    results = [arrange(path) for path in sorted(args.directory.glob('*.png'))]
    args.report.write_text(json.dumps(dict(method='painted; owner-authorized alpha cleanup and cell alignment', files=results), indent=2) + '\n')
    print(f'{len(results)} sheets: RGBA, populated cells, zero-alpha corners, >=10% cell margins')
