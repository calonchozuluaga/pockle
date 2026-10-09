#!/usr/bin/env python3
"""ART-001: draw the Pockle roster identity sheet from simple, editable shape recipes.

Run from the repository root:  python3 tools/build-roster-sheet.py

Writes into docs/concepts/roster/:
  identity-sheet-01.png   front + side view of every character, face placement, lead material colour
  lineup-01.png           all twelve at relative scale against a Pip-height grid
  silhouette-test-01.png  flat black silhouettes (no colour, face, or filling) to check readability

These are proportion and silhouette concepts, not final art. Units: 1.0 = Pip's height.
x runs left to right from the toy's centre, y runs up from the ground. Requires Pillow.
"""
import math
import os
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageFont

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "docs/concepts/roster"
# Fredoka/Nunito come with the UX-002 visual system; POCKLE_FONTS can point elsewhere.
FONTS = Path(os.environ.get("POCKLE_FONTS", ROOT / "Assets/Pockle/Resources/UI/Fonts"))
SS = 3  # supersampling
INK = (74, 54, 74)
MUTED = (122, 102, 108)
PAPER = (253, 249, 243)
EYE = (82, 30, 60)
BLUSH = (240, 150, 150)


class Pen:
    """Draws unit-space primitives into a supersampled mask."""

    def __init__(self, mask, cx, ground, u):
        self.d = ImageDraw.Draw(mask)
        self.cx, self.ground, self.u = cx, ground, u

    def p(self, x, y):
        return (self.cx + x * self.u, self.ground - y * self.u)

    def ellipse(self, x0, y0, x1, y1, fill=255):
        a, b = self.p(x0, y1), self.p(x1, y0)
        self.d.ellipse([a[0], a[1], b[0], b[1]], fill=fill)

    def circle(self, x, y, r, fill=255):
        self.ellipse(x - r, y - r, x + r, y + r, fill)

    def rrect(self, x0, y0, x1, y1, r, fill=255):
        a, b = self.p(x0, y1), self.p(x1, y0)
        self.d.rounded_rectangle([a[0], a[1], b[0], b[1]], radius=r * self.u, fill=fill)

    def poly(self, pts, fill=255):
        self.d.polygon([self.p(x, y) for x, y in pts], fill=fill)

    def ring(self, x, y, rx, ry, w):
        self.ellipse(x - rx, y - ry, x + rx, y + ry, 255)
        self.ellipse(x - rx + w, y - ry + w, x + rx - w, y + ry - w, 0)

    def stroke(self, pts, r0, r1):
        """A tapered tube through points, from radius r0 to r1."""
        n = len(pts) - 1
        for i in range(n * 12 + 1):
            t = i / (n * 12)
            k = min(int(t * n), n - 1)
            f = t * n - k
            x = pts[k][0] + (pts[k + 1][0] - pts[k][0]) * f
            y = pts[k][1] + (pts[k + 1][1] - pts[k][1]) * f
            self.circle(x, y, r0 + (r1 - r0) * t)

    def clip_ground(self):
        g = self.ground
        self.d.rectangle([0, g, 100000, 100000], fill=0)


# Each character: name, lead finish, accent RGB, height note, front(pen), side(pen),
# face (front eyes x-spacing, eye y, scale) and side face (x, y), plus softness (blur for organic merging).
def pip_front(p):
    p.ellipse(-.45, 0, .45, .70); p.ellipse(-.33, .22, .33, .80); p.rrect(-.42, 0, .42, .14, .06)
    p.circle(.08, .92, .12); p.circle(-.21, .85, .08)


def pip_side(p):
    p.ellipse(-.38, 0, .38, .70); p.ellipse(-.27, .22, .27, .80); p.rrect(-.35, 0, .35, .14, .06)
    p.circle(-.04, .92, .12); p.circle(.16, .85, .08)


def dew_front(p):
    p.ellipse(-.52, 0, .52, .46)
    p.circle(-.31, .50, .21); p.circle(0, .62, .24); p.circle(.31, .50, .21)
    p.stroke([(.02, .82), (.06, .93), (.16, .99), (.22, .94)], .055, .05)
    p.circle(.235, .905, .065)


