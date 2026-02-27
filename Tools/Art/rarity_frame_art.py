"""Paints the ornaments of the rarity description frames (RarityDescriptionFrame.cs).

Each rarity has its own set, not one tinted shape:
  Uncommon  moon-vine: a silver-blue vine with leaves and dew running the two sides
  Rare      rune rail: carved violet stone rails with glowing rune medallions, a rune-eye crest, a tassel below
  Mythic    gilded wings: golden feathers cascading down both sides, a sun-crown crest, a gold drop below
  Crimson   blood briar: two black-red thorn stems wound together with roses, a horned ruby crest, a hanging bud below
  Rainbow   celestial prism: a chain of prism shards and stars, a crescent-moon crest, a star pendant below

Per rarity it writes, to Assets/ArcaneBrigade/Resources/RarityFrame/:
  <Rarity>_Side.png   a strip tiling vertically (drawn down the left edge, mirrored on the right)
  <Rarity>_Glow.png   the same strip's glowing parts only, pulsed over it at runtime
  <Rarity>_Crest.png  the piece over the top edge, centred
  <Rarity>_Foot.png   the piece under the bottom edge, centred
  <Rarity>_Corner.png the top-left corner cap (mirrored / flipped for the others)

Everything is drawn 3x oversampled and reduced (antialiased), in premultiplied colour, with dark outlines,
gradient fills, rim highlights and soft glows, so it reads as painted trim at tooltip size.
Usage: python Tools/Art/rarity_frame_art.py [--preview out.png]
"""
import colorsys, math, os, sys
import numpy as np
from PIL import Image, ImageDraw, ImageFilter

SS = 3
OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "ArcaneBrigade", "Resources", "RarityFrame")


class Canvas:
    def __init__(self, w, h, tile=False):
        self.w, self.h, self.tile = w, h, tile
        self.c = np.zeros((h * SS, w * SS, 4), np.float32)  # premultiplied

    # ---- masks ----
    def mask(self, draw):
        img = Image.new("L", (self.w * SS, self.h * SS), 0)
        d = ImageDraw.Draw(img)
        shifts = (-self.h, 0, self.h) if self.tile else (0,)
        for s in shifts:
            draw(d, lambda pts, s=s: [((x) * SS, (y + s) * SS) for x, y in pts], s)
        return np.asarray(img, np.float32) / 255

    def poly(self, pts):
        return self.mask(lambda d, T, s: d.polygon(T(pts), fill=255))

    def ellipse(self, cx, cy, rx, ry):
        return self.mask(lambda d, T, s: d.ellipse([(cx - rx) * SS, (cy - ry + s) * SS, (cx + rx) * SS, (cy + ry + s) * SS], fill=255))

    def line(self, pts, width):
        def dr(d, T, s):
            q = T(pts)
            d.line(q, fill=255, width=max(1, int(width * SS)), joint="curve")
            r = width * SS / 2
            for x, y in (q[0], q[-1]):
                d.ellipse([x - r, y - r, x + r, y + r], fill=255)
        return self.mask(dr)

    # ---- painting ----
    def put(self, mask, colour, alpha=1.0):
        """colour: (r,g,b) or an (H,W,3) array"""
        a = np.clip(mask * alpha, 0, 1)[..., None]
        col = np.broadcast_to(np.asarray(colour, np.float32), self.c[..., :3].shape)
        layer = np.concatenate([col * a, a], -1)
        self.c = layer + self.c * (1 - a)

    def grow(self, mask, px):
        img = Image.fromarray((mask * 255).astype(np.uint8))
        n = max(3, int(px * SS) * 2 + 1)
        if self.tile:
            arr = np.vstack([mask, mask, mask])
            img = Image.fromarray((arr * 255).astype(np.uint8)).filter(ImageFilter.MaxFilter(n | 1))
            out = np.asarray(img, np.float32)[mask.shape[0]:2 * mask.shape[0]] / 255
            return out
        return np.asarray(img.filter(ImageFilter.MaxFilter(n | 1)), np.float32) / 255

    def blur(self, mask, px):
        if self.tile:
            arr = np.vstack([mask, mask, mask])
            img = Image.fromarray((arr * 255).astype(np.uint8)).filter(ImageFilter.GaussianBlur(px * SS))
            return np.asarray(img, np.float32)[mask.shape[0]:2 * mask.shape[0]] / 255
        return np.asarray(Image.fromarray((mask * 255).astype(np.uint8)).filter(ImageFilter.GaussianBlur(px * SS)), np.float32) / 255

    def shift(self, mask, dx, dy):
        return np.roll(np.roll(mask, int(dy * SS), 0), int(dx * SS), 1)

    def grad(self, top, bottom, axis=0, lo=0.0, hi=1.0):
        n = self.c.shape[axis]
        t = np.clip((np.linspace(0, 1, n) - lo) / max(1e-6, hi - lo), 0, 1)
        top, bottom = np.asarray(top, np.float32), np.asarray(bottom, np.float32)
        g = top[None] * (1 - t[:, None]) + bottom[None] * t[:, None]
        return g[:, None, :] if axis == 0 else g[None, :, :]

    def shape(self, mask, fill, outline=(0.12, 0.08, 0.1), line=1.6, rim=None, rim_off=(-1, -1.2), glow=None, glow_px=4, glow_a=0.55, layer=None):
        """outline under, fill, rim highlight (top-left), optional glow under everything"""
        if glow is not None:
            g = self.blur(self.grow(mask, 1), glow_px)
            self.put(g, glow, glow_a)
            if layer is not None: layer.put(g, glow, glow_a)
        if line > 0:
            self.put(self.grow(mask, line), outline)
        self.put(mask, fill)
        if rim is not None:
            r = np.clip(mask - self.shift(mask, -rim_off[0] * 1.6, -rim_off[1] * 1.6), 0, 1) * mask
            self.put(r, rim, 0.75)

    def save(self, path):
        c = self.c
        chans = []
        for i in range(4):
            img = Image.fromarray(c[..., i].astype(np.float32), "F").resize((self.w, self.h), Image.LANCZOS)
            chans.append(np.asarray(img, np.float32))
        p = np.stack(chans, -1)
        a = np.clip(p[..., 3:], 0, 1)
        rgb = np.where(a > 1e-4, p[..., :3] / np.maximum(a, 1e-4), 0)
        out = np.concatenate([np.clip(rgb, 0, 1), a], -1)
        Image.fromarray((out * 255 + 0.5).astype(np.uint8), "RGBA").save(path)
        return Image.open(path)


