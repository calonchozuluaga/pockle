# Collections and series

Owner decisions, October 9, 2026, and the UI and catalog plan that follows from them. The screens are in the owner's design canvas "Pockle screens v2" (Collection, Collections, Shelf, Toy). Sample names such as Cloud Bakery and Lantern Night are placeholders, not approved content.

## Product rules (decided)

1. **A new series every month.** Each series is one themed collection with its own box art.
2. **Every series stays available.** Nothing retires. Older series remain on sale and appear lower in lists; the newest is always first. A brand-new player can buy into any earlier series.
3. **Prices are per box.** One box holds one Pockle. There is no whole-collection purchase.
4. **Vintage matters.** A toy keeps the series it came from and the date it was first found, and the UI shows both (for example "Jelly Garden series 6 (2026)" and "First found Oct 8, 2026"). Toys never leave the shelf.
5. **Secret figures.** Each series can include one secret figure at about **1 in 72** boxes. Its silhouette is shown on the collection page; its identity is not.

The walking reward still grants one daily box; which series the daily box draws from is a reward-logic decision for Codex and the owner. Odds must be shown before purchase (store requirement), including the secret's rate.

## Screens (Claude, UI lane)

- **Collection ("Who's inside")**: the full lineup. Found toys idle with a gentle squish; unfound toys are silhouettes that hop and wave with "!" and "?" bubbles; the box bobs and wiggles; the secret appears as a twinkling silhouette with its odds. Actions: "Open a box" (price) and "Walk for one". Motion stops under Motion calm / reduced motion.
- **Collections**: newest series as a large card, then "Earlier series" in release order, each with its series number, month, and your progress.
- **Shelf**: grouped by series (newest first) with per-series progress, collection filter chips with counts, and an Everything / Missing / Duplicates view. Missing shows the gaps in each set as silhouettes.
- **Toy**: series and first-found date shown with the toy.

## Catalog fields needed (request for Codex, UX-029 follow-up)

Per collection/series, in `ToyCatalog` (read-only, engine-free):

| Field | Purpose |
| --- | --- |
| `SeriesNumber` (int) | Ordering and the "Series 6" label |
| `ReleaseDateUtc` (date) | Newest-first ordering, "October 2026" label, "New" tag |
| `Members` (collectible IDs, ordered) | The lineup on the collection page, including not-yet-found toys |
| `SecretId` (optional collectible ID) | The secret slot; shown as a silhouette until found |
| Box pool weights including the secret | Displayed odds; the existing explicit-pool rules still apply |
| `BoxPriceTier` (or product ID) | Price shown per box |
| Accent colours (field, ink) | Collection page and shelf section colours |

Per owned collectible, in the saved inventory: **first-found date (UTC)** and **source** (walking box or purchased box), recorded once on first grant and never overwritten. This extends the version-2 save; older saves should show "found before dates were recorded" rather than inventing a date.

Per character: a short **idle** and an **eager** animation (hop, wave, peek) for the collection lineup. The lineup animates only the handful of toys on screen; the shelf keeps static thumbnails from the bounded preview cache.

## Not decided yet

- Which series the free daily walking box draws from (newest only, or any).
- Whether duplicates get a use (for example trading or crafting a missing toy). Duplicates are counted and shown, but have no action yet.
