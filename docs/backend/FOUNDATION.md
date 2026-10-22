# Pockle backend foundation — October 21, 2026

Branch `codex/backend-foundation`. Supabase project `azdzafotawbrplukeqcl` (Pockle, Zulapp, us-east-1, Postgres17). This implements the first account/server-clock slice, not production billing, finite-edition minting, trading or the Wally rarity pilot.

## What is deployed

Six versioned migrations in `supabase/migrations/`, aligned to the versions actually recorded by the remote migration API. The CLI2.81.3 created each file; remote deployment assigned final versions. Private tables are in `pockle_private`, not an exposed Data API schema. All have RLS enabled and no client policies or table grants; the service role alone uses narrowly exposed invoker wrappers. The private erasure worker is the sole definer function, has no arguments and cannot be called by client/Edge roles.

`pockle-api` Edge Function verifies the bearer through Supabase Auth on every request. It rejects anonymous and non-Apple/Google identities. It then passes only the verified user ID and JWT session ID to SQL, which checks a still-existing session and current account state. The gateway legacy-JWT switch is intentionally off because the function does its own authentication; a missing or invalid bearer still returns401. No service key or provider secret enters Unity.

Routes: `profile`, `username`, `legacy_upload`, `legacy_latest`, `delete_request`, `sign_out`, `boxes`, `walking_report`, `box_open`. Payload actor IDs are ignored. No route can purchase, trade, mint a production toy or turn a legacy count into a production serial.

## Identity and save protection

Apple/Google use system-browser OAuth with PKCE S256, fresh256-bit verifier/request values, five-minute callback expiry, exact callback route validation and no fragment/access-token redirects. Supabase owns provider credentials. Android tokens/verifiers live in AES-GCM files under `getNoBackupFilesDir`, with a device-bound AndroidKeyStore key and atomic writes. iOS has a device-only Keychain bridge and generated-project URL-scheme registration; iOS compilation/device behavior is not verified yet. Editor sessions use memory only. Provider cancellation is bounded by the five-minute expiry; immediate browser-cancel reporting needs a native browser-session adapter later.

The Unity adapter implements the complete current `IAccountService` and binds `hud.SetAccountService(...)` before onboarding. It waits for persisted-session restoration before the onboarding call. The only package change is adding the built-in `com.unity.modules.unitywebrequest`1.0.0 module, required for HTTPS. Engine version, rendering pipeline and ProjectSettings are unchanged.

Username claims are server-only, case-insensitively unique,3–16 ASCII letters/numbers/underscore, with reserved/offensive-name screening and a12-attempt/minute account limit. The curated filter is a baseline, not a complete moderation operation; expand/review its vocabulary before public release.

`CollectionSession.ExportLegacySnapshot()` reads the exact persisted JSON without a deserialization/reserialization round-trip. Server storage retains exact text plus SHA256, counts, source ID, receipt and the explicit `legacy_beta_unverified` tag. Unknown IDs, future fields and complete fusion objects remain intact. The first import is immutable; bounded retention keeps that first receipt plus the latest99 snapshots. Repeated uploads of a retained digest return the same receipt. Unity compares digest and inventory/fusion counts before acknowledging or binding the local collection to the account. A mismatch/network failure never overwrites the phone save. Subsequent backups are throttled to once per minute and skip unchanged digests, with no per-touch network calls.

The actual owner phone has89 inventory rows/15 fusions, superseding87/13; the uploader uses actual counts, never a fixed expected count. Real Google sign-in and username claim are now verified. The first tagged snapshot and its SHA256 receipt match the exact full phone JSON (including all inventory quantities and fusion fields). Cold restart restored the secure session without another sign-in and acknowledged a second snapshot whose digest also matches the phone; inventory and full fusion records were unchanged across restart. The first snapshot is retained. No synthetic/admin identity was used. Cross-device download/restore remains unverified.

Sign-out revokes the current session when online and clears the local credential vault, retaining the protected local collection. Different-account sign-in cannot upload another account's linked local collection. The latest-backup endpoint is available, but cross-device restore/conflict/merge must be reviewed before replacing local state automatically.

## Erasure

An accepted deletion request disables account operations immediately and removes cloud snapshots. The active five-minute cron worker deletes Auth identity/sessions, username and walking/box state, and records completion. It retains only a de-identified player tombstone/request timestamps and pseudonymous purchase accounting records. No paid purchase has been made or enabled. Purchase records survive the worker; purchase retention periods and project backup/log-retention policy are operational settings still to review. The normal worker runs far inside the30-day promise; monitor pending deadlines with:

