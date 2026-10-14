# Collection art standard

Owner-approved October 14, 2026. Each collection has one coherent character identity and the following complete art set. Fox is the first set to build.

| Component | Required deliverable |
| --- | --- |
| Pockles | Exactly ten playable varieties: nine common, one rare. The rare occupies one of the ten slots. Preserve approved character shape and stable collectible IDs. |
| Worlds | Five distinct environment paintings unique to that collection, matching its habitat and complementing its finish colors. Blank of toys/UI so the selected 3D toy remains the focal subject. Include a dark environment for reflective and glowing finishes. Bake reflection maps; retain original paintings. |
| Cover | One separate illustration at exactly 3:1. No text, logos or UI overlays. Some approved collection toys may interact with the environment. UI adds headings separately. |
| Box | One actual 3D carton with opening hinges/parts and material/UV assets, plus transparent PNG renders from that same model for collection art and walk tease. Reuse its identity for opening animation. |

The cover is independent of the five world backgrounds. A collection has ten toys total, not ten plus an additional secret. Rarity metadata does not by itself approve drop odds or enable a box purchase.

Claude owns the collection/toy screens and consumes catalog resource paths. Codex owns runtime assets, world/reflection loading and opening behavior. Box art may use branding; the no-text/no-overlay constraint specifically applies to the cover. Keep source files, stable .meta GUIDs, and exact generation prompts alongside deliverables.

## Required expression and animation support

Every toy needs eye and mouth expression controls, including blink and expression changes, and a subtle character-specific idle: Pip hops and sways; Fox hops and moves its tail; Axolotl waves or shakes. Touch reactions and later toy sounds must be able to trigger a matching facial expression. Include an expression parameter/animation-event interface so later audio can drive emotion or mouth activity without rebuilding the character. Keep details attached to the toy's mesh, retain rig or morph-target source, and provide neutral poses and calm-motion alternatives. No per-touch network/AI. New illustrations are concepts, not evidence that the corresponding 3D rig/animations already exist. Sound assets themselves remain future work.
