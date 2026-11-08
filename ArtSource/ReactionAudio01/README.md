# Reaction and temperament sound auditions — November 8, 2026

Owner approved five action-specific sounds plus all eight temperament cues for every collection. Eleven collections × thirteen slots = 143. Nine approved Pip/Fox/Wally pilot sources are reused for distinct action slots; 134 new clips were generated in the existing Magnific Pockle project using ElevenLabs Text to Sound v2, three seconds, one output, 50% prompt influence, looping off.

Actual spend: 134 × 15 = 2,010 existing credits. Magnific spent counter changed from 3,878 to 5,888; remaining balance displayed 39.6K. No subscription, top-up, credential change, voice cloning or per-touch service call.

`manifest.json` records all slots, actual new prompts/settings, creation IDs from native exports, source and MPEG hashes, and approval status. The nine reused clips have routing briefs rather than a claim that they were generated from those new prompts; exact prior model/settings were not recorded by Claude. `PILOT_SOURCE.md` preserves that earlier provenance.

`sources/*.mp3` are original provider downloads, including embedded artwork/metadata. The complete export ZIP remains in the owner's local source folder, not Git. Review copies in `docs/audio-review/reactions-01/clips/` remove ID3 artwork only; MPEG audio frames remain byte-identical. No duration cap, resampling, new fade or hard trim has been applied.

Regenerate the page with `python3 tools/build-reaction-audio-review.py`. Import a downloaded Magnific ZIP with its `--import-zip` argument. Match by embedded prompt title; unknown/ambiguous titles and different takes for the same slot fail rather than silently overwriting. `python3 tools/audit-reaction-audio.py` uses macOS CoreAudio and numpy to decode and measure every available take. Waveform measurements do not replace listening approval.

New takes are auditions, not approved Resources assets. The local runtime supports the complete action/temperament paths, with same-collection approved pilot fallbacks until reviewed assets are imported. Mute, volume, pause, background, page and toy transitions remain explicit. Action and temperament voices share one source and never stack. Play itself remains offline.