# ---------- shape builders (final-pixel coordinates) ----------
def leaf_pts(bx, by, ang, length, width, bend=0.0, n=24):
    d = np.array([math.cos(ang), math.sin(ang)]); nrm = np.array([-d[1], d[0]])
    a, b = [], []
    for i in range(n + 1):
        t = i / n
        w = width * math.sin(math.pi * t) ** 0.8 * (1 - 0.25 * t)
        c = np.array([bx, by]) + d * length * t + nrm * bend * length * t * t
        a.append(tuple(c + nrm * w)); b.append(tuple(c - nrm * w))
    return a + b[::-1]


def feather_pts(bx, by, ang, length, width, curl=0.15, n=30):
    d = np.array([math.cos(ang), math.sin(ang)]); nrm = np.array([-d[1], d[0]])
    a, b = [], []
    for i in range(n + 1):
        t = i / n
        w = width * (math.sin(math.pi * min(1, t * 1.15)) ** 0.5 if t < 0.87 else max(0, (1 - t) / 0.13) * 0.55)
        c = np.array([bx, by]) + d * length * t + nrm * curl * length * t ** 2
        a.append(tuple(c + nrm * w * 1.1)); b.append(tuple(c - nrm * w * 0.8))
    return a + b[::-1]


def spine(bx, by, ang, length, curl=0.15, n=20):
    d = np.array([math.cos(ang), math.sin(ang)]); nrm = np.array([-d[1], d[0]])
    return [tuple(np.array([bx, by]) + d * length * t + nrm * curl * length * t ** 2) for t in np.linspace(0.05, 0.85, n)]


def star_pts(cx, cy, r, points=4, inner=0.32, rot=0):
    pts = []
    for i in range(points * 2):
        a = rot + i * math.pi / points - math.pi / 2
        rr = r if i % 2 == 0 else r * inner
        pts.append((cx + math.cos(a) * rr, cy + math.sin(a) * rr))
    return pts


def prism_pts(cx, cy, length, width, ang):
    d = np.array([math.cos(ang), math.sin(ang)]); nrm = np.array([-d[1], d[0]]); c = np.array([cx, cy])
    pts = [c + d * length * 0.5, c + d * length * 0.28 + nrm * width, c - d * length * 0.32 + nrm * width,
           c - d * length * 0.5, c - d * length * 0.32 - nrm * width, c + d * length * 0.28 - nrm * width]
    return [tuple(p) for p in pts]


def arc_pts(cx, cy, r, a0, a1, n=40):
    return [(cx + math.cos(a) * r, cy + math.sin(a) * r) for a in np.linspace(a0, a1, n)]


INK = (0.1, 0.07, 0.09)


