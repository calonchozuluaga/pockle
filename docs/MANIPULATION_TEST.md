> Collection UI update: select a toy on **Shelf** to play. Comfort controls are in **Settings**. Open an earned daily box from **Boxes**; `Space` previews the opening in the editor. Former variant, Reset, and Reveal again buttons are gone. See [current UI tests](COLLECTION_UI.md).

# Lift, pinch, and shake test

The interaction update adds one-finger lifting, two-finger squish/stretch, and accelerometer jiggle to both Peach and Moon. It also includes the store preview published immediately before it.

Close Unity, run `git pull --ff-only` in your existing checkout, reopen with `6000.6.5f1`, and press Play. For phone tests, rebuild and install the Android APK; pulling source does not update the already installed app.

The editor Console should report **Pockle controls: lift · two-finger squish/stretch · phone-motion jiggle**. The guide in the controls card now says **Drag Pip to lift. Two fingers to squish/stretch.** These identify the new controls source rather than the previous one-finger stretch build.

## On the computer

- Press Pip, drag upward, and hold: the whole toy should leave the plate, while the plate stays put. The contact shadow grows and lightens. Release: Pip should return to the plate with a small settling squash, without sinking through it.
- Drag the exposed plate: it should still turn the toy and plate together. Start on the plate then drag onto Pip: it must remain a turn gesture.
- Press **J** with Game view focused to preview a jiggle pulse. This is an editor preview; it does not verify a phone sensor.
- Switch Peach / Moon while lifted or settling. The material/filling should change while the body position, deformation, and plate angle remain intact.
- Reset and replay the reveal, including during a lift. Pip should return to its starting position. Open Store while holding Pip: the gesture should cancel, and the store must block toy input.

## On Android

1. With **Motion · full**, try the one-finger lift and release. Check both portrait and landscape. The height is capped to help keep a stretched/lifted Pip inside the viewer.
2. Start a touch on Pip, then add a second touch on his visible body. Pull apart horizontally, vertically, and diagonally. The body should stretch along the separation direction, with the face and filling following it; the camera should remain fixed. Bring the fingers closer to compress along that direction.
3. Move both fingers upward together: Pip should lift while retaining the two-finger deformation. Remove the first finger, then repeat removing the second: either survivor must continue a one-finger drag without teleporting Pip or rotating the plate. A third finger must not take over. Repeat with fingers crossing or nearly touching; deformation must stay bounded.
4. Start on the plate, then place another finger on Pip. The plate gesture must remain a turn. Touch settings, the store, or background while holding Pip: these must not become the second toy touch.
5. Hold the phone still: gravity alone should not jiggle Pip. Gently shake or move the phone: Pip should wobble, then settle when the phone stops. Rotate the phone and background/resume the app: there must be no stuck gesture or replay of stale sensor motion. Sensors are optional; unsupported hardware should keep touch controls working.
6. Select **Motion · calm**: lifting and pinching should remain available directly, with no shake jiggle or spring bounce. Return to Full and check that jiggle resumes after a fresh sensor baseline.
7. Test Moon's larger pearls and stars through a complete turn and all gestures. Check face attachment, transparency, crown/foot shape, and maximum deformation on the actual phone. Profile frame time and allocations as in [validation](VALIDATION.md).

## What cloud verified

The existing portable suites passed, plus **114,325 manipulation assertions** against the actual production gesture state, gravity filter, and directional deformation on the authored mesh. These cover capture/handoff/cancellation, rejection of plate or third-finger takeover, steady gravity/noise rejection, bounded sensor spikes and spring response, settling, malformed-input recovery, opposite-axis equivalence, fixed foot, finite bounds, and approximate bulk volume through horizontal/vertical/diagonal pinches, including combined pressure and wobble.

Source checks passed **24 Unity C# files and 55 asset GUIDs**. Unity API compilation, input dispatch, local rendering, actual accelerometer delivery, Android builds, and performance remain unrun in cloud. The two existing PlayMode variant tests now include a pinched pose when checking that switching retains deformation; they still require the local editor.

The shell and suspended filling deform together. Independent filling lag/drift and lights-off glow remain separate future work. The redesigned mystery-box opening is now included; follow [the reveal checklist](BOX_REVEAL.md).
