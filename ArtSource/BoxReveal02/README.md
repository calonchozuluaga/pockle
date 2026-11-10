# Mystery carton and reveal sounds — review 02

Owner requested a richer version of the neutral placeholder carton and new common/special reveal sounds on November 9, 2026.

`mystery-carton-02.png` is a concept sheet, generated using the approved packaging and wordmark as references. The same closed box should be used before either rarity outcome; collection-specific cartons retain their identity. This is not a replacement 3D model or production texture atlas. The final carton should use the real wordmark texture and separately hinged lid/flaps.

Two original sound auditions were generated with ElevenLabs Text to Sound v2 through the connected Runway tool, using eight existing credits and no top-up. Exact prompts, task IDs and intended timing are in `manifest.json`. Original MP3s are unchanged; the review uses byte-identical copies. Analysis WAVs establish duration, levels and quiet numerical endings, not subjective listening approval.

Open `docs/box-reveal-review/02/index.html` for the box, two audio players, previous-sound comparisons and a small browser motion/timing concept. The preview targets a pop at 0.8 seconds for common and 1.2 seconds for special. Those are prompt targets; exact sound onset still needs listening review.

The existing Unity reveal invokes its sound at the pop, after the shake. A full take containing shake cannot simply replace that file at the existing callback. After approval, route a shake-start layer and a separate reveal ending to the animation events; preserve tails, saved-result guarantees, mute/volume, Calm/Skip and interruption recovery. Common and special must never stack.

No new sound or image was imported into Unity Resources, no HUD or tests changed, and the current phone builds are unchanged. Review assets only. Build the page with `python3 ArtSource/BoxReveal02/build-review.py`.
