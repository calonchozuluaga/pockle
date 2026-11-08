#!/usr/bin/env python3
"""Decode staged audition MP3s with macOS CoreAudio; report duration, level and tails."""
import json, subprocess, tempfile, wave
from pathlib import Path
import numpy as np
root=Path(__file__).resolve().parents[1]
plan=json.loads((root/'ArtSource/ReactionAudio01/manifest.json').read_text())
rows=[]
with tempfile.TemporaryDirectory(prefix='pockle-audio-audit-') as tmp:
 for entry in plan['entries']:
  source=root/'ArtSource/ReactionAudio01'/entry['source']
  if not source.exists():continue
  wav=Path(tmp)/(entry['id']+'.wav')
  subprocess.run(['/usr/bin/afconvert','-f','WAVE','-d','LEI16@44100','-c','1',str(source),str(wav)],check=True,capture_output=True)
  with wave.open(str(wav)) as f:
   rate=f.getframerate(); frames=f.getnframes(); data=np.frombuffer(f.readframes(frames),dtype='<i2').astype(float)/32768
  peak=float(np.max(np.abs(data))); tail=data[-int(rate*.1):]
  rows.append(dict(id=entry['id'],seconds=frames/rate,peak=peak,rms=float(np.sqrt(np.mean(data**2))),tailRms=float(np.sqrt(np.mean(tail**2))),tail20msRms=float(np.sqrt(np.mean(data[-int(rate*.02):]**2))),lastSample=float(data[-1]),clippedSamples=int(np.sum(np.abs(data)>=.999))))
output=root/'docs/audio-review/reactions-01/quality.json'
output.write_text(json.dumps(rows,indent=2)+'\n')
assert all(1.8<=r['seconds']<=3.2 and r['peak']>.001 for r in rows),'Unexpected duration or silent take'
print(f'Decoded {len(rows)}/143 clips. Peak maximum {max(r["peak"] for r in rows):.5f}.')
print('Audible last-20ms tails:',[(r['id'],round(r['tail20msRms'],4)) for r in rows if r['tail20msRms']>.005])
print('Near-full-scale samples for listening review:',[(r['id'],r['clippedSamples']) for r in rows if r['clippedSamples']])
