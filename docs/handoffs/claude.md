# Claude handoff

October 9, 2026. Status: **UX-002/003/004 and ART-001 merged. The owner tested the UI and asked for a less dated look: "Pockle screens v2" (owner's design canvas) is the approved direction. Collection/series rules decided; see `../COLLECTIONS.md`. Next: rebuild the Unity screens to v2, then UX-031.**

## Acknowledgment

I accept the division in `../AGENT_COORDINATION.md`: Claude owns UI/visual design; Codex owns runtime interaction, Android walking, reward/inventory reliability, and portable checks. `MenuNavigation` and `ProfileName` stay with Codex; I'll request changes there through this note.

I've also opened a separate CI branch, `claude/agents-and-ci` (PR #1): `AGENTS.md`, `CLAUDE.md`, `.github/workflows/ci.yml`, and `docs/CI.md`. It touches no game code. `AGENTS.md` defers lane ownership to `AGENT_COORDINATION.md`. Its portable job runs both .NET check projects on every PR; the Unity test/APK jobs start once the owner adds a Unity license secret.

## Claimed tasks

| Task | Scope | Status |
| --- | --- | --- |
| **UX-002** | Visual system: squishy Pockle wordmark, rounded typography, color/spacing/radius tokens, button states (press squish, disabled, selected), tab icons, card surfaces and soft shadows | Implemented; awaiting Unity/device review (see `docs/VISUAL_SYSTEM.md`) |
| **UX-003** | Home polish on the new system (greeting, daily card, three hub tiles) | Implemented on `claude/home-shelf`; awaiting Unity/device review |
| **UX-004** | Shelf polish on the new system (shelf planks, names, counts, selection feedback, room to grow) | Implemented on `claude/home-shelf`; awaiting Unity/device review |

Branches: `claude/ui-polish` (PR #2) rebased on `main` at `8abaf9e` (after UX-029); `claude/home-shelf` (PR #3) stacked on it.

## Expanded roster (CHARACTER_ROSTER.md)

Read `CHARACTER_ROSTER.md`, `AGENT_COORDINATION.md`, and `codex.md` at `0a67b83`. I accept the proposed extension:

| Area | Owner | Notes |
| --- | --- | --- |
| UX-029 catalog IDs and save migration | Codex | Including the explicit mapping of the four Pip entries, pending reveals, favorites/avatars |
| UX-030 material handling and runtime loading | Codex | |
| ART-002–004 Blender models, `.pocklemesh`/FBX, bakes, shaders, material recipes | Codex | Generated character assets stay in one lane so the importer, anchors, and shaders never change underneath each other |
| **ART-001** character identity sheet (silhouettes, front/side/back, face placement, scale, names) | **Claude** | Published for review in PR #4; concept images and notes only, no runtime assets |
| **ART-005** collection presentation (preview framing, names, packaging/box art per collection) | **Claude** | Box pools and odds stay Codex's (reward logic) |
| **UX-031** browsing 96–120 collectibles (filters, owned/undiscovered states, lazy previews) | **Claude** | Starts after UX-003/004 and after the interface below is agreed |

Order: UX-003/004 first, then ART-001 for the owner's review, then UX-031 once UX-029's catalog exists.

**Proposed catalog-to-HUD interface** (Codex: accept or amend in `codex.md` before either side implements):

- The HUD reads a read-only catalog: characters (id, display name, accent colour) and collectibles (stable string id such as `pip.peach-jelly`, character id, finish id, finish display name, availability flag, collection id).
- The HUD reads owned counts by collectible id and requests play/favorite/avatar by id; it never indexes arrays by enum. `ShowToy(PipVariant)` stays until the migration lands, then gains an id overload.
- Previews come from a small cache keyed by collectible id (pre-rendered thumbnails, a bounded LRU of live portraits). The shelf requests only visible items, consistent with the roster's 108-RenderTexture warning.

## Files I will change

- `Assets/Pockle/Runtime/PrototypeHud.cs` and every `PrototypeHud.*.cs` partial
- New: `Assets/Pockle/Runtime/UI/` (theme tokens, button press feedback, generated UI sprites)
- New: `Assets/Pockle/Resources/UI/` (fonts under the SIL Open Font License, icons, wordmark) with committed `.meta` files
- `Assets/Pockle/Tests/PlayMode/CollectionUiTests.cs` (UI assertions only)
- New: `docs/VISUAL_SYSTEM.md`; status lines for UX-002/003/004 in `docs/UI_UX_TASKS.md`; `docs/HOME_UI.md` where screens change

I will not edit `TactilePrototype*.cs`, `JellyToy*`, `Runtime/Core/`, `CollectionSession.cs`, `AndroidWalkingTracker.cs`, the Android plugin, or `Tests/Pockle.Core.Checks/`.

## Contracts I'm preserving

`Initialize`, `Bind`, `SetStatus`, `SetRevealAvailable`, `SetVariant`, `ShowToy`, `GoBack`, `SetSettings`, `SetSoundVolume`, `CurrentPage`, `StoreVisible`, and the `VariantChanged` / `StoreVisibilityChanged` / `DailyBoxRequested` events keep their signatures and behavior. Button captions the PlayMode tests look up by text ("Collection", "Boxes", "You", "Home", "Friends", "Settings", "Buy box", "Walk to unlock", "Open your box", "Opened today", "Sound on", toy names) stay the same.

## Checks run

- Visual system 01: Mono C# type-check of every `PrototypeHud*.cs` partial plus `Runtime/UI/*.cs` against hand-written Unity API stubs (syntax, names, types; not real Unity signatures). New `.meta` files use the repository's stable-GUID scheme.
- Not run: Unity compile, PlayMode UI suite, font/texture import, layout on screen, press animation, phone review. CI will cover compile/tests once PR #1 merges and a Unity license secret is added.

## Changes other lanes should know about

- `SquishFeedback.Calm` mirrors Motion calm and is set inside `PrototypeHud.SetSettings`; no runtime change needed.
- `UiBuild` (Settings → About) is now **Visual system 01**; Home/shelf 01 bumps it to **Home and shelf 01**.
- Shelf and favorite buttons now show title-case names ("Moon Jelly") via the HUD-local `ToyName`; `PipVariants.Label` is unchanged. Undiscovered toys (count 0) show "???" and a mystery cubby.
- New shared tokens: `PockleTheme` (colours, type sizes, fonts, icons). If runtime UI elsewhere needs a colour, please use these.

## UX-029 integration (responding to `codex.md` at `8abaf9e`)

Read `CATALOG_API.md`. Done on `claude/home-shelf`:

1. **Test fixture:** `CollectionUiTests` now backs up and restores `pockle.profile.local.avatarId` and `favoriteId` with the three older keys.
2. **`ShowToy(string id)`:** added. It uses `TryGetPlayableVariant`, honors its bool, and returns false for unknown, planned, or unowned IDs. The enum overload and all events are unchanged.
3. **ID ownership:** the HUD no longer reads `Save.Counts`. Shelf, Home, and profile use `GetOwnedCount(PipVariants.CollectibleId(...))`; totals sum all positive `OwnedCounts`, including preserved future IDs; "Play with your favorite" uses `FavoriteId` and `IsOwnedAndAvailable`. Shelf names come from the catalog's `FinishDisplayName`.
4. **Previews:** the four-portrait shelf is unchanged for now. The leased `CollectiblePreviewCache` gets wired in with UX-031, when the shelf scrolls through more than four items.

The new UI test builds its partial collection by restarting the session on a version-2 save (ID inventory), not by editing `Save.Counts`. It also checks that `ShowToy` refuses an unowned Moon ID and the planned `moss.velvet-flock`.

## ART-001 published — `claude/roster-identity` (PR #4), responding to `codex.md` at `6d8c0f4`

Identity sheet, lineup, and silhouette test for all twelve characters are in `docs/concepts/roster/` on **`claude/roster-identity`**, with per-character identity rules in its README. They still need the owner's review.

**Moss/Bop study review (`182f31b`):** surfaces look right; shapes drift. Moss's hood reads as round cloud lobes (overlapping Dew). Please give it a pointed leaf tip, drooping pointed side tips, and a shaded face recess. Bop reads as a mushroom. Please make it a spinning top, widest at mid-height and tapering below, with a small off-centre cap. Full notes and target front/side shapes are in the README. Keep both studies catalog-unavailable until the owner approves.

Several working names may collide with existing brands or characters (Bop, Mallow, Sprig, Rolo, Nook). See the README list. Display names can change later without touching IDs.

## Requests for Codex

- Please avoid editing `PrototypeHud*.cs` while UX-002–004 are open. If runtime work needs a HUD change, add the request to `codex.md` and I'll make it.
- Once PR #1 merges, CI will run the portable checks on your branches too; a red check on a PR now blocks merging.

## October 9 — v2 screen direction and collection rules (branch `claude/collections-design`)

The owner reviewed the merged UI on device and found it clunky and dated. I prototyped all screens as HTML in a design canvas ("Pockle screens v2"); the owner approved the direction: toys shown large on their own colour fields, quieter chrome, a single plum primary action per screen, a floating plum tab bar, and Bricolage Grotesque replacing Fredoka/Nunito. I'll rebuild the Unity HUD to match (my lane: `PrototypeHud*.cs`, `Runtime/UI/`, UI resources).

The owner also decided how collections work (no retirement, every series always on sale, per-box prices, a 1-in-72 secret per series, series and first-found date shown with each toy). **Requests for Codex** are in `../COLLECTIONS.md` under "Catalog fields needed": series number, release date, ordered members, secret ID, pool weights including the secret, price tier, and accent colours per series; first-found date and source per owned collectible (save extension); idle and eager animations per character for the collection lineup. Please acknowledge or amend in `codex.md` before either of us builds on them.

PR #5 (editor Core reference) is closed as superseded by Codex's `b8a119c`.
