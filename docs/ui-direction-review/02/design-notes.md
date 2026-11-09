# Pockle UI direction 02 — review only

Branch: `codex/mystery-social-review`. Work is confined to this review directory. No HUD, Resources, player inventory or runtime changes.

## Recommended structure

- **Home / Shelf / Walk / Community / Shop**: five equal slots, identical icon size and tap area, no emphasized or raised center action. The inset pill leaves equal space at both edges. Profile and Settings open from the avatar, rather than occupying a global tab.
- Alternative: **Shelf / Walk / Community / Shop**. Shelf becomes the initial destination and absorbs Home highlights. This reduces navigation to four larger targets.
- **Shelf** is a flat grid of all owned finishes and saved fusions. One tile represents one finish; extra copies are a count on that tile. All owned / Fusions / Extras filters and search narrow the same grid. No collection headings or folder navigation. Walk and Shop are visible discovery actions. Fusion is a secondary Shelf action.
- **Community** has Feed / Friends / Exchange. All three use explicitly labeled sample data in this review. The public exchange follows asynchronous fixed offers: the maker agrees at publication, the taker inspects and accepts, and the future server must validate and transfer both exact available copies atomically. The prototype simulates only a local confirmation; no actual listing, reservation or trade exists.
- **Profile** offers twelve character avatars and an image-upload/circular-crop preview. Every destination's avatar opens Profile; account, sound, notification and privacy settings stay there. Uploaded images are read locally with `FileReader`, held in page memory and never transmitted or persisted. A future real upload service needs format/size validation, moderation, privacy, report and block flows.
- **First registration** has an optional avatar step after sign-in and collector-name selection: twelve presets, photo upload with the same local crop editor, Continue and Skip. Skip uses Peach Pip; closing the preview discards the draft. This concept does not create an account or make avatar selection mandatory. A “First registration preview” entry sits below the Profile study.
- **Native status bar** remains visible above app content, with content and bottom controls respecting safe-area space. The HTML bar is an illustration of the intended native placement; it is not proof of runtime status-bar behavior.

## Artwork

Approved collection covers, worlds, cartons and actual model portraits are reused unchanged. New twelve-avatar art is a separate pending-review set from `ArtSource/Avatars/01`, displayed unchanged with CSS circle crops and explicit 48px size checks. `assets.json` lists each copied source.

## Prototype behavior

The three phone frames navigate independently. Shelf search and filters, Community tabs and offer filters, both navigation options, avatar selection, local upload/crop, offer inspection and local acceptance simulation work in this page. The Fox example correctly disables acceptance because the sample shelf has no spare of its wanted finish; the first copy cannot be offered. Toy Play, fusion, shop purchase, settings and account actions explain the proposed route; they do not operate the app. Mock prices are the approved illustrative US targets ($0.99, $2.49 and $3.99). No odds are invented.

## Verification

JavaScript syntax, asset existence and logic fixtures are checked locally. The root agent rendered the self-contained review using native Chrome with a narrowly scoped localhost server: Shelf, Exchange, Profile and all twelve circular avatars were inspected. There was no overflow or overlap except the intentional content scrolling under fixed navigation; bottom content remained reachable. Fusions showed Sorbet only, Fox avatar selection updated all three phone frames, and the crop editor applied a 202% zoom. The zero-spare Fox offer visibly disabled both agreement and acceptance. Option B switched all frames to four tabs, then option A was restored to five.

The three `*-browser.png` captures record the page, zero-spare guard and navigation comparison. Browser chrome was removed by a deterministic crop, with no screen-content retouching; `preview-hero.jpg` is a smaller crop of the three phone frames for review. `verification.json` records the checks and crop rectangles. Original art files remain unchanged. No Unity implementation or physical-phone acceptance is established by this HTML review. A real uploaded photo has not been published or stored anywhere; public avatar upload/moderation and all live social services remain unimplemented.

The added first-registration flow passes static/VM checks for all twelve presets, independent draft selection, Continue, Skip to Peach Pip, cancel without changing the profile, and the shared crop editor returning to the registration draft. The root agent observed the new first-registration entry button in native Chrome. User activity interrupted attempts to open it, so the chooser dialog itself is not visually verified; its checks are static/VM only. No further browser attempts were made. The existing screenshots precede this small entry-point addition.

## Next decision

Choose option A or B and confirm the flat shelf density, Community organization and avatar art before porting these visual changes into Unity. Real social features remain a service milestone, independent of approving these screens.
