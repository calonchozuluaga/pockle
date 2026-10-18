using System;
using Pockle.Core;

internal static class PeachExpressionChecks
{
    public static void Run()
    {
        Check(PeachExpression.Sample(0, 0, 0, false), 0, 0, 0);
        Check(PeachExpression.Sample(4.66, 0, 0, false), 1, 0, 0);
        Check(PeachExpression.Sample(9.76, 0, 0, false), 1, 0, 0);
        Check(PeachExpression.Sample(0, .24f, 0, false), 0, 1, 0);
        Check(PeachExpression.Sample(0, 0, .6f, false), 0, 0, 1);
        Check(PeachExpression.Sample(4.66, 1, 1, true), 0, 0, 0);
        Check(PeachExpression.Sample(double.NaN, 1, 1, false), 0, 0, 0);
        Check(PeachExpression.Sample(double.PositiveInfinity, 1, 1, false), 0, 0, 0);
        foreach (float value in new[] { float.NaN, float.PositiveInfinity, -1f, 0, .1f, .24f, 100f })
            for (int frame = 0; frame < 1800; frame++)
            {
                var pose = PeachExpression.Sample(frame / 60d, value, value, false);
                foreach (float channel in new[] { pose.Blink, pose.Happy, pose.Surprise })
                    if (float.IsNaN(channel) || channel < 0 || channel > 1) throw new Exception("Peach expression escaped bounds.");
            }
        if (PeachExpression.Sample(0, .24f, .6f, false).Surprise != 0) throw new Exception("Happy/surprise priority overlapped.");
        Console.WriteLine("PASS: Peach expression timing, restart phase, calm, input bounds and reaction priority.");
    }
    private static void Check(PeachExpressionPose pose, float blink, float happy, float surprise)
    {
        if (Math.Abs(pose.Blink-blink) > .0001 || Math.Abs(pose.Happy-happy) > .0001 || Math.Abs(pose.Surprise-surprise) > .0001)
            throw new Exception("Peach expression timing or reaction differs.");
    }
}
