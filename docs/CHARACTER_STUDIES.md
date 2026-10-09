# Moss, Bop, and Nook — representative material studies

Branch: `codex/material-toys`. These are provisional original silhouettes from `CHARACTER_ROSTER.md`, prepared while Claude's ART-001 sheets are pending. Three new models and four study finishes are included, alongside the existing Pip. This is the first representative batch toward twelve characters and nine finishes each.

| Study | Shape | Surface | Handling | Runtime mesh |
| --- | --- | --- | --- | --- |
| [Moss](concepts/roster-studies/moss-study-01.png) | Squat garden sprite, broad three-lobed leaf hood, paired feet | Green short flock: matte fibers and soft rim | Gentle compression, limited stretch/sag, damped recovery | 2,840 vertices / 5,144 triangles |
| [Bop](concepts/roster-studies/bop-study-01.png) | Broad spinning-top cap, rounded stem, chunky paired feet | Warm apricot glossy vinyl | Very little deformation; whole-body rocking, no gel sag | 3,068 vertices / 5,516 triangles |
| [Nook / Oat](concepts/roster-studies/nook-study-01.png) | Puffy rounded-square cushion with four tucked corner paws | Oat bouclé: looped fabric approximation and soft rim | Stuffing-like give and sag, damped recovery | 2,564 vertices / 4,528 triangles |
| [Nook / Lilac](concepts/roster-studies/nook-foam-study-01.png) | Same Nook mesh and face | Smooth matte lilac mochi foam | Deeper press, less stretch/sag, slower recovery | Same mesh |

The PNGs are **Blender studio renders**. Unity uses its own single-pass Built-in shader; its appearance still needs editor/device review. Flock and bouclé use surface shading approximations with subpixel fading, not individual simulated strands or loops. Neither studio geometry nor lights are exported into the runtime models. Foam is matte, with fine surface grain rather than a vinyl reflection.

## Local review

1. Fetch and switch to `codex/material-toys` with Unity closed, preserving any local changes first.
2. Open the Pockle scene in Unity **6000.6.5f1**, allow the `.pocklemesh` imports to finish, and press Play.
3. Open **Pockle → Character studies**. Preview Moss, Bop, **Nook · Oat Bouclé Plush**, or **Nook · Lilac Mochi Foam**. This uses an owned Pip to enter the existing viewer; no study is granted to inventory. Finish any pending box opening first.
4. Drag upward to lift, release to drop, and press **J** to simulate a phone shake. Moss should give and settle; Bop should rock while retaining its shape. Nook plush should hang more softly than foam; foam should compress more deeply and recover more slowly. The current viewer heading still says Pip because Claude owns the UI integration.
5. Select an existing Pip from the shelf to return to normal play. Stop Play to exit review.

The review launcher exists only in the editor. Draft studies are still catalog-unavailable, absent from reward pools and ordinary Android shelf selection. No new starter gifts or purchases are added. Android review follows concept approval and Claude's ID-based shelf integration. The four existing Pip finishes retain their reviewed handling.

## Runtime interface for Claude

- `JellyToy.TrySetCollectible(string id)`: selects existing Pip IDs or the explicit study IDs `moss.velvet-flock`, `bop.gloss-vinyl`, `nook.boucle-plush`, and `nook.mochi-foam`. Returns false for missing/planned/unknown art and keeps the current visual. This is an **art renderer**, not an ownership authorization API. Switching between Nook finishes reuses the mesh, materials, face, and visual root; the raw pose is replayed with the selected compliance.
- `JellyToy.CollectibleId` and `.Handling`: current stable ID and compliance/recovery profile. `.Variant` remains the legacy Pip bridge; do not use it as a new-character ID.
- `TactilePrototype.TryPlayCollectible(string id)`: owned, catalog-available selection, after HUD navigation to the viewer. Returns false for draft/unowned/unavailable IDs or while opening a box. Does not grant ownership or finish a pending reveal.
- `CharacterArt.TryLoad(string id, out PipCharacterAsset asset)`: explicit resource registry, requiring readable mesh assets. `CharacterArt.MossStudyId`, `.BopStudyId`, `.NookPlushStudyId`, and `.NookFoamStudyId` name the studies; `IsStudy(string id)` checks membership. Both Nook IDs load the same resource. Other planned finish combinations remain unavailable.
- `CollectiblePreviewCache` now renders through an ID adapter but still gates on catalog availability. It will not offer draft portraits. After art approval, availability and acquisition policy must be agreed separately; then the same bounded cache can load their portraits.
- `PreviewCharacterStudy(string id)` is editor-only authoring access. It does not modify inventory or the saved Pip preset. It requires at least one owned Pip and does not bypass a running reveal.

No HUD/UI files or UI test fixtures were edited. Please keep the old overloads/events and use the owned runtime API when adding the ID callback. The prior request to back up the new profile ID keys in your UI fixture still applies.

## Authoring and checks

Sources: `ArtSource/Moss/Moss.blend`, `ArtSource/Bop/Bop.blend`, and `ArtSource/Nook/Nook.blend`; exchange FBX under `Assets/Pockle/Art`; native mesh/anchors under `Resources/Characters`. Nook's Blender file retains both material recipes, with bouclé active; its FBX carries that primary finish. Unity applies both finish recipes to the shared body. Nook has a packed UV layout for readable fabric loops. The existing `.pocklemesh` schema and fixed face anchor lengths remain compatible. Body rebuilds deactivate the old visual and release owned meshes, materials, and cubemaps; borrowed imported assets remain intact.

Rebuild from the repository root:

```sh
blender -b --python tools/build-character-studies.py -- --render
python tools/sync-unity-meta.py
python tools/check-character-studies.py
blender -b --python tools/check-character-fbx.py
```

To rebuild only Nook without rewriting existing Moss/Bop assets:

```sh
blender -b --python tools/build-character-studies.py -- --character Nook --render
```

Portable material tests exercise compliance limits, recovery/rebound, shake settling, variable frame timing, and Nook's plush-versus-foam recovery/compression/sag. Geometry checks exercise manifold connectivity/winding, budgets, finite coordinates/UVs, normalized contact bounds, volume, and face placement. The FBX round trip compares imported positions, topology, parts, and UVs with the native export.

`CharacterStudyTests` adds Unity PlayMode coverage for repeated switching and cleanup, missing-art rejection, picking, reset, opaque material selection, relative flock/vinyl deformation, and Nook finish reuse/recipe restoration/compliance. **Unity compilation, shader compilation/rendering, PlayMode tests, Android packaging, and device performance have not run in cloud.**

Next: review these silhouettes against Claude's identity sheets and compare the representative materials in Unity/Android, then expand approved finish recipes and remaining character models. Keep public availability and reward-pool changes explicit.
