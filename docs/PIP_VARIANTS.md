> UI update: variants are now separate toys on **Shelf**. Tap a shelf toy rather than using the former four-button selector. The material profiles below remain applicable.

# Compare Pip variants

All four presets use the same authored shape, face, deformation, camera, and controls. Switching replaces the filling and updates the existing materials while keeping Pip's current pose and plate angle. The selection is saved on that device; existing Peach/Moon saves retain their original meaning.

| Variant | Shell | Filling | Finish |
| --- | --- | --- | --- |
| Peach Jelly | Warm peach with a deeper orange base | Six small cream pearls and six apricot flecks | Bright wet studio reflections |
| Moon Jelly | Icy blue fading to lavender at the base | Three larger icy pearls and eight silver five-point stars | Softer reflections and a blue/lavender pearl sheen at the edges |
| Gold Glitter | Translucent champagne/gold with a deeper amber base | Dense shaded microflakes; no large pearls/stars | Wet gloss with flecks that catch light as the viewing direction changes |
| Mint Soft | Opaque pastel mint | Hidden filling; no interior objects | Subdued broad highlights, stronger diffuse shading, and no wet studio reflections |

## Try the comparison

1. Close Unity, run `git pull --ff-only` in your existing `pockle` directory, and reopen that project with Unity `6000.6.5f1`.
2. Choose **Pockle → Character → Use authored Pip**, then **Pockle → Open tactile prototype**, and press Play. The authored body log should show **3158 vertices**.
3. Tap **Peach**, **Moon**, **Glitter**, and **Soft** in the controls card. The label and highlighted button should follow the selection. Switching should not start a squeeze or turn the plate. Very narrow layouts arrange the four choices in two rows.
4. Rotate the plate and compare finishes from the same angle. Look for pearls/stars through Moon, small glitter highlights across Gold, and the opaque mint surface. Switch Soft → Peach → Moon → Glitter repeatedly: opacity, depth writing, studio reflections, pearl sheen, and glitter must restore correctly, without old filling objects remaining. Check that Soft hides the face from behind while keeping it visible from the front.
5. Squish/stretch each variant and release. Filling should follow the deforming body; independent drifting after release is a later experiment.
6. Stop and restart Play to confirm your selection returns. Test the selector at portrait-phone and landscape-tablet sizes.
7. Build and Run on Android to compare the actual phone rendering. Check the Console and device logs for shader errors, missing features, transparency sorting, or flashes of old filling during repeated switches.

The editor also has **Pockle → Character → Peach Jelly / Moon Jelly / Gold Glitter / Mint Soft** menus to choose the next Play session's starting variant. Use the in-app buttons to switch during Play.

## Validation and scope

Cloud validation for this finish update passed C# syntax and asset GUID consistency (26 source files / 58 GUIDs). The portable deformation/gesture code is unchanged from its passing checks. Two Unity PlayMode tests now cycle all four presets, including restoration of shader/depth settings, preserved deformation/shared resources, filling cleanup, resource destruction, and startup from each saved choice or an invalid preference. These tests need the local Unity editor and have not run in cloud.

Gold's dense glitter is a shader approximation attached to the body's rest UVs, not hundreds of separate particles. It follows deformation and changes highlights with view direction. Check shimmer, aliasing, seams, and shader cost on Android. It does not add independent glitter drifting inside the gel. Soft's opacity and depth writing must prevent back-facing eyes from showing through; the face and blush need inspection during maximum pinches.

Moon Jelly is a material comparison, not a glow-in-the-dark edition. Both finishes use the existing static studio reflections and approximate translucency; they do not refract the environment. Both variants now share lifting, two-finger deformation, phone-motion jiggle, and the branded Jelly Garden reveal; see [the interaction checklist](MANIPULATION_TEST.md) and [box-opening checklist](BOX_REVEAL.md). Independent filling drift remains planned work.
