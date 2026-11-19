"""Look C, Toy shop: the app is a little shop. An awning, a window, wooden shelves, brass labels, buttons you could press."""
from common import *

WALL, CREAM, INK = '#FBEAD6', '#FFF6E8', '#2A1430'
WOOD_TOP, WOOD, WOOD_DARK, WOOD_DEEP = '#E6B581', '#CB905A', '#A66A3B', '#7A4826'
COUNTER, COUNTER_DEEP = '#8C562C', '#6E411F'
BRASS = 'linear-gradient(#EFCB7C, #C9952F)'
ROSE, ROSE_DARK = '#C9306E', '#8F1D4B'
WALLPAPER = 'background-color: %s; background-image: repeating-linear-gradient(90deg, rgba(196, 140, 96, .1) 0 2px, rgba(196, 140, 96, 0) 2px 26px);' % WALL
GRAIN = 'repeating-linear-gradient(90deg, rgba(90, 50, 20, .07) 0 3px, rgba(90, 50, 20, 0) 3px 11px, rgba(255, 230, 190, .08) 11px 13px, rgba(90, 50, 20, 0) 13px 29px)'

def plaque(name, size=62, glyph=28, wood=True):
    if wood:
        face = 'background: %s, linear-gradient(%s, %s); box-shadow: 0 5px 0 %s, 0 10px 12px rgba(90, 50, 20, .3), inset 0 2px 0 rgba(255, 236, 200, .55)' % (GRAIN, WOOD_TOP, WOOD, WOOD_DARK)
        colour = CREAM
    else:
        face = 'background: linear-gradient(#FFFBF2, %s); box-shadow: 0 4px 0 #D9BC98, 0 9px 12px rgba(90, 50, 20, .28), inset 0 2px 0 #fff' % CREAM
        colour = INK
    return ('<div style="width: %dpx; height: %dpx; border-radius: %dpx; display: flex; align-items: center; justify-content: center; %s">%s</div>'
            % (size, size, round(size * .27), face, icon(name, glyph, colour, 2.4)))

def tag(text, turn=0, size=12.5):
    return ('<span style="display: inline-flex; align-items: center; gap: 6px; padding: 6px 11px 6px 8px; border-radius: 7px 4px 4px 7px; font: 800 %spx/1 %s; letter-spacing: .04em; color: %s; '
            'background: %s; border: 1.5px solid #E2C9A6; box-shadow: 0 3px 5px rgba(90, 50, 20, .22); transform: rotate(%ddeg)">'
            '<span aria-hidden="true" style="width: 6px; height: 6px; border-radius: 50%%; background: %s; box-shadow: inset 0 1px 1px rgba(0, 0, 0, .35)"></span>%s</span>' % (size, F, INK, CREAM, turn, WOOD_DARK, text))

def plank(extra=''):
    return ('<div aria-hidden="true" style="height: 20px; border-radius: 4px; background: %s, linear-gradient(%s 0 11px, %s 11px 20px); '
            'box-shadow: 0 12px 14px rgba(90, 50, 20, .26), inset 0 1.5px 0 rgba(255, 236, 200, .7); %s"></div>' % (GRAIN, WOOD_TOP, WOOD_DARK, extra))

def press(label, name, height=58, size=20):
    return ('<button type="button" style="width: 100%%; height: %dpx; border-radius: 18px; display: flex; align-items: center; justify-content: center; gap: 10px; background: linear-gradient(#DA3F7E, %s); color: #fff; '
            'font: 800 %dpx/1 %s; box-shadow: 0 6px 0 %s, 0 13px 16px rgba(120, 20, 60, .3), inset 0 2px 0 rgba(255, 255, 255, .4)">%s<span>%s</span></button>' % (height, ROSE, size, F, ROSE_DARK, icon(name, 24, '#fff', 2.4), label))

