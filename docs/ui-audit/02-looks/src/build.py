#!/usr/bin/env python3
"""build.py <asset-map.json> <out-dir>: writes project/*.dc.html and project/canvas.json for the Pockle looks canvas."""
import json, os, sys, datetime
import common, look_a, look_b, look_c

mapping = json.load(open(sys.argv[1])); out = sys.argv[2]
common.load_assets(mapping)
os.makedirs(os.path.join(out, 'project'), exist_ok=True)
rows = [('A · Jelly', look_a.BOARDS), ('B · Sticker book', look_b.BOARDS), ('C · Toy shop', look_c.BOARDS)]
boards, order, notes = {}, [], {}
ROW_H = 844 + 420
for r, (name, items) in enumerate(rows):
    y = r * ROW_H
    notes['look%d' % r] = {'x': 0, 'y': y - 300, 'text': name, 'kind': 'title1', 'maxW': 3 * 390 + 2 * 80}
    for c, (path, title, make) in enumerate(items):
        open(os.path.join(out, 'project', path), 'w').write(make())
        boards[path] = {'x': c * (390 + 80), 'y': y, 'w': 390, 'h': 844, 'title': title, 'radius': 44}
        order.append(path)
index = {'v': 3, 'createdOnFiles': {'v': 1, 'at': datetime.datetime.utcnow().strftime('%Y-%m-%dT%H:%M:%SZ')}, 'title': 'Pockle looks', 'launch': {'view': 'canvas'},
         'pages': [], 'boards': boards, 'order': order, 'notes': notes, 'designSystems': []}
json.dump(index, open(os.path.join(out, 'project', 'canvas.json'), 'w'), indent=1, ensure_ascii=False)
print('wrote', len(order), 'boards')