# ---------- Uncommon: moon-vine ----------
def uncommon():
    P, W = 192, 96
    side, glow = Canvas(W, P, True), Canvas(W, P, True)
    cx = W * 0.5
    stem = [(cx + 12 * math.sin(2 * math.pi * y / P), y) for y in np.linspace(-P, 2 * P, 180)]
    m = side.line(stem, 5.0)
    side.shape(m, side.grad((0.62, 0.86, 1.0), (0.3, 0.55, 0.85)), INK, 1.4, rim=(0.9, 0.97, 1.0))
    for i, y in enumerate([P * 0.25, P * 0.75]):
        sgn = 1 if i == 0 else -1
        x = cx + 12 * sgn
        for j, (ang, ln, wd) in enumerate([(-0.55, 40, 12), (0.35, 31, 9.5)]):
            pts = leaf_pts(x, y, (0 if sgn > 0 else math.pi) + ang * sgn * -1 - 0.3 * sgn, ln, wd, bend=0.12 * sgn)
            lm = side.poly(pts)
            side.shape(lm, side.grad((0.55, 0.85, 1.0), (0.18, 0.42, 0.78)), INK, 1.4, rim=(0.92, 0.98, 1.0))
            vein = side.line(pts[:25][::3], 1.0)
        # a dew drop on the vine
        dm = side.ellipse(x - sgn * 4, y + 20, 4.2, 4.6)
        side.shape(dm, (0.85, 0.95, 1.0), INK, 1.0, glow=(0.6, 0.9, 1.0), glow_px=3, layer=glow)
        glow.put(dm, (0.9, 1, 1))
    crest = Canvas(384, 160)
    c = crest
    # a crescent moon bud with leaves opening either side
    moon = np.clip(c.ellipse(192, 92, 30, 30) - c.ellipse(204, 84, 26, 26), 0, 1)
    for sgn in (1, -1):
        for ang, ln, wd in [(-0.35, 78, 18), (-0.85, 58, 14), (0.15, 48, 11)]:
            pts = leaf_pts(192 + sgn * 20, 104, (math.pi if sgn < 0 else 0) + sgn * ang, ln, wd, bend=-0.15 * sgn)
            c.shape(c.poly(pts), c.grad((0.6, 0.88, 1.0), (0.2, 0.45, 0.8)), INK, 1.6, rim=(0.95, 1, 1))
    c.shape(moon, c.grad((1, 1, 1), (0.6, 0.82, 1.0)), INK, 1.8, glow=(0.6, 0.9, 1), glow_px=6, glow_a=0.7)
    for sx, sy, r in [(150, 52, 6), (236, 44, 5), (192, 30, 7)]:
        c.shape(c.poly(star_pts(sx, sy, r)), (1, 1, 1), INK, 0.8, glow=(0.7, 0.9, 1), glow_px=3)
    foot = Canvas(256, 96)
    for sgn in (1, -1):
        pts = leaf_pts(128, 30, math.pi / 2 + sgn * 0.7, 48, 12, bend=0.1 * sgn)
        foot.shape(foot.poly(pts), foot.grad((0.55, 0.85, 1.0), (0.2, 0.42, 0.78)), INK, 1.5, rim=(0.95, 1, 1))
    foot.shape(foot.ellipse(128, 34, 7, 7), (0.85, 0.95, 1), INK, 1.2, glow=(0.6, 0.9, 1), glow_px=4)
    corner = Canvas(128, 128)
    curl = [(20 + 40 * math.cos(a) * (1 - t * 0.5), 20 + 40 * math.sin(a) * (1 - t * 0.5)) for t, a in zip(np.linspace(0, 1, 50), np.linspace(0, math.pi / 2, 50))]
    corner.shape(corner.line(curl, 4), corner.grad((0.62, 0.86, 1.0), (0.3, 0.55, 0.85)), INK, 1.4)
    corner.shape(corner.poly(leaf_pts(22, 22, math.pi / 4, 44, 11)), corner.grad((0.6, 0.88, 1.0), (0.2, 0.45, 0.8)), INK, 1.5, rim=(1, 1, 1))
    return side, glow, crest, foot, corner


# ---------- Rare: rune rail ----------
def rune(c, cx, cy, r, seed, layer):
    rng = np.random.default_rng(seed)
    disk = c.ellipse(cx, cy, r, r)
    c.shape(disk, c.grad((0.42, 0.3, 0.62), (0.16, 0.1, 0.28)), INK, 1.6, rim=(0.85, 0.75, 1.0))
    c.shape(np.clip(c.ellipse(cx, cy, r * 0.74, r * 0.74) - c.ellipse(cx, cy, r * 0.64, r * 0.64), 0, 1), (0.75, 0.62, 1), INK, 0)
    strokes = []
    for k in range(3):
        a0 = rng.uniform(0, 2 * math.pi)
        p0 = (cx + math.cos(a0) * r * 0.45, cy + math.sin(a0) * r * 0.45)
        p1 = (cx + math.cos(a0 + rng.uniform(2, 3.5)) * r * 0.45, cy + math.sin(a0 + rng.uniform(2, 3.5)) * r * 0.45)
        strokes.append(c.line([p0, (cx + rng.uniform(-2, 2), cy + rng.uniform(-2, 2)), p1], 2.0))
    g = np.clip(sum(strokes), 0, 1)
    c.shape(g, (0.95, 0.85, 1.0), INK, 0, glow=(0.75, 0.5, 1.0), glow_px=3.5, glow_a=0.9, layer=layer)
    layer.put(g, (1, 0.92, 1))


