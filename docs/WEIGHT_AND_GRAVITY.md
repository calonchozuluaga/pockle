# Weighted lift and gravity

Pip now trails an upward drag with a damped hand constraint. While held above the plate, the gel droops around the picked grip, strongest below it, with a small widening of the hanging belly. The shell, face, smile, and filling share the same deformation. Grip changes when a second finger joins or either finger leaves are eased, and the hand constraint rebases at the current height.

Releasing a lifted toy switches from spring return to a short gravity-driven fall. A small amount of upward hand momentum is retained, within the lift limit. Contact strength comes from the solved impact velocity, so higher drops produce a stronger bounded squash, vertical jiggle, and small lean before the existing deformation springs settle. The root stops on the plate rather than repeatedly bouncing through it. Enabled sound/haptics respond to the landing; a press released on the plate keeps its normal immediate feedback.

`WeightedLift` uses an analytic falling trajectory and solves time to contact, avoiding frame-rate-dependent impact strength. `SuspendedShape` bounds droop and clips its lowest point to the actual plate clearance. Extreme pinch/camera limits also constrain the lift state, so hidden height cannot accumulate drop energy. No Rigidbody, mesh collider recooking, fluid simulation, or new runtime dependency is introduced.

Reduced motion retains direct lift/pinch control and returns to the plate without hanging sag, gravity animation, or impact pulses. Browsing, dialogs, focus loss, and gesture cancellation cancel the suspension safely. The authored box opening keeps its existing separate full/calm timing.

## Try it locally

1. Close Unity, pull with `git pull --ff-only`, reopen, and rebuild the Android APK.
2. Select Peach on Shelf. With Settings → Motion full, drag up and hold. Compare a grip near the upper body with one near the middle: gel below the grip should droop, while the picked area stays supported.
3. Release from a small height and then near the maximum height. The larger drop should fall, squash more, jiggle, and settle. The base must remain above/on the plate; it must not continually bounce or sink below it.
4. Regrab during the fall. Add a second finger, stretch, and remove either finger. Height should remain steady during each handoff, and a later release should still settle.
5. Stretch strongly while lifted and rotate the plate before lifting again. Check the crown stays in view, the face/filling follow the shell, and the base does not penetrate the plate.
6. Repeat for Moon, Gold Glitter, and Mint Soft. They currently share the same weight tuning; per-material mass/stiffness is a later tuning option.
7. Repeat in Motion calm, with sound/haptics disabled, and after opening Settings or leaving the app during a lift. No delayed landing feedback or stuck suspended pose should survive cancellation.

Portable checks passed, including 152,104 weight/geometry assertions across 15–240 fps, impact timing, regrab, calm mode, invalid inputs, and combined sag/pinch/squish on the authored mesh. C# syntax/assets were checked for Editor and Android branches. The Unity PlayMode variant pose check now includes sag, but Unity tests, rendering, touch feel, and APK performance remain unrun in cloud and need local validation.
