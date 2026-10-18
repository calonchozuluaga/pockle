# Pip signature reactions — first preview

Approved Peach Pip now has five local reactions: Hop, Curious Tilt, Happy Wiggle, Peekaboo and Thank-you Bow. Each lasts 1.8 seconds and returns exactly to rest. Movement and painted eyes/mouth use the existing approved mesh/paint and shader; no rig, new sound or inventory change.

Tap the toy for a random reaction. The first tap chooses among all five; subsequent taps choose equally among the other four, excluding the last reaction even after cancellation. A 310 ms delay preserves double-tap reset. Drag/hold/multitouch, menu/viewport changes, focus loss and pause cancel pending taps or reactions. Rotation remains available; pinch deforms soft finishes and zooms rigid finishes, as described in FREE_TOY_VIEW.md. Rigid finishes never squash during these actions. Calm substitutes a subtle happy expression for body movement and eye closure.

`PipReactionDriver.TryPlay(PipAction)` is the explicit action entry point for a future optional Claude Actions tray. `PipReactions.Sample` is engine-free; a separate reaction transform preserves the mesh deformation, viewer framing and visible-shell picking. Driver reattaches rebuilt visual roots when changing characters/finishes. No other character is assigned Pip actions.

Owner decision: no Actions tray or extra buttons. Random tap reactions with no consecutive repeat are now implemented. The displayed preview still shows all five in a fixed order for easy comparison.

Unity 6000.6.5f1 focused PlayMode 4/4 passed: reaction lifecycle/rigid/facial/rebuild handling, existing orbit/hold/reset/framing/parallax tests and Peach appearance. Portable Core passed, including bounded samples, invalid timing, exact rest endpoints, no rigid squash and motionless Calm. Preview frames rendered in Unity and inspected at hop/blink/bow peaks; native ImageIO encodes the five GIFs and combined preview. Device touch, frame time and APK installation remain unverified.

Preview: docs/captures/pip-reactions/Pip-five-reactions.gif. Editor capture: Pockle.Editor.PipReactionCapture.Capture.
