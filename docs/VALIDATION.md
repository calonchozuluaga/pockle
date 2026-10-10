## Settled Pip belly correction (October 10, 2026)

Owner rejected the .129-radius underside from `5ee03f4`: it appeared pointed instead of naturally resting. `codex/pip-jelly-device-pass` now has a broad .587-radius shallow contact area, rounded lower corners and fuller lower volume, with regenerated source/FBX/runtime mesh and face projection. New checks reject a tiny pole patch and require rounded corners above a broad support area; picking covers both the lower belly and the exposed plate corner. No tactile physics, HUD, CI, packages or settings changed in this correction.

Passed locally: **550,284 portable assertions** across eleven suites; FBX round-trip with 20 parts, 2,939 logical vertices / 5,874 triangles and maximum position error .000000254. Metadata synchronization preserved all GUIDs. Current mesh: 3,180 exported vertices. Source checks passed: 66 C# files / three profiles / five assemblies / 142 GUIDs / zero failures. Changed-file whitespace passed. Initial correction run `38069906036` produced actual captures and passed contact/deformation tests, but failed the 6,000-triangle budget with 6,064 triangles. The final decimated export has 5,874 triangles, preserving the .587 contact radius. Final hosted correction tests/captures and Android packaging are pending at publication. The previous run `38066481266` passed 34/34 Unity tests and Android packaging; its captures demonstrate the rejected shape, not approval of this correction. Phone appearance/feel/performance remain owner checks.

## Rounded jelly Pip and Android startup (October 10, 2026)

Branch `codex/pip-jelly-device-pass`, stacked on v2 integration. Removed primitive construction's implicit physics dependencies; the stage retains its explicit touch collider. Regenerated rounded Pip source/FBX/runtime geometry and reprojected face anchors, tuned the four finishes and radial contact shadow. Passed **538,252 portable assertions**, including contact size, face projection, closed winding/UV/volume, deformation and picking; **66 C# files / three profiles / five assemblies / 142 GUIDs / zero source failures**; metadata/whitespace; Blender FBX import (20 parts, 2,868 logical vertices, 5,732 triangles, error at most .000000227).

The collider-only commit passed hosted Unity compilation/tests; the final geometry/shader branch requires its own CI and Android build. New PlayMode startup coverage verifies the plate hit plane and missing implicit colliders, winding and resource disposal. `PipJellyCaptureTests` renders all four finishes plus a grip pose into the existing CI test-results artifact, when a graphics device is available. Unity caught a wrong namespace and then the missing PNG encoding module in this new fixture; both are corrected before another run. The project enables the bundled `com.unity.modules.imageconversion` 1.0.0 for PNG review captures, with no external renderer/post-processing dependency. Actual combined captures have not yet been reviewed at publication. Device startup, appearance, touch and frame cost remain unverified; [PIP_JELLY_REVIEW.md](PIP_JELLY_REVIEW.md) contains the exact owner checks and comparison protocol.

## Collection runtime contracts and backend design (October 10, 2026)

### Combined v2 UI and collection routes

`codex/v2-collection-integration` combines Claude's `d697e12` v2/catalog snapshot, later `de285df` collection notes and PR #9 runtime contracts. All eleven portable suites passed (**549,003 assertions**). Source checks passed (**63 C# files / three profiles / five assembly definitions / 139 GUIDs / zero failures**). Fonts match their remote Git blob hashes; metadata synchronization and whole-tree whitespace passed. The unchanged packaging concept PNG is retained by its existing remote blob when publishing; it is not used by runtime or local tests.

New UI PlayMode coverage is authored for Collections/Collection reachability, slot availability, shared static previews, disabled viewer backdrop changes/reset, Back/scroll and tab clearing without inventory changes. PR #9's CI completed successfully, including Unity; the combined branch needs its own Unity run. Phone visual/touch/performance and Android packaging remain distinct. See [V2_PHONE_TEST.md](V2_PHONE_TEST.md).

