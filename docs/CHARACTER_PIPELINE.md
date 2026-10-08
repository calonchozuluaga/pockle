# Character generation

Start with **Pip**, the original peach jelly companion, while the first Android prototype is being validated. Its successful desktop playtest establishes a useful interaction baseline; phone responsiveness and performance still need testing.

## 1. Design reference

Generate and review a front/side/back concept sheet with consistent proportions. Keep Pip recognizable: a rounded, slightly bottom-heavy seed body, two small unequal crown lobes, plum oval eyes, a tiny smile, peach-coral jelly, and restrained suspended pearls/flecks. The current procedural mesh is a placeholder; the concept sheet is visual guidance, not a game-ready mesh.

Keep a neutral rest pose and a simple silhouette. Small screens must preserve the face and identity. Establish the final material and expression before producing additional characters. All character designs should be original.

![Pip Peach Jelly concept 01](concepts/pip-peach-jelly-concept-01.png)

This generated draft establishes the peach color, rounded silhouette, and face. Its side view is slightly turned, so refine true orthographic proportions during modeling. Rendered transparency and reflections are visual targets; their mobile shader implementation still needs device validation.

## 2. Reusable Blender base

Build one clean, closed jelly mesh centered around the prototype's coordinate convention: local Y up, front negative Z, base at Y=-1. Keep transforms applied and facial anchors identified. Begin near the current geometry cost rather than spending triangles on detail that can live in materials. The current body has 1,073 vertices / 2,016 triangles; that is a reference budget, not a measured performance guarantee.

Use evenly distributed deformation-friendly topology with outward normals. Check maximum squash and stretch, crown intersections, facial attachment, UV seams, transparency sorting, and the body silhouette from every angle. Save the editable `.blend` source and export FBX for Unity. Generated 3D output must receive the same topology, transform, UV, and performance checks as an authored mesh.

## 3. Unity integration

Import into Unity 6000.6.5f1 using the Built-in Render Pipeline. The current `JellyToy` constructs geometry in code; importing an FBX does **not** automatically replace it. Add a renderer path that caches the imported mesh's rest vertices and facial anchors, uses the portable deformation function, updates normals without recurring managed allocations, and retains its shaders in player builds. Keep the procedural renderer available as a comparison until the imported asset passes validation.

Use a material instance for Pip's peach jelly. Tune translucency and internal accents on Android, including release builds. Prefer shared base geometry and material variants to unique high-cost meshes for every collectible. Character generation happens during asset production; touching a toy should remain entirely local.

## 4. Acceptance gate

Compare the generated/imported asset against the responsive procedural prototype on the same Android phone. Verify press, stretch, rebound, rotation, reset, reveal, settings, and lifecycle interruption. Profile frame time, allocations, draw calls, and memory; inspect the shader and face on the device. Follow `docs/VALIDATION.md` and retain the procedural version if the asset makes the interaction worse.

After one jelly character passes this gate, create one plush and one vinyl base with distinct material responses. Expand toward the suggested 12-collectible roster by reusing those three bases. The collecting beta, editions, evolution, and trading remain separate milestones.