def counter(selected):
    items = ''
    for name, label in TABS:
        on = name == selected
        sign = ('background: linear-gradient(#FFFBF2, %s); box-shadow: 0 4px 0 #D9BC98, 0 8px 10px rgba(60, 30, 10, .35), inset 0 2px 0 #fff; transform: translateY(-5px)' % CREAM) if on else ''
        items += ('<button type="button" style="flex: 1; min-width: 0; height: 62px; margin: 0 3px; border-radius: 15px; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 4px; %s">'
                  '%s<span style="font: %s 11.5px/1 %s; color: %s">%s</span></button>' % (sign, icon(name, 24, INK if on else CREAM, 2.3), '800' if on else '700', F, INK if on else CREAM, label))
    return ('<nav aria-label="Main" style="position: absolute; left: 0; right: 0; bottom: 0; height: 112px; box-sizing: border-box; padding: 12px 6px 34px; display: flex; align-items: center; '
            'background: %s, linear-gradient(%s 0 7px, %s 7px 100%%); box-shadow: 0 -10px 18px rgba(90, 50, 20, .28), inset 0 2px 0 rgba(255, 236, 200, .8)">%s</nav>' % (GRAIN, WOOD_TOP, COUNTER, items))

def root(inner, background):
    return ('<div style="position: relative; width: 390px; height: 844px; overflow: hidden; %s font-family: %s; color: %s">%s</div>' % (background, F, INK, inner))

def awning():
    scallops = ''.join('<span style="width: 39px; height: 20px; border-radius: 0 0 20px 20px; background: %s; box-shadow: 0 4px 5px rgba(120, 40, 70, .18)"></span>' % ('#F487AC' if i % 2 == 0 else CREAM) for i in range(10))
    return ('<div aria-hidden="true" style="position: absolute; left: 0; top: 0; width: 390px; height: 66px; background: repeating-linear-gradient(90deg, #F487AC 0 39px, %s 39px 78px); box-shadow: inset 0 -10px 12px rgba(120, 40, 70, .14)"></div>'
            '<div aria-hidden="true" style="position: absolute; left: 0; top: 66px; width: 390px; display: flex">%s</div>' % (CREAM, scallops))

