# Rewards, progression and economy decisions

October 16, 2026. Decisions from the owner's planning session with Claude. Screens are in the "Pockle screens v2" design canvas, row **"Badges, milestones and challenges"**: `PinBoard`, `MilestoneWall`, `Challenges`.

Items marked **CHANGED** replace an earlier rule in `COLLECTIONS.md` or `AGENT_COORDINATION.md`. Items marked **CONFIRM** still need the owner's sign-off before anyone builds on them.

## 1. Boxes and odds

- **One toy per box.**
- **Walking box:** 1,000 steps earns a box, and a box can drop **every 3 hours**. **CHANGED** (was one daily box, then every 6 hours).
- **Paid box:** $0.99 for one box, no walking needed.
- **Rarity per toy:** each toy has 10 variants: **6 common, 3 rare, 1 secret**.
- **Odds per box:** 75% common, 22% rare, 3% secret. **CHANGED / CONFIRM:** 3% is about 1 in 33, which is more generous than the earlier 1-in-72 rule. Pick one before the catalog weights are set.
- **Drop-rate disclosure (required by Apple for paid random items):** an info button on the paid box screen opens a table with the chance per rarity and, for capped series, how many are left. The same table goes in the store listing and on the website.
- **Belgium:** no paid boxes in Belgium (paid loot boxes are effectively banned there). Either gate the paid box by region or leave Belgium out of the launch.

## 2. Supply, serials and series

- **Every copy gets a serial number** (for example #0042), shown on the toy and in the showcase.
- **Launch series (8 toys × 10 variants = 80 collectibles):** open or only loosely capped, so late players aren't locked out.
- **Christmas series:** the first truly limited series. It is planned as the first drop after launch (November is skipped).
- **Retiring series. CHANGED / CONFIRM:** `COLLECTIONS.md` says every series stays on sale forever. This session leaned toward limited series that **retire** (Christmas first). Retired series stay on shelves ("vintage matters") but can't be pulled from boxes, and their series badges become **vaulted**. The owner needs to choose between the two rules.
- **Hard caps:** reserved for secrets and fusion-only Pockles, not commons.

## 3. Duplicates and fusion

- Two spares fuse into a **fusion-only Pockle** that never appears in a box.
- Fusion works **only inside the same collection**.
- The result follows the two spares' personality, temperament, material and rarity.
- The fusion screen **hints** at personality and temperament before you fuse. The **toy's look is revealed only after tapping Fuse.** That reveal gets its own animation and the best sound in the game.
- Fusion-only Pockles are good candidates for numbered, capped editions.

## 4. Competition

- **No global leaderboard at launch.** The player base is small, and with paid boxes it would turn into pay-to-win.
- A **series-only leaderboard** (for example, Christmas only) is the first thing to try later.

## 5. Play: live shelf

- The live shelf holds **6 Pockles** from **any collection**. Temperament and personality decide how neighbours interact.
- Launch extra: **season challenges** (below).
- Later ideas, not scheduled: visiting friends' shelves, weekly shelf contests with votes, and moods based on who sits next to whom.

## 6. Badges (pin board)

- **Look:** 3D **enamel pins**. **One base pin model** in Unity; swap the icon and colours per badge. Pins spin and shine when tapped or unlocked. Players can later display pins on the shelf.
- **Tiers:** bronze, silver and gold rims on the same pin. One model covers three rewards.
- **Categories (tabs):**
  - **Toys:** bronze at 5 of 10 variants, silver at 9 of 10, gold with the secret (all 10). The 5/9/10 thresholds are proposed. **CONFIRM.**
  - **Series:** complete a whole collection. These are the big ones. When a series retires, its pins show a **VAULTED** stamp and can't be earned anymore.
  - **Traits:** every temperament, every material, a set of one temperament. These cut across series.
  - **Fusion:** first fusion, 10 fusions, discover every fusion in a collection.
  - **Activity:** step milestones, box streaks, shelf moments (for example, two rivals side by side who become friends).
- **Unearned pins** show as a faded outline with a progress ring ("3 of 5").
- **Where:** You tab → Badges (pin board). It links to Milestones.

## 7. Milestones (progress wall)

- **Lifetime counters that only go up:** steps walked, boxes opened, Pockles collected, fusions made, days played.
- Each counter has **checkpoints**. Hitting one gives a **free box** or moves an Activity pin up a tier. No new art is needed.
- **Screen:** one row per counter with a chunky bar and evenly spaced checkpoint dots, plus a chip with the next reward. Tapping a row lists every checkpoint and what it gives.
- **Where:** You tab, next to the pin board.

## 8. Challenges

| Cadence | Example tasks | Reward |
| --- | --- | --- |
| Daily (3 tasks) | Walk 3,000 steps · open 2 boxes · poke a Pockle 5 times | Finish all 3 → **1 free box** |
| Weekly (2 tasks) | Fuse twice · seat 2 calm Pockles next to a grumpy one | Each → a **shelf prop** (decoration) |
| Monthly (1 task) | Complete 3 toys from the current series | **Exclusive Pockle**, never in boxes, shown as a silhouette until claimed |

- Each section shows a **countdown** to its reset.
- A finished challenge turns into a **Claim** card. Claiming flips the card, shows the reward in 3D with confetti and a sound, and offers "Place it" for props.
- **Where:** an entry on **Home** (a quest icon with a count of claimable rewards).

## 9. Sound

- **Source:** generate with **ElevenLabs sound effects** (paid plan, for commercial rights), not a free library.
- **Voice recipe per toy:** a base pitch and texture that all of a toy's expressions share, so each toy sounds like one character. Then one sound per expression.
- **Priority sounds:** fusion reveal, challenge claim, pin unlock, box open.
- Prompts are still to be written with the owner.

## 10. Release checklist items raised

- **Google Play:** Pockle needs its **own 14-day closed test with at least 12 testers**. Biotadex testers can be reused, but each must opt in to Pockle specifically.
- **Store listing:** screenshots, description, drop-rate table, privacy policy and a small website (same approach as Biotadex). This is next after the extra characters.
