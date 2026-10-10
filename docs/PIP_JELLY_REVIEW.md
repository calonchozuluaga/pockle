# Android startup and rounded jelly Pip review

October 10, 2026. Test branch: `codex/pip-jelly-device-pass`, stacked on the v2 collection integration. Use Unity **6000.6.5f1**, Built-in RP. Keep current local Android settings when switching branches.

## Changes to inspect

- Stage disks and the carton floor now use owned meshes instead of `CreatePrimitive`. No implicit CapsuleCollider or BoxCollider is constructed. The plate still has one MeshCollider, shared stage geometry is released on teardown, and its top remains at y=.15.
- Pip curves underneath to a small contact patch at y=-1. Contact radius is about **.129**, previously **.706**. The joined crown stays part of the body. Face anchors are reprojected on the regenerated surface; the Blender source, FBX and runtime mesh agree.
- Peach, Moon and Gold have broader glaze highlights, a bounded approximation of transmitted studio light, and brighter saturated edges. Mint stays opaque, with a soft broad sheen and much weaker transmitted light. No refraction, bloom or extra shell is introduced.
- The existing contact shadow draw now fades radially using the retained blush shader; it follows the existing lift/landing behavior. This pass does not add new idle animation or rock physics.

## Real Unity captures

Run PlayMode test `PipJellyCaptureTests.FourFinishesRenderOnTheirColourFieldsAndKeepGripPicking`. With a graphics device, it writes 720×960 PNGs to `test-results/pip-jelly/` at the project root: PeachJelly, MoonJelly, GoldGlitter, MintSoft, and PeachJelly-grip. CI includes that directory in its existing `unity-test-results` artifact. A Null graphics device explicitly skips this test; a skipped capture is not render evidence.

Compare Peach/Moon against `docs/concepts/pip-character-sheet-01.png`, and Peach against `docs/concepts/pip-peach-jelly-concept-01.png`. The concept uses offline lighting; the PNGs use the actual Unity mesh/material/camera. Review the underside, face contact, sheen, colour saturation, interior filling and silhouette. These are render evidence, not a phone performance measurement.

## Owner phone checks

1. Install the new APK as a Development Build if building locally. Cold start and open/replay a box. Confirm no "CapsuleCollider doesn't exist" or BoxCollider errors; capture any other Console errors.
2. On toy play, drag the exposed plate rim. It should still turn the stage without grabbing Pip. Pick Pip from its visible face/body; tapping outside the narrow underside should not grab the old foot.
3. Check all four finishes from the shelf/collection. Rotate slowly, squish/stretch, lift by different parts, release from low/high positions, then repeat with Motion calm. Confirm eyes/smile stay on the shell and the contact patch meets the plate without clipping or hovering.
4. Compare the four captures and the phone at rest. Watch for overly white highlights, hard shadow edges, noisy gold glitter and muddy Moon colour. Mint must remain opaque. Follow the v2 navigation checks in `V2_PHONE_TEST.md` as well.

## Performance evidence and limits

The new body has **3,081 exported vertices / 5,732 triangles**, down from **3,158 / 5,814** (2,868 welded vertices). FBX import agrees within 0.000000227 units. The body remains one shader pass with one existing studio cubemap sample; the changes add bounded arithmetic rather than textures, camera captures or additional live draws. The contact shadow reuses its existing draw. Suspended filling and deformation buffers are unchanged.

Device cost is **not measured here**. Compare #10 and this branch on the same midrange Android phone, resolution, graphics API, quality and temperature. With Deep Profile off, record CPU/GPU frame times, batches, triangles and GC allocation for 30 seconds each at rest, rotation and continuous squish; repeat in Motion calm. Take screenshots with device/model, API and build type. Hosted software-renderer timings cannot establish Android frame rate.

## Validation

Locally passed: **538,252 portable assertions** across eleven suites, including manifold/winding/UV/deformation/volume, rounded contact bounds and front-face anchor projection; Blender FBX round-trip; metadata synchronization; source checks **66 C# files / three profiles / five assembly definitions / 142 GUIDs / zero failures**; whitespace.

The collider-only commit also passed hosted portable checks and Unity compilation/tests. New startup coverage checks plate raycasts, outward mesh winding, no implicit colliders, idempotent carton initialization and owned mesh cleanup. The combined visual commit's Unity shader/render captures and Android packaging are pending at publication. Android startup, phone appearance, touch and profiling require the checks above; they are not inferred from the portable or editor results.
