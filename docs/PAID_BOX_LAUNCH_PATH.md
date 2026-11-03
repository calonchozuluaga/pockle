# Paid-box launch path

Updated November 3, 2026. Owner decision: **paid boxes are mandatory for the first Google Play and Apple release**. This supersedes earlier notes deferring purchases. Trading and finite series supply are also required for the first release (owner confirmed November3). The first release also includes toy play, collections, same-character fusion, accounts, and walking rewards. No submission date is committed until the gates below are verified.

## Recommended purchase model

Sell a single box as a repeatable consumable for a named collection, through the native Apple/Google checkout using RevenueCat. Bind each offer to that collection on the server; do not make one product per toy finish. Use store-localized prices, rather than the prototype's hardcoded dollar labels. Start with one fully tested collection/product pair on each store, then apply the tested setup to the other approved launch collections. This is an implementation pilot, not a decision to launch with only Wally.

No subscription or new virtual-currency wallet is needed for this path. Existing collections can keep their approved roster sizes; expanding every collection to fifteen toys is not a prerequisite. Owner confirms paid duplicates remain spares for fusion and trading. Completion boosts and exact odds remain to settle. The old backend plan's no-duplicate paid proposal is not automatically approved. Wally's materials/tier counts are approved; the proposed Rare15%/Epic6% split is not final purchase odds.

## Critical path and acceptance gates

| Gate | Work | Lead | Evidence required to pass |
| --- | --- | --- | --- |
| 1. Product and store identity | Confirm permanent bundle/package identifiers, Apple team, Play account eligibility, launch collections, one-box contents, duplicate/completion rule, prices and per-collection odds. Create Pockle store records. Check paid-app agreements/tax/banking readiness in the consoles. | Owner decisions; Codex preparation | Reviewed product matrix with exact store IDs and approved rules; actual Pockle console eligibility. Owner personally completes legal/financial steps where required. |
| 2. Accounts and authoritative inventory | Finish Apple sign-in and production Google setup. Define server-owned finite edition/copy inventory, purchase/walking grants, transactional fusion consumption/results and atomic toy-for-toy trades, durable client sync, cross-device recovery and account switching. Preserve legacy toys and full fusion/personality records separately from trusted production awards. | Codex | Disposable account tests, real iPhone/Android sign-in, restart/expiry/offline/sync tests; no arbitrary legacy count import minted as trusted purchases. |
| 3. Store and RevenueCat setup | Create a separate Pockle RevenueCat project, connect its own Apple/Google apps and credentials, and create matching consumable products. Verify webhook availability on the existing RevenueCat plan before depending on it; do not upgrade or incur new charges silently. Configure sandbox testers and authenticated webhook endpoint. | Codex; owner credential/terms steps | Sandbox product discovery on each platform; correct Pockle app/product mapping, environments isolated, Biotadex unchanged. No secrets in chat/Git. |
| 4. Verified delivery | Implement server purchase intents, immutable offer/pool snapshots, verified transactions, durable delivery receipts, retries/reconciliation and refund policy. Reuse the same ownership ledger for walking awards and fusion. | Codex | One verified purchase grants exactly one correct box/result; duplicate events/requests cannot grant twice; interruption cannot reroll, lose delivery or debit twice. |
| 5. App checkout and screens | Integrate pinned RevenueCat Unity SDK; initialize after authenticated account resolution. Show localized price, collection, contents and effective odds before native checkout. Handle loading, cancellation, deferred/pending payment, paid-but-delivery-pending and retry states. Reveal only after ownership persists. | Codex runtime; Claude UI | Real sandbox purchase reaches the right collection once, restarts safely and is playable/fusable; no success-only client grants or fake pricing. |
| 6. End-to-end acceptance | Test the matrix below on both physical phones using each store's sandbox/test track, followed by release-candidate regression. Confirm account deletion/sign-out and purchase-history retention. | Codex verification; owner device review | Test results and device evidence for every case; full required CI/native/portable checks green. Existing owner inventory/fusions preserved. |
| 7. Store review package | Produce release-signed Android AAB and iOS archive, remove development gifts/debug shortcuts, finish privacy/data-safety/age/content declarations, screenshots and purchase review assets. Include initial Apple IAPs with the first app version. Review the exact builds/listings before submitting. | Codex platform; Claude art/screens; owner declarations | Store-valid signed builds with working reviewer access, available products and accessible odds, working trading and checkout, with no incomplete promises, verified Pockle production eligibility. |

Gates 1 and 2 start first. Store setup can proceed while the inventory/delivery implementation is built. Screen work can proceed against a reviewed contract. Final device acceptance waits for the real store products, authentication and server delivery to work together. Sandbox success alone is not a live release.

## Delivery design and boundaries

