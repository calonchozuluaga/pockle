#!/usr/bin/env python3
"""Build a deterministic, review-only 55-pair manifest and local HTML gallery. No runtime/catalog writes."""
from pathlib import Path
import json, itertools, math, argparse
ROOT=Path(__file__).resolve().parents[2]
PARENTS=[
('pip.peach-jelly','Peach Jelly','jelly','#f5ad91','Curious','Leans in, then gives a little hop'),
('pip.moon-pearl','Moon Pearl','pearl','#91aedf','Dreamy','Slow curious tilts and soft blinks'),
('pip.gold-confetti','Gold Glitter','glitter','#edba55','Theatrical','Happy poses and lively wiggles'),
('pip.mint-mochi','Mint Soft','foam','#92cba4','Calm','Small wiggles with gentle settling'),
('pip.velvet-flock','Velvet Flock','flock','#bfa1df','Gentle','Shy tilts and small thank-you bows'),
('pip.boucle-plush','Bouclé Plush','plush','#f4b3c4','Affectionate','Warm greeting hops and long bows'),
('pip.matte-vinyl','Matte Vinyl','matte','#c6e0f4','Steady','Measured head tilts and held poses'),
('pip.gloss-vinyl','Gloss Vinyl','gloss','#f9809e','Playful','Quick peekaboo and happy wiggles'),
('pip.coated-metallic','Coated Metallic','metal','#dca149','Confident','Deliberate poses and bigger bows'),
('pip.glow-jelly','Starlight Glow','glow','#67c7d1','Wonderstruck','Stargazing tilts and gentle surprise')]
SPECIAL={(0,1):('Dusk','pip.fusion-dusk','Pearl jelly','Curious + dreamy'),(0,3):('Sorbet','pip.fusion-sorbet','Soft marbled gel','Curious + calm'),(2,3):('Honeydew','pip.fusion-honeydew','Satin sparkle gel','Playful + composed'),(1,2):('Eclipse','pip.fusion-eclipse','Luminous crystal jelly','Dreamy + theatrical')}
def rgb(h):return [int(h[i:i+2],16)/255 for i in (1,3,5)]+[1]
def recipe(a,b):
    x,y=PARENTS[a],PARENTS[b]; types={x[2],y[2]};same=a==b
    r=dict(key=f'{a:02}-{b:02}',a=a,b=b,name=f'{x[1]} + {y[1]}',specialId='',status='Proposed rule',family='',surface='',pattern='Gradient',personality=f'{x[4]} + {y[4]}' if not same else f'Extra {x[4].lower()}',behavior=f'{x[5]}; {y[5][0].lower()+y[5][1:]}' if not same else x[5]+' with a slightly stronger signature',colorA=rgb(x[3]),colorB=rgb(y[3]),mode=1,roughness=.22,reflection=.60,opacity=.70,transmission=.65,pearl=.40 if 'pearl' in types else 0,glitter=.40 if 'glitter' in types else 0,fuzz=0,metal=0,glow=.55 if 'glow' in types else 0,iridescence=.18 if 'glow' in types else 0,zwrite=0)
    textile=bool(types&{'flock','plush'}); rigid=bool(types&{'matte','gloss','metal'})
    if same:
        r.update(family='Same finish',surface='Deepened '+x[1],pattern='Single color',mode=0)
        r['colorA']=[c*.90 for c in r['colorA'][:3]]+[1];r['colorB']=r['colorA']
        if x[2] in {'foam','flock','plush','matte','gloss','metal'}:r.update(opacity=1,zwrite=1,transmission=.04)
        if textile:r.update(fuzz=.90,roughness=.38,reflection=.14)
        elif x[2] in {'foam','matte'}:r.update(roughness=.37,reflection=.20)
        elif x[2] in {'gloss','metal'}:r.update(roughness=.13,reflection=.88,metal=.85 if x[2]=='metal' else 0)
        if 'glitter' in types:r['glitter']=1
        if 'pearl' in types:r['pearl']=.90
        if 'glow' in types:r['glow']=.85
    elif textile:
        r.update(family='Textile hybrids',surface='Flocked vinyl' if rigid else 'Velvet plush' if types<= {'flock','plush'} else 'Velvet-coated soft gel',pattern='Marbling',mode=2,fuzz=.65,roughness=.36,reflection=.18,opacity=1,zwrite=1,transmission=.06,glitter=min(r['glitter'],.2),pearl=min(r['pearl'],.15),metal=.20 if 'metal' in types else 0)
    elif 'foam' in types:
        r.update(family='Soft-touch hybrids',surface='Soft-touch coated shell' if rigid else 'Marbled soft gel',pattern='Marbling',mode=2,roughness=.35,reflection=.24,opacity=1,zwrite=1,transmission=.08,metal=.18 if 'metal' in types else 0,glitter=min(r['glitter'],.3))
    elif rigid:
        both=all(t in {'matte','gloss','metal'} for t in types)
        r.update(family='Coated hybrids',surface='Two-tone coated vinyl' if both else 'Jelly-glazed coated shell',roughness=.18 if 'gloss' in types or 'metal' in types else .30,reflection=.75 if 'gloss' in types or 'metal' in types else .42,opacity=1 if both else .92,zwrite=1 if both else 0,transmission=.04 if both else .20,metal=.55 if 'metal' in types else 0)
    else:r.update(family='Jelly hybrids',surface='Pearl sparkle jelly' if {'pearl','glitter'}<=types else 'Pearl jelly' if 'pearl' in types else 'Sparkle jelly' if 'glitter' in types else 'Layered jelly')
    if 'glow' in types:r['surface']='Luminous '+r['surface'][0].lower()+r['surface'][1:]
    if (a,b) in SPECIAL:
        name,id,surface,traits=SPECIAL[a,b];r.update(name=name,specialId=id,status='Existing phone study',surface=surface,personality=traits,family='Existing studies')
        if name=='Dusk':r.update(mode=1,roughness=.22,reflection=.64,opacity=.64,pearl=.60,glitter=0,glow=0,pattern='Gradient')
        if name=='Sorbet':r.update(mode=2,roughness=.37,reflection=.20,opacity=1,transmission=.08,zwrite=1,pearl=0,glow=0,pattern='Marbling')
        if name=='Honeydew':r.update(mode=3,roughness=.30,reflection=.34,opacity=.83,transmission=.30,glitter=.38,pattern='Jade with honey accents')
        if name=='Eclipse':r.update(status='Secret recipe candidate',mode=4,roughness=.18,reflection=.72,opacity=.78,pearl=.32,glitter=.30,glow=.60,pattern='Gold ribbons')
    return r