def dew_side(p):
    p.ellipse(-.38, 0, .38, .56); p.circle(-.05, .58, .29); p.circle(.15, .5, .22)
    p.stroke([(-.02, .82), (.03, .93), (.13, .99), (.19, .94)], .055, .05)
    p.circle(.205, .905, .065)


def tula_front(p):
    p.ellipse(-.62, .16, .62, .62); p.rrect(-.58, .16, .58, .28, .1)
    for x in (-.46, .46): p.rrect(x - .11, 0, x + .11, .22, .07)
    p.ellipse(-.20, .02, .20, .34)


def tula_side(p):
    p.ellipse(-.56, .16, .44, .62); p.rrect(-.54, .16, .42, .28, .1)
    for x in (-.34, .26): p.rrect(x - .1, 0, x + .1, .22, .07)
    p.stroke([(.36, .22), (.52, .22)], .1, .11); p.circle(.58, .22, .15)


def ripple_front(p):
    p.ellipse(-.70, .04, .70, .38); p.ellipse(-.38, .04, .38, .48)
    for s in (-1, 1): p.circle(s * .62, .37, .095)


def ripple_side(p):
    p.ellipse(-.34, .04, .40, .42)
    p.stroke([(-.30, .2), (-.48, .25), (-.60, .30)], .08, .045)


def moss_front(p):
    p.ellipse(-.32, .05, .32, .58)
    for s in (-1, 1): p.ellipse(s * .17 - .1, 0, s * .17 + .1, .13)
    # Broad leaf hood: wide rounded leaf with a pointed tip and two drooping side tips.
    p.ellipse(-.52, .44, .52, .92)
    p.poly([(-.18, .86), (0, 1.06), (.18, .86)])
    for s in (-1, 1): p.poly([(s * .30, .62), (s * .62, .42), (s * .46, .74)])


def moss_side(p):
    p.ellipse(-.27, .05, .30, .58); p.ellipse(-.1, 0, .12, .13)
    p.ellipse(-.48, .46, .30, .92); p.poly([(-.05, .86), (-.02, 1.06), (.16, .84)])
    p.poly([(-.30, .62), (-.66, .40), (-.48, .76)])


def nook_front(p):
    p.rrect(-.43, .05, .43, .80, .19)
    for s in (-1, 1):
        p.circle(s * .40, .08, .09); p.circle(s * .43, .70, .085)


def nook_side(p):
    p.rrect(-.25, .05, .25, .80, .14); p.ellipse(-.30, .25, .30, .62)
    p.circle(.0, .08, .09); p.circle(.04, .70, .085)


def wisp_front(p):
    p.circle(0, .40, .37); p.rrect(-.25, 0, .25, .12, .06)
    p.stroke([(.20, .66), (.36, .84), (.30, .98), (.16, .99), (.13, .90)], .11, .045)


def wisp_side(p):
    p.circle(0.04, .40, .37); p.rrect(-.2, 0, .28, .12, .06)
    p.stroke([(-.22, .64), (-.40, .80), (-.44, .96), (-.30, 1.02), (-.24, .93)], .11, .045)


def loop_front(p):
    p.circle(0, .37, .27); p.rrect(-.2, 0, .2, .12, .06)
    for s in (-1, 1): p.ring(s * .47, .44, .25, .21, .095)


def loop_side(p):
    p.circle(0, .37, .27); p.rrect(-.2, 0, .2, .12, .06)
    p.ellipse(-.10, .23, .10, .66)


def bop_front(p):
    p.ellipse(-.56, .34, .56, .78)
    p.poly([(-.40, .44), (.40, .44), (.12, .17), (-.12, .17)])
    p.ellipse(-.18, .10, .18, .30)
    p.circle(.17, .84, .12); p.circle(.21, .96, .045)
    for s in (-1, 1): p.circle(s * .21, .08, .095)


