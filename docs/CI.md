# Continuous integration

Neither Codex nor Claude can run the Unity editor, so CI on GitHub Actions is where Unity compilation, the Unity test suites, and Android builds actually get verified. The workflow is `.github/workflows/ci.yml` and runs on every pull request, every push to `main`, and on demand.

## Jobs

| Job | Runs when | What it proves |
| --- | --- | --- |
| **Portable checks** | Always | The engine-free `Pockle.Core` checks pass, every C# file parses (default, Editor, and Android symbols), every asset has a unique `.meta` GUID, and there are no whitespace errors. No secrets needed. |
| **Unity license available?** | Always | Whether the Unity jobs can run. Without a license secret it posts a warning and the Unity jobs are skipped, not failed. |
| **Unity compile + tests** | License secret present | The project imports and compiles in Unity `6000.6.5f1` (read from `ProjectVersion.txt`), and both suites pass: EditMode `Pockle.Core.EditMode.Tests` and PlayMode `Pockle.Variants.PlayMode.Tests`. Results appear as a "Unity test results" check and a downloadable artifact. |
| **Android APK** | Pushes to `main`, manual runs, and PRs labeled `build-apk` | A development APK builds. Download it from the run's **Artifacts** section and sideload it on a phone. |

CI uses GameCI's `unityci/editor` Docker images, which publish `ubuntu-6000.6.5f1` Android images. The `Library` folder is cached between runs, so the first run is slow (often 20–40 minutes) and later runs are much faster.

## One-time setup: add a Unity license

The Unity jobs need your Unity account's license stored as GitHub secrets. Do this once, in **GitHub → pockle → Settings → Secrets and variables → Actions → New repository secret**.

**Unity Personal (free):**

1. On your Mac, open Unity Hub and make sure you're signed in with an active Personal license (Hub → Preferences/Settings → Licenses).
2. In Finder, choose **Go → Go to Folder…** and open `/Library/Application Support/Unity/`. Open `Unity_lic.ulf` in TextEdit and copy **all** of its contents.
3. Create these three secrets:
   - `UNITY_LICENSE`: the full contents of `Unity_lic.ulf`
   - `UNITY_EMAIL`: your Unity account email
   - `UNITY_PASSWORD`: your Unity account password

**Unity Pro/Plus:** instead of `UNITY_LICENSE`, create `UNITY_SERIAL` with your serial key, plus `UNITY_EMAIL` and `UNITY_PASSWORD`.

Then open the **Actions** tab, choose **CI → Run workflow** on `main`, and confirm the Unity jobs run. If activation fails, the log will say so in the first minutes of the Unity job; see GameCI's activation guide at https://game.ci/docs/github/activation.

Secrets are not available to pull requests opened from forks. That's fine here: Codex and Claude branch inside this repository.

## Reading results

- **Portable checks red:** a real logic or syntax regression; the log names the failing check or file.
- **Unity tests red:** open the "Unity test results" check on the PR for the failing test name, or download the `unity-test-results` artifact.
- **Unity compile error:** appears in the Unity job log before tests run. This is the error class the portable checks cannot catch (Unity API misuse, missing references), so treat it as blocking.
- **Android red but tests green:** usually packaging (plugins, Gradle, manifest). The log's Gradle section has the cause.

CI checks code and builds. It does not replace a phone check for feel, frame rate, sensors, haptics, or how something looks; those stay in `docs/VALIDATION.md`.

## Known first-run risks

- PlayMode tests render toy portraits into RenderTextures. If they fail only in CI with graphics errors, the runner's headless mode is the likely cause, and the fix is in the workflow (not the game).
- `ProjectSettings/` only commits the pinned version, the scene list, and the player settings; Unity generates the rest on first import in CI, as it does on a fresh clone.
