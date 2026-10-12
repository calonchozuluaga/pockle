# Shelf motion and silhouettes — UX-033

Build branch `codex/animated-shelf`, from main `dca3ec3`, Unity **6000.6.5f1**, Built-in RP. Owner approved adding this before the next device test.

## Behavior

Pip's Shelf and Collection each show ten catalog slots: four available finishes and six planned silhouettes. Missing includes unowned planned slots, labelled Coming soon, without granting or enabling them.

- Found Shelf/Collection toys breathe, gently squish and rock using the existing character profiles on real 3D meshes. The tactile viewer has its own toy and is never animated by shelf code.
- Missing entries show a plum outline, with a short eager greeting followed by a rest beat. Names/finishes remain hidden and tiles stay unplayable. Pip uses its authored body silhouette. Planned characters use small outline-only studies based on ART-001; these are not new runtime toys or available rewards. Secrets use a generic mystery shape without exposing character art.
- Visible cards share four motion slots. Found actors share one manually rendered camera and no more than four reusable **192×192** targets, at **15 fps**. Overflow visible cells retain a still portrait/outline. Scrolling releases departing actors before assigning replacements. Existing four 256px still portraits remain shared Home/avatar/fallback resources.
- **Motion calm** restores still portraits and resting outlines and stops live rendering. Hidden pages, dialogs, pause/focus loss, HUD disable and disposal stop animation. Native targets are released on disposal.
- Shelf is now a real navigation tab, preserving scroll and Back to Home while clearing unrelated page history. Existing box art/screens are included unchanged from main.

## Checks before publication

Local portable checks: **543,484 assertions**, including Shelf tab history. Source checks: **68 files / three symbol profiles / five assemblies / 148 GUIDs / zero failures**; metadata/whitespace. Unity tests cover real mesh changes, shared limits, calm rendering stop, pause/disable/disposal, hidden faces, eager silhouettes, no ownership changes and secret concealment. Existing profile/navigation/box/collection tests remain in the suite. Initial runtime head `9f8879b` passed 37/37 Unity tests with no skips in run `38297552203`; actual UI frames were decoded and reviewed. The final ten-slot Shelf and portrait-resolution capture follow-up still require their own CI. Final results/evidence are linked from [PR #14](https://github.com/calonchozuluaga/pockle/pull/14).

The UI capture fixture writes full PNGs and compact CI previews to `test-results/shelf-motion/`: shelf idle frames, Pip lineup, eager silhouette frames, calm and a planned character lineup. These are real Unity UI and preview-camera renders, not concept mockups. Full captures travel in the existing test artifact without changing CI.

## Owner device check

1. Open Shelf: owned toys breathe gently, and each still opens the correct toy. Missing entries have no face/finish/name reveal and cannot be played.
2. Browse Pip's ten-slot lineup. Scroll repeatedly: newly visible cells animate while departing cells stop. Planned slots remain labelled Coming soon. Open other characters to review distinct outline studies.
3. Select Missing/Duplicates/Everything, open a toy and Back, then change tabs. Scroll and selected collection context should be preserved correctly.
4. Turn Motion calm on: every toy/outline rests immediately. Turn it off and return to Shelf: motion resumes. Background/return to the app, open a dialog and close it, and repeat page changes.
5. Profile a release/development build on the same phone: CPU/GPU frame time, GC allocations, batches and texture memory during still Shelf, animated Shelf, continuous scrolling and Calm. Four live targets are a resource bound, not proof of a target phone frame rate. Hosted llvmpipe timing is not phone performance evidence.
