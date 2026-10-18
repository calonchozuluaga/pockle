# Axolotl signature reactions — UX-025

All ten Axolotl finishes now share five random tap reactions: Hello Wave, Gill Flutter, Wiggle Dance, Happy Bounce and Sleepy Sway. Each has coordinated eyes/mouth; arm, gill and rear tail movements are local shader controls on the approved mesh/paint. Hello Wave rotates the hand region about the shoulder. No new model, rig, texture or sound was generated.

AxolotlReactions supplies bounded two-second poses; AxolotlReactionDriver applies a separate body offset and overrides existing face/idle controls during a reaction. A single tap chooses uniformly among five initially and among the four alternatives thereafter, with no immediate repeat. Pip and Fox retain separate reactions/histories. Drag/hold/pair gestures and double-tap reset remain unchanged; soft finishes deform and rigid finishes zoom. Glow follows the actual visual offset after animation.

The new shader controls _ArmWave/_GillFlutter/_TailSwish are zero at rest. Calm substitutes a small happy expression without body/arm/gill/tail movement or blink. Selection changes, menus and focus loss cancel; pause freezes. Source assets, stable IDs, finish recipes and inventory are preserved.

Portable Core and source checks passed; initial focused Unity PlayMode10/10 passed (all ten finish/action controls, no-repeat lifecycle/reset, current-character switching, retired assets rejected, Fox/Pip and material gestures). Final shader renders and full-suite results are recorded in VALIDATION.md and handoff. Native preview frames were inspected for wave, blink/sway and bounce; GIFs are native encodings of Unity renders in docs/captures/axolotl-reactions.

Separate roster cleanup is documented in ACTIVE_ROSTER.md. No Claude HUD or UI test files were edited. Device smoothness/gesture acceptance and installation are separate from editor tests.
