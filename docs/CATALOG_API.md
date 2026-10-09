# Catalog API and version-2 save migration — UX-029

Implementation branch: **`codex/catalog-migration`**. This is runtime/catalog groundwork for Claude's UX-031 UI integration. It does not add new models, shaders, handling profiles, or selectable toys. There are 108 catalog definitions (12 characters × 9 finish families); **four existing Pip assets are available**, and 104 concepts remain unavailable.

## Read-only definitions

All definitions are engine-independent in `Pockle.Core`:

```csharp
ToyCatalog.Characters     // IReadOnlyList<CharacterDefinition>
ToyCatalog.Finishes       // IReadOnlyList<FinishDefinition>
ToyCatalog.Collectibles   // IReadOnlyList<CollectibleDefinition>
ToyCatalog.BoxPools       // IReadOnlyList<BoxPoolDefinition>
ToyCatalog.TryGetCharacter(id, out character)
ToyCatalog.TryGetFinish(id, out finish)
ToyCatalog.TryGetCollectible(id, out collectible)
ToyCatalog.TryGetBoxPool(collectionId, out pool)
```

- Character: `Id`, `DisplayName`, `AccentColor` (`Red`, `Green`, `Blue` in 0–1), `LeadFinishId`.
- Finish: `Id`, `DisplayName`, `HandlingProfileId`. Handling IDs describe intended future behavior; the current renderer still uses its existing four presets.
- Collectible: `Id`, `CharacterId`, `FinishId`, `FinishDisplayName`, `DisplayName`, `Available`, `CollectionId`.
- Box pool: `Id`, `DisplayName`, read-only `Entries` (`CollectibleId`, positive integer `Weight`). `TryChoose(float unitRoll, out string id)` preserves existing probability boundaries. Invalid rolls return false.

`Available` means playable art exists in this build. It does not mean owned, purchased, or eligible for a particular reward. Planned concepts have no approved `CollectionId`. Box pools stay explicit: Jelly Garden is Peach/Mint 50% each; Midnight Glow is Moon 100%; Gold Confetti is Gold 100%. Checkout remains unavailable. `BoxOffer.CollectionId` and `.Pool` expose the same definitions without changing existing fields/captions.

Working character names are still subject to ART-001 review. Display labels can change without changing persisted IDs. IDs use case-sensitive lower-case ASCII letters, digits, `.`, `-`, and `_`, with a maximum length of 128; do not derive them from translated display labels or array positions.

## Ownership and play bridge

After `CollectionSession.Initialize()`:

```csharp
session.OwnedCounts                       // IReadOnlyDictionary<string, int>
session.GetOwnedCount(collectibleId)       // 0 for an absent ID
session.IsOwnedAndAvailable(collectibleId) // ownership AND playable art
session.TryGetPlayableVariant(collectibleId, out PipVariant variant)
session.PendingRevealId                   // "" means no unfinished reveal
session.ClaimDailyCollectible(out string id)
session.FinishReveal(string expectedId)    // bool; validates identity and available owned art
session.IsReadOnlySave
```

The legacy `ClaimDaily(out PipVariant)` and parameterless `FinishReveal()` remain. Current controller/HUD events do not change. `TryGetPlayableVariant` is the temporary bridge to the four actual Pip presets; **honor its bool result**. Unknown/planned IDs must not play the default enum value returned on failure.

| Old index / enum | Stable ID |
| --- | --- |
| 0 / PeachJelly | `pip.peach-jelly` |
| 1 / MoonJelly | `pip.moon-pearl` |
| 2 / GoldGlitter | `pip.gold-confetti` |
| 3 / MintSoft | `pip.mint-mochi` |

`PipVariants.CollectibleId(PipVariant)` and `TryFromCollectibleId(string, out PipVariant)` expose that explicit mapping. Core equivalents are `ToyCatalog.LegacyCollectibleId(int)` and `TryGetLegacyIndex(string, out int)`.

**HUD integration belongs to Claude.** Keep the existing enum overload/events, then add an ID overload along these lines:

```csharp
public bool ShowToy(string id)
{
    if (session == null || !session.TryGetPlayableVariant(id, out var choice)) return false;
    ShowToy(choice);
    return true;
}
```

Render names and shelf entries from definitions; get counts by ID. For totals, use all positive values in `OwnedCounts`, including preserved future IDs. Unknown owned IDs can have an unavailable display state, while their stored identity/count remains intact. Do not modify `.Save.Counts` or `.Save.Inventory` to grant toys: these are persisted snapshots, not mutation APIs.

## Profile IDs

`GuestProfile` is now public, with `AvatarId`, `FavoriteId`, and bool ID overloads `SetAvatar(string)` / `SetFavorite(string)`. The old enum properties/setters remain for the current HUD. New preferences are:

- `pockle.profile.local.avatarId`
- `pockle.profile.local.favoriteId`

When absent, these migrate the existing integer choices explicitly. Valid future IDs survive reads and name updates even when this client cannot render them. ID setters only accept currently available definitions; the UI additionally checks ownership before selecting a favorite. Existing avatars can still be chosen independently of owned counts. Legacy enum properties show Peach for a future ID that cannot map; they do not erase the stored ID. The ID-based UI should use an unavailable placeholder for that case.

**Claude's UI test fixture needs two more backups/restores.** `CollectionUiTests` currently preserves only name/avatar/favorite. Please include the two new string keys above in setup/teardown before running the integrated suite, so test-created selections do not leak into the developer's profile. Codex has not edited this Claude-owned file. The new standalone `CatalogMigrationTests` preserve all affected keys themselves.

## Bounded previews

```csharp
var cache = new CollectiblePreviewCache(capacity: 8);
var lease = cache.TryAcquire(collectibleId); // null if missing art or every slot is in use
if (lease != null) image.texture = lease.Texture;
// When an item leaves the visible set:
image.texture = null;
lease?.Dispose();
// When the screen/cache owner is destroyed:
cache.Dispose();
```

Acquire only visible item IDs. Duplicate leases share one portrait. Unused entries are evicted least-recently-used; visible entries stay pinned, and allocation never exceeds the configured capacity. If every slot is pinned, acquisition returns null, so release offscreen items before requesting replacements. Retry after the visible set changes rather than expanding the cache.

Cache disposal stops new acquisitions and releases idle resources; still-leased textures remain alive until their final lease is released. Consumers must clear their image reference and dispose the lease. The current adapter renders the four Pip assets; pre-rendered thumbnail support and new character loading can extend the same boundary later. The legacy four-portrait HUD is untouched in this branch and does not yet use the cache.

## Save behavior and recovery

The existing PlayerPrefs key remains **`pockle.collection.v1`** for compatibility with the current fixtures/install; its payload is now `Version: 2`:

- `Inventory`: serializable rows `{ Id, Count }`; authoritative ownership, capped at 0–9,999 per ID.
- `PendingRevealId`: stable ID of the already-granted toy whose reveal is unfinished.
- Existing day/steps/claimed/hardware baseline fields retain their meaning.
- `Counts[4]` and `PendingReveal` remain legacy projections for the current controller/HUD. They are rebuilt from IDs, and are never the authority for an existing version-2 inventory.

Before the first version-1 migration, its exact JSON is preserved at `pockle.collection.v1.before-v2`. Missing/short legacy arrays preserve recoverable entries and use zero for missing entries, rather than re-gifting the roster. Only a genuinely new save receives the four beta starter gifts. Repeated migration/restart does not add toys.

Unknown valid owned IDs, zero-count rows, and unknown pending-reveal IDs survive version-2 saves. Duplicate rows use their maximum bounded count rather than adding counts. Invalid IDs/negative counts are normalized. An unknown pending reveal blocks another daily claim and remains saved until a compatible build can finish it.

Malformed JSON, a damaged version-2 payload lacking inventory, and unsupported schemas get a raw `.backup` and remain read-only. The primary key is preserved exactly: the session exposes a read-only legacy snapshot, refuses rewards, and does not overwrite that source on pause or destruction. Read-only status is reported through `WalkingStatus` as well as `IsReadOnlySave`.

The pre-migration backup supports recovery, not live bidirectional sync with older APKs. Old version-1 code does not understand a version-2 payload; downgrade/reconciliation is not implemented. No cloud save, server validation, or purchase delivery is introduced here.

## Validation and review

Executed **1,224 new portable catalog/cache assertions**: 108 unique definitions, explicit legacy IDs, unchanged pools and RNG boundaries, exact legacy JSON migration, day/step/baseline preservation, repeated save/restart, wrong/double reveal acknowledgment, partial arrays, unknown IDs and pending reveals, immutable views, profile mapping, and resource budgets/leases/LRU cleanup. All preexisting portable suites still pass.

C# source/asset checks pass for 49 files across default/Editor/Android symbol profiles and 91 GUIDs. Unity migration tests are authored in `CatalogMigrationTests.cs` but **not executed in cloud**. They cover actual JsonUtility/PlayerPrefs/session migration, restart, profile references, unknown IDs, and newer-schema preservation. This evidence does not establish Unity API compilation, rendering, portrait lifetime on the GPU, or Android APK behavior.

Review in Unity with a copy of an existing local save: confirm counts and daily state, pending reveal resumption, profile mapping, one successful daily grant, and unchanged legacy UI/controller behavior. Update Claude's UI test key backup first, then run the PlayMode suite and inspect warnings. Run portrait-cache rendering/lifetime checks when Claude integrates visible-item leases.