def build(out):
    out.mkdir(parents=True,exist_ok=True);(out/'renders').mkdir(exist_ok=True)
    parents=[dict(id=x[0],name=x[1],kind=x[2],color=x[3],trait=x[4]) for x in PARENTS]
    recipes=[recipe(a,b) for a,b in itertools.combinations_with_replacement(range(10),2)]
    assert len(recipes)==55 and len({r['key'] for r in recipes})==55
    assert sum(r['a']==r['b'] for r in recipes)==10 and sum(bool(r['specialId']) for r in recipes)==4
    for r in recipes:
        assert 0<=r['a']<=r['b']<10
        for k in ['roughness','reflection','opacity','transmission','pearl','glitter','fuzz','metal','glow','iridescence']:assert math.isfinite(r[k]) and 0<=r[k]<=1,(r['key'],k)
    data=dict(version=1,scope='Ten original Pip finishes; unordered pairs including same-finish pairs. Review proposals only.',parents=parents,recipes=recipes)
    (out/'recipes.json').write_text(json.dumps(data,indent=2)+'\n')
    template=(Path(__file__).with_name('gallery-template.html')).read_text()
    (out/'index.html').write_text(template.replace('__DATA__',json.dumps(data,ensure_ascii=False)))
    print(f'Built 55 unique unordered pairs, 10 same-finish pairs, 4 existing studies: {out}')
if __name__=='__main__':
    ap=argparse.ArgumentParser();ap.add_argument('--out',type=Path,default=ROOT/'docs/fusion-review/pip-01');args=ap.parse_args();build(args.out)
