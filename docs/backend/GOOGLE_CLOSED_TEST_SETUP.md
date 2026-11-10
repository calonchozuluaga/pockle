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


Owner decision, November8: intended launch audience13+ (13–15,16–17,18+). Google currently blocks the questionnaire until Ads and Sign-in details are complete. AdsNo is prepared for ownerSave; reviewer account is testerpockle@gmail.com, with credentials reserved to owner. No age setting or runtime gate applied yet; align marketing/onboarding with this productdecision, and complete IARC independently. No under13accountflowapproved.


Latest Console verification: Ads is Complete, 4/11 tasks done. Sign-in details is still incomplete; Target audience remains locked on that prerequisite. Owner asked to complete both Add and outer Save and establish the dedicated reviewer player profile. No reviewer credentials inspected, no audience ranges applied yet.


Reviewer login diagnostic: server confirms LuckySquish21 claimed, but no reviewer cloud snapshot. Owner is using an older Android installation. Backup/account-link failure is suspected; name-claim UI hides that distinction. No save reset or ownership transfer performed. Android connection required to confirm installed package and account-link state before recovery.


Device verified: Pixel7Pro runs Play build4, not development build. Its private save is protected; no local owner marker was read and no data changed. A separate Android user is proposed for clean reviewer setup, pending owner authorization. Do not report the account-conflict hypothesis as proven or delete the personal save.


Owner-authorized reviewer recovery: Pixel user10 “Pockle Review” created, existing Play build4 enabled and user10 selected. MainCarlosuser0 retains installed Pockle with a different private data directory. No data clear/transfer/packageupdate. Owner must complete Android setup and Google sign-in on phone; reviewer backup and playable acceptance are still pending. Return to Carlos using Android user switcher. This is isolated test setup, not a fix for the misleading name-check UI.


Latest verified: reviewer LuckySquish21 has1cloudbackup/34rows/0fusions. Sign-in details Complete, approved13+ ranges saved; Non-government and Game/Casual saved. Publicsupportalias+website saved/published after ownerconfirmation; noappreleasepublished. GroupdraftunderCarlosZulappcanuse@googlegroups.com to expose publicjoining; @zulapp.comonlyofferedorganizationjoining. Finalcreationpendingownerapproval, memberlist/postingrestrictedtoowner, noWorkspacepolicychange. ExistingonboardingcodealreadyenforcesMinimumAge13; earlieragegatenewUIrequestshouldbereadastest/alignmentrequest. No all-paid-content reviewer entitlement verified.


Latest verified: owner created public pockle-beta-testers@googlegroups.com; About confirms public self-join, conversations for members, owner-only posting/member list. Member email visibility tightened to owner-only and reload-verified. Group saved as GoogleGroups eligibility on Alpha, feedback support@playpockle.com. Current group has only its creator; no invitations/posts sent. Public group join: https://groups.google.com/g/pockle-beta-testers. Play opt-in/download remain disabled until the closed release is published. Dashboard8/11 complete; Content rating, Data safety and listing assets remain. IARC Game selected, Terms unchecked pending owner acceptance; no review submission performed.


## November 8 — Data safety draft ready; final declaration pending
Owner approved individual-entry and draft saves. Seven entries completed: name/email/UserIDs, purchase history, optional fitness, gameplay(other actions), installation IDs. AllCollected/notephemeral; purposes verified in expandedpreview. RevenueCatActiveintegrationfilterreturnsnone. Publicdeletionpageverifiedonline with30dayrequestinstructionsandpurchase-recordretention. Draftsavedexplicitly;finalSaveuntouched. PrivacywebsiteomitsRevenueCatandneedscorrectionbeforeownerfinalizes;requestinCodexhandoff. ContentratingalreadyComplete;noappreviewsubmission. Proof:data-safety-draft.png.


## November9 — next closed-test execution checkpoint

Owner approved latest UI preview and requested its TestFlight delivery followed by the submission list. New UI source6a90c37 is exported/uploaded as TestFlight1.0(11), awaiting Apple processing. No Android build containing those fixes is published yet.

Live Play dashboard remains10/11setup tasks complete, store listing incomplete, closed test unpublished and0opt-ins. All remaining setup declarations other than the store listing showComplete. Draft listing has approved text, icon, featuregraphic and four actual gameplay screenshots. Review step has an unanswered AI asset declaration; owner has been asked to review the individual labels and Save personally. Nothing was submitted for review. Google's current asset-specific guidance was read: https://support.google.com/googleplay/android-developer/answer/17262077.

