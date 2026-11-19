"""Look A, Jelly: the app is made of the same stuff as the toys. Milky glass cards, candy buttons, soft light."""
from common import *

BERRY_TOP, BERRY_BOTTOM = '#E0246F', '#A8104C'
GLASS = ('background: rgba(255, 255, 255, .5); border: 1.5px solid rgba(255, 255, 255, .92); backdrop-filter: blur(16px); -webkit-backdrop-filter: blur(16px); '
         'box-shadow: 0 16px 36px rgba(196, 72, 128, .2), inset 0 1px 0 #fff;')
CANDY = {'pink': ('#FF8DB6', '#EC4B8B', 'rgba(224, 60, 126, .42)'), 'lilac': ('#C3B2FF', '#8E76EE', 'rgba(126, 100, 230, .4)'),
         'mint': ('#8FE9C6', '#3CBF93', 'rgba(40, 170, 126, .38)'), 'gold': ('#FFD983', '#F0A929', 'rgba(226, 150, 20, .4)'),
         'sky': ('#9FD8FF', '#4AA6EE', 'rgba(60, 140, 226, .4)')}

def gloss(inset='5px 14% auto 14%', height='42%'):
    return ('<div aria-hidden="true" style="position: absolute; inset: %s; height: %s; border-radius: 999px; '
            'background: linear-gradient(rgba(255, 255, 255, .62), rgba(255, 255, 255, .04)); pointer-events: none"></div>' % (inset, height))

def gumdrop(name, colour, size=62, glyph=28):
    top, bottom, glow = CANDY[colour]
    return ('<div style="position: relative; width: %dpx; height: %dpx; border-radius: 50%%; display: flex; align-items: center; justify-content: center; '
            'background: radial-gradient(circle at 50%% 30%%, %s, %s 78%%); box-shadow: 0 10px 18px %s, inset 0 -6px 10px rgba(0, 0, 0, .1), inset 0 2px 0 rgba(255, 255, 255, .7)">'
            '%s%s</div>' % (size, size, top, bottom, glow, gloss('4px 18% auto 18%', '38%'), icon(name, glyph, '#fff', 2.4, style='filter: drop-shadow(0 1px 1px rgba(0, 0, 0, .18))')))

def glass_round(name, size=44, colour=PLUM, glyph=22, fill='none'):
    return ('<div style="width: %dpx; height: %dpx; border-radius: 50%%; display: flex; align-items: center; justify-content: center; %s">%s</div>'
            % (size, size, GLASS, icon(name, glyph, colour, 2.4, fill)))

