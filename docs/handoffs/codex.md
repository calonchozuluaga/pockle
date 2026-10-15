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

## UX-039 painted world environments — October 14
Owner requested image environments including a dark scene, environment reflection/refraction and rare/glitter light emission. Claimed on codex/painted-worlds in isolated checkout; files: runtime stage/environment asset loader, stage background shader, Pip/Fox finish shaders, generated world textures/reflection assets, editor importer/review captures, runtime tests/docs. Claude UI files remain untouched. Built-in imagegen used for original painted Fox Hollow, Jelly Garden and Moonlit Pond. PNG maps are LDR approximations, not HDR light probes or ray-traced transmission.

Painted worlds implemented locally on codex/painted-worlds. Original PNGs: Resources/Worlds/fox-hollow.png, jelly-garden.png, moonlit-pond.png; imagegen prompts in PAINTED_WORLD_PROMPTS.md. Existing World choices select them (Fox Home, Garden, Midnight); portrait crop keeps Midnight's moon visible. 128px baked environment cubes drive Pip/Fox reflections and approximate jelly refraction; local custom probe supports Classic Fox. Rare glow bodies add a soft surrounding halo and plate spill; glitter has a small warm night spill. No pulsing or extra inventory grants. No Claude HUD changes.

Verified: all 3 final stage runtime tests passed; full combined suite 41/43 (only the two previously recorded Claude UI failures). Required portable/source checks passed; real Unity image import/cubemap baking, shader renders and Android build passed; final night render inspected with visible rare halo. Walking class and all six JNI entry points packaged. Device visual/frame time review and green combined CI not verified. See PAINTED_WORLDS.md and docs/captures/painted-worlds. GitHub publishing still needs local authentication; no main merge.

Painted worlds saved at de7027b. Final APK Builds/Pockle-Painted-Worlds.apk installed with adb install -r Success; app launched and process 18763 confirmed. Unity exit initially interrupted ADB startup; installation was retried successfully after Unity finished. Saved collection preserved; phone visuals/frame time require owner review.

## UX-029/039 Pip owner grant — October 14
Owner requested the remaining Pip finishes, including Starlight Glow, for phone testing. Claimed codex/pip-owner-test: CollectionProgress, CollectionSession, temporary PipOwnerTestGrant resource, portable and grant regression checks, docs. Add missing copies only, persist before marker/UI, preserve duplicates/rewards/pending reveal. Claude's latest shelf changes and new StageViewport contract noted; framing remains separate, HUD preserved.

Pip owner grant saved on codex/pip-owner-test at 44fd8a7. Required portable/source/whitespace checks passed; Unity durable-grant test passed; final Android build and walking JNI packaging passed. Builds/Pockle-Pip-Test-Grant.apk installed with adb install -r Success. Startup briefly hit UnityFoldingFeaturesWrapper.init called twice (activity/engine startup, before grant); force-stopped only this app and relaunched fresh without clearing data. Phone disconnected before the grant log could be confirmed; 10/10 ownership is not claimed as observed on device. Enabled resource grants the missing Pip slots on next successful startup. Latest Claude local UI included in phone build but not edited/committed; StageViewport runtime request remains separate.

Pip phone verification after reconnect: running app process 14915. Navigated Home → Browse collections → Pip. Device UI shows Complete, 10 of 10 / 10 of 10 found; Starlight Glow says Yours to play and the banner says You found Starlight Glow. All ten Pip finish ownership confirmed on device, resolving the previous pending grant confirmation. No inventory writes, rebuild, reinstall or new grant during this check.

## ART-005 / UX-033 / UX-039 Fox art-set claim — October 14
Owner standard: exactly 10 Pockles (9 common + 1 rare included), 5 distinct collection-specific world images, one separate 3:1 text-free cover (toys may interact with environment), and an actual opening-ready 3D box plus transparent PNG renders. Owner chose Fox first and confirmed 3D model plus PNGs. Claimed codex/fox-art-set: catalog art/world metadata, environment loader/importer, Fox world/cover assets, box model/source/export and PNGs, portable/runtime checks/docs. Preserve existing Fox finishes/Aurora identity and rewards. No Claude HUD edits; send new metadata contracts in this handoff.