Execution order after the owner saves that draft:
1. Prepare the matching Android community beta from approved UI source. Keep native checkout disabled for general community testers while the server accepts sandbox transactions only; owner payment testers must be explicitly covered by Play license testing before testing purchases. Do not assume GoogleGroup membership grants license-test billing. Existing release8 must not be rolled out unchanged to general testers while it can open real checkout. Google explicitly documents potential actual charges for test-track users without license-tester status: https://developer.android.com/google/play/billing/test.
2. Verify Google OAuth audience permits newly recruited users; a successful owner/reviewer login alone is not proof of public tester access. Any required publication/security-sensitive audience expansion gets action-time owner approval. Do not create guest accounts.
3. Validate the new signed AAB and reviewer access, verify group/country selection, then preview the exact closed release and submit/roll out only within the owner's authorized test scope. Owner enters upload-key passwords and completes any legal declarations personally. No community invitations or external posts are authorized.
4. Provide the existing GoogleGroup join link and actual enabled Play opt-in link after the release is live. Verify actual opt-ins; group membership alone does not start the12tester/14day requirement. Production release remains disabled until its separate gates pass.

No community checkout gate, new Android build, OAuth audience change, closed rollout or opt-in is claimed complete by this checkpoint.


November9 implementation: community checkout guard and build-channel resource prepared on codex/google-community-beta. Android-target native57/57 pass (new-entry blocking, prior payment recovery, actual13quote fixtures, delivery persistence, checkout UI and walking); fullCore/source325files1143GUIDs0fail. No server/schema or HUD changes. iOS11 remainsTesting on its prior source. MatchingAndroid9community artifact will contain the latest approvedUI with native purchases unavailable for general testers; existing purchase recovery remains. GoogleCloud new-user audience check waits at owner password/2FA for carlos@zulapp.com. No closedrollout or newtesterinvitation yet.


November9 final artifact: sourceec229ab builds communityAndroid1.0(9). Bundletool validates; finalmanifest packagecom.playpockle.app,SDK26/36,Billing9.1.0, singleTop(enum1), noAD_ID/GPS/camera. Actual hashedUnityassetfiles contain all8disabled owner flags and CheckoutBuildChannel=community-beta; source resource restoresowner-sandbox afterbuild. Stripped only debug JARsignature records and SHA256-verified every retained payload. Owner-signable499142993byte bundle is Builds/Pockle-Play-Internal-v9.unsigned.aab, SHA256f3ae4eb2e7b11fc36d16e452f241103ee71d0c2b40046a2e317ccd3ce381cd59. Owner asked to sign using existing key, no --create-key. No upload/closedrollout yet. Evidencegoogle-community-beta9.json. Incidental Unitysettings/importer/iconrewrites restored; ownerruntimecheckoutuntouched.

OAuth correction after current docs/readback: PockleCloud audience isExternal/Testing with one testuser; no registered sensitive/restricted scopes. The client adds no scopes; SupabaseGoogle defaults toemail/profile. Google explicitly exempts basicidentity-only apps from the test-user allowlist, so Testing status alone must not be reported as a proven communitysign-inblock. Prior reviewerGooglelogin outside the one listed testuser is consistent with that exception. A fresh recruitedtester login remains deviceacceptance, and branding/publicverification still needs finishing before publiclaunch. Publishapp currentlydisableduntilBrandingcompleted; no scopes/permissions/audience changed. Sources: https://developers.google.com/identity/protocols/oauth2/production-readiness/overview and https://github.com/supabase/auth/blob/master/internal/api/provider/google.go.


## November 9 — Android community beta 9 submitted

Google accepted the owner-signed `com.playpockle.app` 1.0 (9) into Alpha; removed version 8 from the draft. Bundle validation has no blocking errors and two crash-analysis warnings (mapping file and native debug symbols). Owner saved Advertising ID = No. Submitted 14 changes for the United States closed test using `pockle-beta-testers@googlegroups.com`; Publishing overview now shows **Changes in review**, with automated quick checks still running. This is not a live closed test or production release. Removed the unrequested Google Play Games on PC opt-in before submission. New community-beta purchases stay disabled; existing paid recovery and walking remain. Evidence: `docs/backend/google-community-beta9.json` and `docs/backend/google-testing/build9-submitted.png`. Wait for approval, then verify enabled enrollment link and fresh tester login. The 12-testers/14-continuous-days requirement has not started based on this submission.
