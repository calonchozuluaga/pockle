using System;
using Pockle.Core;
internal static class ReactionResponseChecks
{
    public static void Run()
    {
        int checkedCount=0;
        foreach(var pair in new[]{("pip",typeof(PipAction)),("fox",typeof(FoxAction)),("axolotl",typeof(AxolotlAction)),("firb",typeof(FirbAction)),("coco",typeof(CocoAction)),("snowy",typeof(SnowyAction)),("ginger",typeof(GingerAction)),("reiny",typeof(WinterTrioAction)),("guin",typeof(WinterTrioAction)),("bundy",typeof(WinterTrioAction)),("wally",typeof(WallyAction))})
        {
            foreach(Enum action in Enum.GetValues(pair.Item2))
            {
                var response=ToyReactionResponses.Action(pair.Item1,action);
                if(string.IsNullOrEmpty(response.Text)||response.AudioResource!=CollectionVoicePolicy.ActionResource(pair.Item1,action))throw new Exception("Action response lost its approved clip");
                if(action.ToString().StartsWith("Sleepy")&&(response.Face!=PipTemperament.Sleepy||response.Text!="*yawn*"||response.AllowsMotionExpression))throw new Exception("Sleepy response split across outputs");
                if(response.Face==PipTemperament.Sleepy)
                {var pose=ToyReactionResponses.FacePose(response,.4f);if(pose.Lids<.7f||pose.Blink<.7f||pose.MouthOpen<.5f)throw new Exception("A yawn must retain sleepy eyes and an open mouth");}
                checkedCount++;
            }
            foreach(PipTemperament face in Enum.GetValues(typeof(PipTemperament)))
            {
                var response=ToyReactionResponses.Temperament(pair.Item1,face);
                if(response.Face!=face||response.AudioResource!=CollectionVoicePolicy.TemperamentResource(pair.Item1,face)||response.AllowsMotionExpression)throw new Exception("Personality response split across outputs");
                if(face==PipTemperament.Grumpy&&(response.Text!="hmph!"||PipFaces.Pose(response.Face,1).Frown<=0))throw new Exception("Grumpy sound must pair with a frown");
                checkedCount++;
            }
        }
        if(ToyReactionResponses.Action("fox",PipAction.Hop).AudioResource!=null)throw new Exception("Cross-character action accepted");
        Console.WriteLine("PASS: "+checkedCount+" action/temperament response bundles across all eleven collections.");
    }
}
