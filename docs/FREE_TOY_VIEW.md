# Free toy viewer — UX-025 / UX-039

The toy now turns around its body centre, with unlimited yaw and pitch. The stage has no plate, rim marks, plate collider or plate rings. A soft contact shadow follows the toy's framing offset. No ownership, save format, rewards, engine, packages or owner settings change.

## Gestures

- One finger or left mouse drag anywhere in the free stage: rotate. Release eases a bounded fling to rest.
- Hold still on a soft toy for 350 ms: squish; release recovers according to its finish. Firm finishes stay rigid. Moving before the hold threshold selects rotation.
- Two fingers on soft toys: pinch inward to squish, pull apart to stretch along the finger axis; move both fingers down to press or up to lift. On rigid vinyl/metal finishes, pinch to zoom (0.65–1.8 of fitted size) and move their midpoint to reposition. Fingers beginning over UI cannot join. Ending either finger cancels the pair until both lift, avoiding a jump into rotation or squash.
- Double-tap within 300 ms: reset rotation, zoom, pan and backdrop position. Editor R also resets; mouse wheel zooms.
- Focus loss, pause, opening menus, viewport changes and screen rotation cancel active input. Calm stops inertia immediately and freezes backdrop movement; direct rotation, zoom, pan and squish remain available.

`ToyOrbit` is engine-free and owns bounded frame state, fling decay and panorama clamping. `ToyViewGesture` owns touch/mouse capture and gesture arbitration. Two-finger soft deformation and weighted lift use the existing tactile springs, sag and landing response; rigid finishes use viewer framing.

`PrototypeStage.SetViewport(Camera, Rect)` consumes Claude's `StageViewportChanged` screen-pixel rectangle without changing HUD files. Framing eases over roughly 0.2 seconds and fits the current viewport; `SetZoom` scales that fit. The existing five-world environment-ID hookup is preserved.

Wide paintings pan horizontally at 25% of the hidden width per 180 degrees of yaw, with eased small pitch offsets and 4% overscan. Both axes clamp inside the image; these are painted LDR worlds, not seamless panoramas or new HDR captures. Reset explicitly restores the home crop, including under Calm.

The additive halo's position and rotation are recomputed in camera space behind the actual visual origin each frame, including lifts and reaction offsets. Its offset no longer rotates around the toy. Ground spill follows the framing offset beside the contact shadow, with no plate dependency. Light remains an inexpensive local visual effect.

## Verification

Portable Core and source checks passed. First focused Unity run: 6/6 PlayMode tests passed. Full combined owner snapshot: 57/61 passed; the same four Claude shelf/personality UI failures occurred in the pre-change run (55/59). No new failures in that comparison. Final focused run: 7/7 passed, including backdrop compatibility; EditMode 17/17 passed. Task-file whitespace checks passed. Whole-checkout diff-check still flags pre-existing owner settings/Claude visual notes, which were preserved.

Actual Unity captures of Fox, Starlight Glow Pip and Biolume Axolotl at front, side with pitch and rear were rendered; front Fox and tilted Pip were visually inspected. Phone touch handling, device framing/performance and APK packaging remain unverified. This update is local; no main merge or hosted-CI success is claimed.


October15 owner feedback: soft toys now fill 25% more horizontal space in width-limited portrait framing (half-width budget1.2 vs1.5). Minimum vertical half-height remains1.6 for reaction/rotation clearance. Firm framing and pinch zoom are unchanged; soft pinch remains squish/stretch, with double-tap reset preserving the larger default. Layout/viewport reservation is still supplied by the HUD.
