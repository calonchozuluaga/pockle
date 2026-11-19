"""Look B, Sticker book: a collector's scrapbook. Cream paper, die-cut stickers, washi tape, handwritten notes."""
from common import *

PAPER, CARD, INK = '#FBF1E0', '#FFFDF7', '#2A1430'
DOTS = 'background-color: %s; background-image: radial-gradient(rgba(42, 20, 48, .14) 1.2px, rgba(42, 20, 48, 0) 1.4px); background-size: 18px 18px;' % PAPER
DIECUT = 'filter: drop-shadow(2.5px 0 0 #fff) drop-shadow(-2.5px 0 0 #fff) drop-shadow(0 2.5px 0 #fff) drop-shadow(0 -2.5px 0 #fff) drop-shadow(0 5px 5px rgba(90, 60, 30, .28));'
TAPES = {'pink': ('rgba(255, 150, 186, .82)', 'rgba(255, 190, 212, .82)'), 'mint': ('rgba(126, 214, 178, .8)', 'rgba(176, 234, 208, .8)'),
         'lemon': ('rgba(255, 210, 96, .82)', 'rgba(255, 228, 150, .82)'), 'lilac': ('rgba(178, 162, 255, .8)', 'rgba(210, 200, 255, .8)')}
STICKER = {'lemon': '#FFDE7A', 'pink': '#FFAECB', 'mint': '#B4E9CF', 'lilac': '#D6CEFF', 'sky': '#BFE3FF'}

def tape(colour, style):
    a, b = TAPES[colour]
    return ('<div aria-hidden="true" style="position: absolute; height: 24px; background: repeating-linear-gradient(45deg, %s 0 8px, %s 8px 16px); '
            'box-shadow: 0 2px 3px rgba(90, 60, 30, .14); %s"></div>' % (a, b, style))

def sticker(name, colour, size=62, glyph=28, turn=0):
    return ('<div style="width: %dpx; height: %dpx; border-radius: 50%%; display: flex; align-items: center; justify-content: center; box-sizing: border-box; background: %s; '
            'border: 3.5px solid #fff; box-shadow: 0 5px 8px rgba(90, 60, 30, .26); transform: rotate(%ddeg)">%s</div>' % (size, size, STICKER[colour], turn, icon(name, glyph, INK, 2.3)))

def paper_round(name, size=44, glyph=22, colour=INK, fill='none'):
    return ('<div style="width: %dpx; height: %dpx; border-radius: 50%%; display: flex; align-items: center; justify-content: center; box-sizing: border-box; background: %s; '
            'border: 3px solid #fff; box-shadow: 0 4px 7px rgba(90, 60, 30, .26)">%s</div>' % (size, size, CARD, icon(name, glyph, colour, 2.3, fill)))

def marker(text, size, colour='rgba(255, 150, 186, .6)'):
    return ('<span style="position: relative; display: inline-block; font: 800 %dpx/1 %s; letter-spacing: -.01em">'
            '<span aria-hidden="true" style="position: absolute; left: -6px; right: -8px; bottom: -1px; height: 46%%; border-radius: 6px 12px 8px 10px; background: %s; transform: rotate(-1.2deg)"></span>'
            '<span style="position: relative">%s</span></span>' % (size, F, colour, text))

def tabs(selected):
    items = ''
    for name, label in TABS:
        on = name == selected
        mark = ('<span aria-hidden="true" style="position: absolute; left: 50%; top: 4px; width: 54px; height: 34px; margin-left: -27px; border-radius: 14px 18px 12px 16px; background: #FFAECB; transform: rotate(-4deg)"></span>' if on else '')
        items += ('<button type="button" style="position: relative; flex: 1; min-width: 0; height: 62px; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 4px">'
                  '%s<span style="position: relative">%s</span><span style="position: relative; font: %s 11.5px/1 %s; color: %s">%s</span></button>'
                  % (mark, icon(name, 24, INK if on else SOFT, 2.3), '800' if on else '700', F, INK if on else SOFT, label))
    return ('<nav aria-label="Main" style="position: absolute; left: 0; right: 0; bottom: 0; height: 106px; box-sizing: border-box; padding: 8px 6px 34px; display: flex; align-items: center; '
            'background: %s; border-top: 2px dashed rgba(42, 20, 48, .3); box-shadow: 0 -8px 18px rgba(90, 60, 30, .1)">%s</nav>' % (CARD, items))