def bop_side(p):
    p.ellipse(-.56, .34, .56, .78)
    p.poly([(-.40, .44), (.40, .44), (.12, .17), (-.12, .17)])
    p.ellipse(-.18, .10, .18, .30)
    p.circle(-.05, .85, .12); p.circle(-.07, .97, .045)
    p.circle(.03, .08, .095); p.circle(-.08, .08, .09)


def rolo_front(p):
    p.rrect(-.38, .06, .38, .98, .2); p.ellipse(-.43, .22, .43, .84)
    for s in (-1, 1): p.rrect(s * .52 - .07, .44, s * .52 + .07, .62, .06)
    for s in (-1, 1): p.rrect(s * .18 - .1, 0, s * .18 + .1, .1, .04)


def rolo_side(p):
    p.rrect(-.30, .06, .30, .98, .18); p.ellipse(-.34, .22, .34, .84)
    p.rrect(-.07, .44, .07, .62, .06); p.rrect(.0, .44, .40, .62, .06)
    p.rrect(-.1, 0, .14, .1, .04)


def mallow_front(p):
    p.rrect(-.43, .02, .43, .72, .22); p.ellipse(-.43, .55, .43, .80)
    # Off-centre crescent crest.
    p.stroke([(-.34, .76), (-.33, .90), (-.24, 1.0), (-.10, 1.03)], .07, .03)


def mallow_side(p):
    p.rrect(-.33, .02, .33, .72, .2); p.ellipse(-.33, .55, .33, .80)
    p.ellipse(-.08, .74, .04, 1.0)


def sprig_front(p):
    p.ellipse(-.33, .08, .33, .84)
    for s in (-1, 1): p.ellipse(s * .12 - .07, 0, s * .12 + .07, .14)
    p.rrect(-.035, .78, .035, .94, .03)
    p.ellipse(-.075, .92, .075, 1.12)
    for s in (-1, 1):
        p.stroke([(s * .02, .9), (s * .14, .97), (s * .22, .99)], .07, .065)


def sprig_side(p):
    p.ellipse(-.29, .08, .31, .84); p.ellipse(-.05, 0, .1, .14)
    p.rrect(-.035, .78, .035, .94, .03); p.ellipse(-.07, .92, .07, 1.12)
    p.stroke([(0, .9), (.12, .97), (.2, .99)], .07, .065)


ROSTER = [
    # name, lead, rgb, front, side, (eye spacing, eye y, face scale), side eye (x, y), soft, gloss, note
    ("Pip", "Clear Jelly", (245, 184, 156), pip_front, pip_side, (.15, .40, 1.0), (.26, .40), 5, True,
     "Teardrop body; two crown bumps, larger one right of centre"),
    ("Dew", "Clear Jelly", (156, 219, 220), dew_front, dew_side, (.16, .28, 1.0), (.30, .28), 3, True,
     "Three broad cloud lobes; one short curled droplet antenna"),
    ("Tula", "Mochi Foam", (171, 202, 174), tula_front, tula_side, (.08, .20, .62), (.64, .23), 6, False,
     "Low dome on four stubby feet; small head peeks out the front"),
    ("Ripple", "Pearl Jelly", (169, 191, 235), ripple_front, ripple_side, (.15, .26, .9), (.30, .25), 7, True,
     "Wide manta cushion; rounded fin tips; short tail"),
    ("Moss", "Velvet Flock", (148, 186, 138), moss_front, moss_side, (.11, .31, .8), (.22, .31), 6, False,
     "Shy sprite under a broad pointed leaf hood; face set in the hood's shade"),
    ("Nook", "Bouclé Plush", (212, 187, 165), nook_front, nook_side, (.15, .44, 1.0), (.20, .44), 6, False,
     "Rounded-square pillow; soft paws at all four corners"),
    ("Wisp", "Velvet Flock", (199, 182, 223), wisp_front, wisp_side, (.13, .40, .95), (.32, .40), 4, False,
     "Round comet head; one thick tail curling up and over"),
    ("Loop", "Bouclé Plush", (234, 188, 207), loop_front, loop_side, (.10, .40, .8), (.20, .40), 6, False,
     "Round body with two thick bow loops; openings stay readable"),
    ("Bop", "Gloss Vinyl", (241, 196, 123), bop_front, bop_side, (.16, .56, 1.0), (.44, .56), 3, True,
     "Spinning-top body widest at the middle; small offset cap; chunky feet"),
    ("Rolo", "Matte Vinyl", (150, 187, 199), rolo_front, rolo_side, (.13, .70, .9), (.24, .70), 3, False,
     "Barrel robot; side knobs; belly panel below a screen face"),
    ("Mallow", "Satin Vinyl", (211, 197, 229), mallow_front, mallow_side, (.14, .40, 1.0), (.26, .40), 4, False,
     "Marshmallow block with a flat base; off-centre crescent crest"),
    ("Sprig", "Coated Metallic", (174, 203, 160), sprig_front, sprig_side, (.12, .46, .9), (.24, .46), 4, True,
     "Seed capsule on two tiny feet; three thick round-tipped leaves"),
]


