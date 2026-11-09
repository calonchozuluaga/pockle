#!/usr/bin/env python3
"""Build moment auditions from browser-downloaded Magnific sources; no network calls."""
import hashlib
import html
import json
from pathlib import Path
import subprocess
import tempfile
import wave

import numpy as np

BASE = Path(__file__).resolve().parent
ROOT = BASE.parents[3]
OUT = ROOT / 'docs/moment-audio-review/01'


def id3(data):
    if data[:3] != b'ID3':
        return 0, {}
    version = data[3]
    size = sum(n << shift for n, shift in zip(data[6:10], (21, 14, 7, 0)))
    end = 10 + size + (10 if data[5] & 16 else 0)
    tags, cursor = {}, 10
    while cursor + 10 <= 10 + size:
        key = data[cursor:cursor+4].decode('ascii', errors='replace')
        raw = data[cursor+4:cursor+8]
        n = sum(x << shift for x, shift in zip(raw, (21, 14, 7, 0))) if version == 4 else int.from_bytes(raw, 'big')
        if not key.strip('\0') or n <= 0:
            break
        value = data[cursor+10:cursor+10+n]
        if key.startswith('T') and value:
            encoding = {0: 'latin1', 1: 'utf-16', 2: 'utf-16-be', 3: 'utf8'}.get(value[0], 'utf8')
            tags[key] = value[1:].decode(encoding, errors='replace').strip('\0')
        cursor += n + 10
    return end, tags


