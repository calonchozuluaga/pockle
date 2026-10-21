using System;
using UnityEngine;

namespace Pockle.Runtime
{
    // UX-050 onboarding (owner, October 21): 13+, no guest mode. Splash, birthday, sign in, username, tutorial.
    // The screens are Claude's (PrototypeHud.Onboarding.cs). Sign-in, account sync and username claims are Codex's runtime:
    // implement IAccountService and call hud.SetAccountService(...). See docs/handoffs/claude.md, October 21.

    public enum AccountProvider { Apple, Google }

    public enum AccountStatus { SignedIn, Cancelled, Failed }

    public readonly struct AccountResult
    {
        public AccountResult(AccountStatus status, string message = "", bool hasUsername = false)
        { Status = status; Message = message ?? ""; HasUsername = hasUsername; }
        public AccountStatus Status { get; }
        /// <summary>Shown to the player on failure, e.g. "No connection. Try again."</summary>
        public string Message { get; }
        /// <summary>True for a returning player whose account already has a username; the username step is skipped.</summary>
        public bool HasUsername { get; }
    }

    public enum UsernameCheck { Available, Taken, NotAllowed, Failed }

    /// <summary>Runtime account contract. Callbacks may arrive on any later frame but must arrive on the main thread.</summary>
    public interface IAccountService
    {
        bool IsSignedIn { get; }
        /// <summary>Signs in or creates the account. A phone with an existing local save must link that save to the account,
        /// never discard it.</summary>
        void SignIn(AccountProvider provider, Action<AccountResult> done);
        /// <summary>Reserves the username for this account (also checks moderation). Available means it is now claimed.</summary>
        void ClaimUsername(string username, Action<UsernameCheck> done);
    }

    /// <summary>Pure rules for the age gate and usernames, so they can be tested without a scene.</summary>
    public static class OnboardingRules
    {
        public const int MinimumAge = 13;
        public const int UsernameMin = 3, UsernameMax = 16;

        /// <summary>Whole years between a birthday and today. Feb 29 birthdays count from Mar 1 in other years.</summary>
        public static int AgeOn(DateTime today, int year, int month, int day)
        {
            int age = today.Year - year;
            bool beforeBirthday = today.Month < month || today.Month == month && today.Day < day;
            return beforeBirthday ? age - 1 : age;
        }

        /// <summary>True when the three parts make a real date in the past (not after <paramref name="today"/>).</summary>
        public static bool IsRealBirthday(DateTime today, int year, int month, int day)
        {
            if (year < today.Year - 120 || year > today.Year || month < 1 || month > 12 || day < 1) return false;
            if (day > DateTime.DaysInMonth(year, month)) return false;
            return new DateTime(year, month, day) <= today.Date;
        }

        /// <summary>Letters, digits and underscores, 3 to 16 characters. Returns a short reason when it doesn't fit, else null.</summary>
        public static string UsernameProblem(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length < UsernameMin) return "Use at least " + UsernameMin + " letters or numbers.";
            if (name.Length > UsernameMax) return "Keep it to " + UsernameMax + " characters.";
            foreach (char c in name)
                if (!(c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= '0' && c <= '9' || c == '_'))
                    return "Only letters, numbers and _ please.";
            return null;
        }

        private static readonly string[] Moods = { "Jolly", "Sunny", "Comet", "Lucky", "Bouncy", "Sparkle", "Mellow", "Cosmic", "Velvet", "Zippy" };
        private static readonly string[] Things = { "Jelly", "Squish", "Shelf", "Pocket", "Mochi", "Pearl", "Bubble", "Glow", "Gumdrop", "Wobble" };

        /// <summary>A friendly name that doesn't use the player's real name, e.g. "CometMochi482".</summary>
        public static string SuggestUsername(System.Random random)
        {
            return Moods[random.Next(Moods.Length)] + Things[random.Next(Things.Length)] + random.Next(10, 1000);
        }
    }

    /// <summary>
    /// What this phone remembers about onboarding. Only the age gate's pass/fail and a lockout flag are stored, never the
    /// birthday itself, and a failed gate can't be retried by entering a different date.
    /// </summary>
    public static class OnboardingState
    {
        private const string Prefix = "pockle.onboarding.v1.";
        public static bool AgeConfirmed => PlayerPrefs.GetInt(Prefix + "ageConfirmed", 0) == 1;
        public static bool AgeLocked => PlayerPrefs.GetInt(Prefix + "ageLocked", 0) == 1;
        public static bool Completed => PlayerPrefs.GetInt(Prefix + "completed", 0) == 1;

        public static void ConfirmAge() { PlayerPrefs.SetInt(Prefix + "ageConfirmed", 1); PlayerPrefs.Save(); }
        public static void LockAge() { PlayerPrefs.SetInt(Prefix + "ageLocked", 1); PlayerPrefs.Save(); }
        public static void Complete() { PlayerPrefs.SetInt(Prefix + "completed", 1); PlayerPrefs.Save(); }

        /// <summary>Development builds only: forget everything so the owner can test the flow again.</summary>
        public static void ResetForTesting()
        {
            if (!Debug.isDebugBuild && !Application.isEditor) return;
            PlayerPrefs.SetInt(Prefix + "ageConfirmed", 0); PlayerPrefs.SetInt(Prefix + "ageLocked", 0);
            PlayerPrefs.SetInt(Prefix + "completed", 0); PlayerPrefs.Save();
        }
    }
}
