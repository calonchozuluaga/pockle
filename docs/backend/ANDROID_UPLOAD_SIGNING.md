# First Google Play internal upload

The first bundle establishes `com.playpockle.app`. It is not a purchase-ready release: checkout stays Coming soon; the billing manifest permission is declared but the RevenueCat SDK and authoritative delivery remain unfinished. Existing Android beta package and save are separate and unchanged.

Unity 6000.6.5f1 builds the release-mode ARM64 IL2CPP bundle in the isolated checkout. Codex validates the manifest and strips the temporary debug JAR signature. Only the owner-signed copy may be uploaded to Play.

## Corrected version 2

Google rejected the permission-only first bundle as obsolete AIDL billing. Version 2 includes Google Play Billing Library 8.3.0; its final manifest version metadata was verified. Sign it with the existing upload key:

```sh
python3 /Users/calonchozulu/Desktop/pockle/tools/sign-play-bundle.py /Users/calonchozulu/Desktop/pockle/Builds/Pockle-Play-Internal-v2.unsigned.aab
```

Do not use `--create-key` again. The output is `Pockle-Play-Internal-v2.signed.aab`. Checkout is still unavailable; library packaging is not purchase-flow acceptance.

## Original owner private signing step (completed for v1)

Run this yourself in macOS Terminal, not in chat or an agent terminal:

```sh
python3 /Users/calonchozulu/Desktop/pockle/tools/sign-play-bundle.py /Users/calonchozulu/Desktop/pockle/Builds/Pockle-Play-Internal.unsigned.aab --create-key
```

This creates an RSA upload key at `~/Library/Application Support/Pockle/Signing/pockle-upload.jks`, alias `pockle-upload`. Java keytool asks you to choose private passwords and certificate details. The helper then prompts for the passwords to sign the bundle. Passwords go directly to local Java processes through temporary environment variables; they are not printed or saved by the helper. Back up the keystore and passwords securely, outside Git.

If you already have a Pockle upload key, omit `--create-key` and pass its `--keystore` and `--alias`. Do not use another app's key inadvertently. The helper refuses to overwrite an existing key or signed bundle.

The result is `Builds/Pockle-Play-Internal.signed.aab`. Tell Codex when it is ready without sending passwords or key contents. Codex checks the signature/certificate metadata and uploads that signed artifact to the Pockle internal-testing draft. The owner handles any Play App Signing agreement. No production rollout is authorized by this setup step.

References: [Android app signing](https://developer.android.com/studio/publish/app-signing), [command-line bundle signing](https://developer.android.com/build/building-cmdline).
