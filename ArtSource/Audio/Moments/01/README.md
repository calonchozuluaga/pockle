# Pockle moment sounds — audition 01

Seven new audio auditions generated November 8, 2026 in the existing Magnific **Pockle** project. These are **not approved or imported into Unity**.

- Provider: Magnific, ElevenLabs Text to Sound v2.
- Settings: 3 seconds, one output, 50% prompt influence, loop off.
- Cost: seven submissions × 15 existing credits = **105 credits**. No retries, purchases or top-ups.
- Project asset count observed increasing from 134 to 141. Each finished card showed 00:03.
- Downloads: original MP3s obtained individually using the asset Download menus. Original export names and embedded prompt titles are recorded in `manifest.json`; filename suffixes are not assumed to be generation IDs.
- Exact source prompts, file hashes and MPEG hashes are preserved in the manifest. Original MP3 bytes are unchanged; only filenames were standardized.

Open `docs/moment-audio-review/01/index.html` from the repository root to audition the cues. Its small MP3 copies remove only embedded ID3 artwork/metadata. MPEG audio frames are byte-identical to the originals, with no trims, gain edits or fades.

## Verified

All seven clips decode using macOS CoreAudio to 3.000 seconds of 44.1 kHz mono PCM. Each has a nonzero signal, no near-full-scale samples in that mono decode, and a last-20-ms RMS below 0.00034. The numerical report is `docs/moment-audio-review/01/quality.json`. The system decoder required execution outside the filesystem sandbox; its sandboxed conversion failed to initialize.

These checks establish file integrity and quiet numerical endings, **not subjective listening approval**. The original levels differ: the 1,000-step and trade takes are stronger than the standard reveal. Audition and set individual runtime gains after approval; do not replace the originals or abruptly trim their tails.

## Intended event routing

| Cue | Trigger |
| --- | --- |
| `fusion-complete` | After the new fusion and consumed spares are durably saved, timed to the result appearing. |
| `box-reveal` | Standard toy reveal after its saved delivery is available. |
| `special-reveal` | Replaces the standard cue for special discoveries; do not stack both. |
| `steps-1000` | Once per new qualifying 1,000-step milestone while foregrounded. |
| `walking-box-ready` | Once when a walking box becomes claimable; takes priority over the step cue if simultaneous. |
| `trade-complete` | Future trading only, after both inventories commit atomically. |
| `purchase-saved` | Optional quiet confirmation after verified delivery; omit when a reveal follows immediately. |

All eventual playback must respect sound preferences, preserve the whole ending, avoid repeated triggers on refresh/retry, and stay local to the device. A sound is not evidence that a purchase, fusion or trade succeeded. Do not play success before durable server/local state as appropriate.

## Still to do

Owner listening approval; any requested replacement takes; runtime routing and per-cue volume balance; foreground/background and preference checks; Unity tests and phone listening. No runtime, HUD or Resources files were changed for this audition set.

Rebuild the page and numerical report on macOS with NumPy installed:

```sh
python3 ArtSource/Audio/Moments/01/build-review.py
```
