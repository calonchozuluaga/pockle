# Series metadata and lineup motion — UX-029/030 follow-up

Implementation branch: `codex/series-metadata-idles`, based on main `b8a119c`. Responds to Claude's `collections-design` handoff at `8b49ec8` and the owner's relayed request. No HUD/UI files changed. The owner still needs to review the box artwork on `codex/box-collection-concepts` before the mesh pass.

## Collection definitions

**Owner clarification:** one collection is one character with **at least ten color/texture/material varieties**. Pip is one collection in **Series 1**; Peach/Jelly Garden, Moon/Midnight Glow, and Gold Confetti are Pip varieties/themes, not new character collections. Other collections must introduce genuinely different characters. The branch now defines twelve character collections with ten planned members each (120 definitions); only the same four Pip assets are playable. Glow Jelly is the tenth planned finish recipe, with no implemented glow shader/environment claimed.

```csharp
ToyCatalog.Collections // IReadOnlyList<CollectionDefinition>
ToyCatalog.TryGetCollection(collectionId, out CollectionDefinition series)
BoxOffer.Collection    // same definition, or null for an unmapped offer
```

| Field | Contract |
| --- | --- |
| `Id`, `DisplayName`, `CharacterId` | Character collection identity/name; current IDs are `pip`, `nook`, `moss`, etc. |
| `SeriesNumber` | Positive public number when assigned; **0 means unassigned prototype** |
| `ReleaseDateUtc`, `RetireDateUtc` | Nullable `DateTime`, midnight with `Kind == Utc`; never substitute today for null |
| `HasPublishedSchedule` | A numbered series with a release date |
| `Members` | Immutable ordered variety IDs; every member belongs to the same character |
| `SecretId`, `HasSecret` | Empty/false until an explicit secret belongs to the collection |
| `Odds` | Immutable `CollectionOdds`: ID, integer `Weight`, long `TotalWeight`, double `Probability`, collection-specific `IsSecret` |
| `ProductId`, `BoxPriceTier` | Support future full-collection offers; empty until prices/products are decided. Legacy beta offers retain their existing products/prices |
| `FieldColor`, `InkColor` | Engine-independent `CatalogColor` RGB tokens |
| `Pool`, `HasApprovedOdds` | Nullable approved collection-wide pool; when set it must disclose every member. Null/false means odds have not been approved |

`TryGetOdds(string id, out CollectionOdds odds)` returns false for missing IDs or unapproved odds. Use `Probability * 100` for a percent and the weight/total ratio for exact disclosure. Do not infer odds from finish names or split an unseen remainder across toys. `CollectibleDefinition.IsSecret` is derived from its character collection's `SecretId`, so the identity flag and collection metadata cannot disagree; every current beta collectible remains regular. For a collection slot, use that collection's `Odds.IsSecret`/`SecretId`.

The public `BoxPoolEntry` / `BoxPoolDefinition` constructors validate positive weights, IDs, nonempty pools, and duplicate outcomes. Pools copy the supplied array, and total weight uses a `long` to avoid integer overflow. `TryChoose` uses those exact weights, rejecting malformed rolls and retaining the inclusive RNG endpoint contract. These constructors create definitions; they do not register a new reward, grant inventory, or change catalog availability.

`CollectionDefinition.IsReleasedAt(DateTime utcNow)` is a presentation scheduling helper: release is inclusive, optional retirement exclusive. It requires a UTC clock and returns false for an unscheduled prototype. It does **not** hide an owned toy, change reward eligibility, or enable checkout. Product rules say series never retire; all published catalog entries leave retirement null. Optional retirement exists only as schema support requested by the owner, not as a new retirement policy.

Pip's collection has `SeriesNumber = 1`, ten ordered members, no release/retire date, and no approved full-collection odds or secret. Other character collections have series 0 until numbered. Do not treat a null schedule as today's release or invent uniform chances for the remaining planned members.

Legacy beta **sub-pools/offers** stay Jelly Garden (Peach/Mint 50% each), Midnight Glow (Moon 100%), and Gold Confetti (Gold 100%), all mapped to the **same Pip collection**. `ToyCatalog.TryGetCollectionForPool(poolId, out collection)` exposes that mapping. `BoxOffer.CollectionId` is now `pip` for all three; its new `PoolId` keeps the old pool ID, and `.Pool` still resolves the actual beta draws. The old five-argument `BoxOffer` constructor resolves these legacy mappings; a new six-argument constructor accepts explicit collection and pool IDs. A storefront rewrite can later replace the three prototype offers with a unified Pip collection box after its complete acquisition policy is approved.

The **future secret target is 1 in 72**; the check suite verifies that a fixture pool with weights 71 regular / 1 secret draws and discloses this rate correctly. Actual secret artwork, IDs, full-collection weights, and dates are still needed before publishing new pools. The existing beta offers do not claim this secret rate.

## Per-character motion

