#!/usr/bin/env python3
from pathlib import Path
import json, math
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'docs/personality-review/pip-02'
def pose(**kw):
 p=dict(blink=0,happy=0,surprise=0,lids=0,frown=0,glanceX=0,glanceY=0,blush=0,wink=0,lidAngle=0,sleepCurve=0,mouthOpen=0,mouthTilt=0,highlightStyle=0,mouthScale=1);assert set(kw)<=set(p);p.update(kw);return p
rows=[
('sunny','Sunny','Open and welcoming','A bright greeting smile','Friendly smile',pose(happy=.15,blush=.12),pose(happy=.80,lids=.10,blush=.25),pose(happy=.55,glanceX=.20,blush=.20)),
('grumpy','Grumpy','Inward-angled half lids, pout, looking away','A stronger pout and side glance','Still trying not to smile',pose(lids=.55,lidAngle=.65,frown=1,glanceX=.55),pose(lids=.70,lidAngle=.8,frown=1,glanceX=.85),pose(lids=.35,lidAngle=.5,happy=.10,glanceX=.65,blush=.40,mouthScale=.68)),
('shy','Shy','Small smile, lowered gaze, blush','Looks down and away','Peeks back with a shy smile',pose(lids=.15,glanceX=-.55,glanceY=-.55,blush=.55,mouthScale=.72),pose(lids=.35,glanceX=-.90,glanceY=-.70,blush=.90,mouthScale=.65),pose(happy=.30,glanceX=.25,glanceY=.15,blush=.75,mouthScale=.80)),
('show-off','Show-off','Wide confident grin and double glints','A bigger delighted grin','A curved camera-ready wink',pose(happy=.75,mouthScale=1.15,highlightStyle=1),pose(happy=1,mouthScale=1.25,highlightStyle=1,blush=.20),pose(happy=.60,wink=1,glanceX=.15,blush=.15,highlightStyle=1,mouthScale=1.15)),
('dreamy','Dreamy','Flat soft lids and visible upward gaze','A quiet round-mouthed surprise','Stargazing with star-shaped glints',pose(lids=.30,glanceY=.85,mouthScale=.65),pose(lids=.15,glanceY=1,surprise=.30,mouthOpen=1,mouthScale=.70),pose(lids=.20,glanceY=1,happy=.15,mouthScale=.70,highlightStyle=2)),
('sleepy','Sleepy','Drooping outer lids and a small relaxed mouth','Briefly startled awake with a round o','Eyes drift into sleeping curves',pose(lids=.80,lidAngle=-.65,mouthScale=.65),pose(lids=.05,lidAngle=-.15,surprise=.65,mouthOpen=1),pose(blink=1,lids=.80,lidAngle=-.65,sleepCurve=1,mouthScale=.65)),
('mischief','Mischief','Crooked grin, inward lids and side glance','A cheeky sideways grin','A curved wink with a crooked smile',pose(lids=.28,lidAngle=.65,happy=.35,glanceX=.85,mouthTilt=.8),pose(lids=.40,lidAngle=.75,happy=.65,glanceX=1,mouthTilt=.85,blush=.1),pose(happy=.35,lids=.20,lidAngle=.55,wink=1,glanceX=.85,mouthTilt=.8)),
('calm','Calm','Flat relaxed lids and a gentle smile','A small reassuring smile','A slow contented curved blink',pose(lids=.20,happy=.06,mouthScale=.90),pose(lids=.20,happy=.28,blush=.12,mouthScale=.95),pose(blink=1,lids=.20,happy=.18,mouthScale=.90))]
OUT.mkdir(parents=True,exist_ok=True)
styles=[]
for id,name,restNote,pokeNote,signatureNote,rest,poke,signature in rows:
 for p in [rest,poke,signature]:
  for key,value in p.items():
   assert math.isfinite(value)
   assert (-1<=value<=1 if key.startswith('glance') or key in ['lidAngle','mouthTilt'] else .5<=value<=1.5 if key=='mouthScale' else 0<=value<=2 if key=='highlightStyle' else 0<=value<=1)
 styles.append(dict(id=id,name=name,restNote=restNote,pokeNote=pokeNote,signatureNote=signatureNote,rest=rest,poke=poke,signature=signature))
assert len(styles)==8 and len({s['id'] for s in styles})==8
(OUT/'poses.json').write_text(json.dumps(dict(version=2,styles=styles),indent=2)+'\n')
(OUT/'index.html').write_text(Path(__file__).with_name('gallery-template.html').read_text().replace('__DATA__',json.dumps(styles)))
print('Built eight temperament studies: 24 rest/tap/signature poses; no personality runtime or material changes.')
