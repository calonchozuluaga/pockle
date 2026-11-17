# Main sounds — one per pockle (generated set 02)

November 17, 2026. Each character has one main sound, played only from the Play screen's Sound control. These eleven were generated in the owner's Magnific **Pockle** project and chosen by the owner the same day from a listening page (`docs/main-sound-review/02/index.html`).

| Pockle | Asked for | Chosen | Length in the game |
| --- | --- | --- | --- |
| Pip | a single soft wet bubble pop | take 2 | 0.42 s |
| Fox | one tiny cheerful yip | take 2 | 1.76 s |
| Axolotl | a soft watery plop with rising bubbles | take 3 | 1.42 s |
| Firb | a bright shimmer chime | take 2 | 1.77 s |
| Coco | one short warm owl-like hoot | take 2 | 1.35 s |
| Snowy | a short happy ocarina flourish | take 1 | 2.00 s |
| Ginger | a short high-pitched giggle | take 1 | 1.43 s |
| Reiny | a soft snuffle and a curious coo | take 1 | 2.85 s |
| Guin | one squeaky baby-seabird chirp | take 2 | 1.97 s |
| Bundy | a springy, wobbly boing | take 1 | 0.62 s |
| Wally | a soft sonar-like ping with a rising whine | take 2 | 1.19 s |

## How they were made

- Magnific sound-effect generator (ElevenLabs), three variations per prompt, loop off, 2 or 3 seconds requested. Magnific sweeps prompt influence across the variations; the exact values are not reported.
- 33 takes, 375 credits (5 per second per take). Balance went from 28,102 to 27,727. The account's Unlimited mode does not apply to generations made through the connector, so these were charged.
- Exact prompts, generation IDs, hashes and per-take measurements are in `manifest.json`. All 33 source MP3s are in `sources/`; the 22 not chosen are kept as alternates.

The briefs came from an earlier audition of Epidemic Sound sound effects, where the owner picked the kind of sound each pockle should make. Epidemic's licence does not cover in-app use on ordinary plans, so none of that audio is in this repository.

## Processing

`import-runtime.py` decodes each approved take with macOS CoreAudio to 44.1 kHz mono 16-bit PCM, trims silence before the sound to 20 ms and after it to 250 ms, fades the cut edges, and applies one constant gain toward an active-window RMS of 0.09 with a 0.75 peak ceiling (the rule the moment sounds use). No compression or pitch change. Results and trims per clip are in `runtime-import.json`.

Axolotl's take began with 1.3 seconds of silence, which is why leading silence is trimmed: without it the Sound control would seem not to respond.

```sh
python3 ArtSource/Audio/Main/02/import-runtime.py
```

## Open

Approved by ear from the listening page in a browser. Not yet heard in the app or on a phone speaker.