```csharp
character.IdleProfileId // one profile for each of the twelve character IDs
CharacterMotion.Profiles
CharacterMotion.TryGetProfile(character.IdleProfileId, out var profile)
CharacterMotion.Sample(profile.Id, elapsedSeconds, eager: false, calm: false, phaseOffset: 0f)
```

Profiles expose `Id`, `Description`, `IdleDurationSeconds`, and `EagerDurationSeconds`. Sampling returns a value-type `CharacterIdlePose` with height, pitch/yaw/roll degrees, compression, and stretch. It is deterministic, independent of the frame rate, periodic with neutral boundaries, and allocation-free after initialization in the portable check. Unknown profiles or nonfinite/negative time return a neutral pose; calm always returns neutral.

These are small **whole-body motion recipes**, not Blender skeletal clips or separate arm/face animation. Pip breathes/bounces; Dew nods sleepily; Tula lifts gently; Ripple banks; Moss peeks; Nook breathes softly; Wisp bobs/turns; Loop sways/bounces; Bop rocks/turns; Rolo nods/scans; Mallow sways; Sprig bobs. Firm lead characters use rotation/lift rather than squash. Eager motion plays a short greeting with a rest beat; maximum hop is 0.11 model units, deformation is small, and reduced motion stops it completely.

Having a recipe does not mean all twelve models exist. Planned/missing models still need an unavailable/silhouette presentation; do not substitute Pip or grant ownership. The future UI can sample the same poses on a silhouette placeholder, without loading secret character art or revealing its face.

## Opt-in Unity animator for the live lineup

Create **one shared budget per lineup**, usually four slots and never more than six:

```csharp
var motionBudget = new LineupMotionBudget(capacity: 4);
var animator = toy.gameObject.AddComponent<ToyLineupAnimator>();
if (animator.Configure(toy, motionBudget, eager: !isFound, phaseOffset: .2f))
{
    animator.SetCalm(reducedMotion);
    bool runningOrResting = animator.SetVisible(isInsideViewport);
}
// Leaving the viewport or closing the page:
animator.SetVisible(false);
```

Attach the animator to the **same GameObject as its lineup-only `JellyToy`**. Never attach it to the tactile viewer toy: both would write the same deformation and transform. Configuration chooses no collectible and writes no saves; art loading, availability, ownership, and secret silhouettes remain the caller's responsibility.

Only visible, active, non-calm actors can acquire a slot. `SetVisible(true)` returns false if the shared budget is full; keep that toy still and retry after offscreen actors release their slots. Release offscreen actors before activating replacements. Hiding, calm, disabling/destroying the animator or GameObject, disabling the toy, or pausing releases the slot and restores its rest position/rotation/deformation. Reconfiguration restores the previous toy before capturing the new baseline. The animator remembers visibility, but the UI should reapply it after reactivating a recycled GameObject and retry blocked actors after a visible-set change.

The animation budget caps active motion, **not cameras, meshes, or texture allocations**. Claude should still virtualize the lineup and create only the handful of actual visible actors, and bound any live-render resources independently. This pass does not build a live RenderTexture renderer or change the HUD.

The shelf remains static: continue using `CollectiblePreviewCache` and dispose portrait leases when cells leave the visible set. `ToyPortrait` still renders once with a disabled camera and destroys its scene; no idle component is attached. Do not update all 120 toy meshes/render targets each frame.

## Follow-ups from Claude's expanded brief

- First-found UTC date and acquisition source require their own save migration/grant integration. Old records must stay explicitly unknown; never stamp the migration date. No save fields were changed here.
- Walk-tier targets/odds, crafting costs, and whether secrets can be crafted remain proposals awaiting owner decisions. None are encoded as active behavior in this pass.
- New-series walking selection depends on approved series dates/members and available reward assets. Current daily rewards remain unchanged.
- Claude owns the v2 screens and UX-031 lineup/shelf integration. Codex owns runtime motion, pools, and later save/grant work.

## Verification

Executed: all existing portable suites plus series/motion assertions covering single-character membership, ten-variety minimum, Pip Series 1, UTC dates, permanence/optional schedule boundaries, immutable presentation, unchanged beta pools, weight validation/overflow, a deterministic 72,000-draw 1-in-72 fixture, twelve loop recipes, calm/invalid input, bounded long-running motion, zero sampling allocations, and animation slot reuse/idempotent cleanup. Exact final counts are recorded in `VALIDATION.md`. Source checks cover 61 files, three symbol profiles, five assembly definitions, and 137 asset GUIDs.

Authored but unrun: four Unity PlayMode cases covering legacy offer mapping to Pip, visible budget exhaustion, calm/rest restoration, reconfiguration/destruction cleanup, and disabled-toy slot release. Unity compilation, real lifecycle execution, animation appearance, shader output, live lineup rendering, UI integration, Android packaging, GPU/memory costs, and phone frame time remain unverified in this cloud workspace.
