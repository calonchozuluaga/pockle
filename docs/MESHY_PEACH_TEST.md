# Peach Jelly phone texture repair

Unity **6000.6.5f1**, Built-in RP. Repair branch: `codex/peach-texture-seams`, based on main `c83c6030`. Boxes, animated shelf and silhouettes are included.

The owner reported light cracks across the eyes/body on phone. The repair welds the surface before mesh reduction and rebakes the original paint onto padded texture sections. The result stays within the existing phone budget. Shader and handling remain the same.

After this repair passes Unity/Android checks and merges:

```sh
git switch main
git pull --ff-only origin main
```

Rebuild/install with your existing Unity Android settings. Open **Shelf → Pip → Peach Jelly** and check the eyes/mouth at rest, while turning, stretching and lifting. Switch Peach → Moon → Peach and check that the pose still resets normally. Keep the existing collection save.

Local portable/source/whitespace checks pass. Offline paint comparisons and closed surface checks pass. Hosted Unity tests, closeup renders and Android packaging are pending. Owner phone confirmation remains pending.

Earlier import/material and test-build history: [PR #15](https://github.com/calonchozuluaga/pockle/pull/15) and [previous device notes](https://github.com/calonchozuluaga/pockle/blob/c83c60309f90bf7ae13fbc5c80b96f4d37cd5699/docs/MESHY_PEACH_TEST.md). Current status: [PR #16](https://github.com/calonchozuluaga/pockle/pull/16).
