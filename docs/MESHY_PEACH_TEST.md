# Clean Peach phone test

Preparing the owner's new clean Peach upload on `codex/peach-clean-v2`, based on main `3c7b071d`. Unity **6000.6.5f1**, Built-in RP. This version keeps the uploaded simple paint and fuller 3D body, without the earlier pearls or gold flecks.

The existing seam-safe conversion produces a closed mesh within the phone budget. Native asset and texture GUIDs are retained. Existing seam/import/grip/reset/switch/render checks cover the replacement.

After green Unity/Android checks and merge:

```sh
git switch main
git pull --ff-only origin main
```

Rebuild/install using your existing Unity Android settings. Open **Shelf → Pip → Peach Jelly**. Check eye/smile clarity at rest and while turning/stretching/lifting, the resting base, and Peach → Moon → Peach switching. Keep the existing collection save.

Required local portable/source/whitespace checks pass. Hosted Unity renders/tests, Android packaging and owner phone review are pending.

[Previous seam repair and phone-build history: PR #16](https://github.com/calonchozuluaga/pockle/pull/16).
