# Fox model review import

Owner-uploaded textured red fox, imported on `codex/fox-model-import` (ART-002 / UX-030).

Unity review asset: `Assets/Pockle/Art/Models/Fox/FoxReview.prefab`. Drag it into a review scene. `FoxVinyl.mat` exposes the base-colour texture, metallic value and smoothness using Built-in Standard. Original normal and packed metallic/roughness textures are preserved in `ArtSource/Fox`; the glTF roughness channel requires conversion to Unity smoothness before use.

The original upload has 1,943,990 triangles. The review FBX has 30,000 triangles, source UVs and 2048px paint. Uniform height is 1.94 units, floor at zero. This is a separate art-review prefab; catalog identity, tactile anchors and deformation/mobile budgets remain unverified and it is not a playable collectible yet. Do not replace Pip's assets with this fox.

Regenerate with Blender 4.5.3: `blender -b --python tools/import-meshy-fox.py -- /path/to/source.glb`, then run `python3 tools/sync-unity-meta.py`. Unity generates the initial material/prefab only if missing, preserving later edits.

Verified: pinned Unity 6000.6.5f1 batch import and script compilation; material/prefab creation; converted mesh/paint inspected in a Blender render; required portable and source checks; whitespace. Not verified: Unity rendered appearance, EditMode/PlayMode suites, Android build, phone appearance/performance and tactile interaction.

The owner project receives additive copies of the review assets/editor setup so its open Unity session can import them. Existing local settings and UI edits are preserved; the authoritative change is isolated in the branch above.
