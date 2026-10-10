# V2 collection integration — phone review

Test branch: `codex/v2-collection-integration`. This combines Claude's v2 screens/catalog and Codex's collection/backdrop APIs. The integration PR supplies one checkout; it does not depend on merging the old PR stack first. Keep your local Android settings and use Unity **6000.6.5f1**.

## Quick route

1. Open Home. Confirm the v2 fonts, plum navigation, daily card and toy strip fit the screen. Scroll to **Browse collections**.
2. Open **Pip**. Confirm Series 1, ten slots, four playable beta varieties and six coming-soon mystery slots. Dates and secret odds are not invented. Other character collections show Coming soon and cannot grant or play unavailable toys.
3. Open an owned Pip variety. Peach, Moon, Gold and Mint should each use their own background field, covering the full screen outside the toy viewport as well. The toy should still squish/lift/stretch normally.
4. Press Back. It should return to Pip's lineup at the same scroll position. Back again returns to the series list, then to Home or Shelf, depending on where you entered.
5. Open another planned character. The lineup begins at its top, with all toys unavailable. **View boxes** and **Walk for one** are disabled for it. No toy or reward should be granted by browsing.
6. For Pip, try **View boxes** and **Walk for one**, then Back. The page should return to Pip. Purchases remain unavailable; existing prototype odds and local walking rewards remain unchanged.
7. Check Shelf → Browse collections, favorite hearts, Settings and Home/Boxes/You tab changes. Returning to a main tab should clear the old collection Back chain.
8. Repeat on a small portrait screen and in landscape. Look for clipped labels, overlapping tiles/buttons, readable mystery states, bottom rows hidden behind the tab bar and mismatches between portrait tile and stage colour.

Please report the branch/build, screen size/orientation, and the route that failed. A screenshot is useful for layout or colour issues.

## Scope

The lineup uses shared static portraits for this test pass; live eager/idle lineup motion remains a follow-up. No new shader look, editions/serials, discovery-date migration, Supabase service, paid checkout, check-in rewards, crafting or trades are active. Planned series have no invented release order or pricing. The current four playable Pip presets and saved ownership remain the test content.

Local validation: 549,003 portable assertions across eleven suites; source checks across 63 C# files / three profiles / five assembly definitions / 139 GUIDs. Unity/API/lifecycle and Android packaging are verified through the integration PR's CI, with the exact result reported on the PR. These checks do not establish touch feel, screen appearance or device frame time.
