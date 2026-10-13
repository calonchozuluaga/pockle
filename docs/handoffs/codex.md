# Codex handoff

## Clean Peach version — UX-025/030, ART-002

Owner uploaded a new Peach model after the seam repair. Preparing `codex/peach-clean-v2` from main `3c7b071d`; both handoffs read. Scope: Meshy asset/paint/provenance, conversion inspection, existing seam/import/grip/render checks, and phone-test notes. Owner explicitly approved publishing the converted mesh and texture for the phone build. Preserve the uploaded clean look. Keep the seam-safe weld/rebake pipeline and phone budget. Required local portable/source/whitespace checks pass. Unity/Android checks and owner phone review are pending.

[Previous seam repair and verification: PR #16](https://github.com/calonchozuluaga/pockle/pull/16). [Previous coordination history](https://github.com/calonchozuluaga/pockle/blob/3c7b071d1ee853997ddcc0d88a56e7631967dfbf/docs/handoffs/codex.md).

## Fox upload import — ART-002, UX-030 (October 13)

Claimed on `codex/fox-model-import` from latest origin/main. Files: fox conversion tool, Assets/Pockle/Art/Models/Fox, art source provenance, import notes and validation. Import as separate review model, preserve uploaded paint; no catalog identity assigned yet. Unity/phone verification pending. Owner local edits and Claude UI branch stay in their original checkout.

Fox import completed: native FBX, original paint, editable Standard vinyl material and review prefab. Preserved original maps in ArtSource/Fox. Unity 6000.6.5f1 import/compile, portable/source checks and whitespace passed; Blender preview inspected. Unity visual/phone/tactile tests pending. See `../FOX_MODEL_IMPORT.md`. Additive review assets also copied to owner local checkout for Unity inspection.

Source checker updated to recognize Unity reserved Built-in shader GUID used by Standard. No settings/package changes are included.
