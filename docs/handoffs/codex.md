# Codex handoff

## Active October 10 — Android startup and rounded jelly Pip

Owner approved the follow-up after reviewing PR #6/#7: fix stripped primitive colliders before the next phone build, then round Pip's underside and improve the four finish materials. Branch: `codex/pip-jelly-device-pass`, isolated checkout stacked on the current v2 integration (`518d023` remotely; local equivalent `c321ba4`). Verified main is still `b8a119c`; shell fetch cannot reach its proxy, so GitHub supplies the remote baseline and latest Claude handoff (`25f838a`).

Claimed UX-025/030 and Pip art follow-up: `PrototypeStage`, `MysteryBox`, presentation meshes, JellyToy/shader, Pip generator/source/exports, separate Unity tests, geometry checks and validation notes. Claude's HUD/CI remain untouched. Newer Claude requests (CapsuleCollider errors and rounded bottoms) are acknowledged here; the integration's retained Claude note predates those additions. Preserve the small contact patch and face/grip anchors; tune Peach/Moon/Gold/Mint without changing tactile/reward/save contracts. Device appearance/performance and live Android startup must be verified separately from editor CI.

Delivered on this branch: owned collider-free cylinder/box geometry (only the plate's explicit MeshCollider remains), shared disk disposal and startup/picking tests; regenerated Pip .blend/FBX/runtime mesh with a curved underside and .129-radius contact patch; broader glaze/transmitted-edge shader terms, opaque Mint sheen, and a radial contact shadow. No new runtime AI/backend/rewards, ProjectSettings, motion budget or tactile physics changes. The bundled image-encoding module exception is documented below. No new halo shell or refraction. Existing texture/mesh reuse is preserved.

Local validation: **538,252 portable assertions**, including the new contact/face-anchor constraints; source checks **66 files / three profiles / five assemblies / 142 GUIDs / zero failures**, metadata/whitespace, Blender FBX round-trip (20 parts, 2,868 logical vertices, 5,732 triangles, max error .000000227). The collider-only hosted Unity run passed. Combined shader/render/Android CI is pending at publication; PlayMode captures write actual Unity PNGs into the existing `unity-test-results` artifact. Review/device/profile instructions and precise remaining limits are in [PIP_JELLY_REVIEW.md](../PIP_JELLY_REVIEW.md). Fusion/personality/check-in implementation remains outside this approved pass.

CI follow-up: corrected the capture test's namespace and enabled Unity's bundled `com.unity.modules.imageconversion` 1.0.0, required by `EncodeToPNG`. This necessary package-manifest exception is called out here and in the PR; engine, render pipeline, ProjectSettings and CI are unchanged. The test logs compact gzip/RGB review previews as well as full PNG artifacts, because the connector returns artifact ZIP references without a local extraction path. Earlier failed capture-compilation runs are not render evidence.

## Active October 10 — UX-032/033 integration for owner testing

The owner approved continuing with CI and UI integration after PR #9. Claimed branch: `codex/v2-collection-integration`, isolated checkout from main `b8a119c`, combining Claude v2 snapshot `d697e12`, newer collection notes `de285df`, and PR #9's runtime/backend contracts. This owner-directed integration includes HUD changes in Claude's usual lane; Claude's branch and published files remain untouched. Files: new HUD Collections partial/meta, existing HUD navigation/Home/Shelf hooks, runtime backdrop binding, new UI navigation tests, task/coordination/validation notes and this handoff. Preserve all prior handoff sections and current product decisions.

Scope: reachable collection list and ten-slot lineup, owned/available selection with Back to the same lineup, shared static portraits with honest planned/unowned states, and per-toy stage colour/reset. No new rewards, paid checkout, source dates, backend deployment, or all-120 live toy rendering. Phone testing follows the combined CI result. Integration details/validation will be recorded below.

Implemented: Home/Shelf Browse collections, sorted series cards with honest unscheduled states, one lineup per character, available-owned selection with direct Back to Collection, scroll restoration, no action/grant for planned content, and existing Boxes/Rewards routes. Lineup cells share the four shelf textures; no extra render targets/cameras are created. New `BackdropChanged(Color)` HUD event binds to the runtime's explicit viewer camera, even while it is disabled by menus. Entry, variant changes and return to browsing set/reset both stage cameras.

Verified locally: **549,003 portable assertions** across eleven suites; source checks **63 files / three profiles / five assembly definitions / 139 GUIDs / zero failures**, metadata synchronization, font blob hashes against GitHub, and whole-tree whitespace. Authored UI PlayMode case covers the twelve-card browser, ten Pip slots, four available beta toys, planned content refusal, shared textures, disabled-camera backdrop, lineup/list scroll restoration and tab reset. PR #9's hosted Unity/portable CI passed. The combined integration still needs its own CI and phone review; do not reuse #9's result as proof of the new HUD code.

Read [V2_PHONE_TEST.md](../V2_PHONE_TEST.md) for the owner's routes. This is a static lineup integration; live eager/idle rendering, new jelly shader tuning, source-date save migration and online acquisition remain follow-ups. The backend plan is retained for review. Newer COLLECTIONS/FUSION/PERSONALITIES notes and Claude's UX-032 handoff are preserved, including open decisions.

## October 10 — Claude v2 runtime requests and backend design

Owner asked to begin the newer October 9 handoff. Read GitHub's `claude/ui-v2` at `d697e12` and `claude/collections-design`, including its newer reward/backend notes. Claimed branch: **`codex/collection-backend-contracts`**, separate worktree from verified GitHub main `b8a119c`. Git fetch failed because the executor proxy is unreachable; the connector verified the remote baseline and current handoffs.

Task scope: **UX-033 runtime navigation foundation**, **UX-029 backend/copy contract follow-up**, and the v2 stage colour field. Files: Core `MenuNavigation`, `PrototypeStage`, portable `MenuChecks`, a separate `StageBackdropTests` fixture/meta, `docs/COLLECTION_RUNTIME_API.md`, `docs/BACKEND_PLAN.md`, architecture/validation/coordination/task notes and this handoff. No HUD partials, UI fixtures, CI, shaders, package manifest or project settings changed.

Delivered: appended `AppPage.Collections` / `Collection` without renumbering existing pages; Back/scroll and invalid-destination coverage; `PrototypeStage.SetBackdrop(Color)` plus explicit-camera overload, changing both viewer and full-screen clear cameras without touching portraits or retaining static scene references. Exact integration details are in [COLLECTION_RUNTIME_API.md](../COLLECTION_RUNTIME_API.md).

**Claude actions:** add the two page roots, keep the selected collection ID in the HUD, and let `ShowToy` navigate directly from Collection to Play instead of inserting Shelf. Pass the v2 field colour through the new backdrop API on toy entry/selection. These screens/backdrop calls are not wired by this branch because they are your HUD lane. Existing series metadata/motion/product fields from PR #7 already address the catalog request; this pass does not duplicate or activate them.

Backend design is ready for review in [BACKEND_PLAN.md](../BACKEND_PLAN.md): concrete table keys/constraints/indexes, anonymous-account linking and legacy migration, per-copy serial/discovery/source, private server APIs, exact per-player quote odds, atomic finite allocation, receipt deduplication/reconciliation, spare reservations and two-leg trades, five-spare regular crafting, append-only private ledger with consent-based public projection, and concurrency acceptance cases. This is a design, not SQL deployed to a project.

I accept the newer brief's adopted crafting/no-secret rules and check-in requirement. Check-in cadence/pool/odds and whether walkers receive both boxes remain unconfigured. Actual edition sizes/weights, late-payment handling, legacy import policy and server-day boundary need settled configuration before production. Fusion and personalities remain proposals. Material improvements are a separate follow-up requiring in-engine/device visual evidence; none are claimed here.

Validation is recorded in [VALIDATION.md](../VALIDATION.md). Portable and Unity checks are distinct: the Unity backdrop fixture is authored but unrun locally. Backend SQL/RLS/concurrency, Android performance and visual colour-space correctness remain unverified. Do not merge before required CI is green and runtime/UI integration is reviewed.

## Active UX-029/030 follow-up — series metadata and lineup motion

Owner relayed Claude's request for release/retire dates, series number, secret flags/odds, and character idle motion. Latest Claude handoff read: `claude/collections-design` at `8b49ec8`; I accept the additive collection API and visible-only animation/static shelf split. The approved rule is no retirement, so the optional retire date stays unset. Approved secret target is 1 in 72 for future series; the legacy four-Pip beta pools do not acquire an invented secret. Latest owner clarification overrides the earlier finish-theme grouping: one collection is one character with at least ten color/texture/material varieties; Pip belongs to Series 1. No release dates, other characters' series numbers, or full-collection prize weights have been approved.

Claimed branch: `codex/series-metadata-idles`, from main `b8a119c`. Files: Core catalog/collection metadata and character motion, a runtime lineup-only animator, portable checks, separate Unity animation tests, `docs/CATALOG_API.md`, validation, and this handoff. Claude retains all HUD/UI/CI files. Each character gets idle/eager recipes; the animator is opt-in for visible lineup toys, separate from static shelf portraits and tactile play. Published collection metadata exposes ordered members, product/price tier, accent colours, normalized odds, optional secret ID, and nullable UTC date fields. Pip has series 1 with an unknown release date; other planned collections have series 0 until assigned.

Scope clarification for Claude: walk-tier targets/multipliers and crafting costs are unapproved proposals and remain unset. First-found/source save migration is a separate follow-up; existing dates must remain unknown instead of stamped with today's date. No changes to saves, reward eligibility, ownership, or the existing daily pool in this pass.

Implementation ready on this branch: `ToyCatalog.Collections` / `TryGetCollection`, immutable `CollectionDefinition` / `CollectionOdds`, `BoxOffer.Collection`, collectible secret marker, validated immutable draw pools with overflow-safe totals; twelve idle/eager `CharacterMotion` profiles; `ToyLineupAnimator` with one shared `LineupMotionBudget` (default 4, max 6). **Read `../SERIES_AND_MOTION_API.md` for exact field names, calling conventions, schedule-null handling, and lifecycle guidance before building UI on them.** The shelf's one-shot portraits are unchanged. No live camera renderer, v2 UI, or new reward availability is claimed.

Owner correction implemented: twelve character collections × ten planned varieties = 120 stable IDs, including an unavailable Glow Jelly recipe. Pip's ten members belong to Series 1. Legacy Jelly Garden/Midnight Glow/Gold Confetti are sub-pools/themes of **Pip**, not separate character collections. `BoxOffer.CollectionId` now maps all three to `pip`; new `PoolId` retains each old draw ID. Each collection validates single-character membership; approved odds must cover every member. Full-collection odds are null/unapproved until weights/secret art are chosen. Old daily odds, prices, saves, and four playable assets remain intact. This correction is newer than Claude's current COLLECTIONS.md; please update the v2 collection grouping and packaging plan accordingly. The first three Pip packaging concepts should become variants/themes under one Pip collection. New collection boxes must feature different characters.

Verified: portable suites and source checks across 61 files/3 profiles/5 assembly definitions and 137 GUIDs; exact final assertion totals in VALIDATION.md. Authored but unrun: four Unity PlayMode cases (offer mapping plus animation lifecycle). Not verified: Unity compilation, lifecycle/rendering, animation feel, Android builds/device profiling, hosted CI. Dates and real secret identities remain unknown; future 1-in-72 target is verified with fixture definitions only. Please fetch this branch before UX-031/v2 lineup integration and use still portrait leases for shelf cells.

## ART-005 packaging concept review — owner request

The owner asks for collection box designs featuring the actual Pockle character while preserving the mystery, before replacing store screenshot cutouts with box meshes. Claimed branch: `codex/box-collection-concepts`; files: new packaging concept image/notes and this handoff only. This is an explicit owner request for a packaging concept pass in Claude's ART-005 lane; no concurrent edits to Claude's existing design assets or HUD files. Six proposed fronts: Jelly Garden, Midnight Glow, Gold Confetti, Cozy Club, Little Forest, Playroom Vinyl. New collection names are proposals, not runtime offers or changed prize pools.

Direction: a shared closed carton with a featured toy portrait, collection title, squishy Pockle wordmark, and side-panel mystery variation cues. Hero artwork shows an example character/finish, not the identity of the randomly awarded contents. Front faces keep the character's eyes/smile visible; mystery marks stay beside the artwork or on side panels. Runtime box mesh/UV/material work follows the owner's visual review. No checkout, rewards, inventory, or Unity asset changes in this concept pass.

Concept board ready: `docs/packaging/pockle-collection-box-concepts-03.png`, with direction notes and the proposed mesh/UV pass in `docs/packaging/collection-box-directions-03.md`. This is a generated design preview using the existing character/wordmark references, not a Unity render or usable atlas. Visually reviewed six sealed cartons, character portraits, collection labels, and side mystery cues. Documentation whitespace checked; code/Unity tests not rerun because this pass changes only concept art/notes. Please coordinate any store HUD replacement in Claude's lane after owner review.

## UX-030 correction — editor import compilation

Owner reported Unity CS0234 at `PipCharacterImporter.cs:63` after the integration. Claimed branch: `codex/editor-core-reference`; files: `Pockle.Editor.asmdef`, portable source checks, validation notes, and this handoff. The importer directly uses `Pockle.Core.ToyCatalog` but the editor assembly lacks a direct Core reference. Add the missing reference and make the source checker detect project namespace uses without a declared assembly dependency. No Claude UI/CI files, package manifest, or project settings changes.

Implemented the direct `Pockle.Core` reference. The expanded source checker reproduced the defect before the fix (exit 1, one missing Editor → Core dependency), then passed after the fix (56 sources, three profiles, five assembly definitions, 132 GUIDs, zero failures). All ten portable suites passed again (417,703 assertions). The checker examines project namespace uses against unambiguous asmdef root namespaces and direct name/GUID references; it does not replace Unity API compilation. Unity editor compilation/reimport and Android rebuilding remain unverified in cloud. The screenshot's UnityConnect authentication errors and ADB daemon messages are separate local service issues; no speculative package/settings changes were made for those.

October 9, 2026. Status: **Integrated on `main`: Claude's visual system, Home/shelf UI, catalog ID integration, ART-001 concept sheets, and CI; Codex's Moss, Bop, and Nook draft models/materials.** Unity/device validation and broader UX-031 catalog browsing remain pending. See the integration record below for exact branch tips and validation limits.

## October 9 integration — owner's requested merge

Merged sequentially without conflicts: `codex/material-toys` (`3f67133`), `claude/home-shelf` (`f4b80bb`, including `claude/ui-polish` at `f9cca47`), `claude/roster-identity` (`5eeb131`), and `claude/agents-and-ci` (`dc2f3ee`). The UI build label is **Home and shelf 01**. No `ProjectSettings/` or `Packages/manifest.json` changes were included.

Claude's HUD ID overload, ID ownership reads, profile ID fixture backups, and title-case shelf captions are integrated. Four Pip finishes remain playable; Moss/Bop/Nook are editor-only draft studies under **Pockle → Character studies**, with no inventory grants or reward-pool expansion. ART-001 concepts are included for review. I acknowledge Claude's Moss/Bop silhouette feedback and will use it for the next art pass; those shapes are not revised in this merge. Nook should also be reviewed against the published face placement and scale guidance.

Executed on the combined tree: all ten portable suites (417,703 assertions total), C# syntax across 56 files in default/Editor/Android profiles, 132 asset GUID checks, and manifold/UV/anchor/budget checks for all three study meshes. The CI whole-tree whitespace check exposed trailing spaces in two font-license files; trimmed only those spaces, preserving the license prose. See `../VALIDATION.md` for the integration evidence.

GitHub's check-runs API returned **Forbidden** with the available authentication, so hosted CI status could not be read. Local portable checks passed; this is not a claim that GitHub CI, Unity compilation/shaders, Unity PlayMode/EditMode tests, or Android packaging/device checks passed. The newly merged workflow will run on the main push; Unity jobs depend on repository license secrets. No licensed Unity editor is available in this cloud workspace.

Next coordination: Claude retains the UI/CI lane and UX-031 browsing; Codex retains runtime/art and will align the provisional shapes with ART-001 after owner review. Both agents should fetch this integrated main before starting another branch.

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

## Moss/Bop study implementation ready for review

Implementation commit: **`182f31b`**. Publication notice is on main at **`6d8c0f4`**; this branch incorporates that handoff commit and retains the complete implementation note below.

Branch: **`codex/material-toys`**. Read [CHARACTER_STUDIES.md](../CHARACTER_STUDIES.md) for asset links, exact interfaces, local review, and authoring commands. Added two joined Blender models with FBX/native mesh exports: Moss / Velvet Flock (2,840 vertices, 5,144 triangles) and Bop / Gloss Vinyl (3,068 vertices, 5,516 triangles). Blender preview PNGs are under `docs/concepts/roster-studies/`, separate from Claude's ART-001 concept directory.

- Runtime: opaque single-pass flock/vinyl shader, compliance/recovery profiles, firm whole-body rocking with plate clearance, explicit ID loading, native resource cleanup on character switches, and an ID portrait adapter. Four existing Pip presets retain their prior feel.
- Draft access: **Pockle → Character studies** in the editor while playing. No new Android shelf entries, grants, or changed box pools. Both draft IDs remain catalog-unavailable until art and acquisition review.
- Claude API: `JellyToy.TrySetCollectible(string)` is visual selection only. Use `TactilePrototype.TryPlayCollectible(string)` for owned, available play after viewer navigation. Keep the legacy Pip overload/events. `.Variant` is not a new-character identity. Bounded preview caching retains its availability gate.
- Checks: all existing portable suites plus **8,721 material assertions** passed. Both geometry validators passed. Blender FBX import round trips matched positions/topology/UVs, with eight exported parts per toy. Source validation: 53 C# files / three profiles / 106 GUIDs, zero failures.
- Added standalone `CharacterStudyTests` for switching, cleanup, missing art, picking, material selection, deformation, and reset. **Unrun:** Unity compilation, shader rendering, PlayMode suite, Android packaging/performance/touch/sensors. PNGs are Blender references, not Unity captures.

Requests for Claude: review these provisional shapes against your forthcoming ART-001 identity sheets; keep draft availability false during UX-031 until the owner reviews them; wire ID viewer selection through the owned runtime API when ready. No Claude-owned HUD, UI resources, or UI test files were edited. Nook plush/foam and remaining character/finish production still follow the representative review.

## Active work — Nook plush/foam representative

October 9, 2026. The owner explicitly asked to start Nook. Continuing **UX-030 / ART-002–003** on `codex/material-toys`: original rounded-square pillow body with tucked corner paws, **`nook.boucle-plush`** and **`nook.mochi-foam`** study finishes. Latest Claude handoff remains `a12c902`; no ART-001 character concepts are published yet. The shape follows the agreed draft roster and remains provisional.

Claimed changes: existing Blender build/check scripts, new `ArtSource/Nook`, FBX/native resources/previews, `CharacterArt`, `SolidToy.shader`, `JellyToy`, the editor-only study launcher, handling tests and study-specific Unity tests/docs. No HUD/UI files, inventory grants, box-pool changes, or catalog availability changes. Plush will show a looped fabric surface with stuffing-like deformation; foam will show a smooth matte finish and slower recovery. Claude retains all UI and ART-001 concept files.

## Nook implementation ready for review

Implementation commit: **`4a68f79`**. The publication notice is on main at **`170d4f5`**; this branch incorporates that handoff commit and keeps the detailed implementation notes below.

Nook's original rounded-square cushion and tucked corner paws are built: **2,564 exported vertices / 4,528 triangles**, one shared native mesh, Blender source and FBX. Two explicit study IDs: **`nook.boucle-plush`** (oat looped fabric) and **`nook.mochi-foam`** (smooth matte lilac). Blender previews are `docs/concepts/roster-studies/nook-study-01.png` and `nook-foam-study-01.png`; Unity appearance remains unverified. Nook's Blender source retains both recipes.

- Both studies are selectable through **Pockle → Character studies** while playing. `CharacterArt.NookPlushStudyId` / `.NookFoamStudyId` and `IsStudy(string)` extend the explicit art registry. Production ownership/availability and the four existing Pip presets are unchanged.
- The opaque shader now has separate bouclé and foam properties, alongside flock and vinyl. Recipes reset every finish flag and restore the base palette when switching back from lilac foam.
- Same-character finish selection reuses the body mesh, face, material, and root. The raw pose is replayed with the new profile. The authoring launcher also avoids rebuilding Pip when already on the play page.
- Plush has stuffing-like give/sag; foam compresses more deeply, stretches less, sags less, and recovers more slowly. Portable checks confirm the recovery difference across frame rates. Phone feel still needs review.
- Executed: all portable suites, **8,738 total material assertions**; all three study geometry checks; Blender FBX round trips including Nook's eight parts with maximum position error **0.000000260 m**; source checks **53 files / three symbol profiles / 110 GUIDs / zero failures**.
- New Unity coverage checks Nook finish reuse, recipe restoration, pose compliance, reset, and missing-finish rejection, plus cross-character cleanup. **Unrun:** Unity compilation/shaders/PlayMode, Android packaging, actual touch/sensors/performance.

Please review Nook alongside Moss/Bop when completing ART-001 and preserve all four study IDs as catalog-unavailable until designs and acquisition are reviewed. No Claude-owned HUD/UI files or UI fixtures were changed. Exact interfaces and local review instructions are in [CHARACTER_STUDIES.md](../CHARACTER_STUDIES.md).
