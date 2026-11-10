# First release readiness — November 3, 2026

Owner-approved scope: toy play, collections, same-character fusion, Apple/Google accounts, and walking rewards on Google Play and Apple App Store. Paid boxes are mandatory for the first release (owner confirmed November3); trading is also mandatory (owner confirmed November3), with finite globally tracked series supply. See [the paid-box launch path](PAID_BOX_LAUNCH_PATH.md). Scope approval is not approval of draw odds or authorization to submit an unverified binary.

## November 9 — current submission checkpoint (supersedes older incomplete statuses below)

Read-only audit on `codex/playtest-feedback-11` after fetching origin/main and reading both handoffs. Installed iOS baseline is TestFlight1.0(10), source4bf1161. New playtest corrections6a90c37 are pushed, locally verified (99 focused native cases plus Core/source checks), and reviewable, but are not in a phone build. Apple/Google login, physical iPhone steps, published legal pages and disposable account deletion have later acceptance evidence; do not repeat November3 setup blockers as though those tasks are still absent. Apple store agreement/credentials/products are configured. The owner has now verified Mix5 payment, all five sequential reveals and summary; read-only SQL confirms saved outcomes and cloud backup.

**Blocking production submission under the approved scope:**

1. **Production purchases and ownership.** Current PaidBoxCheckoutService requires SANDBOX, Edge verification accepts sandbox transactions only, and SQL rejects production deliveries. This is a deliberate test boundary, not a live-payment launch path. Finish production transaction verification, immutable grants/obligations, interruption/reconciliation and refund/revocation handling before exposing checkout publicly. Store configuration and sandbox success do not establish this.
2. **Shared authoritative copies, finite stock and public trading.** Public marketplace is still an honest coming-soon screen; fusion uses LocalFusionService. Delivery receipts and uploaded legacy snapshots do not track current spendable copies across devices. Implement one authority for paid/walking mints, fusion consumption, trade reservations and atomic transfers, approved caps/sold-out behavior and legacy eligibility. Public offers trading remains required by owner; no scope reduction is assumed. The detailed plan and remaining policy decisions are in TRADING_ROOM_IMPLEMENTATION.md.
3. **Google closed test.** Live dashboard rechecked November9:10/11 setup tasks complete, store listing still incomplete; closed release not published,0testers opted in, production application disabled. It explicitly requires12continuous opt-ins for14days. Complete listing/review declaration (AI asset labeling step was visibly unselected), publish the closed track, recruit through the existing group and collect actual testing feedback. Keep community billing restricted appropriately while server supports sandbox only. Production-access application/review follows the test; do not imply internal testing starts the clock.
4. **Final release candidate and device acceptance.** Put approved latest feedback corrections on both platforms; run required full CI/native/portable checks on exact candidate. Verify single/Mix3/Mix5, restart, interruption/network loss and receipt replay; walking reward timing/banking; permanent spare spending; account switching/restore/deletion; device photo picker/status bars/audio. Add two-account trade/race acceptance after the marketplace exists. Existing focused test counts are not full hostedCI.
5. **Store review package.** Finish Apple version metadata/screenshots/privacy/reviewer access and IAP review screenshots, attach initial IAPs to first app submission; finish Google's listing and final draft review. Recheck public Google OAuth audience, final product/environment mappings, age/territory settings and reviewer access against release behavior. Existing approved privacy/delete/terms pages and saved Google declarations need only an accuracy recheck for changes, not automatic recreation. Do not promise unfinished social feed/friends features as available.

