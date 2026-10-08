# Pockle UI/UX task tracker

Updated October 8, 2026. This is the working backlog for making Pockle feel like a finished mobile collecting game. Task IDs stay stable so feedback and commits can refer to them. A task becomes complete when its acceptance criteria pass; a working prototype is not the same as a finished screen.

The collecting loop remains **walk → earn a collection box → reveal a toy → enjoy your shelf**, with an optional purchase path. Profiles, friends, shelf visits, rewards, badges, and milestones are now requested planning scope. Trading, memberships, competitive ranks, and a new currency are not part of this plan.

## Current baseline

| Area | Current state |
| --- | --- |
| Toy play | Four finishes, direct touch, lift, pinch, plate rotation, shake, sag, and gravity landings implemented. More expressive tuning is awaiting the next phone review. |
| Collection | Separate shelf entries and local duplicate counts implemented; the beta gifts all four variants for testing. Layout and art remain a first UI pass. |
| Boxes | Collection art, proposed prices, provisional contents/odds, and branded Jelly Garden opening implemented. |
| Walking | Local 1,000-step daily box and Android hardware counter implemented. Full background tracking and authoritative validation are pending. |
| Settings | Sound, haptics, and full/calm motion persist. A complete settings screen is pending. |
| Purchases | Catalog and unavailable-checkout messaging implemented. No charge or paid inventory grant is possible yet. |
| Accounts and social | No account, cloud save, profile, friends, or visited-shelf service implemented. |
| Achievements | No badges, milestone rewards, or lifetime validated stats implemented. |

## Navigation proposal

Working default, pending the owner's preference: **Home** is your shelf, with the daily walking box/progress within reach. Bottom navigation has **Home**, **Boxes**, and **You**. Tap a toy for immersive play; back returns to the same shelf position. **You** leads to the profile, badges, milestones, friends, and Settings. Friend shelves are clearly labeled with their owner and have a direct route back home.

A separate home hub is the alternative under discussion. Private friends/invites are the working social default; public discovery is an alternative, not an assumed feature. These choices are recorded here so we can revise the layout without treating a default as an approved final design.

## First: mobile game foundation

- [ ] **UX-001 — Finalize the screen map and navigation.** Choose shelf-home or a hub; define tab destinations, back behavior, and routes between play, reveal, boxes, and visited shelves. Acceptance: every primary destination is reachable without losing the current shelf/scroll context.
- [ ] **UX-002 — Establish the visual system.** Squishy Pockle logo, typography, color tokens, spacing, button states, icons, and surfaces. Acceptance: home, boxes, profile, and settings look like the same game, with readable text and generous touch targets on a small phone.
- [ ] **UX-003 — Build the finished Home screen.** A personal greeting, collection display, today's walking progress, and one clear daily-box action. Acceptance: opening the app makes the next useful action obvious; there is no prototype control card or debugging information.
- [ ] **UX-004 — Polish the shelf.** Consistent shelves/shadows, toy names, counts, selection feedback, empty spaces, and room to grow beyond four entries. Add favorite/reorder options after the core layout works. Acceptance: each collectible reads as its own toy; a larger collection stays smooth to browse.
- [ ] **UX-005 — Finish toy details and play transitions.** Name, collection, finish, acquisition details, favorite action, and a quiet route into play. Acceptance: transitions preserve context, gestures stay direct, and decorative UI does not obstruct the toy or plate.
- [ ] **UX-006 — Build full Settings.** Sound level, haptics, motion, walking permissions/status, support/about, and account controls when accounts exist. Acceptance: choices persist; calm mode remains comfortable; denial and unavailable sensors have understandable recovery paths.
- [ ] **UX-007 — Add first-session onboarding.** Give the first companion, show one direct-touch hint, explain walking versus buying, and request activity access when the user chooses walking. Acceptance: the user can play as a guest and can skip explanations; production onboarding does not gift the entire test roster automatically.
- [ ] **UX-008 — Validate the mobile shell.** Safe areas, Android Back, portrait/landscape choice, small screens, tablets, scrolling, loading/error/empty states, and tap feedback. Acceptance: no clipped controls, dead ends, duplicate taps, or accidental toy gestures during navigation.

## Next: walking, boxes, and real rewards

- [ ] **UX-009 — Finish the daily walking card.** Goal, progress, ready/claimed states, reset countdown, and a clear opening action. Acceptance: users understand how much remains and when another box arrives, including the chosen day/time-zone policy.
- [ ] **UX-010 — Make walking dependable away from the screen.** Choose and integrate the appropriate Android background/health-data approach; explain tracking and permissions plainly. Acceptance: a real pocket walk, process interruption, restart, and denied access behave as described; the app never promises steps it cannot read.
- [ ] **UX-011 — Add a rewards inbox and claim history.** Ready boxes, pending reveals, delivered rewards, and a return to the new toy on the shelf. Acceptance: interruptions and repeat taps do not duplicate or lose a reward; offline/pending states are understandable.
- [ ] **UX-012 — Polish every collection's unboxing.** Matching box art, opening feedback, reveal identity, duplicate handling, and a clear “Add to shelf / Play” moment. Acceptance: the collection is known before opening, the variant is revealed during opening, and the art matches the selected collection.
- [ ] **UX-013 — Connect authoritative walking and inventory.** Define validated progress, reward eligibility, anti-cheat movement checks, and account/save reconciliation. Acceptance: server-awarded boxes are idempotent and distinct from local beta/test gifts. Depends on service choice.
- [ ] **UX-014 — Finish the shop and billing.** Localized store prices, collection details/odds, native checkout, cancellation, pending payments, failed payments, and redelivery. Acceptance: a verified transaction grants exactly one box; unavailable checkout cannot charge or award. Depends on Google Play products, billing integration, and a validation backend.

