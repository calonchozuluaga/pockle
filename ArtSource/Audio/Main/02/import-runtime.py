#!/usr/bin/env python3
"""Build each pockle's one main sound for Unity from the approved Magnific take.

macOS CoreAudio (afconvert) and NumPy are required. Run from anywhere:
    python3 ArtSource/Audio/Main/02/import-runtime.py
Source MP3s are never modified. Existing .meta files (and their GUIDs) are kept.
"""
import hashlib, json, subprocess, tempfile, wave
from pathlib import Path
import numpy as np

base = Path(__file__).resolve().parent
root = base.parents[3]
out = root / 'Assets/Pockle/Resources/Audio/Main'
TARGET_ACTIVE_RMS, PEAK_CEILING = .09, .75   # same loudness rule as the moment sounds
LEAD, TAIL, FADE_IN, FADE_OUT = .02, .25, .005, .04  # seconds kept around the sound, and edge fades

META = """fileFormatVersion: 2
guid: {guid}
AudioImporter:
  externalObjects: {{}}
  serializedVersion: 8
  defaultSettings:
    serializedVersion: 2
    loadType: 0
    sampleRateSetting: 0
    sampleRateOverride: 0
    compressionFormat: 0
    quality: 1
    conversionMode: 0
    preloadAudioData: 1
  platformSettingOverrides: {{}}
  forceToMono: 0
  normalize: 0
  loadInBackground: 0
  ambisonic: 0
  3D: 0
  userData:
  assetBundleName:
  assetBundleVariant:
"""
FOLDER_META = """fileFormatVersion: 2
guid: {guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData:
  assetBundleName:
  assetBundleVariant:
"""


def guid(path):  # same scheme as tools/sync-unity-meta.py
    return hashlib.md5(('pockle-prototype:' + path).encode()).hexdigest()


def write_meta(path, template):
    meta = Path(str(path) + '.meta')
    if not meta.exists():
        meta.write_text(template.format(guid=guid(path.relative_to(root).as_posix())))


out.mkdir(parents=True, exist_ok=True)
write_meta(out, FOLDER_META)
rows = []
with tempfile.TemporaryDirectory(prefix='pockle-main-sound-') as tmp:
    for e in json.loads((base / 'manifest.json').read_text())['takes']:
        if e['status'] != 'approved':
            continue
        source = base / e['source']
        digest = hashlib.sha256(source.read_bytes()).hexdigest()
        assert digest == e['sha256'], 'Source changed: ' + e['id']
        wav = Path(tmp) / (e['id'] + '.wav')
        subprocess.run(['/usr/bin/afconvert', '-f', 'WAVE', '-d', 'LEI16@44100', '-c', '1', str(source), str(wav)], check=True)
        with wave.open(str(wav)) as f:
            n, rate = f.getnframes(), f.getframerate()
            values = np.frombuffer(f.readframes(n), dtype='<i2').astype(float) / 32768
        # Find the audible span in 10 ms windows, then keep a short lead-in and the natural tail.
        win = rate // 100
        k = n // win
        env = np.sqrt(np.mean(values[:k * win].reshape(k, win) ** 2, axis=1))
        on = np.where(env > max(.004, float(np.max(np.abs(values))) * .03))[0]
        first = max(0, int(on[0] * win - LEAD * rate))
        last = min(n, int((on[-1] + 1) * win + TAIL * rate))
        cut = values[first:last].copy()
        fade_in, fade_out = int(FADE_IN * rate), int(FADE_OUT * rate)
        if first > 0:
            cut[:fade_in] *= np.linspace(0, 1, fade_in)
        cut[-fade_out:] *= np.linspace(1, 0, fade_out)  # always end in silence, so the clip cannot click
        peak = float(np.max(np.abs(cut)))
        loud = [b for b in np.array_split(cut, max(1, len(cut) // win)) if np.sqrt(np.mean(b * b)) > .01]
        active = np.concatenate(loud) if loud else cut
        active_rms = float(np.sqrt(np.mean(active * active)))
        gain = min(TARGET_ACTIVE_RMS / active_rms, PEAK_CEILING / peak)
        scaled = np.rint(cut * gain * 32768).clip(-32768, 32767).astype('<i2')
        target = out / (e['character'] + '.wav')
        with wave.open(str(target), 'wb') as f:
            f.setnchannels(1); f.setsampwidth(2); f.setframerate(rate); f.writeframes(scaled.tobytes())
        write_meta(target, META)
        final = scaled.astype(float) / 32768
        rows.append({
            'character': e['character'], 'take': e['id'], 'sourceSha256': digest,
            'wavSha256': hashlib.sha256(target.read_bytes()).hexdigest(),
            'sourceSeconds': round(n / rate, 3), 'trimmedLeadSeconds': round(first / rate, 3),
            'trimmedTailSeconds': round((n - last) / rate, 3), 'seconds': round(len(cut) / rate, 3),
            'rate': rate, 'gain': round(gain, 4), 'peak': round(float(np.max(np.abs(final))), 4),
            'tail20msRms': round(float(np.sqrt(np.mean(final[-int(rate * .02):] ** 2))), 6),
            'nearFullScaleSamples': int(np.sum(np.abs(final) >= .999)),
        })
report = {
    'method': 'CoreAudio decode to 44.1 kHz mono 16-bit PCM. Silence before the sound is trimmed to %.0f ms and after it to %.0f ms, '
              'with a %.0f ms fade-in when the start is cut and a %.0f ms fade-out at the end. One constant gain per clip toward '
              'active-window RMS %.2f with a %.2f peak ceiling. No compression or pitch change. Source MP3s unchanged.'
              % (LEAD * 1000, TAIL * 1000, FADE_IN * 1000, FADE_OUT * 1000, TARGET_ACTIVE_RMS, PEAK_CEILING),
    'clips': rows,
}
(base / 'runtime-import.json').write_text(json.dumps(report, indent=2) + '\n')
print(json.dumps(report, indent=2))
