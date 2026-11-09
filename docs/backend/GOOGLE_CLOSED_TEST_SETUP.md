# Google closed test — November 8, 2026

Branch: `codex/google-closed-testing`, from verified integration `573eda8`; origin/main fetched at `d5b2294`. Separate checkout `/private/tmp/pockle-ios-20261108`; owner checkout and Claude HUD untouched.

## Actual Console state

Pockle package `com.playpockle.app`, Play app `4975391761406742908`, developer Zulapp `7043066099294508143`. Existing internal version4 is active (older115 roster, before latest materials/audio); no claim that new Android version8 is published. Existing closed Alpha track `4700851388204598089` had no release. Created draft release1, selected/saved United States as a narrow initial test country, confirmed Targeted; change staged for review, not published. No test emails selected; unrelated17-person list and owner-only license-testing list unchanged. Closed join link remains disabled until publication.

Dashboard requires at least12 testers opted in continuously for14days and a production-access application. Zero opt-ins observed. Internal testers do not satisfy the closed-test requirement; anyone participating internally must leave internal testing before joining the closed test. No date guarantee: the14days begin with the required continuous opt-ins, not this setup or an invitation. Production access/release review takes additional time. December remains a target, not a verified ready-to-submit state.

## Recruitment recommendation

Use a dedicated Google Group for community recruitment. Testers first join that group with their Google Play account, then opt in using the closed-test link. This avoids collecting each email manually. Group membership alone is insufficient. No group has been created, no public joining permissions enabled and no invitations/community posts sent. Owner chooses/approves the group and test countries before publishing. A dedicated15–20tester cohort provides a buffer for the12minimum; don't count inactive/sign-up-only people as meaningful test evidence.

## Updated Android build

Unity6000.6.5f1 native release-modeARM64 IL2CPP build1.0(8) succeeds. Build helper accepts POCKLE_PLAY_BUILD_NUMBER, temporarily disables8owner test flags, restores flags/settings/version, and keeps signing private to owner. Final bundle manifest exact package/version8, minSDK26/target36, Billing9.1.0, singleTop. Bundletool validates.143approved sound resource paths,8disabled flag payloads and configured Google publicSDKkey independently found inside actual bundle. Debug JAR signature stripped; payload bytes identical for all retained entries. Unsigned ready at owner Builds/Pockle-Play-Internal-v8.unsigned.aab. Owner upload-key signature pending; no upload/rollout claimed. See google-closed-test-build8.json. FullportableCore and Source288files/3profiles/5asmdefs/1046GUIDs0fail; diffcheckclean. No runtime/HUD/save/backend changes or new generation. Automatic tracked Unity settings/icon/modelmetadata restored.

## Eleven setup tasks / factual answers for owner review

| Task | Current-build evidence / preparation |
| --- | --- |
| Privacy policy | Existing live Pockle privacy page was opened by owner; dashboard later showed1/11completed. Confirm URL and policy accurately cover accounts, purchase processing, cloud progress, steps and deletion. |
| Sign-in details | Yes: Google/Apple authentication required. Owner-only dedicated review credentials go directly into Console. Reviewers cannot create accounts/use their own; reusable full-content access must be arranged and tested. Personal account must not be shared. Form left open; no credentials read/entered by Codex. |
| Ads | No adSDK/advertising in current manifest/source. Selling in-app boxes does not itself make the app contain ads. |
| Content rating | Game; disclose in-app purchases and paid randomized items/loot boxes. Complete every question against actual content. Cosmetic toy rarity is not a reason to deny randomized purchases. Do not invent a rating. |
| Target audience | Owner product decision, not inferred from cute art. Determine intended ages and actual child-directed design/marketing. Do not choose13+/18+ merely to bypass children policies. |
| Data safety | Collects data: Yes. Account email/name when returned byOAuth, userID/collector name; purchase history; gameplay/collection/fusion progress; fitness step totals included in cloud snapshot; installation/source identifier. HTTPS encrypted in transit. No GPS/location permission, contacts, microphone/camera or photos access in final manifest. Review cloud/provider practices before final attestation. |
| Government | No, this is an independent toy game. |
| Financial features | No financial-services features; GooglePlay consumer digital purchases are separate. |
| Health | ACTIVITY_RECOGNITION means Google explicitly requires this form. Prepared Activity and fitness, optional step counting for walking rewards, noGPS/routes/no medical use, step totals in cloud save. Google shows no regional requirements. Owner must review/clickSave. |
| Category/contact | Game; casual/casual collection classification subject to available menu. Public support address/website must be owner-confirmed. No private contact fields published without authorization. |
| Store listing | Need approved text,512icon,1024x500feature graphic and actual gameplay screenshots. Do not claim trading/authoritative finite supplies are complete. Existing runtime/store artwork can be reused with accurate captures; no new AI generation approved/needed here. |

