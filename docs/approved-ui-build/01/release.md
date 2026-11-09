# Approved UI and play controls — TestFlight 1.0 (9)

Branch: `codex/approved-ui-phone`. Isolated Unity project: `/private/tmp/pockle-ios-20261108`.

## Included

- Warm peach/cream/plum interface, painted Home hero and collection-art product cards.
- Collection purchase entry and direct Play from a saved fusion result.
- Pockles / Backdrop / Light / About play controls. Owned playable finishes and saved fusions share one scrolling chooser; selecting closes it. Only Daylight and Night; legacy Golden Hour saves normalize to Daylight without changing Night's numeric value.
- Seven approved local moment clips; saved fusion, box reveal/special reveal, step milestone and walking box-ready bindings. Trade/purchase-confirmation clips are imported for their future distinct flows. No duplicate purchase jingle over the reveal.
- Same existing collection voices, full-tail moment playback, Sound/volume controls and foreground/preference gates.
- Thumbnail capture isolated from live shelf actors; no old Pip overlay on other toys.
- Prior release-feedback fixes: all 165 catalog finishes, server-saved random walking collections, 1,000 steps / one-hour cooldown / four daily / two banked, idempotent walking receipts with additive save v6, refreshed auth and sandbox checkout quotes.

## Preview and checks

`index.html` contains 10 real Unity portrait captures from an offline fixture. Prices and inventory in those images are illustrative; no player records or purchases changed.

Native Unity: 113/113 focused PlayMode tests passed. Core portable checks passed. Ten portrait captures completed with saved preferences restored. Native archive and App Store IPA export succeeded. Build 1.0 (9) uploaded on November 9 at 09:19 MST; Apple reported processing started. Final processing, compliance and existing owner-group assignment are pending browser sign-in. The upload succeeded with the existing UnityRuntime.framework dSYM warning.

## Test on the phone

1. Update through TestFlight and check the existing collection/session survive.
2. Open Pockles during play; select an owned toy from another collection and a saved fusion. Check Backdrop and both lighting choices, then rotate the phone.
3. Complete a fusion with actual spare copies; Play should open its saved result and spent copies must stay spent after restart.
4. Earn a walking box with 1,000 credited steps and the one-hour gate; verify its server-selected collection, open it and restart. Limits remain four a day and two banked.
5. Try a non-Wally sandbox box, review contents/odds, finish the store test purchase and check one saved delivery/reveal survives restart. No production charges are enabled.
6. Listen to fusion/reveal/step/box-ready moments. Muting should suppress them; returning from background must not replay old milestones.

## Not included / not verified

Public trading remains an approved design, not an implemented marketplace. No public store-review submission. Phone purchase/walking/audio acceptance of build 9 remains with the owner. Server-authoritative current ownership and trading/edition allocation remain release gates. The known UnityRuntime dSYM warning is tracked separately.