def rare():
    P, W = 192, 96
    side, glow = Canvas(W, P, True), Canvas(W, P, True)
    cx = W * 0.5
    for x in (cx - 19, cx + 19):
        m = side.mask(lambda d, T, s, x=x: d.rectangle([(x - 4.2) * SS, (s - 2) * SS, (x + 4.2) * SS, (s + P + 2) * SS], fill=255))
        side.shape(m, side.grad((0.62, 0.5, 0.86), (0.32, 0.22, 0.5), axis=1, lo=0.3, hi=0.7), INK, 1.4, rim=(0.88, 0.8, 1.0))
    # cross ties and diamond studs between the medallions
    for y in (P * 0.0, P * 0.5):
        stud = side.poly([(cx, y - 12), (cx + 10, y), (cx, y + 12), (cx - 10, y)])
        side.shape(stud, side.grad((0.95, 0.85, 1), (0.5, 0.35, 0.8)), INK, 1.4, rim=(1, 1, 1))
    for i, y in enumerate((P * 0.25, P * 0.75)):
        rune(side, cx, y, 27, 11 + i, glow)
    crest = Canvas(384, 160)
    c = crest
    # a rune-eye medallion between two carved wing bars
    for sgn in (1, -1):
        bar = c.poly([(192 + sgn * 34, 92), (192 + sgn * 150, 70), (192 + sgn * 160, 84), (192 + sgn * 40, 112)])
        c.shape(bar, c.grad((0.62, 0.5, 0.86), (0.28, 0.2, 0.46)), INK, 1.6, rim=(0.9, 0.82, 1))
        for k in range(3):
            x = 192 + sgn * (70 + k * 30); y = 90 - k * 5
            c.shape(c.poly([(x, y - 7), (x + 5, y), (x, y + 7), (x - 5, y)]), (0.9, 0.8, 1), INK, 1.0, glow=(0.75, 0.5, 1), glow_px=2.5)
    rune(c, 192, 92, 40, 7, Canvas(384, 160))
    eye = np.clip(c.ellipse(192, 92, 18, 9), 0, 1)
    c.shape(eye, (0.98, 0.92, 1), INK, 1.4, glow=(0.8, 0.55, 1), glow_px=5, glow_a=0.9)
    c.shape(c.ellipse(192, 92, 6, 6), (0.35, 0.15, 0.6), INK, 0)
    foot = Canvas(256, 128)
    cord = foot.line([(128, 0), (128, 40)], 3)
    foot.shape(cord, (0.7, 0.58, 0.95), INK, 1.2)
    rune(foot, 128, 44, 14, 3, Canvas(256, 128))
    for k in range(9):
        x = 128 + (k - 4) * 3.2
        foot.shape(foot.line([(128 + (k - 4) * 1.2, 60), (x, 104 + abs(k - 4) * -2)], 2.2), foot.grad((0.8, 0.68, 1), (0.42, 0.28, 0.66)), INK, 0.9)
    foot.shape(foot.ellipse(128, 60, 9, 5), (0.85, 0.75, 1), INK, 1.2, rim=(1, 1, 1))
    corner = Canvas(128, 128)
    sq = corner.poly([(20, 4), (36, 20), (20, 36), (4, 20)])
    corner.shape(sq, corner.grad((0.95, 0.85, 1), (0.45, 0.3, 0.75)), INK, 1.6, rim=(1, 1, 1), glow=(0.7, 0.5, 1), glow_px=4)
    return side, glow, crest, foot, corner


# ---------- Mythic: gilded wings ----------
GOLD_HI, GOLD_LO = (1.0, 0.93, 0.62), (0.78, 0.48, 0.16)


