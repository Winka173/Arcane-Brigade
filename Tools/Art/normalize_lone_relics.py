"""Owner-authorized alpha cleanup and 512px delivery layout; no drawn substitutes."""
import json
import argparse
from pathlib import Path
import numpy as np
from PIL import Image
from scipy import ndimage

parser = argparse.ArgumentParser()
parser.add_argument('--folder', type=Path, default=Path('Assets/ArcaneBrigade/Art/Playtest/LoneRelics'))
parser.add_argument('--report', type=Path, default=Path('Docs/Lone_Relics_Art_Verification.json'))
args = parser.parse_args()
folder = args.folder
report = []
for path in sorted(folder.glob('*.png')):
    original = Image.open(path).convert('RGBA')
    pixels = np.array(original)
    labels, _ = ndimage.label(pixels[:,:,3] > 8)
    counts = np.bincount(labels.ravel())
    counts[0] = 0
    keep = ndimage.binary_dilation(counts[labels] >= 16, iterations=2)
    pixels[:,:,3] = np.where(keep & (pixels[:,:,3] > 4), pixels[:,:,3], 0)
    pixels[pixels[:,:,3] == 0] = 0
    cleaned = Image.fromarray(pixels)
    subject = cleaned.crop(cleaned.getbbox())
    scale = 408 / max(subject.size)
    subject = subject.resize((round(subject.width*scale),round(subject.height*scale)), Image.Resampling.LANCZOS)
    out = Image.new('RGBA',(512,512))
    out.alpha_composite(subject,((512-subject.width)//2,(512-subject.height)//2))
    data = np.array(out)
    data[data[:,:,3] <= 4] = 0
    out = Image.fromarray(data)
    out.save(path)
    alpha = out.getchannel('A')
    box = alpha.getbbox()
    corners = [alpha.getpixel(p) for p in ((0,0),(511,0),(0,511),(511,511))]
    assert min(box[0],box[1],512-box[2],512-box[3]) >= 50
    assert not any(corners)
    report.append(dict(file=str(path).replace('\\','/'), mode=out.mode, size=list(out.size), source_size=list(original.size), corner_alpha=corners, content_bounds=list(box)))
args.report.write_text(json.dumps(dict(tool='painted', cleanup='Owner approved alpha cleanup and layout; explicit 512x512 relic delivery', files=report),indent=2)+'\n')
print(f'{len(report)} relics: RGBA, 512x512, centered, clear borders and zero-alpha corners')
