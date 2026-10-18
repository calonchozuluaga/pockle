# Painted worlds and emitted light — UX-039

Three original static painted backgrounds, generated with the built-in imagegen tool:

| World control | Image asset | Look |
| --- | --- | --- |
| Home with Fox selected | Assets/Pockle/Resources/Worlds/fox-hollow.png | Golden woodland clearing |
| Jelly Garden | Assets/Pockle/Resources/Worlds/jelly-garden.png | Pink hills and flowering garden |
| Midnight | Assets/Pockle/Resources/Worlds/moonlit-pond.png | Dark woodland pond and moonlight |

Studio and collections without Home art keep their existing gradient. Original PNGs are 1774 × 887; exact prompts are in [PAINTED_WORLD_PROMPTS.md](PAINTED_WORLD_PROMPTS.md). Images have a responsive portrait crop; Midnight pans slightly right to retain the moon. These assets represent painted panoramas, not verified seamless HDR captures.

`PaintedWorldImport.Build` bakes a 128-pixel mipmapped cubemap per image, retaining the original PNG and stable asset GUIDs when rebuilding. `PaintedWorldAsset` binds the image and cube. `ToyStageEnvironment` applies the baked cube to the selected Pip/Fox material and a local custom reflection probe for Standard materials (Classic Fox). No runtime environment capture occurs. Existing shelf portrait materials retain their own studio reflections. Opaque/soft finishes keep their opacity; translucent shells sample a refracted environment direction with an approximate index of 1.35. This is environment refraction, not ray-traced thickness or caustics; the actual filling remains visible geometry.

Glow finishes emit softly from the body, surrounded by an additive halo and colored plate light spill. Night increases these effects. Glitter finishes have subtler warm spill at Night and view-dependent glitter highlights. Two small additive cards emulate illumination without post-processing, dynamic lights or real-time shadows. The effects clear on normal-toy selection. They do not pulse or flash, so Motion calm remains quiet.

## Checks

Real Unity import and fixed renders are required to verify texture import, shader support and appearance. World/light tests check map binding, ordinary/rare/glitter switches, bounded geometry, collision and cleanup. Device appearance and frame time remain owner review. Two previously reported Claude UI failures remain separate; do not describe the combined suite or CI as green.
