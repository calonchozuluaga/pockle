# Pip collection art set

October 14, 2026 — original paintings generated using built-in imagegen; exact prompts in PIP_ART_PROMPTS.md. Approved latest Pip model and all ten finishes retained, including Starlight Glow as the single rare.

| World | Resource ID |
| --- | --- |
| Peach Jelly Garden (home) | pip-jelly-garden |
| Mint Spring | pip-mint-spring |
| Pearl Lagoon | pip-pearl-lagoon |
| Golden Meadow | pip-confetti-meadow |
| Starlight Grove (dark) | pip-starlight-grove |

Each has its own painted PNG and baked 128px reflection cubemap. Painted scenes are LDR scenic backplates, not full spherical HDR captures; environmental reflections/refraction use the existing local stage shaders.

Cover: Resources/Collections/Pip/Cover.png, exactly 2172 × 724 (3:1), no lettering or overlays, approved Peach/Mint/Pearl Pip identity.

Carton: Resources/Boxes/PipCollectionBox.prefab; genuine 874-triangle open-top model, 13 renderers, independent lid and two dust flaps, 1.958s opening clip. Source ArtSource/PipBox/PipCollectionBox.blend, generated GLB models/PipCollectionBox.glb, Unity FBX Art/Models/PipBox/PipCollectionBox.fbx. Build tools/build-pip-box.py in Blender; assemble via Pockle.Editor.PipArtSetBuild.Build. Printed mystery silhouette and Pockle wordmark follow approved surprise-box reference. Closed/open transparent PNGs: Resources/UI/Boxes/Collection-pip.png and Collection-pip-open.png, 1024px. The default Pip reveal now loads this native carton, retaining procedural fallback and calm opening.

Catalog provides HomeWorld, five Worlds, CoverArt, BoxArt and BoxModel. Claude's Collections UI discovers the box PNG automatically. The five-world picker and separate cover presentation require Claude's UI integration; existing Home choice loads Peach Jelly Garden. No grant, save migration or reward odds change.

Pip's existing blink and touch expressions remain; this art task adds no character rig or sound files. Phone visual/performance acceptance and combined UI suite remain unverified.
