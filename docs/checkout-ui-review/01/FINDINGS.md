# Checkout visual review — November 4, 2026

Native Unity6000.6.5f1 passed77/77 targeted tests: Checkout12, BoxReveal11, existing focused regressions54. All nine requested/related checkout fixture captures were rendered at1080×2340. Prices, availability, remaining counts and odds are fixtures, not live offers. No store SDK or account/server service was attached.

## Request for Claude: Common odds label is blank

Observed in ready-wally, ready-mix and all later quote states: Common rows show a grey dot and percentage but no tier name (77.19% for Wally,100.0% for the Pip fixture). Rare/Epic/Secret names render correctly.

Root cause: Assets/Pockle/Runtime/PrototypeHud.Checkout.cs, OddsRow, calls RarityLook.Words(row.Tier) for the label. RarityLook.Words intentionally returns an empty string for Common because ordinary collectible badges are hidden. Use an explicit local Common label for checkout odds; preserve the existing badge helper semantics. Please add an assertion to CheckoutUiTests that Common is visibly labelled for both single and mix quotes, then send back for a native rerun. Codex has not edited Claude’s HUD or tests.

## Verified and not verified

Verified localized price strings, quote-gated Buy, stale/incomplete quote rejection, changed odds/sold-out transitions, mix collection odds, cancel/back behavior, payment/delivery pending without duplicate Buy, pending-delivery recovery banner and delivered-to-reveal handoff through existing tests. Visually checked all nine captures and full-size ready-mix/payment-pending states. Other than the blank Common tier label, no additional issue is claimed from this portrait pass.

Not verified: real RevenueCat/store checkout, authoritative quotes, purchase/edition-ledger delivery, real restart/refund recovery, owner phone layouts or iOS/Android native store sheets. Captures show HUD fixtures only. Final capture helper exited0 with the9-image success marker; Unity’s SearchDatabase startup emitted an unrelated editor index exception. Two earlier helper-only attempts failed (one missing Unity API default argument, then an overbroad button lookup); corrected in the capture helper, with no checkout runtime change. The77-test result predates this editor-only helper and its runtime inputs remain identical by SHA256.
