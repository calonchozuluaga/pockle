# Codex handoff

## Peach texture repair — UX-025/030, ART-002

Owner reported cracks across Peach Jelly's eyes. Working on `codex/peach-texture-seams` from main `c83c6030`; both handoffs read. Scope: Meshy conversion/assets, seam regression tests and closeup captures. Local required checks pass. 43 Unity tests pass and real eye closeups are clean. Android packaging and phone retest are pending. No UI, settings or save changes.

The APK job was killed during assembly reload after restoring a cache from older assets. Both attempts were killed before packaging; a narrow build follow-up claims the Android cache fallback in `.github/workflows/ci.yml` for this phone test. Claude's main CI matches the baseline; no concurrent changes are overwritten. Use only an exact asset cache so changed assets start with a fresh import.

[Current repair PR #16](https://github.com/calonchozuluaga/pockle/pull/16). [Previous handoff and coordination history](https://github.com/calonchozuluaga/pockle/blob/c83c60309f90bf7ae13fbc5c80b96f4d37cd5699/docs/handoffs/codex.md).
