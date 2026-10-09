using System;
using Pockle.Core;

internal static class ExpressionChecks
{
    private static int checks;
    public static void Run()
    {
        var face = new FaceExpressionController();
        face.Reset();
        Check(face.Expression == FaceExpression.Happy && face.Pose.EyeOpen == 1, "Rest face changed.");
        Step(face, held: true, compression: .23f);
        Check(face.Expression == FaceExpression.Delighted, "Press did not delight.");
        Step(face, held: true, compression: .23f, height: .4f);
        Check(face.Expression == FaceExpression.Curious, "Lift did not take priority over compression.");
        Step(face, held: true, pinch: .3f);
        Check(face.Expression == FaceExpression.Curious, "Two-finger stretch did not show curiosity.");
        Step(face, held: true, pinch: -.2f);
        Check(face.Expression == FaceExpression.Delighted, "Two-finger squeeze did not delight.");
        Step(face, landed: true);
        Check(face.Expression == FaceExpression.Delighted, "Landing did not delight.");
        for (int i = 0; i < 30; i++) Step(face);
        Check(face.Expression == FaceExpression.Delighted, "Brief reaction was lost on release.");
        for (int i = 0; i < 40; i++) Step(face);
        Check(face.Expression == FaceExpression.Happy, "Reaction did not settle to happy.");
        for (int i = 0; i < 900; i++) Step(face);
        Check(face.Expression == FaceExpression.Sleepy && face.Pose.EyeOpen < .001f, "Idle face did not sleep.");
        Step(face, held: true);
        Check(face.Expression == FaceExpression.Happy, "Direct touch did not wake sleepy face.");
        face.Reset(); Step(face, shake: .2f);
        Check(face.Expression == FaceExpression.Delighted, "Shake did not delight.");
        face.Preview(FaceExpression.Sleepy);
        for (int i = 0; i < 90; i++) Step(face, held: true, compression: .3f, shake: 1);
        Check(face.IsPreview && face.Expression == FaceExpression.Sleepy && face.Pose.EyeOpen < .001f, "Gesture overwrote manual preview.");
        face.Preview(null); Step(face, held: true, height: .3f, calm: true);
        Check(!face.IsPreview && face.Expression == FaceExpression.Curious && face.Pose.EyeOpen == 1, "Auto/calm did not restore direct feedback.");
        bool rejected = false;
        try { face.Preview((FaceExpression)999); } catch (ArgumentOutOfRangeException) { rejected = true; }
        Check(rejected, "Invalid preview accepted.");

        foreach (FaceExpression expression in Enum.GetValues<FaceExpression>())
        {
            FacePose pose = FacePose.For(expression);
            for (int i = 0; i <= 120; i++) CheckPose(pose, i / 120f);
            if (expression == FaceExpression.Curious || expression == FaceExpression.Delighted)
            {
                Point3 first = pose.MouthPoint(0), last = pose.MouthPoint(1);
                Check(Math.Abs(first.X - last.X) < .00001f && Math.Abs(first.Y - last.Y) < .00001f, "Open mouth did not close.");
            }
        }
        Check(FacePose.For(FaceExpression.Sleepy).EyeArc < 0 && FacePose.For(FaceExpression.Delighted).EyeArc > 0,
            "Sleepy and delighted eyelids are indistinguishable.");
        Check(FacePose.For(FaceExpression.Curious).MouthPoint(0).X != FacePose.For(FaceExpression.Delighted).MouthPoint(0).X,
            "Curious mouth must be smaller than delighted.");
        // Variable frame timing and every transition remain bounded, finite, and without timer carryover.
        foreach (float dt in new[] { 1f / 240, 1f / 60, 1f / 15, .5f, float.NaN, float.PositiveInfinity, -1, 0 })
        {
            face.Reset();
            foreach (FaceExpression from in Enum.GetValues<FaceExpression>())
            foreach (FaceExpression to in Enum.GetValues<FaceExpression>())
            {
                face.Preview(from); face.Step(0, false, 0, 0, 0, 0, 0, false, true);
                face.Preview(to);
                for (int i = 0; i < 50; i++)
                {
                    FacePose pose = face.Step(dt, false, float.NaN, float.PositiveInfinity, -1, float.NaN, float.NaN, false, false);
                    CheckPose(pose, i / 49f);
                    Check(pose.Curious + pose.Sleepy + pose.Delighted <= 1.00001f, "Face blend weights escaped simplex.");
                }
            }
        }
        face.Reset(); face.Step(60, false, 0, 0, 0, 0, 0, false, false);
        Check(face.Expression == FaceExpression.Happy, "Resume hitch jumped straight to sleepy.");
        face.Reset(); bool blinked = false, reopened = false;
        for (int i = 0; i < 330; i++)
        {
            FacePose pose = Step(face);
            if (pose.Blink > .9f) blinked = true;
            if (blinked && pose.Blink == 0) reopened = true;
        }
        Check(blinked && reopened, "Automatic blink did not close and reopen.");
        face.Reset();
        for (int i = 0; i < 600; i++) Check(Step(face, calm: true).Blink == 0, "Calm mode blinked.");
        face.Reset(); face.Preview(FaceExpression.Delighted);
        for (int i = 0; i < 100; i++) Step(face);
        Check(Math.Abs(face.Pose.Delighted - 1) < .00001f, "Face failed to converge.");
        face.Reset();
        for (int i = 0; i < 100; i++) Step(face);
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 10000; i++) face.Step(.016f, i % 2 == 0, .2f, 0, 0, 0, 0, false, false);
        Check(GC.GetAllocatedBytesForCurrentThread() == before, "Face clock allocated per frame.");
        Console.WriteLine("PASS: " + checks + " expression assertions (reactions, preview, blending, mouth curves, timing, calm, allocations).");
    }
    private static FacePose Step(FaceExpressionController face, bool held = false, float compression = 0,
        float height = 0, float pinch = 0, float shake = 0, bool landed = false, bool calm = false)
    { return face.Step(1f / 60, held, compression, 0, height, pinch, shake, landed, calm); }
    private static void CheckPose(FacePose pose, float t)
    {
        Point3 point = pose.MouthPoint(t);
        Check(float.IsFinite(point.X) && float.IsFinite(point.Y) && Math.Abs(point.X) <= 1.00001f && point.Y >= -.60001f && point.Y <= 1.20001f,
            "Mouth curve escaped its safe bounds.");
        Check(float.IsFinite(pose.EyeOpen) && pose.EyeOpen >= 0 && pose.EyeOpen <= 1 && pose.MouthOpen >= 0 && pose.MouthOpen <= 1.00001f,
            "Face feature scales became invalid.");
    }
    private static void Check(bool condition, string message) { checks++; if (!condition) throw new InvalidOperationException(message); }
}
