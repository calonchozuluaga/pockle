# Collections and series

Owner decisions, October 9, 2026, and the UI and catalog plan that follows from them. The screens are in the owner's design canvas "Pockle screens v2" (Collection, Collections, Shelf, Toy). Sample names such as Cloud Bakery and Lantern Night are placeholders, not approved content.

## Product rules (decided)

1. **A new series every month.** Each series is one themed collection with its own box art.
2. **Every series stays available.** Nothing retires. Older series remain on sale and appear lower in lists; the newest is always first. A brand-new player can buy into any earlier series.
3. **Prices are per box.** One box holds one Pockle. There is no whole-collection purchase.
4. **Vintage matters.** A toy keeps the series it came from and the date it was first found, and the UI shows both (for example "Jelly Garden series 6 (2026)" and "First found Oct 8, 2026"). Toys never leave the shelf.
5. **Secret figures.** Each series can include one secret figure at about **1 in 72** boxes. Its silhouette is shown on the collection page; its identity is not.

6. **Walking boxes feature the newest series.** Longer walks raise the chance of a rare figure (decided October 9).
7. **Duplicates have two uses:** trade them with other players, or combine spares to craft a missing toy (decided October 9).

Odds must be shown before purchase and before opening (store requirement), including the secret's rate and how a longer walk changes it.

## Proposed specifics (need the owner's sign-off)

**Walk tiers.** The daily box always comes from the newest series. Walking further upgrades the box rather than adding more boxes, which keeps the existing one-box-per-day rule:

| Steps today | Box | Secret chance | Rare finish chance |
| --- | --- | --- | --- |
| 1,000 | Daily box | 1 in 72 (base) | base |
| 5,000 | Daily box, glowing | about 1 in 48 | about 1.5× base |
| 10,000 | Daily box, golden | about 1 in 36 | about 2× base |

The box can be opened at any tier; opening it ends the day's upgrades. Step targets and multipliers are placeholders to tune. Higher tiers depend on dependable background step counting (UX-010) so they reward real walks; today steps only count while Pockle is open.

**Crafting.** Combine spares from the **same series** to make a toy you're missing from that series:

- 5 duplicates craft 1 regular toy of your choice.
- The secret can't be crafted (it stays a find), or costs much more (for example 15). Pick one.
- A crafted toy shows "Crafted" on its record instead of "Found", so the vintage history stays honest.
- Crafting is local and doesn't need accounts, so it can ship before trading.

**Trading.** One-for-one swaps of duplicates between players, both confirming. Trading needs accounts, a server that owns inventory, and protection against duplication and scams (UX-013, UX-016–019). It also needs care because paid random boxes plus trading can look like a secondary market to app stores and regulators: no selling for money, no trading toys you have only one of, and a clear age rating. It ships after accounts and a backend exist.

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
| Walk-tier odds (per tier) | Displayed and applied odds for 1k/5k/10k-step boxes |
| Craft cost (spares per toy, secret rule) | Crafting screen and validation |

Per owned collectible, in the saved inventory: **first-found date (UTC)** and **source** (walking box, purchased box, crafted, or traded), recorded once on first grant and never overwritten. This extends the version-2 save; older saves should show "found before dates were recorded" rather than inventing a date.

Per character: a short **idle** and an **eager** animation (hop, wave, peek) for the collection lineup. The lineup animates only the handful of toys on screen; the shelf keeps static thumbnails from the bounded preview cache.

## Not decided yet

- Walk tier targets and odds, crafting cost, and whether secrets can be crafted (proposals above).
- Backend and account provider, which trading depends on.
