# Fox finish phone preview

The owner approved the Fox material sheet and a test copy of all ten finishes on October 14, 2026. `codex/fox-material-collection` extends the corrected upright Fox branch. All variants share the original readable 30,000-triangle mesh and 2048px paint; face/chest/black accents remain painted and opaque. No additional large textures are created.

| Display name | Stable saved ID | Runtime surface |
| --- | --- | --- |
| Classic | fox.gloss-vinyl | Original gloss vinyl |
| Apricot | fox.matte-vinyl | Pastel orange matte |
| Cloud | fox.velvet-flock | Cream-peach velvet grain and soft rim |
| Mochi | fox.mochi-foam | Blush soft porous finish |
| Copper | fox.coated-metallic | Brushed copper and studio reflections |
| Pearl | fox.pearl-jelly | Warm pearlescent sheen |
| Honey | fox.clear-jelly | Amber transparent shell with gold inclusions |
| Rose Quartz | fox.confetti-gel | Rosy transparent shell with crystal glints |
| Midnight | fox.boucle-plush | Indigo satin (keeps the existing planned slot ID) |
| Aurora · Super rare | fox.glow-jelly | Lavender transparent shell, angle-shifting coating and gold stars |

Aurora is the Fox collection's secret identity. No public draw odds, prices, release date, box pool or walking outcomes were changed. These are mobile approximations of the offline concept materials: no true refraction, ray-traced glass, bloom, or strand fur. Fox finishes keep the owner's approved turn-only interaction; material names do not activate Pip stretching.

The temporary `FoxOwnerTestGrant.txt` configuration enables both the old Classic award and the new independent `pockle.owner-test.fox-finishes.v1` grant on non-batch startup. The new grant fills only missing Fox IDs; it never adds another copy to owned finishes. Inventory is persisted before the marker and UI refresh, with interrupted startup recovery remaining idempotent. Existing Pip counts, daily steps, pending reveal and unknown save IDs survive. Disable/remove this test resource before public distribution.

Phone APK is built from an isolated snapshot of the owner's current local Assets/Packages/ProjectSettings, preserving local Android settings and Claude's UI/Settings, Peach expressions, walking repair and these new finishes. The branch contains only the Fox topic. Install the test APK as an update to the same app and launch it; the Fox collection will contain the ten owned playable finishes. No phone was connected during preparation.

Verified: Unity 6000.6.5f1 material renders, rigid/switching/inclusion tests, durable one-time grant/restart tests, owned finish UI selection/title tests, portable Core and source checks. Exact final suite/build results are recorded in VALIDATION.md. Phone visual quality, GPU frame time and sensor behavior remain unverified.
