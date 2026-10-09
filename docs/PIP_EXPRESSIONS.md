# Pip expression pass 01

Pip now has the four faces from the [character sheet](concepts/PIP_CHARACTER_SHEET.md). Reactions run locally on the selected toy, including Android. They do not add controls to the game screen.

| Face | Automatic trigger | Look |
| --- | --- | --- |
| Happy | Rest, or waking with a light touch | Existing plum oval eyes and smile; an occasional blink |
| Curious | Held above the plate, or stretched with two fingers | Open eyes and a small round mouth |
| Delighted | Squish, two-finger compression, shake, or landing | Happy closed eye arches and an open pink-tongued smile |
| Sleepy | Twelve seconds without touching/shaking the toy | Closed low eyelids and a quieter smile |

Lifting/stretching takes priority over compression while held. A gesture face lingers for 0.75 seconds after the gesture ends, then returns to Happy. Touch wakes Sleepy. The expression clock pauses while browsing and during the box reveal. Selecting another toy or resetting clears the clock and manual previews. Motion calm keeps direct facial reactions and removes automatic blinking and animated face transitions. Shelf thumbnails keep the Happy rest face and never run a face clock.

## Test in Unity and on Android

1. Enter Play mode and open an owned Pip from **Collection**.
2. Open **Pockle → Expressions**. Click Happy, Curious, Sleepy, and Delighted to compare the shapes. These are session-only previews, with no saved preference or inventory changes. Click **Automatic reactions** to return to gesture reactions.
3. Press Pip to squish. Lift him, pull two fingers apart, then compress them together. Drop him back to the plate. On the phone, shake gently; in the editor, press **J**. Look for curiosity during lift/stretch and delight during squish, shake, and landing.
4. Leave Pip alone for twelve seconds. Touch him to wake him. Test **Motion calm** as well.
5. Compare Peach, Moon, Gold Glitter, and opaque Mint Soft. Turn the plate through the side and rear; check that mouth, eyelids, and glints stay attached through deformation and that marks remain visible on Mint.

This is a first art/feel pass. Unity rendering, imports, phone readability, and gesture timing still need owner review. Cloud checks validate reaction timing and mouth curves; they cannot establish what these faces look like in Unity.

## Runtime interface for Claude

`Pockle.Core.FaceExpression`: `Happy`, `Curious`, `Sleepy`, `Delighted`.

`FacePose.For(expression)` yields a static pose. `JellyToy.SetFacePose(pose)` updates the face independently of the body deformation cache. The tactile viewer owns `FaceExpressionController` and steps it after the current body pose. `TactilePrototype.CurrentExpression` exposes its target face. Keep a separate clock for any future live lineup face; do not add face ticking to shelf portraits. The editor-only `PreviewExpression(FaceExpression?)` overrides the viewer, with null restoring automatic reactions.

Eyes, glints, mouth outlines, closed eyelid lines, smile fill, and tongue reuse resources across frames and Pip finish switches. Alternate mouth/eyelid rest anchors are projected onto the actual front mesh once during construction, then follow the same deformation as the shell. Authored meshes/anchors remain unchanged. Rebuilding a character releases its old owned expression mesh/materials.
