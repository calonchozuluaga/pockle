# Codex and Claude coordination

Owner-approved October 10 follow-up: Codex is integrating the v2 HUD and UX-033 collection routes in `codex/v2-collection-integration`, using snapshots of Claude's published code rather than editing Claude's branch. The integration combines existing PR #6/#7/#8 content and PR #9 contracts, adds reachable collection pages and backdrop binding, and retains both handoffs. Static portraits are shared; live lineup rendering and visual shader tuning remain separate. Review this branch as one test build before choosing a main merge path.

October 10: Codex accepts Claude's v2 runtime requests on `codex/collection-backend-contracts`: collection navigation, stage backdrop, and backend/copy design. Exact contracts and remaining HUD work are in [COLLECTION_RUNTIME_API.md](COLLECTION_RUNTIME_API.md); backend review is in [BACKEND_PLAN.md](BACKEND_PLAN.md). Existing series metadata/motion lives in PR #7 and should be reused. This branch does not merge PR #6/#7/#8 or change Claude's HUD. Check-in tuning, fusion and living-shelf proposals remain distinct from implemented beta features.

Prepared October 8, 2026. This is an asynchronous handoff through the shared GitHub repository, not a live connection between the two assistants. Each assistant must fetch the latest repository and read the other's notes at the start of a task. The owner reports that Claude is participating. Claude acknowledged its assignments in `handoffs/claude.md` (UX-002–004, UX-031, ART-001, ART-005).

## Shared context

- Repository: `calonchozuluaga/pockle`. Current integrated baseline: Home and shelf 01, catalog/save ID foundation, ART-001 concept sheets, CI, and editor-only Moss/Bop/Nook studies. Exact source branch tips and review limits are recorded in `handoffs/codex.md` under the October 9 integration.
- Unity **6000.6.5f1**, Built-in Render Pipeline, UGUI **2.6.0**. The owner tests Android APKs on their own computer and phone.
- Product direction: Home hub with Collection/Rewards/Friends tiles; Home/Boxes/You tabs; public profiles and shelf discovery eventually. Walking target: **1,000 steps per daily Jelly Garden box**. Proposed prices: $0.99 standard, $2.99 special.
- Implemented: four separate shelf toys, glossy/pearl/glitter/soft finishes, lift/sag/gravity, two-finger squish/stretch, shake jiggle, branded daily opening, local walking rewards, Home, full Settings, and a local profile.
- Pending: live accounts, public shelves, friends, cloud save, verified purchases, authoritative rewards, badges/milestones, dependable background walking, glow environments, and reactive filling.
- Read `UI_UX_TASKS.md`, `HOME_UI.md`, `COLLECTION_UI.md`, and `VALIDATION.md`. Keep implemented local features distinct from unavailable online services.
- Expanded roster requested by the owner: at least **12 distinct character collections with at least 10 color/texture/material varieties each**. Pip is one collection in Series 1; finish themes are not separate collections. Read `CHARACTER_ROSTER.md` for the updated minimum 120-collectible plan, ART-001–005, and UX-029–031. The current build still has four finishes of Pip; the proposed roster is not shipped content.

## Proposed division

| Assistant | First area | File boundaries | Status |
| --- | --- | --- | --- |
| Claude | UX-002 visual system, then UX-003/004 Home and shelf polish: squishy branding, typography, spacing, icons, card/button states | `PrototypeHud*.cs`, new UI artwork/resources, UI PlayMode tests, UI design notes | **Acknowledged** in `handoffs/claude.md`; UX-002 in review (PR #2) |
| Codex | Runtime interaction, Android walking integration, reward/inventory reliability, and portable checks; use device feedback for UX-025 and prepare UX-010/011 work | `TactilePrototype.cs`, `JellyToy*`, runtime Core, `CollectionSession.cs`, `AndroidWalkingTracker.cs`, Android plugin, portable checks | Proposed; no new implementation claimed |

Core `MenuNavigation` and `ProfileName` remain Codex's area. Claude can request a change through their handoff rather than editing those files concurrently. UI work should preserve the current HUD callbacks/events, `ShowToy`, `GoBack`, `StoreVisible`, settings restoration, and collection binding. Codex should request any HUD change through Claude while Claude owns UI work. Backend/authentication and billing choices are separate tasks; neither assignment implies a selected provider or configured service.

For the larger roster, the proposed extension is Codex on catalog/save migration and material handling/runtime; Claude on character/material concept refinement and collection UI presentation. Agree the catalog-to-HUD interface and generated asset ownership in handoffs before implementing either side. The owner can talk to both assistants simultaneously, but that does not create an automatic connection between their chats.

## How to work together

1. Fetch the latest `origin/main`, read this document and both handoff notes, then claim a specific task ID and list the files you will change in your own note. Check for overlapping work before editing.
2. Use separate checkouts/worktrees and feature branches, such as `claude/ui-polish` and `codex/walking-rewards`. Two assistants should not switch branches inside the same working directory.
3. Make small commits with the relevant UX task IDs. Use a pull request or leave the branch/commit reference in the handoff for review. Read-only reviews of each other's changes are welcome.
4. Keep `main` as the owner's testable build. Merge reviewed branches sequentially, fetch again before publishing, and resolve overlaps without overwriting the other assistant's work. Do not force-push shared branches.
5. Write your own note at `docs/handoffs/codex.md` or `docs/handoffs/claude.md`: task/status, branch and commits, files changed, checks actually run, checks still pending, and any requests for the other assistant. Separate notes avoid both editing the same status log.
6. Update task acceptance status accurately. Portable/source checks do not establish Unity compilation, rendering, Gradle packaging, or actual sensor behavior. The owner's phone review remains necessary.

The assistants only receive repo notes when they read/fetch them; this workflow does not automatically wake the other assistant or deliver a live chat message.

## Starter message for Claude

> You are collaborating with Codex on calonchozuluaga/pockle. Fetch the latest main and read docs/AGENT_COORDINATION.md, docs/UI_UX_TASKS.md, and docs/handoffs/codex.md. Please take the proposed UI/visual design lane, starting with UX-002 and Home/shelf polish under UX-003/004. Work on a separate branch, preserve existing runtime events and reward behavior, and leave your acknowledgment, file ownership, progress, and requests in docs/handoffs/claude.md. Codex handles runtime interactions, Android walking, reward logic, and portable validation. Coordinate shared interfaces through the handoff notes before changing both sides.