def main():
    plan_path = BASE / 'manifest.json'
    plan = json.loads(plan_path.read_text())
    (OUT / 'clips').mkdir(parents=True, exist_ok=True)
    for source in sorted((BASE / 'sources').glob('magnific__*.mp3')):
        data = source.read_bytes()
        _, tags = id3(data)
        title = tags.get('TIT2', '')
        matches = [e for e in plan['entries'] if title and e['prompt'].startswith(title)]
        assert len(matches) == 1, (source.name, title)
        entry = matches[0]
        target = BASE / entry['source']
        if target.exists():
            assert target.read_bytes() == data, 'A different take already exists'
        else:
            source.rename(target)
        entry['exportName'] = source.name
        entry['exportTitle'] = title

    plan_path.write_text(json.dumps(plan, indent=2) + '\n')

    quality = []
    waves = {}
    with tempfile.TemporaryDirectory(prefix='pockle-moment-audit-') as tmp:
        for entry in plan['entries']:
            source = BASE / entry['source']
            data = source.read_bytes()
            offset, tags = id3(data)
            assert entry['prompt'].startswith(tags.get('TIT2', 'INVALID')), entry['id']
            entry['exportTitle'] = tags['TIT2']
            # Review copies remove metadata/artwork only. No audio cut, gain, or fade.
            mpeg = data[offset:]
            (OUT / 'clips' / (entry['id'] + '.mp3')).write_bytes(mpeg)
            entry.update(status='generated; awaiting owner audition', sha256=hashlib.sha256(data).hexdigest(),
                         mpegSha256=hashlib.sha256(mpeg).hexdigest())
            wav = Path(tmp) / (entry['id'] + '.wav')
            subprocess.run(['/usr/bin/afconvert', '-f', 'WAVE', '-d', 'LEI16@44100', '-c', '1', str(source), str(wav)], check=True, capture_output=True)
            with wave.open(str(wav)) as f:
                rate, frames = f.getframerate(), f.getnframes()
                samples = np.frombuffer(f.readframes(frames), dtype='<i2').astype(float) / 32768
            rms = lambda x: float(np.sqrt(np.mean(x ** 2)))
            quality.append(dict(id=entry['id'], seconds=frames / rate, peak=float(np.max(np.abs(samples))),
                rms=rms(samples), tail100msRms=rms(samples[-int(rate*.1):]),
                tail20msRms=rms(samples[-int(rate*.02):]), lastSample=float(samples[-1]),
                nearFullScaleSamples=int(np.sum(np.abs(samples) >= .999))))
            blocks = np.array_split(samples, 80)
            heights = [max(2, min(40, float(np.max(np.abs(b))) * 46)) for b in blocks]
            waves[entry['id']] = ''.join(f'<rect x="{i*5}" y="{42-h:.2f}" width="3" height="{2*h:.2f}" rx="1.5"/>' for i, h in enumerate(heights))

    assert len(quality) == 7
    assert all(2.9 <= q['seconds'] <= 3.2 and q['peak'] > .001 for q in quality)
    plan.update(status='seven generated audition takes; not approved or imported',
                generationCount=7, observedCreditsUsed=105,
                generationEvidence='Seven UI generation submissions at 15 credits each; Pockle asset count increased from 134 to 141. No retries or top-ups.',
                provenance='Original MP3s downloaded individually via Magnific asset Download menus; exact ID3 title matches recorded prompts. Export filename suffixes are not treated as generation IDs.',
                reviewCopyProcessing='Only ID3 metadata/artwork removed; MPEG audio frames remain byte-identical. No trim, fade, normalization or resynthesis.',
                qualityScope='CoreAudio decode and numerical duration/level/tail checks only. Human listening, app timing and phone playback remain unverified.')
    plan_path.write_text(json.dumps(plan, indent=2) + '\n')
    (OUT / 'quality.json').write_text(json.dumps(quality, indent=2) + '\n')

    esc = html.escape
    icons = ['✦', '□', '✧', '↗', '◇', '⇄', '✓']
    cards = []
    for n, (entry, q) in enumerate(zip(plan['entries'], quality)):
        flagged = q['tail20msRms'] > .005 or q['nearFullScaleSamples'] > 0
        note = 'Listen closely to the ending / level; numerical check flagged this take.' if flagged else 'Decoded successfully · quiet last 20 ms · no near-full-scale samples'
        cards.append(f'''<article style="--accent:{['#d68b78','#bf8c58','#a392cf','#6baaa0','#ba9a68','#bd86a3','#939cba'][n]}">
        <div class="top"><span class="number">0{n+1}</span><span class="tag">{'Optional confirmation' if n==6 else 'Moment cue'}</span></div>
        <span class="symbol" aria-hidden="true">{icons[n]}</span><h2>{esc(entry['label'])}</h2><p class="trigger">{esc(entry['trigger'])}</p>
        <svg viewBox="0 0 400 84" role="img" aria-label="Waveform of {esc(entry['label'])}">{waves[entry['id']]}</svg>
        <audio id="{entry['id']}" controls preload="metadata" src="clips/{entry['id']}.mp3"></audio>
        <div class="meta"><span>{q['seconds']:.2f} seconds</span><a href="clips/{entry['id']}.mp3" download>Download audition</a></div>
        <details><summary>Brief &amp; checks</summary><p>{esc(entry['prompt'])}</p><p>{note}</p><p>Peak {q['peak']:.3f} · last 20 ms RMS {q['tail20msRms']:.5f}. Waveform checks do not establish subjective sound quality.</p></details></article>''')
    css = '''*{box-sizing:border-box}body{margin:0;color:#eadfe9;background:radial-gradient(ellipse at 7% 0%,#4f334b 0,transparent 42%),radial-gradient(ellipse at 95% 35%,#283e48 0,transparent 40%),#201b29;font:16px/1.6 system-ui,sans-serif}main{max-width:1320px;margin:auto;padding:60px 28px 44px}.eyebrow{font-size:12px;letter-spacing:3px;color:#d5b7c9;text-transform:uppercase}h1{font-size:clamp(38px,5vw,68px);line-height:1.08;letter-spacing:-2px;max-width:900px;margin:20px 0}.intro{max-width:790px;color:#ccbccc;font-size:18px}.badge{display:inline-block;padding:6px 12px;background:#ffffff0c;border:1px solid #ffffff1a;border-radius:20px;font-size:12px}.controls{display:flex;flex-wrap:wrap;gap:10px;margin:28px 0 40px;align-items:center}button{font:600 14px system-ui;cursor:pointer;border:0;border-radius:22px;padding:13px 21px;background:#eed2ba;color:#35273b}button.secondary{background:#ffffff12;color:#eadfe9;border:1px solid #ffffff22}.hint{color:#ac9eb4;font-size:13px;margin-left:8px}.grid{display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:18px}article{background:linear-gradient(145deg,#ffffff0e,#ffffff04);border:1px solid #ffffff14;border-radius:25px;padding:24px;min-width:0;box-shadow:0 12px 30px #0806101c}.top,.meta{display:flex;justify-content:space-between;gap:8px;align-items:center}.number{font-size:12px;letter-spacing:2px;color:var(--accent)}.tag{font-size:11px;color:#bbaec2;text-transform:uppercase;letter-spacing:1px}.symbol{display:block;font-size:42px;color:var(--accent);line-height:1;margin:25px 0 18px}h2{font-size:23px;line-height:1.2;letter-spacing:-.5px;margin:0 0 14px}.trigger{font-size:14px;color:#c2b5c8;min-height:70px}svg{width:100%;height:64px;fill:var(--accent);opacity:.8}audio{display:block;width:100%;height:40px;margin:16px 0 10px}.meta{font-size:11px;color:#bbaec2}a{color:#e4c8df}details{margin-top:22px;padding-top:15px;border-top:1px solid #ffffff12;font-size:12px;color:#bbaec2}summary{cursor:pointer;color:#dfcddd}footer{font-size:13px;color:#bcaec4;margin-top:35px;padding-top:25px;border-top:1px solid #ffffff18}footer p{max-width:950px}.protocol{margin:38px 0 0;padding:22px 26px;border:1px solid #ffffff16;border-radius:20px;background:#ffffff05}.protocol h2{font-size:20px}.protocol p{color:#bcaec4;font-size:14px}.playing{border-color:var(--accent)}@media(max-width:1000px){.grid{grid-template-columns:repeat(2,minmax(0,1fr))}}@media(max-width:650px){main{padding:32px 18px}.grid{grid-template-columns:1fr}.trigger{min-height:0}.hint{margin:0}h1{letter-spacing:-1px}}'''
    script = '''const audios=[...document.querySelectorAll('audio')];let playlist=false;audios.forEach((a,i)=>{a.addEventListener('play',()=>{audios.forEach(b=>{if(b!==a){b.pause();b.closest('article').classList.remove('playing')}});a.closest('article').classList.add('playing');});a.addEventListener('pause',()=>a.closest('article').classList.remove('playing'));a.addEventListener('ended',()=>{a.closest('article').classList.remove('playing');if(playlist&&i<audios.length-1){audios[i+1].currentTime=0;audios[i+1].play().catch(()=>playlist=false);}else playlist=false;});});document.querySelector('#play-all').onclick=()=>{playlist=true;audios[0].currentTime=0;audios[0].play().catch(()=>playlist=false);};document.querySelector('#stop').onclick=()=>{playlist=false;audios.forEach(a=>{a.pause();a.currentTime=0});};'''
    (OUT / 'index.html').write_text(f'''<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Pockle · Little moments, big feelings</title><style>{css}</style></head><body><main>
        <div class="eyebrow">Pockle sound studio · audition 01</div><h1>Little moments.<br>Big feelings.</h1><span class="badge">Seven new takes · awaiting your approval · not in the app</span>
        <p class="intro">A soft, tactile sound family for discovery, fusion and progress. Each cue stands on its own; character voices stay separate so these moments feel special without getting noisy.</p>
        <div class="controls"><button id="play-all">Listen to all seven</button><button class="secondary" id="stop">Stop</button><span class="hint">Start at a comfortable volume · only one cue plays at a time</span></div><div class="grid">{''.join(cards)}</div>
        <section class="protocol"><h2>How these should behave</h2><p>One cue per moment. A special reveal replaces the standard reveal. A ready walking box takes priority over the 1,000-step cue when both happen together. The saved-delivery confirmation is optional and should stay silent when a reveal starts immediately.</p><p>Fusion, purchase and trade cues play only after the result is saved. Respect the app sound preference, preserve the whole tail, and keep all playback local. Trade audio is an audition for the future trading flow, not evidence that trading is implemented.</p></section>
        <footer><p>Generated November 8, 2026 in the existing Magnific Pockle project using ElevenLabs Text to Sound v2. Seven 3-second generations at 15 credits each: <strong>105 existing credits</strong>. No retries or top-ups. Original downloads and prompts are preserved.</p><p>Verified: all seven MP3s decode, have expected duration and audible signal; review audio frames match the originals. No trimming, fades or volume normalization. Human listening approval, Unity integration and phone playback remain to be done. <a href="quality.json">Numerical checks</a> · <a href="../../../ArtSource/Audio/Moments/01/manifest.json">Provenance &amp; prompts</a></p></footer>
        </main><script>{script}</script></body></html>''')
    print(json.dumps(quality, indent=2))
    print(f'Review ready: {OUT / "index.html"}')


if __name__ == '__main__':
    main()
