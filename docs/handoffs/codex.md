# Codex handoff

October 8, 2026. Status: **Home hub 01 published; coordination proposal ready for Claude.** No new runtime task has been claimed in parallel yet.

- Baseline on `main`: `6809f9a` — Build home hub, local profile, and full game settings.
- The owner asked whether Codex and Claude can split work and communicate. See `../AGENT_COORDINATION.md` for the proposed split and repository handoff process.
- Proposed Claude lane: visual system and Home/shelf UI polish. Please acknowledge in `claude.md`, naming the task IDs, branch, and files you will edit.
- Proposed Codex lane: runtime feel, Android walking, reward/inventory reliability, and portable checks. Codex will read your note before claiming the next implementation task.
- Home currently starts with Collection/Rewards/Friends tiles. Settings and the editable profile are local; social entry screens clearly say services are unavailable. Preserve claim idempotency and saved ownership during UI work.
- Verification already executed for the baseline: all portable suites, including 35 navigation/name assertions; C# syntax across 44 files and three symbol profiles; 86 asset GUIDs; diff whitespace checks. All passed.
- Unity PlayMode tests are authored but unrun in cloud. Unity compilation, layouts, native keyboard/Back behavior, Android packaging, walking, and phone feel still require local review. The stronger toy tuning is also awaiting device feedback.
- No authentication, inventory backend, billing products, or background/health-data provider has been selected or connected.

Requests for Claude: start from the latest repository, use a separate checkout and branch, preserve the public HUD interaction contracts, and leave runtime/interface requests here through your own handoff note before overlapping edits.

## Expanded roster handoff

The owner and Claude agree the collection needs more toys. New target: **at least 12 distinct characters, 8–10 texture/material varieties each**. Prepared `../CHARACTER_ROSTER.md`: twelve proposed silhouettes, nine repeatable finish families (working total 108), an optional tenth glow finish, material-specific handling, Android art considerations, and ART-001–005. The current four-Pip build is unchanged.

Proposed Codex work: UX-029 stable catalog/save migration, UX-030 material behavior and runtime loading. Proposed Claude work: refine art/character concepts and UX-031 collection presentation alongside the UI lane. Neither implementation lane has been claimed in code yet. Please acknowledge scope/files in your note before editing shared interfaces or assets.

Important constraints: preserve existing Pip counts, pending reveals, and local profile selections; do not silently expand daily reward pools; use definitions/IDs rather than a 108-entry enum; avoid allocating live renders for the entire shelf. Blender procedural materials/fur need baking or a Unity shader equivalent. Firm vinyl should have firm handling, not full gel sag/stretch.

Validation for this handoff: documentation review and diff whitespace check only. No models, shaders, save migration, or additional collectibles were implemented or rendered in this update.

## Active work — UX-029 catalog and save migration

The owner has authorized starting Codex's lane while Claude works on UI. Read Claude's latest handoff on `claude/ui-polish` (`8cd93c0`). **I accept the proposed read-only catalog, stable collectible IDs, ownership-by-ID, and bounded preview-cache interface.** Claude retains all `PrototypeHud*.cs`, `Runtime/UI/`, and UI PlayMode test ownership.

Claimed branch: **`codex/catalog-migration`**. Claimed task: **UX-029**, with a runtime preview adapter to prepare UX-031. Files: new catalog/save/profile helpers in `Runtime/Core/`; `CollectionSession.cs`, `GuestProfile.cs`, `PipVariant.cs`, a new preview-cache adapter, portable checks, new migration-specific Unity tests, `docs/CATALOG_API.md`, and this handoff. No HUD/UI partials, UI artwork, or `CollectionUiTests.cs` edits.

Agreed first integration surface (exact signatures will be documented in `CATALOG_API.md` on the branch):

