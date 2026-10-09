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
| Home and navigation | First Home hub with Collection/Rewards/Friends tiles and Home/Boxes/You tabs implemented. Back history and per-page scroll preserved; phone review pending. |
| Settings | Full local screen with mute/volume, haptics, motion, walking access/status, help, and About implemented. Device review pending. |
| Purchases | Catalog and unavailable-checkout messaging implemented. No charge or paid inventory grant is possible yet. |
| Profile and social | Editable local display name, Pip avatar, owned favorite, and collection summary implemented. Discover/Friends entry screens have explicit unavailable states. Accounts, cloud save, public publishing, and live shelf services remain pending. |
| Achievements | No badges, milestone rewards, or lifetime validated stats implemented. |

## Confirmed direction

The owner selected a **separate home hub**, with **Collection**, **Rewards**, and **Friends** tiles. Home shows a personal greeting, today's walking progress, and a ready-box cue; the collection shelf is its own destination. Tap a shelf toy for immersive play, then return to the same shelf position.

The owner also selected **public profiles and shelf discovery**. Browsing a published profile or shelf does not require becoming friends first. Friends remains a distinct relationship with requests and a friends list; discovery is the route to finding people and visiting their published shelves. Public-profile, visibility, and shelf services still need implementation.

Proposed navigation within those confirmed choices: bottom tabs **Home**, **Boxes**, and **You**. Home's tiles open the collection shelf, rewards inbox/milestones, and a social screen with **Discover** and **Friends** sections. You contains the profile, badge cabinet, and Settings. Visited shelves identify their owner and provide a clear return route. Exact tab labels and layout remain design work under UX-001; the hub and public-discovery choices are settled.

## Implementation progress — Home hub 01

| Task | Implemented in this pass | Remaining acceptance work |
| --- | --- | --- |
| UX-001 | Home/Boxes/You tabs, contextual Back, shelf/play/settings history, session scroll restoration | Device input review; visited shelves depend on services |
| UX-003 | Personal greeting, three hub tiles, actual walking/collection state, ready daily-box action | Visual/art polish and phone review |
| UX-006 | Full local Settings, persistent volume/mute and existing comfort choices, activity status/help/About | Permission/device review; account controls after accounts exist |
| UX-009 | Home remaining steps and ready/claimed states, UTC reset countdown, shared Rewards/Boxes claim card | Confirm production day policy; dependable tracking remains UX-010 |
| UX-015 | Saved local name/avatar, owned favorite/play action, actual collection summary | Device/keyboard review, badges and public visibility after services exist |
| UX-017/020 | Discover/Friends sections and clear empty states | Real requests, discovery, publishing and shelf visits remain unimplemented |

See [Home hub review](HOME_UI.md) for routes, limitations, and phone checks. Checkboxes below stay open until each task's full acceptance criteria pass. No achievement or social service is represented as completed by this UI pass.

## Implementation progress — Visual system 01 and Home/shelf 01 (Claude)

| Task | Implemented in this pass | Remaining acceptance work |
| --- | --- | --- |
| UX-002 | Theme tokens (`PockleTheme`), Fredoka/Nunito type, squishy plum wordmark matching the box art, tab and header icons, card shadows, button lips, press squish with calm variant, best-fit text guard. See [VISUAL_SYSTEM.md](VISUAL_SYSTEM.md). | Unity compile/render check, small-phone readability, CJK/emoji fallback on device, owner's look-and-feel review |
| UX-003 | Home: today's box card with Jelly Garden box art, step count and progress, status, and one action that turns primary when the box is ready; "Your places" with a Collection tile showing owned toys, plus Rewards (live daily status) and Friends ("Soon") half tiles. See [HOME_UI.md](HOME_UI.md). | Same Unity/device review; production onboarding (UX-007) still decides first-run content |
| UX-004 | Shelf: discovery progress card, display cubbies on ceramic shelves, title-case names, duplicate badges, favorite heart, mystery cubby ("???") for undiscovered toys, centered partial rows, 1–4 columns by width, and an honest "more collections" row. | Same Unity/device review; favorite/reorder actions and 100+ item browsing move to UX-031 |

## First: mobile game foundation

- [ ] **UX-001 — Finalize the screen map and navigation.** Use the confirmed home hub with Collection, Rewards, and Friends tiles; define tab destinations, back behavior, and routes between play, reveal, boxes, discovery, and visited shelves. Acceptance: every primary destination is reachable without losing the current shelf/scroll context.
- [ ] **UX-002 — Establish the visual system.** Squishy Pockle logo, typography, color tokens, spacing, button states, icons, and surfaces. Acceptance: home, boxes, profile, and settings look like the same game, with readable text and generous touch targets on a small phone.
- [ ] **UX-003 — Build the finished Home screen.** A personal greeting, Collection/Rewards/Friends tiles, today's walking progress, and one clear daily-box action. Acceptance: opening the app makes the next useful action obvious; there is no prototype control card or debugging information.
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
- [ ] **UX-017 — Build friend invitations and the friends list.** Friend code/link, pending requests, acceptance, removal, and empty/offline states. Acceptance: a user can establish a connection and manage it without an unsolicited contacts upload. Public discovery is confirmed; friend requests and relationship management depend on accounts and the social service.
- [ ] **UX-018 — Build another user's shelf view.** Owner identity, showcased toys, collection arrangement, loading/private/unavailable states, and back navigation. Acceptance: visiting is read-only and never treats the owner's toys as the visitor's inventory. Depends on the public-profile/shelf service and server-enforced visibility; public visits do not require friendship.
- [ ] **UX-019 — Add social controls.** Visibility choices, block/report where appropriate, invitation limits, and hidden-profile behavior. Acceptance: the confirmed public-discovery model respects each shelf's publication/visibility state consistently in the UI and service, including after a connection is removed.
- [ ] **UX-020 — Build public profile and shelf discovery.** A Discover section, profile previews, public shelf browsing, search/browse behavior, and loading/empty/error states. Acceptance: published profiles lead to their shelves without requiring friendship; hidden shelves are excluded, and prototype data is clearly distinguished from live users. Public discovery is approved; implementation depends on profile/shelf services.

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

