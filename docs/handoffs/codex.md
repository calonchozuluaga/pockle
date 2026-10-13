# Codex handoff

## Clean Peach version — UX-025/030, ART-002

Owner uploaded a new Peach model after the seam repair. Preparing `codex/peach-clean-v2` from main `3c7b071d`; both handoffs read. Scope: Meshy asset/paint/provenance, conversion inspection, existing seam/import/grip/render checks, and phone-test notes. Owner explicitly approved publishing the converted mesh and texture for the phone build. Preserve the uploaded clean look. Keep the seam-safe weld/rebake pipeline and phone budget. Required local portable/source/whitespace checks pass. Unity/Android checks and owner phone review are pending.

[Previous seam repair and verification: PR #16](https://github.com/calonchozuluaga/pockle/pull/16). [Previous coordination history](https://github.com/calonchozuluaga/pockle/blob/3c7b071d1ee853997ddcc0d88a56e7631967dfbf/docs/handoffs/codex.md).

## Fox upload import — ART-002, UX-030 (October 13)

Claimed on `codex/fox-model-import` from latest origin/main. Files: fox conversion tool, Assets/Pockle/Art/Models/Fox, art source provenance, import notes and validation. Import as separate review model, preserve uploaded paint; no catalog identity assigned yet. Unity/phone verification pending. Owner local edits and Claude UI branch stay in their original checkout.

Fox import completed: native FBX, original paint, editable Standard vinyl material and review prefab. Preserved original maps in ArtSource/Fox. Unity 6000.6.5f1 import/compile, portable/source checks and whitespace passed; Blender preview inspected. Unity visual/phone/tactile tests pending. See `../FOX_MODEL_IMPORT.md`. Additive review assets also copied to owner local checkout for Unity inspection.

Source checker updated to recognize Unity reserved Built-in shader GUID used by Standard. No settings/package changes are included.

## Fox collection — UX-029/030/033, ART-002 (October 13)

Owner requested a new collection for the imported fox. Claimed branch `codex/fox-collection`, extending the isolated import branch. Files: Core catalog/motion, runtime art registry/JellyToy, editor fox review/runtime asset generator, Resources/Characters/Fox, portable and Unity tests, docs. Existing collection HUD is catalog driven; no concurrent edits to Claude HUD. Default display name Fox pending owner input. Preserve legacy saves and reward pools; original Gloss Vinyl is available art, other finish slots planned. No automatic inventory grants.

Fox collection catalog/runtime built and portable checks pass. Eight related Unity runtime tests passed (Fox, existing character studies and Pip variants). Original artwork uses a firm Standard vinyl material, no added Pip face/crowns. Request for Claude: CollectionUiTests has a hardcoded twelve-card assertion; use `ToyCatalog.Collections.Count`. Generic owned non-Pip navigation still calls `TryGetPlayableVariant`; future HUD ID-selection support must call `TryPlayCollectible(id)` and preserve the selected ID in title/backdrop/profile. Fox discovery/reward decisions are not invented in this change.

Owner explicitly approved updating the single twelve-card UI test assertion to use catalog count. No other existing Claude UI/test code changed.

October 13 Fox collection validation: Unity 6000.6.5f1 imported the native runtime asset (20,009 vertices / 30,000 triangles); full PlayMode suite 29/29 passed, including Fox browser/no-grant and model/material/switching checks; full EditMode suite 16/16 passed; portable Core checks and source checks passed; whitespace passed on the branch. Android build and phone visuals/performance remain unverified. An initial no-graphics UI run crashed during portrait rendering; the graphics-enabled suite passed after correcting the new test to use visible buttons.
