using System;
using System.IO;
using Pockle.Core;

internal static class RevealChecks
{
    private static int assertions;
    private static void Check(bool pass, string message)
    {
        assertions++;
        if (!pass) throw new InvalidDataException(message);
    }

    internal static void Run()
    {
        foreach (bool calm in new[] { false, true })
        {
            float duration = RevealSequence.Duration(calm);
            RevealPose first = RevealSequence.Sample(0f, calm);
            Check(!first.ToyVisible && !first.Finished && first.BoxAlpha == 1f && first.Opening == 0f,
                "Reveal did not begin with a closed mystery box.");
            float previousOpening = 0f, previousAlpha = 1f;
            float peakLift = 0f, peakCompression = 0f;
            for (int frame = 0; frame <= 400; frame++)
            {
                float elapsed = duration * (frame / 400f);
                RevealPose pose = RevealSequence.Sample(elapsed, calm);
                Check(Finite(pose.Opening) && Finite(pose.BoxAlpha) && Finite(pose.ToyScale) &&
                    Finite(pose.ToyLift) && Finite(pose.Compression), "Reveal pose became nonfinite.");
                Check(pose.Opening >= previousOpening && pose.Opening <= 1f, "Lid reversed or escaped its opening range.");
                Check(pose.BoxAlpha <= previousAlpha && pose.BoxAlpha >= 0f, "Packaging reappeared during reveal.");
                Check(pose.ToyScale > 0f && pose.ToyScale <= 1f && pose.ToyLift >= 0f && pose.ToyLift <= 1.05f &&
                    pose.Compression >= 0f && pose.Compression <= .12f, "Reveal escaped toy bounds.");
                Check(!pose.ToyVisible || pose.Opening > .8f, "Toy appeared before the lid had opened.");
                Check(!pose.Finished || (pose.ToyVisible && pose.BoxAlpha == 0f && pose.ToyScale == 1f &&
                    pose.ToyLift == 0f && pose.Compression == 0f), "Touch handoff preceded a settled reveal pose.");
                Check(pose.Finished == (frame == 400), "Reveal completion happened at the wrong time.");
                if (calm) Check(pose.ToyScale == 1f && pose.ToyLift == 0f && pose.Compression == 0f,
                    "Reduced-motion reveal retained the large rise or bounce.");
                peakLift = Math.Max(peakLift, pose.ToyLift);
                peakCompression = Math.Max(peakCompression, pose.Compression);
                previousOpening = pose.Opening;
                previousAlpha = pose.BoxAlpha;
            }
            if (!calm) Check(peakLift > 1f && peakCompression > .10f, "Full reveal lost emergence or settling.");
            RevealPose after = RevealSequence.Sample(duration + 10f, calm);
            Check(after.Finished && after.ToyVisible && after.BoxAlpha == 0f && after.ToyLift == 0f,
                "A delayed frame restored packaging or stranded Pip in the air.");
            RevealPose replay = RevealSequence.Sample(0f, calm);
            Check(!replay.Finished && !replay.ToyVisible && replay.BoxAlpha == 1f,
                "Replay retained the previous reveal's completion state.");
            foreach (float invalid in new[] { float.NaN, float.NegativeInfinity, float.PositiveInfinity, -1f })
            {
                RevealPose repaired = RevealSequence.Sample(invalid, calm);
                Check(!repaired.Finished && !repaired.ToyVisible && repaired.BoxAlpha == 1f,
                    "Invalid elapsed time escaped the safe closed pose.");
            }
        }
        Check(RevealSequence.Duration(true) < RevealSequence.Duration(false), "Calm reveal is not shorter.");
        Console.WriteLine("PASS: " + assertions + " reveal assertions (opening order, emergence, settled handoff, replay, and calm motion).");
    }

    private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
}
