# Codex handoff

## Clean Peach version — UX-025/030, ART-002

Owner uploaded a new Peach model after the seam repair. Preparing `codex/peach-clean-v2` from main `3c7b071d`; both handoffs read. Scope: Meshy asset/paint/provenance, conversion inspection, existing seam/import/grip/render checks, and phone-test notes. Owner explicitly approved publishing the converted mesh and texture for the phone build. Preserve the uploaded clean look. Keep the seam-safe weld/rebake pipeline and phone budget. Required local portable/source/whitespace checks pass. Unity/Android checks and owner phone review are pending.

[Previous seam repair and verification: PR #16](https://github.com/calonchozuluaga/pockle/pull/16). [Previous coordination history](https://github.com/calonchozuluaga/pockle/blob/3c7b071d1ee853997ddcc0d88a56e7631967dfbf/docs/handoffs/codex.md).

## October 13 — Fox collection

Implemented on `codex/fox-collection` in the attached isolated worktree; collection/runtime/check files copied to this local project for Unity use. Fox is collection/character `fox`; uploaded art is `fox.gloss-vinyl`, with nine planned finish slots. No inventory grants or box-pool changes. Unity 6000.6.5f1: 29 PlayMode and 16 EditMode tests passed; portable/source/branch whitespace checks passed. Android/phone review pending. Existing local settings and Claude UI edits preserved; owner approved the one collection-count test assertion update. See `docs/FOX_MODEL_IMPORT.md`.

## Latest Claude handoff — walking repair (UX-010/011)

Implemented on `codex/walking-plugin-fix`, with walking files copied into the local project. Old APK lacks StepCounter; exported Unity default library Gradle comments out the legacy Java source path. Explicit build.gradle fixes compilation, inherits SDK settings, and retains JNI entry points. Rebuilt previous exported APK passes the new DEX/API packaging checker; original APK fails it. `CollectionSession.WalkingFailed` is ready for your UI cleanup; development status includes exception type, log includes full exception. 17 EditMode and required portable/source checks pass. No connected phone, so logcat and actual step progress remain pending. Rebuild the current Unity project for all current C#/Fox/UI changes. Settings cleanup left untouched. Add `python3 tools/check-walking-apk.py <built.apk>` after CI packaging. See `docs/WALKING_PLUGIN_REPAIR.md`.

Review: appended personality/fusion navigation and optional shelf backdrop API accepted; shelf moments can stay in the local UI preference for beta. Older Pip shape notes are superseded by clean owner-uploaded Peach. Fusion runtime/save/ledger/art remains pending separately.

Walking fix validation on isolated main-based branch: 17/17 EditMode and 27/27 PlayMode tests passed; rebuilt previous exported APK contains JNI class/API and ACTIVITY_RECOGNITION. Current local C#/Fox/UI needs a fresh Unity build for phone testing. No phone connected. Details in `docs/WALKING_PLUGIN_REPAIR.md`.

## Owner Fox test grant — UX-029/030/033

Claimed on `codex/fox-owner-test`. Owner explicitly authorized a one-time local Fox award and necessary owned-HUD selection changes, preserving Claude Settings. Files: Core inventory, CollectionSession, temporary FoxOwnerTestGrant resource, HUD main/Shelf/Collections and theme, TactilePrototype, grant tests. Inventory persisted before refresh, existing saves/rewards preserved. Test grant resource must be disabled/removed before public distribution.

## Approved Peach expressions and visible filling — October 13

Implemented and applied locally from `codex/peach-expressions`: short blink, squint on compression, lifted expression; stronger moving reflections and transparent shell over six cream pearls/six gold flecks. Approved mesh/paint and other Pips unchanged. Unity 6000.6.5f1: 28/28 PlayMode and 16/16 EditMode tests passed; portable/source checks and fixed renders passed/inspected. New APK/device frame time unverified. See `docs/PEACH_EXPRESSIONS.md` and `docs/captures/peach-expressions/`.

Fox owner test award also applied to actual editor save (UI verified five toys, up from four). Grant and owned-ID HUD selection are on `codex/fox-owner-test`, 30 PlayMode tests passed. Resource `FoxOwnerTestGrant.txt` awards once on non-batch startup, preserves existing counts/steps/pending reveal, and must be disabled before public distribution. Owner explicitly approved the necessary HUD changes; Claude Settings cleanup preserved.

## Fox orientation and rigid controls — UX-025/030/033

`codex/fox-orientation-handling`: repaired native Fox mesh basis in place, preserving stable asset identity; owner local project updated without overwriting Peach, walking, Settings or other HUD changes. Fox body/plate drags turn only; play hint says “Drag Fox or the plate to turn.” No lift, stretch, squish or sag. Unity front render inspected upright. Regression/check results recorded after completion; phone rebuild pending.

Fox fix saved as `codex/fox-orientation-handling`: Unity 30/30 PlayMode tests, combined local portable/source checks passed. Upright front render inspected. Phone rebuild pending.

October 14 Fox finish implementation: all ten existing Fox slot IDs are available and named from the approved concept. Aurora is fox.glow-jelly / secret identity, no odds/pool/pricing changes. New independent finishes marker fills only missing copies; persists before UI. Necessary Fox HUD title/backdrop changes under earlier owner authorization, Settings preserved. See FOX_FINISHES.md. Axolotl import remains authorized next.

Fox finishes saved on codex/fox-material-collection at 9a231a2. Unity 32/32 PlayMode and portable/source checks passed; final owner-local APK built and StepCounter packaging check passed. File: Builds/Pockle-Fox-Finishes.apk; grant runs on next app launch. No connected phone.

Axolotl implementation applied locally: 30,000 triangles / 48,940 UV-split vertices, uploaded paint retained, native upright plush asset and catalog entry. Generic titles support the new collection; Settings unchanged. No Axolotl grant or reward-pool changes. See AXOLOTL_IMPORT.md.

Final owner test APK: Builds/Pockle-Fox-Axolotl.apk (37 MB), current local Peach/walking/Claude Settings plus Fox finishes and Axolotl. Android build and JNI packaging passed. Fox-only Builds/Pockle-Fox-Finishes.apk also retained. No device connected.

Final phone preview: Builds/Pockle-Fox-Axolotl.apk (37 MB). Combined snapshot passed 36/36 PlayMode tests; collection branch 16/16 EditMode and required portable/source/whitespace checks passed. Android build and walking JNI packaging passed. No phone connected; Fox awards run on app startup. Axolotl import is on codex/axolotl-plush; Fox finish topic on codex/fox-material-collection at 9a231a2.

Owner-selected single-color maki base completed on codex/sushi-base, deliverables copied locally to models/SushiBase.glb/.fbx and ArtSource/SushiBase/SushiBase.blend. One connected cream skin, 11,998 triangles, reusable UVs, no face/paint variants or runtime changes. Local Blender fallback, no Meshy task/credits. See SUSHI_BASE.md.

Latest Pip collection: owner confirmed all ten finishes, and authorized the single UI test assertion update. Same approved native mesh/paint across all ten; old Moon/Glitter/Mint art replaced under the existing IDs. Recolor/pearl/glitter/opaque/cloth/vinyl/metal/glow recipes, original expressions, latest unknown silhouette. No inventory/reward/settings changes. Runtime files applied locally; see LATEST_PIP_COLLECTION.md.

## Claude toy-stage handoff acknowledged during Pip collection work

Preserved owner-local new PrototypeHud.Play.cs / StageChoices.cs and its title/test edits. Names StageWorld { Home, Studio, Garden, Midnight } / StageLight { Day, Golden, Night } noted. World/Light runtime, higher stage framing and per-finish swatches remain a separate stage topic, not implemented in this material update. Local Pip APK includes your stage UI and fallback tint.

Your combined owner-local suite: 39 tests, 37 passed, two failures in CollectionUiTests: MissingLineupHasEagerSilhouettesWithoutFacesOrOwnershipChanges at line 270 (ActiveCount=0 after focusing first silhouette); ToyStageShowsCollectionAndSwitchesWorldAndLight at line 610 (no active Stage finish buttons). The latter checks collection panel children while trayTab defaults to TrayClosed; please open the Collection tab before asserting. Please investigate the silhouette viewport/focus setup in the new UI. Pip isolated suite passes including latest silhouette topology. No further Claude UI/tests edited beyond the owner's authorized Coming-soon assertion.

Runtime fix: ShelfPreviewRuntime previously accepted only legacy enum IDs and substituted Peach for other live actors. It now accepts registered available art and selects the actual collectible ID, supporting six new Pip finishes and already available Fox/Axolotl art without leaking undiscovered finishes. New regression checks owned/unowned art visibility. No inventory changes.

Pip final: 37/37 isolated PlayMode, 16/16 EditMode, portable/source checks passed. Full ten captures inspected; final owner-local Android build succeeded and walking JNI packaging passed. APK updated on connected phone with install -r, preserving data; launch confirmed. Visual screenshot unavailable while Dozing, device frame time unverified. New collection finish IDs do not grant copies. Combined new stage UI failures remain for Claude as recorded above.

Latest Pip topic saved as codex/pip-collection-latest at c769fa2 (approved Peach baseline incorporated separately). Final APK installed successfully as a data-preserving update; no automatic new Pip awards. Owner-local Claude stage UI preserved; two combined UI test failures are recorded for Claude, no merge performed.

Owner-approved Pip special: glow slot is now Starlight Glow · Ultra rare, stable ID pip.glow-jelly, collection secret identity. Existing secret UI badge/concealment uses this metadata. Shared approved mesh and aqua glow/iridescent material retained; no inventory grant, box odds or reward pool changes.

Pip special saved on codex/pip-special at 7ca84bc. Starlight Glow · Ultra rare marks pip.glow-jelly as the single Pip secret slot; existing badge/concealment picks it up. Portable/source/whitespace and Android build passed; existing model/material/save/reward pools unchanged. Updated APK retains prior stage UI limitations recorded above.

## UX-039 runtime claim — October 14
Working on codex/toy-stage-runtime in an isolated checkout, with an owner-local Claude snapshot. Files: PrototypeStage, new ToyStageEnvironment, TactilePrototype, Core collection/catalog metadata, Pip/Fox shaders, runtime tests and docs. Preserve Claude HUD/UI files. Implement existing WorldChanged/LightChanged contracts, scenery, plate/light looks, Motion calm, higher framing and catalog swatches.

UX-039 implementation applied locally: existing HUD events now control worlds/lighting; seven drifting leaves respect Motion calm; fixed Garden/Midnight scenery and themed plates; stage-only Pip/Fox night shader glow; higher framing. Catalog contracts: `CollectionDefinition.HomeWorld` is `fox-hollow` or `studio`; `CollectibleDefinition.SwatchColor` is a CatalogColor. Explicit APIs `PrototypeStage.SetWorld(Camera, StageWorld, string collectibleId)` / `SetLight(Camera, StageLight)`; bool return rejects invalid inputs. HUD already persists choices. See TOY_STAGE_RUNTIME.md.

Requests for Claude (your files preserved): use SwatchColor for owned finish dots while retaining unowned mystery concealment; change prospective Pip patch/Axolotl pond Home labels to Studio until art exists. Open Collection tray before checking active finish dots in ToyStageShowsCollectionAndSwitchesWorldAndLight. MissingLineupHasEagerSilhouettesWithoutFacesOrOwnershipChanges still returns ActiveCount=0 under the new UI; investigate its scroll/layout fixture. Combined Unity suite 40/42 passes, including both new runtime tests; the only failures are those two UI tests. No green combined CI claim.

Owner explicitly approved uploading the four requested branches. Push attempted to https://github.com/calonchozuluaga/pockle.git, but failed because Git could not read Username (no usable HTTPS login, gh unavailable). Branches remain local; authenticate GitHub locally before retrying. No force push or main merge.

Final UX-039: commit 1f50615 on codex/toy-stage-runtime (Claude owner snapshot c3c942d retained separately). Final Android APK walking JNI packaging passed; adb install -r returned Success, preserving app data, and process 10573/activity launch confirmed. Phone appearance/frame time still needs owner review. Whitespace check passed. Four requested branch pushes remain blocked by missing GitHub HTTPS authentication after explicit owner approval.
