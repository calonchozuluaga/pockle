# Approved Peach expressions and translucent gloss

October 13, 2026 — UX-025/030, ART-002; `codex/peach-expressions`.

Only the final owner-approved Meshy Peach (`pip.peach-jelly`) receives this change. Imported geometry, UVs and paint are unchanged. Localised shader deformation animates the existing painted eyes and smile: a short blink every 5.1 seconds, contented squint while squeezed, and an alert lifted expression. Shader feature centres follow the existing CPU squish deformation. Motion calm produces the neutral face; app pause freezes expression time. Other Pip models are unchanged.

The textured jelly shader now uses front/back alpha-blended shell passes, default opacity 0.62, stronger moving studio reflections and opaque wet highlights. Dark face pigment remains readable. Six real interior cream pearls and six gold flecks draw before the shell, follow the existing deformation and are tinted by the jelly in front of them. There is no scene-copy/refraction pass or runtime image generation. Opacity and reflection strength remain shader properties for future material tuning.

Renders inspected in Unity 6000.6.5f1: [before](captures/peach-expressions/before.png), [neutral](captures/peach-expressions/neutral.png), [blink](captures/peach-expressions/blink.png), [squint](captures/peach-expressions/happy.png), [lifted](captures/peach-expressions/surprised.png), [turned reflection](captures/peach-expressions/turned.png). The capture helper sets a fixed expression and camera, and restores the previous Meshy preference. Portable checks cover blink timing, periodicity, bounded invalid inputs, calm and reaction priority. Full PlayMode suite: 28/28 passed, including preserved imported mesh, presence of contents, material settings, variant switching and calm expressions.

Cost: shell grows from one pass to two, with twelve interior renderers sharing two small accent meshes and a material per kind. This is a bounded local rendering change; midrange Android frame time, transparency sorting and finger picking near animated facial patches still need owner device review. The existing CPU body surface continues to handle picking; shader-only face motion is a small visual offset. The uploaded source mesh is not changed to chase an unverified phone look.

Not verified: Android shader build and phone performance/appearance. A new APK is needed to include the changes. No inventory/save/settings/package changes in this Peach topic.

Final checks: Unity 6000.6.5f1 PlayMode 28/28 and EditMode 16/16 passed; portable/source checks passed; fixed captures inspected. Other Pip models and uploaded source mesh/paint unchanged. Android build/device frame time remains pending.

October 14 update: owner approved the latest Pip shape for all ten finishes. The expression driver and painted face now extend to the shared collection model, with finish-specific materials and filling. Earlier Peach-only scope is superseded by LATEST_PIP_COLLECTION.md.
