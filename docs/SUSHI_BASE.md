# Sushi base — single-color maki

Owner-selected maki roll, created locally in Blender 4.5.3 on `codex/sushi-base` (ART-002 / UX-030). Meshy CLI/npm were unavailable; no Meshy task or paid generation was submitted.

Deliverables: `models/SushiBase.glb`, `models/SushiBase.fbx`, editable `ArtSource/SushiBase/SushiBase.blend`. All toy geometry uses exactly one pale cream (#EFE5D1) base material and no texture maps. Rounded roll, molded rice ring, three shallow filling forms; no face, painted seaweed, topping accessories or finish variants. The studio floor/cameras/lights are only in the rendered previews, excluded from the saved authoring model and exports.

The single closed connected skin has 6,001 authored vertices / 11,998 triangles, one UV layer, smooth normals, floor origin, and applied object transform. GLB material/mesh/texture/UV/triangle metadata verified independently from the authoring scene. Blender bmesh check confirms one connected component and zero non-manifold edges. Front, top and three-quarter renders inspected. Source/provenance and deterministic generator `tools/build-sushi-base.py` are retained for later shape changes. Full surface materials/color variations can share this base mesh and UVs.

Regenerate with Blender 4.5.3: `blender -b -t 4 --python tools/build-sushi-base.py`. This rebuilds only the owned SushiBase files. This is an art-review base; Unity catalog, deformability, collisions, rewards and phone performance have not been integrated or verified.
