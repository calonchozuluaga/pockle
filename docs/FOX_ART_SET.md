# Fox collection art set — ART-005 / UX-033 / UX-039

Owner selected Fox as the first complete collection art set. Existing stable ten finish IDs are preserved: nine common and Aurora (`fox.glow-jelly`) as the rare within those ten. The new owner-uploaded Fox v2 GLB replaces the shared base mesh/paint, fixing the tail. Source has 251,754 triangles; converted native runtime mesh has 20,141 UV-split vertices and 30,000 triangles. Existing asset GUIDs, ownership and finish recipes are preserved.

## Assets

| Deliverable | Location |
| --- | --- |
| Five unique Fox worlds | Resources/Worlds/fox-hollow, fox-riverbank, fox-autumn-orchard, fox-snowy-den, fox-aurora-grove (.png + baked .asset maps) |
| Text-free 3:1 cover | Assets/Pockle/Resources/Collections/Fox/Cover.png |
| Closed box transparent PNG | Assets/Pockle/Resources/UI/Boxes/Collection-fox.png |
| Open box transparent PNG | Assets/Pockle/Resources/UI/Boxes/Collection-fox-open.png |
| Opening-ready Unity model | Assets/Pockle/Resources/Boxes/FoxCollectionBox.prefab + FoxBox_Open.anim |
| Native interchange/source | models/FoxCollectionBox.glb; Assets/Pockle/Art/Models/FoxBox/FoxCollectionBox.fbx; ArtSource/FoxBox/FoxCollectionBox.blend |
| Exact image prompts | docs/FOX_ART_PROMPTS.md |
| Updated Fox source | models/Meshy_AI_vinyl_toy_red_fox_v2_1014202109_image-to-3d-texture.glb |

The cover is exactly 2172 × 724, independent of the world backgrounds, and includes Classic, Apricot and Pearl in the woodland. Worlds are LDR illustrated environments with 128px reflection cubes, not seamless HDR captures. Five moods: golden clearing, misty stream, autumn orchard, snowy dawn, dark aurora grove. The real box model is 1,253 triangles with hinged lid and two dust flaps; PNGs are renders of that same model. GLB contains the opening tracks; native Unity animation is 1.958 seconds and samples against the existing reveal pose. Motion calm limits opening amplitude. Regenerate with tools/build-fox-box.py; the owner-uploaded Fox conversion uses tools/import-meshy-fox.py.

## Runtime and Claude contracts

`CollectionDefinition.Worlds` is an immutable ordered list of `CollectionWorldDefinition { Id, DisplayName, EnvironmentArt }`. First is the Home world. Separate `CoverArt`, `BoxArt`, `BoxModel`, `AccentColor` and `InkColor` provide the approved collection presentation. Non-Fox art remains pending; no fabricated world sets are registered for other collections.

`PrototypeStage.SetEnvironment(Camera, string worldId, string collectibleId)` validates membership in the selected toy's collection and applies its background/reflections. Claude should switch the five-world picker to ID callbacks, retaining its saved selections. The old StageWorld API remains compatible. CoverArt should supply the banner separately from world thumbnails. Existing CollectionLook box naming already recognizes the closed PNG. Native opening is available via `MysteryBox.Initialize("fox")`; existing default Pip opening remains unchanged. Fox box pools/odds/purchases are not enabled by adding art.

Every toy now requires eye/mouth expression controls, specific subtle idles and touch reactions, calm alternatives and future sound-expression hooks (COLLECTION_ART_STANDARD.md). Pip already has face controls. Imported static Fox v2 art does not itself establish a finished facial/tail rig: that animation pass and Axolotl facial/limb reactions remain implementation work.

## Validation

Results recorded in VALIDATION.md and codex.md. Actual native model/ten finish captures inspected upright; both FoxArtSet runtime tests passed (five maps, collection membership, cover/box resources, real lid opening, calm). Isolated full PlayMode suite: 44/46, two previously reported Claude UI fixtures. Required portable/source checks passed. Claude's newer owner-local UI is preserved separately and must be verified in a combined build; no green CI or full device acceptance claim.
