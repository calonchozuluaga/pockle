# Supabase inventory and rewards — design for review

Prepared October 10, 2026 on `codex/collection-backend-contracts`. This is a schema and server-function proposal, not a deployed backend or an executable migration. It responds to Claude's October 9 `collections-design` and `ui-v2` handoffs. Source rules: [COLLECTIONS.md on Claude's branch](https://github.com/calonchozuluaga/pockle/blob/claude/collections-design/docs/COLLECTIONS.md). Series metadata and lineup motion already exist on `codex/series-metadata-idles` and are included in PR #7; reuse them rather than recreate them.

## Product boundary

One character is one collection, with at least ten regular varieties and an optional secret. Pip is Series 1. Jelly Garden, Midnight Glow, and Gold Confetti are Pip sub-pools, not separate series. No scheduled retirement. Supply exhaustion closes grants, not the owner's shelf. Release dates, products, edition sizes, and production weights require published configuration; none are invented from the beta catalog.

Adopted rules from the newer collection brief: paid boxes never duplicate currently owned toys and close after all regular members are owned; walking boxes may duplicate; five same-series spares craft a missing regular toy; secrets cannot be crafted; friend swaps and have/want trades exchange spares without money. Check-in boxes are adopted, but their cadence, pool, odds, and interaction with walking remain open. Walk thresholds are 1,000/5,000/10,000; final rare/secret weights remain tuning data. The newer brief supersedes the older series handoff calling crafting itself undecided.

Local PlayerPrefs remains authoritative for this beta until the server cutover is implemented. No UI claims live accounts, purchases, inventory sync, trades, finite supply, or sensor verification from this design alone. Fusion and living-shelf personalities stay outside this implementation.

## Identity, access, and migration

Use Supabase Auth anonymous sign-in for an online guest. It creates a real authenticated user with a stable subject; it is distinct from an unauthenticated API caller. Link an identity to that same user when signing up, preserving the player ID and copies. Store session tokens through the platform's secure storage integration. Rate-limit guest creation and claims; do not let reinstalling create unlimited free editions.

Use a stable `player_id` separate from `auth.users.id`. `player_accounts` maps a verified Auth subject to one player. Auth deletion/relinking must not cascade-delete copy history. New public trading requires a permanent, enabled account; `authenticated` alone also includes anonymous guests and is insufficient. Resolve the actor from the verified session at the Edge boundary, then recheck current account status/session for sensitive mutations. Never authorize from user-editable metadata or a request's player ID.

Linking to an existing account requires proof of both sessions through a short-lived server merge ticket. Lock both players, cancel/release their listings and paid reservations, transfer eligible copies atomically, append account-merge events, and deduplicate claims/purchases. Merge claim histories conservatively so neither identity gains another claim in an already claimed period. Revoke the old session and reject its subsequent requests. Do not simply overwrite one inventory or trust a submitted source user ID. Paid eligibility treats merged copies as owned.

The current v2 local save is editable and contains counts, not provable copies. Import it as a separately tagged legacy snapshot with unknown first-found dates. It must not mint production serials or consume edition supply based on client assertions. Recommended cutover: preserve legacy toys for local play, with an explicit legacy badge; only server-issued copies enter production crafting/trading. Any compensating welcome grant is a separately approved, bounded, idempotent server grant. Unknown future IDs remain in the snapshot. Keep a backup until the client acknowledges the committed migration receipt. An interrupted upload retries the same operation instead of importing twice.

## Proposed relational schema

Authoritative tables live in a non-exposed `pockle_private` schema. Edge endpoints expose filtered DTOs. Enable RLS as defense in depth; revoke `anon`/`authenticated` table mutation and function execution. Public ledger projection contains no Auth UUIDs, email, private player IDs, purchase tokens, or step evidence.

| Table | Keys, fields, and invariants |
| --- | --- |
| `players` | UUID primary key; display name; public-ledger opt-in; account status; created time. Account status is server-owned; clients cannot make themselves eligible. |
| `player_accounts` | Auth subject primary key; player FK/index; unique active mapping; disabled time. No delete cascade into copies/ledger. |
| `characters` | Stable string ID; display name; accent tokens and motion recipe IDs. |
| `series` | Stable character collection ID; character FK; unique positive series number when assigned; nullable release UTC date; field/ink colours; draft/published state; immutable published policy revision. No retirement date required. |
| `collectibles` | Stable ID; series/character FK; finish ID; lineup position; secret flag; art availability; rarity category. Unique `(series_id, position)` and partial unique `(series_id) WHERE is_secret`. Publication validates one character, ten regular members, approved art and complete pool data. |
| `editions` | Collectible primary key/FK; immutable published size; allocated counter; reserved counter; next serial. Nonnegative counts, `allocated + reserved <= size`; serialized row lock for allocation. Retired copies never replenish supply or recycle serials. |
| `box_offers` | Offer ID; series; explicit pool; platform/product bindings; price tier; enabled state. Store-localized price comes from billing; a proposed USD label is not a verified checkout price. |
| `pool_versions`, `pool_entries` | Immutable version; series; source and walk tier; effective time; entry `(version, collectible)` primary key, positive integer weight. Validate every outcome belongs to that series and has an edition. Changing odds creates a version. |
| `reward_policies` | Version; source; cadence and period definition; eligible series selector; craft cost; configured flag. Check-in begins disabled/unconfigured. Freeze rule values for an active quote. |
| `copies` | UUID key; `(collectible_id, serial)` unique; edition-size snapshot; first-found UTC time; immutable mint source; initial player; current owner; active/reserved/retired status; owner-acquired time. Original discovery/source never changes on trade. FK/index collectible and owner; partial `(owner, collectible)` index for active/reserved copies. |
| `operations` | UUID key; actor; kind; idempotency key; canonical request digest; result/error; completed time. Unique `(actor, kind, idempotency_key)`. Reusing a key with different input fails. Mutations and their final result commit together. |
| `reward_claims` | Unique `(player, reward_group, period_key)`; source; policy version; operation; copy; claimed time. The policy defines whether walk/check-in use separate groups or share one replacement group. |
| `walking_days` | Unique `(player, period_key)`; accepted cumulative steps; evidence revision; verification level; granted claim link. Raw sensor events are private and retained only as needed. Client clock/step total is never sufficient proof. |
| `draw_quotes` | Actor; source/offer; eligible IDs, integer weights and total; pool/policy revisions; inventory revision; expiry; status; opaque quote ID. Normalized odds are calculated from exactly these weights. |
| `paid_reservations` | Quote/operation key; actor and series; hidden selected collectible/serial; expiry/state. At most one live purchase session per actor/series. Private draw results stay hidden until fulfilment. |
| `purchase_receipts` | Unique `(platform, environment, store_transaction_key)`; actor-bound reservation; verified product; state; operation/copy; store acknowledgement state. Environment isolates sandbox receipts from production. Sensitive receipt payload is private. |
| `copy_reservations` | Copy ID unique; listing/trade/craft operation; expiry/status. One spare cannot participate in two concurrent operations. |
| `trade_listings`, `trade_confirmations` | Listing owner, offered copy, requested collectible, version/status/expiry; confirmation unique `(trade, player, offer_version)`. Immutable confirmed terms; no currency fields. Indexed open have/want terms and owner. |
| `friendships`, `blocks`, `reports` | Canonical player pair and acceptance state; directional block key; moderation reports. Check either-direction blocks and current account status at completion, not just listing creation. |
| `ledger_events` | Monotonic event ID; copy FK; operation FK; per-operation event ordinal unique; mint/trade/craft-retire/account-merge/reversal type; server timestamp; private from/to players; immutable payload. Append only. |
| `legacy_imports` | Unique source import ticket; target player; input digest; sanitized snapshot; migration receipt. No production serials. |

FK lookup columns and owner/claim/listing predicates need explicit indexes; Postgres does not automatically index all foreign keys. Query owned copies and ledger with keyset pagination. Remaining supply is `size - allocated - reserved`; public counts are advisory snapshots, and only the grant transaction authorizes allocation.

## Server boundary and transaction rules

Unity uses the publishable key and a user access token. Secrets stay in server configuration. Edge verifies the token, applies request limits, validates the input shape, and resolves the actor. Store verification uses server-to-store credentials. Every inventory mutation calls one Postgres transaction function; multiple separate REST inserts are not an atomic grant.

Prefer `SECURITY INVOKER` functions in the private schema, executed only by a narrowly granted backend role. Do not expose a service-key RPC accepting an unchecked actor ID to the client. Revoke default `PUBLIC` function execution, use a fixed/empty search path and qualified names, and explicitly grant each server function. Ledger writes are INSERT-only for the runtime role; reject UPDATE/DELETE with an immutable-event trigger. Administrative corrections append compensating events. Backups and reconciliation still matter: append-only application permissions do not make an administrator cryptographically unable to edit the database.

If the chosen Supabase transport cannot call private functions directly, add a narrowly scoped exposed wrapper callable only by the backend role, or use a pooled server SQL connection. Do not enable private schema exposure for clients as a shortcut. Review this choice before executable migrations. Read APIs either use the user's JWT with ownership RLS or explicitly filter a backend query by the verified actor. Views in exposed schemas use `security_invoker = true` and correct underlying RLS; privileged public ledger reads must be projected through the Edge DTO instead of bypassing policies with a broad view.

All mutations use this global lock order: player rows sorted by UUID; operation row; policy/quote/reservation rows; relevant edition rows sorted by collectible ID; copy rows sorted by UUID; listing/confirmation rows sorted by UUID. Every path, including expiry/reconciliation, uses the same order. Re-read eligibility after taking locks. Lock the full eligible pool before calculating odds/drawing, so a sold-out outcome cannot race the sampler. Do not use `SKIP LOCKED` to silently remove a busy toy and change its probability.

Use bounded retries for deadlocks/serialization failures with the same idempotency key. Draws use a cryptographically secure server RNG with integer rejection sampling over the exact total weight. RNG/output are private. Roll, serial, ownership, claim/receipt consumption, ledger, inventory revision, and saved operation result commit atomically. Failures roll back all of them. Reveal begins only after that commit; reconnect returns the same copy/result.

Locking the player row serializes ownership-affecting operations so two paid requests cannot both consider the same toy missing. Listings reserve exact copies and require at least one unreserved active copy of each offered collectible to remain afterward. Recheck the aggregate spare invariant at completion, including all reservations and items removed by that operation. It must hold across concurrent crafting, listings, trades, and account merges, not just within one screen.

## Function and endpoint contracts

Inputs below omit actor/session, which always come from verification. Mutation responses include operation ID, committed inventory revision, and copy/event IDs. Stable error codes distinguish unauthenticated, account disabled, unsupported policy, stale quote, exhausted supply, already claimed, not spare, terms changed, receipt mismatch, and payment reconciliation pending.

| Endpoint / transaction | Input and result |
| --- | --- |
| `catalog_snapshot` | Published revision → safe series/member metadata, availability, products and display odds; unknown dates stay null. Secret slots hide name/art until owned; never expose a private hidden draw. |
| `inventory_snapshot` | Cursor/revision → caller's paginated active copies, immutable first-found/source, serial/edition size and owner-acquired history; legacy inventory separately tagged. |
| `quote_box` | Source, offer/tier → immutable exact eligible distribution, total weight, period, expiry and quote ID, or no eligible outcomes. No grant. |
| `claim_free_box` | Quote ID, idempotency key → one atomic check-in or walk grant; duplicate request returns its original result. Revalidate policy, verified steps, period and current supply. Stale distribution returns refreshed odds for confirmation rather than silently changing a draw. |
| `prepare_purchase` | Accepted paid quote, idempotency key → opaque actor-bound checkout reservation and product/environment binding. Locks eligible supply, draws once, reserves its serial, and freezes ownership-changing operations for that player's series during the short checkout window. |
| `fulfil_purchase` | Reservation, platform receipt/token, idempotency key → server verification followed by one atomic conversion of reservation into owned copy and ledger mint. Store transaction uniqueness overrides different client retry keys. |
| `craft_regular` | Five exact spare IDs, missing regular target, idempotency key → same-series target from remaining edition, five retirement events and one mint in one transaction. Duplicate inputs, secret target, owned target, unavailable/sold-out target fail with no consumption. |
| `create_listing` | Exact spare, wanted collectible, version/idempotency key → reserved have/want listing; account and block checks. Reservation expiry releases the copy without granting anything. |
| `confirm_swap` / `complete_match` | Trade ID and version → each permanent account confirms the exact two-copy terms; completion locks both players/copies and swaps ownership atomically with two ledger events. Cancellation/replay cannot steal a reserved item. |
| `cancel_listing` | Listing/version → owner-only cancellation and reservation release, idempotent. A completed trade cannot be cancelled. |
| `import_legacy` / `merge_guest` | Server ticket and idempotency key → migration receipt; rules and proof above. No arbitrary user IDs or free edition minting from a local file. |
| `public_copy_history` | Public copy reference/cursor → serial, edition, source and events; display names only for opted-in players. Opt-out redacts names retroactively at projection time, while private history stays intact. |

### Paid draws and disclosed odds

Paid eligibility excludes owned regulars and an owned secret, exhausted editions, unpublished/unavailable outcomes, and pending reservations. If no missing regular with supply exists, close purchase even if the secret is missing. This follows the proposed paid edge-case rule; Claude/owner should confirm it before billing goes live. Filter the pool first, then normalize remaining integer weights. The secret's base relative weight is preserved, not a false constant 1-in-72 claim after regulars disappear. The quote shows the resulting per-player probability.

For purchase safety, `prepare_purchase` reserves the hidden selected serial from precisely the accepted distribution. A short-lived persistent actor/series guard prevents a free grant, craft, trade receive, or another purchase in that series from making the chosen paid toy a duplicate before fulfilment. It is not a database transaction held open during billing. Reads/play remain available; mutating endpoints report a checkout-in-progress conflict. Rate-limit reservations so they cannot hoard global supply.

After a verified payment, acknowledgement/consumption occurs only after the inventory grant commits. If the network fails, retry the same receipt/reservation. If a payment arrives after reservation expiry/release, do not silently reroll at new odds or discard it: record a durable unfulfilled entitlement for reconciliation/refund under a documented platform policy. Successful charges must end in a grant or an explicit refunded resolution. The late-payment/refund policy, reservation TTL, refund/revocation behavior after a trade, and store integration are launch blockers, not implemented behavior in this pass.

### Walking and check-in

Walking claim uses the newest published, released series with eligible supply. A 1,000-step player can open now or wait for 5,000/10,000; one successful claim freezes that day's tier. Weights and verified step evidence are versioned. No client-selected tier or local timestamp can increase odds. Server calendar UTC is the proposed initial day boundary; owner acceptance and UI disclosure are needed before cutover. If the newest series is empty, return supply exhausted until an explicit fallback policy is published.

Check-in has a separate source `check-in`, one claim per configured period, and the same serial allocator/ledger as any free box. A policy records cadence, eligible older/all series, exact weights, period boundary and whether it shares a daily claim group with walking. Until these choices are settled, it returns `policy_unconfigured` and grants nothing. Do not quietly choose daily cadence or grant both boxes. The unique claim key and player lock must cover all qualifying sources when replacement is configured.

Hardware step counters provide evidence, not tamper-proof proof of walking. First backend release must define anti-abuse limits, device/session binding, attestation where supported, counter resets, offline replay limits, and refusal/review of suspicious totals. The existing local sensor path must not be presented as authoritative background verification. Tablets/no-sensor devices get honest walking-unavailable status and a configured check-in path.

## Implementation order and acceptance checks

1. Claude reviews DTO names against the v2 screens. Confirm paid edge cases, cutover policy and server-day boundary; collect check-in tuning and publication data. This plan does not need those choices to support the new navigation/backdrop APIs.
2. Build a disposable local Supabase schema and pgTAP/concurrency tests. Define least-privilege grants, RLS, indexes, publication validation and immutable ledger constraints. Run advisors; generate a migration through the CLI after the schema is verified. No remote project is changed by this design.
3. Implement Auth guest/linking, own-inventory reads, and legacy migration receipts. Verify cross-account reads/writes fail, disabled/anonymous trade attempts fail, merge replay fails, and unknown dates/IDs survive cutover.
4. Implement allocator, exact quote distributions and free claims. Race the last serial and same-day claim from many sessions; assert one serial/claim, no oversupply, and exactly one ledger result. Reconnect after commit returns the same copy.
5. Add verified store receipts and purchase reservation/reconciliation. Race paid draws against free grants and incoming trades; no duplicate paid toy, no changed accepted odds, no double grant from alternate keys/users, and no acknowledgement before persistence.
6. Add crafting and both trading modes. Concurrent listings/crafts/swaps must preserve one copy per type and consume each spare at most once. Crash after one intended swap leg must leave neither leg committed. Confirm secrets/sold-out/owned targets cannot craft.
7. Integrate Unity only after server contracts pass. Cache read snapshots; queue no optimistic ownership changes. Confirm actual shader/backdrop colour space, tiny-screen layouts, Android storage/billing/sensor behavior and phone frame time separately.

Required database scenarios also include policy/odds changes between quote and open, 1-in-72 base fixture versus filtered paid distribution, empty/sold-out pools, claim-source replacement, device clock rollback, expired reservations with late receipts, reversed trades/blocked players, account deletion redaction, cursor stability and ledger UPDATE/DELETE denial. These are acceptance specifications, not reported passing tests.

## Documentation verified for this design

Reviewed official [anonymous sign-ins and identity linking](https://supabase.com/docs/guides/auth/auth-anonymous), [database functions and execution permissions](https://supabase.com/docs/guides/database/functions), [Edge authentication](https://supabase.com/docs/guides/functions/auth), and RLS guidance through Supabase documentation search on October 10. Anonymous users share the authenticated role; privileged function execution and data ownership need explicit restrictions.

The required changelog index fetch at `https://supabase.com/changelog.md` failed in the available web tool with unsupported markdown content type; the executor's network proxy was also unreachable. No claim of a complete breaking-change review is made. Recheck the changelog and deployed versions before actual migrations or SDK integration. No database, SQL execution, advisors, backend runtime or credentials were configured/tested here.
