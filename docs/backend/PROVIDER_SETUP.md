# Pockle Apple and Google setup

Project: **Pockle**, ref `azdzafotawbrplukeqcl`, Zulapp, us-east-1.

Both providers return to exactly:

`https://azdzafotawbrplukeqcl.supabase.co/auth/v1/callback`

The Unity adapter uses the system-browser OAuth/PKCE flow on Android and iOS, with a fresh256-bit verifier and request binding. Provider secrets are used by Supabase Auth only. Do not put them in Unity, Git, chat or a screenshot. The checked-in client configuration contains only the public project URL and modern publishable key.

## Google

1. In Google Cloud Console → Google Auth Platform, create/select the Pockle project. Configure Branding, Audience and Data Access for basic OpenID/email/profile sign-in. Use the real support email, `playpockle.com` domain and published privacy/terms URLs. While in Testing, add your Google account as a test user. Complete Google's publishing/verification requirements before release.
2. Clients → Create client → **Web application** (even for this phone browser flow). Name it Pockle Supabase. Add the exact HTTPS callback above as an Authorized redirect URI. A native Android OAuth client/SHA certificate fingerprint is not needed for this implementation; those become relevant if we later adopt a native Google SDK.
3. Copy the **Web Client ID** (`…apps.googleusercontent.com`) and **Client secret** into [Supabase → Authentication → Sign In / Providers → Google](https://supabase.com/dashboard/project/azdzafotawbrplukeqcl/auth/providers). Enable Google. Keep nonce verification enabled; do not skip it.

Required Google credentials: **one OAuth Web Client ID + its Client secret**.

## Apple

Requires membership in the Apple Developer Program.

1. Certificates, Identifiers & Profiles → Identifiers: register/select the app's **primary App ID**, enable **Sign in with Apple**. Use the final owned iOS bundle identifier; do not change the existing Android beta application's ID as part of this setup.
2. Create a **Services ID**, e.g. `com.playpockle.pockle.signin` if available in your team. Enable Sign in with Apple; associate it with the primary App ID. Under website configuration, set domain `azdzafotawbrplukeqcl.supabase.co` (no scheme), and Return URL to the exact HTTPS callback above. Save and complete Apple's registration steps.
3. Keys → create a **Sign in with Apple key**, associate the primary App ID, record the **Key ID**, and download its private **`.p8` signing key** once. Keep that private key outside the repository. Record your **Apple Developer Team ID** from Membership.
4. Generate an **ES256-signed Apple client-secret JWT** using the Team ID (`iss`), Key ID (`kid` header), Services ID (`sub`), Apple audience `https://appleid.apple.com`, issued-at and expiry. Expiry must be within Apple's six-month limit. Use the generator in the official Supabase Apple guide or a trusted local tool; never commit the `.p8` or generated JWT.
5. In [Supabase → Authentication → Sign In / Providers → Apple](https://supabase.com/dashboard/project/azdzafotawbrplukeqcl/auth/providers), enable Apple; enter the **Services ID as Client ID** and the **generated JWT as Secret Key**. The `.p8` signing file is input for generating that secret, not a Unity asset. Regenerate and replace the secret before expiry (at least every six months). Native Apple sign-in, if added later, also needs its bundle ID in the provider audience configuration.

Required Apple values to create: **primary App ID + Services ID + Team ID + Key ID + `.p8` signing key**. The two values entered in Supabase for the current browser flow are **Services ID and generated client-secret JWT**.

## Supabase Auth settings (required)

- Enable **Google and Apple only**. Disable anonymous sign-ins, email/password/magic-link and phone signup/providers. The game Edge boundary independently rejects anonymous accounts and sessions whose verified identity provider is neither Apple nor Google.
- [URL Configuration](https://supabase.com/dashboard/project/azdzafotawbrplukeqcl/auth/url-configuration): Site URL `https://playpockle.com`. Add redirect `com.playpockle.app://auth/callback?request=*` (the wildcard covers the random request binding). Do not add broad HTTPS wildcard domains. The app accepts only this exact scheme/host/path and matching one-use request value. Test this query pattern in the dashboard: Supabase glob rules can vary; if it rejects query matching use `com.playpockle.app://auth/callback**`, keeping the app's strict route/query validation.
- Use a short access-token expiry (the normal one hour is acceptable for beta). Sensitive operations additionally require an existing live Auth session and an active player, so deleted/signed-out tokens cannot continue operating merely until JWT expiry.
- No full birthday is transmitted to this backend; only the on-device gate controls opening the OAuth flow.
- The account-erasure cron job runs every five minutes, so normal deletion completes well inside30 days. Check its execution history and overdue-request query during operations. Retained purchases are not erased by this worker. Public promises also require backup/log retention policies outside this application schema to be kept within the documented retention window.

## First real phone check

After configuring the providers and redirect, tell Codex **“Google/Apple configured”** without sending secrets. A new signed-in account must claim a username, then upload and acknowledge the exact local save. The latest verified actual phone baseline is **89 inventory rows and15 complete fusions**, not the earlier87/13. We must compare the receipt counts and checksum and verify a server download before calling the real migration successful. Do not create a test identity and attach the owner's save through admin SQL as a substitute for signing in.

The current implementation preserves local saves and binds a phone's legacy collection to its first acknowledged account. It does not silently overwrite local progress when a different account signs in. Automatic cross-device restore/merge and an account-switch conflict screen require a later reviewed policy; the latest-backup API provides a read candidate, not an unconditional replacement of PlayerPrefs.

Official references checked October21: [Google](https://supabase.com/docs/guides/auth/social-login/auth-google), [Apple](https://supabase.com/docs/guides/auth/social-login/auth-apple), [PKCE](https://supabase.com/docs/guides/auth/sessions/pkce-flow), [Edge authentication](https://supabase.com/docs/guides/functions/auth). Supabase's October12 TLS CBC deprecation and September25 Postgres17.11 security changes were reviewed; no custom cipher/security overrides were introduced.


## Configured beta status — October 21

Google Cloud project `pockle-512403` (Pockle) has Web client **Pockle Supabase Auth**, with the single callback documented above. Owner accepted Google's policy. Credentials were transferred directly into the Pockle Supabase dashboard; Google now shows **Enabled**, with nonce checks retained. Email, phone and anonymous sign-in remain disabled. Google audience is **External / Testing**, with `carlos@zulapp.com` as the sole saved test user. Support email is `support@playpockle.com`. No secrets are recorded here.

Apple remains unconfigured. Real phone sign-in and the first acknowledged legacy snapshot have not yet been verified; dashboard enablement alone does not establish a successful backup. Production branding/publishing must still be completed before public release.
