#!/usr/bin/env python3
"""Reuse the approved hinged carton construction for four original Christmas prints."""
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
template=ROOT/'tools/build-pip-box.py'
for key in ['firb','coco','snowy','ginger']:
 title=key.title()
 source=template.read_text().replace('Pip',title).replace('pip',key).replace('PIP',key.upper())
 exec(compile(source,str(template),'exec'),{'__file__':str(template),'__name__':'__main__'})
