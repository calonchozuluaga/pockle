# Axolotl complete collection

October 14, 2026. Branch codex/axolotl-art-set. Approved uploaded Axolotl mesh/paint reused for all ten stable IDs; 30,000 triangles, 48,940 UV-split vertices. Original Blossom Plush remains axolotl.boucle-plush. Nine new local material recipes complete the collection. Exactly one rare: axolotl.glow-jelly, Biolume Glow · Ultra rare. No box odds, purchases, public release schedule or save schema change.

| Finish | ID suffix |
| --- | --- |
| Honey Jelly | clear-jelly |
| Pearl | pearl-jelly |
| Rose Glitter | confetti-gel |
| Mint Mochi | mochi-foam |
| Lavender Flock | velvet-flock |
| Blossom Plush | boucle-plush |
| Sky Matte | matte-vinyl |
| Coral Gloss | gloss-vinyl |
| Copper | coated-metallic |
| Biolume Glow · Ultra rare | glow-jelly |

All preserve the uploaded face and three gills each side. Local recipes simulate translucency, pearl, glitter, fuzzy pile, matte/gloss and copper. Baked fur details/gill geometry remain on every finish; this is not strand-fur simulation. Shaders use stage reflection/refraction maps. Night glow uses the existing bounded halo and plate-light cards, not scene-wide dynamic lighting.

Five painted aquatic worlds with separate 128px reflection cubes: axolotl-blossom-pond (Home), axolotl-mint-creek, axolotl-pearl-cove, axolotl-sunset-marsh, axolotl-biolume-lagoon (dark). LDR paintings approximate reflection environments; they are not full spherical HDR captures.

Separate text-free 2172×724 cover: Resources/Collections/Axolotl/Cover. Actual 874-triangle native opening carton: Resources/Boxes/AxolotlCollectionBox; 13 renderers, real lid/two flap hinges, 1.958s opening clip, calm amplitude. Branded mystery print maps preserve surprise. Transparent closed/open 1024px PNGs: Resources/UI/Boxes/Collection-axolotl and Collection-axolotl-open. Blender source ArtSource/AxolotlBox/AxolotlCollectionBox.blend, FBX Art/Models/AxolotlBox/AxolotlCollectionBox.fbx; reproducible generated GLB models/AxolotlCollectionBox.glb. tools/build-axolotl-box.py and Editor AxolotlArtSetBuild.Build rebuild carton assets.

AxolotlExpressionDriver supplies periodic blink, happy/compression and surprised/lift mouth changes plus subtle gill/arm wave. Painted facial areas deform locally in the shader; neutral paint and CPU touch geometry stay intact. Calm suppresses these motions; app pause freezes them. React(PeachExpressionPose, seconds) provides a bounded three-second expression hook for later sound/animation events. No new character sound recordings or skeletal rig added.

Owner award: Resources/AxolotlOwnerTestGrant.txt enabled, one-time marker pockle.owner-test.axolotl-finishes.v1. EnsureAxolotlFinishTestCopies fills only missing available IDs, preserves prior copies, Pip/Fox inventory, steps and pending reveal. CollectionSession persists before setting marker or invoking Changed; batch-mode startup skips the grant and unknown/newer saves remain read-only. Disable/remove the test resource before public distribution. A phone must run the updated APK for this award; editor assets alone do not change the phone save.

Claude UI automatically discovers the box PNG by convention. Five-world picker and separate cover presentation are still Claude work; Home stage supports the new pond. Claude latest six-spot live-shelf decision is acknowledged as presentation preference; current local HUD still uses four fixed spots. Shelf background delivered separately, see LIVE_SHELF_ART.md. StageViewport runtime easing remains a separate request.

Review every world, native box, ten actual Unity finish captures and expression states in AXOLOTL_ART_REVIEW.html. Exact built-in imagegen prompts are in AXOLOTL_ART_PROMPTS.md.
