# Pockle UI direction 03 — fixed navigation review

Branch: `codex/fixed-navigation-review`. This revision changes only the HTML review. No Unity HUD, Resources, player inventory or runtime behavior changed.

## Owner direction

The owner approved **Home / Shelf / Walk / Community / Shop**, and requested new icons with a full-width fixed bottom bar instead of the floating rounded pill. The four-tab alternative is removed from this revision.

## New navigation component

- Five equal-width destinations, each with an icon and a visible label. No raised center action or global Fuse button.
- The opaque plum bar spans the complete app content width, with a flat top edge and no outer radius, floating gap or drop shadow. Its color continues through the home-indicator area.
- The bar is a fixed-size sibling below the scrolling content in each phone's flex layout. Scrolling ends above it rather than underneath it, and bottom padding keeps the last row reachable.
- A small peach highlight sits behind only the selected icon. Selected icons use the matching filled form; other icons use regular forms. Labels and tap areas have equal weight.
- Phosphor house, squares-four, sneaker-move, users-three and storefront form one consistent family. The large comparison shows regular and filled states for each. The close-up uses the same component as the interactive phones.
- Profile is reached through the avatar, so no bottom destination is selected while Profile is open. Fusion remains inside Shelf.
- The status bar and home indicator are HTML illustrations, not evidence of native Unity safe-area behavior.

## Preserved preview behavior

The flat owned Shelf includes fusions and counts extra copies on each finish. All owned / Fusions / Extras filters and search narrow the same grid. Walk and Shop remain discovery actions. Toy buttons explain the direct Play route; they do not run Unity.

Community retains Feed / Friends / Exchange with sample data only. Public offers have fixed terms already confirmed by the maker; the taker inspects and accepts. The Fox example is unavailable because the sample shelf has zero spares of its wanted finish, and the first copy is protected. No real offer, reservation, transfer, friend request or social post exists.

Profile retains twelve character avatars, local image upload and circular crop controls. First registration retains its optional avatar step after sign-in and collector-name choice: twelve presets, shared local crop editor, Continue, and Skip to Peach Pip. Draft choices do not change the displayed profile until Continue; closing cancels. The prototype does not create an account or block registration.

Images chosen through upload remain in page memory via `FileReader`; nothing is transmitted or persisted. A real public avatar service still needs validation, moderation, privacy and reporting controls.

## Artwork and implementation scope

The prior approved artwork and model portraits are copied unchanged. The twelve avatar originals remain review assets and are displayed with CSS circle crops. `assets.json` records art provenance; the icon provenance and license accompany the pinned Phosphor files. No raster artwork was generated or edited for this navigation revision. Store prices in sample screens are illustrative approved US targets, with no invented odds.

## Verification

`verification.json` records this revision's passing static/VM checks and native Chrome inspection. All three portrait previews, ten regular/filled icon states, Home/Shop highlights, the Shelf Fusions filter and the final shelf row above the fixed bar were visually checked. Current revision03 screenshots are in `proof/`; browser chrome is cropped out, and earlier review02 screenshots are not reused. There is no physical-phone or Unity acceptance for these proposed screens. Registration logic remains covered by static/VM checks, not a new visual run.
