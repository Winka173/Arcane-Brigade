"""Builds the game's UI fonts (SIL OFL 1.1 sources from google/fonts), sized to sit where Be Vietnam Pro sat:
glyph outlines are scaled so x-height and line height stay close to the old fonts, hinting is removed, and the
numbers face gets tabular digits so counters don't jitter."""
import sys, os
from fontTools.ttLib import TTFont
from fontTools.ttLib.scaleUpem import scale_upem

src, out = sys.argv[1], sys.argv[2]

def build(name, path, factor, tabular=False, family=None):
    f = TTFont(path)
    upem = f['head'].unitsPerEm
    for t in ('fpgm', 'prep', 'cvt ', 'hdmx', 'LTSH', 'VDMX'):
        if t in f: del f[t]
    glyf = f['glyf']
    for g in glyf.glyphs.values():
        g.expand(glyf)
        if hasattr(g, 'removeHinting'): g.removeHinting()
    scale_upem(f, round(upem * factor))
    f['head'].unitsPerEm = upem          # same em, bigger outlines
    if tabular:
        cmap = f.getBestCmap(); hmtx = f['hmtx']
        digits = [cmap[ord(c)] for c in '0123456789']
        width = max(hmtx[d][0] for d in digits)
        for d in digits:
            adv, lsb = hmtx[d]; shift = (width - adv) // 2
            g = glyf[d]
            if g.isComposite():
                for c in g.components: c.x += shift
            elif g.numberOfContours > 0:
                g.coordinates.translate((shift, 0))
            g.recalcBounds(glyf)
            hmtx[d] = (width, g.xMin if g.numberOfContours else lsb + shift)
    if family:
        for rec in f['name'].names:
            if rec.nameID in (1, 16): rec.string = family
            elif rec.nameID == 4: rec.string = family + ' ' + name.split('-')[-1]
            elif rec.nameID == 6: rec.string = name
    f.save(os.path.join(out, name + '.ttf'))
    print(name, 'ok')

build('AlegreyaSans-Body', os.path.join(src, 'AlegreyaSans-Bold.ttf'), 1.08, family='Alegreya Sans UI')
build('AlegreyaSans-Numbers', os.path.join(src, 'AlegreyaSans-ExtraBold.ttf'), 1.08, tabular=True, family='Alegreya Sans Numbers')
build('Fraunces-Heading', os.path.join(src, 'Fraunces-SemiBold.ttf'), 1.05, family='Fraunces UI')
build('Fraunces-Display', os.path.join(src, 'Fraunces-Black.ttf'), 1.05, family='Fraunces Display')
