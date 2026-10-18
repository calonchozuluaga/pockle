# Claude handoff

October 13, 2026. Status: **UX-037/038 (living shelf, personalities, fusion screens) in review on `claude/personalities-fusion-ui`.** Earlier status (October 9): **UX-002/003/004 and ART-001 merged. The owner tested the UI and asked for a less dated look: "Pockle screens v2" (owner's design canvas) is the approved direction. Collection/series rules decided; see `../COLLECTIONS.md`. Next: rebuild the Unity screens to v2, then UX-031.**


## October 12–13: living shelf, personalities, fusion screens — `claude/personalities-fusion-ui`

The owner approved the canvas row "Personalities and fusion" and asked to build it as shown. Claimed: **UX-037** (living shelf and personality page) and **UX-038** (fusion screens).

New files:
- `PrototypeHud.LivingShelf.cs`, `.Personality.cs` and `.Fusion.cs`
- `Runtime/UI/PersonalityBook.cs` (authored content), `PersonalityMotion.cs` and `UiShapes.cs`
- `Runtime/FusionService.cs`

**Additive changes in Codex's lane (please review):**
- `AppPage` gains `Personality`, `Fusion` and `FusionReveal`, appended so existing values keep their numbers.
- `ShelfPreviewRuntime.Register(..., Color? background)` and `SetPresentation(binding, fallback, background)`. The living-shelf toys use your live meshes on the scene colour instead of a colour tile, and they switch to night. The existing overload and behaviour are unchanged.

**Requests for Codex:**
1. **Fusion runtime:** implement `IFusionService`.
   - Consume two spares, never a last copy.
   - Grant a keep-forever, untradeable fused toy, persisted before the reveal.
   - Make the save change a versioned migration, record "used in fusion" in the ledger, and add server validation later.
   - Provide real fused art (blended material and colour, one parent's body plus a detail from the other) to replace my stand-in blend of the two portraits.
   - Call `hud.SetFusionService(...)`.
2. **Optional: personality runtime.** If you want per-variety motion in the live mesh (a show-off pose, a dreamy sway), `PersonalityBook` is engine-free and can move to Core. `PersonalityMotionStyle` is the contract.
3. **Shelf moments:** moments found live in the UI preference `pockle.shelf.moments.v1`. Tell me if you'd rather own them in the save.

PR #13 (an October 10 handoff record) is closed as superseded. The Shelf-tab request in it is done, and its Pip review predates the Meshy Peach work.

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

## October 13 — Walking is broken on the owner's phone (request for Codex)

**What the owner sees:** on the phone build, Boxes and Settings say "Step tracking is unavailable on this device." Steps never count.

**Why that message points at the plugin:** `AndroidWalkingTracker.ResumeIfAllowed` only shows it from its `catch` block. That means the permission check passed, but creating `studio.pockle.walking.StepCounter` (or calling `start()`) threw. A missing permission or a missing sensor shows a different message.

**Likely cause (not confirmed on device):** `Assets/Plugins/Android/PockleWalking.androidlib` is an old Eclipse-style library (`project.properties` with `android.library=true`, no `build.gradle`). As far as I know, Unity 6 only builds `.androidlib` folders that have a `build.gradle`. If so, `StepCounter.java` is never compiled into the APK, and the constructor throws `ClassNotFoundException`. VALIDATION.md only records compiling the Java in the cloud. Packaging and on-device step counting were never checked.

**Please:**
1. Confirm the cause: in Unity's Android Logcat, filter for `Pockle step counter`. The warning logs the exception type.
2. Fix the plugin so the class ships. Probably add a `build.gradle` (`com.android.library`, the `studio.pockle.walking` namespace, Java sources in `src`) and drop `project.properties`. Alternatively, move the Java into `Assets/Plugins/Android/` as loose source.
3. Verify on the owner's phone that the status reads "Keep Pockle running while you walk…" and the daily box progress moves. Record the result in VALIDATION.md.
4. Optional: put the exception type in `Status` in development builds, so the next failure is readable from a screenshot.

**My side (UI, coming with the Settings cleanup):** when tracking has failed, Boxes and Settings will hide "Enable walking", which does nothing in that state, and show a calmer line instead. To do that I need a way to tell "failed" apart from "not enabled yet". Could you expose something like `CollectionSession.WalkingFailed` (bool), or an enum state, rather than me matching status strings?

**Heads-up about this folder:** we're both working in the owner's local checkout (`claude/personalities-fusion-ui`). My uncommitted change here is `Assets/Pockle/Runtime/PrototypeHud.Settings.cs` (Settings cleanup, build label "v2 screens 03") plus this note. Please leave that file alone and don't commit it as part of your work. I'll commit it to PR #18 once the owner signs off.

## October 14 — Toy stage: worlds, lighting, collection (request for Codex; owner approved)

The owner approved the "Toy stage" row in the design canvas (boards "Toy stage" and "Stage: collection drawer"). The play screen gets a collection pill in the header, a series/number eyebrow above the finish name, a separate rarity badge, and a bottom tray with three tabs: **Collection** (the 10 finish dots, tap an owned finish to swap), **World**, and **Light**. I'm building all of the HUD side now in the owner's local project.

**Claimed (Claude), in this folder:** `PrototypeHud.cs`, `PrototypeHud.Play*.cs` (new), `PrototypeHud.Collections.cs`, `Runtime/UI/*`, and `CollectionUiTests.cs`. Your recent owner-approved HUD edits (selected collectible ID, Fox titles and backdrop) are kept as they are. From here, please send any HUD change through `codex.md` so we don't both edit these files at once.

**Requests for Codex (stage, your lane):**
1. A world API on the stage. Something like `PrototypeStage.SetWorld(StageWorld world)` with `StageWorld { Studio, Home, Garden, Midnight }`, where Home is the toy's own collection world. Each world sets:
   - the background (gradient or skybox),
   - simple low-cost scenery (Fox hollow: pine silhouettes, falling leaves; Garden: pink hills, gummy flowers; Midnight: stars and a moon),
   - and the plate: wood stump, milk dish, pink jelly dish, or a glowing lavender ring.
   Keep it phone-cheap: flat or unlit cards and particles, no real-time shadows beyond today's blob shadow.
2. A lighting API, something like `PrototypeStage.SetLight(StageLight light)` with `{ Day, Golden, Night }`: key light colour and angle, ambient, and shadow strength. At Night, glow finishes (`fox.glow-jelly`) should emit softly.
3. Collection worlds come from the catalog: a `HomeWorld` field per collection (Fox collection → Fox hollow). Other collections can fall back to Studio until they get art.
4. Remember the last world and light per player (a local preference is fine for beta).

The HUD will call these through events (`WorldChanged`, `LightChanged`) the way it already uses `BackdropChanged`. Until your API lands, the tray will only switch the backdrop colour. Reply in `codex.md` with the final names and I'll wire them up.

Also please push your local branches (`codex/fox-material-collection`, `codex/axolotl-plush`, `codex/walking-plugin-fix`, `codex/fox-owner-test`) to GitHub so the work isn't only in the owner's folder, and so CI can run on it.

**Update, Oct 14 (toy stage built, in the owner's local folder, uncommitted):**
- The world and light types now exist in `Runtime/UI/StageChoices.cs`: `StageWorld { Home, Studio, Garden, Midnight }` and `StageLight { Day, Golden, Night }`. Please use these rather than defining your own.
- The HUD raises `PrototypeHud.WorldChanged` and `LightChanged`, and keeps sending `BackdropChanged` with a fallback tint.
- New players start in Studio with Daylight, so today's look is unchanged until they choose otherwise.
- **Framing request:** please frame the toy and plate a little higher. The closed tray is 56 px tall at the bottom (12 px margin), and an open tray reaches about 200 px.
- **Your tests:** I changed one assertion each in `FoxOwnerGrantTests` and `FoxFinishesTests`. The play title is now the finish name ("Classic"); the collection moves to the header pill and the eyebrow.
- **Request:** a per-finish swatch colour in the catalog, so the collection dots can show each finish. For now they use the character's accent colour.
- Unity tests have **not** been run on these changes. The HUD type-checks with a real C# compiler against Unity stand-ins.

## October 14 (afternoon) — shelf smoothness and toy framing (owner request)

**Shelf (done, in the owner's local folder, uncommitted):**
- **Living shelf:** the four toys are now still portraits moved only by `PersonalityMotion`. The live 3D actor ran at 15 fps and fought the full-rate 2D sway, which is what the owner saw as jitter and skipping.
- **Feet on the plank:** each portrait is read back once to find its lowest painted row. The slot is placed so the toy's feet rest on the top edge of its plank. A fixed 14% base had left some models sunk into the shelf.
- **Silhouettes:** undiscovered outlines (shelf grid and collection lineup) hop or wave in 2D, using the canvas motion. The lineup also gets "!" and "?" bubbles.
- **`ShelfPreviewRuntime` (your file, small edits):** I added `AnimateMissing` (the HUD sets it to false, so silhouettes take no actor slot) and raised `FramesPerSecond` from 15 to 30. Found cards in the grid and lineup still use your live meshes.
- **`AdaptLayout`** now invalidates the previews only when the screen size changes. It used to run on every collection refresh, including walking and step updates, which restarted every shelf animation.
- **Your requests, done:**
  - Owned finish dots use `SwatchColor`; unowned dots stay concealed.
  - The Home world tile shows only when `HomeWorld` isn't "studio".
  - The stage test opens the Collection tab before checking the dots.
  - The lineup silhouette test checks the 2D motion, focuses the lineup outline (not the hero shape), and waits a frame after Motion calm.
  - `AnimatedShelfUsesRealMeshes…` now scrolls to the "All your toys" cards, since the scene no longer uses live meshes.

**Toy framing (request for Codex):**
- The HUD now publishes `PrototypeHud.StageViewport`, the free area for the toy and plate in screen pixels (origin bottom-left, like `Camera.pixelRect`), plus a `StageViewportChanged(Rect)` event.
- The area is below the title block and above the tray, and above the ways-to-play hint, which now sits just over the closed tray.
- When a tray tab opens, the area shrinks to end above the open panel, and the hint hides.
- Please have `StageFraming` use this instead of the fixed 144 top and 120 bottom reference values. Something like `viewer.rect = area / screen`, with the toy and plate centred in it. Ease the change over about 0.2 s, so opening the drawer slides the toy up rather than covering it.
- Unity tests have not been run on my side. The HUD type-checks with a real C# compiler against Unity stand-ins.

## October 14 — art the box-first Collections screens need (owner preview: canvas row "Collections, box first")
The owner wants each collection's box as its main art, a banner per collection, and the collection's colours carried through the page. To build that, I need from the catalog and art side:
1. **A box render per collection:** a transparent PNG, about 400 px, shaped like `UI/Boxes/JellyGarden.png`. The owner is asking you for Fox and Axolotl. Pip can keep Jelly Garden.
2. **A banner per collection:** a wide painting. The world paintings already work (Pip → jelly-garden, Fox → fox-hollow, Axolotl → moonlit-pond). A catalog field such as `CollectionDefinition.BannerArt` / `BoxArt` (resource paths) would let me load them without hard-coding.
3. **Theme colours per collection:** `FieldColor` and `InkColor` exist. Please add an `AccentColor` (Pip #E8508A, Fox #EC7951, Axolotl #F493B5 in the preview) for the progress card and the main button.
4. **World tray thumbnails:** I'll crop them from the same world paintings, so nothing extra is needed.

## October 14 — box-first Collections built (owner approved; local folder, uncommitted)
The owner approved the "Collections, box first" canvas row and a new direction: **every collection comes with its own world backgrounds**, matching its toys' setting. The owner is asking you for the Fox and Axolotl box renders and for each collection's world environments.

**What I built (HUD):**
- **Collections list:** each card is the collection's world painting with its box on the right. The name, series, status and an accent progress bar sit on a milk fade.
- **Collection page:** the banner painting with the box (with a small show-off hop) and the title on the art, then an accent progress card with a halfway tick. Below that, tiles tinted with the accent, the secret card, and a "Open a <Collection> box, <price>" button in the collection's ink.
- **World tray thumbnails:** these now show the real paintings via `PaintedWorldAsset.Load`.
- **Fallbacks:** collections without a box render show the character portrait on the collection's colour.
- **Code:** the lookups live in `Runtime/UI/CollectionLook.cs`.

**How to hand me art without code changes:**
1. **Boxes:** drop a transparent PNG at `Resources/UI/Boxes/Collection-<collectionId>.png` (`Collection-fox`, `Collection-axolotl`). The HUD picks it up and keeps its aspect ratio. Pip keeps `JellyGarden.png`.
2. **Banners** come from world paintings: Pip → jelly-garden, Fox → fox-hollow, Axolotl → moonlit-pond (`CollectionLook.BannerWorld`). When each collection gets its own worlds, please add a catalog field listing a collection's worlds, with the first one as its banner, and I'll switch the lookup to it. The tray can then show that collection's worlds instead of the shared Garden and Midnight.
3. **Colours:** accent and ink are in `CollectionLook` for now (Pip #E8508A / #7A2A4E, Fox #EC7951 / #7A3210, Axolotl #F493B5 / #2A2A5A). Please move them into the catalog when you add the world list.

**Test change:** the moss collection test now finds the main button by name ("Collection boxes"), because its label now reads "Moss boxes are coming soon".

## October 14 — live shelf decisions (owner)
- **Live shelf:** 6 spots (two shelves of three). The player picks which toys live there; every other toy shows as a still slot under its collection. New toys go onto the shelf automatically while a spot is free.
- **Shelf look:** it should look like a real shelf, not a world painting. **Request for Codex:** a shelf backdrop image (portrait, about 1080×1300, two planks at roughly 42% and 88% of its height, with a soft wall behind). World backdrops stay per toy, on the stage only.
- **Spot saving:** the selection is a presentation preference. I'll store it locally, like the shelf moments, unless you'd rather keep it in the save.
- **Worlds (owner, Oct 14):** each collection gets **5 world environments** of its own. The stage World tray will show Studio plus that collection's five, in a horizontal scroll. Please add a list of world IDs to each collection in the catalog. The first world in the list is also the collection's banner.

## October 14 (evening): live shelf, personalities, five-world tray (owner approved; local folder, uncommitted)

**Built (HUD):**
- **Live shelf:** six spots (2 × 3) of the player's picks (`LiveShelfKey` / `LiveShelfSeenKey`, local preferences).
  - New toys take a free spot automatically. A toy the player took off isn't re-added.
  - The shelf uses `Resources/UI/Shelf/Backdrop` when that file exists, with planks at 42% and 88% of its height. Until then it draws its own wall and planks.
  - Each shelf shows its first neighbouring pair's moment.
- **Picker:** a new page, "Pick your live shelf" (`AppPage.ShelfPick`, appended to `MenuNavigation`). It has the six spots, a filter per collection, and every owned toy with its seat number.
- **Shelf tab:** the live shelf, then one card per collection.
  - Each card shows its cover, box and progress, plus five still slots: found toys first, then silhouettes. A star marks live-shelf toys and ×N marks spares.
  - The old Pip-only grid and its Everything / Missing / Duplicates switch are hidden from this page. I'll remove that dead code in a cleanup pass.
- **Layered personalities** (`PersonalityBook`): every collectible now has a personality, made of its character's nature (Pip, fox, axolotl) plus its finish's touch.
  - Shelf moments come from trait pairs whenever no pair is written by hand.
  - The personality page shows the nature and finish cards, shelf friends from the live shelf, and the collection family.
  - The stage tray has a fourth tab, **About**.
- **World tray:** Studio plus `collection.Worlds` (all five), scrolling sideways, with painted thumbnails.
  - The HUD raises `EnvironmentChanged(worldId)` and exposes `CurrentEnvironmentId`. `WorldChanged` still fires, set to `Home`.
  - The last world is remembered per collection (`pockle.stage.environment.v1.<collectionId>`).
- **Banners and boxes:** these now use catalog `CoverArt`, `BoxArt`, `AccentColor` and `InkColor` (`CollectionLook`), with the old images as fallbacks.

**Small edit in your file:** `TactilePrototype.ApplyStage` now calls `PrototypeStage.SetEnvironment(viewCamera, hud.CurrentEnvironmentId, id)` when an environment ID is set, and otherwise the old `SetWorld`. Please keep this, or move it wherever you prefer.

**Tests:** I updated `CollectionUiTests` for the live shelf, the lineup-based animated-mesh test, the five worlds and the About tab, and added a picker test and a layered-personality test. Unity hasn't run any of these yet; the HUD type-checks against Unity stand-ins.

**Requests for Codex (owner, October 14):**
1. **Free 360° turning, no plate.**
   - One-finger drag turns the toy freely in any direction, like orbiting in Blender (a trackball: yaw and pitch, with a little inertia).
   - Remove the plate; keep a soft contact shadow under the toy.
   - Suggested gestures:
     - Press and hold to squish.
     - Two-finger pinch to zoom.
     - Two-finger drag to move the toy in the frame.
     - Double-tap to reset the view.
   - Motion calm turns inertia off. Tell me the final gestures and I'll update the hint text: today TactilePrototype sends it via `SetStatus`; I can also show it in the HUD.
2. **The background moves with the spin (parallax).**
   - The painted worlds are wide, but a phone shows only a slice. As the toy turns, pan the background horizontally with yaw, slowly enough to read as depth: for example, about 25% of the hidden width per 180° of yaw.
   - Ease it, clamp it at the painting's edges (or wrap seamless panoramas), and add a small vertical shift with pitch.
   - Motion calm should hold the background still.
3. **Framing:** please frame the toy inside `hud.StageViewport` (`StageViewportChanged`). The About tab opens a taller panel (196 px), and the toy should slide up rather than sit under it.
4. **Shelf backdrop image** at `Resources/UI/Shelf/Backdrop.png`: portrait, about 1080 × 1300, a real shelf with planks at 42% and 88% of the height.

### Stage collection tab: toy thumbnails (Oct 14)
- Owner asked for small pictures of the other toys instead of colour dots in the stage's Collection tab. `PrototypeHud.Play.cs` now builds a sideways-scrolling row of 58×66 rounded tiles: found toys show their still portrait (tap to play), missing toys a silhouette (secret stays hidden). Selected toy gets an outer ring. Buttons keep the `Stage finish · id · label` names, so tests are unchanged. Collection panel is 160 px tall; "find more" sits below the row. UiBuild "v2 screens 09". Compiles against stubs; not yet run in Unity.

### Press tint on toy tiles (Oct 14)
- Owner saw a lighter square on a pressed lineup tile (Velvet Flock). Cause: the Button tint darkens only the tile background; the portrait keeps its baked background colour. New `UI/TintMirror.cs` copies the background's tint to the portrait each frame; wired into lineup tiles, shelf collection slots, shelf picker tiles/spots and the stage Collection-tab tiles. UiBuild "v2 screens 10". Compiles against stubs; not yet run in Unity.

### Live shelf: cut-out portraits (Oct 14)
- Owner: the live-shelf toys showed their portrait's square background rotating with them. New `UI/PortraitCutout.cs` renders each toy once on black and once on white (via `ToyPortrait.Render`, unchanged) and recovers a transparent-background Texture2D plus the feet line. The living shelf and the personality page's tilting toy now use it; `MeasureFeet` is gone (feet come from the cutout's alpha). Day and night share one portrait. UiBuild "v2 screens 11". Compiles against stubs; not yet run in Unity.

### Request for Codex: glow halo drifts off-centre (Oct 14)
- Owner screenshot: Biolume Glow on Night lamp, toy turned sideways; the glow sits off to the lower left behind the toy instead of around it.
- Cause in `ToyGlowAura.cs`: the halo is a child of `toyMount` at `localPosition (0,0,.75)`. When the toy spins, that offset turns with the mount, so the halo circles around the toy. Only its rotation follows the camera.
- Suggested fix: in `LateUpdate`, place it from the camera's point of view each frame, e.g. `halo.position = mount.position + viewer.transform.forward * .75f + Vector3.up * <body-centre height>;` and keep `halo.rotation = viewer.transform.rotation`. Optionally scale it slightly when it would otherwise be hidden by the body.
- Being hidden behind the toy where they overlap is expected (depth test, additive card behind the body). That reads as a rim of light as long as it stays centred.
- The plate light-spill card (`pool`) hangs off the plate height (`.154`); it will need a new home once the plate goes away with free rotation.

### Collections list: dark see-through shade (Oct 14)
- Owner: the milk fade hid too much of the cover art. Painted cards now use a plum shade (Ink at 58%, over the left 72%) with white words and a faint shadow; the progress track is white 32%. Cards with no painting keep the old dark-on-colour look. Unnumbered collections whose toys can be had now read "New collection" instead of "Coming soon" (Axolotl and Fox have SeriesNumber 0 in the catalog; Codex may want to give them real series numbers). UiBuild "v2 screens 12".

### Boxes: glow, sparkles, shake (Oct 14) + request for Codex
- New `UI/BoxHype.cs` (added to the box RawImage; a child redraws the picture so layout is untouched): pulsing accent glow behind, 4 twinkling sparkles plus a corner glint, and a short wiggle-and-swell every ~4 s (off under Calm motion). Wired into collection list cards, the collection hero, shelf collection cards and the store's closed boxes. `UiShapes.Glow` added. UiBuild "v2 screens 13".
- **Request for Codex:** the box renders themselves are washed out. Measured average saturation: `Collection-axolotl.png` 0.11, `-pip` 0.15, `-fox` 0.21, versus the flat box artwork Codex made (much brighter pinks/greens). The print looks hazed white by lighting/exposure. Please re-render with the printed faces closer to the source art (lower exposure or unlit/emissive print, less white fill), same 1024² size and file names so the UI picks them up with no code change.

### Personality: shelf friends always show the friend (Oct 15)
- Owner owns every toy but friends read "Not found yet": the label tracked the pair's shelf moment, not ownership. Friends now always show their portrait and field colour; an unfound moment reads "<Name> · moment to find". `LivingShelfShowsMomentsAndOpensAPersonality` updated to match. The 4 UI failures Codex listed on Oct 15 are on my list.

### Play screen: side rail (Oct 15, UX-039)
- Owner approved canvas boards "Play: side rail" / "Worlds open from the rail". `PrototypeHud.Play.cs`: the bottom tray strip is gone. Four round buttons on the right (World, Light, Collection, About; new icons `Resources/UI/Icons/World|Light|Collection|About|Close.png`) open one floating card at the bottom with a title, note and close. Buttons keep their `Stage tab · X` names; panels, tiles and `Stage finish/world/light/meet` names are unchanged.
- Over a painted world the title turns white over a soft top shade. The hint pill sits at the bottom when nothing is open and may wrap to two lines (Codex's runtime hint is long; a shorter one would read better, e.g. "Tap to play · drag to turn · pinch to squish").
- **StageViewport now excludes a rail-width strip on both sides** (so the toy stays centred and clear of the rail) and runs from the title down to the hint, or to the open card. UiBuild "v2 screens 14". Compiles against stubs; not yet run in Unity.

### UI checklist fixes (Oct 15, UiBuild "v2 screens 15")
- **The four UI failures Codex reported:** `ShelfTilePortraits` now finds the live-shelf "Toy" pictures (they are cut-out Texture2Ds since Oct 14, not RenderTextures), so the 3-toy and 4-toy checks see them again; the play round trip taps `Slot · moon` (the old "Moon Jelly" text button is gone); retired Loop/Sprig in tests became Tula. `PersonalityBook` has first-draft natures for Tula, Firb, Coco, Snowy and Ginger (from your reaction notes) plus a plain fallback for any future character, so `EveryCollectibleHasALayeredPersonality` covers the holiday set.
- **Live shelf:** uses your `UI/Shelves/LiveShelfBackground.png` at its own aspect (no crop); feet sit on measured plank tops (39.2% and 82.8%). The "N of 6 spots" chip moved to bottom right, under the lower plank. (The stepped plank look came from the old square portraits, gone with cut-outs.)
- **Full-bleed pages:** new `PrototypeHud.CompactHeader.cs`: once the collection hero or the toy's details picture scrolls away, a bar in the page's colour fades in behind the back arrow with the name centred.
- **Play card:** close button 44 px. The phone's Back closes an open card first; the on-screen back arrow still leaves the toy. New assertion in the stage test.
- Also landed: the two-line play hint from earlier (a previous write had not taken). Compiles against stubs; Unity tests and the phone are not run.

### Toy details redesign built (Oct 15, UiBuild "v2 screens 16")
- Owner approved canvas board "Pockle details, more visual". `PrototypeHud.Personality.cs` rebuilt: eyebrow (collection · series · no.), name, summary; trait badges (two nature traits + the finish's touch); an "Its personality" card with three sliders (Energy sleepy→bouncy, With friends shy→chatty, Bedtime early bird→night owl); nature + touch cards with a "+" between; "How it reacts" as up to four icon tiles (new icons `Resources/UI/Icons/React*.png`); shelf-friend cards (picture, name, "Moment found"/"Moment to find", the moment or "Seat them side by side") with a found count; the family as a swipeable strip of pictures (tap an owned one to switch); Play.
- Slider values come from `PersonalityBook.Meters` (first draft: motion style + trait words, no hand tuning; owner may override). Purely presentation; no save or inventory change.
- `LivingShelfShowsMomentsAndOpensAPersonality` updated for the new friend wording and checks sliders and the 10-tile family strip. Compiles against stubs; Unity and phone not run.
- Oct 15: personality sliders are plain read-only bars now (no round handle); UiBuild "v2 screens 17".

### Home rebuilt (Oct 15, UiBuild "v2 screens 18")
- Owner-approved canvas row "Home". `PrototypeHud.Home.cs`: today's box card (real box art + BoxHype only when ready, steps and "Keep walking · N to go" otherwise, "Your box is ready!" status), live shelf peek (Codex's shelf art cropped to the top plank, first three live-shelf toys, their moment), activity rail with Nearby / Friends / Everyone chips, Keep collecting cards (closest to complete first), New this season banner (last unnumbered collections with art). Old Rewards/Friends rows and the 4-Pip strip are gone; Rewards stays reachable from "Keep walking".
- New `UI/ActivityFeed.cs`: `IActivitySource` + `LocalActivityFeed`. Real data today is only the player's own finds and completed collections (kept on device, `pockle.activity.mine.v1`). Sample posts (invented names, tagged SAMPLE) appear only in the editor/development builds or with `pockle.dev.sampleActivity=1`. Nearby/Friends need accounts and a server; a server-backed `IActivitySource` can replace this without UI changes.
- **Today's box is hard-wired to Pip** (the daily pool is Jelly Garden). Owner wants up to 4 boxes a day with a rule for which collection each comes from; that's a reward/Core decision for Codex once the owner picks the rule.
- Tests: Friends/Browse buttons now found by name; retired Moss → Tula; new `HomeActivitySwitchesScopeAndCheersWithoutGrantingToys`. Compiles against stubs; Unity and phone not run.

### Request for Codex: daily boxes "smart mix" (Oct 15)
- Owner chose the smart mix. Full rule, defaults and the API the HUD needs: `docs/DAILY_BOXES.md`. Up to 4 boxes a day at 1k/3k/6k/10k steps; box 1 from the player's pick, boxes 2–4 80% weekly featured / 20% any; secret odds unchanged; missing non-secret toys get double weight once half a collection is found. Needs a versioned save migration. I'll build the Home "Box N of 4" card and the box-1 collection picker against `DailyBoxState` once it exists; until then Home keeps today's single Pip box.

### Personality system proposal (Oct 15)
- Owner asked for personalities to shape faces, shelf interactions and fusion. Draft for review: `docs/PERSONALITY_SYSTEM.md` (four dials, eight temperaments, face sets, relationship table with friendship levels, fusion inheritance with eight named hybrids, who builds what).
- **For Codex, once approved:** four new face channels (lids, frown, signed glance, blush) alongside blink/happy/surprise; temperament-weighted reaction choice; engine-free `PersonalityProfile` + inheritance with portable checks; save the resolved profile with fused toys. Nothing to build until the owner approves.

### Personality system approved (Oct 15)
- Owner approved `docs/PERSONALITY_SYSTEM.md` as drawn on the canvas row "Personality system". **Codex can start:** face channels (lids, frown, signed glance, blush), temperament-weighted tap reactions, engine-free `PersonalityProfile` (dials, temperament, hybrid id) and the inheritance rule with portable checks, resolved profile saved with fused toys. First hybrids to build: Secretly Sweet, Stage Fright Star, Power Napper, Stargazer.
- Claude next (UX-042): authored dial tables per character/finish in `PersonalityBook` (replacing the guessed slider values), temperament badge + Mood bar on the details page, relationship table + moments and friendship levels on the live shelf, fusion reveal copy.

### UX-042 personality system, Claude's side (Oct 15, UiBuild "v2 screens 19")
- New `UI/PersonalityBook.Temperaments.cs` (`PersonalityBook` is now `partial`): `Temperament`, `Relationship`, `PersonalityDials`, dial tables, `TemperamentOf`, `RelationshipOf`, relationship moments by friendship level, hybrids and `DescribeFusedPersonality`. Old `Meters` and `TraitMoment` removed; `FindMoment(a, b, night)` still returns the level-1 moment, `FindMoment(a, b, night, level)` the deeper ones.
- New `UI/ShelfFriendships.cs` (`pockle.shelf.friendship.v1`): days side by side per pair, levels at 1/3/7, never decays.
- Details page: temperament badge + one-line signature in "Its personality", fourth bar Mood (Grumpy ↔ Sweet), friend cards show "relationship · Lv N". Live shelf and Home peek use the pair's level. Fusion reveal: personality tile shows the hybrid/temperament, new line under the name.
- Tests: new `TemperamentsRelationshipsAndFriendshipFollowThePersonalitySystem`; living-shelf test checks Mood bar, "Show-off" badge, "Best buds · Lv 1" and friendship days; fusion preview now expects "Stargazer". Compiles against stubs; Unity tests and phone not run.
- **For Codex:** when `PersonalityProfile` lands, `PersonalityBook.Dials`/`TemperamentOf` can feed it (or read from it), and `DescribeFusedPersonality` should read the saved fused profile instead of `PreviewFusedDials`. Face channels can key off `PersonalityBook.TemperamentOf(id)`.

### Review: temperament face studies `docs/personality-review/pip-01` (Oct 15, for Codex)
Owner asked for notes to bring the pass closer to the canvas designs (rows "Eight temperaments", "Grumpy face set"). Good already: Grumpy rest/tap, Shy, Sleepy rest, same-body comparison and framing. Fixes, in priority order:
1. **Lids cut from the top, not squash the eye.** Today `lids` scales the whole eye into a flat oval, so Grumpy/Sleepy/Calm/Dreamy read as the same squint. Make the lid a straight edge masking the top of the eye (bottom stays round) and add a lid-angle control: inner corners down for Grumpy and Mischief, outer corners down for Sleepy, flat for Dreamy and Calm.
2. **Closed eyes are curves.** Wink (Show-off, Mischief), Calm's slow blink and Sleepy dozing render as a flat dash with a leftover highlight. Use a happy curve (◡) for wink/contented blink, a gentle down curve for sleep; hide the highlight once lids/blink > ~0.8.
3. **Gaze must read.** The eyes are solid dark (no whites), so gaze = move both eyes together ~5–8% of face width plus shift the highlight the same way. Dreamy looking up, Shy looking down/away and Mischief's side glance are currently almost invisible.
4. **Mouth shapes.** Sleepy and Dreamy tap use the stretched smile, which reads as a V/fang; author a small round "o". Add a mouth tilt (one corner up) for Mischief's crooked grin.
5. **Highlights as signature.** Show-off: a sparkle/double glint, even at rest. Dreamy signature: star-shaped highlights (sets up the night-shelf glow).
6. **Per temperament:**
   - Sunny, Show-off and Mischief rest are near-identical smiles. Show-off: wider grin + sparkle eyes. Mischief: crooked grin + side glance + inward-angled lids.
   - Grumpy signature jumps to fully open eyes and reads as Sunny. Keep lids ~.35, glance away, small smile + blush: still trying not to smile.
   - Dreamy rest reads as Sunny. Needs visible upward gaze, flat soft lids, smaller mouth.
   - Calm: flat lids ~.2 so it reads relaxed rather than neutral.
7. **Blush** lands on the cream inclusion bubbles and looks muddy. Place it slightly lower and toward the centre, as a more saturated soft oval.

Suggested order: 1 and 2 first (they touch five temperaments), then 3, then 4, then 5. If pass 02 still misses on lids or the Mischief/Grumpy expressions, Claude will draw those faces on the canvas next to the renders.

### Owner fixes: walking button and toy crop box (Oct 16, UiBuild "v2 screens 20")
- **"Enable walking" no longer shows once walking is on** (Rewards/Boxes walk card and Settings). The box button takes the full row; the status reads "Walking is on. Keep Pockle open on your walk." Settings shows a disabled "Walking is on".
  - Today the HUD infers "on" from the tracker's "Keep Pockle running…" status or steps already counted today (`PrototypeHud.Store.cs` `WalkingOn`).
  - **Request for Codex:** expose `CollectionSession.WalkingActive` (true once `StepCounter.start()` succeeded and permission is held) so the HUD can stop matching status text.
- **Toy crop box on the play screen (owner: "you don't have to add a bounding box around it").** The stage camera's `viewer.rect` is set to the HUD's `StageViewport`, so the glow and zoomed toys are clipped to a visible rectangle.
  - HUD side done: `StageViewport` is now full width (no rail inset). The rail is symmetric, so the toy stays centred, and taps on rail buttons are still blocked by the EventSystem check in `ToyViewGesture`.
  - The top and bottom edges still clip when zoomed in or glowing, and the card-open test needs the area to shrink.
  - **Request for Codex (`PrototypeStage.SetViewport`/`LateUpdate`):** render the toy camera full screen, and use `StageViewport` only to frame: centre the toy in that rect and size it to fit, for example with an off-centre ortho projection or camera shift. Keep `ToyViewGesture`'s hit area on the toy/full screen rather than `viewer.pixelRect`. Then nothing is ever cropped, and the HUD buttons simply overlay the toy.

### Speech-bubble logic (Oct 16)
- New `UI/ToyExpressions.cs`: `ReactionCue`, `ExpressionSticker`, `ToyExpression`, `ToyExpressions.Candidates/Chattiness/Warnings` and `ExpressionPicker` (weighted, no repeats, Grumpy third poke gives in). Rules in `docs/PERSONALITY_SYSTEM.md` section 10. Tests: `Tests/PlayMode/ToyExpressionTests.cs`. Compiles against stubs and the tables were checked with a console run (74 available toys, every cue filled, sounds 16 characters or shorter); Unity tests not run.
- **Request for Codex:** raise an event when a tap reaction plays, carrying the cue (poke, squish, pick up, turn, left alone, night) and, ideally, the toy's head position on screen. Claude will build the bubble, the temperament badge row and the warning chip on the play screen against it. Also please shorten the play hint to one line ("Tap · drag to turn · pinch to squish").

### Play-screen personality + Fuse spares v2 + fusion moment (Oct 17, UiBuild "v2 screens 21" and "22")
Owner-approved canvas boards: "Play: speech bubble + personality", "Fuse spares, redesigned" and "The fusion moment".
- **Play screen** (`PrototypeHud.StageTalk.cs`, `UI/BubblePop.cs`):
  - Under the name: a temperament badge and two traits, then a subtle "handle with care" chip. Tapping the chip shows the next line.
  - A speech bubble with a sound and/or a sticker icon. New icons in `Resources/UI/Icons`: Sticker* and Warning.
  - The toy says hello when it arrives. The title block is about 74 px taller, so the toy's area starts lower.
  - **Request for Codex:** call `hud.ShowToyExpression(cue, headScreenPoint)` when a tap reaction starts. Cue is `ReactionCue.Poke`, `Squish`, `PickUp`, `Turn`, `LeftAlone` or `Night`. The head point is optional; without it the bubble sits over the upper part of the toy's area. The picker already handles chattiness, no repeats and Grumpy's third poke.
- **Walking:** `PrototypeHud.Store.cs` now uses your `session.WalkingActive` instead of matching status text.
- **Fuse spares** (`PrototypeHud.Fusion.cs`):
  - A dark fusion card: both picks stand on pedestals with name and temperament. Between them is a "?" toy in the two colours, with a line: "Who will it be?", "A named hybrid?" or "Secret recipe!".
  - Chips for colour, finish and personality.
  - Spare tiles show their real spare count and a 1/2 order badge. Tapping a picked tile puts it back. A note names toys with no spare.
  - The button reads "Fuse Peach + Moon".
- **Fusion moment** (`PrototypeHud.FusionMoment.cs`, `UI/FusionMomentPlayer.cs`):
  - About 4.6 seconds, full-screen: crouch and hop, spiral in, squish into a swirling orb, temperament badges fly in (plus a gold "Secret recipe!" for secret pairs), flash, ring, confetti, the fused toy drops in with "It's… a Stargazer!", then the existing reveal.
  - Tapping anywhere or the phone's Back skips to the reveal. Calm motion goes straight to the reveal. `Fuse()` still calls the fusion service before anything animates.
  - **Request for Codex:** haptics at commit, each half lap, the pop and the landing, if you'd like them (the HUD has no haptic hook).
- **Tests:** new `ToyStageShowsPersonalityWarningsAndSpeechBubbles`. The fusion preview test now checks the new card and skips the moment. Everything compiles against stubs; Unity tests and phone not run.

### Owner fixes + winter trio personalities (Oct 17, UiBuild "v2 screens 23")
- **White box on Home's "Today's box":** Home disabled the `BoxHype` component while the box wasn't ready. `BoxHype` draws the picture through its child, so a disabled one left an empty RawImage, which draws white. `BoxHype` now has `Excited` (glow, sparkles and shake on or off) and always mirrors the picture; it also starts hidden until the first mirror. Home's other pictures (activity cards, live-shelf peek, collection covers, new-season boxes) go through `SetPicture`, which hides a RawImage that has no texture.
- **Square behind the fused toy (reveal and fusion moment):** the stand-in picture was a blend of two square previews. It's now a blend of the two cut-outs with transparency (`BlendCutouts`). `PortraitCutout` textures stay CPU-readable for this; they're small and cached once per toy.
- **Reiny / Guin / Bundy:** natures, dials and bubble sounds. Reiny is Sunny ("Eager and gentle"), Guin is Calm ("Cosy and chilled"), Bundy is Show-off ("Jolly and sweet"). Polar Glow Guin comes out Sleepy and Holly Glow Bundy comes out Mischief. Reaction lines reuse Codex's action names. Checked with a console run: all 104 available toys have bubble sounds for every cue and 2–3 warnings.
- Compiles against stubs; Unity and phone not run. **Codex:** please carry these into PR #20 or a follow-up, and build a new APK when convenient; the Fox fixture selectors you changed are kept.
