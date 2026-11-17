# CI validation

The workflow runs portable/source/whitespace checks, Unity EditMode/PlayMode tests, and Android packaging. The `build-apk` label requests a phone APK on pull requests. Required jobs must pass before merge; owner phone visuals and handling remain device checks.

The whitespace step checks the whole tree, not only a PR's changes, and the Unity job does not start until it passes. The root `.gitattributes` lists the only exemptions: one store metadata sheet kept with Windows line endings, and two applied Supabase migrations that end with an extra blank line. Fix new whitespace errors in the file; add an exemption only for a file that must not be edited.

Unity graphics stay enabled for test captures. Android compilation uses `-nographics -job-worker-count 2`. The Android Library cache now restores only for an exact asset/package/settings hash. Changed sources start with a fresh import. Two PR #16 attempts were killed during assembly reload after restoring a fallback cache from older assets. The fresh build must pass before merge; this observation alone does not establish cache causality.

[Current repair and results: PR #16](https://github.com/calonchozuluaga/pockle/pull/16). [Earlier CI setup and validation history](https://github.com/calonchozuluaga/pockle/blob/c83c60309f90bf7ae13fbc5c80b96f4d37cd5699/docs/CI.md).