- Configure RevenueCat with an opaque authenticated Pockle player identifier, not an email or username. A RevenueCat identifier alone is not authorization. Server delivery verifies the signed-in player and binds the trusted store transaction to the appropriate player and offer; reject mismatches and unsafe account-transfer cases.
- Create a server intent for the exact collection and versioned outcome rules before checkout. For any hidden outcome reservation, keep its result stable across retries. Concurrent free rewards, fusion or another purchase cannot invalidate any promised eligibility/guarantee.
- Let RevenueCat verify store purchases. Do not grant from a client success callback, a client-supplied product/transaction claim, or a permanent entitlement attached to a consumable. Confirm product, app, owner binding, environment, purchase status and transaction evidence server-side.
- Authenticate webhook calls. Durably store accepted events before acknowledging them, then process them. Deduplicate both incoming event IDs and the canonical store transaction scoped by store/environment/app; different retries/events for the same payment still produce one delivery. Provide reconciliation if webhooks are delayed, exhausted or missing.
- Store receipt/ownership/result atomically, then send the durable result to the client. Persist the local copy before reveal. If payment succeeds while the app is closed or offline, show delivery pending and recover after sign-in; never invite another purchase as the recovery mechanism.
- Preserve the agreed draw if delivery arrives after an intent/reservation expires. Record an unresolved paid obligation for recovery/refund rather than silently dropping or rerolling it. Define refund/revocation behavior, including a toy already consumed by fusion, before launch. Trading is a v1 dependency; lock ownership across paid grants, fusion and both sides of a trade.
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
- Single box: owner-approved US$0.99 baseline, with store-localized equivalents. Owner delegated launch pack pricing: MysteryMix3 US$2.49; MysteryMix5 US$3.99. See LAUNCH_BOX_PRICING.md.
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

## November3 — price and pack discussion

Owner approved a99-cent single-box price (US$0.99 baseline; actual localized price comes from each store). Discounted packs were suggested, with randomized contents, but no size/price or guarantee was approved. Clarify whether collection selection stays with the shopper or becomes random too. Until answered, the collection-specific consumable design above remains a recommendation, not a newly confirmed rule. Bulk quantities must receive the same durable delivery/retry protections; a pack purchase cannot reroll its draws on restart.

## November3 — mix packs included and launch pricing selected

Owner confirms collection-choice singles plus mixed-collection packs from day one and delegates pack sizes/prices. The launch targets are1collection box US$0.99,3MysteryMix boxes US$2.49,5MysteryMix boxes US$3.99. [Pricing matrix and delivery rules](LAUNCH_BOX_PRICING.md). This supersedes the tentative pack/collection-selection notes above. Collection-choice multipacks are not added. Mix-pack variety/repeat rules, paid duplicate/completion policy and final odds remain to settle. Store products are not yet created.

## November3 — duplicate policy confirmed

Owner confirms duplicates remain extras for fusion and trading. This supersedes the old paid no-duplicate proposal: owned toys remain eligible unless a separately approved policy says otherwise, and duplicates do not auto-convert. Completion boosts and exact odds still require approval. Trading's first-release timing needs clarification before changing the earlier deferred-trading scope. The shared server inventory must prevent a spare being consumed by fusion and traded twice if/when trading is enabled.

## November3 — finite series and trading acceptance gate

Both are mandatory for v1, superseding earlier deferral. The recommendation is fixed per-finish caps with stable copy identities/serials, so a sold-out series stays collectible and tradable without new official copies being created. Actual edition quantities are not set. Once official supply is exhausted, new paid and walking awards from that pool stop; retained owners keep their toys. Trades transfer copies rather than minting replacements. Fusion/retirement/refund cannot silently restore edition supply or recycle serials. Fused toy identity/tradability is a separate explicit policy, not a source of extra capped official toys.

Paid offers and mixed packs must reserve enough eligible supply for all child draws before opening checkout. Near exhaustion, server-confirmed eligibility and effective odds are frozen for that purchase. No charging for five boxes then delivering fewer, no undisclosed substitution from another collection, no retry reroll. A stale offer needs a fresh accepted quote. Late payments retain a durable delivery/refund obligation. Walking bank reservations and paid reservations must share supply accounting.

Trading baseline follows prior owner-approved direction: toy-for-toy, authenticated collectors, exact spare offers/wants, both sides see and confirm the actual exchange, preset actions with no free-text stranger messaging, cancellation/expiry and report/block. Existing proposed24-hour expiry/counter caps are not newly changed by this task. A copy cannot be offered in two trades or consumed by fusion while reserved. Final completion checks both owners/status/blocks and available spares, transfers both sides in one transaction, stores a replay-safe receipt/history, and survives reconnect. Both inventory views reconcile from that result. No real-money resale.

Additional acceptance cases: simultaneous attempts at the last edition; full mixed pack with insufficient stock; free and paid grants racing; trade versus fusion of the same spare; competing trades; one party cancelling/expiring/disabling/blocking while confirmation is in flight; network interruption after acceptance; duplicate accept requests; signed-out/foreign-account access; sold-out browsing/play/trade; legacy copies cannot silently enter trusted finite production supply. Unapproved legacy-to-production migration must not lose existing toys.

Codex owns edition/inventory/grant/fusion/trade services and concurrency verification. Claude owns trading screens/confirmation/reservation/sold-out copy and UI tests. Owner supplies edition sizes and approves the inventory/fused-result policies. This expands the release critical path; it does not establish a submission date.