Owner also requested Christmas character sheets for Meshy (four original concepts, separate sheets) and made expressive animation mandatory for every toy: eyes/mouth, character-specific idles and touch reactions, with later sound-triggered expression support and calm alternatives. Recorded in COLLECTION_ART_STANDARD.md. Existing Pip face controls remain; Fox/Axolotl face/tail/limb animation support requires a dedicated implementation pass, not implied by imported static art.

Fox art-set assets ready: five Fox-specific worlds, separate Cover.png (3:1, no text), real hinged 1,253-triangle box prefab/GLB/FBX/.blend + closed/open transparent PNG renders. Latest uploaded Fox v2 tail/paint replaced the shared runtime mesh in place (30k tris / 20,141 UV-split verts), preserving all ten IDs/GUIDs/inventory. All ten finish captures inspected upright. New native box API: MysteryBox.Initialize("fox"), samples authored opening; Motion calm caps amplitude.

Claude contracts: CollectionDefinition.Worlds ordered immutable list of CollectionWorldDefinition { Id, DisplayName, EnvironmentArt }; first world is Home. Separate CoverArt="Collections/Fox/Cover", BoxArt="UI/Boxes/Collection-fox", BoxModel="Boxes/FoxCollectionBox", AccentColor and InkColor now exist. Please use CoverArt for the 3:1 banner (not the first world), all five Worlds for thumbnails, and an ID event to PrototypeStage.SetEnvironment(Camera, worldId, collectibleId). Legacy WorldChanged enums remain compatible until your picker integration. Closed box path already follows your convention. No HUD files edited. Five-world selection and cover integration in your new screens remain for your lane; StageViewport framing request remains separate.

Owner packaging correction: match pockle-folded-box-concepts-02, with prominent recognizable Pockle brand at top, question-mark silhouette tease, patterned surprise-toy printing. Revising actual Fox box panel textures and regenerating GLB/FBX/native prefab plus both PNG renders from the same geometry. Cover illustration remains separate and text-free. Christmas concepts saved separately on codex/christmas-concepts at cf31a40; Fox art-set baseline at 54e4a8b, normalized at 4500884. Updated Fox APK installed with data preserved; process 12269 confirmed. New five-world picker/cover hook remains for Claude.

Final surprise-box revision is applied locally: brand-first Pockle front/lid, unknown Fox '?' silhouette and side pattern, no revealed-toy cover on the packaging. Resources/Boxes/FoxFront.png / FoxSide.png / FoxLid.png are flat print maps. Actual model rebuilt to 874 triangles; PNGs refreshed at the same Collection-fox paths, so your new UI picks them up. Native opening API/resources unchanged; both targeted art-set tests passed after revision. Cover remains independent. Three print textures packed in ArtSource/FoxBox/FoxCollectionBox.blend. Builds/Pockle-Fox-Art-Set.apk updated with latest owner's local UI and all prior inventory grants; final device installation recorded below.

Final Fox mystery-box revision saved as d3d21bb; final APK installed with install -r Success, preserving inventory. Initial launch exited; a subsequent cold launch succeeded and process 20585 was confirmed. Device visuals/frame time and broader new Claude UI still require owner review. Christmas sheets are separate files in ArtSource/ChristmasConcepts, branch codex/christmas-concepts cf31a40. Full art-set docs and mandatory animation standard are retained; no main merge or GitHub push.


## ART-005 / UX-033 / UX-039 Pip art set — October 14
Claimed on codex/pip-art-set. Pip worlds/cover/native carton, catalog and loaders, editor importers, runtime tests and docs. Claude UI preserved.

Pip art ready: catalog HomeWorld pip-jelly-garden, Worlds five unique environments, CoverArt Collections/Pip/Cover, BoxArt UI/Boxes/Collection-pip, BoxModel Boxes/PipCollectionBox. Default reveal loads native carton. Request for Claude: consume catalog Worlds and distinct CoverArt; box-first UI finds PNG automatically. Change hardcoded Pip BannerWorld to catalog HomeWorld. UI files untouched.


Pip art set (October 14, codex/pip-art-set): Unity 6000.6.5f1 imports succeeded, native 1.958s opening/13 renderers; targeted PlayMode 8/8 passed (Pip/Fox art set, legacy presentation fallback, latest Pip collection). Required portable checks passed, source checks passed before final baked assets; final source/whitespace checks recorded below. Cover 2172×724, box PNGs 1024×1024 RGBA with transparent edges inspected. All five paintings and box prints/renders visually inspected. No phone update or combined UI/hosted CI verification in this task. Claude UI five-world picker and separate cover hookup pending; assets and metadata applied locally.