Branch `codex/collection-backend-contracts` from verified remote main `b8a119c`. Executed all ten portable suites: **417,710 assertions**, including **42 menu/profile assertions** with Collection/Collections Back, independent scroll, main-tab reset and invalid destination rejection. Source checks passed: **57 C# files / three symbol profiles / five assembly definitions / 133 GUIDs / zero failures**. Metadata synchronization created one `.meta` and preserved all existing GUIDs. `git diff --check` passed. SDK: prepared `/workspace/.tools/dotnet/dotnet` 8.0.425; `DOTNET_CLI_HOME` and `NUGET_PACKAGES` point to writable prepared workspace directories.

Authored but unrun locally: `StageBackdropTests`, covering the viewer and full-screen clear camera through a framing update, default reset, bad RGB rejection, camera isolation and destroyed-stage cleanup. Unity editor/API compilation, colour-space appearance, HUD integration, Android builds/frame time and hosted CI remain separate verification. Backend schema/function design and acceptance cases are documentation only: no SQL migration, live project, auth integration, RLS tests or concurrency execution is claimed.

## Series metadata and lineup motion (October 9, 2026)

Executed: **130,254 series/motion assertions**, plus 418,742 assertions across the existing suites (**548,996 total**). The expanded catalog suite now reports 1,296 assertions and material handling 9,705 because the planned catalog has ten varieties per character. Coverage includes one character per collection, at least ten members per catalog collection, Pip in Series 1, immutable membership and disclosed probabilities, positive/unique prize validation and overflow-safe totals, null release dates, UTC date boundaries, permanent collections, a deterministic 72,000-draw 1-in-72 secret fixture, all twelve idle/eager loop profiles, bounded nonfinite/extreme-time behavior, calm neutrality, zero sampling allocations, and shared animation slot limits/reuse. The existing four-Pip inventory migration and daily odds still pass unchanged.

Source checks pass for **61 C# files**, three symbol profiles, five assembly definitions, and **137 GUIDs**. Four Unity PlayMode cases are authored but unrun: beta offer mapping to Pip, visibility/calming/restoration, reconfiguration/destruction, and disabled toy cleanup. Unity compiler/lifecycle execution, rendering and motion feel, actual live lineup UI integration, Android packaging, and performance remain unverified. No licensed Unity editor is available here. See [exact APIs and Claude integration guidance](SERIES_AND_MOTION_API.md).

## Editor assembly reference correction (October 9, 2026)

The owner supplied a Unity Console error: `PipCharacterImporter.cs(63,18): CS0234`, because `Pockle.Editor.asmdef` referenced Runtime but not Core while the importer directly called `Pockle.Core.ToyCatalog`. Added the required direct Core reference. This is a confirmed Unity compilation failure in the previous merged build, despite the syntax/geometry checks passing.

Extended the portable source checker to inspect project namespace uses against unambiguous asmdef root namespaces and direct assembly references (names or GUIDs). With the old assembly definition it exits 1 and identifies exactly the missing Editor → Core reference; after the fix it passes: 56 C# files, three symbol profiles, five assembly definitions, 132 GUIDs. All 417,703 portable assertions passed again. Whitespace checks also pass. This dependency check does not resolve every symbol or compile Unity APIs; actual Unity reimport/compilation and an Android rebuild still need local confirmation. The owner's UnityConnect token-exchange and ADB service messages are separate from this assembly error.

## Combined Claude/Codex integration (October 9, 2026)

Executed against the merged Home/shelf UI, ID integration, roster concepts, CI, and Moss/Bop/Nook study tree:

- Portable suites: 29,837 core, 104,048 authored geometry, 531 pointer interaction, 114,326 manipulation, 6,031 reveal, 124 collection, 152,809 weight, 35 menu/profile, 1,224 catalog, and 8,738 material assertions; all passed (417,703 total).
- C# source checks: 56 files across default, Editor, and Android symbol profiles; 132 asset GUIDs; zero failures. These check syntax/references, not Unity API compilation.
- `python tools/check-character-studies.py`: Moss 2,840 vertices / 5,144 triangles, Bop 3,068 / 5,516, Nook 2,564 / 4,528; all connected, closed, within budget, with finite UVs and valid front anchors.
- CI's whole-tree whitespace check caught two trailing spaces in font-license text; removed the spaces without changing the license prose. The same whole-tree check passes after the cleanup.