def mythic():
    P, W = 192, 128
    side, glow = Canvas(W, P, True), Canvas(W, P, True)
    cx = W - 30  # the border line sits near the strip's inner side; feathers sweep outward (left) and down
    for k in range(4):
        y = k * P / 4 - 10
        for s in (-P, 0, P):
            pass
        pts = feather_pts(cx, y, math.pi * 0.86, 92, 13, curl=-0.12)
        fm = side.poly(pts)
        side.shape(fm, side.grad(GOLD_HI, GOLD_LO, axis=1, lo=0.0, hi=1.0), INK, 1.5, rim=(1, 1, 0.85))
        side.put(side.line(spine(cx, y, math.pi * 0.86, 92, curl=-0.12), 1.1), (0.55, 0.32, 0.1), 0.8)
        pts2 = feather_pts(cx - 4, y + 18, math.pi * 0.9, 60, 11, curl=-0.1)
        side.shape(side.poly(pts2), side.grad((1, 0.97, 0.75), (0.85, 0.58, 0.22), axis=1), INK, 1.4, rim=(1, 1, 0.9))
    # a beaded gold cord running down the border
    cord = side.mask(lambda d, T, s: d.rectangle([(cx - 2.5) * SS, (s - 2) * SS, (cx + 2.5) * SS, (s + P + 2) * SS], fill=255))
    side.shape(cord, side.grad(GOLD_HI, GOLD_LO, axis=1, lo=0.7, hi=0.85), INK, 1.2)
    for k in range(4):
        b = side.ellipse(cx, k * P / 4 + P / 8, 5.5, 5.5)
        side.shape(b, (1, 0.9, 0.55), INK, 1.2, rim=(1, 1, 1), glow=(1, 0.8, 0.35), glow_px=3, layer=glow)
        glow.put(b, (1, 0.97, 0.8))
    crest = Canvas(448, 200)
    c = crest
    cx, cy = 224, 128
    rays = np.zeros_like(c.c[..., 0])
    for i in range(13):
        a = math.pi + i * math.pi / 12
        ln = 88 if i % 2 == 0 else 62
        rays = np.maximum(rays, c.poly([(cx + math.cos(a - 0.07) * 30, cy + math.sin(a - 0.07) * 30), (cx + math.cos(a) * ln, cy + math.sin(a) * ln), (cx + math.cos(a + 0.07) * 30, cy + math.sin(a + 0.07) * 30)]))
    c.shape(rays, c.grad(GOLD_HI, GOLD_LO), INK, 1.4, glow=(1, 0.8, 0.4), glow_px=6, glow_a=0.6, rim=(1, 1, 0.9))
    for sgn in (1, -1):
        for k, (ang, ln, wd) in enumerate([(0.18, 120, 15), (0.42, 104, 14), (0.68, 86, 13), (0.95, 66, 11)]):
            a = (math.pi if sgn < 0 else 0) - sgn * ang
            pts = feather_pts(cx + sgn * 30, cy + 6, a, ln, wd, curl=0.1 * sgn)
            c.shape(c.poly(pts), c.grad(GOLD_HI, GOLD_LO, axis=0), INK, 1.5, rim=(1, 1, 0.85))
    crown = c.poly([(cx - 34, cy + 8), (cx - 38, cy - 30), (cx - 18, cy - 10), (cx, cy - 46), (cx + 18, cy - 10), (cx + 38, cy - 30), (cx + 34, cy + 8)])
    c.shape(crown, c.grad((1, 0.95, 0.7), (0.8, 0.5, 0.18)), INK, 1.8, rim=(1, 1, 0.95))
    for x, y in [(cx - 38, cy - 32), (cx, cy - 48), (cx + 38, cy - 32)]:
        c.shape(c.ellipse(x, y, 5, 5), (1, 0.97, 0.85), INK, 1.2, glow=(1, 0.85, 0.5), glow_px=3)
    c.shape(c.poly([(cx, cy - 16), (cx + 10, cy - 2), (cx, cy + 12), (cx - 10, cy - 2)]), c.grad((1, 0.6, 0.45), (0.65, 0.12, 0.12)), INK, 1.4, rim=(1, 0.9, 0.85), glow=(1, 0.5, 0.3), glow_px=3)
    foot = Canvas(256, 128)
    for sgn in (1, -1):
        foot.shape(foot.poly(feather_pts(128 + sgn * 6, 20, math.pi / 2 + sgn * 0.55, 60, 10, curl=0.1 * sgn)), foot.grad(GOLD_HI, GOLD_LO), INK, 1.4, rim=(1, 1, 0.85))
    drop = foot.poly([(128, 26)] + arc_pts(128, 60, 14, -math.pi * 0.15, math.pi * 1.15)[::-1] + [(128, 26)])
    foot.shape(drop, foot.grad((1, 0.97, 0.75), (0.8, 0.5, 0.18)), INK, 1.6, rim=(1, 1, 1), glow=(1, 0.8, 0.4), glow_px=4)
    corner = Canvas(128, 128)
    fl = corner.poly([(20, 4), (30, 16), (44, 20), (30, 24), (20, 44), (10, 24), (4, 20)])
    corner.shape(fl, corner.grad(GOLD_HI, GOLD_LO), INK, 1.5, rim=(1, 1, 0.9), glow=(1, 0.8, 0.4), glow_px=3)
    for a in (0, math.pi / 2):
        corner.shape(corner.poly(feather_pts(22, 22, a + 0.1, 70, 9, curl=0.08)), corner.grad(GOLD_HI, GOLD_LO), INK, 1.3, rim=(1, 1, 0.85))
    return side, glow, crest, foot, corner


# ---------- Crimson: blood briar ----------
RED_HI, RED_LO = (0.95, 0.25, 0.3), (0.35, 0.03, 0.08)


def rose(c, cx, cy, r, layer=None):
    c.shape(c.ellipse(cx, cy, r, r * 0.92), c.grad((0.85, 0.12, 0.2), (0.4, 0.02, 0.07)), INK, 1.6, glow=(1, 0.2, 0.25) if layer else None, glow_px=4, layer=layer)
    for k, (rr, ox, oy) in enumerate([(0.78, -0.12, -0.1), (0.56, 0.1, 0.05), (0.36, -0.06, -0.04)]):
        petal = np.clip(c.ellipse(cx + ox * r, cy + oy * r, r * rr, r * rr * 0.9) - c.ellipse(cx + ox * r + r * 0.18, cy + oy * r - r * 0.16, r * rr * 0.82, r * rr * 0.75), 0, 1)
        c.put(c.grow(petal, 0.6), INK, 0.9)
        c.put(petal, (1.0, 0.35 + k * 0.1, 0.4 + k * 0.08))
    c.shape(c.ellipse(cx - r * 0.05, cy - r * 0.05, r * 0.16, r * 0.14), (1, 0.6, 0.62), INK, 0.8)