def shape(fn, size, cx, ground, u, soft):
    mask = Image.new("L", size, 0)
    pen = Pen(mask, cx, ground, u)
    fn(pen)
    if soft:
        # Blur-and-threshold merges primitives into one soft, moulded body.
        mask = mask.filter(ImageFilter.GaussianBlur(soft * SS)).point(lambda v: 255 if v > 118 else 0)
    Pen(mask, cx, ground, u).clip_ground()
    return mask.filter(ImageFilter.GaussianBlur(SS * .6))


def render(img, mask, rgb, gloss, cx, ground, u):
    w, h = img.size
    # Contact shadow.
    sh = Image.new("L", img.size, 0)
    bbox = mask.getbbox() or (0, 0, 1, 1)
    ImageDraw.Draw(sh).ellipse([bbox[0] + 4 * SS, ground - 7 * SS, bbox[2] - 4 * SS, ground + 7 * SS], fill=70)
    img.paste((150, 130, 130), (0, 0), sh.filter(ImageFilter.GaussianBlur(6 * SS)))
    # Body: base colour with a soft top-to-bottom shade.
    grad = Image.new("RGB", img.size)
    gd = ImageDraw.Draw(grad)
    top, bottom = bbox[1], bbox[3]
    for y in range(top, bottom + 1):
        t = (y - top) / max(1, bottom - top)
        k = 1.06 - .22 * t
        gd.line([(bbox[0], y), (bbox[2], y)], fill=tuple(min(255, int(c * k)) for c in rgb))
    img.paste(grad, (0, 0), mask)
    # Rim darkening keeps the silhouette crisp on light backgrounds.
    rim = ImageChops.subtract(mask, mask.filter(ImageFilter.MinFilter(5)))
    img.paste(tuple(int(c * .72) for c in rgb), (0, 0), rim)
    if gloss:
        hl = Image.new("L", img.size, 0)
        hx, hy = bbox[0] + (bbox[2] - bbox[0]) * .30, top + (bottom - top) * .18
        r = (bbox[2] - bbox[0]) * .09
        ImageDraw.Draw(hl).ellipse([hx - r * 1.4, hy - r * .8, hx + r * 1.4, hy + r * .8], fill=200)
        hl = ImageChops.multiply(hl.filter(ImageFilter.GaussianBlur(3 * SS)), mask)
        img.paste((255, 255, 255), (0, 0), hl)


