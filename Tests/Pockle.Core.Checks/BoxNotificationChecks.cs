using System;
using Pockle.Core;
internal static class BoxNotificationChecks
{
    public static void Run()
    {
        var p = new BoxNotificationPolicy();
        Check(!p.Observe(20, 0, new string[0], false));
        Check(p.ReserveEligibility(20)); Check(!p.ReserveEligibility(20));
        Check(!p.Observe(20, 1, new[]{"a"}, true));
        Check(!p.Observe(20, 1, new[]{"a"}, false));
        Check(p.Observe(20, 2, new[]{"a","b"}, false));
        Check(!p.Observe(20, 3, new[]{"b","c"}, false));
        Check(!p.Observe(19, 4, new[]{"d"}, false));
        Check(p.ReserveEligibility(21));
        Check(p.Observe(21, 1, new[]{"e"}, false));
        Console.WriteLine("PASS: box notification baseline, foreground, replay, daily caps and rollover.");
    }
    private static void Check(bool value) { if (!value) throw new Exception("Box notification policy regression"); }
}
