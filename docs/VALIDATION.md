# Tactile prototype validation

## What has run in cloud

The machine has no Unity editor or activated Unity license. Cloud validation covers the real portable C# spring/deformation code, C# syntax, and local asset reference consistency. It cannot establish Unity import, API compilation, shader output, native builds, audio output, or physical-device responsiveness.

The verified .NET SDK 8.0.425 passed **29,837 core assertions** plus **104,048 assertions against the actual Blender-exported character mesh**. These cover complete/wound surfaces, indices, UVs, a single connected crown/body, the vertex budget, safe coordinates, finite deformation, anchored base, bulk volume, rest recovery, and full-stretch crown bounds. Unity EditMode tests and device checks remain unrun in cloud.

Blender 4.3.2 generated the editable scene, character source, FBX, and an offline model study. Re-importing the FBX into Blender verified all 20 character parts, 2,909 logical body vertices, 5,814 triangles, a UV map, and no studio objects. Maximum body-position difference was 0.000000267 meters against the explicit exported source. The runtime seam-split body has 3,158 vertices. Source checks passed 18 Unity C# files and 45 GUIDs, including the custom importer reference. These checks do not establish Unity importer execution or shader parity.

The owner reported that the original procedural Pip appeared on their computer and its controls felt responsive. The first Android APK was being built; its device result has not yet been reported. The owner subsequently confirmed that the first authored character loaded in Unity and supplied a screenshot. It showed overlapping crown transparency and hard blush; the second art pass addresses those differences. The owner subsequently reported that the revised model was much closer, and supplied a screenshot showing a still-matte shell. The latest viewer update adds static studio reflections and changes its controls; final Unity shader appearance and Android performance remain unverified.

The turntable update additionally passed **531 interaction assertions**: initial surface/finger capture, rejection of takeover by a second finger, release/lifecycle cancellation, triangle front-face picking, and picking the actual authored shell at rest, squash, stretch, and a quarter turn. Empty space within the old spherical hit region no longer starts a squeeze. These checks exercise the production core functions; they do not execute Unity Physics, input dispatch, layout, or shaders.

The dependency-free .NET runner reports its assertion count and exits nonzero on any failure. Unity EditMode tests are included separately and remain unrun until an editor is available. No tests are skipped or replaced to produce a passing result.

## Local editor

1. Use the installed Unity 6.6 editor version `6000.6.5f1`, matching the project pin, and open the project using the README steps. Confirm platform requirements before native phone builds.
2. Resolve packages, prepare settings, and open `PockleTactile.unity`. The Console must have no compile or shader errors. Confirm the built-in rendering pipeline and Input Manager are active.
3. Run the EditMode test assembly. All thirteen cases must execute and pass; a zero-test run is not validation. The three new cases load the imported character, check readable mesh/UVs/normal groups, verify feature anchors and full-stretch bounds, and exercise the authored flat base.
4. Press Play. A peach Pip must appear after the box reveal with eyes, cheeks, smile, and a grounded body. Check that the face remains attached during squash/stretch and there are no pink materials or disappearing features.
5. Hold the toy, drag upward and sideways, and release. Motion should respond immediately, stay bounded, and return to rest. Press **Reset** during a gesture and during the reveal; it must restore the visible toy and re-enable **Reveal again**.
6. Confirm there are no Touch/Rotate mode buttons. Drag the exposed front or side of the plate to rotate Pip and the rim marks together. Turning must not start a squeeze, release sound, or haptic. Drag directly on Pip to squish/stretch; moving onto the plate must keep the squish gesture. Start on the plate then drag onto Pip: it must keep turning. Release must stop turning immediately. Interacting with settings/actions or empty background must not capture either gesture. Reset restores the plate and Pip orientation.
7. Toggle sound off during a sound and verify it stops. With sound on, press, release, and reveal should be audible. Toggle reduced motion: direct deformation remains, but rebound and large reveal movements disappear. Stop and restart Play to verify preference persistence.
8. Inspect 320×568, 390×844, tablet portrait, and tablet landscape sizes, including safe-area insets. The character and controls must remain visible; reveal and maximum stretch should not cover labels.

For the new art pass, choose **Pockle → Character → Use authored Pip** before Play and confirm **Pip: authored mesh · 3158 vertices** in the Console. Check the joined asymmetric crown, broader flat base, glazed plum eyes, diffused blush, broad reflections, facial attachment, and UV-seam lighting under maximum deformation. Confirm the crown has no overlap band at rest or during rotation. Check front/side/back views on both cream backgrounds and the reveal box. Toggle **Use procedural baseline** and restart Play to compare responsiveness. Finish the existing APK build before pulling/importing changes; keep that APK for the Android baseline comparison.

## Android / iOS phone

Install the target's Unity build support through Hub. Android requires its supported SDK/NDK/JDK; iOS requires a compatible Mac, Xcode, signing setup, and device provisioning. Cloud code generation does not supply these prerequisites. Use a Development Build for the first profile; choose an appropriate package identifier and signing identity locally before distributing anything.

1. Build and install on a real midrange phone. Cold start and reveal must succeed; check retained shaders in the player rather than relying on the editor view.
2. Repeat the touch, rotation, reset, and preferences checks. Add a second finger during a press; the first finger remains in control. Cancel a touch, background the app while holding, and return; the toy must not stay held.
3. Enable haptics. Verify brief release/reveal vibration if the device supports it, then disable it. Simulator/editor behavior is not evidence of device haptics. Basic `Handheld.Vibrate` behavior varies by hardware and OS.
4. Rotate the device between portrait and landscape, including during a gesture. Check notches/home indicators, all buttons, toy centering, and maximum stretch.
5. Connect the Unity Profiler, warm up, then manipulate for at least 60 seconds. Aim for stable 60 fps on the selected phone, or explicitly record a lower target if needed. Record median/p95 frame time, CPU deformation cost, render-thread cost, draw calls, and memory. Do not infer speed from the mesh's small vertex count. Investigate recurring managed allocations during steady interaction.
6. Record the editor version, device, OS, build target, observed failures, and whether the toy feels responsive. A playable milestone requires this device check; numeric tests alone do not prove a satisfying tactile experience.

## Deliberate prototype limits

- The authored model is a first art pass; Unity material appearance and its new importer require local validation. Synthetic tones and a simple reveal remain placeholders.
- Visible-shell picking is calculated only on pointer-down using current cached vertices; it does not rebuild a collider during deformation. The approximate sphere component is no longer used to choose the gesture. Pointer capture continues a valid drag outside the initial silhouette. Profile pointer-down cost as well as steady interaction on Android.
- Transparency is a lightweight material approximation, without refraction or physically correct subsurface scattering. The authored crown no longer overlaps a separate shell. Inspect internal accent sorting, reflections, and blush on target hardware. Rotate through front, side, and back and confirm bright studio highlights move across the wet shell. There must be no pink materials, missing cubemap, or blown-out white toy on Android. The cubemap uses RGBAHalf where supported and RGBA32 otherwise; that fallback is not yet device-tested.
- Haptics are basic vibration, not tuned iOS/Android native patterns.
- Saved data consists of comfort preferences. Inventory, edition issuance, and the collecting loop belong to later milestones.
