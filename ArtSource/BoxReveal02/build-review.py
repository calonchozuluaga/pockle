#!/usr/bin/env python3
"""Build a local concept/audio audition. Does not import anything into Unity."""
import hashlib
import html
import json
import shutil
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'ArtSource/BoxReveal02'
OUT = ROOT / 'docs/box-reveal-review/02'
OUT.mkdir(parents=True, exist_ok=True)
(OUT / 'clips').mkdir(exist_ok=True)
quality = json.loads((SOURCE / 'quality.json').read_text())
shutil.copy2(SOURCE / 'mystery-carton-02.png', OUT / 'mystery-carton-02.png')
shutil.copy2(ROOT / 'Assets/Pockle/Resources/UI/PockleWordmark.png', OUT / 'wordmark.png')
for cue in quality:
    shutil.copy2(SOURCE / (cue + '.mp3'), OUT / 'clips' / (cue + '.mp3'))
for old, name in [('box-reveal', 'previous-common'), ('special-reveal', 'previous-special')]:
    shutil.copy2(ROOT / f'ArtSource/Audio/Moments/01/sources/{old}.mp3', OUT / 'clips' / (name + '.mp3'))
(OUT / 'quality.json').write_text(json.dumps(quality, indent=2) + '\n')

def wave(cue):
    levels = quality[cue]['waveformRms80']
    maximum = max(levels)
    bars = ''.join(f'<rect x="{i*5}" y="{32-max(1,v/maximum*29):.2f}" width="3" height="{max(2,v/maximum*58):.2f}" rx="1.5"/>' for i, v in enumerate(levels))
    return '<svg viewBox="0 0 400 64" aria-label="Audio waveform" role="img">' + bars + '</svg>'

