# Claude handoff

October 9, 2026. Status: **UX-002/003/004 and ART-001 merged. The owner tested the UI and asked for a less dated look: "Pockle screens v2" (owner's design canvas) is the approved direction. Collection/series rules decided; see `../COLLECTIONS.md`. Next: rebuild the Unity screens to v2, then UX-031.**







## October 10: merged to main (owner request)

The owner asked to merge everything so they can test in one pass. Merged in order:
- #10: Codex's v2 collection integration, which includes #7 and #9.
- #11: Android collider fix and Pip concept pass `745bd17`.
- #12: Shelf tab, wordmark and box proportions, and the Pip render-vs-concept note.

All three were green before merging: Unity 32/32, 34/34 and 34/34, with Android packaging passing on #10 and #11. #6 and #8 were closed as superseded. Their only unmerged notes (rounded bottoms, CapsuleCollider) were done by Codex in #11.

**Request for Codex: Shelf tab.** The bottom bar now has four tabs: Home, Shelf, Boxes, You. `MenuNavigation.SelectTab` only accepts Home, Boxes, and Profile, so the HUD selects Home and then pushes Shelf. That gives the same history a tab would, and Back from Shelf returns Home. When convenient, please let `SelectTab` accept `AppPage.Shelf` (with a portable check). The HUD will then call it directly.

**Pip concept pass review (Claude, on `745bd17` captures).** It's much closer: the shape, crown bubbles, face placement, gradient, and upper-right highlight all follow the note. Still open, in order of impact:
1. **Pearls read as surface bumps.** They're body-coloured spots on the skin, and dents on Moon. They should be milky white, glossy, and clearly inside the jelly.
2. **The material reads as opaque.** The top goes nearly cream-white. The concept stays peach throughout and is *more* saturated at the thin edges, not paler.
3. **The crown bubbles are almost white.** They should be the same translucent peach, with their own highlights.
4. **The lower corners are boxy.** The sides drop straight down to a bread-loaf base. The concept's lower corners are rounder and fuller.
5. **The crown stretches into bunny ears** in the grip pose.

The owner's phone test comes next. Please wait for that feedback before the next art pass.
## Pip render vs. concept, side by side (owner review of #11, October 10)

The owner compared the front concept (`pip-character-sheet-01.png`, FRONT) with the Unity capture of Peach Jelly from #11, and it still doesn't match. Most of the gap is **silhouette and proportion**, plus a few material details. In order of impact:

1. **The body is the wrong way round.** The concept is a bottom-heavy gumdrop or pear: narrow shoulders, widest about two-thirds of the way down, then a broad, soft base that curves under. The Unity body is close to a circle or egg, widest at mid-height and narrowing toward the base. Please:
   - pull the shoulders in (roughly 75–80% of the base width);
   - move the widest point down to about 65–70% of the height;
   - keep the new rounded underside.
2. **The crown bumps should be two separate bubbles.** In the concept they read as two distinct glossy spheres set on top of the head, each with its own highlight:
   - **left:** smaller and lower;
   - **right:** larger and taller;
   - **both:** sitting off-centre toward the right.

   In Unity they're two small ear-like nubs, close together, centred, and blended into the head.
3. **Face placement.** The concept's eyes sit lower (just above mid-height) and further apart, as larger glossy ovals with a small catch-light. The smile is small and centred between them, and the blush is large, soft, and low on the cheeks. In Unity the eyes sit higher and closer together, and the blush is small. Please re-anchor the face after the body change.
4. **The colour should change from top to bottom.** The concept is pale, almost milky peach at the top and deepens to coral and orange in the lower third, as if the filling has settled. Unity is one saturated orange all over. A vertical tint gradient, or a thickness term that deepens toward the base, gives most of this.
5. **Highlights should be soft, not streaks.** The concept has one broad, soft oval highlight at upper right and small soft reflections on the bubbles. Unity has a long, hard white vertical streak at upper left, which reads as hard plastic. Please widen and soften the specular, and move the key light to upper right.
6. **Drop the bright outline.** Unity shows a thin yellow-white halo around the whole outline. It reads as an outline or aliasing, not light passing through. Please tone down the transmitted-edge or rim term so the edge is only slightly lighter and more saturated than the body, never brighter than the highlight.
7. **Make the filling read as inside the jelly.** The concept has a few large milky-white pearls and small gold flecks low in the body, softened by the jelly in front of them. Unity's filling is barely visible. A few larger pearls, partly hidden by depth, would help.
8. **Ground contact.** In the concept, Pip sits right on the surface with a soft, wide shadow. That's fine to keep on the plate, but the contact shadow should be wider and softer, to match the new base.

Please re-capture the same front view at the same framing and lighting as the concept for the next comparison.
## Target jelly look for the toys (owner feedback, October 9)

The owner says the in-app toys read as **smooth blobs** next to the concept renders (for example `../concepts/pip-peach-jelly-concept-01.png`, which the v2 preview screens use). The concepts look like jelly you could squish: lit from within, glowing, with things floating inside.

**This looks like a material and lighting gap, not a modeling one.** The silhouettes are probably fine. The concepts come from offline rendering, where light travels through the material. A phone can't do that, but most of the look can be faked in real time.

`JellyCandy.shader` already has a lot of this:
- gloss and rim terms
- studio cubemap reflections
- pearl sheen and glitter flakes
- suspended pearls drawn under the shell

So the work is mostly about tuning and adding what's missing, not starting over. Materials and 3D are your lane. Here's the target, grouped by how achievable it is on a phone.

**Very achievable (do these first):**
- **Glassy sheen:** a soft, broad highlight that rolls over the shell as the toy turns and squishes. The concept's highlights are larger and softer than ours.
- **Lit-through edges:** a stronger translucent rim, so the edges look lighter and more saturated than the centre, as if light passes through the thin parts. Tie it to the view angle (fresnel), not a flat edge colour.
- **Soft glow:** the concepts have a gentle halo. Built-in RP has no bloom without a post-processing package, and `Packages/manifest.json` changes need the owner's approval. A cheap alternative is a slightly larger additive back shell or a soft glow sprite behind the toy.
- **Keep the squish:** the existing deformation is the best part. Nothing here should cost it frame rate.

**Medium effort:**
- **Lit-from-within:** a cheap subsurface approximation, not true SSS. Wrapped diffuse lighting, a thickness term (thicker in the middle, thinner at the edges, so a baked thickness map or vertex value), and a back-light transmission term, so the toy glows when lit from behind. This is the stylized "glowing gummy" look most mobile games use.
- **Floating bits that read as inside:** the pearls exist but should feel suspended in depth.
  - Fade and tint them by depth inside the shell.
  - Let them drift slightly, with a parallax offset as the toy rotates.
  - Use an inner layer or interior sprites rather than more real geometry.
  - Let them shift a little when the toy is squished (already in the interaction plans).

**Hard; probably skip on phones:**
- **True refraction:** bending the background through the toy needs a grab pass or an opaque-texture copy every frame. It's expensive, and it matters least for this cute, glowing look.

**Ask:** please put the four Pip finishes in this material first:
- Peach Jelly
- Moon Jelly
- Gold Glitter
- Mint Soft, which is opaque and soft, so it mostly needs the sheen and a softer inner glow

Then share an in-engine render or screenshot of each, next to the concept, so the owner can compare. Please also note the cost on a midrange Android phone. The stage lighting and background (`PrototypeStage`, also your lane) matter a lot to this look. A light from behind or the side that the transmission term can catch will help.
## Fusion (proposal, October 9)

The owner wants two spares to fuse into a unique toy you can't buy. Proposal: `../FUSION.md`.
- Results come from rules that blend shape, colour, material, and personality, with a few secret recipes on top.
- Fusion uses up both spares.
- Fused toys can't be traded or bought and have no serials or finite supply.
- Each spare can be used once: traded, crafted, or fused.

**For Codex:** the rule-based art (mesh, material, and colour blending), the fusion runtime, save and ledger changes, and server validation are your lane, and more technical than anything scoped so far. **Please don't start until the owner approves the approach.** Once approved, a short spike would help: blend two materials, and attach one parent's detail to the other's body. Open owner questions are listed in the doc.
## Personalities and a living shelf (proposal, October 9)

The owner wants Pockles to have personalities and react to each other on the shelf, so there's something to do after the daily box. Proposal: `../PERSONALITIES.md`. Key points:
- Everything is authored and shipped. No runtime AI, per the no-AI-calls rule, cost, offline play, and child safety.
- Personality lives on the character, and the variety tints it.
- Alive but never needy: no decay or guilt mechanics.

It awaits the owner's review.

**For Codex, once the owner approves:** the shelf runtime is your lane. Proposed interface, to accept or amend in `codex.md`:
- engine-free personality profiles on characters in the catalog (temperament, reaction IDs, per-variety tint values, pairing rules);
- a runtime that plays a reaction by ID on a lineup toy, reusing `CharacterMotion` and `LineupMotionBudget`;
- the HUD asks for reactions by ID and never drives motion directly.

Please don't build it until the owner settles the open questions.
## Daily check-in box (owner decision, October 9)

The owner added a free **daily check-in box** that doesn't require walking. It is the accessibility floor, so players who can't walk, won't walk, or won't pay still get a toy and don't leave on day one. It also covers tablets and phones without a step sensor. Walking stays the better path, with the existing tiers. Details and the three tuning options for the owner (older series, every two days, lower rare/secret odds) are in `../COLLECTIONS.md` under "Daily check-in box".

**Request for Codex:** please plan the check-in box in the reward logic, alongside the walking box:
- claim timing and the pool it draws from;
- exactly one claim per period, persisted before the reveal;
- the same finite-supply and ledger rules as other boxes, with source "check-in".

Hold the tuning values until the owner chooses. I'll add the check-in card to Home and Boxes once the reward API exists.
## UX-032 v2 screens — `claude/ui-v2` (stacked on PR #6)

Claimed: UX-032. Files: `PrototypeHud*.cs`, `Runtime/UI/PockleTheme.cs`, `Resources/UI/Fonts/` (Bricolage Grotesque replaces Fredoka/Nunito; Fredoka moves to `ArtSource/Fonts/` for the wordmark script), `CollectionUiTests.cs`, `docs/VISUAL_SYSTEM.md`, `docs/HOME_UI.md`.

**One additive change in Codex's lane:** `ToyPortrait.Render(string id, Color background)`, so portraits render on their toy's colour field. The existing overloads are unchanged.

**HUD contracts:** public methods, events, and `ShowToy` behavior are unchanged. Some captions changed with the design, and the PlayMode tests were updated to match:

- Home's "Collection" tile became "See all"; Home's greeting is "Morning/Afternoon/Evening, name".
- Buy buttons show the price and are named `Buy · <offer>`.
- Settings opens from the gear on You (not from toy play), with switches named `Sound switch` and `Vibration switch`.
- The favorite is set with the heart while playing a toy.
- Friends' second segment is "Your friends".

**Requests for Codex (unchanged from the collections note):** `AppPage.Collections` / `AppPage.Collection` in `MenuNavigation`, and `PrototypeStage.SetBackdrop(Color)` so toy play can sit on the toy's colour field like the canvas.

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

## October 9 — Supabase chosen; v2 UI build starting

The owner chose **Supabase** and approved the v2 UI direction. I'm starting **UX-032** on `claude/ui-v2`: restyling every existing HUD page to v2 (Bricolage Grotesque, plum tab bar, colour-field toy tiles, flat surfaces) and regrouping the shelf by collection with Everything / Missing / Duplicates views. Files: `PrototypeHud*.cs`, `Runtime/UI/`, `Resources/UI/`, `CollectionUiTests.cs`, UI docs.

**Requests for Codex:**

1. **Backend plan (your lane).** Please draft the Supabase schema and server functions for the rules in `../COLLECTIONS.md`: accounts and guest migration, server-owned inventory with per-copy serials, finite supply with atomic allocation, never-duplicate paid boxes with per-player odds, walk tiers, crafting, the append-only ledger, and trades (friend swaps and the have/want market). I'll review it and build the screens on top.
2. **Two new pages in `MenuNavigation`:** `AppPage.Collections` (all series) and `AppPage.Collection` (one series' lineup), with Back behaving like Shelf → Play. `MenuNavigation` is yours; I'll add the HUD pages once the enum values exist.
3. **Stage backdrop per toy.** v2 shows each toy on its own colour field. Could `PrototypeStage` expose a way to set the viewer's background colour (for example `SetBackdrop(Color)`) so the HUD can pass the toy's field colour on entering play?
4. **Catalog fields** from `../COLLECTIONS.md` (series number, release date, members, secret, odds, accent colours) when you get to them.
