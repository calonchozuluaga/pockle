"""Shared pieces for the three Pockle look boards. Everything is inline-styled markup for a 390 x 844 artboard."""
import json, os

HERE = os.path.dirname(os.path.abspath(__file__))
ASSETS = {}

def load_assets(mapping):
    ASSETS.clear(); ASSETS.update(mapping)

def A(name):
    return ASSETS[name]

F = "'Bricolage Grotesque', 'Avenir Next', 'Segoe UI', sans-serif"
HAND = "'Patrick Hand', 'Comic Sans MS', cursive"
PLUM, SOFT, MILK = '#2A1430', '#5E4A63', '#FBF8F5'
W, H = 390, 844

# 24 x 24 stroke icons, drawn for these boards.
ICONS = {
    'home': '<path d="M4 11.2 12 4l8 7.2V19a1.5 1.5 0 0 1-1.5 1.5H15v-5.5H9v5.5H5.5A1.5 1.5 0 0 1 4 19z"/>',
    'shelf': '<path d="M3.5 10.5h17M3.5 19.5h17"/><circle cx="8" cy="7" r="2.6"/><rect x="13" y="4.6" width="5" height="5" rx="1.4"/><rect x="6" y="13.6" width="5" height="5" rx="1.4"/><circle cx="16" cy="16" r="2.6"/>',
    'walk': '<ellipse cx="8" cy="8" rx="2.7" ry="4" transform="rotate(-10 8 8)"/><ellipse cx="9.2" cy="15.3" rx="1.7" ry="1.8"/><ellipse cx="16" cy="11.5" rx="2.7" ry="4" transform="rotate(10 16 11.5)"/><ellipse cx="14.8" cy="18.800" rx="1.7" ry="1.8"/>',
    'community': '<circle cx="9" cy="8.5" r="3.2"/><path d="M3 19.500c0-3.200 2.700-5.300 6-5.300s6 2.100 6 5.300"/><circle cx="16.800" cy="9.300" r="2.500"/><path d="M16.500 14.300c2.600.100 4.700 1.900 4.700 4.700"/>',
    'shop': '<path d="M4 9.200 5.600 5h12.800L20 9.200a2.650 2.650 0 0 1-5.300 0 2.700 2.700 0 0 1-5.400 0A2.650 2.650 0 0 1 4 9.200z"/><path d="M5.500 12v8h13v-8M10 20v-4.500h4V20"/>',
    'box': '<path d="M12 3.200 20 7.500v9L12 20.800 4 16.500v-9z"/><path d="M4 7.500l8 4.300 8-4.300M12 11.800v9"/>',
    'fuse': '<circle cx="9" cy="12" r="5.500"/><circle cx="15" cy="12" r="5.500"/>',
    'badge': '<circle cx="12" cy="9.500" r="5.500"/><path d="M8.700 14 7.300 21l4.700-2.400 4.700 2.400L15.300 14"/><path d="M12 7.200l.8 1.600 1.700.2-1.300 1.200.4 1.700-1.600-.9-1.600.9.4-1.700-1.300-1.200 1.700-.2z"/>',
    'toy': '<path d="M12 4.500c4.300 0 7.800 5.600 7.800 10 0 3.300-3 5-7.800 5s-7.800-1.700-7.800-5c0-4.400 3.500-10 7.800-10z"/><path d="M9.300 13.200v.6M14.700 13.200v.6M10.600 16.200c.9.700 1.900.7 2.800 0"/>',
    'search': '<circle cx="11" cy="11" r="6.200"/><path d="M20 20l-4.400-4.400"/>',
    'heart': '<path d="M12 20s-7.200-4.500-7.200-9.700A4 4 0 0 1 12 8a4 4 0 0 1 7.200 2.300C19.200 15.500 12 20 12 20z"/>',
    'back': '<path d="M14.500 5.500 8 12l6.500 6.500"/>',
    'backdrop': '<rect x="3.800" y="5" width="16.400" height="14" rx="3"/><path d="M4 16.200l4.400-4.400 3.600 3.600 2.600-2.600L20 17"/><circle cx="15.600" cy="9.400" r="1.400"/>',
    'light': '<circle cx="12" cy="12" r="4"/><path d="M12 3v2.400M12 18.600V21M3 12h2.400M18.600 12H21M5.600 5.600l1.700 1.700M16.700 16.700l1.700 1.700M5.600 18.400l1.700-1.700M16.700 7.300l1.700-1.700"/>',
    'about': '<circle cx="12" cy="12" r="8.500"/><path d="M12 11v5.500M12 7.800v.2"/>',
    'sound': '<path d="M4.500 10v4H8l4.500 3.800V6.200L8 10z"/><path d="M16 9.300a3.800 3.800 0 0 1 0 5.400M18.400 7.200a6.800 6.800 0 0 1 0 9.600"/>',
    'star': '<path d="M12 3.800l2.400 5.100 5.600.7-4.100 3.800 1.100 5.500L12 16.200l-5 2.700 1.100-5.500L4 9.600l5.600-.7z"/>',
    'gem': '<path d="M7.500 5h9l4 5-8.500 9.500L3.500 10z"/><path d="M3.500 10h17M9.500 5l-1.500 5 4 9.500 4-9.500-1.500-5"/>',
    'sparkle': '<path d="M12 3.500l1.900 5.600 5.600 1.900-5.600 1.900L12 18.500l-1.900-5.600L4.500 11l5.600-1.900z"/>',
    'tick': '<path d="M5.500 12.500l4.200 4.200 8.800-9.200"/>',
}

