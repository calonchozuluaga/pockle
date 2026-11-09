# playpockle.com

Static site for Pockle: home, Privacy Policy, Terms, Support and Delete account pages (website v1).
Later: the public trading ledger (website v2), reading Supabase's public ledger view.

- The home page is hand-written in `home/index.html`, with `assets/home/home.css` and images in `assets/home/` (exported from the art masters in `ArtSource/Website` by `home/build_assets.py`). Add `?review` to the home page URL to outline every line that still needs owner confirmation before launch.
- Legal and support pages: edit content in `build.py`, styles in `assets/site.css`.
- Run `python3 build.py` to regenerate `public/` after any change.
- Vercel project `playpockle` serves `website/public` (no build step).
- Privacy, Delete account and Terms wording approved by the owner November 8, 2026. Terms use Colorado law. Changes to legal declarations still need owner review. Contacts: support@, privacy@, legal@playpockle.com.
