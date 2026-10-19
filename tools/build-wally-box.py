#!/usr/bin/env python3
"""Reuse the approved real hinged carton with Wally's original ocean prints."""
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
template=ROOT/'tools/build-pip-box.py'
source=template.read_text().replace('Pip','Wally').replace('pip','wally').replace('PIP','WALLY')
source=source.replace("'WallyBox · ivory paper',(1,.84,.74)","'WallyBox · aqua paper',(.74,.95,1)")
source=source.replace('Unity FBX: Resources/Boxes/','Unity FBX: Assets/Pockle/Art/Models/WallyBox/')
exec(compile(source,str(template),'exec'),{'__file__':str(template),'__name__':'__main__'})
