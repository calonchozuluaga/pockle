# Collection navigation and viewer backdrop

Integration follow-up: `codex/v2-collection-integration` now supplies the HUD page roots and collection-origin `ShowToy` routing below. The HUD emits `BackdropChanged(Color)`; `TactilePrototype` binds it to the explicit viewer-camera overload, so colours update even when menu browsing disables that camera. Home/Shelf collection entries and shared static lineups are wired. The remaining "Claude integration needed" paragraphs describe the original PR #9 boundary and are satisfied by this later integration; live lineup animation remains pending.

Branch: `codex/collection-backend-contracts`, based on GitHub main `b8a119c`. Complements the catalog/motion branch in PR #7 and Claude's v2 HUD in PR #8. No HUD partials changed.

## Navigation (UX-033 foundation)

`AppPage.Collections` is the all-series browser; `AppPage.Collection` is a single-series lineup. They are appended after `Play`, preserving all eight existing enum values. Scroll storage covers all declared pages. `Push` rejects unknown enum values before modifying history.

Use `Push(Collections)`, then `Push(Collection)`, then `Push(Play)`. Back returns Play → Collection → Collections → originating page, retaining each page's scroll. They are not main tabs; selecting Home/Boxes/You resets history as before. The HUD must keep the selected collection ID itself; the navigation model tracks page destinations, not catalog selections or per-series scroll.

**Claude integration needed:** add page roots/layout and route direct lineup-to-play without forcing Shelf first. Current `ShowToy` still inserts Shelf for origins other than Shelf/Play; its HUD condition needs to accept Collection as another play origin. Do not claim the new pages are visible before that UI change. Preserve ownership/availability checks for `ShowToy(string)`.

## Stage colour field

```csharp
bool changed = PrototypeStage.SetBackdrop(fieldColour);
// Explicit camera overload is preferable when the caller already owns the viewer:
changed = PrototypeStage.SetBackdrop(viewCamera, fieldColour);
PrototypeStage.SetBackdrop(viewCamera, PrototypeStage.Background); // reset
```

The first overload resolves the active `Camera.main` and succeeds only when it has the Pockle stage framing component. The explicit overload isolates multiple stages and tests. Both update the viewer and full-screen clear camera; updating just the viewer fails once framing switches it to depth-only clearing. Portrait/unrelated cameras are untouched. There are no static camera references to survive destroyed stages or accidentally target portraits.

RGB channels clamp to 0–1; the field is opaque. NaN/infinite RGB rejects the update without changing either camera. A missing/destroyed/uninitialized stage returns false. The method allocates no meshes/materials/render targets and does not rebuild the toy. It uses the supplied Unity colour directly; colour-space appearance still needs Unity/device review.

**Claude integration needed:** pass the v2 toy field colour when entering/changing play, and reset to the default when appropriate. The current HUD does not call this API yet. Prefer the explicit overload through the runtime owner when camera ownership is available; a direct one-argument HUD call also works with the single active main stage.

## Existing catalog and next backend contracts

Reuse `ToyCatalog.Collections`, `CollectionDefinition`, explicit offer pools and lineup motion from PR #7. Product/date/weights remain unapproved where null/empty; never infer today's release or a full-collection probability from the beta sub-pools. This branch changes no catalog, save format or reward eligibility.

The [backend plan](BACKEND_PLAN.md) defines serials, immutable discovery/source, server quotes and atomic grants; legacy dates remain unknown. It also plans check-in claims without activating unsettled tuning, and adopts the newer five-spare/no-secret crafting decision. Unity still runs local beta inventory until the server cutover.

## Verification status

Portable cases added: Collection/Collections Back and scroll, tab reset and invalid destinations. Unity PlayMode case added: both camera colours survive framing, reset, malformed input, unrelated camera isolation, and destroyed stage rejection.

Executed: all ten portable suites (417,710 assertions, including 42 menu/profile assertions), and source checks (57 C# files across three symbol profiles, five assembly definitions, 133 GUIDs, zero failures). Used the prepared .NET 8.0.425 SDK with writable CLI/NuGet directories. Metadata synchronization and whitespace checks also passed.

Unity PlayMode is authored but unrun locally; no Unity editor is installed. Source checks do not establish Unity API compilation, rendering or lifecycle correctness. The outbound proxy is unreachable; GitHub reads/publication use the connector.
