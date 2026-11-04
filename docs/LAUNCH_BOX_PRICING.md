# Launch box pricing

November 3, 2026. Owner confirmed collection-choice singles plus Mystery Mix packs at launch and delegated pack sizes/prices to Codex. These are the selected US-dollar pricing targets for the launch plan, not published store products.

| Offer | Boxes | Target USD price | Per-box equivalent | Collection selection |
| --- | ---: | ---: | ---: | --- |
| Collection Box | 1 | $0.99 | $0.99 | Buyer picks collection; toy finish remains random |
| Mystery Mix — 3 | 3 | $2.49 | $0.83 | Collections and toy finishes are random |
| Mystery Mix — 5 | 5 | $3.99 | about $0.80 | Collections and toy finishes are random |

Three mixed boxes cost48cents less than three99-cent singles (16.16% less per box); five cost96cents less than five singles (19.39%). This is a comparison against the single-box unit price, not an assertion that an identical pack was previously sold at a higher price. Collection choice is the explicit trade-off for the mixed-pack unit price. No crossed-out invented previous pack price or false limited-time urgency.

The range stays at three offer types. No subscriptions, currency-wallet layer, large ten/twenty packs, or rarity-guaranteed premium pack is added. Collection-specific multipacks can wait; they are not required for the first release. Mixed packs are repeatable consumables. Bigger packs buy more draws, not an unapproved better rarity rate. Final effective odds still need approval and must be disclosed before checkout.

One store transaction must deliver exactly3 or5 persistent child draws as appropriate. All child outcomes/collection assignments are fixed under the saved pack receipt; interruption or retry cannot reroll or grant another pack. The server records per-child reveal state so the player can resume opening. Each store product maps to an immutable count/selection policy; the client cannot choose quantity by editing a request. Sandbox/production transactions stay isolated.

Duplicates are allowed and remain owned spares for fusion and trading (owner confirmed November3). Trading is required for launch; collection variety within a pack and any completion boost remain unresolved. No guarantee of distinct collections, no duplicates, a complete set, or rare toys is advertised by these prices. This is separate from random selection and should be resolved before the exact product descriptions/odds are published.

Localized prices come from the store, not hand-built currency conversion. Store price-point availability, regional tax treatment and final translated labels must be checked in each console. Show actual localized unit-price savings only when the local product prices support them; never hardcode the USD savings globally.

