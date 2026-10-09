#!/usr/bin/env python3
"""Build Pockle's UI wordmark and tab icons (UX-002).

Run from the repository root:  python3 tools/build-ui-assets.py [path/to/Fredoka-Bold.ttf]

Outputs PNGs into Assets/Pockle/Resources/UI/. Icons are white on transparent so the
UI can tint them with theme colors. The wordmark needs a bold Fredoka instance
(SIL Open Font License); pass its path, or the script falls back to the bundled
SemiBold weight. Requires Pillow.
"""
import math
import sys
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageFont

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Assets/Pockle/Resources/UI"
INK = (74, 54, 74)
PEACH = (247, 176, 146)
PEACH_DEEP = (233, 136, 108)
SS = 4  # supersampling factor for smooth edges


def wordmark(font_path):
    """Squishy, slightly bouncing letters with a jelly highlight and plum outline."""
    text = "Pockle"
    size = 260 * SS
    font = ImageFont.truetype(str(font_path), size)
    tilt = [-6, 4, -3, 5, -4, 6]
    lift = [0, -14, 6, -10, 4, -16]
    glyphs = []
    for i, ch in enumerate(text):
        box = font.getbbox(ch)
        w, h = box[2] - box[0], box[3] - box[1]
        pad = 60 * SS
        mask = Image.new("L", (w + pad * 2, h + pad * 2), 0)
        ImageDraw.Draw(mask).text((pad - box[0], pad - box[1]), ch, font=font, fill=255)
        mask = mask.rotate(tilt[i], resample=Image.BICUBIC, expand=False)
        glyphs.append((mask, font.getlength(ch), box, pad))
    total = sum(adv for _, adv, _, _ in glyphs) - 8 * SS * (len(text) - 1)
    width, height = int(total + 160 * SS), int(size * 1.35 + 120 * SS)
    shape = Image.new("L", (width, height), 0)
    x, base = 80 * SS, 70 * SS
    for i, (mask, adv, box, pad) in enumerate(glyphs):
        shape.paste(255, (int(x + box[0] - pad), int(base + box[1] - pad + lift[i] * SS)), mask)
        x += adv - 8 * SS
    outline = dilate(shape, 11 * SS)
    shadow = outline.filter(ImageFilter.GaussianBlur(14 * SS))
    canvas = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    shadow_offset = Image.new("L", (width, height), 0)
    shadow_offset.paste(shadow, (0, 16 * SS))
    canvas.paste((INK[0], INK[1], INK[2], 70), (0, 0), shadow_offset)
    canvas.paste(INK + (255,), (0, 0), outline)
    # Peach body, deepening toward the bottom like light through gel.
    grad = Image.new("RGBA", (width, height))
    gd = ImageDraw.Draw(grad)
    for y in range(height):
        t = min(1, max(0, (y - base) / (size * 1.0)))
        gd.line([(0, y), (width, y)], fill=tuple(int(PEACH[c] + (PEACH_DEEP[c] - PEACH[c]) * t) for c in range(3)) + (255,))
    canvas.paste(grad, (0, 0), shape)
    # Glossy highlight: a soft crescent along the top edge of every letter.
    inner = erode(shape, 12 * SS)
    below = Image.new("L", (width, height), 0)
    below.paste(inner, (0, 26 * SS))
    gloss = ImageChops.subtract(inner, below).filter(ImageFilter.GaussianBlur(4 * SS))
    canvas.paste((255, 255, 255, 255), (0, 0), gloss.point(lambda v: int(v * 0.55)))
    canvas = canvas.crop(canvas.getbbox())
    return canvas.resize((canvas.width // SS, canvas.height // SS), Image.LANCZOS)


def dilate(mask, radius):
    """Fast approximate dilation: blur, then keep anything the blur reached."""
    return mask.filter(ImageFilter.GaussianBlur(radius / 2)).point(lambda v: 255 if v > 6 else int(v * 42))


def erode(mask, radius):
    return mask.filter(ImageFilter.GaussianBlur(radius / 2)).point(lambda v: 255 if v > 249 else 0)


def icon(draw_fn, px=128):
    big = px * SS
    img = Image.new("L", (big, big), 0)
    draw_fn(ImageDraw.Draw(img), big)
    img = img.resize((px, px), Image.LANCZOS)
    out = Image.new("RGBA", (px, px), (255, 255, 255, 0))
    out.putalpha(img)
    return out


def rounded_poly(d, pts, width):
    d.line(pts + [pts[0], pts[1]], fill=255, width=width, joint="curve")
    for p in pts:
        d.ellipse([p[0] - width / 2, p[1] - width / 2, p[0] + width / 2, p[1] + width / 2], fill=255)


def home(d, s):
    w = int(s * 0.085)
    roof = [(s * .14, s * .50), (s * .50, s * .18), (s * .86, s * .50)]
    d.line(roof, fill=255, width=w, joint="curve")
    for p in (roof[0], roof[2]):
        d.ellipse([p[0] - w / 2, p[1] - w / 2, p[0] + w / 2, p[1] + w / 2], fill=255)
    d.rounded_rectangle([s * .24, s * .42, s * .76, s * .84], radius=s * .12, outline=255, width=w)
    d.rounded_rectangle([s * .43, s * .60, s * .57, s * .84], radius=s * .07, fill=255)


def box(d, s):
    w = int(s * 0.085)
    d.rounded_rectangle([s * .18, s * .40, s * .82, s * .84], radius=s * .10, outline=255, width=w)
    d.rounded_rectangle([s * .12, s * .28, s * .88, s * .44], radius=s * .07, fill=255)
    d.rectangle([s * .46, s * .30, s * .54, s * .84], fill=255)
    # A little bow.
    d.ellipse([s * .30, s * .10, s * .50, s * .30], outline=255, width=w)
    d.ellipse([s * .50, s * .10, s * .70, s * .30], outline=255, width=w)


def you(d, s):
    w = int(s * 0.085)
    d.ellipse([s * .33, s * .12, s * .67, s * .46], outline=255, width=w)
    d.rounded_rectangle([s * .18, s * .56, s * .82, s * 1.12], radius=s * .28, outline=255, width=w)
    d.rectangle([0, s * .86, s, s], fill=0)
    for x in (s * .18 + w / 2, s * .82 - w / 2):
        d.ellipse([x - w / 2, s * .86 - w / 2, x + w / 2, s * .86 + w / 2], fill=255)


def settings(d, s):
    cx = cy = s / 2
    teeth = 8
    for i in range(teeth):
        a = 2 * math.pi * i / teeth
        x, y = cx + math.cos(a) * s * .33, cy + math.sin(a) * s * .33
        d.ellipse([x - s * .10, y - s * .10, x + s * .10, y + s * .10], fill=255)
    d.ellipse([cx - s * .32, cy - s * .32, cx + s * .32, cy + s * .32], fill=255)
    d.ellipse([cx - s * .12, cy - s * .12, cx + s * .12, cy + s * .12], fill=0)


def back(d, s):
    w = int(s * 0.10)
    pts = [(s * .62, s * .20), (s * .32, s * .50), (s * .62, s * .80)]
    d.line(pts, fill=255, width=w, joint="curve")
    for p in (pts[0], pts[2]):
        d.ellipse([p[0] - w / 2, p[1] - w / 2, p[0] + w / 2, p[1] + w / 2], fill=255)


def main():
    font = Path(sys.argv[1]) if len(sys.argv) > 1 else OUT / "Fonts/Fredoka-SemiBold.ttf"
    (OUT / "Icons").mkdir(parents=True, exist_ok=True)
    wordmark(font).save(OUT / "PockleWordmark.png", optimize=True)
    for name, fn in [("Home", home), ("Box", box), ("You", you), ("Settings", settings), ("Back", back)]:
        icon(fn).save(OUT / "Icons" / (name + ".png"), optimize=True)
    print("Wrote wordmark and icons to", OUT.relative_to(ROOT))


if __name__ == "__main__":
    main()
