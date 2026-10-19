#!/usr/bin/env python3
"""Resample saved Wally originals into the runtime art sizes without changing originals."""
from pathlib import Path
from PIL import Image
import json,shutil
r=Path(__file__).resolve().parents[1];manifest=json.loads((r/'ArtSource/WallyArt/generated-manifest.json').read_text())
for item in manifest:
 original=r/item['original'] # Use the preserved project original, never a transient generation path.
 key=item['id'];image=Image.open(original)
 if key=='cover':path=r/'Assets/Pockle/Resources/Collections/Wally/Cover.png';size=(2160,720)
 elif key.startswith('box-'):path=r/f'Assets/Pockle/Resources/Boxes/Wally{key.split("-")[1].title()}.png';size=(1024,1024)
 else:path=r/f'Assets/Pockle/Resources/Worlds/wally-{key}.png';size=(2048,1024)
 path.parent.mkdir(parents=True,exist_ok=True);image.resize(size,Image.Resampling.LANCZOS).save(path)
 print(key,image.size,'->',size)