GitHub check-runs could not be inspected: the API returned Forbidden. Local results do not establish hosted CI success. No Unity editor/license is available in cloud, so Unity compilation, shader rendering, EditMode/PlayMode execution, Android APK builds, font/texture imports, UI layout, and device performance remain unverified. Settings → About identifies this UI as **Home and shelf 01**. Review the Home/shelf locally and use **Pockle → Character studies** in the editor for the provisional models; they remain unavailable on the ordinary Android shelf.

## v2 screens 01 (UX-032, Claude)

Executed: a Mono type-check of every `PrototypeHud*.cs` partial, `PockleTheme`, `SquishFeedback`/`SquishMesh`, `GuestProfile`, `BoxCatalog`, `PipVariant`, and Codex's real `Pockle.Core` sources, against hand-written Unity API stubs (names and types only, not real Unity signatures). `git diff --check` and the meta sync pass. Portable checks and the Unity suites run in CI on the pull request.

Updated PlayMode tests (run in CI): shelf selection and unavailable checkout through the price button; Play round trip restoring the shelf scroll; Settings opened from You with the volume preserved when muted via the sound switch; profile name, avatar, and favorite (set with the heart while playing) persisting across a restart; Home greeting by time of day; Missing/Duplicates shelf views.

Not verified until the owner tries it on a phone: how close each screen looks to the canvas, Bricolage import and weights, rounded radii from the sliced sprite, portrait colour matching its tile (linear vs. gamma colour space), the rotated box art, and layout at 320×568 and tablet sizes.

## Home and shelf 01 (UX-003/004, Claude)

Executed: a type-check of all HUD partials, `Runtime/UI/*.cs`, `GuestProfile`, `BoxCatalog`, and Codex's real `Pockle.Core` catalog/progress/profile sources (nullable annotations stripped for the older compiler), against hand-written Unity API and `CollectionSession` stand-ins (passes); glyph coverage of both bundled fonts for `×`, `·`, `’`, and accented Latin (present; CJK falls back to system fonts); a rendered design mock of Home (walking and ready) and the shelf (complete and with undiscovered toys) at 390×844.

Authored, unrun: PlayMode test `HomeAndShelfReflectProgressAndKeepUndiscoveredToysASurprise` (ready-box action, Rewards status, collection summary, discovery count, hidden names, disabled undiscovered toy, duplicate badge, portraits only for owned toys). Shelf/favorite captions in existing tests changed from `MOON JELLY` to the new title case `Moon Jelly`.

Not executed: Unity compilation, layout on screen, masks on the Home mini portraits, and phone review. Device checks: Home at 320×568 (half tiles stack), 390×844, and tablet; the ready-box transition; the shelf with 0, 1, and 4 owned toys (edit the save or use **Pockle → Testing**); favorite heart after changing it in **You**.

## Visual system 01 (UX-002, Claude)

Executed: a type-check of all `PrototypeHud*.cs` partials and the new `Runtime/UI/` files with the Mono C# compiler against hand-written Unity API stubs (catches syntax, typos, and type mismatches in the changed code; does **not** prove the real Unity signatures). Rendered the wordmark and icons and reviewed them visually. CI's portable/source checks run on the pull request.

Not executed: Unity compilation, the PlayMode UI suite, font/texture import, on-screen layout, press animation, and any phone check. Follow the review steps in [VISUAL_SYSTEM.md](VISUAL_SYSTEM.md) and the existing [Home hub device review](HOME_UI.md); Settings → About should read **Visual system 01**.

## Home hub 01: navigation, Settings, and local profile

Executed the portable suite, including **35 new menu/profile assertions** covering Home cold start, contextual Back, Settings → Play → Shelf, independent scroll restoration, tab history, repeated taps, reward/social routes, invalid scroll, Unicode name normalization, and emoji-safe truncation. All existing spring, authored geometry, pointer, manipulation, reveal, collection, and weight checks still pass.

C# source checks pass for **44 files across default, Editor, and Android symbol profiles**, with **86 asset GUIDs** and no syntax/metadata failures. `git diff --check` passes. The daily progress fill now has a sprite, which is required for UGUI's Filled image path rather than an unfilled plain quad.

