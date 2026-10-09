# AGENTS.md — working agreement for Pockle

Two AI agents build Pockle: **Codex** and **Claude**. The owner (Carlos) runs Unity and phones locally and makes the product calls. This file is the shared rulebook; both agents read it before every task. Codex reads `AGENTS.md` automatically; Claude reads it through `CLAUDE.md`.

## Sources of truth

| Question | File |
| --- | --- |
| What are we building and why? | `docs/PRODUCT_BRIEF.md` |
| What's done, what's next, acceptance criteria | `docs/UI_UX_TASKS.md` (stable task IDs: UX-001…) |
| How the code fits together | `docs/ARCHITECTURE.md` |
| What has actually been verified, and how | `docs/VALIDATION.md` |
| CI and how to read its results | `docs/CI.md` |

If a change alters any of these, update the doc in the same PR.

## Lanes — who owns what

Lanes keep two agents from editing the same files at once. Before starting, check **Current work** below and add your line in your PR. If you must touch another lane's files, keep the change minimal and say so in the PR description.

| Lane | Owner | Main paths |
| --- | --- | --- |
| Toy feel, deformation, materials, shaders, reveal | Codex | `Runtime/Core/*` (springs, shape, gesture, reveal), `JellyToy.cs`, `TactilePrototype*.cs`, `MysteryBox.cs`, `Shaders/` |
| Menus and screens (Home, Shelf, Boxes, You, Settings, onboarding) | Codex | `PrototypeHud*.cs`, `PrototypeStage.cs`, `MenuNavigation.cs` |
| CI, build pipeline, test infrastructure | Claude | `.github/`, `docs/CI.md` |
| Content catalog: toys, series, rarity, drop tables, save migrations | Claude | `CollectionProgress.cs`, `CollectionSession.cs`, `BoxCatalog.cs`, new catalog assets |
| Walking (Android Health Connect, iOS Core Motion/HealthKit) | Claude | `AndroidWalkingTracker.cs`, `Assets/Plugins/` |
| Backend, accounts, purchases/IAP validation | Claude | new `Runtime/Services/` (once a backend is chosen) |
| Product/design reviews of the other agent's PRs | Either | PR comments only |

### Current work

Keep one line per open branch. Remove your line when the PR merges.

- `claude/agents-and-ci` — Claude — this file, `CLAUDE.md`, CI workflow, `docs/CI.md`.

## Branches and pull requests

- Never commit directly to `main`. Branch as `codex/<topic>` or `claude/<topic>` from the latest `main`, and open a PR.
- One topic per PR. Rebase on `main` before asking for review; resolve conflicts in favor of whatever is already on `main` unless the PR's purpose is to change it.
- CI must be green before merge (see `docs/CI.md`). A red Unity job is a real failure, not noise.
- Every PR description ends with **Verified** (what ran and passed: CI jobs, portable checks, owner device test) and **Not verified** (what still needs Unity editor or a phone). Never describe unrun tests or unseen visuals as passing.
- Reference task IDs (`UX-0xx`) in titles or descriptions when relevant, and update their status in `docs/UI_UX_TASKS.md`.

## Project rules

- **Engine:** Unity `6000.6.5f1` exactly (`ProjectSettings/ProjectVersion.txt`), Built-in Render Pipeline, Input Manager (Old). Don't change these without the owner's approval.
- **`ProjectSettings/` and `Packages/manifest.json`:** change only when the task requires it, and call it out in the PR. The owner has local Android settings that must survive a `git pull`.
- **`Pockle.Core` stays engine-free.** No `UnityEngine` references in `Assets/Pockle/Runtime/Core/`; that is what lets CI and the .NET checks test it without Unity.
- **Every asset needs a committed `.meta` with a unique, stable GUID.** For new files created outside Unity, run `python3 tools/sync-unity-meta.py`; never regenerate existing GUIDs.
- **No per-touch network or AI calls.** Tactile play is fully local.
- **Originality:** all characters, names, and designs are original. Labubu, Needoh, Sonny Angel, etc. are emotional references only.
- **Money:** no payment credentials, live product keys, or real charges in the repo. Purchases must be validated server-side and granted exactly once before they go live.
- **Saves:** inventory changes must be persisted before any animation that reveals them, and save format changes need a versioned migration that preserves existing beta saves.

## Checks to run before opening a PR

```sh
dotnet run --project Tests/Pockle.Core.Checks/Pockle.Core.Checks.csproj
dotnet run --project tools/Pockle.SourceChecks/Pockle.SourceChecks.csproj -- .
git diff --check
```

CI runs these plus the Unity EditMode/PlayMode suites (`Pockle.Core.EditMode.Tests`, `Pockle.Variants.PlayMode.Tests`) and an Android build. When adding behavior, add portable checks in `Tests/Pockle.Core.Checks/` for engine-free logic and Unity tests in `Assets/Pockle/Tests/` for anything that touches Unity APIs.