Final Pip art checks: targeted Unity PlayMode 8/8 passed; portable Core checks passed; source checks 109 C# files / 290 asset GUIDs, zero failures; git diff --cached --check passed. Phone build/install, combined UI and hosted CI not run. Branch codex/pip-art-set retained for review; no main merge.


ART-002/005 UX-033/039 Axolotl complete collection claim, codex/axolotl-art-set. Runtime materials/expressions, five worlds/cover/native carton, durable owner grant, tests/editor/docs; Claude UI preserved.


Claude latest live shelf note reviewed before publish: agree six spots (two rows of three), local presentation preference separate from inventory, auto-fill free spots for newly found toys. Current local PrototypeHud.LivingShelf still has four fixed slots/SceneOrder, so six-slot selection is not implemented yet. Empty backdrop supplied separately as Resources/UI/Shelves/LiveShelfBackground.png; see LIVE_SHELF_ART.md for actual support lines and aspect. No Claude HUD edits.

All three completed collections now supply five Worlds, first Home. Please use catalog HomeWorld for list banners, separate CoverArt for text-free collection cover, Worlds for Studio+five picker, and PrototypeStage.SetEnvironment for ID selection. Hardcoded CollectionLook.BannerWorld still uses old Pip/Axolotl shared paintings. Collection-axolotl PNG follows your automatic lookup convention. Native box and material runtime ready.

Combined owner-local snapshot PlayMode: 51/53 passed. Runtime ToyStageRuntimeTests had stale Pip HomeWorld=studio assertion (corrected to pip-jelly-garden). Remaining Claude test AnimatedShelfUsesRealMeshesAndStopsForCalmNavigationAndPause has RenderedFrameCount=0 at WaitForPreviewFrame line142 after scrolling to All your toys; please investigate layout/viewport/focus fixture. No edits to CollectionUiTests. StageViewport easing remains a separate runtime topic. No green combined CI claim.

Axolotl final pre-device validation: all ten finish/face/gill render captures inspected; five worlds/cover and carton prints/closed/open renders inspected. Initial targeted PlayMode 9/9 passed; final combined owner-UI snapshot 51/53 passed including new event/calm reaction test, all Axolotl tests and grant durability. Corrected stale Pip HomeWorld runtime assertion; focused follow-up 3/4 passed, with only Claude live-preview UI fixture still failing. EditMode 17/17 passed. Portable/source/whitespace passed. Android owner build succeeded (639812855 bytes), walking StepCounter + six JNI entries verified. No hosted CI or main merge. Phone installation follows below.


UX-033 shelf backdrop supplied separately on codex/live-shelf-background. Resources/UI/Shelves/LiveShelfBackground.png, 1143×1376; support lines approx39%/83% from top, measured on front plank edge; see LIVE_SHELF_ART.md. Claude requested approximate42%/88%; align portrait feet with visible edges without cover-cropping. Texture2D metadata preserves stable GUID; no shelf selection/UI behavior changed. Original built-in imagegen prompt documented.

Final device update: Builds/Pockle-Axolotl-Art-Set.apk installed with adb install -r Success on owner serial28141FDH3000L5, preserving data. Cold launch succeeded; process31246 confirmed. Unity device log: Pockle owner test: Axolotl finishes saved (10/10). Existing Fox10/10 also visible; phone frame time and full Axolotl on-device visual acceptance remain pending. Owner explicitly requested publish after latest Claude note review; push of codex/axolotl-art-set, codex/pip-art-set and codex/live-shelf-background failed with missing GitHub HTTPS Username. Branches remain local, no main merge. Axolotl commit2d0ff69; shelf backdrop c97123e; Pip104e130.

October 14 publish retry completed: GitHub CLI browser login authenticated calonchozuluaga and configured local HTTPS Git credentials. Push succeeded for codex/axolotl-art-set, codex/pip-art-set and codex/live-shelf-background. These branches are on origin; no main merge or green hosted-CI claim. New model inventory found firb.glb, coco.glb and snowy.glb; fourth filename requested from owner. Source models are static/unrigged (no skins/animation clips); import/art work is not represented as complete.
