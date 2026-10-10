using System.Collections.Generic;

namespace Pockle.Core
{
    public enum AppPage { Home, Shelf, Rewards, Social, Profile, Settings, Boxes, Play, Collections, Collection }

    /// <summary>Navigation history and per-page scroll survive visits, tabs and toy play.</summary>
    public sealed class MenuNavigation
    {
        private readonly Stack<AppPage> history = new Stack<AppPage>();
        private readonly float[] scroll = new float[System.Enum.GetValues(typeof(AppPage)).Length];
        public AppPage Current { get; private set; } = AppPage.Home;
        public float Scroll => scroll[(int)Current];

        public void SaveScroll(float value) { scroll[(int)Current] = Numeric.IsFinite(value) ? System.Math.Max(0, value) : 0; }
        public bool Push(AppPage next)
        {
            if (!System.Enum.IsDefined(typeof(AppPage), next))
                throw new System.ArgumentOutOfRangeException(nameof(next));
            if (next == Current) return false;
            history.Push(Current); Current = next; return true;
        }
        public void SelectTab(AppPage tab)
        {
            if (tab != AppPage.Home && tab != AppPage.Boxes && tab != AppPage.Profile)
                throw new System.ArgumentException("Not a main navigation destination.");
            if (Current == tab) return;
            history.Clear();
            if (tab != AppPage.Home) history.Push(AppPage.Home);
            Current = tab;
        }
        public bool Back()
        {
            if (history.Count == 0) return false;
            Current = history.Pop(); return true;
        }
    }
}