def berry_button(label, name, height=60, size=20):
    return ('<button type="button" style="position: relative; width: 100%%; height: %dpx; border-radius: %dpx; display: flex; align-items: center; justify-content: center; gap: 10px; '
            'background: linear-gradient(%s, %s); color: #fff; font: 800 %dpx/1 %s; letter-spacing: .01em; '
            'box-shadow: 0 12px 22px rgba(190, 20, 90, .38), inset 0 -7px 12px rgba(90, 0, 40, .32), inset 0 2px 0 rgba(255, 255, 255, .5)">%s%s<span>%s</span></button>'
            % (height, height // 2, BERRY_TOP, BERRY_BOTTOM, size, F, gloss('5px 12% auto 12%', '40%'), icon(name, 24, '#fff', 2.4), label))

def dock(selected):
    items = ''
    for name, label in TABS:
        on = name == selected
        chip = ('position: relative; width: 54px; height: 34px; border-radius: 17px; display: flex; align-items: center; justify-content: center; '
                + ('background: linear-gradient(#FF8DB6, #EC4B8B); box-shadow: 0 6px 12px rgba(224, 60, 126, .4), inset 0 -4px 8px rgba(120, 0, 60, .2), inset 0 1.5px 0 rgba(255, 255, 255, .7)' if on else ''))
        items += ('<button type="button" style="flex: 1; min-width: 0; height: 58px; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 3px">'
                  '<span style="%s">%s%s</span><span style="font: %s 11px/1 %s; color: %s">%s</span></button>'
                  % (chip, gloss('3px 16% auto 16%', '40%') if on else '', icon(name, 23, '#fff' if on else SOFT, 2.3), '800' if on else '700', F, PLUM if on else SOFT, label))
    return ('<nav aria-label="Main" style="position: absolute; left: 12px; right: 12px; bottom: 22px; height: 70px; border-radius: 35px; display: flex; align-items: center; padding: 0 6px; box-sizing: border-box; '
            'background: rgba(255, 255, 255, .72); border: 1.5px solid #fff; backdrop-filter: blur(18px); -webkit-backdrop-filter: blur(18px); '
            'box-shadow: 0 18px 34px rgba(160, 60, 110, .26), inset 0 1px 0 #fff">%s</nav>' % items)

def root(inner, background):
    return ('<div style="position: relative; width: 390px; height: 844px; overflow: hidden; background: %s; font-family: %s; color: %s">%s</div>' % (background, F, PLUM, inner))

def home():
    head = ('<div style="position: absolute; left: 20px; right: 20px; top: 52px; height: 44px; display: flex; align-items: center; justify-content: space-between">'
            '%s<button type="button" aria-label="Your profile" style="width: 44px; height: 44px; border-radius: 50%%; overflow: hidden; border: 3px solid #fff; box-sizing: border-box; box-shadow: 0 8px 16px rgba(196, 72, 128, .3)">%s</button></div>'
            % (img('wordmark', 'height: 38px; width: auto; display: block', 'Pockle'), img('avatar-fox', 'width: 100%; height: 100%; object-fit: cover; display: block')))
    hero = ('<section style="position: absolute; left: 16px; top: 108px; width: 358px; height: 316px; border-radius: 36px; box-sizing: border-box; %s">'
            '<div aria-hidden="true" style="position: absolute; right: -20px; top: 4px; width: 230px; height: 230px; border-radius: 50%%; background: radial-gradient(circle, rgba(255, 255, 255, .95), rgba(255, 255, 255, 0) 68%%)"></div>'
            '%s'
            '<div style="position: absolute; left: 22px; top: 22px; width: 156px; display: flex; flex-direction: column; align-items: flex-start; gap: 10px">'
            '<span style="position: relative; padding: 7px 12px; border-radius: 999px; font: 800 12px/1 %s; letter-spacing: .05em; text-transform: uppercase; color: #0B4634; '
            'background: linear-gradient(#B9F5DC, #7BDDB4); box-shadow: 0 5px 10px rgba(40, 170, 126, .3), inset 0 1.5px 0 rgba(255, 255, 255, .8)">1 box ready</span>'
            '<h1 style="margin: 0; font: 800 30px/1.02 %s; letter-spacing: -.01em">Wally box</h1>'
            '<p style="margin: 0; font: 500 15px/1.3 %s; color: %s">Someone new is waiting inside.</p></div>'
            '<div style="position: absolute; left: 18px; right: 18px; bottom: 18px">%s</div></section>'
            % (GLASS, img('box-wally', 'position: absolute; right: 6px; top: 26px; width: 176px; height: auto; filter: drop-shadow(0 16px 14px rgba(60, 90, 160, .28))', 'Wally collection box'),
               F, F, F, SOFT, berry_button('Open', 'box')))
    beads = ''.join('<span style="width: 13px; height: 13px; border-radius: 50%%; %s"></span>' % (
        'background: radial-gradient(circle at 50% 30%, #FF8DB6, #EC4B8B); box-shadow: 0 3px 6px rgba(224, 60, 126, .4), inset 0 1px 0 rgba(255, 255, 255, .7)' if i == 0
        else 'background: rgba(255, 255, 255, .7); box-shadow: inset 0 1.5px 3px rgba(150, 60, 100, .25)') for i in range(4))
    nextbox = ('<section style="position: absolute; left: 16px; top: 438px; width: 358px; height: 68px; border-radius: 34px; box-sizing: border-box; display: flex; align-items: center; gap: 12px; padding: 0 18px 0 11px; %s">'
               '<div style="position: relative; width: 46px; height: 46px; flex: none">%s<div style="position: absolute; inset: 0; display: flex; align-items: center; justify-content: center">%s</div></div>'
               '<div style="flex: 1; min-width: 0"><div style="font: 800 16px/1.15 %s">680 steps to go</div><div style="font: 500 13px/1.3 %s; color: %s">Next box · 320 of 1,000</div></div>'
               '<div aria-label="1 of 4 boxes today" style="display: flex; gap: 5px">%s</div></section>'
               % (GLASS, ring(46, .32, 'rgba(255, 255, 255, .9)', '#EC4B8B', 6), icon('walk', 20, '#C2306E', 2.3), F, F, SOFT, beads))
    colours = ['pink', 'lilac', 'mint', 'gold']
    shortcuts = ''.join('<button type="button" style="width: 76px; display: flex; flex-direction: column; align-items: center; gap: 7px">%s<span style="font: 700 13.5px/1 %s">%s</span></button>'
                        % (gumdrop(name, colours[i]), F, label) for i, (name, label) in enumerate(SHORTCUTS))
    shortcuts = '<div style="position: absolute; left: 20px; right: 20px; top: 522px; display: flex; justify-content: space-between">%s</div>' % shortcuts
    toys = ''.join(img(t, 'width: 84px; height: 84px; object-fit: contain; filter: drop-shadow(0 6px 6px rgba(150, 60, 100, .22))') for t in PEEK)
    peek = ('<section style="position: absolute; left: 20px; right: 20px; top: 626px">'
            '<div style="display: flex; align-items: baseline; justify-content: space-between"><h2 style="margin: 0; font: 800 20px/1 %s">Your shelf</h2><a href="#" style="font: 700 14px/1 %s; color: #B01E60; text-decoration: none">See all</a></div>'
            '<div style="display: flex; justify-content: space-around; align-items: flex-end; margin-top: 4px; padding: 0 10px">%s</div>'
            '<div aria-hidden="true" style="height: 14px; margin-top: -5px; border-radius: 8px; background: linear-gradient(rgba(255, 255, 255, .95), rgba(255, 220, 232, .9)); border: 1.5px solid #fff; box-shadow: 0 10px 16px rgba(196, 72, 128, .22)"></div></section>'
            % (F, F, toys))
    body = (img('scene-clouds', 'position: absolute; left: 0; top: 0; width: 390px; height: 844px; object-fit: cover')
            + '<div aria-hidden="true" style="position: absolute; left: 0; top: 380px; width: 390px; height: 464px; background: linear-gradient(rgba(255, 238, 244, 0), #FFEEF4 46%)"></div>'
            + head + hero + nextbox + shortcuts + peek + dock('home'))
    return page('Home, Jelly look', root(body, '#FFEEF4'))

RARITY = {'rare': ('gold', 'star', 'Rare'), 'epic': ('sky', 'gem', 'Epic'), 'secret': ('lilac', 'sparkle', 'Secret')}

def shelf():
    head = ('<div style="position: absolute; left: 20px; right: 20px; top: 52px; height: 44px; display: flex; align-items: center; gap: 10px">'
            '<h1 style="margin: 0; font: 800 32px/1 %s; letter-spacing: -.01em">Shelf</h1>'
            '<span style="padding: 7px 11px; border-radius: 999px; font: 700 13px/1 %s; color: %s; background: rgba(255, 255, 255, .7); border: 1.5px solid #fff">53 toys</span>'
            '<span style="flex: 1"></span><button type="button" aria-label="Search">%s</button></div>' % (F, F, SOFT, glass_round('search')))
    segs = ''
    for i, label in enumerate(['All', 'Fusions', 'Extras']):
        on = i == 0
        segs += ('<button type="button" style="position: relative; flex: 1; height: 36px; border-radius: 18px; font: %s 14.5px/1 %s; color: %s; %s">%s%s</button>'
                 % ('800' if on else '700', F, '#fff' if on else SOFT,
                    'background: linear-gradient(%s, %s); box-shadow: 0 6px 12px rgba(190, 20, 90, .32), inset 0 -4px 8px rgba(90, 0, 40, .28), inset 0 1.5px 0 rgba(255, 255, 255, .5)' % (BERRY_TOP, BERRY_BOTTOM) if on else '',
                    gloss('3px 14% auto 14%', '40%') if on else '', label))
    switch = ('<div role="group" aria-label="Show" style="position: absolute; left: 20px; right: 20px; top: 108px; height: 46px; border-radius: 23px; padding: 4px; box-sizing: border-box; display: flex; gap: 4px; %s">%s</div>' % (GLASS, segs))
    rows = ''
    for r, row in enumerate(SHELF):
        toys = labels = ''
        for name, pic, rarity, copies in row:
            extra = ('<span style="position: absolute; right: 2px; top: 0; min-width: 26px; height: 26px; padding: 0 6px; box-sizing: border-box; border-radius: 13px; display: flex; align-items: center; justify-content: center; '
                     'font: 800 12.5px/1 %s; color: %s; background: rgba(255, 255, 255, .92); border: 1.5px solid #fff; box-shadow: 0 4px 8px rgba(196, 72, 128, .25)">×%d</span>' % (F, PLUM, copies)) if copies > 1 else ''
            bead = ''
            if rarity:
                colour, glyph, word = RARITY[rarity]
                top, bottom, glow = CANDY[colour]
                bead = ('<span role="img" aria-label="%s" style="position: absolute; left: 2px; bottom: 8px; width: 26px; height: 26px; border-radius: 50%%; display: flex; align-items: center; justify-content: center; '
                        'background: radial-gradient(circle at 50%% 30%%, %s, %s); box-shadow: 0 4px 8px %s, inset 0 1.5px 0 rgba(255, 255, 255, .7)">%s</span>' % (word, top, bottom, glow, icon(glyph, 14, '#fff', 2.8)))
            toys += ('<div style="position: relative; width: 104px; height: 100px; display: flex; align-items: flex-end; justify-content: center">%s%s%s</div>'
                     % (img(pic, 'width: 96px; height: 96px; object-fit: contain; filter: drop-shadow(0 7px 6px rgba(150, 60, 100, .22))', name), extra, bead))
            labels += '<span style="width: 104px; font: 700 12.5px/1.15 %s; text-align: center">%s</span>' % (F, name)
        rows += ('<div style="display: flex; flex-direction: column"><div style="display: flex; justify-content: space-between; padding: 0 6px">%s</div>'
                 '<div aria-hidden="true" style="height: 15px; margin-top: -6px; border-radius: 8px; background: linear-gradient(rgba(255, 255, 255, .96), rgba(255, 222, 234, .92)); border: 1.5px solid #fff; box-shadow: 0 11px 16px rgba(196, 72, 128, .2)"></div>'
                 '<div style="display: flex; justify-content: space-between; padding: 9px 6px 0">%s</div></div>' % (toys, labels))
    grid = '<div style="position: absolute; left: 20px; right: 20px; top: 170px; display: flex; flex-direction: column; gap: 16px">%s</div>' % rows
    body = (img('scene-clouds', 'position: absolute; left: 0; top: 0; width: 390px; height: 844px; object-fit: cover; opacity: .55')
            + '<div aria-hidden="true" style="position: absolute; left: 0; top: 200px; width: 390px; height: 644px; background: linear-gradient(rgba(255, 238, 244, 0), #FFEEF4 40%)"></div>'
            + head + switch + grid + dock('shelf'))
    return page('Shelf, Jelly look', root(body, '#FFEEF4'))

def play():
    top = ('<div style="position: absolute; left: 16px; right: 16px; top: 52px; height: 44px; display: flex; align-items: center; justify-content: space-between">'
           '<button type="button" aria-label="Back">%s</button>'
           '<button type="button" style="height: 40px; padding: 0 16px; border-radius: 20px; display: flex; align-items: center; gap: 8px; font: 700 14.5px/1 %s; %s">%s<span>Pip · 9 of 15</span></button>'
           '<button type="button" aria-label="Favorite">%s</button></div>'
           % (glass_round('back'), F, GLASS, ring(20, .6, 'rgba(42, 20, 48, .16)', '#EC4B8B', 4), glass_round('heart', 44, '#D62A72', 22, '#EC4B8B')))
    chips = ''.join('<span style="padding: 7px 12px; border-radius: 999px; font: 700 13.5px/1 %s; background: rgba(255, 255, 255, .62); border: 1.5px solid rgba(255, 255, 255, .95)">%s</span>' % (F, w) for w in ['Sunny', 'Curious'])
    title = ('<div style="position: absolute; left: 20px; top: 112px"><h1 style="margin: 0; font: 800 36px/1 %s; letter-spacing: -.015em; text-shadow: 0 1px 12px rgba(255, 255, 255, .9)">Peach Jelly</h1>'
             '<div style="display: flex; gap: 6px; margin-top: 10px">%s</div></div>' % (F, chips))
    toy = ('<div aria-hidden="true" style="position: absolute; left: 75px; top: 588px; width: 240px; height: 34px; border-radius: 50%; background: radial-gradient(ellipse, rgba(150, 50, 90, .34), rgba(150, 50, 90, 0) 70%)"></div>'
           + img('hero-peach', 'position: absolute; left: 45px; top: 312px; width: 300px; height: auto', 'Peach Jelly'))
    bubble = ('<div style="position: absolute; left: 236px; top: 262px; padding: 11px 18px; border-radius: 24px 24px 24px 6px; font: 800 20px/1 %s; background: rgba(255, 255, 255, .86); border: 1.5px solid #fff; '
              'box-shadow: 0 10px 20px rgba(160, 60, 110, .22), inset 0 1px 0 #fff">yay!</div>' % F)
    colours = ['pink', 'mint', 'gold', 'lilac', 'sky']
    controls = ''.join('<button type="button" style="flex: 1; min-width: 0; display: flex; flex-direction: column; align-items: center; gap: 6px">%s<span style="font: 700 12.5px/1 %s">%s</span></button>'
                       % (gumdrop(name, colours[i], 50, 24), F, label) for i, (name, label) in enumerate(CONTROLS))
    tray = ('<div role="group" aria-label="Play controls" style="position: absolute; left: 12px; right: 12px; bottom: 26px; height: 96px; border-radius: 34px; box-sizing: border-box; padding: 0 6px; display: flex; align-items: center; '
            'background: rgba(255, 255, 255, .7); border: 1.5px solid #fff; backdrop-filter: blur(18px); -webkit-backdrop-filter: blur(18px); box-shadow: 0 18px 34px rgba(120, 40, 80, .28), inset 0 1px 0 #fff">%s</div>' % controls)
    hint = ('<div style="position: absolute; left: 0; right: 0; bottom: 136px; display: flex; justify-content: center"><span style="padding: 8px 14px; border-radius: 999px; font: 700 13px/1 %s; background: rgba(255, 255, 255, .6); border: 1.5px solid rgba(255, 255, 255, .9)">Tap · drag to turn</span></div>' % F)
    body = img('world-jelly-garden', 'position: absolute; left: 0; top: 0; width: 390px; height: 844px; object-fit: cover') + top + title + toy + bubble + hint + tray
    return page('Play, Jelly look', root(body, '#F6C9D2'))

BOARDS = [('Main.dc.html', 'A · Jelly · Home', home), ('A-Shelf.dc.html', 'A · Jelly · Shelf', shelf), ('A-Play.dc.html', 'A · Jelly · Play', play)]
