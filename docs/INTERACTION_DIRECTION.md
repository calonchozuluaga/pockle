# Pockle viewer direction

The current test build uses direct touch on the toy for squeezing/stretching and an exposed plate rim for rotation. The first surface and finger keep control until release. Turning stops on release. There are no Touch/Rotate mode buttons. The glossy authored shell uses a static studio environment; Unity appearance and Android behavior still need owner testing.

The owner's next viewer ideas are recorded here without claiming they are implemented:

- **Two-finger stretch:** both fingers start on the visible toy. Pulling apart stretches along their separation axis; bringing them together compresses. It changes the jelly rather than zooming the camera. A plate gesture never upgrades to a toy pinch. When one finger lifts, rebase the remaining gesture to avoid a jump. Constrain deformation and preserve reduced-motion behavior.
- **Reactive filling:** gold stars and pearls mostly follow a squeeze/stretch, lag behind the gel, then drift briefly and settle after release. Keep pieces inside the changing shell. Begin with a few readable inclusions instead of a full fluid simulation.
- **Lights-off glow:** selectable lighting environments, beginning with a simple lights-off control for glowing editions. Combine a luminous material with a subtle halo and grounded colored glow; emission alone does not illuminate the surrounding scene. Material and reflection settings must respond to the chosen environment.
- **Wet outer shell:** target the approved jelly concept's broad white reflections, clear warm rim, layered filling, rounded lower edge, and soft contact shadow. The current static reflection pass is an approximation. Test on Android before introducing expensive scene refraction.

The collecting direction is daily collection-themed boxes unlocked by walking, plus optional paid boxes at proposed standard/special price points. See [the product brief](PRODUCT_BRIEF.md). The current viewer and [packaging concepts](packaging/README.md) do not implement walking validation, payments, ownership, or inventory.
