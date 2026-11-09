# Apple purchases and TestFlight — November 8, 2026

Work branch: `codex/ios-testflight-purchases`, based on the installed build7 integration (`cdd1eae`). The existing development app `studio.pockle.prototype` and its local save are preserved. Store/TestFlight app: **com.playpockle.app**, App Store Connect **6830781336**, version **1.0**, build **8**. This installs separately; no legacy inventory transplantation or production mint is implied.

## Verified account and store connections

- App Store Connect Paid Apps Agreement, banking, U.S. tax form and compliance sections show **Active**. No financial forms, account details or agreements changed.
- Separate RevenueCat project **Pockle** (`9abea771`), Apple app `appde6a12c13c`, bundle `com.playpockle.app`. Both IAP-key and App Store Connect API-key validations show **Valid credentials**. Private keys were neither read nor entered by Codex.
- Default RevenueCat offering **launch_boxes_v1** has all thirteen approved packages mapped to the matching Apple products and existing Android products. Biotadex untouched.
- Existing public Apple SDK key is configured in `Resources/Backend/PocklePurchases.json`. Platform guard accepts `appl_` only on iOS and `goog_` only on Android. No secret key embedded.
- All thirteen existing Apple consumables now have approved English (U.S.) display names/descriptions, US base prices (collection $0.99, mix3 $2.49, mix5 $3.99), Apple-localized schedules and all 175 current storefronts. Reloaded all thirteen product pages and independently verified saved metadata, availability and actual US price. Exact rows are recorded in testflight-setup/status-20261108.json. Review screenshots remain missing; no IAP or public app submission performed.

## Build and signing

`Pockle.Editor.TestFlightIosBuild.Export`, with `POCKLE_TESTFLIGHT_BUILD_NUMBER=8`, creates a nondevelopment version1.0 export at Builds/iOS-TestFlight. It temporarily disables all eight owner grant/face-test flags, synchronously imports them and restores original bytes/settings in finally. Source owner grants are retained for the unchanged development build. The generated project includes StoreKit/IAP capability. Runtime/export checks find all143approved audio paths, Apple public SDK key and eight disabled flag payloads. App Store1024icon is opaqueRGB.

Owner-approved Apple Distribution certificate and App Store profile created; owner completed the Mac Keychain authorization. Final archive and App Store IPA exported successfully. Actual IPA passes deep strict codesign verification: exact bundle/version/build, exact team/app entitlement, get-task-allow=false, beta-reports-active=true, no development device list. Public SDK key, eight disabled owner flags and all143audio paths verified in final signed payload. Three packaged privacy manifests observed; this is packaging verification, not completion of legal privacy disclosures.

Uploaded successfully to App Store Connect. Apple processed build1.0(8), now shows **Missing Compliance**. The App Encryption Documentation form is open for the owner's legal export declaration; no answer selected by Codex. No internal group/invitation yet, no public App Store submission. Upload has a nonblocking warning: missing UnityRuntime.framework dSYM UUID B5BCB31B-E424-3C81-A0EE-2EF2ADFA93D7. Matching symbols were not found in the installed pinned Unity iOS module; engine crash symbolication remains limited. Do not fabricate symbols or call this resolved.

Artifacts: Builds/Pockle-TestFlight.xcarchive and Builds/TestFlight/Pockle.ipa (ignored local build outputs). Upload log /private/tmp/pockle-testflight-upload8.log reports Upload succeeded / EXPORT SUCCEEDED. Preliminary unsigned preflight passed; one premature export before archive creation failed archive-not-found and was retried only after successful archive completion.
Native Unity6000.6.5f1/iOS-target checks: **23/23** pass (CheckoutUiTests, PaidDeliveryRuntimeTests, CollectionVoiceTests). Full portable Core checks pass. Source checks:288C#files/3profiles/5asmdefs/1046GUIDs, zero failures. Source helper is guarded by UNITY_IOS so Android-only editors do not require the iOS Xcode assembly. Automatic Unity settings/icon/model metadata changes restored. Final distribution IPA validation passed; actual Apple sandbox purchase remains pending.

## Purchase acceptance still required

The existing local server handler verifies RevenueCat non-subscription transactions, accepts Apple/Google sandbox stores and delegates idempotent delivery to the saved transaction ledger; client callbacks cannot choose the outcome or claim a grant. No Supabase function/schema/deployment changed here. Local code review is not a fresh live backend acceptance test.

Once TestFlight is available, the owner signs in and tests a sandbox single-box purchase: see localized price and actual server odds, confirm sandbox checkout, get exactlyone saved toy, view the reveal, restart and verify retained ownership. Retry/recovery must not charge again, reroll or grant twice. Packs additionally require exactly3/5persistent outcomes. Production sales and App Store submission remain gated by the full launch acceptance path, including the inventory/trading/stock work already listed in RELEASE_READINESS.md. Do not treat TestFlight packaging as complete store readiness.

Owner-only remaining actions, if prompted: Apple export-compliance/legal answers and phone sign-in/purchase prompts. Existing paid agreement/bank/tax setup needs no repeat action based on today's inspection.

Sources: [Apple sandbox setup](https://developer.apple.com/help/app-review/before-submitting-for-review/configure-in-app-purchases/), [Apple product metadata](https://developer.apple.com/help/app-store-connect/manage-in-app-purchases/view-and-edit-in-app-purchase-information/), [RevenueCat SDK configuration](https://www.revenuecat.com/docs/getting-started/configuring-sdk), [TestFlight](https://developer.apple.com/testflight/).


November8 follow-up: owner completed encryption declaration. Apple cleared compliance. Internal group Pockle owner internal created with automatic distribution off; build1.0(8) added and visibly Ready to Test. Carlos-only invitation prepared, not sent pending explicit message authorization; no other team members selected. Phone sandbox acceptance remains pending.


November8: owner explicitly authorized invitation to carlos@zulapp.com only. Sent using Apple Add Testers. Live group confirms 1Tester/1Build, Carlos Internal status Invited. No other tester selected, automatic distribution off, no role/admin changes. Owner should open invitation on iPhone, install TestFlight build1.0(8), sign in and perform sandbox purchase/restart acceptance. Existing development app/save retained separately; no purchase test or install claimed.