def details(img, name, cx, ground, u, rgb, side=False):
    """Surface details that carry identity: Rolo's screen and belly panel, Moss's shaded face recess."""
    d = ImageDraw.Draw(img)
    pen = Pen(Image.new("L", (1, 1)), cx, ground, u)
    tint = lambda k: tuple(min(255, int(c * k)) for c in rgb)
    def rr(x0, y0, x1, y1, r, col):
        a, b = pen.p(x0, y1), pen.p(x1, y0)
        d.rounded_rectangle([a[0], a[1], b[0], b[1]], radius=r * u, fill=col)
    if name == "Rolo" and not side:
        rr(-.27, .54, .27, .86, .1, tint(1.18))
        rr(-.20, .14, .20, .40, .07, tint(.88))
        for k in range(3):
            a, b = pen.p(-.12 + k * .12 - .025, .30), pen.p(-.12 + k * .12 + .025, .25)
            d.ellipse([a[0], a[1], b[0], b[1]], fill=tint(1.15))
    if name == "Moss" and not side:
        a, b = pen.p(-.24, .50), pen.p(.24, .14)
        d.ellipse([a[0], a[1], b[0], b[1]], fill=tint(.86))


def face(img, cx, ground, u, spacing, ey, scale, side_x=None):
    d = ImageDraw.Draw(img)
    s = u * scale
    eyes = [(-spacing, ey), (spacing, ey)] if side_x is None else [(side_x, ey)]
    for x, y in eyes:
        px, py = cx + x * u, ground - y * u
        d.ellipse([px - .045 * s, py - .062 * s, px + .045 * s, py + .062 * s], fill=EYE)
        d.ellipse([px - .02 * s + .012 * s, py - .045 * s, px + .012 * s, py - .015 * s], fill=(255, 255, 255))
        bx = px + (.085 * s if x >= 0 else -.085 * s)
        if side_x is None or True:
            d.ellipse([bx - .045 * s, py + .03 * s, bx + .045 * s, py + .065 * s], fill=BLUSH)
    if side_x is None:
        mx, my = cx, ground - (ey - .065 * scale) * u
        d.arc([mx - .045 * s, my - .04 * s, mx + .045 * s, my + .03 * s], 20, 160, fill=EYE, width=max(2, int(.012 * s)))
    else:
        mx, my = cx + (side_x + .05 * scale) * u, ground - (ey - .065 * scale) * u
        d.arc([mx - .03 * s, my - .03 * s, mx + .03 * s, my + .02 * s], 30, 150, fill=EYE, width=max(2, int(.012 * s)))


def font(name, size):
    try:
        return ImageFont.truetype(str(FONTS / name), size)
    except OSError:
        return ImageFont.truetype("DejaVuSans-Bold.ttf" if "Fredoka" in name else "DejaVuSans.ttf", size)


