#!/usr/bin/env python3
"""Stage downloaded, original Magnific MP3s and build the local audition page.
No generation/API calls. Match exported ID3 titles against the approved prompt manifest.
"""
import argparse, hashlib, html, json, shutil, struct, zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PLAN = ROOT / 'ArtSource/ReactionAudio01/manifest.json'
OUT = ROOT / 'docs/audio-review/reactions-01'

def id3(data):
    if data[:3] != b'ID3': return 0, {}
    version = data[3]
    size = sum(n << shift for n, shift in zip(data[6:10], (21, 14, 7, 0)))
    end = 10 + size + (10 if data[5] & 16 else 0)
    tags, cursor = {}, 10
    while cursor + 10 <= 10 + size:
        key = data[cursor:cursor+4].decode('ascii', errors='replace')
        raw = data[cursor+4:cursor+8]
        n = sum(x << shift for x, shift in zip(raw, (21,14,7,0))) if version == 4 else int.from_bytes(raw, 'big')
        if not key.strip('\0') or n <= 0: break
        value = data[cursor+10:cursor+10+n]
        if key.startswith('T') and value:
            enc = {0:'latin1',1:'utf-16',2:'utf-16-be',3:'utf8'}.get(value[0], 'utf8')
            tags[key] = value[1:].decode(enc, errors='replace').strip('\0')
        cursor += n + 10
    return end, tags

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--import-zip', type=Path)
    args = parser.parse_args()
    plan = json.loads(PLAN.read_text())
    sources = PLAN.parent / 'sources'; sources.mkdir(exist_ok=True)
    if args.import_zip:
        with zipfile.ZipFile(args.import_zip) as archive:
            for member in archive.infolist():
                if not member.filename.lower().endswith('.mp3'): continue
                data = archive.read(member)
                _, tags = id3(data)
                title = tags.get('TIT2', '')
                matches = [e for e in plan['entries'] if title and e['prompt'].startswith(title)]
                if len(matches) != 1: raise ValueError(f'Cannot uniquely match exported clip {member.filename}: {title}')
                target = sources / (matches[0]['id'] + '.mp3')
                if target.exists() and target.read_bytes() != data:
                    raise ValueError(f'Duplicate different take for {target.name}; review explicitly.')
                target.write_bytes(data)
                matches[0]['exportName'] = member.filename
    (OUT/'clips').mkdir(parents=True, exist_ok=True)
    complete = 0
    for entry in plan['entries']:
        path = PLAN.parent / entry['source']
        if not path.exists(): continue
        data = path.read_bytes(); offset, tags = id3(data)
        # Remove only ID3 artwork/metadata from review copies; MPEG frames stay byte-identical.
        (OUT/'clips'/(entry['id']+'.mp3')).write_bytes(data[offset:])
        entry['sha256'] = hashlib.sha256(data).hexdigest()
        entry['mpegSha256'] = hashlib.sha256(data[offset:]).hexdigest()
        if entry['status'] not in ('approved-existing','approved'): entry['status'] = 'generated'
        complete += 1
    PLAN.write_text(json.dumps(plan,indent=2)+'\n')
    esc=html.escape
    body = []
    for character in dict.fromkeys(e['character'] for e in plan['entries']):
        body.append(f'<section id="{character}"><h2>{esc(character.title())}</h2><div class="grid">')
        for e in (e for e in plan['entries'] if e['character'] == character):
            ready = e['status'] in ('generated','approved-existing','approved')
            body.append(f'<article data-kind="{e["kind"]}"><small>{esc(e["kind"].title())} · {esc(e["status"])}</small><h3>{esc(e["label"])}</h3>')
            body.append(f'<audio controls preload="none" src="clips/{e["id"]}.mp3"></audio>' if ready else '<p class="pending">Generation pending</p>')
            body.append(f'<details><summary>Sound brief</summary><p>{esc(e["prompt"])}</p></details></article>')
        body.append('</div></section>')
    all_approved = all(e['status'] == 'approved' for e in plan['entries'])
    banner = 'Approved · imported locally' if all_approved else 'Audition review · not imported'
    review_note = 'Owner approved all 143 cues on November 8. Imported into local Unity Resources; phone build pending.' if all_approved else 'These are audition takes; generated does not mean approved.'
    nav=''.join(f'<a href="#{c}">{c.title()}</a>' for c in dict.fromkeys(e['character'] for e in plan['entries']))
    document='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Pockle reaction sounds · 01</title><style>
    *{box-sizing:border-box}body{margin:0;background:#fff5ed;color:#35243d;font:16px system-ui,sans-serif}main{max-width:1250px;margin:auto;padding:36px 22px}h1{font-size:clamp(30px,5vw,52px);margin:10px 0}h2{font-size:30px;margin:36px 0 16px}p{line-height:1.6}.intro{max-width:850px}nav{display:flex;flex-wrap:wrap;gap:10px;margin:24px 0}nav a{background:#eedfe9;border-radius:25px;padding:10px 16px;color:inherit;text-decoration:none}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(270px,1fr));gap:15px}article{background:#fff;border:1px solid #e7d6df;border-radius:20px;padding:20px;min-width:0}small{font-size:12px;color:#70586f}h3{margin:10px 0 18px;font-size:20px}audio{width:100%;height:42px}details{margin-top:14px;font-size:13px}summary{cursor:pointer}.pending{color:#8d718a}header>strong{display:inline-block;background:#f5d6ae;padding:8px 14px;border-radius:20px}footer{padding-top:38px;color:#70586f}
    </style><main><header><strong>'''+banner+'''</strong><h1>Every Pockle has a voice</h1>'''+f'<p class="intro">{complete} of 143 slots have audio: five action cues and eight temperament cues for each of the eleven collections. Nine approved pilot clips retain their complete original endings. New ElevenLabs clips use your existing Magnific credits; no live AI calls during play.</p><p class="intro">Listen for a consistent character voice, clear differences between actions and emotions, and a natural ending. {review_note} Only one preview plays at a time.</p></header><nav>{nav}</nav>'+''.join(body)+'''<footer>Source prompts, hashes and generation settings: ArtSource/ReactionAudio01/manifest.json. Runtime cutoff fixes are tested separately from the approval of these new takes.</footer></main><script>document.addEventListener('play',e=>{if(e.target.tagName==='AUDIO')document.querySelectorAll('audio').forEach(a=>{if(a!==e.target)a.pause()})},true)</script></html>'''
    (OUT/'index.html').write_text(document)
    print(f'Review ready: {complete}/143 clips; {OUT / "index.html"}')

if __name__ == '__main__': main()