def root(inner, background):
    return ('<div style="position: relative; width: 390px; height: 844px; overflow: hidden; %s font-family: %s; color: %s">%s</div>' % (background, F, INK, inner))

def home():
    head = ('<div style="position: absolute; left: 20px; right: 20px; top: 52px; height: 46px; display: flex; align-items: center; justify-content: space-between">'
            '%s<button type="button" aria-label="Your profile" style="width: 46px; height: 46px; border-radius: 50%%; overflow: hidden; border: 3.5px solid #fff; box-sizing: border-box; '
            'box-shadow: 0 5px 8px rgba(90, 60, 30, .28); transform: rotate(5deg)">%s</button></div>'
            % (img('wordmark', 'height: 38px; width: auto; display: block; transform: rotate(-2deg); ' + DIECUT, 'Pockle'), img('avatar-fox', 'width: 100%; height: 100%; object-fit: cover; display: block')))
    hero = ('<section style="position: absolute; left: 20px; top: 116px; width: 350px; height: 306px; border-radius: 10px; background: %s; box-shadow: 0 1px 0 rgba(90, 60, 30, .1), 0 12px 24px rgba(90, 60, 30, .17); transform: rotate(-1.1deg)">'
            '%s%s'
            '<p style="position: absolute; left: 22px; top: 26px; margin: 0; font: 400 27px/1 %s; transform: rotate(-3deg)">today\'s box!</p>'
            '<svg aria-hidden="true" width="74" height="46" viewBox="0 0 74 46" fill="none" stroke="%s" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" style="position: absolute; left: 96px; top: 60px"><path d="M4 6c16 26 34 32 60 28"/><path d="M53 26l12 8-11 9"/></svg>'
            '<div style="position: absolute; left: 22px; top: 126px; display: flex; flex-direction: column; align-items: flex-start; gap: 9px">'
            '<span style="padding: 7px 11px 6px; font: 800 13px/1 %s; letter-spacing: .16em; text-transform: uppercase; color: %s; background: %s; border-radius: 3px; transform: rotate(1.5deg)">Wally box</span>'
            '<span style="font: 400 19px/1.1 %s; color: %s; max-width: 112px">who\'s hiding inside?</span></div>'
            '%s'
            '<button type="button" style="position: absolute; left: 20px; right: 20px; bottom: 20px; height: 58px; border-radius: 29px; box-sizing: border-box; display: flex; align-items: center; justify-content: center; gap: 10px; '
            'background: #FF9CBE; border: 4px solid #fff; box-shadow: 0 4px 0 rgba(42, 20, 48, .16), 0 9px 14px rgba(90, 60, 30, .22); font: 800 20px/1 %s; color: %s">%s<span>Open</span></button></section>'
            % (CARD, tape('pink', 'left: -18px; top: -9px; width: 86px; transform: rotate(-9deg)'), tape('mint', 'right: -16px; top: -8px; width: 78px; transform: rotate(7deg)'),
               HAND, INK, F, CARD, INK, HAND, SOFT,
               img('box-wally', 'position: absolute; right: 6px; top: 22px; width: 190px; height: auto; transform: rotate(4deg); ' + DIECUT, 'Wally collection box'), F, INK, icon('box', 24, INK, 2.4)))
    stamps = ''
    for i in range(4):
        stamps += ('<span style="width: 26px; height: 26px; border-radius: 50%%; box-sizing: border-box; display: flex; align-items: center; justify-content: center; %s">%s</span>'
                   % ('background: #FFAECB; border: 2px solid %s; transform: rotate(-10deg)' % INK if i == 0 else 'border: 2px dashed rgba(42, 20, 48, .4)', icon('star', 13, INK, 2.6) if i == 0 else ''))
    nextbox = ('<section style="position: absolute; left: 20px; top: 438px; width: 350px; height: 68px; box-sizing: border-box; border-radius: 12px; display: flex; align-items: center; gap: 12px; padding: 0 14px 0 12px; '
               'background: %s; border: 2px dashed rgba(42, 20, 48, .45); transform: rotate(.6deg)">'
               '<div style="position: relative; width: 46px; height: 46px; flex: none">%s<div style="position: absolute; inset: 0; display: flex; align-items: center; justify-content: center">%s</div></div>'
               '<div style="flex: 1; min-width: 0"><div style="font: 400 21px/1.05 %s">680 steps to go</div><div style="font: 700 12.5px/1.3 %s; color: %s">320 of 1,000</div></div>'
               '<div aria-label="1 of 4 boxes today" style="display: flex; gap: 5px">%s</div></section>'
               % (CARD, ring(46, .32, 'rgba(42, 20, 48, .14)', '#F0528C', 6), icon('walk', 20, INK, 2.3), HAND, F, SOFT, stamps))
    colours = [('lemon', -5), ('pink', 4), ('mint', -3), ('lilac', 6)]
    shortcuts = ''.join('<button type="button" style="width: 76px; display: flex; flex-direction: column; align-items: center; gap: 5px">%s<span style="font: 400 19px/1 %s">%s</span></button>'
                        % (sticker(name, colours[i][0], 62, 28, colours[i][1]), HAND, label) for i, (name, label) in enumerate(SHORTCUTS))
    shortcuts = '<div style="position: absolute; left: 22px; right: 22px; top: 524px; display: flex; justify-content: space-between">%s</div>' % shortcuts
    turns = [-4, 3, -2]
    toys = ''.join(img(t, 'width: 78px; height: 78px; object-fit: contain; transform: rotate(%ddeg); %s' % (turns[i], DIECUT)) for i, t in enumerate(PEEK))
    peek = ('<section style="position: absolute; left: 22px; right: 22px; top: 626px">'
            '<div style="display: flex; align-items: baseline; justify-content: space-between">%s<a href="#" style="font: 400 19px/1 %s; color: %s">see all</a></div>'
            '<div style="position: relative; display: flex; justify-content: space-around; align-items: flex-end; margin-top: 6px; padding: 0 10px">%s%s</div></section>'
            % (marker('Your shelf', 20), HAND, INK, tape('lemon', 'left: -6px; right: -6px; bottom: -8px; height: 18px; transform: rotate(-.6deg); z-index: 0'),
               '<div style="position: relative; display: flex; justify-content: space-around; align-items: flex-end; width: 100%%">%s</div>' % toys))
    return page('Home, Sticker book look', root(head + hero + nextbox + shortcuts + peek + tabs('home'), DOTS))