## Expanded roster and catalog scope

The owner clarified **one collection per distinct character, with at least ten color/texture/material varieties**. Pip is one collection in Series 1. At least twelve characters means a minimum 120 planned collectibles. See [character/art tasks ART-001–005](CHARACTER_ROSTER.md) for the draft roster and material pipeline. These are planned assets; they are not additional toys already available in the build.

- [ ] **UX-029 — Replace the four-variant prototype catalog.** Stable character, finish, and collectible IDs; data-driven definitions and box pools; explicit migration from the four Pip entries. Acceptance: existing counts, favorites/avatars, and pending reveals survive; catalog ordering cannot change saved identity; future IDs are preserved safely. Coordinate runtime/UI contracts before parallel edits. **Foundation merged into `main` from `codex/catalog-migration`:** read-only 108-entry catalog, four available assets, ID inventory/profile migration, explicit existing pools, and portable checks; Unity migration validation and Claude HUD integration remain pending. See [catalog API](CATALOG_API.md).
- [ ] **UX-030 — Support material-specific toy handling and loading.** Jelly, foam, flock/plush, and firm vinyl profiles with reviewed grip/face anchors, calm motion, feedback, and per-selection asset loading. Acceptance: finishes feel different on the phone, rigid toys do not stretch like jelly, and assets/materials are reused or released appropriately.
  - Series/motion follow-up on `codex/series-metadata-idles`: immutable collection metadata/disclosed odds and twelve idle/eager recipes, with an opt-in, visible-only animator and a shared budget. Pip is Series 1; dates, other characters' numbering, and real secret identities remain unset, and current beta pools are unchanged. Claude's lineup/shelf integration and Unity/device verification are pending. See [series and motion API](SERIES_AND_MOTION_API.md).
  - In progress on `codex/material-toys`: three draft models / four study finishes, flock/vinyl/plush/foam rendering and handling, explicit ID asset loading, same-character finish reuse, and an editor review launcher. [Study details](CHARACTER_STUDIES.md). UI integration and phone acceptance remain pending.
- [ ] **UX-031 — Scale browsing to at least 120 collectibles.** Cached previews, bounded visible-item work, character/material filters, owned/undiscovered states, and selection identity. Acceptance: the full roster is navigable without rendering every toy live, startup allocation stays bounded, and existing play/back context remains reliable. Availability and box odds reflect approved pools rather than automatically including planned art.
- [ ] **UX-032 — Rebuild the HUD to the approved v2 screens.** Home, Shelf, Toy, Box reveal, Boxes, You, Settings, Friends, Collection, and Collections, following the owner-approved "Pockle screens v2" canvas and `COLLECTIONS.md`. Acceptance: matches the canvas on a phone, keeps existing HUD contracts and tests, and Motion calm stops lineup animation.
- [ ] **UX-033 — Collection lineup and series browsing.** Collection page with animated lineup (found idle, unfound eager silhouettes, secret slot with odds) and newest-first series list; shelf grouped by series with Missing/Duplicates views. Depends on the catalog fields in `COLLECTIONS.md`.
- [ ] **UX-034 — Walk tiers and crafting.** Daily box from the newest series that upgrades at longer step goals; craft a missing toy from same-series spares. Acceptance: tier odds are shown before opening; crafting spends exactly the stated spares once and records "Crafted"; depends on dependable walking (UX-010) for higher tiers. Trading follows accounts and a backend (UX-013, UX-016–019).
- [ ] **UX-035 — Finite editions, never-duplicate paid boxes, and the public ledger.** Edition sizes and serial numbers per copy; paid boxes draw only toys you don't own and close once the regular set is complete; sold-out states; an append-only ledger with an opt-in public view. Acceptance: supply can't go negative under concurrent purchases; every copy has one serial and a complete history. Depends on the backend choice.
- [ ] **UX-036 — Trading: friend swaps and open market.** Spares only, one-for-one, no currency; open-market "have / want" listings matched by the server with both items swapped at once. Acceptance: you can never trade your only copy; interrupted trades never lose or duplicate a toy; reporting, blocking, and rate limits exist. Depends on accounts and UX-035.

## Suggested work order

1. Review **Home hub 01** and the stronger toy feel when the owner is back; both are available for the next phone build.
2. Refine the implemented Home/Settings/profile navigation after that review, then polish the shelf and first-session onboarding (UX-001–008, UX-015).
3. Finish reward delivery and dependable walking, alongside the backend/billing decisions (UX-009–014).
4. Add profile and cloud save before friends and visited shelves (UX-015–020).
5. Define badge rules early; implement grants once validated events exist (UX-021–024).
6. Apply polish and test the complete journey (UX-025–028).

Use this file as the task checklist. Each implementation should name its task ID, update its status, and record relevant evidence in `VALIDATION.md`. Prototype capabilities above are already available; unchecked tasks describe the remaining finished-game work, not features currently promised by the app.

For the proposed Codex/Claude work split and repository handoffs, see [agent coordination](AGENT_COORDINATION.md). Assignments remain proposed until each assistant acknowledges its task and file ownership.