The Unity PlayMode UI suite now starts from Home and adds profile persistence/restart, inventory preservation on social/profile routes, volume/mute preservation, play/settings Back, shelf scroll, and root exit behavior. These tests are **authored but unrun**: the cloud does not have a licensed Unity editor. No Unity API compilation, UI rendering, native keyboard/Back dispatch, or Android APK validation is claimed for this pass. Follow [Home hub device review](HOME_UI.md). About shows **Home hub 01** to distinguish the rebuilt UI from older installed APKs.

## More expressive toy tuning

Executed the portable suite: 114,326 manipulation checks and 152,809 weight/sag checks, including stronger deliberate shakes, bounded sustained motion, high/low impact scaling, visible rebound followed by settling, and maximum sag clearance on the authored mesh. Source checks passed for 37 C# files across default, Editor, and Android profiles, with 79 asset GUIDs and no failures. Unity/device validation of the new sound and exaggerated feel is pending the owner's return; calm mode retains its existing behavior. The previous APK build failure succeeded on the owner's second attempt; no unverified Gradle fix was applied.

## Weighted lift update

Executed the complete portable suite with 152,104 new weight/sag assertions, plus C# syntax/assets across default, Editor, and Android branches. The Unity variant pose test now includes sag and remains unrun in cloud. Follow [the weight/device checklist](WEIGHT_AND_GRAVITY.md) for grip support, higher drops, regrab, handoffs, clearance, and calm mode.

## Collection UI beta (October 8, 2026)

Executed: the full portable suite, including 124 walking/inventory assertions; C# syntax and asset references; Android Java compilation against the checksum-verified official API 23 jar; `git diff --check`. New PlayMode UI tests cover shelf portraits/selection, unavailable checkout without awards, and one-time daily unlocks. Unity is unavailable in cloud, so these PlayMode tests, actual UI rendering, Android library import/packaging, physical step counting, background restrictions, and phone layout are unrun here. Follow [the local collection checklist](COLLECTION_UI.md).

# Tactile prototype validation

## What has run in cloud

The machine has no Unity editor or activated Unity license. Cloud validation covers the real portable C# spring/deformation code, C# syntax, and local asset reference consistency. It cannot establish Unity import, API compilation, shader output, native builds, audio output, or physical-device responsiveness.

The verified .NET SDK 8.0.425 passed **29,837 core assertions** plus **104,048 assertions against the actual Blender-exported character mesh**. These cover complete/wound surfaces, indices, UVs, a single connected crown/body, the vertex budget, safe coordinates, finite deformation, anchored base, bulk volume, rest recovery, and full-stretch crown bounds. Unity EditMode tests and device checks remain unrun in cloud.

Blender 4.3.2 generated the editable scene, character source, FBX, and an offline model study. Re-importing the FBX into Blender verified all 20 character parts, 2,909 logical body vertices, 5,814 triangles, a UV map, and no studio objects. Maximum body-position difference was 0.000000267 meters against the explicit exported source. The runtime seam-split body has 3,158 vertices. Source checks passed 26 Unity C# files and 58 GUIDs, including the custom importer reference, variant test assets, store preview assets, manipulation code, and branded box reveal. These checks do not establish Unity importer execution or shader parity.

The owner reported that the original procedural Pip appeared on their computer and its controls felt responsive. The first Android APK was being built; its device result has not yet been reported. The owner subsequently confirmed that the first authored character loaded in Unity and supplied a screenshot. It showed overlapping crown transparency and hard blush; the second art pass addresses those differences. The owner subsequently reported that the revised model was much closer, and supplied a screenshot showing a still-matte shell. The latest viewer update adds static studio reflections and changes its controls; The owner then reported that the new shiny shell and filled appearance looked much better in Unity. This is a desktop visual observation; the latest controls, shader compilation on Android, frame time, and sensor behavior still need device testing.