MARKS = {'rare': ('#FFDE7A', 'star', 'Rare'), 'epic': ('#BFE3FF', 'gem', 'Epic'), 'secret': ('#2A1430', 'sparkle', 'Secret')}

def shelf():
    head = ('<div style="position: absolute; left: 22px; right: 20px; top: 52px; height: 46px; display: flex; align-items: center; gap: 12px">'
            '<h1 style="margin: 0">%s</h1>'
            '<span style="padding: 6px 10px 5px; font: 800 12px/1 %s; letter-spacing: .12em; text-transform: uppercase; color: %s; background: %s; border-radius: 3px; transform: rotate(-2deg)">53 toys</span>'
            '<span style="flex: 1"></span><button type="button" aria-label="Search">%s</button></div>' % (marker('Shelf', 32), F, CARD, INK, paper_round('search')))
    segs = ''
    for i, label in enumerate(['All', 'Fusions', 'Extras']):
        on = i == 0
        segs += ('<button type="button" style="flex: 1; height: %dpx; border-radius: 12px 12px 0 0; box-sizing: border-box; font: %s 15px/1 %s; color: %s; %s">%s</button>'
                 % (44 if on else 38, '800' if on else '700', F, INK,
                    'background: #FFAECB; border: 2px solid %s; border-bottom: 0' % INK if on else 'background: %s; border: 2px dashed rgba(42, 20, 48, .4); border-bottom: 0' % CARD, label))
    switch = ('<div role="group" aria-label="Show" style="position: absolute; left: 22px; right: 22px; top: 110px; height: 44px; display: flex; align-items: flex-end; gap: 6px; border-bottom: 2px solid %s">%s</div>' % (INK, segs))
    tapes = ['mint', 'pink', 'lemon', 'lilac']
    turns = [[-3, 2, -2], [3, -2, 4], [-2, 3, -3], [2, -4, 2]]
    rows = ''
    for r, row in enumerate(SHELF):
        toys = labels = ''
        for c, (name, pic, rarity, copies) in enumerate(row):
            extra = ('<span style="position: absolute; right: 2px; top: 2px; min-width: 28px; height: 28px; padding: 0 5px; box-sizing: border-box; border-radius: 14px; display: flex; align-items: center; justify-content: center; '
                     'font: 400 18px/1 %s; background: %s; border: 2px dashed rgba(42, 20, 48, .55); transform: rotate(8deg)">×%d</span>' % (HAND, CARD, copies)) if copies > 1 else ''
            mark = ''
            if rarity:
                colour, glyph, word = MARKS[rarity]
                mark = ('<span role="img" aria-label="%s" style="position: absolute; left: 4px; bottom: 6px; width: 26px; height: 26px; border-radius: 50%%; box-sizing: border-box; display: flex; align-items: center; justify-content: center; '
                        'background: %s; border: 3px solid #fff; box-shadow: 0 3px 5px rgba(90, 60, 30, .3); transform: rotate(-12deg)">%s</span>' % (word, colour, icon(glyph, 13, '#fff' if rarity == 'secret' else INK, 2.6)))
            toys += ('<div style="position: relative; width: 106px; height: 98px; display: flex; align-items: flex-end; justify-content: center">%s%s%s</div>'
                     % (img(pic, 'position: relative; width: 92px; height: 92px; object-fit: contain; transform: rotate(%ddeg); %s' % (turns[r][c], DIECUT), name), extra, mark))
            labels += '<span style="width: 106px; font: 400 17.5px/1.05 %s; text-align: center">%s</span>' % (HAND, name)
        rows += ('<div style="display: flex; flex-direction: column"><div style="position: relative; display: flex; justify-content: space-between; padding: 0 4px">%s'
                 '<div style="position: relative; display: flex; justify-content: space-between; width: 100%%">%s</div></div>'
                 '<div style="display: flex; justify-content: space-between; padding: 12px 4px 0">%s</div></div>'
                 % (tape(tapes[r], 'left: -8px; right: -8px; bottom: -9px; height: 18px; transform: rotate(%sdeg)' % ('-.5' if r % 2 == 0 else '.6')), toys, labels))
    grid = '<div style="position: absolute; left: 22px; right: 22px; top: 172px; display: flex; flex-direction: column; gap: 14px">%s</div>' % rows
    return page('Shelf, Sticker book look', root(head + switch + grid + tabs('shelf'), DOTS))