Health draft text entered (not submitted):

> Pockle is a toy-collection game. With the user’s permission, it uses Android step-counter sensor data to count walking steps and show progress toward in-game walking rewards. Physical activity access is optional; users can decline it and continue playing with their toys and collections. The app does not use GPS or collect walking routes, and it does not diagnose, monitor or treat medical conditions. Step totals and reward progress are included in the signed-in user’s cloud save. The permission is used for this walking-reward feature, not advertising.

## Data safety detail still needs owner review

This is a source/manifest-based draft, not a completed privacy certification. Supabase receives account identity, collectorname and fullsave snapshot including steps/progress. RevenueCat receives account-linked purchase records. RevenueCat requires purchasehistory disclosure, not ephemeral, required, purposes Appfunctionality and Analytics per its documentation. Additional configured non-service-provider integrations can change sharing answers; no assumption of None across all providers. Source contains no advertisingID collection call and final manifest has noAD_ID permission. App-generated source_id may still require DeviceorotherIDs disclosure. Do not confuse noGPS with nofitnessdata. Account deletion is request-based with purchase-record retention; don't promise every purchase record is immediately deleted. Owner must verify support/deletionURL and actual RevenueCat deletion process before certifying deletion practices.

Console additionally flags an urgent Zulapp payments-account issue and trader declaration dueFebruary2027. Owner was asked to review payments personally; no bank/account details read or changed. No terms accepted, financial operations or public production submission performed.

Sources: [Google testing requirements](https://support.google.com/googleplay/android-developer/answer/14151465), [Google track setup](https://support.google.com/googleplay/android-developer/answer/9845334), [App-content review setup](https://support.google.com/googleplay/android-developer/answer/9859455), [Data safety](https://support.google.com/googleplay/android-developer/answer/10787469), [RevenueCat disclosures](https://www.revenuecat.com/docs/platform-resources/google-platform-resources/google-plays-data-safety).


November8 follow-up: owner signed version8 with existinguploadkey. jarsigner verifies; retainedbundlepayloadmatchesunsigned exactly; certificateSHA256matchesv4. SignedSHA256 de6ecb787a44a47cd125e37e8125ea0ba15b139669cc8af7fbd2e76f1ff8f4d3. Upload started intoAlpha draft1; Google optimizingfordistribution, no publishedclosedreleaseyet. HealthsavedbyownerandverifiedComplete;dashboard2/11done. Dedicatedreviewer@playpockle.comrecommendedonlyasaregisteredGoogleAccount, notanemailaliasalone. No revieweraccountcreated orcredentialsshared.


Latest verified: Google accepted artifact8(1.0); Alpha release1 saved as “1.0(8)—Closed beta” with accurate beta notes. UI confirmsChangesSaved. Not rolled out/reviewsubmitted. HealthCompleteverified. Community membership does not automatically enable license-test billing: currentlicensegroup only2owneraccounts. Before broaderpublication, either enroll actualpurchase-testaccounts appropriately or gate checkout to thoseaccounts; sandbox-onlyserverrejectingrealtransactions is not sufficient paymentprotection. Never askcommunitytesterstorealpay. Group/audience/revieweraccess/datasafety/listing remain pending.


November8 verified follow-up: Googleprocessed/accepted8(1.0), savedAlpha release1 “1.0(8)—Closed beta” andnotes; previewreports3errors: finishdashboard,missingfulldescription,financialfeaturesundeclared.3warnings:noassignedtesters,nodeobfuscationfile,nonativesymbols. Subsequentlysaved67-character shortdescription/1089-character accuratefulldescriptionasstoredraft;UIconfirmsChangesSaved. No claimthatalllistingrequirementsarecomplete(art/screenshotsremain). No trading/finiteauthoritypromisedintext. Screenshotsdocs/backend/google-testing/release8-validation.png/listing-draft.png. Actualclosedpublication/cohortclockremainsblocked; nogroupcreatedorinvites.


November8 finalformhandoff: FinancialfeaturesNonepreparedafterreviewingactualoptions(no banking,cashout,cryptowallet,lending,paymenttransfers). Regionalstepsaysnoadditionaldocumentation; owneraskedreview+Save,notclicked. Storelistingdraftsavedconfirmationobserved; ConsolethenwarnedunsavednavigationdespiteChangesSaved, leftafterconfirmation; verifytextonreopenbeforepublication. No inventorygrant/revieweraccount/groupcreation, no backenddeploy. Signeddraft8readybutremainingsetupstillblocksclosedrelease.


November 8: Financial features is now verified Complete, with Health and Privacy policy (3/11 tasks complete). Owner created a dedicated Gmail; its confirmed address, group address and intended launch audience are pending. No reviewer or group credentials are recorded here. The closed release remains a draft.
