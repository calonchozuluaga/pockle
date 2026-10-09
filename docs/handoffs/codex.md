# Codex handoff

October 9, 2026. Status: **UX-029 is merged. Moss/Bop/Nook draft models and handling are published separately on `codex/material-toys`; Nook implementation is `4a68f79`.** Claude HUD integration and Unity/device review remain pending.

- Prior Home hub baseline: `6809f9a` — Build home hub, local profile, and full game settings.
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

Status: **runtime implementation `ef4fc4e` merged into `main` via fast-forward of `44a564a`, at the owner’s request.** Read `../CATALOG_API.md` for exact public signatures, migration policy, preview leases, and Unity review steps. No Claude-owned HUD/UI files or `CollectionUiTests.cs` were changed.

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

The runtime code and API documents are now on main. Claude can fetch main and integrate the ID overload, inventory lookups, preview leases, and profile test-fixture key backups listed above. This merge adds catalog/save infrastructure; the app still has four playable Pip finishes. No Unity/device checks are claimed by merging.

## Active work — UX-030 / ART-002–004 representative toy studies

October 9, 2026. The owner asked Codex to continue building the other toys. Claimed branch: **`codex/material-toys`**. First batch: **Moss / Velvet Flock** and **Bop / Gloss Vinyl**, distinct shapes and handling alongside Pip. Latest Claude branch read: `a12c902`; no ART-001 roster concepts have been published there yet. These are provisional shape studies from `CHARACTER_ROSTER.md`, awaiting concept and device review.

Claimed files: Blender build/check scripts and new `ArtSource/Moss`, `ArtSource/Bop`, character resources/FBX/study renders; `PipCharacterAsset`, importer, `JellyToy`, runtime portrait adapter, handling helpers/tests, and a separate editor study launcher. Any runtime viewer integration stays in `TactilePrototype*.cs`. No Claude-owned HUD/UI files or UI tests will be edited.

Please preserve `ShowToy(PipVariant)` during UI migration. This pass will expose an ID-based runtime selection API and portraits for new character studies; exact signatures and whether studies are published as catalog-available will be documented before handoff. New art will not expand reward pools, change saved ownership, or grant paid products. Claude retains ART-001 concepts and all shelf presentation.

## Moss/Bop studies published — `182f31b`

Implementation and assets are on **`codex/material-toys`**, awaiting art/Unity review; they are not merged into the main app. Read [study instructions and exact runtime interfaces](https://github.com/calonchozuluaga/pockle/blob/codex/material-toys/docs/CHARACTER_STUDIES.md), or fetch the branch and inspect its own handoff.

- Original draft models: **Moss / Velvet Flock** (2,840 vertices / 5,144 triangles) and **Bop / Gloss Vinyl** (3,068 / 5,516). `.blend`, FBX, `.pocklemesh`, and Blender studio renders are committed. Preview PNGs are in `docs/concepts/roster-studies/` on that branch, separate from your ART-001 concepts.
- Runtime: single-pass opaque flock/vinyl shader, compliance/recovery profiles, firm rocking with plate clearance, ID art loading, native cleanup, and ID portrait rendering. The current four Pip presets keep their prior feel.
- Local study review: while playing the Pockle scene, use **Pockle → Character studies** to select Moss/Bop. Editor access does not grant inventory. Both study IDs remain catalog-unavailable, with unchanged reward pools; ordinary Android shelf selection is pending your ID UI integration and art/acquisition review.
- Exact APIs: `JellyToy.TrySetCollectible(string id)` selects visual art only; `TactilePrototype.TryPlayCollectible(string id)` requires owned, catalog-available play after navigation. Keep the legacy enum overload/events. `.Variant` remains a Pip compatibility bridge, not new-character identity. Preview leases retain their availability gate.
- Validation: existing portable suites and **8,721 new material assertions passed**, both geometry checks and Blender FBX round trips passed, source checks **53 C# files / three profiles / 106 GUIDs / zero failures**. New `CharacterStudyTests` are authored but unrun. Unity compilation, shader rendering, PlayMode, Android packaging/performance and touch remain unverified.

Please review these provisional silhouettes against your ART-001 sheets and retain draft availability until the owner reviews them. No Claude-owned HUD/UI files or UI test fixtures were edited. Nook plush/foam and the rest of the twelve-character/nine-finish production still follow the representative review.

## Nook published — `4a68f79`

The owner asked to start Nook. Its representative model and both finishes are now on **`codex/material-toys`**, alongside Moss and Bop. Read the branch's [updated study guide](https://github.com/calonchozuluaga/pockle/blob/codex/material-toys/docs/CHARACTER_STUDIES.md) for exact APIs, asset links, and review steps.

- **Nook**: original puffy rounded-square cushion with four tucked corner paws, **2,564 vertices / 4,528 triangles**; Blender source, FBX, and native readable mesh with packed UVs. Two finish IDs share the same model: **`nook.boucle-plush`** (oat looped fabric approximation) and **`nook.mochi-foam`** (smooth matte lilac).
- Nook is selectable under **Pockle → Character studies** in editor Play mode. Studio preview PNGs are `docs/concepts/roster-studies/nook-study-01.png` and `nook-foam-study-01.png` on that branch. The PNGs are Blender renders, not Unity screenshots.
- Plush hangs more softly; foam gives more deeply, stretches/sags less, and recovers more slowly. Switching Nook finishes reuses body/face/mesh/material assets and resets all recipe flags. The launcher avoids rebuilding Pip when already on the play page.
- The explicit art registry adds `NookPlushStudyId`, `NookFoamStudyId`, and `IsStudy(string)`. The owned play API, production availability gate, old enum contracts, saved inventory, and reward pools remain unchanged. Four draft study IDs are still catalog-unavailable and not on the ordinary Android shelf.
- Executed: all portable suites, **8,738 material assertions**, all three geometry validations, all three Blender FBX round trips, and source checks **53 files / three profiles / 110 GUIDs / zero failures**. Nook round-trip error was at most **0.000000260 m**.
- Authored but unrun: Nook Unity tests for finish reuse, recipe restoration, pose compliance/reset, missing-finish rejection, and cross-character cleanup. Unity compilation/shader rendering/PlayMode, Android packaging and device feel remain pending.

No Claude-owned HUD, UI assets, or UI test fixtures were edited. Please include these draft silhouettes/finishes in the ART-001 review before enabling availability or acquisition. Nook's representative implementation is ready; remaining character/finish production and all device acceptance still follow review.
