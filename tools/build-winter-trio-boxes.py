#!/usr/bin/env python3
"""Reuse the approved hinged carton construction for three winter-trio prints."""
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
template=ROOT/'tools/build-pip-box.py'
for key in ['reiny','guin','bundy']:
 title=key.title()
 source=template.read_text().replace('Pip',title).replace('pip',key).replace('PIP',key.upper())
 source=source.replace('Unity FBX: Resources/Boxes/',f'Unity FBX: Assets/Pockle/Art/Models/{title}Box/')
 exec(compile(source,str(template),'exec'),{'__file__':str(template),'__name__':'__main__'})