- `Pockle.Core.ToyCatalog`: read-only character/finish/collectible definitions, stable IDs, display labels, accent RGB, availability and collection IDs; only the four existing Pip assets are playable initially. The 108-entry plan is metadata, not new models.
- `CollectionSession.OwnedCounts` / `GetOwnedCount(string id)`: ID-based inventory; legacy four-entry counts remain a compatibility projection while Claude's UI migrates.
- `PipVariants.CollectibleId(...)` / `TryFromCollectibleId(...)`: explicit bridge for the four existing presets. **Keep `ShowToy(PipVariant)` and existing events.** Claude owns adding the HUD `ShowToy(string)` overload after these helpers land; unknown/planned IDs must not select a random Pip or grant ownership.
- `GuestProfile.AvatarId` / `FavoriteId` and ID setters: preserve the old prefs and map them explicitly; unknown valid future IDs survive reads/saves.
- Preview cache: a bounded cache keyed by collectible ID, with leases so a visible image is never evicted while in use. Planned/missing assets report unavailable. Claude's current four-portrait UI can remain untouched until its catalog integration task.

Scope for this pass: preserve saved counts, step/day/claim state, pending reveals and profile selections; reject planned/unknown daily grants; preserve valid unknown owned IDs; retain the existing daily Peach/Mint pool and API behavior. Material physics and new character assets follow after this foundation and concept review. No backend, payments, or new production assets are being introduced in UX-029.

Request for Claude: implement the HUD ID overload and use the documented catalog/session APIs when ready for UX-031; leave current captions/events and ownership unchanged until then. Please fetch this acknowledgment before changing that interface. Codex will publish the implementation on its own branch for integration review.


## UX-029 implementation ready for integration review

Status: **published as `ef4fc4e` on `codex/catalog-migration`; runtime code is not merged into main yet.** Read `../CATALOG_API.md` for exact public signatures, migration policy, preview leases, and Unity review steps. No Claude-owned HUD/UI files or `CollectionUiTests.cs` were changed.

- Catalog: 12 characters, nine finish families, 108 stable collectible definitions; four available Pip assets, 104 unavailable concepts. Read-only explicit box pools preserve current odds.
- Inventory: version-1 JSON migrates to ID rows while preserving counts, daily/sensor state and pending reveal. Version-2 future IDs round-trip; unsupported future schemas remain untouched/read-only. The original pre-migration JSON is retained.
- Profile: public `GuestProfile` exposes ID choices, keeps the existing enum surface, and preserves valid future IDs when saving other settings.
- Preview: bounded leased LRU adapter is available for UX-031. Existing HUD preview allocation is unchanged; no models/fur/shaders/new handling are claimed implemented.
- Checks: **1,224 new portable catalog/cache assertions**, all existing portable suites passing; source checks 49 files / three symbol profiles / 91 GUIDs. Standalone Unity migration tests authored but unrun.

**Claude integration requests:**

1. Before running your UI tests against this branch, back up/delete/restore **`pockle.profile.local.avatarId` and `pockle.profile.local.favoriteId`** alongside the three existing profile keys in `CollectionUiTests`. Otherwise test writes can affect the developer's saved profile. This fixture change stays in your lane.
2. Add `ShowToy(string id)` in the HUD when starting UX-031, using `CollectionSession.TryGetPlayableVariant` and checking its bool result. Keep the existing overload/events during migration.
3. Use `OwnedCounts` / `GetOwnedCount(id)` and catalog labels; acquire preview leases only for visible items, clear image textures before releasing leases, and keep counts/availability separate.
4. `Save.Counts` and the old pending index are compatibility projections; new code should read ID ownership and `PendingRevealId`. No expanded box pools or automatic gifts of planned collectibles.

Next Codex lane after integration: material behavior/model pipeline under UX-030 and ART-002–004, using the reviewed ART-001 concepts. Public profiles, billing and background walking remain separate pending integrations.

The runtime branch is ready for review at https://github.com/calonchozuluaga/pockle/compare/main...codex/catalog-migration. The GitHub API is unavailable in this cloud session, so no pull request was created automatically. These handoff/API documents are published on main so Claude can read them independently of the runtime merge.
