# Pip concept pass and Android startup review

October 10, 2026. Test branch: `codex/pip-jelly-device-pass`, stacked on the v2 collection integration. Use Unity **6000.6.5f1**, Built-in RP. Keep current local Android settings when switching branches.

## Changes to inspect

- Stage disks and the carton floor now use owned meshes instead of `CreatePrimitive`. No implicit CapsuleCollider or BoxCollider is constructed. The plate still has one MeshCollider, shared stage geometry is released on teardown, and its top remains at y=.15.
- Pip now settles onto a broad shallow belly at y=-1, with smoothly rounded lower corners. Contact radius is now about **.632**; the preceding settled-belly pass used .587. The previous **.129** pole patch was rejected by the owner because it read as a pointed egg; the older **.706** flat foot also lacked the soft roundover. The new floor width is about 67% of the body width, with fuller volume immediately above it. The joined crown stays part of the body. Face anchors are reprojected on the regenerated surface; the Blender source, FBX and runtime mesh agree.
- The PR #12 brief now drives narrower shoulders, a broad low belly, round small-left/large-right crown lobes, lower/wider eyes, larger low blush and reprojected face anchors. Peach has a pale top and coral lower third, a soft upper-right studio reflection and subdued edge light. Larger milky pearls and gold flecks sit inside the lower body. Moon and Gold share the softer reflection; Mint remains opaque. No refraction, bloom or extra shell is introduced.
- The existing contact shadow draw is wider and lighter, and fades radially using the retained blush shader; it follows the existing lift/landing behavior. This pass does not add new idle animation or rock physics.

## Real Unity captures

Run PlayMode test `PipJellyCaptureTests.FourFinishesRenderOnTheirColourFieldsAndKeepGripPicking`. With a graphics device, it writes 720×960 PNGs to `test-results/pip-jelly/` at the project root: PeachJelly, MoonJelly, GoldGlitter, MintSoft, PeachJelly-front-reference (neutral background), and PeachJelly-grip. CI includes that directory in its existing `unity-test-results` artifact. A Null graphics device explicitly skips this test; a skipped capture is not render evidence.

Compare Peach/Moon against `docs/concepts/pip-character-sheet-01.png`, and Peach against `docs/concepts/pip-peach-jelly-concept-01.png`. The concept uses offline lighting; the PNGs use the actual Unity mesh/material/camera. Review the underside, face contact, sheen, colour saturation, interior filling and silhouette. These are render evidence, not a phone performance measurement.

The project enables Unity's bundled `com.unity.modules.imageconversion` **1.0.0** for PNG encoding. This is the only package-manifest change; no render pipeline or external post-processing package is added. The capture fixture also emits compact 240×320 gzip/RGB previews in CI logs (`PIP_JELLY_PREVIEW`) so connector-only reviewers can inspect actual renders when ZIP references cannot be opened locally. Full-resolution PNGs remain in the artifact. A first combined run caught a capture namespace error; the next caught this missing encoding module. Those runs did not verify rendering or Android packaging.

## Owner phone checks

1. Install the new APK as a Development Build if building locally. Cold start and open/replay a box. Confirm no "CapsuleCollider doesn't exist" or BoxCollider errors; capture any other Console errors.
2. On toy play, drag the exposed plate rim. It should still turn the stage without grabbing Pip. Pick Pip from its visible face/body; the exposed rounded corner at x=.86/y=-.95 should miss, while the settled belly at x=.6/y=-.9 remains pickable.
3. Check all four finishes from the shelf/collection. Rotate slowly, squish/stretch, lift by different parts, release from low/high positions, then repeat with Motion calm. Confirm eyes/smile stay on the shell and the contact patch meets the plate without clipping or hovering.
4. Compare the four captures and the phone at rest. Watch for overly white highlights, hard shadow edges, noisy gold glitter and muddy Moon colour. Mint must remain opaque. Follow the v2 navigation checks in `V2_PHONE_TEST.md` as well.

## Performance evidence and limits

The current body has **3,126 exported vertices / 5,732 triangles** (2,868 welded vertices), within the existing 6,000-triangle mobile budget. FBX import agrees within .000000270 units. One watertight shell includes both crowns. Its broad contact radius is .632, shoulders are approximately 74% of maximum width, and the widest sampled band lies around 71% down the body. The shader remains one pass with the existing shared cubemap; the contact shadow reuses its existing draw. Larger pearls reuse the same six objects, and containment is checked in six directions.

Device cost is **not measured here**. Compare #10 and this branch on the same midrange Android phone, resolution, graphics API, quality and temperature. With Deep Profile off, record CPU/GPU frame times, batches, triangles and GC allocation for 30 seconds each at rest, rotation and continuous squish; repeat in Motion calm. Take screenshots with device/model, API and build type. Hosted software-renderer timings cannot establish Android frame rate.

## Validation

Locally passed: **543,482 portable assertions** across eleven suites, including manifold/winding/UV/deformation/volume, settled contact bounds, shoulder/crown/face placement and pearl containment; Blender FBX round-trip; metadata synchronization; source checks **66 C# files / three profiles / five assembly definitions / 142 GUIDs / zero failures**; whitespace.

Preceding settled-belly commit `0c32c67` passed 34/34 Unity tests with no skips in run `38070363818`; its captures supplied the PR #12 comparison feedback. Those results are historical and do not validate this new full concept pass. New Unity compilation, captures and Android packaging are pending at publication. Device appearance, startup, touch and profiling remain owner checks.
