# Owner-uploaded Peach Jelly phone test

Branch: `codex/meshy-peach-test`, based on main `1d9c1257`. Unity **6000.6.5f1**, Built-in RP. Boxes, animated shelf, and missing silhouettes from main are included.

## Run on a phone

```sh
git fetch origin
git switch codex/meshy-peach-test
git pull --ff-only origin codex/meshy-peach-test
```

Open Unity 6000.6.5f1 and rebuild/install Android using your existing local build settings. Open **Shelf → Pip → Peach Jelly** (or the owned Peach tile) and enter play. Meshy Peach is the default art for the existing `pip.peach-jelly` collectible in this branch. It does not create a collectible or grant inventory. Moon, Gold, and Mint retain the prior authored model.

Compare front/side appearance, painted face readability, plate contact, press/release, two-finger stretch, lift/sag, rotation, and frame rate while touching. Switch Peach → Moon → Peach and check responsiveness. Motion calm and animated shelf previews use the normal paths.

In the Unity Editor, compare **Pockle → Character → Use authored Pip** and **Use Meshy Peach phone test** on the next Play session. Editor preferences do not transfer to Android; the phone defaults to Meshy without an Editor toggle. An existing procedural-baseline developer override takes precedence. Delete only that developer preference if deliberately enabled, rather than clearing your collection save.

## Asset and material

Owner upload: `Meshy_AI_Peach_Jelly_Pip_Smoot_1012213852_image-to-3d-texture.glb`. SHA-256/conversion details: `ArtSource/Pip/Meshy/PROVENANCE.json`. Keep the original GLB outside Assets; it is not re-added as an unused 18 MB runtime resource. Reproduce with:

```sh
blender -b --python tools/import-meshy-peach.py -- /path/to/the/original.glb
python tools/sync-unity-meta.py
```

The uploaded **64,102 triangles** become **3,845 triangles / 3,951 UV-split vertices**, with a 2048² sRGB base texture, compressed at import and mipmapped. Conversion keeps the owner's silhouette, aligns the bottom at local y=-1 and crown at y=.94, preserves UVs, and reverses winding for Unity coordinates. It does not flatten/redesign the base. Mesh/normals use existing CPU deformation and visible-shell picking. Face/filling is painted onto this mesh, so extra authored eyes, crown and pearls are omitted.

`Pockle/Textured Peach Jelly` is a retained single-pass Built-in shader: roughness **.20**, transmission-light approximation **.90**, peach scattering weight **.05**, restrained moving reflections, and small surface emission on gold texture flecks. Opaque depth keeps the painted face readable. Blender's ray-traced transmission/SSS do not transfer directly; this uses wrapped light/backlight, with no scene-color grab/refraction, extra shell, package or postprocessing. Baked pearls/highlights remain surface details. Uploaded metallic/roughness and normal maps are omitted in favor of constant wet gloss and deforming mesh normals.

## Verification

Local: all **543,484** existing portable assertions pass; reduced geometry is finite, outward, correctly indexed, nondegenerate and within the 6,000-triangle budget. Source checks: **69 files / three profiles / five asmdefs / 154 GUIDs / zero failures**. Meta/whitespace checks pass. Added Unity coverage for native import/material retention, default phone selection, no duplicate accents, pose-preserving art switches, reset/cleanup, and actual front/quarter/grip/reference captures.

Hosted CI [38315430626](https://github.com/calonchozuluaga/pockle/actions/runs/38315430626) verified code `79cc98bb`: **41/41 Unity tests** (15 EditMode, 26 PlayMode, no skips), asset/texture import, supported shader, pose switches, picking/reset/cleanup, and actual front/quarter/grip/reference renders on llvmpipe software graphics. [Full capture artifact](https://github.com/calonchozuluaga/pockle/actions/runs/38315430626/artifacts/11776732740). The initial 5,410-vertex mesh was reduced to meet the existing 4,000-vertex limit; its failed run is not the final result.

Android packaging is being requested with the repository's `build-apk` label in [PR #15](https://github.com/calonchozuluaga/pockle/pull/15). This documentation-only follow-up preserves the tested asset/runtime/shader. Final packaging/merge status is recorded on that PR. Phone visuals, performance, Android drivers and tactile acceptance remain owner checks.