def play():
    top = ('<div style="position: absolute; left: 16px; right: 16px; top: 52px; height: 46px; display: flex; align-items: center; justify-content: space-between">'
           '<button type="button" aria-label="Back">%s</button>'
           '<button type="button" style="padding: 9px 13px 8px; font: 800 13px/1 %s; letter-spacing: .14em; text-transform: uppercase; color: %s; background: %s; border-radius: 3px; transform: rotate(-1.5deg); box-shadow: 0 4px 7px rgba(60, 30, 20, .3)">Pip · 9 of 15</button>'
           '<button type="button" aria-label="Favorite">%s</button></div>' % (paper_round('back'), F, CARD, INK, paper_round('heart', 44, 22, '#C2306E', '#F0528C')))
    title = ('<div style="position: absolute; left: 20px; top: 116px; padding: 16px 18px 12px; background: %s; border-radius: 6px; box-shadow: 0 8px 16px rgba(60, 30, 20, .26); transform: rotate(-2deg)">'
             '%s<h1 style="margin: 0; font: 800 32px/1 %s; letter-spacing: -.015em">Peach Jelly</h1>'
             '<p style="margin: 5px 0 0; font: 400 20px/1 %s; color: %s">sunny · curious</p></div>'
             % (CARD, tape('lemon', 'left: 50%; top: -12px; width: 76px; margin-left: -38px; transform: rotate(3deg)'), F, HAND, SOFT))
    toy = ('<div aria-hidden="true" style="position: absolute; left: 75px; top: 590px; width: 240px; height: 34px; border-radius: 50%; background: radial-gradient(ellipse, rgba(150, 50, 90, .34), rgba(150, 50, 90, 0) 70%)"></div>'
           + img('hero-peach', 'position: absolute; left: 45px; top: 314px; width: 300px; height: auto', 'Peach Jelly'))
    bubble = ('<div style="position: absolute; left: 238px; top: 268px; padding: 9px 18px 11px; border-radius: 22px 26px 24px 6px; font: 400 27px/1 %s; background: #fff; border: 2.5px solid %s; transform: rotate(4deg); box-shadow: 0 5px 0 rgba(42, 20, 48, .14)">yay!</div>' % (HAND, INK))
    colours = [('pink', -5), ('mint', 4), ('lemon', -3), ('lilac', 5), ('sky', -4)]
    controls = ''.join('<button type="button" style="flex: 1; min-width: 0; display: flex; flex-direction: column; align-items: center; gap: 3px">%s<span style="font: 400 18px/1 %s">%s</span></button>'
                       % (sticker(name, colours[i][0], 52, 25, colours[i][1]), HAND, label) for i, (name, label) in enumerate(CONTROLS))
    tray = ('<div role="group" aria-label="Play controls" style="position: absolute; left: 10px; right: 10px; bottom: 24px; height: 102px; box-sizing: border-box; padding: 4px 6px 0; display: flex; align-items: center; '
            'background: %s; border-radius: 8px; box-shadow: 0 10px 20px rgba(60, 30, 20, .3); transform: rotate(-.8deg)">%s%s%s</div>'
            % (CARD, tape('pink', 'left: 26px; top: -11px; width: 70px; transform: rotate(-6deg)'), tape('mint', 'right: 26px; top: -11px; width: 70px; transform: rotate(5deg)'), controls))
    hint = ('<div style="position: absolute; left: 0; right: 0; bottom: 142px; display: flex; justify-content: center"><span style="padding: 6px 13px 7px; border-radius: 4px; font: 400 18px/1 %s; background: rgba(255, 253, 247, .9); transform: rotate(1deg)">tap · drag to turn</span></div>' % HAND)
    body = img('world-jelly-garden', 'position: absolute; left: 0; top: 0; width: 390px; height: 844px; object-fit: cover') + top + title + toy + bubble + hint + tray
    return page('Play, Sticker book look', root(body, 'background: #F6C9D2;'))

BOARDS = [('B-Home.dc.html', 'B · Sticker book · Home', home), ('B-Shelf.dc.html', 'B · Sticker book · Shelf', shelf), ('B-Play.dc.html', 'B · Sticker book · Play', play)]
