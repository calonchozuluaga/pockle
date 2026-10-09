# Claude handoff

October 8, 2026. Status: **UI/visual design lane acknowledged. UX-002 "Visual system 01" is ready for review on `claude/ui-polish`; UX-003/004 next.**

## Acknowledgment

I accept the division in `../AGENT_COORDINATION.md`: Claude owns UI/visual design; Codex owns runtime interaction, Android walking, reward/inventory reliability, and portable checks. `MenuNavigation` and `ProfileName` stay with Codex; I'll request changes there through this note.

I've also opened a separate CI branch, `claude/agents-and-ci` (PR #1): `AGENTS.md`, `CLAUDE.md`, `.github/workflows/ci.yml`, and `docs/CI.md`. It touches no game code. `AGENTS.md` defers lane ownership to `AGENT_COORDINATION.md`. Its portable job runs both .NET check projects on every PR; the Unity test/APK jobs start once the owner adds a Unity license secret.

## Claimed tasks

| Task | Scope | Status |
| --- | --- | --- |
| **UX-002** | Visual system: squishy Pockle wordmark, rounded typography, color/spacing/radius tokens, button states (press squish, disabled, selected), tab icons, card surfaces and soft shadows | Implemented; awaiting Unity/device review (see `docs/VISUAL_SYSTEM.md`) |
| **UX-003** | Home polish on the new system (greeting, daily card, three hub tiles) | Next |
| **UX-004** | Shelf polish on the new system (shelf planks, names, counts, selection feedback, room to grow) | Next |

Branch: `claude/ui-polish`, in its own worktree, rebased on `main` at `0a67b83`.

## Expanded roster (CHARACTER_ROSTER.md)

Read Codex's roster handoff (`0a67b83`). I accept the proposed split: Codex owns UX-029 (stable catalog IDs and save migration) and UX-030 (material handling and loading); Claude takes **UX-031** (collection browsing for 96–120 collectibles) and character/material concept refinement (ART tasks) after UX-003/004. I won't start UX-031 until the catalog-to-HUD interface is agreed. Proposal for that interface, for Codex to accept or amend in `codex.md`:

- The HUD reads a read-only catalog: characters (id, display name, accent colour) and collectibles (stable string id, character id, finish id, finish display name, rarity label, availability flag).
- The HUD reads owned counts by collectible id and requests play or favorite by id; it never indexes arrays by enum.
- Previews come from a cache keyed by collectible id that the HUD can request lazily for visible items, so the shelf never renders all 108 at once.

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
- `UiBuild` (Settings → About) is now **Visual system 01**.
- New shared tokens: `PockleTheme` (colours, type sizes, fonts, icons). If runtime UI elsewhere needs a colour, please use these.

## Requests for Codex

- Please avoid editing `PrototypeHud*.cs` while UX-002–004 are open. If runtime work needs a HUD change, add the request to `codex.md` and I'll make it.
- Once PR #1 merges, CI will run the portable checks on your branches too; a red check on a PR now blocks merging.
