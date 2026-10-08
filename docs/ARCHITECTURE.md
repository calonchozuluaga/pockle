# Prototype architecture

`PockleTactile.unity` contains a single `PrototypeBootstrap`. At play, it creates the controller; that controller creates the scene presentation, procedural Pip, HUD, reveal box, and synthesized audio. Keeping content in source lets Codex work on the empty repository without creating binary meshes or depending on an unavailable Unity editor.

The Built-in rendering pipeline is intentional. The three custom shaders use one simple pass each and no screen grabs, AI requests, post-processing stack, or real-time shadow maps. Resources materials retain those shaders for builds because `Shader.Find` alone does not reliably preserve a shader. A future art pipeline can replace the procedural renderer while keeping the interaction core.

The selected editor is Unity 6.6, pinned to the owner's installed version `6000.6.5f1`. UGUI is pinned to 2.6.0, matching Unity's official 6000.6 source. Its `UnityEngine.UI` assembly name is retained, and the UI Elements module is declared for that assembly's reference. The bootstrap uses Unity 6's `FindFirstObjectByType` API. These source/package updates still require package resolution and compilation in the actual editor.

`Pockle.Core` has no engine dependency. `Spring1D` integrates an analytic damped oscillator instead of frame-sensitive Euler steps. Four independent springs control compression, stretch, and lateral lean; release overshoot maps gently into the opposing deformation. The portable geometry function anchors the bottom, scales horizontal dimensions against the vertical change, and adds a localized pressure dent. This approximates bulk conservation rather than simulating a physical jelly volume.

The Unity renderer caches rest coordinates and mutable mesh arrays. Deformed normals are rebuilt into reused buffers; idle deformation is skipped. Facial anchors, crown lobes, and suspended accents use the same deformation mapping as the body. No network request occurs during touch. The first active pointer retains control until release/cancel; UI hits are excluded and lifecycle interruptions release the gesture.

`PrototypeHud` adapts its safe area to orientation and insets. `PrototypeStage` places the viewer in the left portion of landscape screens and keeps controls on the right. Sound and haptics are separate preferences; reduced motion retains direct manipulation while disabling spring rebound and large reveal transforms. Only these preferences use `PlayerPrefs`; no identity, rarity, or scarcity claim is made by local storage.

The editor menu configures the prototype's scene and presentation defaults. Build preprocessing guards against a missing scene, render pipeline mismatch, or missing custom shaders. It is not a substitute for an editor build or a real-device check.

The next milestone should introduce a plush and vinyl renderer with different responses behind a shared manipulation interface. Collections, reward pacing, backend inventories, fixed-edition serials, and trades remain outside this tactile sandbox.
