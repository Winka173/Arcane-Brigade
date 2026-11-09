"""Prepares redrawn boss / big enemy frames for Unity (BigRedoBuilder).
usage: python Tools/Art/big_redo_frames.py <drawn folder, e.g. C:/ABWork/art/bigredo/out> [Id ...]

Each creature's frames <Id>_fNN.png are cropped to the union of their painted pixels (so every frame shares one canvas and
the feet stay on one baseline), scaled so the canvas is as tall as the creature needs on screen (bosses 640 px, big enemies
400 px: about 1.5x their height on a 1440p screen, no more, to keep the build small), and written to
Assets/ArcaneBrigade/Art/BigRedo/<Id>/."""
import glob, os, re, sys
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT = os.path.join(ROOT, 'Assets', 'ArcaneBrigade', 'Art', 'BigRedo')
BOSSES = {'SuitcaseLeviathan', 'ChoirEngine', 'MarqueeMaw', 'LastLantern', 'WaxAdmiral', 'SleepCollector', 'ScarletIndex', 'UndeliveredKing'}

def prepare(source, creature):
    frames = sorted(glob.glob(os.path.join(source, creature, creature + '_f[0-9][0-9].png')))
    want = 12 if creature in BOSSES else 10
    if len(frames) < want:
        print(creature, 'incomplete', len(frames), '/', want); return False
    images = [Image.open(f).convert('RGBA') for f in frames[:want]]
    boxes = [im.getchannel('A').point(lambda a: 255 if a > 12 else 0).getbbox() for im in images]
    if any(b is None for b in boxes): print(creature, 'empty frame'); return False
    left = min(b[0] for b in boxes); top = min(b[1] for b in boxes); right = max(b[2] for b in boxes); bottom = max(b[3] for b in boxes)
    margin = int(0.03 * (bottom - top))
    box = (max(0, left - margin), max(0, top - margin), right + margin, bottom)  # the feet sit on the canvas floor
    height = 640 if creature in BOSSES else 400
    scale = height / (box[3] - box[1])
    width = int(round((box[2] - box[0]) * scale / 2)) * 2
    os.makedirs(os.path.join(OUT, creature), exist_ok=True)
    for i, im in enumerate(images):
        im.crop(box).resize((width, height), Image.LANCZOS).save(os.path.join(OUT, creature, f'{creature}_f{i + 1:02d}.png'), optimize=True)
    print(creature, want, 'frames', width, 'x', height)
    return True

if __name__ == '__main__':
    source = sys.argv[1]
    names = sys.argv[2:] or [d for d in os.listdir(source) if os.path.isdir(os.path.join(source, d))]
    for name in names: prepare(source, name)
