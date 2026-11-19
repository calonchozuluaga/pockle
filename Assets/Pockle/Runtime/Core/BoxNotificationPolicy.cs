using System;
using System.Collections.Generic;

namespace Pockle.Core
{
    /// <summary>Persistent presentation history only; never grants boxes or infers an award from steps.</summary>
    [Serializable]
    public sealed class BoxNotificationPolicy
    {
        public int EligibilityDay = -1, EarnedDay = -1, LastDay = -1, LastEarned;
        public List<string> Seen = new List<string>();
        public bool Observe(int day, int earned, string[] banked, bool foreground)
        {
            if (day < 0 || day < LastDay || earned < 0 || earned > 4 || banked == null) return false;
            bool first = LastDay < 0, fresh = false;
            foreach (string id in banked)
                if (!string.IsNullOrEmpty(id) && !Seen.Contains(id)) { Seen.Add(id); fresh = true; }
            bool increased = day > LastDay ? earned > 0 : earned > LastEarned;
            LastDay = day; LastEarned = earned;
            // Retain current bank only: completed old IDs cannot return as a newly earned edge without an increased count.
            Seen.RemoveAll(id => Array.IndexOf(banked, id) < 0);
            if (first || foreground || !fresh || !increased || EarnedDay >= day) return false;
            EarnedDay = day; return true;
        }
        public bool ReserveEligibility(int day)
        {
            if (day < 0 || EligibilityDay >= day) return false;
            EligibilityDay = day; return true;
        }
    }
}
