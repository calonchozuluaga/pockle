"""Look A2, Jelly glass: look A with calmer colour and clear glass. The app's paintings show through thin glass
panels; the one strong colour left is a deep rose jelly button. Small jelly icons carry the rest of the colour."""
from common import *
import common

ROSE_TOP, ROSE_BOTTOM = 'rgba(170, 78, 120, .9)', 'rgba(118, 40, 84, .94)'
SHADOW = '0 12px 28px rgba(74, 34, 58, .2), 0 2px 6px rgba(74, 34, 58, .1)'
RIM = 'inset 0 1.5px 1px rgba(255, 255, 255, .95), inset 0 -1px 1px rgba(255, 255, 255, .45), inset 0 0 14px rgba(255, 255, 255, .26)'
# Clear glass: for controls. The painting behind stays readable through it.
CLEAR = ('background: linear-gradient(135deg, rgba(255, 255, 255, .36), rgba(255, 255, 255, .12) 58%%, rgba(255, 255, 255, .22)); border: 1px solid rgba(255, 255, 255, .55); '
         'backdrop-filter: blur(9px) saturate(1.5) brightness(1.05); -webkit-backdrop-filter: blur(9px) saturate(1.5) brightness(1.05); box-shadow: %s, %s;' % (RIM, SHADOW))
# Frosted glass: for panels that carry words.
FROST = ('background: linear-gradient(135deg, rgba(255, 255, 255, .66), rgba(255, 255, 255, .4)); border: 1px solid rgba(255, 255, 255, .7); '
         'backdrop-filter: blur(22px) saturate(1.4); -webkit-backdrop-filter: blur(22px) saturate(1.4); box-shadow: %s, %s;' % (RIM, SHADOW))
HALO = 'text-shadow: 0 1px 10px rgba(255, 255, 255, .95), 0 0 2px rgba(255, 255, 255, .8);'
TINT = {'butter': 'rgba(236, 214, 150, .78)', 'sky': 'rgba(166, 204, 232, .8)', 'plum': 'rgba(92, 62, 104, .86)', 'sage': 'rgba(170, 208, 186, .8)'}

def sheen(radius='inherit'):
    return ('<div aria-hidden="true" style="position: absolute; inset: 0; border-radius: %s; pointer-events: none; '
            'background: linear-gradient(118deg, rgba(255, 255, 255, .5), rgba(255, 255, 255, 0) 26%%, rgba(255, 255, 255, 0) 74%%, rgba(255, 255, 255, .26))"></div>' % radius)

def art(name, size, fallback):
    """A generated jelly icon when it exists, else the drawn glyph."""
    if name in common.ASSETS:
        return img(name, 'position: relative; width: %dpx; height: %dpx; object-fit: contain; filter: drop-shadow(0 3px 4px rgba(74, 34, 58, .2))' % (size, size))
    return icon(fallback, round(size * .6), PLUM, 2.3, style='position: relative')

def bead(inner, size=62):
    return ('<div style="position: relative; width: %dpx; height: %dpx; border-radius: 50%%; display: flex; align-items: center; justify-content: center; box-sizing: border-box; %s">%s%s</div>'
            % (size, size, CLEAR, sheen('50%'), inner))

def glyph_bead(name, size=44, colour=PLUM, glyph=22, fill='none'):
    return bead(icon(name, glyph, colour, 2.3, fill, 'position: relative'), size)

