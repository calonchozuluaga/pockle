# Roster finish refresh — November 8, 2026

Owner request: apply the 15-toy rarity principle to every collection, make the finishes visibly distinct, and disclose each collection slot's rarity even before discovery. Owner correction: Secrets need not all be molten. Owner authorizes direct collection HUD/test changes for this task.

## Local design

Each of the eleven active collections has nine Common, three Rare, two Epic and one Secret. Wally's approved fifteen and shader recipes stay unchanged. The other ten retain all existing IDs, including their glow IDs. Glow becomes Epic, coated metal becomes Rare, and five new IDs are additive: signature-matte (Common), gilded-leaf and rose-gold (Rare), diamond-prism (Epic), secret-edition (Secret). Common/rare/epic/secret membership is explicit catalog metadata; appearance never depends on personality.

The Commons use separated collection-specific pigments and material profiles. Classic uploaded paint is preserved where appropriate. Firb's green mask now covers dark branches as well as the middle tier, while preserving cream face paint and red/gold ornaments. The two additional golds use leaf and rose treatments; diamond has broad, independently reflective facets.

| Collection | Secret | Treatment |
| --- | --- | --- |
| Pip | Cosmic Core | Star-filled galaxy glass |
| Fox | Sunfire Spirit | Warm luminous ribbons on a dark reflective coat |
| Axolotl | Biolume Bloom | Glowing lagoon spots |
| Firb | Aurora Evergreen | Flowing aurora ribbons |
| Coco | Midnight Ganache | Gold marbling over dark chocolate |
| Snowy | Polar Opal | Milky iridescent opal |
| Ginger | Stained Sugar | Jewel-colored sugar cells |
| Reiny | Solstice Spirit | Warm solstice ribbons |
| Guin | Polar Night | Deep blue star field |
| Bundy | Sugar Nebula | Plum nebular glass |
| Wally | Molten Abyss | Existing approved obsidian and cyan fissures |

The review gallery at `roster-finish-review/02/index.html` includes all 165 native captures and individual sheets. Its hidden-toy toggle is a review aid; the real HUD continues using model-derived silhouettes, now with all four rarity labels visible.

## Integration boundary

This is an isolated local design/runtime branch, not a store rollout. Live purchase catalog/odds and finite supply records still describe the previous lineup. They must be reconciled transactionally with the approved new designs before a paid or walking reward rollout. No new prices, products, draw weights, edition caps or backend grants are introduced by this branch. The checkout continues to disclose the server's eligible outcomes; it must never claim that the new visual lineup has already entered live paid stock.

Existing owned IDs and fusion references remain loadable; there is no save-format change, renaming or deletion of owned IDs. New IDs use existing handling profiles. No new network requests or AI calls occur during tactile play.
