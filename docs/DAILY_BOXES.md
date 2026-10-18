# Daily boxes: the smart mix (owner decision, October 15)

Status: **decided, not implemented.** Reward rules and saves are Codex's lane (`Runtime/Core/`, `CollectionProgress`, `CollectionSession`, portable checks); the Home and box-picker screens are Claude's. Numbers marked *default* are first proposals the owner can change.

## How many boxes

- Up to **4 boxes a day**, each unlocked by a step tier: **1,000 / 3,000 / 6,000 / 10,000** steps (*default*).
- A tier's box waits until you open it; unopened boxes don't carry over past the day (UTC day, as today).
- Opening is still one tap and still saves before any reveal animation (AGENTS.md "Saves").

## Which collection each box comes from

| Box | Collection | Why |
| --- | --- | --- |
| 1st of the day | **Your pick** from collections that have playable toys | You always progress on what you care about. |
| 2nd–4th | **80%** the featured collection, **20%** any other playable collection, equally (*default*) | Keeps new collections visible and gives "come back tomorrow" pull. |

- **Featured collection** rotates **weekly** (*default*), newest unnumbered collections first, then the rest in catalog order. It's the same for every player in the same week, so the activity feed and "New this season" line up.
- If you haven't picked by the time you open box 1, it uses the collection you're closest to completing (the first "Keep collecting" card).

## Which toy inside the collection

- Base odds stay each collection's pool weights; **the secret's odds never change** (e.g. Aurora stays 1 in 72 of Fox boxes).
- **Softer duplicates near the end** (*default*): once you've found at least half of a collection's non-secret toys, each non-secret toy you're missing gets **double weight** in that collection's rolls. No guarantees, no pity counter, nothing that reveals the secret.
- Duplicates still matter: they become fusion spares.

## Fairness and safety

- Odds shown to players must match the code (the collection page already states the secret's odds). If the featured split or duplicate softening changes, that text changes with it.
- No paid boxes are involved in this rule; paid boxes, when they exist, are validated server-side and granted once (AGENTS.md "Money").
- Save format: needs per-day state for up to 4 tiers (claimed flags), the picked collection for box 1, and the week's featured id. This is a save format change, so it needs a versioned migration that keeps current beta saves (one claimed box today maps to tier 1 claimed).

## What the UI will show (Claude)

- Home's box card: "Box 2 of 4", the next tier's steps ("1,200 to go"), and the box art of the collection it will come from (your pick for box 1, the featured collection for 2–4 with "Featured this week").
- Box 1: tapping Open asks which collection, with each one's art and progress, then opens it.
- The Collections page marks the featured collection.

## Requested from Codex

1. Core API, engine-free, with portable checks: `DailyBoxes` (tier thresholds, which tiers are unlocked/claimed for a step count), `FeaturedCollection(int weekIndex)`, and `ChooseCollection(int tierIndex, string? picked, Random)` → collection id; within-collection roll with the duplicate softening.
2. Save migration for the new per-day state.
3. `CollectionSession`: `ClaimDaily(tier, pickedCollectionId)`, plus read-only `DailyBoxState` for the HUD (tier, steps to next, collection for the next box, featured id). The HUD will call only these.
