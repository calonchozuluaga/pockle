# CI validation

The workflow runs portable/source/whitespace checks, Unity EditMode/PlayMode tests, and Android packaging. The `build-apk` label requests a phone APK on pull requests. Required jobs must pass before merge; owner phone visuals and handling remain device checks.

Unity graphics stay enabled for test captures. Android compilation uses `-nographics -job-worker-count 2`. The Android Library cache now restores only for an exact asset/package/settings hash. Changed sources start with a fresh import. Two PR #16 attempts were killed during assembly reload after restoring a fallback cache from older assets. The fresh build must pass before merge; this observation alone does not establish cache causality.

[Current repair and results: PR #16](https://github.com/calonchozuluaga/pockle/pull/16). [Earlier CI setup and validation history](https://github.com/calonchozuluaga/pockle/blob/c83c60309f90bf7ae13fbc5c80b96f4d37cd5699/docs/CI.md).

The source check also fails if `ProjectSettings/GraphicsSettings.asset` is missing or no longer lists `UI/Default` (built-in fileID 10770) under Always Included Shaders, and `UiShaderBuildGuard` stops any local player build in the same state. Without it every uGUI element renders magenta on device (TestFlight 15).