def home():
    sign = ('<div style="position: absolute; left: 20px; top: 70px; height: 54px; padding: 0 18px; border-radius: 27px; display: flex; align-items: center; background: linear-gradient(#FFFBF2, %s); '
            'border: 2px solid #E2C9A6; box-shadow: 0 5px 0 #D9BC98, 0 12px 14px rgba(90, 50, 20, .24)">%s</div>'
            '<button type="button" aria-label="Your profile" style="position: absolute; right: 20px; top: 72px; width: 50px; height: 50px; border-radius: 50%%; overflow: hidden; box-sizing: border-box; '
            'border: 4px solid #D9A441; box-shadow: 0 4px 0 #A9781F, 0 10px 12px rgba(90, 50, 20, .28)">%s</button>'
            % (CREAM, img('wordmark', 'height: 34px; width: auto; display: block', 'Pockle'), img('avatar-fox', 'width: 100%; height: 100%; object-fit: cover; display: block')))
    window = ('<section style="position: absolute; left: 16px; top: 140px; width: 358px; height: 292px; border-radius: 22px; box-sizing: border-box; padding: 9px; background: %s, linear-gradient(%s, %s); '
              'box-shadow: 0 6px 0 %s, 0 16px 22px rgba(90, 50, 20, .32), inset 0 2px 0 rgba(255, 236, 200, .7)">'
              '<div style="position: relative; width: 100%%; height: 100%%; border-radius: 14px; overflow: hidden; box-shadow: inset 0 0 0 2px rgba(90, 50, 20, .35)">'
              '%s'
              '<div aria-hidden="true" style="position: absolute; left: 70px; top: -40px; width: 200px; height: 300px; background: radial-gradient(ellipse at 50%% 0, rgba(255, 250, 230, .85), rgba(255, 250, 230, 0) 70%%)"></div>'
              '<div aria-hidden="true" style="position: absolute; left: 62px; bottom: 58px; width: 216px; height: 40px; border-radius: 50%%; background: linear-gradient(%s, %s); box-shadow: 0 8px 0 %s, 0 16px 16px rgba(60, 30, 10, .35)"></div>'
              '%s'
              '<div style="position: absolute; right: 26px; top: 20px">%s</div>'
              '<div style="position: absolute; left: 14px; right: 14px; bottom: 12px; height: 34px; border-radius: 8px; display: flex; align-items: center; justify-content: center; background: %s; '
              'box-shadow: 0 3px 0 #A9781F, inset 0 1.5px 0 rgba(255, 246, 210, .8); font: 800 17px/1 %s; letter-spacing: .08em; text-transform: uppercase; color: #3F2708">Wally box</div>'
              '</div></section>'
              % (GRAIN, WOOD_TOP, WOOD, WOOD_DARK,
                 img('scene-room', 'position: absolute; left: 0; top: -150px; width: 340px; height: auto; filter: blur(1.5px) saturate(1.05)'),
                 WOOD_TOP, WOOD, WOOD_DARK,
                 img('box-wally', 'position: absolute; left: 88px; top: 26px; width: 164px; height: auto; filter: drop-shadow(0 12px 8px rgba(60, 30, 10, .35))', 'Wally collection box'),
                 tag('READY', 8), BRASS, F))
    button = '<div style="position: absolute; left: 16px; right: 16px; top: 448px">%s</div>' % press('Open', 'box')
    stamps = ''
    for i in range(4):
        stamps += ('<span style="width: 26px; height: 26px; border-radius: 50%%; box-sizing: border-box; display: flex; align-items: center; justify-content: center; %s">%s</span>'
                   % ('background: %s; box-shadow: 0 2px 0 %s; transform: rotate(-10deg)' % (ROSE, ROSE_DARK) if i == 0 else 'border: 2px dashed #C9A57C', icon('star', 13, '#fff', 2.6) if i == 0 else ''))
    card = ('<section style="position: absolute; left: 16px; top: 528px; width: 358px; height: 64px; box-sizing: border-box; border-radius: 14px; display: flex; align-items: center; gap: 12px; padding: 0 14px 0 10px; '
            'background: linear-gradient(#FFFBF2, %s); border: 1.5px solid #E2C9A6; box-shadow: 0 4px 0 #D9BC98, 0 10px 12px rgba(90, 50, 20, .2)">'
            '<div style="position: relative; width: 46px; height: 46px; flex: none">%s<div style="position: absolute; inset: 0; display: flex; align-items: center; justify-content: center">%s</div></div>'
            '<div style="flex: 1; min-width: 0"><div style="font: 800 16px/1.15 %s">680 steps to go</div><div style="font: 500 13px/1.3 %s; color: %s">Next box · 320 of 1,000</div></div>'
            '<div aria-label="1 of 4 boxes today" style="display: flex; gap: 5px">%s</div></section>'
            % (CREAM, ring(46, .32, '#EAD3B4', ROSE, 6), icon('walk', 20, WOOD_DEEP, 2.3), F, F, SOFT, stamps))
    shortcuts = ''.join('<button type="button" style="width: 76px; display: flex; flex-direction: column; align-items: center; gap: 11px">%s<span style="font: 700 13.5px/1 %s">%s</span></button>'
                        % (plaque(name, 60, 27), F, label) for name, label in SHORTCUTS)
    shortcuts = '<div style="position: absolute; left: 20px; right: 20px; top: 612px; display: flex; justify-content: space-between">%s</div>' % shortcuts
    return page('Home, Toy shop look', root(awning() + sign + window + button + card + shortcuts + counter('home'), WALLPAPER))

GEMS = {'rare': ('linear-gradient(#FFE08A, #E8A21C)', 'star', 'Rare', '#5A3A06'), 'epic': ('linear-gradient(#BFE6FF, #4E9FE6)', 'gem', 'Epic', '#0B3358'), 'secret': ('linear-gradient(#5A3A66, #2A1430)', 'sparkle', 'Secret', '#fff')}

