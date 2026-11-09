# Pockle UI direction 01

Prepared for owner review. HTML concepts only, using approved local artwork copied without image changes. No Unity HUD, runtime, tests or Resources changed.

## Recommended direction

Peach & plum: tinted page backgrounds, dark plum primary actions, collection art as large purposeful surfaces. Twilight playroom is a comparison option with the same layouts; it can later serve special reveals or become an optional palette. Avoid applying a flat dark overlay to every artwork. Keep cover art vivid, put readable text on intentional surfaces, and use generous tap areas.

## Screens and behavior

- Home: artwork extends behind the main toy; clear Play entry; quiet walking-progress module. Existing 1,000-step goal shown. Hourly cooldown and daily/bank limits are not set by this preview.
- Boxes: collection covers make each product a destination. Rendered carton sits on top as product anchor. Clear collection, one random toy, store price, contents/probabilities link. Mystery Mix remains separate, so selected-collection and mixed-collection products are distinguishable.
- Collection detail: show rarity on every slot, including hidden models. Keep a bottom Get a Pip box entry, opening the matching contents sheet. No direct charging on a browse tap.
- Fusion result: after durable save, show success, spent-spare summary and Play with [name] as the primary action. Make another fusion is secondary; About optional.
- Trading room: the owner chose a **public offers marketplace**, replacing the private invite-room proposal. Browse public spare-for-toy listings and filter by the offered collection and rarity. Each card shows the offered spare, wanted toy and both rarity levels. “View offer” opens a simulated detail showing both toys and sample available counts. The maker explicitly confirms the fixed terms when publishing, so their side is already confirmed while browsing. The taker checks “I agree to this swap” and uses “Simulate accepting offer”; this shows a clearly labeled mock receipt without waiting for the maker to be online. No second maker confirmation or private-room handshake is needed. Posting an offer is explanatory only. Never trade the protected first copy. The real server must atomically validate the current offer version, both exact available copies and ownership, then commit the exchange and receipt. Changed, spent, cancelled or expired offers must fail acceptance without partial transfer. No request is sent or inventory changed in this preview. No live trading is implemented by this page.

All inventory quantities, collector names, public offers and traits are illustrative UI fixtures. Collection pricing examples: $0.99; Mystery Mix 3 $2.49; Mystery Mix 5 $3.99. Real checkout must use localized store price strings. The interactive contents-sheet mockup deliberately displays no made-up probability values and disables purchase because it has no live quote.

## Assets

See assets.json for source paths. Fonts and imagery are already in the project. The Sorbet render is an existing native fusion-material capture and shown in a round frame without removing or painting over its original background.

## Validation

Local asset references and dimensions checked. JavaScript syntax checked. Public-market filters and the asynchronous fixed-offer acceptance flow were checked with a Node DOM harness. Browser visual verification was blocked by the tool URL policy and remains unverified; final visual-direction approval remains for the owner. These are not Unity screenshots, and live purchase/trade behavior has not been tested by this artifact.
