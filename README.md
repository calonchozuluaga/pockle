# Pockle

Unity source for the **first tactile prototype**: one original peach jelly companion, Pip. The project follows the [development brief](docs/PRODUCT_BRIEF.md).

Pip can be pressed, dragged upward to stretch, released to rebound, and rotated. A short procedural box reveal introduces the toy. Reset restores its pose. Sound, optional device vibration, and reduced motion are configurable; those settings persist locally.

The character mesh, materials, audio tones, reveal box, and UI are **procedural placeholders**, not final production art or sound. There are no collection rewards, purchases, accounts, trades, walking mechanics, or AI calls in this milestone.

For the next art step, follow the [character generation pipeline](docs/CHARACTER_PIPELINE.md): establish Pip's design reference, build a reusable Blender mesh, and validate its Unity deformation and materials on Android before expanding the roster.

## Open locally

1. Use your installed **Unity 6.6, version `6000.6.5f1`**. That exact version is recorded in `ProjectSettings/ProjectVersion.txt`. Before building for a phone, install that editor's Android or iOS build-support modules and check its [platform requirements](https://docs.unity3d.com/6000.6/Documentation/Manual/system-requirements.html), including Xcode for iOS.
2. This prototype uses the **Built-in Render Pipeline**, with UGUI 2.6.0 matching [Unity's official 6000.6 package source](https://github.com/Unity-Technologies/uGUI/blob/15824acdaa856cd70780e8fc06a86025ea70d1cb/com.unity.ugui/package.json). Editor compilation and package resolution remain unverified in cloud.
3. In Unity Hub, choose **Add project from disk** and select this directory with `6000.6.5f1`. Let Unity resolve the declared UGUI and Test Framework packages. Use the existing checkout; each Codex cloud task is already isolated, so a separate Git worktree is unnecessary.
4. Choose **Pockle → Prepare prototype settings**, then **Pockle → Open tactile prototype**, and press **Play**. The scene generates its character, presentation, and UI at runtime. Use the Built-in pipeline and **Input Manager (Old)** input handling. Both are the supplied defaults.

Try Game view sizes for a tall phone, a small phone, a portrait tablet, and a landscape tablet. Menu controls use safe-area layout. See [device validation](docs/VALIDATION.md) before calling the milestone playable on a phone.

## Controls

| Action | Touch / mouse |
| --- | --- |
| Squish | Press and hold Pip in **Touch** mode |
| Stretch / lean | Keep holding; drag up / sideways |
| Rebound | Release |
| Inspect | Choose **Rotate**, then drag Pip sideways |
| Start over | **Reset**, or `R` in the editor |
| Reveal | **Reveal again**, or `Space` in the editor |
| Comfort | Toggle **Sound**, **Haptics**, and **Motion** |

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
- `Assets/Pockle/Runtime/JellyToy.cs`: procedural mesh, deformed facial features, and materials.
- `Assets/Pockle/Runtime/TactilePrototype.cs`: pointer capture, reveal, feedback, and preferences.
- `Assets/Pockle/Runtime/PrototypeHud.cs` / `PrototypeStage.cs`: responsive controls and presentation.
- `Assets/Pockle/Shaders/`: simple Built-in candy, accent, and reveal shaders.
- `Assets/Pockle/Resources/`: small material references that retain runtime-resolved shaders in player builds.
- `Assets/Pockle/Scenes/PockleTactile.unity`: committed scene entry point.
- `Assets/Pockle/Editor/`: setup menu and build checks.

The body has 1,073 vertices and 2,016 triangles. Features reuse small meshes and materials. Deformation uses reused vertex/normal buffers and skips idle updates; actual CPU cost, draw calls, frame rate, and transparency artifacts still need profiling on a midrange phone. See [architecture notes](docs/ARCHITECTURE.md).