def crimson():
    P, W = 192, 96
    side, glow = Canvas(W, P, True), Canvas(W, P, True)
    cx = W * 0.5
    for ph in (0, math.pi):
        stem = [(cx + 16 * math.sin(2 * math.pi * y / P + ph), y) for y in np.linspace(-P, 2 * P, 200)]
        side.shape(side.line(stem, 5.0), side.grad((0.45, 0.06, 0.1), (0.2, 0.02, 0.05)), INK, 1.3, rim=(0.8, 0.25, 0.3))
        for y in np.linspace(0, P, 7)[:-1] + (P / 12 if ph else 0):
            x = cx + 16 * math.sin(2 * math.pi * y / P + ph)
            sgn = 1 if math.cos(2 * math.pi * y / P + ph) > 0 else -1
            th = side.poly([(x - 3, y), (x + sgn * 17, y - 9), (x + 3, y - 4)])
            side.shape(th, (0.55, 0.08, 0.12), INK, 1.1, rim=(0.9, 0.4, 0.4))
    for sgn, y in ((1, P * 0.25), (-1, P * 0.75)):
        lf = side.poly(leaf_pts(cx, y + 18, (0 if sgn > 0 else math.pi) + 0.4 * sgn, 34, 10))
        side.shape(lf, side.grad((0.35, 0.12, 0.12), (0.12, 0.03, 0.05)), INK, 1.2, rim=(0.6, 0.25, 0.25))
        rose(side, cx + sgn * 2, y, 19, glow)
    crest = Canvas(448, 200)
    c = crest
    cx, cy = 224, 124
    for sgn in (1, -1):
        # a horn: a bezier from beside the rose, out and up, curling back in at the tip, tapering
        p0, p1, p2 = np.array([cx + sgn * 26, cy - 4]), np.array([cx + sgn * 128, cy - 6]), np.array([cx + sgn * 96, cy - 104])
        ts = np.linspace(0, 1, 48)
        horn = [tuple((1 - t) ** 2 * p0 + 2 * (1 - t) * t * p1 + t * t * p2) for t in ts]
        wid = [max(0.6, 16 * (1 - t) ** 0.85) for t in ts]
        left, right = [], []
        for k in range(len(horn)):
            q0, q1 = np.array(horn[max(0, k - 1)]), np.array(horn[min(len(horn) - 1, k + 1)])
            tan = (q1 - q0) / max(1e-6, np.linalg.norm(q1 - q0)); nrm = np.array([-tan[1], tan[0]])
            left.append(tuple(np.array(horn[k]) + nrm * wid[k])); right.append(tuple(np.array(horn[k]) - nrm * wid[k]))
        hm = c.poly(left + right[::-1])
        c.shape(hm, c.grad((0.6, 0.12, 0.16), (0.18, 0.02, 0.05)), INK, 1.8, rim=(0.95, 0.45, 0.45))
        for k in range(4):
            t = 0.15 + k * 0.18
            j = int(t * 47); x, y = horn[j]; L, R = left[j], right[j]
            c.put(c.line([L, R], 1.6), INK, 0.6)
        for k, (ang, ln, wd) in enumerate([(0.25, 70, 15), (0.7, 54, 12)]):
            pts = leaf_pts(cx + sgn * 26, cy + 20, (math.pi if sgn < 0 else 0) + sgn * ang, ln, wd)
            c.shape(c.poly(pts), c.grad((0.35, 0.12, 0.12), (0.12, 0.03, 0.05)), INK, 1.4, rim=(0.6, 0.25, 0.25))
    rose(c, cx, cy + 8, 30, c)
    gem = c.poly([(cx, cy - 46), (cx + 14, cy - 30), (cx, cy - 14), (cx - 14, cy - 30)])
    c.shape(gem, c.grad((1, 0.55, 0.6), (0.6, 0.0, 0.08)), INK, 1.6, rim=(1, 0.9, 0.9), glow=(1, 0.15, 0.2), glow_px=6, glow_a=0.8)
    foot = Canvas(256, 128)
    stem = [(128 + 4 * math.sin(t * 5), t * 60) for t in np.linspace(0, 1, 30)]
    foot.shape(foot.line(stem, 3.2), (0.4, 0.05, 0.09), INK, 1.2)
    for y in (16, 34):
        foot.shape(foot.poly([(127, y), (140, y - 6), (129, y - 2)]), (0.55, 0.08, 0.12), INK, 1)
        foot.shape(foot.poly([(129, y + 8), (116, y + 2), (127, y + 6)]), (0.55, 0.08, 0.12), INK, 1)
    bud = foot.poly([(128, 54)] + arc_pts(128, 78, 13, -math.pi * 0.2, math.pi * 1.2)[::-1])
    foot.shape(bud, foot.grad((0.95, 0.2, 0.28), (0.4, 0.02, 0.07)), INK, 1.5, rim=(1, 0.6, 0.6), glow=(1, 0.2, 0.25), glow_px=4)
    corner = Canvas(128, 128)
    knot = corner.line([(6, 46), (20, 20), (46, 6)], 4)
    corner.shape(knot, (0.45, 0.06, 0.1), INK, 1.3)
    for x, y, a in [(12, 34, math.pi * 1.1), (34, 12, -math.pi * 0.6), (20, 20, -math.pi * 0.75)]:
        corner.shape(corner.poly([(x - 3, y), (x + math.cos(a) * 14, y + math.sin(a) * 14), (x + 3, y)]), (0.55, 0.08, 0.12), INK, 1.1)
    rose(corner, 24, 24, 13, corner)
    return side, glow, crest, foot, corner


