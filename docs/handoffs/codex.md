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
