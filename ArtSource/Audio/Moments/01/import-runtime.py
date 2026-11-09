#!/usr/bin/env python3
"""Rebuild approved Unity WAVs from unchanged Magnific MP3s. macOS CoreAudio + NumPy required."""
import hashlib,json,subprocess,tempfile,wave
from pathlib import Path
import numpy as np
base=Path(__file__).resolve().parent
root=base.parents[3]
out=root/'Assets/Pockle/Resources/Audio/Moments'
out.mkdir(parents=True,exist_ok=True)
rows=[]
with tempfile.TemporaryDirectory(prefix='pockle-moment-import-') as tmp:
 for e in json.loads((base/'manifest.json').read_text())['entries']:
  source=base/e['source']; wav=Path(tmp)/(e['id']+'.wav')
  assert hashlib.sha256(source.read_bytes()).hexdigest()==e['sha256'], 'Original source changed: '+e['id']
  subprocess.run(['/usr/bin/afconvert','-f','WAVE','-d','LEI16@44100','-c','1',str(source),str(wav)],check=True)
  with wave.open(str(wav)) as f:
   n=f.getnframes(); rate=f.getframerate(); values=np.frombuffer(f.readframes(n),dtype='<i2').astype(float)/32768
  peak=float(np.max(np.abs(values)))
  # Compare the active portion of each cue, not the varying amount of quiet tail.
  blocks=np.array_split(values,150)
  active=np.concatenate([b for b in blocks if np.sqrt(np.mean(b*b))>.01])
  active_rms=float(np.sqrt(np.mean(active*active)))
  gain=min(.09/active_rms,.75/peak)
  scaled=np.rint(values*gain*32768).clip(-32768,32767).astype('<i2')
  target=out/(e['id']+'.wav')
  with wave.open(str(target),'wb') as f:
   f.setnchannels(1);f.setsampwidth(2);f.setframerate(rate);f.writeframes(scaled.tobytes())
  final=scaled.astype(float)/32768
  rows.append({'id':e['id'],'sourceSha256':hashlib.sha256(source.read_bytes()).hexdigest(),'wavSha256':hashlib.sha256(target.read_bytes()).hexdigest(),
   'frames':n,'rate':rate,'seconds':n/rate,'gain':gain,'sourceActiveRms':active_rms,'peak':float(np.max(np.abs(final))),
   'tail20msRms':float(np.sqrt(np.mean(final[-882:]**2))),'nearFullScaleSamples':int(np.sum(np.abs(final)>=.999))})
report={'method':'Full 3-second CoreAudio mono PCM decode, constant gain to active 20ms-window RMS 0.09 with peak ceiling 0.75; no trimming, fades, compression or tail replacement. Original MP3s unchanged. Runtime master adds further headroom.','clips':rows}
(base/'runtime-import.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report,indent=2))