def identity_sheet():
    cols, rows = 4, 3
    cw, ch = 520 * SS, 430 * SS
    head = 150 * SS
    sheet = Image.new("RGB", (cols * cw, rows * ch + head), PAPER)
    d = ImageDraw.Draw(sheet)
    d.text((40 * SS, 34 * SS), "POCKLE — ROSTER IDENTITY SHEET 01", font=font("Fredoka-SemiBold.ttf", 34 * SS), fill=INK)
    d.text((40 * SS, 86 * SS), "ART-001 · front and side views at relative scale · lead material colour · working names for review",
           font=font("Nunito-SemiBold.ttf", 18 * SS), fill=MUTED)
    u = 160 * SS
    for i, (name, lead, rgb, front, side, (sp, ey, fs), (sx, sy), soft, gloss, note) in enumerate(ROSTER):
        ox, oy = (i % cols) * cw, head + (i // cols) * ch
        d.rounded_rectangle([ox + 14 * SS, oy + 10 * SS, ox + cw - 14 * SS, oy + ch - 14 * SS], 22 * SS, fill=(255, 253, 249),
                            outline=(236, 226, 216), width=SS * 2)
        ground = oy + ch - 112 * SS
        d.line([ox + 34 * SS, ground, ox + cw - 34 * SS, ground], fill=(232, 222, 212), width=SS)
        for view, (fn, cx) in enumerate(((front, ox + cw * .29), (side, ox + cw * .73))):
            # Build the mask in a local tile (fast), then place it on the sheet.
            tile_w, tile_h = int(2.0 * u), int(1.3 * u)
            lcx, lground = tile_w / 2, int(1.2 * u)
            local = shape(fn, (tile_w, tile_h), lcx, lground, u, soft)
            box = (int(cx - lcx), int(ground - lground))
            tile = sheet.crop((box[0], box[1], box[0] + tile_w, box[1] + tile_h))
            render(tile, local, rgb, gloss, lcx, lground, u)
            details(tile, name, lcx, lground, u, rgb, side=view == 1)
            if view == 0:
                face(tile, lcx, lground, u, sp, ey, fs)
            else:
                face(tile, lcx, lground, u, 0, sy, fs, side_x=sx)
            sheet.paste(tile, box)
            d.text((cx, ground + 14 * SS), "FRONT" if view == 0 else "SIDE", font=font("Fredoka-SemiBold.ttf", 12 * SS),
                   fill=MUTED, anchor="ma")
        d.text((ox + 36 * SS, ground + 40 * SS), name, font=font("Fredoka-SemiBold.ttf", 26 * SS), fill=INK)
        nw = font("Fredoka-SemiBold.ttf", 26 * SS).getlength(name)
        d.text((ox + 36 * SS + nw + 12 * SS, ground + 50 * SS), lead.upper(), font=font("Fredoka-SemiBold.ttf", 12 * SS), fill=MUTED)
        d.text((ox + 36 * SS, ground + 76 * SS), note, font=font("Nunito-SemiBold.ttf", 14 * SS), fill=MUTED)
    return sheet.resize((sheet.width // SS, sheet.height // SS), Image.LANCZOS)


def lineup(silhouette=False):
    u = 190 * SS
    gap = 40 * SS
    widths = []
    masks = []
    width = 0
    for item in ROSTER:
        m = shape(item[3], (int(2 * u), int(1.4 * u)), u, int(1.25 * u), u, item[7])
        bb = m.getbbox()
        masks.append((m, bb)); widths.append(bb[2] - bb[0])
    width = sum(widths) + gap * (len(ROSTER) + 1)
    h = int(1.25 * u) + 170 * SS
    img = Image.new("RGB", (width, h), PAPER)
    d = ImageDraw.Draw(img)
    ground = int(1.25 * u) + 60 * SS
    title = "SILHOUETTE TEST 01 — no colour, face, or filling" if silhouette else "LINEUP 01 — relative scale (grid = ¼ Pip height)"
    d.text((gap, 20 * SS), title, font=font("Fredoka-SemiBold.ttf", 24 * SS), fill=INK)
    if not silhouette:
        for k in range(1, 5):
            y = ground - k * .25 * u
            d.line([gap // 2, y, width - gap // 2, y], fill=(236, 226, 216) if k != 4 else (226, 196, 186), width=SS)
    d.line([gap // 2, ground, width - gap // 2, ground], fill=(214, 200, 188), width=SS * 2)
    x = gap
    for item, (m, bb), w in zip(ROSTER, masks, widths):
        cx = x + (u - bb[0])  # the toy's own centre, not its bounding-box centre
        box = (int(x - bb[0]), int(ground - 1.25 * u))
        tile = img.crop((box[0], box[1], box[0] + m.width, box[1] + m.height))
        if silhouette:
            tile.paste((40, 30, 40), (0, 0), m)
        else:
            render(tile, m, item[2], item[8], u, int(1.25 * u), u)
            details(tile, item[0], u, int(1.25 * u), u, item[2])
            sp, ey, fs = item[5]
            face(tile, u, int(1.25 * u), u, sp, ey, fs)
        img.paste(tile, box)
        d.text((cx, ground + 18 * SS), item[0], font=font("Fredoka-SemiBold.ttf", 20 * SS), fill=INK, anchor="ma")
        x += w + gap
    return img.resize((img.width // SS, img.height // SS), Image.LANCZOS)


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    identity_sheet().save(OUT / "identity-sheet-01.png", optimize=True)
    lineup().save(OUT / "lineup-01.png", optimize=True)
    lineup(silhouette=True).save(OUT / "silhouette-test-01.png", optimize=True)
    print("Wrote", ", ".join(p.name for p in sorted(OUT.glob("*.png"))))


if __name__ == "__main__":
    main()
