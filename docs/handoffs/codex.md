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