The turntable update additionally passed **531 interaction assertions**: initial surface/finger capture, rejection of takeover by a second finger, release/lifecycle cancellation, triangle front-face picking, and picking the actual authored shell at rest, squash, stretch, and a quarter turn. Empty space within the old spherical hit region no longer starts a squeeze. These checks exercise the production core functions; they do not execute Unity Physics, input dispatch, layout, or shaders.

The dependency-free .NET runner reports its assertion count and exits nonzero on any failure. Unity EditMode tests are included separately and remain unrun until an editor is available. No tests are skipped or replaced to produce a passing result.

The second-variant update adds Moon Jelly and live Peach/Moon selection. The existing portable suites passed again. Its two PlayMode tests cover repeated switches preserving the body mesh/material and deformation, removal of old filling, resource cleanup, and startup from the saved variant. Both remain unrun in cloud, as do Moon's visual and Android checks.

The store preview adds closed collection boxes and proposed prices, with scrolling and navigation back to the viewer. Its source and GUID checks passed. Follow [the store checklist](STORE_PREVIEW.md) locally to verify texture import, responsive layout, and blocked toy input while browsing. No purchase flow is implemented.

The lift/pinch/shake update additionally passed **114,325 manipulation assertions**: explicit second-finger capture and either-finger handoff, cancellation, rejection of plate/third-finger takeover, gravity/noise filtering, bounded spikes and spring motion, settling/recovery, and directional deformation of the authored mesh with finite bounds, a planted foot, and approximate bulk volume, including combined press and wobble. These execute production portable code. Unity input dispatch, moving shadows, accelerometer delivery, and actual device rendering require [the local interaction checklist](MANIPULATION_TEST.md).

The branded Jelly Garden opening additionally passed **6,031 reveal assertions** against its production sequence: closed start, opening order, finite bounds, emergence, settled handoff, replay, delayed/invalid elapsed time, and a shorter calm sequence without rise/scale/bounce. The existing suites passed again. The printed carton geometry, texture import, modified reveal shader, fade sorting, and controller handoff still require [the local box checklist](BOX_REVEAL.md).

The finish-variety update adds Gold Glitter and Mint Soft without changing the portable interaction code. Source/GUID checks passed (26 files / 58 GUIDs). The two PlayMode variant tests now cycle all four finishes and check material/depth restoration, filling cleanup, pose preservation, and saved startup. They remain unrun in cloud. Follow [the finish comparison checklist](PIP_VARIANTS.md) for opaque face occlusion, restored transparency/reflections, glitter aliasing/seams, and Android shader performance.

## Local editor

1. Use the installed Unity 6.6 editor version `6000.6.5f1`, matching the project pin, and open the project using the README steps. Confirm platform requirements before native phone builds.
2. Resolve packages, prepare settings, and open `PockleTactile.unity`. The Console must have no compile or shader errors. Confirm the built-in rendering pipeline and Input Manager are active.
3. Run the EditMode test assembly. All thirteen cases must execute and pass; a zero-test run is not validation. The three new cases load the imported character, check readable mesh/UVs/normal groups, verify feature anchors and full-stretch bounds, and exercise the authored flat base.
4. Run the two tests in the **PlayMode** assembly `Pockle.Variants.PlayMode.Tests`. Then press Play. Pip must appear in the saved variant after the box reveal with eyes, cheeks, smile, and a grounded body. Check that the face remains attached during squash/stretch and there are no pink materials or disappearing features. Tap **Peach / Moon**: the label and filling must update while the pose and plate angle stay unchanged. Compare reflections through a full turn, repeat switches, and restart Play to verify the saved choice. Follow [the variant checklist](PIP_VARIANTS.md).
5. Hold the toy, drag upward to lift the whole body off the plate, drag sideways to lean, and release. The shadow should change with lift height and the toy should settle onto the plate. Press **J** with Game view focused to preview a jiggle pulse; this does not test a sensor. Press **Reset** during a gesture and during the reveal; it must restore the visible toy and re-enable **Reveal again**.
6. Confirm there are no Touch/Rotate mode buttons. Drag the exposed front or side of the plate to rotate Pip and the rim marks together. Turning must not start a squeeze, release sound, or haptic. Drag directly on Pip to squish/lift; moving onto the plate must keep the toy gesture. Start on the plate then drag onto Pip: it must keep turning. Release must stop turning immediately. Interacting with settings/actions or empty background must not capture either gesture. Reset restores the plate and Pip orientation.
7. Toggle sound off during a sound and verify it stops. With sound on, press, release, and reveal should be audible. Toggle reduced motion: direct deformation remains, but rebound and large reveal movements disappear. Stop and restart Play to verify preference persistence.
8. Inspect 320×568, 390×844, tablet portrait, and tablet landscape sizes, including safe-area insets. The character and controls must remain visible; reveal and maximum stretch should not cover labels.

