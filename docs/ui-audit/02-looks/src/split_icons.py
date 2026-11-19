import sys
sys.path.append('<your site-packages, when run with python3 -I>')
from PIL import Image, ImageFilter
src, out, cut = sys.argv[1], sys.argv[2], int(sys.argv[3])
names = sys.argv[4].split(',')
im = Image.open(src).convert('RGBA'); W, H = im.size
a = im.getchannel('A')
hist = a.histogram(); total = W * H
print('alpha: 0=%.1f%% 1-39=%.1f%% 40-119=%.1f%% 120-254=%.1f%% 255=%.1f%%' % tuple(100.0 * x / total for x in (hist[0], sum(hist[1:40]), sum(hist[40:120]), sum(hist[120:255]), hist[255])))
# Drop the faint speckled halo, keep the icon's own soft edge: a blurred solid mask decides what belongs to the icon.
solid = a.point(lambda v: 255 if v >= cut else 0).filter(ImageFilter.MedianFilter(5))
keep = solid.filter(ImageFilter.MaxFilter(5)).filter(ImageFilter.GaussianBlur(1.6))
from PIL import ImageChops
a2 = ImageChops.multiply(a, keep)
im.putalpha(a2)
mask = im.getchannel('A').point(lambda v: 255 if v > 24 else 0)
def runs(profile, least=40):
    out, start = [], None
    for i, on in enumerate(profile + [False]):
        if on and start is None: start = i
        if not on and start is not None:
            if i - start >= least: out.append((start, i))
            start = None
    merged = []
    for run in out:  # parts of one icon (a loudspeaker and its sound waves) sit closer together than two icons do
        if merged and run[0] - merged[-1][1] <= 40: merged[-1] = (merged[-1][0], run[1])
        else: merged.append(run)
    return merged
row_on = [mask.crop((0, y, W, y + 1)).getbbox() is not None for y in range(H)]
bands = runs(row_on); print('rows', bands)
boxes = []
for top, bottom in bands:
    band = mask.crop((0, top, W, bottom))
    col_on = [band.crop((x, 0, x + 1, bottom - top)).getbbox() is not None for x in range(W)]
    for left, right in runs(col_on):
        bb = mask.crop((left, top, right, bottom)).getbbox()
        boxes.append((left + bb[0], top + bb[1], left + bb[2], top + bb[3]))
assert len(boxes) == len(names), boxes
for name, bb in zip(names, boxes):
    icon = im.crop(bb); side = max(icon.size) + 16
    sq = Image.new('RGBA', (side, side), (0, 0, 0, 0)); sq.paste(icon, ((side - icon.width) // 2, (side - icon.height) // 2))
    sq = sq.resize((256, 256), Image.LANCZOS); sq.save('%s/%s.png' % (out, name), optimize=True)
    print(name, bb, icon.size)
# contact sheet on a mid tone and on white to judge the edges
sheet = Image.new('RGB', (9 * 150, 320), (236, 214, 208))
for i, name in enumerate(names):
    ic = Image.open('%s/%s.png' % (out, name)).resize((140, 140), Image.LANCZOS)
    sheet.paste(ic, (5 + i * 150, 10), ic)
    bg = Image.new('RGB', (140, 140), (90, 60, 100)); bg.paste(ic, (0, 0), ic); sheet.paste(bg, (5 + i * 150, 170))
sheet.save(out + '/_check.jpg', quality=88)