# ---------- Rainbow (Prismatic): celestial prism ----------
def hsv_img(h_arr, s, v):
    # vectorised HSV to RGB
    h6 = (h_arr % 1) * 6; i = np.floor(h6).astype(int) % 6; f = h6 - np.floor(h6)
    p = v * (1 - s); q = v * (1 - s * f); t = v * (1 - s * (1 - f))
    v = np.full_like(h_arr, v)
    r = np.choose(i, [v, q, p, p, t, v]); g = np.choose(i, [t, v, v, q, p, p]); b = np.choose(i, [p, p, t, v, v, q])
    return np.stack([r, g, b], -1).astype(np.float32)


def prism2(c, cx, cy, length, width, ang, hue, layer=None):
    pts = prism_pts(cx, cy, length, width, ang)
    m = c.poly(pts)
    h, w = c.c.shape[:2]
    yy, xx = np.mgrid[0:h, 0:w].astype(np.float32) / SS
    d = (math.cos(ang), math.sin(ang))
    t = ((xx - cx) * d[0] + (yy - cy) * d[1]) / length + 0.5
    rgb = hsv_img(hue + t * 0.5, 0.48, 1.0)
    c.shape(m, rgb, (0.14, 0.12, 0.24), 1.5, rim=(1, 1, 1), glow=(0.85, 0.95, 1), glow_px=4, glow_a=0.45, layer=layer)
    c.put(c.line([pts[0], pts[3]], 1.1), (1, 1, 1), 0.75)
    c.put(c.line([pts[1], pts[2]], 1.0), (1, 1, 1), 0.45)


def rainbow():
    P, W = 192, 96
    side, glow = Canvas(W, P, True), Canvas(W, P, True)
    cx = W * 0.5
    chain = side.mask(lambda d, T, s: d.rectangle([(cx - 1.2) * SS, (s - 2) * SS, (cx + 1.2) * SS, (s + P + 2) * SS], fill=255))
    side.shape(chain, (0.88, 0.92, 1), (0.14, 0.12, 0.24), 1.0)
    for k, (y, ang, hue) in enumerate([(P * 0.18, -0.35, 0.0), (P * 0.5, 0.35, 0.33), (P * 0.82, -0.35, 0.66)]):
        prism2(side, cx + (9 if k % 2 else -9), y, 62, 13, math.pi / 2 + ang, hue, glow)
    for y, r in [(P * 0.34, 9), (P * 0.66, 8), (P * 0.0, 10)]:
        st = side.poly(star_pts(cx, y, r))
        side.shape(st, (1, 1, 1), (0.14, 0.12, 0.24), 0.9, glow=(0.8, 0.95, 1), glow_px=3, glow_a=0.8, layer=glow)
        glow.put(st, (1, 1, 1))
    crest = Canvas(448, 200)
    c = crest
    cx, cy = 224, 110
    for i, ang in enumerate(np.linspace(-1.0, 1.0, 7)):
        a = -math.pi / 2 + ang
        ln = 96 - abs(ang) * 30
        prism2(c, cx + math.cos(a) * ln * 0.55, cy + 20 + math.sin(a) * ln * 0.55, ln * 0.7, 9, a, i / 7)
    moon = np.clip(c.ellipse(cx, cy + 14, 36, 36) - c.ellipse(cx + 14, cy + 4, 31, 31), 0, 1)
    c.shape(moon, c.grad((1, 1, 1), (0.75, 0.85, 1)), (0.14, 0.12, 0.24), 1.8, glow=(0.8, 0.95, 1), glow_px=7, glow_a=0.8, rim=(1, 1, 1))
    for sx, sy, r in [(cx - 64, cy + 54, 7), (cx + 64, cy + 54, 7), (cx - 26, cy + 64, 5), (cx + 30, cy + 66, 5)]:
        c.put(c.line([(sx, cy + 40), (sx, sy)], 1.0), (0.85, 0.9, 1), 0.8)
        c.shape(c.poly(star_pts(sx, sy, r)), (1, 1, 1), (0.14, 0.12, 0.24), 0.9, glow=(0.8, 0.95, 1), glow_px=3)
    foot = Canvas(256, 128)
    foot.put(foot.line([(128, 0), (128, 34)], 1.2), (0.85, 0.9, 1))
    st = foot.poly(star_pts(128, 58, 24, 8, 0.45))
    rgb = hsv_img(np.linspace(0, 1, foot.c.shape[1])[None, :].repeat(foot.c.shape[0], 0), 0.45, 1.0)
    foot.shape(st, rgb, (0.14, 0.12, 0.24), 1.6, rim=(1, 1, 1), glow=(0.85, 0.95, 1), glow_px=5)
    foot.shape(foot.poly(star_pts(128, 58, 9, 4, 0.35)), (1, 1, 1), (0.14, 0.12, 0.24), 0.8)
    corner = Canvas(128, 128)
    corner.shape(corner.poly(star_pts(20, 20, 18, 4, 0.3)), (1, 1, 1), (0.14, 0.12, 0.24), 1.2, glow=(0.8, 0.95, 1), glow_px=4)
    prism2(corner, 52, 18, 40, 7, 0.0, 0.1)
    prism2(corner, 18, 52, 40, 7, math.pi / 2, 0.6)
    return side, glow, crest, foot, corner


