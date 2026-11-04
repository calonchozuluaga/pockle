"""Re-export home-page images from the ArtSource/Website masters (owner's local folder; the masters are not in this branch).
Run from the pockle folder: python3 website/home/build_assets.py, then python3 website/build.py."""
from PIL import Image
import os
src, out = 'ArtSource/Website', 'website/assets/home'
jobs = [('H1/desktop.png','hero-desktop',2400),('H1/mobile.png','hero-mobile',1080),
 ('S1/walk.png','walk',1000),('S2/open-common.png','open',1000),('S3/play.png','play',1000),
 ('M1/jelly-macro.png','mat-jelly',800),('M2/plush-macro.png','mat-plush',800),('M3/vinyl-macro-v2.png','mat-vinyl',800),('M4/glitter-macro.png','mat-glitter',800),
 ('SE1/desktop.png','series-desktop',2400),('SE1/mobile.png','series-mobile',1080),
 ('SE2/common.png','tier-common',700),('SE2/rare.png','tier-rare',700),('SE2/epic.png','tier-epic',700),('SE2/secret-v2.png','tier-secret',700),
 ('SE3/finite-edition.png','edition',1200),('F1/desktop.png','fusion-desktop',2000),('F1/mobile.png','fusion-mobile',1080),
 ('T1/desktop.png','trade-desktop',2000),('T1/mobile.png','trade-mobile',1080),('W1/desktop.png','walk-desktop',2000),('W1/mobile.png','walk-mobile',1080),
 ('D1/desktop.png','closing-desktop',2400),('D1/mobile.png','closing-mobile',1080)]
for i, n in enumerate(['pip','fox','axolotl','firb','coco','snowy','ginger','reiny','guin','bundy','wally'], 1):
    jobs.append((f'C{i}/{n}.png', f'col-{n}', 900))
os.makedirs(out, exist_ok=True)
for s, n, w in jobs:
    im = Image.open(os.path.join(src, s)); im.load()
    if im.width > w: im = im.resize((w, round(im.height * w / im.width)), Image.LANCZOS)
    im.save(f'{out}/{n}.webp', 'WEBP', quality=82, method=6)
print('exported', len(jobs))