Sources: [Apple IAP pricing](https://developer.apple.com/help/app-store-connect/manage-in-app-purchases/set-a-price-for-an-in-app-purchase/), [Google one-time products](https://support.google.com/googleplay/android-developer/answer/16430488).

Duplicate policy confirmed: extra copies remain spares for fusion and trading; no automatic conversion. No no-duplicate purchase guarantee. Trading joins the first release, owner confirmed November3. Finite series supply must cover all child draws before a pack can be sold.

## November 4 — proposed store product identifiers (owner approval required)

No products, projects or apps have been created under this proposal. Recommend all eleven active character collections at launch: Pip, Fox, Axolotl, Firb, Coco, Snowy, Ginger, Reiny, Guin, Bundy and Wally. Fusion Lab and retired characters are excluded. These are collection choices; finishes remain random.

Use the same product IDs on both stores, configured as repeatable consumables. RevenueCat gets a separate **Pockle** project (Biotadex remains untouched), offering **launch_boxes_v1**, and the package names below. Do not attach boxes to a permanent unlock entitlement; durable ownership comes from the server inventory ledger.

| Collection/offer | Store product ID (both stores) | RevenueCat package | Boxes | Target US price |
| --- | --- | --- | ---: | ---: |
| Pip | `pockle.box.pip.v1` | `collection_pip` | 1 | $0.99 |
| Fox | `pockle.box.fox.v1` | `collection_fox` | 1 | $0.99 |
| Axolotl | `pockle.box.axolotl.v1` | `collection_axolotl` | 1 | $0.99 |
| Firb | `pockle.box.firb.v1` | `collection_firb` | 1 | $0.99 |
| Coco | `pockle.box.coco.v1` | `collection_coco` | 1 | $0.99 |
| Snowy | `pockle.box.snowy.v1` | `collection_snowy` | 1 | $0.99 |
| Ginger | `pockle.box.ginger.v1` | `collection_ginger` | 1 | $0.99 |
| Reiny | `pockle.box.reiny.v1` | `collection_reiny` | 1 | $0.99 |
| Guin | `pockle.box.guin.v1` | `collection_guin` | 1 | $0.99 |
| Bundy | `pockle.box.bundy.v1` | `collection_bundy` | 1 | $0.99 |
| Wally | `pockle.box.wally.v1` | `collection_wally` | 1 | $0.99 |
| Mystery Mix 3 | `pockle.mix.3.v1` | `mix_3` | 3 | $2.49 |
| Mystery Mix 5 | `pockle.mix.5.v1` | `mix_5` | 5 | $3.99 |

App identity: approved iOS production bundle **com.playpockle.app**; propose the same **com.playpockle.app** for the Android production listing. Existing phone beta remains **com.PocklePrototype.Pockle**; no bundle/package change, replacement install, or save move is implied by this proposal. Confirm the Android production identity with the product list before creating its listing. `.v1` identifies the immutable grant policy: single specified collection or3/5random collection draws, not a promised toy finish. Store-localized price strings replace these USD targets in checkout.

### Owner-only legal, tax and banking steps

**Apple:** Sign in to App Store Connect yourself as Account Holder. Open **Business → Agreements**, review and accept the current **Paid Apps Agreement** and any outstanding program agreement. In Business, complete the tax forms Apple requires for your legal entity/location and submit the requested banking/payout details; approve bank changes yourself if another team role initiated them. Confirm the paid agreement, tax and banking sections have no outstanding actions. Select/confirm the IAP tax category and provide any requested business/trader verification. Enter credentials, legal/tax identifiers, bank information and verification codes only into Apple's own console. Do not send them to Codex. Exact forms and verification tasks are account-dependent; Codex has not audited your console status.

**Google:** In Play Console, open **Settings → Payments profile** and link the existing appropriate merchant payments profile, or create it yourself if prompted. Confirm legal entity/country/address; complete the tax information and any identity verification requested in Google Payments. Under **How you get paid**, add/confirm the payout bank and complete Google's requested verification yourself. Review and accept any outstanding Google Play Developer Distribution/merchant/payment agreements. Existing Biotadex monetization may mean these account-level steps are already complete: verify rather than replace shared account payment settings. Do not change the developer-account type or payment profile solely for this task. Enter credentials, tax/bank information and codes only into Google's console.

**RevenueCat/store integrations:** owner creates or enters Apple IAP/App Store Connect signing material and Google service-account credentials directly into the service dashboards when requested. No private keys, credentials, financial data or webhook secrets go in Git or chat. Public platform SDK keys may be embedded only after project/app identity is confirmed. Consumable delivery must use authenticated account identity and a server-validated transaction, never the client callback as a grant authority.

### Work after approval

1. Create separate Pockle RevenueCat project and store apps; match approved production IDs. Create13matching consumables and review price points/localizations in App Store Connect and Play Console. Owner handles legal, money and secret-entry gates.
2. Bind store product identifiers/package mapping to HUD localized prices, authoritative per-purchase quotes, purchase states and pending delivery recovery. Do not enable Buy without a usable server quote and supply reservation.
3. Validate transaction and account ownership server-side. One immutable transaction receipt fixes all child outcomes and inventory writes before reveal; retries return the same result. Matchsandbox/production environments and make completion/refund/recovery handling explicit.
4. Only when the real catalog drives the app, remove old BoxCatalog test offers and UnconfiguredBoxCheckout; ask Claude/update authorized LineupMotionTests as needed. Until then, preserve these services as explicitly requested and keep the preview non-purchasable.
5. Acceptance on each phone: one sandbox purchase, exactlyone saved toy for a single box, reveal, restart preservation, retry/interruption without additional charge or reroll. Packs additionally save exactly3/5child outcomes and resume opening consistently. No production sales until both store sandbox paths and authoritative delivery pass.

Sources checked November4: [RevenueCat consumables](https://www.revenuecat.com/docs/platform-resources/non-subscriptions), [Apple IAP prerequisites](https://developer.apple.com/help/app-store-connect/configure-in-app-purchase-settings/overview-for-configuring-in-app-purchases/), [Apple banking](https://developer.apple.com/help/app-store-connect/manage-banking-information/enter-banking-information), [Google payments-profile linking](https://support.google.com/googleplay/android-developer/answer/3092739), [Google merchant bank account](https://support.google.com/googleplay/android-developer/answer/7161440), [Google payout-bank verification](https://support.google.com/googleplay/android-developer/answer/7161378).

## November 4 — product setup approved
Owner explicitly approved all thirteen consumables, eleven launch collections, RevenueCat Pockle/launch_boxes_v1, and com.playpockle.app on both production stores. Proceed with setup; no second approval of these IDs required. Existing beta app unchanged. Owner retains all agreement, financial and credential-entry actions. External setup/real sandbox acceptance are not yet verified.
