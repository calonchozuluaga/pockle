#!/usr/bin/env python3
"""Lay out native Unity finish captures without modifying character pixels."""
import collections
import html
import json
from pathlib import Path
import sys
from PIL import Image, ImageDraw, ImageFont
root = Path(sys.argv[1])
groups = collections.defaultdict(list)
for entry in json.loads((root / 'catalog.json').read_text())['entries']:
    groups[entry['collection']].append(entry)
font = lambda size: ImageFont.truetype('/System/Library/Fonts/Supplemental/Arial.ttf', size)
colors = {'Common': '#e7e1db', 'Rare': '#ffdb7a', 'Epic': '#d6f1ff', 'Secret': '#172c38'}
(root / 'sheets').mkdir(exist_ok=True)
sections = []
for key, entries in groups.items():
    name = entries[0]['collectionName']
    sheet = Image.new('RGB', (1800, 140 + ((len(entries)+4)//5)*430), '#f4eee8')
    draw = ImageDraw.Draw(sheet)
    draw.text((32,20), f'{name} — collection finish review', font=font(38), fill='#39253d')
    draw.text((32,75), '9 Common / 3 Rare / 2 Epic / 1 Secret · Native Unity renders · November 8, 2026', font=font(23), fill='#72616f')
    cards = []
    for i, e in enumerate(entries):
        x = i%5*360
        y = 140+i//5*430
        draw.rounded_rectangle((x+8,y+4,x+352,y+420), radius=18, fill='#fffaf6')
        image_path = root/'renders'/f'{e["id"]}.png'
        im = Image.open(image_path).convert('RGBA')
        im.thumbnail((340,340), Image.Resampling.LANCZOS)
        sheet.paste(im,(x+(360-im.width)//2,y+2),im)
        label = e['finish']
        size = 23 if len(label)<23 else 20
        draw.text((x+18,y+344),label,font=font(size),fill='#39253d')
        draw.rounded_rectangle((x+17,y+380,x+117,y+409), radius=12,fill=colors[e['tier']])
        draw.text((x+28,y+384),e['tier'],font=font(18),fill='#7ff3ff' if e['tier']=='Secret' else '#39253d')
        cards.append(f'<figure data-tier="{e["tier"]}"><a href="renders/{e["id"]}.png"><img loading="lazy" src="renders/{e["id"]}.png" alt="{html.escape(e["name"])}"></a><figcaption><span class="name">{html.escape(label)}</span><small class="{e["tier"]}">{e["tier"]}</small></figcaption></figure>')
    sheet.save(root/'sheets'/f'{key}.jpg',quality=94,subsampling=0)
    sections.append(f'<section id="{key}"><h2>{name}</h2><a href="sheets/{key}.jpg">Full collection sheet ↗</a><div class="grid">'+''.join(cards)+'</div></section>')
# A compact cross-collection comparison of the Secret identities.
secrets = [e for entries in groups.values() for e in entries if e['tier'] == 'Secret']
contact = Image.new('RGB', (1600, ((len(secrets)+3)//4)*460), '#f4eee8')
draw = ImageDraw.Draw(contact)
for i, entry in enumerate(secrets):
    im = Image.open(root/'renders'/f'{entry["id"]}.png').convert('RGBA')
    im.thumbnail((390,390), Image.Resampling.LANCZOS)
    x, y = i%4*400, i//4*460
    contact.paste(im,(x,y),im)
    draw.text((x+15,y+385),entry['collectionName'],font=font(22),fill='#39253d')
    draw.text((x+15,y+417),entry['finish'],font=font(22),fill='#39253d')
contact.save(root/'secrets.jpg',quality=94,subsampling=0)
nav=''.join(f'<a href="#{key}">{entries[0]["collectionName"]}</a>' for key,entries in groups.items())
(root/'index.html').write_text('''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><title>Pockle · Rarity finish review</title><style>*{box-sizing:border-box}body{margin:0;background:#f4eee8;color:#39253d;font:17px system-ui}main{max-width:1600px;margin:auto;padding:32px}h1{font-size:42px;margin-bottom:10px}p{max-width:1000px;line-height:1.6}nav{display:flex;gap:10px;flex-wrap:wrap;margin:24px 0}a{color:#67416b}nav a,button{padding:9px 16px;background:white;border:0;border-radius:24px;text-decoration:none;font:inherit;color:inherit;cursor:pointer}section{padding:25px 0;scroll-margin:20px}h2{font-size:30px;margin-bottom:7px}.grid{display:grid;grid-template-columns:repeat(5,minmax(0,1fr));gap:12px;margin-top:16px}figure{margin:0;background:#fffaf6;border-radius:18px;overflow:hidden}img{width:100%;display:block}figcaption{padding:8px 16px 18px;font-weight:600}small{display:table;padding:4px 12px;border-radius:16px;font-weight:500;margin-top:8px;background:#e7e1db}.Rare{background:#ffdb7a}.Epic{background:#d6f1ff}.Secret{background:#172c38;color:#7ff3ff}aside{padding:18px 22px;background:#e7eee2;border-radius:18px;max-width:1100px}.hidden img{filter:brightness(0);opacity:.25}.hidden .name{visibility:hidden}.hidden figure a{pointer-events:none}@media(max-width:850px){.grid{grid-template-columns:repeat(2,minmax(0,1fr))}main{padding:18px}h1{font-size:32px}}</style><main><h1>A distinct finish at every tier</h1><p>11 collections · 165 finishes. Existing toy IDs are preserved. Glow is Epic; each collection has its own Secret identity. Wally retains its approved pilot materials. This is a local visual review, not a change to live purchase odds or a phone update.</p><p><a href="secrets.jpg">Compare all eleven Secret finishes ↗</a></p><aside>Static native material captures. Moving reflections, pulsing light and transparency are best judged in the interactive viewer. <button id="toggle" aria-pressed="false">Preview hidden toys</button> Rarity remains visible when a toy is hidden.</aside><nav>'''+nav+'</nav>'+''.join(sections)+'''</main><script>document.querySelector('#toggle').onclick=function(){const on=document.body.classList.toggle('hidden');this.setAttribute('aria-pressed',String(on));this.textContent=on?'Show finishes':'Preview hidden toys';};</script></html>''')
print(f'Built {len(groups)} sheets / {sum(map(len,groups.values()))} finishes')
