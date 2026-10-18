# Badge runtime facts — UX-021/024

October17, 2026. Codex runtime APIs for Claude UiBuild25, branch `codex/badge-runtime-facts`. HUD/BadgeBook bindings remain Claude-owned.

After CollectionSession.Initialize():

| Cabinet fact | Runtime API | Meaning |
| --- | --- | --- |
| Fusions | `session.FusionsMade` | Distinct valid persisted fusion receipts; excludes fixed Fusion Lab studies |
| Owned for achievements | `session.BadgeEligibleOwnedCounts` | Proven earned copies only; regular ownership/play still uses OwnedCounts |
| Lifetime steps | `session.LifetimeSteps` | Validated sensor deltas; uncapped long total |
| Best walking day | `session.BestDailySteps` | Uncapped highest validated day |
| Current walking day | `session.DailyWalkingSteps` | Uncapped progress for the saved UTC day |
| Walking dates | `session.WalkingDays` | Distinct UTC days with validated steps |
| Walking-box dates | `session.WalkingBoxDays` | Distinct successful daily-box claim days |
| Walking streak | `session.LongestWalkingStreak` | Longest known consecutive walking-day run |
| Box-claim streak | `session.LongestWalkingBoxStreak` | Longest known consecutive box-claim run |

## Requested Claude bindings

- BadgeFactsNow: set Fusions to session.FusionsMade (0 is tracked), Owned to session.BadgeEligibleOwnedCounts. Do not change global toy ownership or collection-progress UI.
- Add optional tracked facts in BadgeFacts for lifetime/best steps and chosen streak metric, then measure First10kDay/1MSteps. The current authored streak says open a walking box 30 days in a row, so LongestWalkingBoxStreak supplies that fact. Walking-day history is also exposed if the owner changes the wording later; Codex has not made that product choice.
- Keep unknown-feature trading/gifting rules untracked. Existing collector-level proposal is unchanged.
- Keep BadgeCabinet.Key `pockle.badges.earned.v1` and its existing string format. CollectionSession automatically merges that preference with the saved snapshot at startup, before the HUD is built; checks each regular once-per-second session poll; and flushes new dates to the primary collection save. No cabinet API change is required. Read-only saves do not import/overwrite badge preferences.

## Version4 data and migration

CollectionSave.BadgeState stores history, earned-copy counts and the earned-date snapshot. Versions1/2/3 migrate; `.before-v4` keeps original JSON. Unknown toy IDs, inventory stacks, existing fusion receipts and pending reveals survive. Unsupported/damaged saves remain read-only.

Past full step history and copy origins were never recorded. Migration carries only the old saved day's known capped steps/claim as a lower bound, and recognizes valid v3 fused-toy receipts as proof of earned results. It invents no missing days, million-step totals or earned provenance for old regular inventory. HistoryStartDay identifies the beginning of that known data. Starter/test grants remain owned/playable but do not create eligible copies. New validated walking rewards and user-created fusions do; fusion consumes unearned extras first so a real earned copy is preserved when possible. Existing earned badges/dates are never retroactively revoked.

Badges remain local. Restoring a collection save can restore its badge dates; uninstalling without a backup/cloud save still removes local data. No accounts or cloud service are added.