page = '''<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Pockle · A little mystery, a bigger reveal</title><style>
*{box-sizing:border-box}body{margin:0;background:#211724;color:#fae8df;font:16px/1.65 system-ui,-apple-system,sans-serif}main{max-width:1240px;margin:auto;padding:45px 26px 65px}header{max-width:850px;margin-bottom:30px}.brand{width:150px;height:auto}.eyebrow{text-transform:uppercase;letter-spacing:.18em;font-size:11px;color:#dfb7ca;margin-top:28px}h1{font-size:clamp(36px,5.1vw,62px);line-height:1.1;letter-spacing:-.045em;margin:16px 0 20px}h2{letter-spacing:-.035em;line-height:1.2;font-size:29px;margin:6px 0 12px}p{color:#ccb9ca;margin:8px 0 16px}.intro{font-size:18px;max-width:780px}.pill{display:inline-block;border:1px solid #ffffff25;border-radius:22px;padding:5px 13px;font-size:12px;color:#f1d5c7}.hero{border-radius:25px;overflow:hidden;background:#faede3;margin:25px 0 12px}.hero img{display:block;width:100%;height:auto}.caption{font-size:13px;max-width:880px}.grid{display:grid;grid-template-columns:1fr 1fr;gap:22px;margin-top:34px}article{border-radius:26px;border:1px solid #ffffff1a;background:linear-gradient(130deg,#473048,#2d2232);padding:25px;min-width:0}article.special{background:linear-gradient(130deg,#46373c,#302337);--accent:#f6d49e}article.common{--accent:#ffbfad}.category{font-size:11px;letter-spacing:.18em;text-transform:uppercase;color:var(--accent)}.stage{height:285px;border-radius:20px;position:relative;overflow:hidden;background:#f9ede3;isolation:isolate;margin:20px 0}.carton{position:absolute;inset:0;transform-origin:50% 80%;will-change:transform,opacity}.crop{position:absolute;inset:0;overflow:hidden}.crop img{position:absolute;height:100%;width:auto;max-width:none;left:50%;transform:translateX(-25%)}.open{opacity:0}.open img{transform:translateX(-75%)}.halo{position:absolute;inset:0;opacity:0;background:radial-gradient(ellipse at 50% 60%,#ffe2a7bb,transparent 68%);mix-blend-mode:screen;z-index:2;pointer-events:none}.sparks{position:absolute;inset:0;opacity:0;z-index:3;pointer-events:none}.spark{position:absolute;font-size:26px;color:#ffd982;text-shadow:0 0 12px #fbe3a7}.stage.shaking .carton{animation:shake .13s linear infinite}.stage.opened .closed{opacity:0}.stage.opened .open{opacity:1}.stage.opened .carton{animation:pop .45s ease-out both}.special .stage.opened .halo{opacity:1;transition:opacity .3s}.special .stage.opened .sparks{opacity:1;animation:sparkle 2.2s ease-out forwards}.status{position:absolute;bottom:8px;left:14px;right:14px;text-align:center;z-index:4;font-size:11px;color:#5d3b54;background:#fff9efdc;border-radius:20px;padding:3px 7px}.controls{display:flex;align-items:center;gap:10px;flex-wrap:wrap;margin:20px 0 10px}button{font:650 14px system-ui;cursor:pointer;background:#f5d7c5;color:#3b203a;border:0;border-radius:22px;padding:12px 20px}button.secondary{background:#ffffff12;color:#f4e0ee;border:1px solid #ffffff20}audio{width:100%;height:38px;margin:12px 0 4px}svg{width:100%;height:56px;fill:var(--accent);opacity:.8}.timing{display:flex;gap:7px;flex-wrap:wrap;font-size:11px;margin:17px 0}.timing span{border:1px solid #ffffff1a;border-radius:20px;padding:4px 10px;color:#dac5d7}.small{font-size:12px;color:#bdaabd}details{border-top:1px solid #ffffff15;margin-top:16px;padding-top:12px;font-size:13px}summary{cursor:pointer;color:#e8ccdd}.notes{border-top:1px solid #ffffff1a;padding-top:25px;margin-top:35px;max-width:950px}.notes h2{font-size:23px}.notes p{font-size:14px}a{color:#f1ccdc}.motion{display:flex;gap:8px;align-items:center;color:#dbc6d7;font-size:13px;margin:25px 0 4px}@keyframes shake{0%,100%{transform:rotate(-2deg) translateX(-2px)}50%{transform:rotate(2.8deg) translateX(3px)}}@keyframes pop{0%{transform:scale(.95)}60%{transform:scale(1.035)}100%{transform:scale(1)}}@keyframes sparkle{0%{transform:scale(.7);opacity:0}15%{opacity:1}100%{transform:scale(1.3) translateY(-22px);opacity:0}}@media(max-width:760px){main{padding:25px 16px 45px}.grid{grid-template-columns:1fr}.stage{height:275px}article{padding:20px}h1{letter-spacing:-.035em}}
</style></head><body><main>
<header><img class="brand" src="wordmark.png" alt="Pockle"><div class="eyebrow">Box studio · revision 02 · November 9</div><h1>A little mystery.<br>A bigger reveal.</h1><p class="intro">A proper surprise-toy carton for the universal teaser, with a tactile shake and two different reveal endings.</p><span class="pill">Concept &amp; audio auditions · awaiting your review</span></header>
<figure class="hero"><img src="mystery-carton-02.png" alt="Closed and hinged-open Pockle mystery cartons with peach fronts, plum sides, colorful jelly decoration, a large Pockle logo and a mystery question mark"></figure>
<p class="caption">The branded peach-and-plum carton stays the same before opening. Common discoveries get a soft little pop; Rare, Epic and Secret discoveries get a brighter celebration after the opening begins. Collection-specific packaging can keep its own identity.</p>
<label class="motion"><input id="calm" type="checkbox"> Calm preview · no shake or sparkles</label>
<section class="grid">
<article class="common"><div class="category">Common reveal</div><h2>A satisfying little surprise</h2><p>Dry paper rattles, a crisp lid release, a rounded toy pop and a small warm chime.</p>
<div class="stage" id="stage-common"><div class="carton"><div class="crop closed"><img src="mystery-carton-02.png" alt="Closed mystery carton"></div><div class="crop open"><img src="mystery-carton-02.png" alt="Open mystery carton"></div></div><div class="status">Tap Preview to audition the motion target</div></div>
<div class="timing"><span>Shake → pop target 0.8s</span><span>3.2s full audio</span></div>
WAVE_COMMON
<div class="controls"><button data-preview="common">Preview common</button><button class="secondary" data-stop>Stop</button></div><audio id="audio-common" controls preload="metadata" src="clips/common-reveal-02.mp3"></audio>
<p class="small">Full generated take, including its natural ending. No trimming or gain changes.</p><details><summary>Compare the previous sound</summary><audio controls preload="none" src="clips/previous-common.mp3"></audio><p>The previous cue begins with the lid/reveal and has no distinct shake lead-in.</p></details></article>
<article class="special"><div class="category">Rare · Epic · Secret</div><h2>A moment worth celebrating</h2><p>A longer accelerating shake, a brighter lid pop, an airy flourish and sparkling bell-like celebration.</p>
<div class="stage" id="stage-special"><div class="halo"></div><div class="carton"><div class="crop closed"><img src="mystery-carton-02.png" alt="Same closed mystery carton"></div><div class="crop open"><img src="mystery-carton-02.png" alt="Same open mystery carton"></div></div><div class="sparks" aria-hidden="true"><span class="spark" style="left:16%;top:25%">✦</span><span class="spark" style="left:76%;top:22%">✧</span><span class="spark" style="left:70%;top:63%">✦</span><span class="spark" style="left:23%;top:65%">✧</span></div><div class="status">Same unopened box · bigger reveal ending</div></div>
<div class="timing"><span>Shake → pop target 1.2s</span><span>4.0s full audio</span></div>
WAVE_SPECIAL
<div class="controls"><button data-preview="special">Preview special</button><button class="secondary" data-stop>Stop</button></div><audio id="audio-special" controls preload="metadata" src="clips/special-reveal-02.mp3"></audio>
<p class="small">One special cue replaces the common cue. Both sounds never play together.</p><details><summary>Compare the previous sound</summary><audio controls preload="none" src="clips/previous-special.mp3"></audio><p>The previous cue is a soft magical bloom. This audition adds physical carton movement and a larger ending.</p></details></article>
</section>
<section class="notes"><h2>What to listen for</h2><p>Does the opening sound physical and satisfying? Is the special ending clearly more exciting? Listen at the same phone volume and check the whole decay. These are short original effects, without speech or a music bed.</p><p>The animations above illustrate the intended beats; the generated takes still need your listening review to confirm their exact pop placement. They are a browser concept, not a capture of the Unity reveal. Final in-game timing should use separate shake-start and saved-result pop events, preserve the full tail, and respect sound and calm-motion settings.</p><p>Both MP3s decode completely at 44.1 kHz. Neither has near-full-scale decoded samples, and both have quiet final 20 ms. Numerical checks establish integrity, not listening approval. <a href="quality.json">Quality report</a>.</p><p>Box concept: built-in image generation using the approved packaging and Pockle identity as references. Audio: two ElevenLabs Text to Sound v2 takes through the connected Runway tool, using eight existing credits; no top-up. Original sources, prompts and timing notes are in <a href="../../../ArtSource/BoxReveal02/manifest.json">the manifest</a>. These auditions have not replaced the existing Unity assets.</p></section>
</main><script>
const cues={common:{audio:document.querySelector('#audio-common'),stage:document.querySelector('#stage-common'),pop:.8},special:{audio:document.querySelector('#audio-special'),stage:document.querySelector('#stage-special'),pop:1.2}};
const calm=document.querySelector('#calm');calm.checked=window.matchMedia('(prefers-reduced-motion: reduce)').matches;
let raf=0,active=null;
function stop(){cancelAnimationFrame(raf);active=null;document.querySelectorAll('audio').forEach(a=>{a.pause()});Object.values(cues).forEach(c=>{c.stage.classList.remove('shaking','opened');c.stage.querySelector('.status').textContent='Ready to preview'});}
function frame(){if(!active)return;let c=cues[active],t=c.audio.currentTime,pop=calm.checked?0:c.pop;c.stage.classList.toggle('shaking',!calm.checked&&t<pop&&!c.audio.paused);c.stage.classList.toggle('opened',t>=pop);c.stage.querySelector('.status').textContent=t<pop?'Shake · building anticipation':calm.checked?'Calm · complete audio ending':'Open · letting the full sound finish';if(calm.checked)c.stage.querySelectorAll('.halo,.sparks').forEach(e=>e.style.display='none');else c.stage.querySelectorAll('.halo,.sparks').forEach(e=>e.style.display='');raf=requestAnimationFrame(frame);}
document.querySelectorAll('[data-preview]').forEach(b=>b.addEventListener('click',async()=>{stop();active=b.dataset.preview;const c=cues[active];c.audio.currentTime=0;try{await c.audio.play();frame()}catch(e){c.stage.querySelector('.status').textContent='Use the audio player to allow playback';}}));
document.querySelectorAll('[data-stop]').forEach(b=>b.addEventListener('click',stop));
document.querySelectorAll('audio').forEach(a=>a.addEventListener('play',()=>{document.querySelectorAll('audio').forEach(other=>{if(other!==a)other.pause()});if(!Object.values(cues).some(c=>c.audio===a)){cancelAnimationFrame(raf);active=null}}));
Object.values(cues).forEach(c=>c.audio.addEventListener('ended',()=>{cancelAnimationFrame(raf);c.stage.classList.remove('shaking');c.stage.querySelector('.status').textContent='Finished · full audio tail retained'}));
calm.addEventListener('change',()=>{if(active)frame()});document.addEventListener('visibilitychange',()=>{if(document.hidden)stop()});
</script></body></html>'''
page = page.replace('WAVE_COMMON', wave('common-reveal-02')).replace('WAVE_SPECIAL', wave('special-reveal-02'))
(OUT / 'index.html').write_text(page)
print(f'Built {OUT / "index.html"}')
