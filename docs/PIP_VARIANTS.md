# Compare Pip variants

Both variants use the same authored shape, face, deformation, camera, and controls. Switching replaces the filling and updates the existing materials while keeping Pip's current pose and plate angle. The selection is saved on that device.

| Variant | Shell | Filling | Finish |
| --- | --- | --- | --- |
| Peach Jelly | Warm peach with a deeper orange base | Six small cream pearls and six apricot flecks | Bright wet studio reflections |
| Moon Jelly | Icy blue fading to lavender at the base | Three larger icy pearls and eight silver five-point stars | Softer reflections and a blue/lavender pearl sheen at the edges |

## Try the comparison

1. Close Unity, run `git pull --ff-only` in your existing `pockle` directory, and reopen that project with Unity `6000.6.5f1`.
2. Choose **Pockle → Character → Use authored Pip**, then **Pockle → Open tactile prototype**, and press Play. The authored body log should show **3158 vertices**.
3. Tap **Peach** and **Moon** in the controls card. The label and highlighted button should follow the selection. Neither switch should start a squeeze or turn the plate.
4. Rotate the plate, switch again, and compare both finishes from the same angle. Look for stars through the shell and check that old pearls/flecks disappear when switching to Moon.
5. Squish/stretch each variant and release. Filling should follow the deforming body; independent drifting after release is a later experiment.
6. Stop and restart Play to confirm your selection returns. Test the selector at portrait-phone and landscape-tablet sizes.
7. Build and Run on Android to compare the actual phone rendering. Check the Console and device logs for shader errors, missing features, transparency sorting, or flashes of old filling during repeated switches.

The editor also has **Pockle → Character → Peach Jelly / Moon Jelly** menus to choose the next Play session's starting variant. Use the in-app buttons to switch during Play.

## Validation and scope

Cloud checks passed the existing portable deformation and gesture suites, C# syntax, and asset GUID consistency. Two Unity PlayMode tests cover repeated switching with preserved deformation and shared resources, filling cleanup, resource destruction, and startup from the saved choice. These tests need the local Unity editor and have not run in cloud.

Moon Jelly is a material comparison, not a glow-in-the-dark edition. Both finishes use the existing static studio reflections and approximate translucency; they do not refract the environment. Both variants now share lifting, two-finger deformation, and phone-motion jiggle; see [the interaction checklist](MANIPULATION_TEST.md). The redesigned box opening and independent filling drift remain planned work.
