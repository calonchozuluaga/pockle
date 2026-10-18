# Ginger tap reactions

Ginger has five two-second reactions across all ten finishes: CookieWave (one iced paw waves), EarWiggle, ButtonDance (a cheerful side-to-side dance), HappyHop, and SweetBow (a head nod with sleepy eyes and a smile). Local paw, ear and head masks carry the existing icing with the mesh; the source mesh and finish recipes are unchanged. The bow keeps the feet planted and rigid finishes keep their whole-body scale.

A single tap chooses uniformly among all five actions initially, then among the four other actions. Taps during an active reaction do not interrupt it or queue another. Gesture start, reset, collectible changes, disable and focus loss cancel the pose. Existing free rotation, material-aware pinch, lift and double-tap reset remain intact. Calm uses a small smile without motion or blinking.

Core/GingerReactions contains engine-free action timing/selection. GingerReactionDriver coordinates the visual offset, HolidayExpressionDriver face, and Ginger-only HolidayFinish shader controls. Other holiday characters retain their existing animation paths. No action tray, sounds, new inventory grants or save format changes are included; TryPlay remains a future sound/event hook.

Native Unity preview: docs/captures/ginger-reactions/Ginger-five-reactions.gif. Executed checks and device status are recorded in VALIDATION.md.