def icon(name, size=24, color='#fff', sw=2.2, fill='none', style=''):
    return ('<svg aria-hidden="true" width="%d" height="%d" viewBox="0 0 24 24" fill="%s" stroke="%s" stroke-width="%s" stroke-linecap="round" '
            'stroke-linejoin="round" style="display: block; flex: none; %s">%s</svg>' % (size, size, fill, color, sw, style, ICONS[name]))

def img(name, style, alt=''):
    return '<img src="%s" alt="%s" style="%s">' % (A(name), alt, style)

def ring(size, pct, track, bar, width=6, extra=''):
    r = (size - width) / 2.0; c = 2 * 3.14159265 * r
    return ('<svg aria-hidden="true" width="%d" height="%d" viewBox="0 0 %d %d" style="display: block; flex: none; transform: rotate(-90deg); %s">'
            '<circle cx="%s" cy="%s" r="%.2f" fill="none" stroke="%s" stroke-width="%d"/>'
            '<circle cx="%s" cy="%s" r="%.2f" fill="none" stroke="%s" stroke-width="%d" stroke-linecap="round" stroke-dasharray="%.1f %.1f"/></svg>'
            % (size, size, size, size, extra, size / 2.0, size / 2.0, r, track, width, size / 2.0, size / 2.0, r, bar, width, c * pct, c))

FONTS = ('<link rel="preconnect" href="https://fonts.googleapis.com">'
         '<link href="https://fonts.googleapis.com/css2?family=Bricolage+Grotesque:opsz,wght@12..96,500;12..96,700;12..96,800&family=Patrick+Hand&display=swap" rel="stylesheet">')

def page(title, body, extra_css=''):
    return '''<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<title>%s</title>
<script src="./support.js"></script>
</head>
<body>
<x-dc>
<helmet>
%s
<style>
body{margin:0}
button{font:inherit;color:inherit;border:0;padding:0;margin:0;background:none;cursor:pointer}
a{color:#2A1430}a:hover{color:#5E4A63}
%s
</style>
</helmet>
%s
</x-dc>
<script type="text/x-dc" data-dc-script data-props='{"$preview":{"width":390,"height":844}}'>
class Component extends DCLogic {
renderVals() {
return {};
}
}
</script>
</body>
</html>
''' % (title, FONTS, extra_css, body)

TABS = [('home', 'Home'), ('shelf', 'Shelf'), ('walk', 'Walk'), ('community', 'Community'), ('shop', 'Shop')]
SHORTCUTS = [('toy', 'Play'), ('fuse', 'Fuse'), ('shop', 'Shop'), ('badge', 'Badges')]
CONTROLS = [('toy', 'Pockles'), ('backdrop', 'Backdrop'), ('light', 'Light'), ('about', 'About'), ('sound', 'Sound')]

# name, picture, rarity, copies
SHELF = [
    [('Peach Jelly', 'toy-pip_peach-jelly', '', 3), ('Amber Jelly', 'toy-fox_clear-jelly', '', 3), ('Honey Jelly', 'toy-axolotl_clear-jelly', '', 3)],
    [('Lagoon Jelly', 'toy-wally_clear-jelly', '', 3), ('Cranberry Jelly', 'toy-firb_clear-jelly', '', 1), ('Ice Lagoon', 'toy-guin_clear-jelly', '', 3)],
    [('Caramel Jelly', 'toy-coco_clear-jelly', '', 2), ('Polished Gold', 'toy-pip_coated-metallic', 'rare', 1), ('Diamond Prism', 'toy-firb_diamond-prism', 'epic', 2)],
    [('Violet Flock', 'toy-fox_velvet-flock', '', 2), ('Glacier Jelly', 'toy-snowy_clear-jelly', '', 3), ('Aurora Evergreen', 'toy-firb_secret-edition', 'secret', 1)],
]
PEEK = ['toy-pip_peach-jelly', 'toy-fox_clear-jelly', 'toy-axolotl_clear-jelly']
