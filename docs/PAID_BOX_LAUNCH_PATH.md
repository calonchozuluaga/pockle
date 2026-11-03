# Paid-box launch path

Updated November 3, 2026. Owner decision: **paid boxes are mandatory for the first Google Play and Apple release**. This supersedes earlier notes deferring purchases. Trading stays deferred. The first release also includes toy play, collections, same-character fusion, accounts, and walking rewards. No submission date is committed until the gates below are verified.

## Recommended purchase model

Sell a single box as a repeatable consumable for a named collection, through the native Apple/Google checkout using RevenueCat. Bind each offer to that collection on the server; do not make one product per toy finish. Use store-localized prices, rather than the prototype's hardcoded dollar labels. Start with one fully tested collection/product pair on each store, then apply the tested setup to the other approved launch collections. This is an implementation pilot, not a decision to launch with only Wally.

No subscription or new virtual-currency wallet is needed for this path. Existing collections can keep their approved roster sizes; expanding every collection to fifteen toys is not a prerequisite. Whether paid boxes permit duplicates, use a completion boost, or guarantee missing toys is still a product decision. The old backend plan's no-duplicate paid proposal is not automatically approved. Wally's materials/tier counts are approved; the proposed Rare15%/Epic6% split is not final purchase odds.

## Critical path and acceptance gates

| Gate | Work | Lead | Evidence required to pass |
| --- | --- | --- | --- |
| 1. Product and store identity | Confirm permanent bundle/package identifiers, Apple team, Play account eligibility, launch collections, one-box contents, duplicate/completion rule, prices and per-collection odds. Create Pockle store records. Check paid-app agreements/tax/banking readiness in the consoles. | Owner decisions; Codex preparation | Reviewed product matrix with exact store IDs and approved rules; actual Pockle console eligibility. Owner personally completes legal/financial steps where required. |
| 2. Accounts and authoritative inventory | Finish Apple sign-in and production Google setup. Define server-owned purchase/reward inventory and transactional fusion consumption/results, durable client sync, cross-device recovery and account switching. Preserve legacy toys and full fusion/personality records separately from trusted production awards. | Codex | Disposable account tests, real iPhone/Android sign-in, restart/expiry/offline/sync tests; no arbitrary legacy count import minted as trusted purchases. |
| 3. Store and RevenueCat setup | Create a separate Pockle RevenueCat project, connect its own Apple/Google apps and credentials, and create matching consumable products. Verify webhook availability on the existing RevenueCat plan before depending on it; do not upgrade or incur new charges silently. Configure sandbox testers and authenticated webhook endpoint. | Codex; owner credential/terms steps | Sandbox product discovery on each platform; correct Pockle app/product mapping, environments isolated, Biotadex unchanged. No secrets in chat/Git. |
| 4. Verified delivery | Implement server purchase intents, immutable offer/pool snapshots, verified transactions, durable delivery receipts, retries/reconciliation and refund policy. Reuse the same ownership ledger for walking awards and fusion. | Codex | One verified purchase grants exactly one correct box/result; duplicate events/requests cannot grant twice; interruption cannot reroll, lose delivery or debit twice. |
| 5. App checkout and screens | Integrate pinned RevenueCat Unity SDK; initialize after authenticated account resolution. Show localized price, collection, contents and effective odds before native checkout. Handle loading, cancellation, deferred/pending payment, paid-but-delivery-pending and retry states. Reveal only after ownership persists. | Codex runtime; Claude UI | Real sandbox purchase reaches the right collection once, restarts safely and is playable/fusable; no success-only client grants or fake pricing. |
| 6. End-to-end acceptance | Test the matrix below on both physical phones using each store's sandbox/test track, followed by release-candidate regression. Confirm account deletion/sign-out and purchase-history retention. | Codex verification; owner device review | Test results and device evidence for every case; full required CI/native/portable checks green. Existing owner inventory/fusions preserved. |
| 7. Store review package | Produce release-signed Android AAB and iOS archive, remove development gifts/debug shortcuts, finish privacy/data-safety/age/content declarations, screenshots and purchase review assets. Include initial Apple IAPs with the first app version. Review the exact builds/listings before submitting. | Codex platform; Claude art/screens; owner declarations | Store-valid signed builds with working reviewer access, available products and accessible odds, no incomplete trading/checkout promises, verified Pockle production eligibility. |

Gates 1 and 2 start first. Store setup can proceed while the inventory/delivery implementation is built. Screen work can proceed against a reviewed contract. Final device acceptance waits for the real store products, authentication and server delivery to work together. Sandbox success alone is not a live release.

## Delivery design and boundaries

