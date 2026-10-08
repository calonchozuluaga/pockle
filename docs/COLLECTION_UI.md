# Collection UI and walking beta

Pockle starts on **Shelf**. Peach Jelly, Moon Jelly, Gold Glitter, and Mint Soft are separate collectible entries, each shown as the actual mesh/material rendered once into a 256px portrait. Four starter toys are gifted on the first launch of this local beta so all finishes remain testable. Duplicate drops increase the count under that toy. The production starter assortment is still a product decision.

Tap a toy to play. Direct touch, upward lift, two-finger squish/stretch, plate rotation, and phone-motion jiggle remain. The variant selector, Reset, and Reveal again controls are removed from the game UI. **Shelf** returns to the collection. **Settings** contains sound, haptics, and full/calm motion. Browsing and dialogs cancel gesture capture and hide the viewer camera and plate; shelf portraits do not run idle cameras.

## Walking path

**Boxes** shows one daily Jelly Garden box. The confirmed target is **1,000 steps**. Enable walking requests Android's **Physical activity** permission on Android 10+. Devices without a step-counter sensor show an unavailable message rather than using the toy's shake accelerometer as a substitute. No GPS/location permission is requested in this pass.

The Android library uses `Sensor.TYPE_STEP_COUNTER`. Counts begin from the first sensor sample after tracking starts; historical steps are not awarded. Keep Pockle running while walking. Counts can arrive in batches, and persisted progress survives a normal restart. Android may stop sensor collection after force quit, process termination, or background restrictions; this is not a full-day health-data integration or a guaranteed background service.

The box unlocks at 1,000 steps. Tap **Open your box** to receive Peach or Mint with equal provisional probability. A duplicate is allowed. Ownership, the daily claim, and a pending reveal are saved together before the box animates. A restart resumes an interrupted reveal without granting it twice. Progress and claim state reset at midnight **UTC**; incomplete steps do not carry over. Intervals spanning midnight are rebased rather than assigned to the new day. Backward device-date changes cannot reset an already claimed day. Counter resets, boot changes, and implausibly fast step batches are rebased without awarding the batch.

This is a **local beta reward**, stored in PlayerPrefs under `pockle.collection.v1`. Hardware step classification and rate checks improve on awarding raw shake acceleration; they do not provide authoritative anti-cheat. Device time, local saves, and sensor data are not trusted production evidence. Server reward validation, occasional consented location/movement checks, and cross-device ownership remain future work.

## Purchase path

The catalog offers Jelly Garden at a proposed $0.99, Midnight Glow at $2.99, and Gold Confetti at $2.99. Closed box art, contents/odds, prices, and Buy actions are wired into the UI. The current provisional test pools are Peach/Mint equally, Moon only, and Gold only. Midnight Glow's current Moon toy is pearlescent; lights-off glow is not implemented.

`BoxCatalog` defines proposed Google Play consumable IDs. `IBoxCheckout` isolates platform billing from browsing. The installed adapter is explicitly unavailable: pressing Buy explains that this build cannot charge, and does not grant a toy.

To enable real purchases next, configure a Google Play Console app and the consumable products, choose an account/inventory backend, connect supported Unity IAP/Google Play billing, and verify purchase tokens on the server. Replace display strings with localized storefront prices. Grant each validated transaction once, persist its box before consumption/acknowledgment, and handle pending payments, cancellation, retry, and redelivery. No payment credentials or SDK are included in this commit. The only new package is Unity's built-in `com.unity.modules.androidjni` 1.0.0, required for the native sensor bridge. Android project settings are unchanged.

## Local test

1. Close Unity, run `git pull --ff-only`, reopen, and press Play. Expect four shelf toys and no prototype control card. Tap each toy and check its material, gestures, and Shelf return.
2. Open Settings during a gesture; close it and confirm a fresh gesture works. Check calm motion and disabled sound/haptics survive restarting Play.
3. Open Boxes, scroll through all three offers, and press Buy. Confirm the unavailable message and unchanged toy counts. Check Android Back/Escape dismisses the dialog first.
4. In the editor only, choose **Pockle → Testing → Complete today's walk (Play Mode only)**. The daily button should unlock. Open the box, watch the existing Jelly Garden opening, then return to the shelf and check one count increased. The same day cannot grant again. The simulator is not included as a phone UI control.
5. Rebuild the Android APK. Enable walking; test permission denial and acceptance, a pocket walk, saved progress after reopening, sensor unavailability if applicable, and gentle toy shaking. Note any false step detections rather than treating the hardware classifier as an anti-cheat guarantee.
6. Check 320×568, 390×844, and tablet/landscape layouts, notches, navigation, scrolling, dialog targets, and plate visibility. Check interrupted openings and the next UTC day's reset.

Portable state/geometry checks and source checks run in cloud. Android Java compilation is checked against Google's API 23 jar. Unity API compilation, plugin import/Gradle packaging, portrait rendering, UI dispatch, physical steps, background behavior, and APK performance still require the local Unity/device checks above.

If `git pull --ff-only` reports a local `Packages/manifest.json` change would be overwritten, close Unity and preserve that file with `git stash push -m "Unity packages before collection UI" -- Packages/manifest.json`. Confirm Git created a **new stash**, then pull and apply that new stash with `git stash apply 'stash@{0}'`. Keep the stash as backup. If there is a merge conflict, retain the existing package entries and the new `com.unity.modules.androidjni` entry; do not reset your Android project settings.
