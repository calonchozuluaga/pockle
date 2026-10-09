# Pockle

Unity source for Pockle's **Home hub UI beta**. Home opens with Collection, Rewards, and Friends tiles; the bottom tabs are Home, Boxes, and You. Peach Jelly, Moon Jelly, Gold Glitter, and Mint Soft each have their own toy on the shelf. Tap one to play; direct squish, lift, two-finger stretch, plate rotation, and phone-motion jiggle remain. The prototype control card is gone. Full Settings includes volume and comfort preferences; You has an editable local name, Pip avatar, favorite toy, and collection summary. See [the Home hub guide](docs/HOME_UI.md).

Lifting now has [weight and gravity](docs/WEIGHT_AND_GRAVITY.md): Pip sags below the grip and follows the hand with slight lag. Release makes him fall, squash, jiggle, and settle, with stronger feedback for higher drops. Motion calm keeps direct control without those extra movements.

**Boxes** offers a daily Jelly Garden box earned by **1,000 steps**, plus three proposed paid offers. Supported Android phones use the hardware step counter with Physical activity permission. Daily progress, local ownership, and interrupted box reveals persist. This beta starts with all four toys so each finish remains testable.

Buy actions currently explain that checkout is unavailable; they cannot charge or grant toys. Real billing, authoritative reward/inventory validation, location checks, and guaranteed background walking are not connected. See [the collection UI and test guide](docs/COLLECTION_UI.md) for the exact behavior and next integration steps.

The project uses the Built-in Render Pipeline and Unity **6000.6.5f1**. Cloud checks verify portable logic, C# syntax/assets, and Android Java compilation; Unity rendering, plugin packaging, and device behavior need your local editor and phone.

The [UI/UX task tracker](docs/UI_UX_TASKS.md) tracks Home, Settings, profiles, friends, visited shelves, rewards, badges, and milestones, with stable IDs and acceptance criteria. The owner has also expanded the content target to **12 characters with 8–10 material varieties each**; the [roster proposal](docs/CHARACTER_ROSTER.md) outlines twelve silhouettes, nine finish families, and catalog/migration work. These additional models and finishes are planned; the current build still has four Pip finishes.

## Open locally