def shelf():
    head = ('<div style="position: absolute; left: 20px; right: 20px; top: 52px; height: 46px; display: flex; align-items: center; gap: 12px">'
            '<h1 style="margin: 0; font: 800 32px/1 %s; letter-spacing: -.01em">Shelf</h1>%s<span style="flex: 1"></span><button type="button" aria-label="Search">%s</button></div>'
            % (F, tag('53 toys', -4), plaque('search', 44, 22, False)))
    segs = ''
    for i, label in enumerate(['All', 'Fusions', 'Extras']):
        on = i == 0
        segs += ('<button type="button" style="flex: 1; height: 36px; border-radius: 11px; font: %s 14.5px/1 %s; color: %s; %s">%s</button>'
                 % ('800' if on else '700', F, INK if on else CREAM, 'background: linear-gradient(#FFFBF2, %s); box-shadow: 0 3px 0 #D9BC98, inset 0 1.5px 0 #fff' % CREAM if on else '', label))
    switch = ('<div role="group" aria-label="Show" style="position: absolute; left: 20px; right: 20px; top: 110px; height: 48px; border-radius: 15px; padding: 5px 5px 8px; box-sizing: border-box; display: flex; gap: 4px; '
              'background: %s, linear-gradient(%s, %s); box-shadow: 0 4px 0 %s, 0 10px 12px rgba(90, 50, 20, .24), inset 0 2px 0 rgba(255, 236, 200, .35)">%s</div>' % (GRAIN, COUNTER, COUNTER_DEEP, '#4F2C12', segs))
    rows = ''
    for r, row in enumerate(SHELF):
        toys = labels = ''
        for name, pic, rarity, copies in row:
            extra = ('<span style="position: absolute; right: 0; top: 4px; min-width: 28px; height: 28px; padding: 0 6px; box-sizing: border-box; border-radius: 14px; display: flex; align-items: center; justify-content: center; '
                     'font: 800 12.5px/1 %s; background: %s; border: 1.5px solid #E2C9A6; box-shadow: 0 3px 5px rgba(90, 50, 20, .26); transform: rotate(8deg)">×%d</span>' % (F, CREAM, copies)) if copies > 1 else ''
            toys += ('<div style="position: relative; width: 106px; height: 102px; display: flex; align-items: flex-end; justify-content: center">'
                     '<div aria-hidden="true" style="position: absolute; left: 14px; right: 14px; bottom: -5px; height: 14px; border-radius: 50%%; background: radial-gradient(ellipse, rgba(70, 35, 10, .38), rgba(70, 35, 10, 0) 70%%)"></div>%s%s</div>'
                     % (img(pic, 'position: relative; width: 96px; height: 96px; object-fit: contain', name), extra))
            gem = ''
            if rarity:
                fill, glyph, word, glyph_colour = GEMS[rarity]
                gem = ('<span role="img" aria-label="%s" style="position: absolute; left: -7px; top: -9px; width: 22px; height: 22px; border-radius: 50%%; display: flex; align-items: center; justify-content: center; background: %s; '
                       'border: 2px solid %s; box-sizing: border-box; box-shadow: 0 2px 4px rgba(90, 50, 20, .3)">%s</span>' % (word, fill, CREAM, icon(glyph, 11, glyph_colour, 3)))
            labels += ('<div style="position: relative; width: 110px; height: 26px; box-sizing: border-box; border-radius: 5px; padding: 0 4px; display: flex; align-items: center; justify-content: center; background: %s; '
                       'border: 1.5px solid #C9952F; box-shadow: 0 2px 3px rgba(90, 50, 20, .22)">%s<span style="font: 700 11px/1 %s; white-space: nowrap">%s</span></div>' % (CREAM, gem, F, name))
        rows += ('<div style="display: flex; flex-direction: column"><div style="display: flex; justify-content: space-between; padding: 0 6px">%s</div>%s'
                 '<div style="display: flex; justify-content: space-between; padding: 0 2px; margin-top: -3px; position: relative">%s</div></div>' % (toys, plank('margin: 0 -8px'), labels))
    grid = '<div style="position: absolute; left: 20px; right: 20px; top: 166px; display: flex; flex-direction: column; gap: 6px">%s</div>' % rows
    lamp = '<div aria-hidden="true" style="position: absolute; left: 0; top: 150px; width: 390px; height: 300px; background: radial-gradient(ellipse at 50% 0, rgba(255, 246, 214, .7), rgba(255, 246, 214, 0) 70%)"></div>'
    return page('Shelf, Toy shop look', root(lamp + head + switch + grid + counter('shelf'), WALLPAPER))

