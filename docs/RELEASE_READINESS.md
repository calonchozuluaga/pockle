# First release readiness — November 3, 2026

Owner-approved scope: toy play, collections, same-character fusion, Apple/Google accounts, and walking rewards on Google Play and Apple App Store. Paid boxes are mandatory for the first release (owner confirmed November3); trading stays deferred. See [the paid-box launch path](PAID_BOX_LAUNCH_PATH.md). Scope approval is not approval of draw odds or authorization to submit an unverified binary.

## Current evidence

| Area | Established | Required before submission |
| --- | --- | --- |
| Android gameplay | October22 phone build installed; 89 inventory rows and 15 complete fusions preserved; focused 52/52 native checks | Latest focused integration suite54/54 passed November3; fresh install/returning player, full suites, release build and real walking remain |
| Current UI | Claude finished October22 polish and October23 official logos; unchanged snapshots imported into release branch November3 | 54/54 native focused checks passed November3; 12 native Home/Boxes fixture captures rendered and inspected, collection/sign-in captures and final device review remain |
| Google accounts | Real Google phone sign-in, username, exact tagged cloud snapshot and cold restart verified October21 | Remove testing-only audience restriction through provider setup; expiry, sign-out, deletion and restore/conflict tests |
| Apple accounts | Owner confirms Apple membership and iPhone available; iOS Keychain source exists | Provider configuration, signed build, callback/keychain and real phone sign-in |
| iOS toolchain | Xcode27.2 runs with explicit developer directory; pinned Unity6000.6.5f1 iOS module installed November3 | Export/build/archive and physical iPhone tests; do not change shared engine or Android settings |
| Walking boxes | Server timing/reservation foundation and HUD snapshot contract exist | Server outcome/grant receipts, retry-safe durable client, approved odds, no phone-clock minting, platform step integration |
| Inventory/fusion | Existing local persistence/consumption/replay checks and phone preservation evidence | Define launch server/local authority and cross-device behavior; legacy save preservation is not trusted production minting |
| Account deletion | SQL worker and fixture checks exist; UI request exists | End-to-end disposable account test and local-cache policy verified; never delete owner account to test |
| Paid boxes | Mandatory for v1; RevenueCat account exists but Pockle setup is absent | Complete the product/server/RevenueCat/native-checkout/device acceptance path; actual prices and odds approved before live offers |
| Trading | Deferred by owner | Claude hides unfinished trading promises; paid boxes remain in launch screens |
| Store packaging | Prior APK is development/debug signed | Store package identifiers, signing ownership, Android AAB, iOS archive, versioning, removal of test grants/debug paths |
| Store content/privacy | Public website/legal drafts exist | Accurate privacy/data-safety/age declarations, screenshots, app icon, review notes/access and reachable support/deletion pages |

## Google Play account inspection

Read-only browser inspection November3 confirms Zulapp is a Personal developer account, with Biotadex already in Production. No Pockle listing exists. This does not establish Pockle's production access. Do not infer a mandatory wait from account type alone or infer exemption from another app being live. Account creation date and Pockle-specific gate remain unconfirmed.

Google's current documentation applies the 12 continuously opted-in testers / 14-day closed-test rule to personal accounts created after November13,2023. Verify the actual Pockle dashboard before promising a submission date. [Google testing requirements](https://support.google.com/googleplay/android-developer/answer/14151465).

## Sequence and ownership

1. Codex recovers the phone-verified source, integrates unchanged finished Claude UI, runs native and portable checks, and verifies the iOS toolchain.
2. Claude owns launch-only screen/navigation/copy changes, official-provider appearance, and store visual assets. Request is in Codex handoff; no external message sent.
3. Codex finishes accounts, shared inventory/rewards and paid delivery with RevenueCat; Claude implements checkout states. Owner supplies provider/store access/signing and approves actual prices, pool odds and duplicate policy.
4. Both platforms pass fresh-install and returning-save end-to-end device acceptance. Verify process interruption, offline retry, denied motion permission, fusion consumption and retained ownership.
5. Produce release-signed candidates and store listings, then review the exact artifacts before submission. No current release date commitment.

## Boundaries

Preserve current beta saves and all full fusion/personality records. No arbitrary production imports of legacy counts. No live paid draws until the paid-box gates pass; no live trading, new scarcity promises, or invented server odds. Repository main is older than the last installed build; release branch intentionally retains the phone-tested integration dependencies, with upstream reconciliation required before merge.

Working branch: codex/release-readiness, isolated /private/tmp/pockle-release-readiness-20261103, recovered from ec4e9c3. The owner's Claude checkout remains dirty and must not be reset/staged wholesale. Old temporary caches were cleaned; old log paths are historical evidence, not current reruns.

## RevenueCat inspection — November 3

Read-only browser inspection found an accessible RevenueCat account with only the Biotadex project in its project selector. No Pockle project or store products were created or changed. Pockle has no Google Play app listing yet. Existing Biotadex configuration must remain isolated.

RevenueCat supports Unity and non-subscription purchases; repeated boxes are consumables rather than permanent entitlements. RevenueCat transaction verification is not proof that Pockle delivered a box. An authenticated server receipt/transaction ledger must atomically deliver once, handle retries and pending outcomes, and account for refunds. Existing account does not establish Pockle store credentials, products, integration, approved pricing or paid draw policy. Sandbox setup can be prepared without enabling live checkout; owner subsequently made paid boxes mandatory for v1. [Implementation path](PAID_BOX_LAUNCH_PATH.md).

Sources: [RevenueCat Unity installation](https://www.revenuecat.com/docs/getting-started/installation/unity), [RevenueCat non-subscription products](https://www.revenuecat.com/docs/platform-resources/non-subscriptions).

Price update: owner approved US$0.99 baseline per single box November3. Discounted randomized packs are a tentative idea, with collection selection and pack size/price unresolved. No live offer was created.

Launch pricing selected under owner delegation: CollectionBox1 US$0.99; MysteryMix3 US$2.49; MysteryMix5 US$3.99. Buyer picks a collection only for the single; mixed packs randomize collection and finish. See [pricing matrix](LAUNCH_BOX_PRICING.md). No store products have been published.
