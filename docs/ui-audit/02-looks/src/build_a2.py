#!/usr/bin/env python3
"""build_a2.py <asset-map.json> <out-dir>: writes the three A2 (Jelly glass) boards under <out-dir>/project."""
import json, os, sys
import common, look_a2
common.load_assets(json.load(open(sys.argv[1]))); out = os.path.join(sys.argv[2], 'project'); os.makedirs(out, exist_ok=True)
for path, title, make in look_a2.BOARDS:
    open(os.path.join(out, path), 'w').write(make())
print('wrote', [b[0] for b in look_a2.BOARDS])
