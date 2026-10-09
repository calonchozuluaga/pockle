# Claude handoff

October 8, 2026. Status: **Acknowledged the proposed UI/visual design lane. UX-002 in progress.**

## Acknowledgment

I accept the division in `../AGENT_COORDINATION.md`: Claude owns UI/visual design; Codex owns runtime interaction, Android walking, reward/inventory reliability, and portable checks. `MenuNavigation` and `ProfileName` stay with Codex; I'll request changes there through this note.

I've also opened a separate CI branch, `claude/agents-and-ci` (PR #1): `AGENTS.md`, `CLAUDE.md`, `.github/workflows/ci.yml`, and `docs/CI.md`. It touches no game code. `AGENTS.md` defers lane ownership to `AGENT_COORDINATION.md`. Its portable job runs both .NET check projects on every PR; the Unity test/APK jobs start once the owner adds a Unity license secret.

## Claimed tasks

| Task | Scope | Status |
| --- | --- | --- |
| **UX-002** | Visual system: squishy Pockle wordmark, rounded typography, color/spacing/radius tokens, button states (press squish, disabled, selected), tab icons, card surfaces and soft shadows | In progress |
| **UX-003** | Home polish on the new system (greeting, daily card, three hub tiles) | Next |
| **UX-004** | Shelf polish on the new system (shelf planks, names, counts, selection feedback, room to grow) | Next |

Branch: `claude/ui-polish`, in its own worktree, based on `main` at `229341d`.

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

- None yet for UI code; this note is the claim.

## Requests for Codex

- Please avoid editing `PrototypeHud*.cs` while UX-002–004 are open. If runtime work needs a HUD change, add the request to `codex.md` and I'll make it.
- Once PR #1 merges, CI will run the portable checks on your branches too; a red check on a PR now blocks merging.
