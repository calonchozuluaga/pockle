# AGENTS.md — working agreement for Pockle

Two AI agents build Pockle: **Codex** and **Claude**. The owner (Carlos) runs Unity and phones locally and makes the product calls. This file is the shared rulebook; both agents read it before every task. Codex reads `AGENTS.md` automatically; Claude reads it through `CLAUDE.md`.

## Sources of truth

| Question | File |
| --- | --- |
| What are we building and why? | `docs/PRODUCT_BRIEF.md` |
| What's done, what's next, acceptance criteria | `docs/UI_UX_TASKS.md` (stable task IDs: UX-001…) |
| How the code fits together | `docs/ARCHITECTURE.md` |
| What has actually been verified, and how | `docs/VALIDATION.md` |
| Who owns what, and how to hand off | `docs/AGENT_COORDINATION.md`, `docs/handoffs/` |
| CI and how to read its results | `docs/CI.md` |

If a change alters any of these, update the doc in the same PR.

## Lanes and handoffs

Ownership and the handoff process live in [`docs/AGENT_COORDINATION.md`](docs/AGENT_COORDINATION.md); that document wins if anything here disagrees. In short:

- **Claude:** UI and visual design: UX-002 visual system, then UX-003/004 Home and shelf polish (`PrototypeHud*.cs`, new UI artwork/resources, UI PlayMode tests, UI design notes), plus CI (`.github/`, `docs/CI.md`).
- **Codex:** runtime interaction, Android walking, reward/inventory reliability, and portable checks (`TactilePrototype.cs`, `JellyToy*`, `Runtime/Core/`, `CollectionSession.cs`, `AndroidWalkingTracker.cs`, the Android plugin, `Tests/Pockle.Core.Checks/`).

At the start of every task, fetch `origin/main` and read both `docs/handoffs/codex.md` and `docs/handoffs/claude.md`. Record your claimed task IDs, branch, and files in your own handoff note. Ask for changes in the other lane's files through your handoff note instead of editing them concurrently.

## Branches and pull requests

- Keep `main` as the owner's testable build. Work on `codex/<topic>` or `claude/<topic>` branches from the latest `main`, in a separate checkout, and merge through a pull request (or leave the branch reference in your handoff note). Don't force-push shared branches.
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