**Immediate priorities for the December target:** unblock Google closed testing/recruitment in parallel with production ownership/trading work, then distribute the next feedback build for acceptance. This audit does not authorize public submission or enable production payments. Official references: [Google closed-test gate](https://support.google.com/googleplay/android-developer/answer/14151465) and [Apple initial IAP submission](https://developer.apple.com/help/app-store-connect/manage-submissions-to-app-review/submit-an-in-app-purchase).

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
| Trading and finite series | Required for v1, owner confirmed November3 | Globally capped edition inventory, exact-spare reservations, atomic two-sided trades, expiry/cancel/block/report, sold-out states and two-account concurrency tests |
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

Preserve current beta saves and all full fusion/personality records. No arbitrary production imports of legacy counts. No live paid draws until the paid-box gates pass; no live trading until its acceptance gates pass, no invented edition sizes or server odds. Repository main is older than the last installed build; release branch intentionally retains the phone-tested integration dependencies, with upstream reconciliation required before merge.

Working branch: codex/release-readiness, isolated /private/tmp/pockle-release-readiness-20261103, recovered from ec4e9c3. The owner's Claude checkout remains dirty and must not be reset/staged wholesale. Old temporary caches were cleaned; old log paths are historical evidence, not current reruns.

## RevenueCat inspection — November 3

Read-only browser inspection found an accessible RevenueCat account with only the Biotadex project in its project selector. No Pockle project or store products were created or changed. Pockle has no Google Play app listing yet. Existing Biotadex configuration must remain isolated.

RevenueCat supports Unity and non-subscription purchases; repeated boxes are consumables rather than permanent entitlements. RevenueCat transaction verification is not proof that Pockle delivered a box. An authenticated server receipt/transaction ledger must atomically deliver once, handle retries and pending outcomes, and account for refunds. Existing account does not establish Pockle store credentials, products, integration, approved pricing or paid draw policy. Sandbox setup can be prepared without enabling live checkout; owner subsequently made paid boxes mandatory for v1. [Implementation path](PAID_BOX_LAUNCH_PATH.md).

Sources: [RevenueCat Unity installation](https://www.revenuecat.com/docs/getting-started/installation/unity), [RevenueCat non-subscription products](https://www.revenuecat.com/docs/platform-resources/non-subscriptions).

Price update: owner approved US$0.99 baseline per single box November3. Discounted randomized packs are a tentative idea, with collection selection and pack size/price unresolved. No live offer was created.

Launch pricing selected under owner delegation: CollectionBox1 US$0.99; MysteryMix3 US$2.49; MysteryMix5 US$3.99. Buyer picks a collection only for the single; mixed packs randomize collection and finish. See [pricing matrix](LAUNCH_BOX_PRICING.md). No store products have been published.

Duplicate policy confirmed: duplicate toys remain owned spares usable for fusion and trading; trading's inclusion in the first release is awaiting clarification. No live trade implementation is claimed.

## November3 — trading and finite series required for launch

Owner confirms trading belongs in the first release and series become unavailable for new awards when their finite supply is exhausted. Earlier deferred-trading statements are superseded. Owned toys remain playable/tradable after sellout. The existing editions/copies/ledger/trade design is now on the critical path, not a later feature. Actual edition sizes, remaining-supply odds behavior, legacy production eligibility and fused-result treatment must be settled before launch. A committed submission date cannot precede this added work and two-account testing.

Edition caps approved November3: per-finish worldwide supply Common10,000/Rare6,000/Epic3,500/Secret1,600. Wally total116,600. Draw/boost/depletion rules remain to settle; no live mint/configuration claim.

## Native iPhone build checkpoint — November3

After owner accepted Xcode terms, the diagnostic nativearm64 iPhone build succeeded. Actual app packaging checks confirmOAuthcallback and linked Keychain bridge/Security framework. Subsequent owner-authorized Apple Development signing, device registration and physical iPhone installation succeeded. Deep strict signature verification passed using normal Mac trust access. Remote activation reported a locked phone, then CoreDeviceService initialization timeout on retry; visible startup awaits owner confirmation. Apple sign-in, real Keychain persistence, iOS walking and store readiness remain unverified. This fresh iPhone install did not restore the Android collection.


November 3 owner device acceptance: onboarding runs on the physical iPhone. Apple browser sign-in returns HTTP400 provider-not-enabled; public live Auth settings independently confirm Apple disabled and Google enabled. Complete separate Apple provider credentials/setup before retry. Initial reported location prompt is not yet identified; no location request found in runtime source or privacy keys in the built Info.plist.


November 3 Apple provider setup completed: approved primary App ID com.playpockle.app, Services ID com.playpockle.app.signin, dedicated login key and Supabase OAuth secret configured. UI/public settings confirm enabled and PKCE request redirects to Apple. Full owner iPhone Apple login and Keychain restart acceptance still pending. OAuth secret expiry May 2, 2027; rotate beforehand. No app rebuild or automatic collection restore performed.


November 3 owner acceptance: real Apple sign-in on the installed iPhone succeeded (“worked!”). Session persistence after cold restart and token refresh remain unverified; no cross-device collection restore is implied.


## November 8 — Apple TestFlight checkpoint
Production-ID version1.0/build8 uploaded and processed; owner export compliance pending. All13Apple products now have verified saved metadata/prices/storefronts and RC mappings. Distribution IPA verified,23native checks/Core/Source pass. Apple sandbox purchase, restart/retry acceptance and IAP review screenshots remain unverified. Missing UnityRuntime dSYM warning affects crash symbolication. This does not resolve authoritative inventory/trading/stock or authorize public submission. See docs/backend/APPLE_TESTFLIGHT_SETUP.md.


## November8 — Google closed-test preparation
Google explicitly requires12testers continuously opted in for14days;0observed. Alpha draft created,USstaged,latestAndroid1.0(8)verifiedreadyforownersigning. Storedeclarations/revieweraccess/listingandcohortconfigurationblockclosedpublication. Internalv4remainsactiveanddoesnotstarttheclock. GoogleGrouprecruitmentrecommended; no group/invitations created. See docs/backend/GOOGLE_CLOSED_TEST_SETUP.md.


## November 9 — Android community beta 9 submitted

Google accepted the owner-signed `com.playpockle.app` 1.0 (9) into Alpha; removed version 8 from the draft. Bundle validation has no blocking errors and two crash-analysis warnings (mapping file and native debug symbols). Owner saved Advertising ID = No. Submitted 14 changes for the United States closed test using `pockle-beta-testers@googlegroups.com`; Publishing overview now shows **Changes in review**, with automated quick checks still running. This is not a live closed test or production release. Removed the unrequested Google Play Games on PC opt-in before submission. New community-beta purchases stay disabled; existing paid recovery and walking remain. Evidence: `docs/backend/google-community-beta9.json` and `docs/backend/google-testing/build9-submitted.png`. Wait for approval, then verify enabled enrollment link and fresh tester login. The 12-testers/14-continuous-days requirement has not started based on this submission.
