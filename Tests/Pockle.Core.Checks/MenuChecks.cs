using System;
using System.Globalization;
using Pockle.Core;

internal static class MenuChecks
{
    private static int checks;
    public static void Run()
    {
        var menu = new MenuNavigation();
        Assert(menu.Current == AppPage.Home && !menu.Back(), "Cold start or root Back does not match Home.");
        menu.Push(AppPage.Shelf); menu.SaveScroll(127);
        menu.Push(AppPage.Play); menu.Push(AppPage.Settings); menu.SaveScroll(290);
        Assert(menu.Back() && menu.Current == AppPage.Play, "Settings lost the current toy play destination.");
        Assert(menu.Back() && menu.Current == AppPage.Shelf && menu.Scroll == 127, "Returning from play lost the shelf position.");
        Assert(menu.Push(AppPage.Settings) && menu.Scroll == 290, "Settings lost its own scroll position.");
        Assert(!menu.Push(AppPage.Settings), "Repeated taps created duplicate history entries.");
        Assert(menu.Back() && menu.Current == AppPage.Shelf, "Repeated settings tap changed Back behavior.");
        menu.SelectTab(AppPage.Boxes); menu.SaveScroll(450); menu.SelectTab(AppPage.Profile);
        Assert(menu.Back() && menu.Current == AppPage.Home && !menu.Back(), "Tab changes left a hidden stack of old pages.");
        menu.Push(AppPage.Shelf);
        Assert(menu.Scroll == 127, "Tab navigation erased collection context.");
        menu.SelectTab(AppPage.Boxes);
        Assert(menu.Scroll == 450, "Returning to Boxes lost its scroll position.");
        menu.SelectTab(AppPage.Boxes);
        Assert(menu.Back() && menu.Current == AppPage.Home, "Repeated tab selection duplicated Home history.");
        menu.Push(AppPage.Rewards); menu.Push(AppPage.Shelf); menu.Push(AppPage.Play);
        Assert(menu.Back() && menu.Current == AppPage.Shelf, "A reward reveal does not return to the toy's shelf.");
        Assert(menu.Back() && menu.Current == AppPage.Rewards, "Reward navigation lost the originating page.");
        menu.SelectTab(AppPage.Home); menu.Push(AppPage.Social); menu.Push(AppPage.Profile);
        Assert(menu.Back() && menu.Current == AppPage.Social && menu.Back() && menu.Current == AppPage.Home, "Social/profile Back did not preserve its entry point.");
        foreach (float value in new[] { -1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity })
        { menu.SaveScroll(value); Assert(menu.Scroll == 0, "Invalid scroll value escaped bounds."); }
        bool rejected = false;
        try { menu.SelectTab(AppPage.Play); } catch (ArgumentException) { rejected = true; }
        Assert(rejected && menu.Current == AppPage.Home, "An invalid tab damaged navigation state.");

        // Collection browsing has its own Back chain and scroll, including a return from toy play.
        menu.Push(AppPage.Collections); menu.SaveScroll(810);
        menu.Push(AppPage.Collection); menu.SaveScroll(240);
        menu.Push(AppPage.Play);
        Assert(menu.Back() && menu.Current == AppPage.Collection && menu.Scroll == 240,
            "Toy play lost the originating collection lineup and its scroll.");
        Assert(menu.Back() && menu.Current == AppPage.Collections && menu.Scroll == 810,
            "Collection Back lost the series list and its scroll.");
        Assert(menu.Back() && menu.Current == AppPage.Home && !menu.Back(), "Series browsing lost Home.");
        foreach (AppPage candidate in new[] { AppPage.Collections, AppPage.Collection })
        {
            rejected = false;
            try { menu.SelectTab(candidate); } catch (ArgumentException) { rejected = true; }
            Assert(rejected && menu.Current == AppPage.Home, "Collection page was treated as a main tab.");
        }
        rejected = false;
        try { menu.Push((AppPage)999); } catch (ArgumentOutOfRangeException) { rejected = true; }
        Assert(rejected && menu.Current == AppPage.Home && !menu.Back(), "Unknown page damaged Back history.");
        menu.Push(AppPage.Collection); menu.SelectTab(AppPage.Profile);
        Assert(menu.Back() && menu.Current == AppPage.Home && !menu.Back(), "Tab change retained hidden collection history.");

        Assert(ProfileName.Normalize(null!) == "Collector" && ProfileName.Normalize(" \t\n") == "Collector", "An empty display name did not fall back.");
        Assert(ProfileName.Normalize("  Lucía 李  ") == "Lucía 李", "Non-English names or whitespace trimming broke.");
        Assert(ProfileName.Normalize("A\nB\tC\u2028D\u2029E\u202eF") == "ABCDEF", "Control/bidi characters escaped display-name normalization.");
        Assert(ProfileName.Normalize("<b>Pip</b>") == "<b>Pip</b>", "Literal angle brackets were treated as markup by the name model.");
        string combined = string.Concat(System.Linq.Enumerable.Repeat("e\u0301", 24));
        string emoji = string.Concat(System.Linq.Enumerable.Repeat("\U0001F31F", 24));
        foreach (string input in new[] { combined, emoji, new string('x', 100), "\u200d" + emoji })
        {
            string name = ProfileName.Normalize(input);
            Assert(StringInfo.ParseCombiningCharacters(name).Length == 20, "Name limit split text elements or exceeded 20.");
            Assert(!char.IsHighSurrogate(name[name.Length - 1]), "Name truncation split an emoji surrogate pair.");
            Assert(ProfileName.Normalize(name) == name, "Normalizing a saved name changed it again.");
        }
        Assert(ProfileName.Normalize("\U0001F469\u200d\U0001F680") == "\U0001F469\u200d\U0001F680", "Joined emoji lost its joiner.");
        Console.WriteLine("PASS: " + checks + " menu/profile assertions (Back, tabs, repeated taps, per-page scroll, Unicode names).");
    }
    private static void Assert(bool condition, string message)
    { checks++; if (!condition) throw new InvalidOperationException(message); }
}
