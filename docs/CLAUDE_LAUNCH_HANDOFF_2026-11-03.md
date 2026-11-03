# Current Claude launch handoff — November 3

This consolidates the latest owner decisions and supersedes older deferred-purchase/trading and unapproved-cap notes. Codex's detailed evidence remains in docs/handoffs/codex.md.

## Settled launch scope

Both Apple and Google: paid boxes, walking rewards, collections/toy play, same-character fusion, accounts, toy-for-toy trading, finite official series, and a polished box reveal. Paid duplicates remain spares for fusion/trading. Sold-out official copies stay playable/tradable; no new supply is minted by transfers or fusion.

Launch US-dollar targets: collection-choice single$0.99; MysteryMix3$2.49; MysteryMix5$3.99. Mixed packs randomize collection/finish. Prices are store-localized. Approved worldwide caps per finish: Common10,000;Rare6,000;Epic3,500;Secret1,600. Wally9/3/2/1 total116,600. No invented server serials or live supply counters.

## Odds: reconcile the voice drafts

The owner explicitly delegated choosing sensible odds to Codex. Working recommendation for implementation is remaining-stock tier probabilities, not a fixed88/8/3/1 ladder. The final spoken recap introduced88/8/3/1 as a draft after describing stock-proportional draws; these are different models. Treat88/8/3/1 as a superseded alternate draft, not an approved constant to hardcode. Nothing is deployed.

For a collection: server locks eligible edition rows and computes available copies as cap minus issued minus active reservations. Tier chance is the tier's available copies divided by all eligible available copies. For Wally at full supply: Common77.1870%,Rare15.4374%,Epic6.0034%,Secret1.3722% (about1in73). Chances change as stock/reservations change, not only when a tier sells out. Empty tiers have zero chance; an empty collection closes new awards. The accepted server quote freezes the purchase's rules/results and is the only source for displayed live odds.

Completion help is conditional within the Common tier only: missing Common finishes receive weight multiplier1+2×(owned distinct Common finishes / total Common finishes), on top of their remaining-copy weights. Other Common weights remain unboosted. This preserves the previously approved gradual completion-help shape while protecting the Rare/Epic/Secret tier chances. It DOES change the probabilities of individual Common finishes, so do not say “odds unchanged”; say “Common completion help leaves the other tiers' chances unchanged.” Use the ownership snapshot frozen at checkout; no reroll on resume. This is a transparent working policy selected under delegated judgment, not an already working service.

Mystery Mix working selection: each child chooses uniformly among eligible nonempty launch collections, then uses that collection's server odds. Collections/finishes may repeat; no promised diversity, complete set or rare guarantee. Exact snapshot/count/eligibility and child outcomes are server-owned. The review UI must consume actual per-offer/per-child odds from the runtime rather than reproducing this arithmetic in HUD code. Other collections have differing current rosters: do not show Wally's percentages as universal.

## Claude work that can proceed now

1. **Box reveal design:** actual branded3Dbox and toy, tier treatment, name/finish/personality, server serial/edition count, new/extra, Play/Keep. Sequential pack openings, skip-animation and final summary; Calm/settings support. A mockup must label placeholder serial/stock data clearly.
2. **Checkout screens:** the three offers, localized price, collection choice vs mystery mix, clear odds, stock/sold-out, cancellation/pending payment/delivery recovery. Client success never implies a saved toy.
3. **Trading screens:** exact spare offer/want, both-side preview/confirmation, reserved-copy state, cancel/expiry, report/block and preset actions/no stranger free text. Sold-out toys still tradable. Existing proposed expiry/counter details remain in the backend plan.
4. **Audio pilot in parallel:** docs/AUDIO_PILOT_CLAUDE_BRIEF.md. Nine initial nonverbal happy/curious/playful auditions for Pip/Fox/Wally, existing Magnific credits only; verify tool access/credit cost/rights, no top-up/new subscription. Review page and sources before Resources import. This does not block inventory/payment implementation; stop after pilot for owner listening.

Claude owns PrototypeHud/UI/contracts/tests and audio-generation assets. Codex owns runtime/platform/server, receipt/ownership/fusion/trade invariants, audio playback/action/face binding, native verification and packaging. Agree additive UI contracts in the two handoffs before runtime binding; absent services must not be portrayed as live. No active owner HUD edits by Codex.

## Current evidence and gaps

Latest focused native54/54, portable and source checks pass for the integrated UI/runtime. iOS Unity export succeeded; native compile is blocked by the unaccepted Xcode/Apple SDK license, left open for owner review. iOS motion permission/pedometer, Apple provider setup, signing and physical-device tests remain. Google auth/legacy phone backup were verified earlier; live production inventory/payments/trading are not implemented. Play is a Personal account with Biotadex live but no Pockle listing; Pockle eligibility unconfirmed. RevenueCat shows only Biotadex; Pockle products/project/SDK/webhooks are not connected.

Next acceptance milestone: one actual sandbox purchase and one atomic toy trade on Android and iPhone, with durable saved results/reveal, interruption/retry/refund and full legacy preservation. Then all pack/catalog cases and release/store review package. No promise of store submission within days.