1. Use your installed **Unity 6.6, version `6000.6.5f1`**. That exact version is recorded in `ProjectSettings/ProjectVersion.txt`. Before building for a phone, install that editor's Android or iOS build-support modules and check its [platform requirements](https://docs.unity3d.com/6000.6/Documentation/Manual/system-requirements.html), including Xcode for iOS.
2. This prototype uses the **Built-in Render Pipeline**, with UGUI 2.6.0 matching [Unity's official 6000.6 package source](https://github.com/Unity-Technologies/uGUI/blob/15824acdaa856cd70780e8fc06a86025ea70d1cb/com.unity.ugui/package.json). Editor compilation and package resolution remain unverified in cloud.
3. In Unity Hub, choose **Add project from disk** and select this directory with `6000.6.5f1`. Let Unity resolve the declared UGUI and Test Framework packages. Use the existing checkout; each Codex cloud task is already isolated, so a separate Git worktree is unnecessary.
4. Choose **Pockle → Prepare prototype settings**, then **Pockle → Open tactile prototype**, and press **Play**. The scene generates its character, presentation, and UI at runtime. Use the Built-in pipeline and **Input Manager (Old)** input handling. Both are the supplied defaults.

Try Game view sizes for a tall phone, a small phone, a portrait tablet, and a landscape tablet. Menu controls use safe-area layout. See [device validation](docs/VALIDATION.md) before calling the milestone playable on a phone.

## Try Pip's authored art

After the first APK finishes, preserve it as your baseline and pull the latest repository changes. Unity imports `Pip.pocklemesh` into a native readable mesh and character asset automatically; Blender is not required to open or build the game. Choose **Pockle → Character → Use authored Pip**, then restart Play. The Console should report **Pip: authored mesh · 3158 vertices**. Choose **Use procedural baseline** before the next Play session to compare.

The editable source is `ArtSource/Pip/Pip.blend`; the standard interchange export is `Assets/Pockle/Art/Pip/Pip.fbx`. The runtime uses the custom source's imported native mesh and face anchors so coordinates, scale, UV seams, and deformation remain explicit. The FBX includes the complete character for further art work. The [Blender model study](docs/concepts/pip-model-study-02.png) shows geometry and an offline material; it does not establish the Unity shader's appearance.

On the shelf, tap **Peach Jelly**, **Moon Jelly**, **Gold Glitter**, or **Mint Soft** to play with that toy. Peach keeps the glossy warm shell and small pearls/flecks. Moon uses blue/lavender pearl sheen, larger icy pearls, and silver stars. Gold Glitter adds dense shaded gold microflakes. Mint Soft uses an opaque pastel shell with subdued highlights. See [variant comparison](docs/PIP_VARIANTS.md) for local tests; the new finishes still need Unity/Android visual validation.

## Update an existing checkout

Close Unity and run `git pull --ff-only` from your local `pockle` directory. Reopen that same project and let Unity finish importing. Select **Pockle → Character → Use authored Pip**, then enter Play mode and check for **Pip: authored mesh · 3158 vertices** before rebuilding Android.

The second art pass joins the crown and body into one closed deformable shell, broadens the flat foot, and shortens the crown. The Unity material adds broad studio reflections and glazed plum eyes; blush now has soft edges. Internal pearls render beneath the shell. The UI no longer labels the authored character a procedural placeholder. The screenshot confirmed the first authored import on the owner's computer; the revised shader and mesh still need local visual and Android performance checks. The approved concept is the visual target, rather than an exact render of the Unity material.

The latest viewer update uses a small static studio cubemap for bright softbox reflections across the jelly shell. Reflections become opaque while the peach tint stays translucent; this approximates clear gel without scene refraction. A thin plate collider and pointer-down checks against the visible deformed shell keep turning and squishing separate. Local Unity/Android testing is still required.

If Git reports that local changes would be overwritten, inspect the listed files before restoring or stashing anything; preserve local Android settings and package changes. The collection update adds Unity's built-in Android JNI module in `Packages/manifest.json`; it leaves `ProjectSettings` unchanged. See the [collection guide](docs/COLLECTION_UI.md) for preserving local package changes if Git blocks the pull.

## Controls

| Action | Touch / mouse |
| --- | --- |
| Squish | Press and hold Pip directly |
| Lift / lean | Hold Pip; drag up / sideways |
| Stretch / compress | Start both touches on Pip; pull apart / pinch together |
| Jiggle | Gently move or shake the phone with **Motion · full**; `J` previews a pulse in the editor |
| Rebound | Release |
| Inspect | Drag the exposed plate sideways to rotate Pip and the plate |
| Pick a toy | **Home → Collection**, then tap its shelf entry |
| Browse / earn boxes | Tap **Boxes**, or **Home → Rewards** |
| Start over | `R` in the editor only |
| Reveal | Open an earned box; `Space` previews in the editor |
| Comfort | Open **Settings** for mute/volume, haptics, motion, and walking status |
| Profile | Tap **You** to set a local name, avatar, and owned favorite |
| Friends | **Home → Friends** previews the Discover/Friends routes; online services are pending |

The starting surface selects the gesture: Pip squishes/lifts; the plate turns. A second touch that starts on Pip can join a toy gesture; it cannot change a plate gesture into a pinch. Removing either finger hands control to the survivor with a rebased drag. Rotation stops immediately on release. Small rim marks show the plate turning. The Touch/Rotate mode buttons have been removed.

Reduced motion preserves direct lifting and two-finger deformation but removes spring rebound, phone-motion jiggle, and large reveal movements. Sound is local synthesized placeholder audio. Haptics default off and use Unity's basic device vibration; finely tuned native haptics are future work. UI touches do not manipulate the toy. See [the new interaction checklist](docs/MANIPULATION_TEST.md) for Android testing.

## Working on this repo

Codex and Claude both contribute through pull requests; [AGENTS.md](AGENTS.md) holds the shared rules and who owns which area. GitHub Actions runs the checks below, the Unity test suites, and an Android build; see [CI](docs/CI.md), including the one-time Unity license setup.

## Checks available without Unity

Install .NET SDK 8, then run from the repository root:

```sh
dotnet run --project Tests/Pockle.Core.Checks/Pockle.Core.Checks.csproj
dotnet run --project tools/Pockle.SourceChecks/Pockle.SourceChecks.csproj -- .
```

In the prepared cloud workspace, the verified SDK is at `/workspace/.tools/dotnet/dotnet`. These commands test the actual engine-independent collection/reward and deformation/spring code and check C# syntax and asset GUID references. **They do not compile Unity APIs, render shaders, import the Unity scene, or validate touch on a device.**

In Unity, open **Window → General → Test Runner → EditMode** and run `Pockle.Core.EditMode.Tests`. Under **PlayMode**, run `Pockle.Variants.PlayMode.Tests` to check variants and the collection UI: separate shelf portraits, toy selection, menu/play Back and scroll, profile persistence, sound volume, unavailable checkout, daily unlocks, and repeated claim protection. Both Unity suites remain unrun in cloud. Then follow [the manual device checklist](docs/VALIDATION.md).

## Source map

- `Assets/Pockle/Runtime/Core/`: portable analytic springs and volume-conscious deformation.
- `Assets/Pockle/Runtime/JellyToy.cs`: authored or procedural mesh, deformed facial features, and materials.
- `Assets/Pockle/Runtime/PipCharacterAsset.cs`: native mesh and authored feature anchors.
- `Assets/Pockle/Runtime/TactilePrototype.cs`: pointer capture, reveal, feedback, and preferences.
- `Assets/Pockle/Runtime/PrototypeHud.cs` / `PrototypeHud.Store.cs` / `PrototypeStage.cs`: the collection UI, box catalog, and presentation.
- `Assets/Pockle/Shaders/`: simple Built-in candy, accent, and reveal shaders.
- `Assets/Pockle/Resources/`: imported character source and material references that retain shaders in player builds.
- `Assets/Pockle/Scenes/PockleTactile.unity`: committed scene entry point.
- `Assets/Pockle/Editor/`: setup menu and build checks.

The authored body has 3,158 vertices and 5,814 triangles; the original baseline has 1,073 vertices and 2,016 triangles. Features reuse small meshes and materials. Deformation uses reused vertex/normal buffers, welds UV-seam normals, and skips idle updates; actual CPU cost, draw calls, frame rate, and transparency artifacts still need profiling on a midrange phone. See [architecture notes](docs/ARCHITECTURE.md).

## Current collecting direction

The owner has simplified the collecting loop to daily collection-themed boxes unlocked by walking, plus optional paid boxes (proposed price points: $0.99 standard and $2.99 special). Packaging identifies the collection while color/variant remains a surprise. The [local collection beta](docs/COLLECTION_UI.md) implements Home, shelf, local profile, Settings, and walking reward; location checks, server authority, and real purchases remain to be connected. Lights-off glow and squeeze-reactive internal stars/pearls are also planned after the current viewer test.

See the [collection box studies](docs/packaging/README.md) for three editable concept dielines and the [viewer direction](docs/INTERACTION_DIRECTION.md) for the planned two-finger, glow, and reactive-filling experiments.