- Configure RevenueCat with an opaque authenticated Pockle player identifier, not an email or username. A RevenueCat identifier alone is not authorization. Server delivery verifies the signed-in player and binds the trusted store transaction to the appropriate player and offer; reject mismatches and unsafe account-transfer cases.
- Create a server intent for the exact collection and versioned outcome rules before checkout. For any hidden outcome reservation, keep its result stable across retries. Concurrent free rewards, fusion or another purchase cannot invalidate any promised eligibility/guarantee.
- Let RevenueCat verify store purchases. Do not grant from a client success callback, a client-supplied product/transaction claim, or a permanent entitlement attached to a consumable. Confirm product, app, owner binding, environment, purchase status and transaction evidence server-side.
- Authenticate webhook calls. Durably store accepted events before acknowledging them, then process them. Deduplicate both incoming event IDs and the canonical store transaction scoped by store/environment/app; different retries/events for the same payment still produce one delivery. Provide reconciliation if webhooks are delayed, exhausted or missing.
- Store receipt/ownership/result atomically, then send the durable result to the client. Persist the local copy before reveal. If payment succeeds while the app is closed or offline, show delivery pending and recover after sign-in; never invite another purchase as the recovery mechanism.
- Preserve the agreed draw if delivery arrives after an intent/reservation expires. Record an unresolved paid obligation for recovery/refund rather than silently dropping or rerolling it. Define refund/revocation behavior, including a toy already consumed by fusion, before launch. No trading dependency in v1.
- RevenueCat consumption/acknowledgement behavior must be verified for the exact SDK version. A store acknowledgement is not proof that Pockle inventory received the toy. Keep the app/server delivery ledger durable regardless of SDK timing.
- Recover delivered consumables through the Pockle account ledger; do not imply that a store restore button recreates already-consumed boxes. Deletion retains only required purchase/accounting records under the reviewed retention policy while disabling further access.
- Use native Apple/Google billing for this initial implementation. Both stores require advance odds disclosure for purchased randomized virtual items. Published effective odds must match the actual server draw; ownership-dependent odds need a fresh, accepted quote.

## Minimum purchase test matrix

| Scenario | Required result |
| --- | --- |
| First and repeated purchase | Correct collection delivered once per real transaction; allowed repeat purchase works |
| User cancels / store fails | No charge-success claim, no toy grant |
| Deferred or pending payment | No premature toy grant; eventual success or failure reconciles |
| App killed after payment | Delivery recovered without another charge or reroll |
| Offline after payment | Honest pending state; exactly one grant after reconnect |
| Duplicate webhook and client retries | One durable delivery for the transaction |
| Missing webhook / redelivery | Server reconciliation finds the same verified purchase |
| Second phone / sign-out / account switch | Correct owner recovers paid inventory; another account cannot claim it |
| Concurrent walking/fusion/purchase | No double-spend, ownership race, changed promised odds or duplicate guarantee violation |
| Refund and fused result | Documented, tested policy; no unrelated toy loss |
| Fresh release install | No development grants; localized price and real odds visible; no unavailable checkout advertised |
| Legacy owner upgrade | Every existing count and complete fusion/personality record survives |

## Decisions to settle before publishing products

- Permanent app identifiers and initial paid collections.
- Launch prices (existing prototype dollar labels are not approval).
- Paid duplicate/completion rule, and whether it differs from free walking boxes.
- Final per-collection odds, including Wally's unapproved tier split.
- Refund/late-delivery behavior and production/legacy inventory policy.

These decisions do not block code scaffolding, platform exports, disposable-account tests, or preparing the store forms. They do block publishing an actual paid offer. Never invent them from old proposals.

## Current starting point

Completed November3: recovered committed phone-tested source; latest unchanged Claude polish and official logos integrated; focused Unity54/54, portable checks and source258C#/834GUIDs passed; twelve Home/Boxes fixture captures inspected; exact pinned Unity iOS module installed. Android's earlier owner save was verified89 inventory rows/15fusions. Xcode is installed; iOS signing/device validation is outstanding.

Inspected read-only: Play account is Personal, Biotadex is live, no Pockle listing. This does not confirm Pockle's production-access gate. RevenueCat shows only Biotadex, no Pockle project. Live Supabase has the six earlier foundation migrations, not the paid outcome/delivery ledger. No paid setup, products, purchase grant, store submission or new phone installation was performed by this planning change.

## Sources checked November 3

- [RevenueCat consumables and non-subscription purchases](https://www.revenuecat.com/docs/platform-resources/non-subscriptions)
- [RevenueCat Unity setup](https://www.revenuecat.com/docs/getting-started/installation/unity)
- [RevenueCat customer identity](https://www.revenuecat.com/docs/customers/identifying-customers)
- [RevenueCat webhook authentication and retries](https://www.revenuecat.com/docs/integrations/webhooks)
- [Apple review guidelines: purchases and randomized-item odds](https://developer.apple.com/app-store/review/guidelines/)
- [Apple first IAP submission](https://developer.apple.com/help/app-store-connect/manage-submissions-to-app-review/submit-an-in-app-purchase/)
- [Google billing and randomized-item odds](https://support.google.com/googleplay/android-developer/answer/9858738)
- [Google new-personal-account testing requirements](https://support.google.com/googleplay/android-developer/answer/14151465)