def rose_button(label, name, height=58, size=19):
    return ('<button type="button" style="position: relative; width: 100%%; height: %dpx; border-radius: %dpx; box-sizing: border-box; display: flex; align-items: center; justify-content: center; gap: 10px; '
            'background: linear-gradient(%s, %s); border: 1px solid rgba(255, 255, 255, .4); color: #fff; font: 800 %dpx/1 %s; letter-spacing: .01em; '
            'backdrop-filter: blur(10px) saturate(1.4); -webkit-backdrop-filter: blur(10px) saturate(1.4); '
            'box-shadow: inset 0 1.5px 1px rgba(255, 255, 255, .6), inset 0 -8px 14px rgba(70, 10, 44, .34), inset 0 0 12px rgba(255, 196, 220, .22), 0 12px 24px rgba(110, 36, 76, .3)">'
            '<div aria-hidden="true" style="position: absolute; inset: 4px 10%% auto 10%%; height: 40%%; border-radius: 999px; background: linear-gradient(rgba(255, 255, 255, .36), rgba(255, 255, 255, 0))"></div>'
            '%s<span style="position: relative">%s</span></button>' % (height, height // 2, ROSE_TOP, ROSE_BOTTOM, size, F, icon(name, 23, '#fff', 2.3, style='position: relative'), label))

def dock(selected):
    items = ''
    for name, label in TABS:
        on = name == selected
        lens = ('position: absolute; left: 4px; right: 4px; top: 5px; bottom: 5px; border-radius: 26px; background: linear-gradient(135deg, rgba(255, 255, 255, .86), rgba(255, 255, 255, .5)); '
                'box-shadow: inset 0 1.5px 1px #fff, inset 0 0 10px rgba(255, 255, 255, .5), 0 5px 12px rgba(74, 34, 58, .16)')
        items += ('<button type="button" style="position: relative; flex: 1; min-width: 0; height: 66px; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 4px">'
                  '%s%s<span style="position: relative; font: %s 11px/1 %s; color: %s">%s</span></button>'
                  % ('<span aria-hidden="true" style="%s"></span>' % lens if on else '', icon(name, 23, PLUM if on else SOFT, 2.3, style='position: relative'), '800' if on else '700', F, PLUM if on else SOFT, label))
    return ('<nav aria-label="Main" style="position: absolute; left: 14px; right: 14px; bottom: 22px; height: 66px; border-radius: 33px; display: flex; align-items: center; padding: 0 3px; box-sizing: border-box; %s">%s%s</nav>'
            % (FROST.replace('blur(22px)', 'blur(16px)'), sheen('33px'), items))

def root(inner, background):
    return ('<div style="position: relative; width: 390px; height: 844px; overflow: hidden; background: %s; font-family: %s; color: %s">%s</div>' % (background, F, PLUM, inner))

def glass_plank():
    return ('<div aria-hidden="true" style="position: relative; height: 14px; margin-top: -5px; border-radius: 7px; background: linear-gradient(rgba(255, 255, 255, .7), rgba(255, 255, 255, .2)); '
            'border: 1px solid rgba(255, 255, 255, .8); backdrop-filter: blur(6px) brightness(1.06); -webkit-backdrop-filter: blur(6px) brightness(1.06); '
            'box-shadow: inset 0 1.5px 0 #fff, 0 12px 18px rgba(74, 34, 58, .17)"></div>')

def home(scene='scene-room', veil='rgba(251, 244, 240, .16), rgba(251, 244, 240, .34)', ground='#F4E3DC', name='Home, Jelly glass look'):
    head = ('<div style="position: absolute; left: 20px; right: 20px; top: 52px; height: 44px; display: flex; align-items: center; justify-content: space-between">'
            '%s<button type="button" aria-label="Your profile" style="width: 44px; height: 44px; border-radius: 50%%; overflow: hidden; border: 2.5px solid rgba(255, 255, 255, .9); box-sizing: border-box; box-shadow: %s">%s</button></div>'
            % (img('wordmark', 'height: 38px; width: auto; display: block', 'Pockle'), SHADOW, img('avatar-fox', 'width: 100%; height: 100%; object-fit: cover; display: block')))
    hero = ('<section style="position: absolute; left: 16px; top: 108px; width: 358px; height: 312px; border-radius: 36px; box-sizing: border-box; %s">%s'
            '<div aria-hidden="true" style="position: absolute; right: -14px; top: 6px; width: 226px; height: 226px; border-radius: 50%%; background: radial-gradient(circle, rgba(255, 255, 255, .8), rgba(255, 255, 255, 0) 68%%)"></div>'
            '%s'
            '<div style="position: absolute; left: 22px; top: 22px; width: 152px; display: flex; flex-direction: column; align-items: flex-start; gap: 10px">'
            '<span style="position: relative; display: inline-flex; align-items: center; gap: 6px; padding: 6px 11px 6px 8px; border-radius: 999px; font: 800 12px/1 %s; letter-spacing: .05em; text-transform: uppercase; color: %s; %s">'
            '<span aria-hidden="true" style="width: 8px; height: 8px; border-radius: 50%%; background: #5FA584; box-shadow: 0 0 0 3px rgba(95, 165, 132, .22)"></span>1 box ready</span>'
            '<h1 style="margin: 0; font: 800 30px/1.02 %s; letter-spacing: -.01em">Wally box</h1>'
            '<p style="margin: 0; font: 500 15px/1.3 %s; color: %s">Someone new is waiting inside.</p></div>'
            '<div style="position: absolute; left: 18px; right: 18px; bottom: 18px">%s</div></section>'
            % (FROST, sheen('36px'), img('box-wally', 'position: absolute; right: 8px; top: 26px; width: 172px; height: auto; filter: drop-shadow(0 16px 14px rgba(60, 70, 120, .26))', 'Wally collection box'),
               F, PLUM, CLEAR, F, F, SOFT, rose_button('Open', 'box')))
    beads = ''.join('<span style="width: 12px; height: 12px; border-radius: 50%%; box-sizing: border-box; %s"></span>' % (
        'background: linear-gradient(%s, %s); box-shadow: inset 0 1px 0 rgba(255, 255, 255, .6), 0 2px 5px rgba(110, 36, 76, .3)' % (ROSE_TOP, ROSE_BOTTOM) if i == 0
        else 'background: rgba(255, 255, 255, .45); border: 1px solid rgba(255, 255, 255, .8); box-shadow: inset 0 1px 3px rgba(74, 34, 58, .18)') for i in range(4))
    nextbox = ('<section style="position: absolute; left: 16px; top: 434px; width: 358px; height: 66px; border-radius: 33px; box-sizing: border-box; display: flex; align-items: center; gap: 12px; padding: 0 18px 0 10px; %s">%s'
               '<div style="position: relative; width: 46px; height: 46px; flex: none">%s<div style="position: absolute; inset: 0; display: flex; align-items: center; justify-content: center">%s</div></div>'
               '<div style="position: relative; flex: 1; min-width: 0"><div style="font: 800 16px/1.15 %s">680 steps to go</div><div style="font: 500 13px/1.3 %s; color: %s">Next box · 320 of 1,000</div></div>'
               '<div aria-label="1 of 4 boxes today" style="position: relative; display: flex; gap: 5px">%s</div></section>'
               % (FROST, sheen('33px'), ring(46, .32, 'rgba(74, 34, 58, .12)', '#8E3C68', 5), icon('walk', 19, '#6E2C52', 2.3), F, F, SOFT, beads))
    arts = ['ic-play', 'ic-fuse', 'ic-shop', 'ic-badge']
    shortcuts = ''.join('<button type="button" style="width: 76px; display: flex; flex-direction: column; align-items: center; gap: 7px">%s<span style="font: 700 13.5px/1 %s; %s">%s</span></button>'
                        % (bead(art(arts[i], 44, name), 62), F, HALO, label) for i, (name, label) in enumerate(SHORTCUTS))
    shortcuts = '<div style="position: absolute; left: 20px; right: 20px; top: 518px; display: flex; justify-content: space-between">%s</div>' % shortcuts
    toys = ''.join(img(t, 'width: 84px; height: 84px; object-fit: contain; filter: drop-shadow(0 6px 6px rgba(74, 34, 58, .2))') for t in PEEK)
    peek = ('<section style="position: absolute; left: 20px; right: 20px; top: 624px">'
            '<div style="display: flex; align-items: baseline; justify-content: space-between"><h2 style="margin: 0; font: 800 20px/1 %s; %s">Your shelf</h2><a href="#" style="font: 700 14px/1 %s; color: #7A2A54; text-decoration: none; %s">See all</a></div>'
            '<div style="display: flex; justify-content: space-around; align-items: flex-end; margin-top: 4px; padding: 0 10px">%s</div>%s</section>' % (F, HALO, F, HALO, toys, glass_plank()))
    body = (img(scene, 'position: absolute; left: 0; top: 0; width: 390px; height: 844px; object-fit: cover; filter: saturate(.82)')
            + '<div aria-hidden="true" style="position: absolute; left: 0; top: 0; width: 390px; height: 844px; background: linear-gradient(%s)"></div>' % veil
            + head + hero + nextbox + shortcuts + peek + dock('home'))
    return page(name, root(body, ground))

def home_dusk():
    return home('scene-dusk', 'rgba(244, 240, 251, .1), rgba(244, 240, 251, .3)', '#E4DDF2', 'Home at dusk, Jelly glass look')

MARK = {'rare': ('butter', 'star', 'Rare', PLUM), 'epic': ('sky', 'gem', 'Epic', PLUM), 'secret': ('plum', 'sparkle', 'Secret', '#fff')}

def shelf():
    head = ('<div style="position: absolute; left: 20px; right: 20px; top: 52px; height: 44px; display: flex; align-items: center; gap: 10px">'
            '<h1 style="margin: 0; font: 800 32px/1 %s; letter-spacing: -.01em; %s">Shelf</h1>'
            '<span style="position: relative; padding: 7px 11px; border-radius: 999px; font: 700 13px/1 %s; color: %s; %s">53 toys</span>'
            '<span style="flex: 1"></span><button type="button" aria-label="Search">%s</button></div>' % (F, HALO, F, PLUM, FROST, glyph_bead('search')))
    segs = ''
    for i, label in enumerate(['All', 'Fusions', 'Extras']):
        on = i == 0
        segs += ('<button type="button" style="position: relative; flex: 1; height: 36px; border-radius: 18px; font: %s 14.5px/1 %s; color: %s; %s">%s</button>'
                 % ('800' if on else '700', F, PLUM if on else SOFT,
                    'background: linear-gradient(135deg, rgba(255, 255, 255, .92), rgba(255, 255, 255, .6)); box-shadow: inset 0 1.5px 1px #fff, 0 5px 12px rgba(74, 34, 58, .16)' if on else '', label))
    switch = ('<div role="group" aria-label="Show" style="position: absolute; left: 20px; right: 20px; top: 108px; height: 46px; border-radius: 23px; padding: 4px; box-sizing: border-box; display: flex; gap: 4px; %s">%s%s</div>'
              % (FROST.replace('blur(22px)', 'blur(14px)'), sheen('23px'), segs))
    rows = ''
    for row in SHELF:
        toys = labels = ''
        for name, pic, rarity, copies in row:
            extra = ('<span style="position: absolute; right: 2px; top: 0; min-width: 26px; height: 26px; padding: 0 6px; box-sizing: border-box; border-radius: 13px; display: flex; align-items: center; justify-content: center; '
                     'font: 800 12.5px/1 %s; color: %s; %s">×%d</span>' % (F, PLUM, FROST.replace('blur(22px)', 'blur(8px)'), copies)) if copies > 1 else ''
            mark = ''
            if rarity:
                tint, glyph, word, colour = MARK[rarity]
                mark = ('<span role="img" aria-label="%s" style="position: absolute; left: 2px; bottom: 8px; width: 26px; height: 26px; border-radius: 50%%; box-sizing: border-box; display: flex; align-items: center; justify-content: center; '
                        'background: %s; border: 1px solid rgba(255, 255, 255, .75); box-shadow: inset 0 1.5px 1px rgba(255, 255, 255, .8), 0 4px 8px rgba(74, 34, 58, .22)">%s</span>' % (word, TINT[tint], icon(glyph, 14, colour, 2.6)))
            reflection = img(pic, 'position: absolute; left: 4px; top: 100%; width: 96px; height: 96px; object-fit: contain; transform: scaleY(-1); opacity: .2; '
                                  '-webkit-mask-image: linear-gradient(to top, #000, rgba(0, 0, 0, 0) 26%); mask-image: linear-gradient(to top, #000, rgba(0, 0, 0, 0) 26%)')
            toys += ('<div style="position: relative; width: 104px; height: 100px; display: flex; align-items: flex-end; justify-content: center">%s%s%s%s</div>'
                     % (reflection, img(pic, 'position: relative; width: 96px; height: 96px; object-fit: contain; filter: drop-shadow(0 6px 5px rgba(74, 34, 58, .18))', name), extra, mark))
            labels += '<span style="width: 104px; font: 700 12.5px/1.15 %s; text-align: center; %s">%s</span>' % (F, HALO, name)
        rows += ('<div style="display: flex; flex-direction: column"><div style="display: flex; justify-content: space-between; padding: 0 6px">%s</div>%s'
                 '<div style="display: flex; justify-content: space-between; padding: 10px 6px 0">%s</div></div>' % (toys, glass_plank(), labels))
    grid = '<div style="position: absolute; left: 20px; right: 20px; top: 170px; display: flex; flex-direction: column; gap: 15px">%s</div>' % rows
    body = (img('scene-room', 'position: absolute; left: 0; top: 0; width: 390px; height: 844px; object-fit: cover; filter: saturate(.8)')
            + '<div aria-hidden="true" style="position: absolute; left: 0; top: 0; width: 390px; height: 844px; background: linear-gradient(rgba(251, 244, 240, .3), rgba(251, 244, 240, .5))"></div>'
            + head + switch + grid + dock('shelf'))
    return page('Shelf, Jelly glass look', root(body, '#F4E3DC'))

def play():
    top = ('<div style="position: absolute; left: 16px; right: 16px; top: 52px; height: 44px; display: flex; align-items: center; justify-content: space-between">'
           '<button type="button" aria-label="Back">%s</button>'
           '<button type="button" style="position: relative; height: 40px; padding: 0 16px 0 12px; border-radius: 20px; display: flex; align-items: center; gap: 8px; font: 700 14.5px/1 %s; %s">%s%s<span style="position: relative">Pip · 9 of 15</span></button>'
           '<button type="button" aria-label="Favorite">%s</button></div>'
           % (glyph_bead('back'), F, FROST.replace('blur(22px)', 'blur(14px)'), sheen('20px'), ring(20, .6, 'rgba(42, 20, 48, .16)', '#8E3C68', 4, 'position: relative;'), glyph_bead('heart', 44, '#8E3C68', 22, '#B8587F')))
    chips = ''.join('<span style="position: relative; padding: 7px 12px; border-radius: 999px; font: 700 13.5px/1 %s; %s">%s</span>' % (F, FROST.replace('blur(22px)', 'blur(10px)'), w) for w in ['Sunny', 'Curious'])
    title = ('<div style="position: absolute; left: 20px; top: 112px"><h1 style="margin: 0; font: 800 36px/1 %s; letter-spacing: -.015em; %s">Peach Jelly</h1>'
             '<div style="display: flex; gap: 6px; margin-top: 10px">%s</div></div>' % (F, HALO, chips))
    toy = ('<div aria-hidden="true" style="position: absolute; left: 75px; top: 588px; width: 240px; height: 34px; border-radius: 50%; background: radial-gradient(ellipse, rgba(120, 50, 80, .32), rgba(120, 50, 80, 0) 70%)"></div>'
           + img('hero-peach', 'position: absolute; left: 45px; top: 312px; width: 300px; height: auto', 'Peach Jelly'))
    bubble = ('<div style="position: absolute; left: 236px; top: 262px; padding: 11px 18px; border-radius: 24px 24px 24px 6px; font: 800 20px/1 %s; %s">yay!</div>' % (F, FROST.replace('blur(22px)', 'blur(12px)')))
    arts = ['ic-pockles', 'ic-backdrop', 'ic-light', 'ic-about', 'ic-sound']
    controls = ''.join('<button type="button" style="position: relative; flex: 1; min-width: 0; display: flex; flex-direction: column; align-items: center; gap: 6px">%s<span style="font: 700 12.5px/1 %s">%s</span></button>'
                       % (bead(art(arts[i], 36, name), 50), F, label) for i, (name, label) in enumerate(CONTROLS))
    tray = ('<div role="group" aria-label="Play controls" style="position: absolute; left: 12px; right: 12px; bottom: 26px; height: 96px; border-radius: 34px; box-sizing: border-box; padding: 0 6px; display: flex; align-items: center; %s">%s%s</div>'
            % (FROST.replace('blur(22px)', 'blur(18px)'), sheen('34px'), controls))
    hint = ('<div style="position: absolute; left: 0; right: 0; bottom: 136px; display: flex; justify-content: center"><span style="position: relative; padding: 8px 14px; border-radius: 999px; font: 700 13px/1 %s; %s">Tap · drag to turn</span></div>'
            % (F, FROST.replace('blur(22px)', 'blur(10px)')))
    body = img('world-jelly-garden', 'position: absolute; left: 0; top: 0; width: 390px; height: 844px; object-fit: cover; filter: saturate(.9)') + top + title + toy + bubble + hint + tray
    return page('Play, Jelly glass look', root(body, '#F1CFD2'))

BOARDS = [('A2-Home.dc.html', 'A2 · Jelly glass · Home', home), ('A2-Shelf.dc.html', 'A2 · Jelly glass · Shelf', shelf), ('A2-Play.dc.html', 'A2 · Jelly glass · Play', play),
          ('A2-Home-Dusk.dc.html', 'A2 · Jelly glass · Home at dusk', home_dusk)]
