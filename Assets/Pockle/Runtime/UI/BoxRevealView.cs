using System;
using System.Collections.Generic;
using System.Globalization;

namespace Pockle.Runtime
{
    // Box reveal, Claude's side (owner, November 3: paid boxes and packs are mandatory for the first release). Codex's
    // runtime owns the draw, the receipt, the saved copies and their serials; the HUD only draws what it is handed.
    //
    // Contract:
    //   1. The server confirms delivery and the runtime SAVES the result first (AGENTS.md: persist before any reveal).
    //   2. Codex calls hud.ShowBoxReveal(view). Nothing in the reveal draws, grants or rerolls.
    //   3. hud.BoxRevealProgress(receiptId, revealedCount) fires each time another toy has been shown, so the runtime can
    //      save how far the player got. After a restart Codex passes the same receipt with StartAt = that count; toys
    //      already seen go straight to the summary instead of replaying, and no outcome changes.
    //   4. hud.BoxRevealFinished(receiptId) fires once when the player leaves the reveal (Keep, Done, Play or Back).
    //   5. hud.BoxRevealOpened(tier) fires at the moment each box pops open, for tier sound and haptics (Codex plays them,
    //      respecting the Sound and Haptics settings). Calm motion still fires it, once, without the build-up.
    //   6. hud.BoxRevealPlayRequested(collectibleId) fires when the player taps Play; the HUD then shows that toy.

    public enum BoxRevealSource { WalkingBox, CollectionBox, MysteryMix }

    /// <summary>One server-confirmed, already-saved box result (a single box, or one child of a pack).</summary>
    public sealed class BoxRevealItem
    {
        /// <summary>The saved toy's stable catalog ID.</summary>
        public string CollectibleId;
        /// <summary>Whose box it came out of (box art). Null: the toy's own collection.</summary>
        public string CollectionId;
        /// <summary>The copy's real serial, e.g. 412. Null until the server has one: the reveal then shows no number at all.</summary>
        public long? Serial;
        /// <summary>The finish's published edition size, e.g. 10,000. Null: no number shown.</summary>
        public int? EditionSize;
        /// <summary>True when this is the player's first copy of this toy (server-decided at delivery); false = a spare.</summary>
        public bool IsNew;
        /// <summary>The tier the server delivered. Null: the catalog's own tier (<see cref="RarityLook.Of"/>).</summary>
        public RarityTier? Tier;

        /// <summary>"#412 of 10,000", or null when the server sent no real serial and edition size.</summary>
        public string SerialText => Serial.HasValue && EditionSize.HasValue && Serial.Value > 0 && EditionSize.Value > 0 && Serial.Value <= EditionSize.Value
            ? "#" + Serial.Value.ToString("N0", CultureInfo.InvariantCulture) + " of " + EditionSize.Value.ToString("N0", CultureInfo.InvariantCulture) : null;
    }

    /// <summary>A whole delivery: one box, or a 3- or 5-box Mystery Mix opened one after another.</summary>
    public sealed class BoxRevealView
    {
        /// <summary>The server receipt or operation ID the progress events report back. Required.</summary>
        public string ReceiptId;
        public BoxRevealSource Source = BoxRevealSource.CollectionBox;
        /// <summary>Saved results in the order they open.</summary>
        public IReadOnlyList<BoxRevealItem> Items = Array.Empty<BoxRevealItem>();
        /// <summary>How many were already revealed before an interruption (0 for a fresh delivery).</summary>
        public int StartAt;

        public int Count => Items?.Count ?? 0;
        public bool IsPack => Count > 1;
    }
}
