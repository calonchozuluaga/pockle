using System;
using System.Collections.Generic;
using System.Text.Json;
using Pockle.Core;
internal static class FusionSpendChecks
{
    private static void Check(bool value,string why) { if(!value) throw new Exception(why); }
    private static FusedToy Result(string a,string b,int n) => new FusedToy { Id="fusion.spend-"+n, First=a,Second=b,Name="Spent once",Material="Mixed",Personality="Calm",Temperament=7,Energy=40,Mood=70,Boldness=45,Bedtime=40 };
    public static void Run()
    {
        var options=new JsonSerializerOptions {IncludeFields=true};
        foreach(bool same in new[]{false,true})
        {
            string a=ToyCatalog.PeachId,b=same?a:ToyCatalog.MoonId;
            var save=new CollectionSave { Version=CollectionProgress.SaveVersion, Day=42, Inventory=new List<OwnedCollectible>{new OwnedCollectible(a,same?5:3)} };
            if(!same)save.Inventory.Add(new OwnedCollectible(b,3));
            var progress=new CollectionProgress(save,42);
            for(int i=0;i<2;i++)
            {
                var result=Result(a,b,i);
                Check(progress.TryFuse(result),"Available spare copies must fuse.");
                Check(!progress.TryFuse(result),"A saved fusion cannot consume again.");
                Check(progress.FinishFusion(result.Id),"Acknowledge saved reveal.");
                progress=new CollectionProgress(JsonSerializer.Deserialize<CollectionSave>(JsonSerializer.Serialize(progress.Save,options),options),42);
                Check(progress.FusionCopiesSpent(a)==(i+1)*(same?2:1),"Persistent spend history is wrong.");
            }
            Check(progress.GetOwnedCount(a)==1&&progress.GetOwnedCount(b)==1,"Only the display copies may remain.");
            Check(!progress.TryFuse(Result(a,b,2)),"Spent copies must not be reused after restart.");
            Check(progress.FusionsMade==2,"An exhausted fusion must not create a result.");
        }
        Check(CollectionVoicePolicy.Resource("axolotl",CollectionVoiceMood.Happy)=="","Unapproved collection must not borrow another voice.");
        Check(CollectionVoicePolicy.For(PipAction.CuriousTilt)==CollectionVoiceMood.Curious,"Pip curious routing.");
        Check(CollectionVoicePolicy.For(FoxAction.Jump)==CollectionVoiceMood.Playful,"Fox jump routing.");
        Check(CollectionVoicePolicy.For(WallyAction.FlipperWave)==CollectionVoiceMood.Happy,"Wally greeting routing.");
        var voicePaths = new HashSet<string>();
        foreach (string character in new[]{"pip","fox","axolotl","firb","coco","snowy","ginger","reiny","guin","bundy","wally"})
        {
            Type actionType = character == "pip" ? typeof(PipAction) : character == "fox" ? typeof(FoxAction)
                : character == "axolotl" ? typeof(AxolotlAction) : character == "firb" ? typeof(FirbAction)
                : character == "coco" ? typeof(CocoAction) : character == "snowy" ? typeof(SnowyAction)
                : character == "ginger" ? typeof(GingerAction) : character == "wally" ? typeof(WallyAction) : typeof(WinterTrioAction);
            foreach (Enum action in Enum.GetValues(actionType))
            {
                string path = CollectionVoicePolicy.ActionResource(character, action);
                Check(path.StartsWith("Audio/Collections/" + character + "/") && voicePaths.Add(path), "Missing or duplicate collection action cue.");
            }
            foreach (PipTemperament face in Enum.GetValues(typeof(PipTemperament)))
                Check(voicePaths.Add(CollectionVoicePolicy.TemperamentResource(character, face)), "Duplicate temperament cue.");
        }
        Check(voicePaths.Count == 143, "Eleven collections must each have five action and eight temperament slots.");
        Check(CollectionVoicePolicy.ActionResource("fox", PipAction.Hop) == "", "Cross-character action must be rejected.");
        Check(CollectionVoicePolicy.ActionResource("unknown", PipAction.Hop) == "", "Unknown character must be rejected.");
        Check(CollectionVoicePolicy.ActionResource("pip", (PipAction)99) == "", "Unknown action must be rejected.");
        Check(CollectionVoicePolicy.TemperamentResource("pip", (PipTemperament)99) == "", "Unknown temperament must be rejected.");
        Check(CollectionVoicePolicy.ActionResource("pip", PipAction.ThankYouBow).EndsWith("/thank-you-bow"), "Action slug must match the audition manifest.");
        Check(CollectionVoicePolicy.TemperamentResource("fox", PipTemperament.ShowOff).EndsWith("/temperament-show-off"), "Temperament slug must match the audition manifest.");
        Console.WriteLine("PASS: single-use fusion spares, same-parent double spending, persistent balances/receipts, exhaustion and approved voice routing.");
    }
}
