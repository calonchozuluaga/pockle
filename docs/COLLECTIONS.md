# Collections and series

Owner decisions, October 9, 2026, and the UI and catalog plan that follows from them. The screens are in the owner's design canvas "Pockle screens v2" (Collection, Collections, Shelf, Toy). Sample names such as Cloud Bakery and Lantern Night are placeholders, not approved content.

## Product rules (decided)

1. **A new series every month.** Each series is one themed collection with its own box art.
2. **Every series stays available until it sells out.** Nothing is retired on a schedule. Older series remain on sale and appear lower in lists; the newest is always first. A brand-new player can buy into any earlier series while supply lasts (see rule 9).
3. **Prices are per box.** One box holds one Pockle. There is no whole-collection purchase.
4. **Vintage matters.** A toy keeps the series it came from and the date it was first found, and the UI shows both (for example "Jelly Garden series 6 (2026)" and "First found Oct 8, 2026"). Toys never leave the shelf.
5. **Secret figures.** Each series can include one secret figure at about **1 in 72** boxes. Its silhouette is shown on the collection page; its identity is not.

6. **Walking boxes feature the newest series.** Longer walks raise the chance of a rare figure (decided October 9).
7. **Duplicates have two uses:** trade them with other players, or combine spares to craft a missing toy (decided October 9).

Odds must be shown before purchase and before opening (store requirement), including the secret's rate and how a longer walk changes it.

8. **Paid boxes never give a duplicate.** A purchased box always contains a toy you don't own yet, so you can only buy your own copy. Once you own every regular toy in a series, that series' paid box closes for you. Spares come only from free walking boxes. This stops one player from buying up supply to resell (decided October 9).
9. **Finite editions.** Each toy has a fixed edition size. A series stays on sale until its toys sell out; a sold-out toy can only be traded for. Every copy carries a serial number (for example "#0421 of 5,000") and its history, recorded in a public ledger (decided October 9).
10. **Trading has no money value.** Only spares can be traded, so you always keep your only copy. Both friend-to-friend swaps and an open trading market (similar to Biotadex) launch together (decided October 9).
11. **Secrets can't be crafted.** They stay a true find, from a box or a trade (decided October 9).

Odds must be shown before purchase and before opening (store requirement). Because paid boxes skip toys you own, the odds shown must be **your** odds for the next box, including the secret's chance and how a longer walk changes it.

## How the pieces fit

**Walk tiers (adopted, numbers to tune).** The free daily box comes from the newest series and upgrades with longer walks; it's still one box a day. Walking boxes can give duplicates, which is where spares come from.

| Steps today | Box | Secret chance | Rare finish chance |
| --- | --- | --- | --- |
| 1,000 | Daily box | 1 in 72 (base) | base |
| 5,000 | Daily box, glowing | about 1 in 48 | about 1.5× base |
| 10,000 | Daily box, golden | about 1 in 36 | about 2× base |

The box can be opened at any tier; opening it ends the day's upgrades. Higher tiers depend on dependable background step counting (UX-010).

**Crafting (adopted).** 5 spares from one series craft 1 regular toy you're missing from that series. The spares are retired in the ledger ("used in crafting"), and the crafted copy comes from the remaining supply and is marked "Crafted". A sold-out toy can't be crafted. Crafting validation moves to the server once inventory is server-owned.

**Paid box edge cases (proposed).** A paid box draws only from regular toys you don't own, plus the secret at its normal relative weight. When you own every regular toy, the paid box closes even if you lack the secret; the secret then comes only from walking boxes or trades. A toy that sells out drops out of every pool, and the remaining toys' odds are re-shown.

**Supply and walking (proposed).** Walking boxes draw from the same finite supply as paid boxes. Edition sizes are set per series before release (placeholder: 5,000 per regular toy, 500 per secret). When the newest series' walking pool runs low, the daily box keeps working from the toys still available.

**Public ledger (recommended approach).** A server-side, append-only record per copy: serial, series, first owner, date, source (walking, purchase, craft), every trade, and retirement by crafting. The public view shows serials, counts, and display names only for players who opt in. A blockchain or NFT is not recommended: app-store rules for NFTs, regulatory risk, and cost outweigh the benefit, and a well-run server ledger gives the same transparency.

**Trading (adopted).** Friend swaps: one-for-one, both confirm. Open market: players list spares with what they want in return ("have / want"), and the server matches and swaps both items at once so nothing is lost mid-trade. No currency, no cash-out, no gifting of your only copy. It needs accounts, a server that owns inventory, rate limits, reporting and blocking (UX-013, UX-016–019).

**Legal note.** Paid random boxes plus finite, tradable items draw closer scrutiny under loot-box and gambling rules in some countries, even without a cash-out. The no-money and keep-your-copy rules help, but get legal advice on the box mechanics, trading, and age rating before launch.

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

Per owned copy (server-owned once accounts exist): **serial number**, **first-found date (UTC)**, and **source** (walking box, purchased box, crafted, or traded), recorded once and never overwritten. Per collectible: **edition size** and **remaining supply**. This extends the version-2 save; older saves should show "found before dates were recorded" rather than inventing a date.

Per character: a short **idle** and an **eager** animation (hop, wave, peek) for the collection lineup. The lineup animates only the handful of toys on screen; the shelf keeps static thumbnails from the bounded preview cache.

## Not decided yet

- Edition sizes per toy and secret, and the exact walk-tier odds.
## Backend (decided October 9)

**Supabase** is the backend for accounts, server-owned inventory, finite supply, purchase validation, crafting, the ledger, and trading. Inventory and rewards are Codex's lane, so Codex drafts the schema and server functions against the rules above; Claude reviews them and builds the UI on top. Until it's live, the local save stays the source of truth and the UI marks online features as coming soon.