def play():
    top = ('<div style="position: absolute; left: 16px; right: 16px; top: 52px; height: 46px; display: flex; align-items: center; justify-content: space-between">'
           '<button type="button" aria-label="Back">%s</button><button type="button">%s</button><button type="button" aria-label="Favorite">%s</button></div>'
           % (plaque('back', 44, 22, False), tag('Pip · 9 of 15', -2, 13.5),
              '<div style="width: 44px; height: 44px; border-radius: 12px; display: flex; align-items: center; justify-content: center; background: linear-gradient(#FFFBF2, %s); box-shadow: 0 4px 0 #D9BC98, 0 9px 12px rgba(90, 50, 20, .28), inset 0 2px 0 #fff">%s</div>'
              % (CREAM, icon('heart', 22, ROSE, 2.4, '#E8508A'))))
    title = ('<div style="position: absolute; left: 20px; top: 114px; padding: 12px 16px 11px; border-radius: 10px; background: linear-gradient(#FFFBF2, %s); border: 2px solid #C9952F; '
             'box-shadow: 0 4px 0 #A9781F, 0 12px 16px rgba(60, 30, 10, .3)"><h1 style="margin: 0; font: 800 30px/1 %s; letter-spacing: -.015em">Peach Jelly</h1>'
             '<p style="margin: 6px 0 0; font: 700 14px/1 %s; color: %s">Sunny · Curious</p></div>' % (CREAM, F, F, SOFT))
    toy = ('<div aria-hidden="true" style="position: absolute; left: 75px; top: 586px; width: 240px; height: 34px; border-radius: 50%; background: radial-gradient(ellipse, rgba(150, 50, 90, .34), rgba(150, 50, 90, 0) 70%)"></div>'
           + img('hero-peach', 'position: absolute; left: 45px; top: 310px; width: 300px; height: auto', 'Peach Jelly'))
    bubble = ('<div style="position: absolute; left: 240px; top: 262px; padding: 10px 17px; border-radius: 20px 20px 20px 5px; font: 800 19px/1 %s; background: linear-gradient(#FFFBF2, %s); border: 1.5px solid #E2C9A6; '
              'box-shadow: 0 4px 0 #D9BC98, 0 10px 14px rgba(60, 30, 10, .28)">yay!</div>' % (F, CREAM))
    controls = ''.join('<button type="button" style="flex: 1; min-width: 0; display: flex; flex-direction: column; align-items: center; gap: 9px">%s<span style="font: 700 12.5px/1 %s; color: %s">%s</span></button>'
                       % (plaque(name, 50, 24, False), F, CREAM, label) for name, label in CONTROLS)
    tray = ('<div role="group" aria-label="Play controls" style="position: absolute; left: 0; right: 0; bottom: 0; height: 132px; box-sizing: border-box; padding: 4px 8px 34px; border-radius: 26px 26px 0 0; display: flex; align-items: center; '
            'background: %s, linear-gradient(%s 0 7px, %s 7px 100%%); box-shadow: 0 -10px 20px rgba(60, 30, 10, .34), inset 0 2px 0 rgba(255, 236, 200, .8)">%s</div>' % (GRAIN, WOOD_TOP, COUNTER, controls))
    hint = '<div style="position: absolute; left: 0; right: 0; bottom: 146px; display: flex; justify-content: center">%s</div>' % tag('Tap · drag to turn', 0, 13)
    body = img('world-jelly-garden', 'position: absolute; left: 0; top: 0; width: 390px; height: 844px; object-fit: cover') + top + title + toy + bubble + hint + tray
    return page('Play, Toy shop look', root(body, 'background: #F6C9D2;'))

BOARDS = [('C-Home.dc.html', 'C · Toy shop · Home', home), ('C-Shelf.dc.html', 'C · Toy shop · Shelf', shelf), ('C-Play.dc.html', 'C · Toy shop · Play', play)]
