# Pip fusion material studies

Owner-approved material test pass, October 15. These are fixed native Unity material prototypes, not a working fusion crafting system. They consume no parents and have no recipes, trading or purchase offers. Personality inheritance remains a design discussion; both currently retain Pip's existing random reactions, expressions and material-aware gestures.

Find them in **Collections → Pip Fusion Lab**. IDs are `pip.fusion-dusk` and `pip.fusion-sorbet`. This separate, two-member owner lab uses the five Pip worlds for material comparison. The regular Pip collection remains ten members, its one ultra-rare and its existing completion target. The normal eight collections and their box odds are unchanged. The catalog temporarily exposes nine collections and 82 entries (72 playable) including the lab; this is not a new production character/ten-finish collection.

Dusk: peach-pink base through lilac/periwinkle crown, translucent pearl jelly, peach pearls and cool-blue stars. Gel handling. Sorbet: wrapping three-dimensional peach/mint marbling, opaque silky surface, higher roughness and restrained reflections, foam handling. Both share the approved native Pip mesh and face texture. The shader resets all fusion properties when switching back to a regular finish. Marbling uses rest-space mesh coordinates, so it follows deformation and wraps around the back.

Owner test grants are gated by `Resources/PipFusionOwnerTestGrant.txt` = `enabled`, using marker `pockle.owner-test.pip-fusion-studies.v1`. Startup saves one missing copy of each before writing the marker and refreshing the HUD, preserving existing duplicates/rewards. Saves remain v2. Batch-mode test runs do not apply automatic grants. Disable this and the other owner grant resources before public distribution. Regular Pip's owner grant does not include the lab entries.

Implementation: engine-free PipFusionStudies/catalog membership and exact grant logic; CollectionSession persistence; PipFinishMaterial recipes; TexturedPeachJelly color fields; JellyToy Dusk inclusions; PipFusionMaterialCapture native renders. No HUD or UI design files are edited. Claude may author bespoke Dusk/Sorbet personality content later; generic Pip profiles are retained for this material test.

Native front/65-degree/back renders are in docs/captures/pip-fusion-materials. See VALIDATION.md for executed checks and installation status.


## Honeydew and Eclipse expansion

Owner approved both for phone testing after Dusk/Sorbet and the larger soft framing. Pip Fusion Lab now contains four fixed studies: `pip.fusion-honeydew` and `pip.fusion-eclipse` join the originals. The UI catalog has nine collections/84 entries/74 playable; the regular eight collections remain80 entries/70 playable with their original completion targets and reward odds. Eclipse is a secret-recipe concept visually, not a catalog secret or an approved crafting recipe.

Honeydew: pale jade satin gel with honey-gold crown/base accents and fine gold sparkle, restrained reflections, gel handling. Eclipse: midnight blue/lavender translucent pearl jelly, sparse native gold stars, simulated gold ribbons sampled beneath the shell with object-space view parallax, and a warm gold night halo. The ribbon is a local shader approximation, not a separate fluid simulation or dynamic light. The existing bounded aura provides glow spill. Both retain Pip's existing expressions, random actions, squish gestures and larger soft framing.

The resource gate is unchanged; grant marker is now `pockle.owner-test.pip-fusion-studies.v2`. Startup ensures at least one of all four studies before persisting/marking/refreshing, so previous v1 owners get only missing Honeydew/Eclipse and keep Dusk/Sorbet duplicates. The old marker remains intact and saves remain v2. This is an owner test build; disable owner grant resources for public distribution. No parents are consumed and the fusion service remains preview-only.

Native captures include front/65-degree/back studio views and dark-background shader comparisons; the dark shader capture is separate from the tested in-game halo. No bespoke personality inheritance is implemented in this material pass.