## Then: you, friends, and visiting shelves

- [ ] **UX-015 — Design the profile.** Display name, original avatar choices, favorite toy, collection summary, badge display, and profile visibility. Acceptance: a guest profile works locally and clearly distinguishes personal details from anything shared.
- [ ] **UX-016 — Add account linking and cloud save.** Define sign-in, guest-to-account migration, returning users, device changes, conflicts, and sign-out. Acceptance: existing earned ownership is preserved and reconciled; the UI explains sync status. Depends on authentication and inventory services.
- [ ] **UX-017 — Build friend invitations and the friends list.** Friend code/link, pending requests, acceptance, removal, and empty/offline states. Acceptance: a user can establish a connection and manage it without an unsolicited contacts upload. Depends on social visibility choice and accounts.
- [ ] **UX-018 — Build another user's shelf view.** Owner identity, showcased toys, collection arrangement, loading/private/unavailable states, and back navigation. Acceptance: visiting is read-only and never treats the owner's toys as the visitor's inventory. Depends on server-approved shelf visibility.
- [ ] **UX-019 — Add social controls.** Visibility choices, block/report where appropriate, invitation limits, and hidden-profile behavior. Acceptance: the chosen private/public model is enforced consistently by the UI and service, including after a connection is removed.
- [ ] **UX-020 — Decide whether public discovery belongs in the first release.** If selected, design browsing/search and visibility rules before implementing it. Acceptance: discovery has a defined audience and scope; it does not accidentally expose private shelves. Pending owner direction.

## Progression: badges and milestones

- [ ] **UX-021 — Define the badge set and earning rules.** Suggested starting ideas: first unboxing, first earned walking box, a collection completed, and a cumulative walking milestone. Acceptance: each badge has a clear rule and icon; beta starter gifts do not count as earned achievements. The final set needs a content decision.
- [ ] **UX-022 — Design milestone progress.** Suggested cumulative walking tiers: 1,000 / 10,000 / 50,000 validated steps; collection milestones can use distinct discovered toys. Acceptance: requirements, progress, and the exact reward are visible before claiming. Thresholds and rewards remain proposals.
- [ ] **UX-023 — Implement achievement state and delivery.** Track validated events, unlock each badge once, and deliver each approved milestone reward once. Acceptance: reopening, reinstalling, and syncing do not reset or duplicate grants. Depends on authoritative stats and inventory.
- [ ] **UX-024 — Add the badge cabinet and milestone screen.** Earned/locked states, progress, claim feedback, and optional badges displayed on the profile. Acceptance: the screens feel rewarding without introducing a mandatory streak, countdown pressure, or an unrelated currency.

## Polish and release readiness

- [ ] **UX-025 — Tune sensory feedback on a phone.** Review exaggerated shake, sag, high/low drops, material-specific stiffness, synthesized sound replacement, and native haptics. Acceptance: playful full motion, stable bounds, satisfying settling, and a comfortable calm mode. Latest exaggeration patch is awaiting device feedback.
- [ ] **UX-026 — Add lighting environments and reactive filling.** Lights-off/glow interaction, environment selection, and stars/pearls that lag and drift after a squeeze. Acceptance: choices affect the toy visibly and perform well on the target Android device. These remain planned, not shipped.
- [ ] **UX-027 — Polish loading, notifications, and offline recovery.** Cache shelves, show progress during sync, retain interrupted rewards, and make any reminders opt-in. Acceptance: offline browsing/play is useful and pending actions recover without duplicate delivery.
- [ ] **UX-028 — Run the full first-day journey.** Guest → first toy → walk → daily box → reveal → shelf → profile/badge → friend visit → home; test optional purchase separately. Acceptance: device checks cover interrupted builds/sessions, permissions, accounts, payments, and low-end performance before calling the flow release-ready.

## Suggested work order

1. Review the stronger toy feel when the owner is back; it does not block UI planning.
2. Decide the screen map, then finish Home, shelf, Settings, and first-session onboarding (UX-001–008).
3. Finish reward delivery and dependable walking, alongside the backend/billing decisions (UX-009–014).
4. Add profile and cloud save before friends and visited shelves (UX-015–020).
5. Define badge rules early; implement grants once validated events exist (UX-021–024).
6. Apply polish and test the complete journey (UX-025–028).

Use this file as the task checklist. Each implementation should name its task ID, update its status, and record relevant evidence in `VALIDATION.md`. Prototype capabilities above are already available; unchecked tasks describe the remaining finished-game work, not features currently promised by the app.