```sql
select count(*) from pockle_private.account_deletions
where completed_at is null and deadline < now();
```

The current local beta collection remains on the device after account sign-out/deletion request; account-linked cloud data is erased. Coordinate the local-cache erasure/product wording with Claude before public release so an intentional delete cannot be mistaken for ordinary sign-out. Never erase the owner's collection while testing this foundation without an explicit deletion action.

## Server box clock

- Server timestamp and UTC day only. No client clock/day parameter can mint a reservation.
- Six-hour slots; the initial slot becomes available with the first1000 newly observed steps. Historical sensor totals are baselines, not credits.
-1000 steps per reservation, at most4 earned per UTC day, at most2 banked. Extra validated beta step credit carries forward, bounded to100000.
- A stable collection cursor skips completed collections, falls back to the full enabled roster when all are complete, and freezes the chosen collection on the earned reservation. Opening/replaying cannot reroll that reservation. An ordinal gives deterministic chronological ordering even when two bank at once.
- All mutations serialize on the stable player lock; IDs and bank state commit atomically. Counter sequence/boot/reset/device binding and a server-elapsed rate limit reject simple replay and impossible batches.
- Evidence is explicitly `client_reported_beta`. This is not hardware attestation or tamper-proof walking. Responses state `production_grants_enabled:false`. Opening a reservation does not mint/award a toy; authoritative pools/serial allocator/attestation are a later slice.

The current local daily reward screen has not been silently switched to this server policy. Claude needs to bind the clock/bank fields and coordinate the production grant cutover after the real owner account backup. No paid-box or existing save rules were changed.

## Notifications

`NotificationGate` checks `NotificationPrefs.IsOn(kind)` at both schedule and presentation time, rejects unknown kinds and cancels queued notifications when a preference switches off. Tests cover all four kinds. No push delivery SDK, device-token registration, notification permission prompt or scheduled OS notification is introduced in this slice. Future adapters must route through this gate; permission remains an explicit walking opt-in action.

## Actual verification

- Pinned Unity6000.6.5f1 focused native suite:13/13 passed (backend exact export, no anonymous adapter session, notification gating, updated onboarding/account screens and existing glow/input guards).
- Android development build succeeded. Actual DEX definitions contain SecureSession.get/put and the callback activity; manifest includes exact callback scheme/path and Internet permission. Existing walking class and all six JNI APIs still packaged. APK `/private/tmp/Pockle-Backend-Foundation.apk`; not installed in this task.
- Portable suite passed, including RFC7636 PKCE/SHA256 known vectors, callback confusion/replay rejection and publishable-key-only config boundary. Source checks250 C#/813 GUIDs, whitespace passed.
- Deno2.5.6:3 Edge tests passed, entrypoint type-check passed. No runtime dependency imports.
- Live database rollback tests: exact89-row/15-fusion fixture with unknown fields, idempotent receipt,100-version retention with original preserved, name rules/taken/reserved, wrong/cross-account session denial, client privileges denied, immediate deletion disable, worker erasure with purchase preserved; clock/day/bank/rate/baseline/replay/saved rotation passed.
- Live unauthenticated/invalid-session HTTP smoke requests rejected; direct server RPC inaccessible to client. Email/anonymous/phone providers disabled. Erasure cron active, no overdue deletions; real owner account and tagged backups subsequently verified as described above.
- Advisors: no error/warning-level security findings. Informational no-policy notices are intentional private deny-by-default RLS; fresh-index-unused and default Auth absolute connection allocation notices remain. [RLS advisory](https://supabase.com/docs/guides/database/database-linter?lint=0008_rls_enabled_no_policy), [production connection tuning](https://supabase.com/docs/guides/deployment/going-into-prod).

Not verified: Apple login, refresh after token expiry, real sign-out/deletion roundtrip, cross-device restore, iOS build, concurrent multi-session stress, notification delivery, release build/store compliance or hosted CI. Do not describe this as completed account cutover or production rewards.

## Claude handoff

Active PrototypeHud files were snapshotted read-only for contracts/tests and not authored/copied back. The completed-onboarding early-return currently bypasses the sign-in screen on cold start without a session; the requested account-required gate change is in Codex's handoff. Official platform buttons, local-cache deletion wording and server-clock HUD integration remain coordinated screen work.

[Provider credentials and dashboard steps](PROVIDER_SETUP.md). Wally15 (9 common/3 gold/2 diamond/1 molten; glow becomes Epic) and vinyl flick-spin/tap-tok remain later work after this foundation.
