# Pockle

Unity source for the **first tactile prototype**: one original peach jelly companion, Pip, now with a Blender-authored art pass based on the approved design. The project follows the [development brief](docs/PRODUCT_BRIEF.md).

Pip can be pressed, dragged upward to stretch, released to rebound, and rotated. A short procedural box reveal introduces the toy. Reset restores its pose. Sound, optional device vibration, and reduced motion are configurable; those settings persist locally.

The authored character is an initial art pass; its Unity material and Android appearance still need validation. The original procedural character remains available for comparison. Audio tones, reveal box, and UI are prototype content. There are no collection rewards, purchases, accounts, trades, walking mechanics, or AI calls during gameplay.

For the next art step, follow the [character generation pipeline](docs/CHARACTER_PIPELINE.md): establish Pip's design reference, build a reusable Blender mesh, and validate its Unity deformation and materials on Android before expanding the roster.

## Open locally

1. Use your installed **Unity 6.6, version `6000.6.5f1`**. That exact version is recorded in `ProjectSettings/ProjectVersion.txt`. Before building for a phone, install that editor's Android or iOS build-support modules and check its [platform requirements](https://docs.unity3d.com/6000.6/Documentation/Manual/system-requirements.html), including Xcode for iOS.
2. This prototype uses the **Built-in Render Pipeline**, with UGUI 2.6.0 matching [Unity's official 6000.6 package source](https://github.com/Unity-Technologies/uGUI/blob/15824acdaa856cd70780e8fc06a86025ea70d1cb/com.unity.ugui/package.json). Editor compilation and package resolution remain unverified in cloud.
3. In Unity Hub, choose **Add project from disk** and select this directory with `6000.6.5f1`. Let Unity resolve the declared UGUI and Test Framework packages. Use the existing checkout; each Codex cloud task is already isolated, so a separate Git worktree is unnecessary.
4. Choose **Pockle → Prepare prototype settings**, then **Pockle → Open tactile prototype**, and press **Play**. The scene generates its character, presentation, and UI at runtime. Use the Built-in pipeline and **Input Manager (Old)** input handling. Both are the supplied defaults.

Try Game view sizes for a tall phone, a small phone, a portrait tablet, and a landscape tablet. Menu controls use safe-area layout. See [device validation](docs/VALIDATION.md) before calling the milestone playable on a phone.

## Try Pip's authored art

After the first APK finishes, preserve it as your baseline and pull the latest repository changes. Unity imports `Pip.pocklemesh` into a native readable mesh and character asset automatically; Blender is not required to open or build the game. Choose **Pockle → Character → Use authored Pip**, then restart Play. The Console should report **Pip: authored mesh · 3158 vertices**. Choose **Use procedural baseline** before the next Play session to compare.

The editable source is `ArtSource/Pip/Pip.blend`; the standard interchange export is `Assets/Pockle/Art/Pip/Pip.fbx`. The runtime uses the custom source's imported native mesh and face anchors so coordinates, scale, UV seams, and deformation remain explicit. The FBX includes the complete character for further art work. The [Blender model study](docs/concepts/pip-model-study-02.png) shows geometry and an offline material; it does not establish the Unity shader's appearance.

## Update an existing checkout

Close Unity and run `git pull --ff-only` from your local `pockle` directory. Reopen that same project and let Unity finish importing. Select **Pockle → Character → Use authored Pip**, then enter Play mode and check for **Pip: authored mesh · 3158 vertices** before rebuilding Android.

The second art pass joins the crown and body into one closed deformable shell, broadens the flat foot, and shortens the crown. The Unity material adds broad studio reflections and glazed plum eyes; blush now has soft edges. Internal pearls render beneath the shell. The UI no longer labels the authored character a procedural placeholder. The screenshot confirmed the first authored import on the owner's computer; the revised shader and mesh still need local visual and Android performance checks. The approved concept is the visual target, rather than an exact render of the Unity material.

The latest viewer update uses a small static studio cubemap for bright softbox reflections across the jelly shell. Reflections become opaque while the peach tint stays translucent; this approximates clear gel without scene refraction. A thin plate collider and pointer-down checks against the visible deformed shell keep turning and squishing separate. Local Unity/Android testing is still required.

If Git reports that local changes would be overwritten, inspect the listed files before restoring or stashing anything; preserve local Android settings and package changes. This art update changes neither `Packages/manifest.json` nor `ProjectSettings`.

## Controls

| Action | Touch / mouse |
| --- | --- |
| Squish | Press and hold Pip directly |
| Stretch / lean | Keep holding; drag up / sideways |
| Rebound | Release |
| Inspect | Drag the exposed plate sideways to rotate Pip and the plate |
| Start over | **Reset**, or `R` in the editor |
| Reveal | **Reveal again**, or `Space` in the editor |
| Comfort | Toggle **Sound**, **Haptics**, and **Motion** |

The starting surface selects the gesture: Pip squishes; the plate turns. A gesture stays assigned until release, even when dragged across the other surface. Rotation stops immediately on release. Small rim marks show the plate turning. The Touch/Rotate mode buttons have been removed.

Reduced motion preserves direct manipulation but removes the spring rebound and large reveal movements. Sound is local synthesized placeholder audio. Haptics default off and use Unity's basic device vibration; finely tuned native haptics are future work. UI touches do not manipulate the toy, and only one finger controls an active gesture.

## Checks available without Unity

Install .NET SDK 8, then run from the repository root:

```sh
dotnet run --project Tests/Pockle.Core.Checks/Pockle.Core.Checks.csproj
dotnet run --project tools/Pockle.SourceChecks/Pockle.SourceChecks.csproj -- .
```

In the prepared cloud workspace, the verified SDK is at `/workspace/.tools/dotnet/dotnet`. These commands test the actual engine-independent deformation/spring code and check C# syntax and asset GUID references. **They do not compile Unity APIs, render shaders, import the Unity scene, or validate touch on a device.**

In Unity, open **Window → General → Test Runner → EditMode** and run `Pockle.Core.EditMode.Tests`. Then follow [the manual device checklist](docs/VALIDATION.md).

## Source map

- `Assets/Pockle/Runtime/Core/`: portable analytic springs and volume-conscious deformation.
- `Assets/Pockle/Runtime/JellyToy.cs`: authored or procedural mesh, deformed facial features, and materials.
- `Assets/Pockle/Runtime/PipCharacterAsset.cs`: native mesh and authored feature anchors.
- `Assets/Pockle/Runtime/TactilePrototype.cs`: pointer capture, reveal, feedback, and preferences.
- `Assets/Pockle/Runtime/PrototypeHud.cs` / `PrototypeStage.cs`: responsive controls and presentation.
- `Assets/Pockle/Shaders/`: simple Built-in candy, accent, and reveal shaders.
- `Assets/Pockle/Resources/`: imported character source and material references that retain shaders in player builds.
- `Assets/Pockle/Scenes/PockleTactile.unity`: committed scene entry point.
- `Assets/Pockle/Editor/`: setup menu and build checks.

The authored body has 3,158 vertices and 5,814 triangles; the original baseline has 1,073 vertices and 2,016 triangles. Features reuse small meshes and materials. Deformation uses reused vertex/normal buffers, welds UV-seam normals, and skips idle updates; actual CPU cost, draw calls, frame rate, and transparency artifacts still need profiling on a midrange phone. See [architecture notes](docs/ARCHITECTURE.md).

## Current collecting direction

The owner has simplified the collecting loop to daily collection-themed boxes unlocked by walking, plus optional paid boxes (proposed price points: $0.99 standard and $2.99 special). Packaging identifies the collection while color/variant remains a surprise. These are product decisions recorded in [the brief](docs/PRODUCT_BRIEF.md), not implemented rewards, location tracking, or purchases. Lights-off glow and squeeze-reactive internal stars/pearls are also planned after the current viewer test.
