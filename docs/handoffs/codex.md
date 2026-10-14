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

## Owner Fox test grant — UX-029/030/033

Claimed on `codex/fox-owner-test`. Owner explicitly authorized a one-time local Fox award and necessary owned-HUD selection changes, preserving Claude Settings. Files: Core inventory, CollectionSession, temporary FoxOwnerTestGrant resource, HUD main/Shelf/Collections and theme, TactilePrototype, grant tests. Inventory persisted before refresh, existing saves/rewards preserved. Test grant resource must be disabled/removed before public distribution.

## Fox orientation and rigid controls — UX-025/030/033

Owner screenshot shows nose-down Fox plus incorrect Pip lift/stretch guidance. Claimed `codex/fox-orientation-handling`, extending the Fox test branch. Correct the native asset's import basis in place (preserve GUID), verify front-facing upright render, enforce rigid turn-only input and update per-toy guidance. User's earlier authorization for necessary Fox HUD changes applies; preserve Claude Settings and local Peach/walking changes. Files: FoxCollectionBuild/runtime Fox asset, handling/input, guidance/HUD hint, focused tests and docs.

## Fox orientation and rigid controls — UX-025/030/033

`codex/fox-orientation-handling`: repaired native Fox mesh basis in place, preserving stable asset identity; owner local project updated without overwriting Peach, walking, Settings or other HUD changes. Fox body/plate drags turn only; play hint says “Drag Fox or the plate to turn.” No lift, stretch, squish or sag. Unity front render inspected upright. Regression/check results recorded after completion; phone rebuild pending.

Fox fix verification: Unity 30/30 PlayMode tests passed (including rigid vertices); combined local portable and source checks passed. Corrected front render saved in `docs/captures/fox-orientation/upright.png`. Phone rebuild pending.

## Approved Fox material collection — UX-025/030/033

Owner approved ten concept-sheet finishes and one copy of each for phone testing. Claimed on `codex/fox-material-collection`, extending the Fox orientation/grant branch. Files: catalog, Fox finish recipes/shader/runtime, CharacterArt registry, idempotent inventory grant/session, Fox tests/checks, editor captures and docs. Preserve existing save IDs, upright shape, Pip/Peach, walking and Claude Settings/HUD. Aurora is special metadata; public reward odds/purchases unchanged. Axolotl import remains authorized next.

October 14 Fox finish implementation: all ten existing Fox slot IDs are available and named from the approved concept. Aurora is fox.glow-jelly / secret identity, no odds/pool/pricing changes. New independent finishes marker fills only missing copies; persists before UI. Necessary Fox HUD title/backdrop changes under earlier owner authorization, Settings preserved. See FOX_FINISHES.md. Axolotl import remains authorized next.

Fox finishes final validation: 32/32 Unity PlayMode tests passed; required portable/source checks passed. Owner-local Android APK build succeeded, final log /private/tmp/fox-phone-release.log. Final APK is Builds/Pockle-Fox-Finishes.apk. No phone connected.

## Axolotl plush import — UX-030/033, ART-002

Owner approved uploaded axolotl as a new collection, after Fox materials. Claimed `codex/axolotl-plush` extending the completed Fox branch. Files: converter, model/paint/native asset, catalog/art registry and generic baked material/title routing, import/play tests and docs. Preserve all existing IDs, phone settings and Claude Settings. No additional Axolotl inventory grant authorized or box policy changes.

Axolotl implementation applied locally: 30,000 triangles / 48,940 UV-split vertices, uploaded paint retained, native upright plush asset and catalog entry. Generic titles support the new collection; Settings unchanged. No Axolotl grant or reward-pool changes. See AXOLOTL_IMPORT.md.

Axolotl validation: native upright render inspected, 33/33 Unity PlayMode tests and required portable/source checks passed. Owner local combined Fox build previously passed 35/35 tests. New combined Android build pending completion; device remains disconnected.

Final owner test APK: Builds/Pockle-Fox-Axolotl.apk (37 MB), current local Peach/walking/Claude Settings plus Fox finishes and Axolotl. Android build and JNI packaging passed. Fox-only Builds/Pockle-Fox-Finishes.apk also retained. No device connected.

Final combined owner snapshot verification: 36/36 PlayMode tests passed (Fox finishes + Axolotl + Peach/walking/Claude UI); isolated collection branch 16/16 EditMode passed. Required portable/source checks and branch whitespace passed. APK build and six JNI methods passed; phone appearance, frame time and sensor behavior unverified.

## Single-color sushi base — ART-002 / UX-030

Owner requested a blank single-color sushi squishy base and selected a maki roll with shallow rice/filling relief. Claimed `codex/sushi-base`, extending finished Axolotl/Fox topics, files: tools/build-sushi-base.py, ArtSource/SushiBase/, models/SushiBase.glb, rendered preview and art notes. Local Blender authoring fallback because Node is present but npm/Meshy CLI is unavailable. One pale cream material, one connected smooth mesh, reusable UVs; no variants, face, catalog/inventory, Unity runtime or project settings edits.

Sushi base completed for shape review: local Blender fallback, one connected cream skin, UVs, GLB/FBX and editable authoring source. 6,001 vertices / 11,998 triangles, zero non-manifold edges. Three-quarter/top/front renders inspected; GLB metadata independently validated. No paid task, Unity/catalog/UI/Settings or inventory edits. See SUSHI_BASE.md.