SETS = {"Uncommon": uncommon, "Rare": rare, "Mythic": mythic, "Crimson": crimson, "Rainbow": rainbow}


def main():
    os.makedirs(OUT, exist_ok=True)
    made = {}
    for name, build in SETS.items():
        side, glow, crest, foot, corner = build()
        made[name] = {}
        for part, cv in (("Side", side), ("Glow", glow), ("Crest", crest), ("Foot", foot), ("Corner", corner)):
            made[name][part] = cv.save(os.path.join(OUT, f"{name}_{part}.png"))
        print("painted", name)
    if "--preview" in sys.argv:
        preview(made, sys.argv[sys.argv.index("--preview") + 1])


def preview(made, path):
    """a mock of each frame at tooltip size, laid out as RarityDescriptionFrame does"""
    tints = {"Uncommon": (0.25, 0.7, 1), "Rare": (0.75, 0.6, 1), "Mythic": (1, 0.76, 0.35), "Crimson": (1, 0.16, 0.27), "Rainbow": (0.58, 1, 0.92)}
    bg = Image.new("RGBA", (2000, 5 * 420 + 40), (44, 32, 30, 255))
    d = ImageDraw.Draw(bg)
    for i, (name, parts) in enumerate(made.items()):
        x, y, w, h = 300, 60 + i * 420, 1100, 300
        t = tints[name]
        paper = tuple(int(255 * (0.04 * 0.81 + c * 0.19)) for c in t)
        side = parts["Side"]; sw = 34 if name != "Mythic" else 52
        scale = sw / side.width; th = int(side.height * scale)
        col = side.resize((sw, th), Image.LANCZOS)
        strip = Image.new("RGBA", (sw, h + 24))
        for yy in range(-th, h + 24, th): strip.alpha_composite(col, (0, max(0, yy)) if yy >= 0 else (0, 0), (0, -yy if yy < 0 else 0))
        inset = int(sw * (0.22 if name != "Mythic" else 0.25))
        bg.alpha_composite(strip, (x - sw + inset, y - 12))
        bg.alpha_composite(strip.transpose(Image.FLIP_LEFT_RIGHT), (x + w - inset, y - 12))
        d.rectangle([x, y, x + w, y + h], fill=paper + (250,))
        d.rectangle([x, y, x + w, y + h], outline=tuple(int(c * 255) for c in t), width=3)
        cr = parts["Crest"]; cw = int(min(w * 0.42, 300)); ch = int(cr.height * cw / cr.width)
        bg.alpha_composite(cr.resize((cw, ch), Image.LANCZOS), (x + w // 2 - cw // 2, y - int(ch * 0.62)))
        ft = parts["Foot"]; fw = int(cw * 0.5); fh = int(ft.height * fw / ft.width)
        bg.alpha_composite(ft.resize((fw, fh), Image.LANCZOS), (x + w // 2 - fw // 2, y + h - int(fh * 0.2)))
        co = parts["Corner"]; cs = 44; ci = co.resize((cs, cs), Image.LANCZOS); k = int(cs * 0.16)
        bg.alpha_composite(ci, (x - k, y - k)); bg.alpha_composite(ci.transpose(Image.FLIP_LEFT_RIGHT), (x + w - cs + k, y - k))
        bg.alpha_composite(ci.transpose(Image.FLIP_TOP_BOTTOM), (x - k, y + h - cs + k)); bg.alpha_composite(ci.transpose(Image.ROTATE_180), (x + w - cs + k, y + h - cs + k))
        d.text((x + 24, y + 20), name.upper(), fill=(230, 220, 200, 255))
    bg.convert("RGB").save(path, quality=88)


if __name__ == "__main__":
    main()
