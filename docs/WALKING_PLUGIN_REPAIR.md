# Walking plugin packaging repair — UX-010/011

October 13, `codex/walking-plugin-fix`.

The owner's existing `APKs/pockle.apk` has no `studio.pockle.walking.StepCounter` class definition. The exported Gradle project includes the Android library, but Unity 6000.6.5f1's default library template comments out `java.srcDirs = ['src']`. The source is in that legacy directory, so the class is not compiled. This confirms a packaging defect; a device exception log is still needed to prove the exact exception on the owner's phone.

The library now has an explicit `build.gradle` that compiles its existing `src` tree, declares its manifest path and namespace, and inherits SDK/Java versions from Unity's generated project properties. Consumer rules retain public JNI entry points if R8 is enabled. The manifest's namespace is defined in Gradle. Source locations and existing asset GUIDs remain stable.

`CollectionSession.WalkingFailed` exposes plugin startup/read exceptions to Claude's UI. It is false for an idle tracker, missing permission, unsupported editor/platform, or missing step sensor. A failed tracker remains failed until app restart, matching the existing behavior. Development builds append the exception type to the status; warning logs include the full exception. Cleanup errors do not abort disposal.

Verified: rebuilt an isolated copy of the owner's previously exported Android Gradle project with the repaired plugin; `:launcher:assembleRelease` passed offline. APK DEX inspection confirms the real class definition and all six public JNI entry points; manifest analysis confirms ACTIVITY_RECOGNITION. The regression checker fails on the original APK and passes on the repaired packaging build. Unity 6000.6.5f1 EditMode suite: 17/17 passed, including the failure-state contract. Required portable/source checks pass.

This packaging-only rebuild reuses the previous phone build's native Unity/C# payload; it does not compile the new C# failure flag or the subsequently added Fox collection. Rebuild the current local Unity project to test all current changes together. No owner ProjectSettings or Packages changes are included.

Not verified: device logcat exception, actual hardware step increments, permissions on the owner's phone, daily progress movement. `adb devices -l` found no connected devices. After rebuilding: allow Physical activity, keep Pockle running, walk enough steps to produce a sensor event, and verify both the running status and daily progress. A fresh sensor baseline requires a subsequent event before credited steps move.

Regression check: `python3 tools/check-walking-apk.py /path/to/built.apk`. Please add this after Android packaging in CI (Claude's lane), so a successful Gradle build alone cannot hide an absent JNI bridge.

Unity library documentation: https://docs.unity3d.com/6000.0/Documentation/Manual/android-library-plugin-create.html . Unity can generate build.gradle automatically; lack of a committed build.gradle alone is not evidence that the whole library was ignored.

Walking repair regression validation: full Unity 6000.6.5f1 EditMode suite 17/17 and PlayMode suite 27/27 passed. APK packaging test passed; portable/source checks passed. Actual sensor counting/logcat pending no connected phone.