For the new art pass, choose **Pockle → Character → Use authored Pip** before Play and confirm **Pip: authored mesh · 3158 vertices** in the Console. Check the joined asymmetric crown, broader flat base, glazed plum eyes, diffused blush, broad reflections, facial attachment, and UV-seam lighting under maximum deformation. Confirm the crown has no overlap band at rest or during rotation. Check front/side/back views on both cream backgrounds and the reveal box. Toggle **Use procedural baseline** and restart Play to compare responsiveness. Finish the existing APK build before pulling/importing changes; keep that APK for the Android baseline comparison.

## Android / iOS phone

Install the target's Unity build support through Hub. Android requires its supported SDK/NDK/JDK; iOS requires a compatible Mac, Xcode, signing setup, and device provisioning. Cloud code generation does not supply these prerequisites. Use a Development Build for the first profile; choose an appropriate package identifier and signing identity locally before distributing anything.

1. Build and install on a real midrange phone. Cold start and reveal must succeed; check retained shaders in the player rather than relying on the editor view.
2. Repeat the touch, rotation, reset, and preferences checks. Add a second finger that starts on Pip, pull apart and pinch together along horizontal/vertical/diagonal axes, then remove either finger. The survivor must continue a rebased drag. A second touch on the plate/background/UI and a third finger must not take over. Gently shake the phone in Full motion and check bounded jiggle/settling; a stationary phone and Calm motion must stay quiet. Follow the complete [manipulation checklist](MANIPULATION_TEST.md). Cancel a touch, background the app while holding, and return; the toy must not stay held.
3. Enable haptics. Verify brief release/reveal vibration if the device supports it, then disable it. Simulator/editor behavior is not evidence of device haptics. Basic `Handheld.Vibrate` behavior varies by hardware and OS.
4. Rotate the device between portrait and landscape, including during a gesture. Check notches/home indicators, all buttons, toy centering, and maximum stretch.
5. Connect the Unity Profiler, warm up, then manipulate for at least 60 seconds. Aim for stable 60 fps on the selected phone, or explicitly record a lower target if needed. Record median/p95 frame time, CPU deformation cost, render-thread cost, draw calls, and memory. Do not infer speed from the mesh's small vertex count. Investigate recurring managed allocations during steady interaction.
6. Record the editor version, device, OS, build target, observed failures, and whether the toy feels responsive. A playable milestone requires this device check; numeric tests alone do not prove a satisfying tactile experience.

## Deliberate prototype limits

- The authored model is an art pass; Unity material appearance and its importer require local validation. Synthetic tones remain placeholders. The branded hinged reveal is included and needs a local player check.
- Visible-shell picking is calculated only on pointer-down using current cached vertices; it does not rebuild a collider during deformation. The approximate sphere component is no longer used to choose the gesture. Pointer capture continues a valid drag outside the initial silhouette. Profile pointer-down cost as well as steady interaction on Android.
- Transparency is a lightweight material approximation, without refraction or physically correct subsurface scattering. The authored crown no longer overlaps a separate shell. Inspect internal accent sorting, reflections, and blush on target hardware. Rotate through front, side, and back and confirm bright studio highlights move across the wet shell. There must be no pink materials, missing cubemap, or blown-out white toy on Android. The cubemap uses RGBAHalf where supported and RGBA32 otherwise; that fallback is not yet device-tested.
- Haptics are basic vibration, not tuned iOS/Android native patterns.
- Saved data consists of comfort preferences and the selected comparison variant. Inventory, edition issuance, and the collecting loop belong to later milestones.
