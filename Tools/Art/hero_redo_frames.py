"""Prepares redrawn hero frames for Unity (HeroRedoBuilder): <Id>_cNN (combat, 12) and <Id>_hNN (the main-menu room, 22).
All frames of a hero are cropped to one shared box and scaled so the whole canvas is 400 px tall (the feet on the canvas floor),
so every pose keeps the same scale. Combat frames go to Art/HeroRedo/<Id>/, room frames to Resources/HeroHub/<Id>/.
    python Tools/Art/hero_redo_frames.py C:/ABWork/art/heroes/out LanternKnight
"""
import glob, os, sys
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
COMBAT = os.path.join(ROOT, 'Assets', 'ArcaneBrigade', 'Art', 'HeroRedo')
HUB = os.path.join(ROOT, 'Assets', 'ArcaneBrigade', 'Resources', 'HeroHub')
HEIGHT = 400

def prepare(source, hero):
    files = {k: sorted(glob.glob(os.path.join(source, hero, f'{hero}_{k}[0-9][0-9].png'))) for k in 'ch'}
    if len(files['c']) < 12: print(hero, 'combat frames', len(files['c']), '/ 12'); return False
    images = {k: [Image.open(f).convert('RGBA') for f in v] for k, v in files.items()}
    boxes = [im.getchannel('A').point(lambda a: 255 if a > 12 else 0).getbbox() for ims in images.values() for im in ims]
    left = min(b[0] for b in boxes); top = min(b[1] for b in boxes); right = max(b[2] for b in boxes); bottom = max(b[3] for b in boxes)
    margin = int(0.03 * (bottom - top))
    box = (max(0, left - margin), max(0, top - margin), right + margin, bottom)
    scale = HEIGHT / (box[3] - box[1]); width = int(round((box[2] - box[0]) * scale / 2)) * 2
    for kind, folder in (('c', os.path.join(COMBAT, hero)), ('h', os.path.join(HUB, hero))):
        if kind == 'h' and len(images['h']) < 22: print(hero, 'room frames', len(images['h']), '/ 22'); continue
        os.makedirs(folder, exist_ok=True)
        for i, im in enumerate(images[kind][:12 if kind == 'c' else 22]):
            im.crop(box).resize((width, HEIGHT), Image.LANCZOS).save(os.path.join(folder, f'{hero}_{kind}{i + 1:02d}.png'), optimize=True)
    print(hero, 'frames', width, 'x', HEIGHT)
    return True

if __name__ == '__main__':
    for name in sys.argv[2:]: prepare(sys.argv[1], name)
