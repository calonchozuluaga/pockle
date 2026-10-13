# Fox model review import

Owner-uploaded textured red fox, imported on `codex/fox-model-import` (ART-002 / UX-030).

Unity review asset: `Assets/Pockle/Art/Models/Fox/FoxReview.prefab`. Drag it into a review scene. `FoxVinyl.mat` exposes the base-colour texture, metallic value and smoothness using Built-in Standard. Original normal and packed metallic/roughness textures are preserved in `ArtSource/Fox`; the glTF roughness channel requires conversion to Unity smoothness before use.

The original upload has 1,943,990 triangles. The review FBX has 30,000 triangles, source UVs and 2048px paint. Uniform height is 1.94 units, floor at zero. The review prefab remains available. The Fox collection now registers `fox.gloss-vinyl` as available runtime art, with nine planned finish slots. Runtime loading uses `Resources/Characters/Fox/Fox.asset` and `FoxVinyl.mat`. There is no box pool, public series number, release date or automatic inventory grant for Fox. Normal collection discovery rules apply; editor Character studies can preview its real mesh without ownership. Generic non-Pip owned navigation remains a separate HUD contract request to Claude. Phone budget/appearance are unverified. Do not replace Pip's assets with this fox.

Regenerate with Blender 4.5.3: `blender -b --python tools/import-meshy-fox.py -- /path/to/source.glb`, then run `python3 tools/sync-unity-meta.py`. Unity generates the initial material/prefab only if missing, preserving later edits.

Verified: pinned Unity 6000.6.5f1 batch import and script compilation; material/prefab creation; converted mesh/paint inspected in a Blender render; required portable and source checks; whitespace. Not verified: Android build and phone appearance/performance; Unity collection/runtime checks are recorded below.

The owner project receives additive copies of the review assets/editor setup so its open Unity session can import them. Existing local settings and UI edits are preserved; the authoritative change is isolated in the branch above.

## Fox collection integration

Branch `codex/fox-collection` (UX-029/030/033, ART-002). Collection ID and character ID are `fox`; display name is Fox, approved by owner. The runtime mesh has 20,009 UV-split vertices and 30,000 triangles, centered in the supported toy coordinates. Native asset generation uses `Pockle.Editor.FoxCollectionBuild.Build`; it refuses to overwrite an existing runtime asset/GUID. Firm vinyl response, painted face/ears, a fox silhouette, and a calm-compatible idle recipe are registered. Other varieties remain planned.

October 13 Fox collection validation: Unity 6000.6.5f1 imported the native runtime asset (20,009 vertices / 30,000 triangles); full PlayMode suite 29/29 passed, including Fox browser/no-grant and model/material/switching checks; full EditMode suite 16/16 passed; portable Core checks and source checks passed; whitespace passed on the branch. Android build and phone visuals/performance remain unverified. An initial no-graphics UI run crashed during portrait rendering; the graphics-enabled suite passed after correcting the new test to use visible buttons.
