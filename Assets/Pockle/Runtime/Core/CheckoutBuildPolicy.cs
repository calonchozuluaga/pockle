#nullable enable
using System;

namespace Pockle.Core
{
    /// <summary>Community Play testers are not automatically license testers. This only gates new
    /// checkout entry; verified existing-purchase recovery must remain available in every channel.</summary>
    public static class CheckoutBuildPolicy
    {
        public const string OwnerSandbox = "owner-sandbox";
        public const string CommunityBeta = "community-beta";
        public static bool AllowsNewPurchases(bool android, string? channel)
            => !android || string.Equals(channel?.Trim(), OwnerSandbox, StringComparison.Ordinal);
    }
}
