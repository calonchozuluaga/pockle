using System;
using System.Collections.Generic;

namespace Pockle.Runtime
{
    // UX-052 server walking boxes, Claude's side (owner, October 21). Codex's ServerWalkingBoxService owns the rules and the
    // clock; the HUD only draws what it is handed. Codex pushes a snapshot with hud.SetWalkingBoxes(view) whenever its state
    // changes, and calls hud.ClearWalkingBoxes() to go back to the local beta card (signed out, test builds). Nothing here
    // reads the phone's clock to decide anything: the countdown only ticks a server-sent number down between snapshots.

    public enum WalkingBoxConnection { Online, Reconnecting, Offline }

    /// <summary>One server snapshot of the player's walking boxes.</summary>
    public sealed class WalkingBoxView
    {
        public const int StepsPerBoxDefault = 1000, MaxBankedDefault = 2, MaxPerDayDefault = 4, CooldownHoursDefault = 1;

        /// <summary>Earned, unopened boxes in the order they will open, each as its server-saved collection ID (0 to 2).</summary>
        public IReadOnlyList<string> BankedCollectionIds = Array.Empty<string>();
        public int MaxBanked = MaxBankedDefault;
        /// <summary>Boxes earned in the current server day (0 to 4).</summary>
        public int EarnedToday;
        public int MaxPerDay = MaxPerDayDefault;
        /// <summary>Steps counted toward the next box (0 to StepsPerBox).</summary>
        public int StepCredit;
        public int StepsPerBox = StepsPerBoxDefault;
        /// <summary>Server-configured cooldown, for rule copy only. Eligibility always comes from the server.</summary>
        public int CooldownHours = CooldownHoursDefault;
        /// <summary>Server seconds until the next box can be earned, or null when nothing is counting
        /// down: earning is open, the bank is full, or the day's four are used.</summary>
        public double? SecondsUntilNextBox;
        /// <summary>Collection the next earned box will come from, if the server has already saved it (else null: a mystery).</summary>
        public string NextCollectionId;
        public WalkingBoxConnection Connection = WalkingBoxConnection.Online;
        /// <summary>True while a claim or open is in flight; actions are disabled.</summary>
        public bool Busy;
        /// <summary>The server has confirmed the first banked box can be opened now.</summary>
        public bool CanOpen;
        /// <summary>A short message to show (e.g. "Couldn't reach Pockle. Trying again…"), or null.</summary>
        public string Message;
        /// <summary>Odds for the first banked box (or the next box when none is banked), by tier, as the server will apply
        /// them. Null until the server sends them; the UI never invents odds.</summary>
        public IReadOnlyList<TierOdds> Odds;

        public int Banked => BankedCollectionIds?.Count ?? 0;
        public bool BankFull => Banked >= MaxBanked;
        public bool DayDone => EarnedToday >= MaxPerDay;
        public bool Online => Connection == WalkingBoxConnection.Online;
        public bool OpenEnabled => Online && !Busy && CanOpen && Banked > 0;
    }

    /// <summary>The chance of one rarity tier in a box, as the server will apply it (0 to 1).</summary>
    public readonly struct TierOdds
    {
        public TierOdds(RarityTier tier, double chance) { Tier = tier; Chance = chance; }
        public RarityTier Tier { get; }
        public double Chance { get; }

        /// <summary>"79%", "6%", "about 1 in 72" (anything under 2% reads as "1 in N").</summary>
        public static string Format(double chance)
        {
            if (chance <= 0) return "none";
            if (chance >= .02) return Math.Round(chance * 100, chance >= .1 ? 0 : 1).ToString("0.#") + "%";
            return "about 1 in " + Math.Round(1 / chance).ToString("N0");
        }
    }
}
