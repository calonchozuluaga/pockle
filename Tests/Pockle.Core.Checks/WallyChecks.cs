using System;
using System.Linq;
using Pockle.Core;
internal static class WallyChecks
{
    private static void Check(bool value,string message){if(!value)throw new Exception(message);}
    public static void Run()
    {
        Check(ToyCatalog.TryGetCollection("wally",out var c)&&c.DisplayName=="Wally"&&c.Members.Count==15&&c.SecretId==WallyCatalog.SpecialId,"Wally needs fifteen own finishes and one molten secret.");
        Check(c.Members.All(id=>ToyCatalog.TryGetCollectible(id,out var t)&&t.Available&&t.CharacterId=="wally"&&!ToyCatalog.IsDailyEligible(id)),"Wally art cannot change rewards.");
        Check(ToyCatalog.TryGetCharacter("wally",out var ch)&&ch.LeadFinishId=="gloss-vinyl","Wally should lead with the approved blue whale.");
        Check(c.Worlds.Count==5 && c.HomeWorld=="wally-coral-cove" && c.Worlds[4].Id=="wally-midnight-trench","Wally needs five own worlds including a dark one.");
        Check(c.CoverArt=="Collections/Wally/Cover" && c.BoxArt=="UI/Boxes/Collection-wally" && c.BoxModel=="Boxes/WallyCollectionBox","Wally needs its own cover and real carton resource bindings.");
        Check(c.Members.Count(id=>ToyCatalog.TryGetCollectible(id,out var t)&&t.Tier==ToyTier.Common)==9,"Nine commons.");
        Check(c.Members.Count(id=>ToyCatalog.TryGetCollectible(id,out var t)&&t.Tier==ToyTier.Rare)==3,"Three gold rares.");
        Check(c.Members.Count(id=>ToyCatalog.TryGetCollectible(id,out var t)&&t.Tier==ToyTier.Epic)==2,"Two epics.");
        Check(c.Members.Count(id=>ToyCatalog.TryGetCollectible(id,out var t)&&t.Tier==ToyTier.Secret)==1,"One molten secret.");
        Check(ToyCatalog.TryGetCollectible(WallyCatalog.GlowId,out var glow)&&glow.Tier==ToyTier.Epic&&!glow.IsSecret,"Owned glow ID becomes Epic.");
        Check(c.Members.Take(9).All(id=>WallyCatalog.Tier(id)==ToyTier.Common)&&c.Members[14]==WallyCatalog.SpecialId,"Lineup groups tiers with molten last.");
        Check(!c.HasApprovedOdds,"Pilot art does not approve draw odds.");
        var legacy=new CollectionProgress(new CollectionSave{Version=4,Day=42,Inventory=new System.Collections.Generic.List<OwnedCollectible>{new OwnedCollectible(WallyCatalog.GlowId,7),new OwnedCollectible("wally.coated-metallic",5)}},42);
        Check(legacy.GetOwnedCount(WallyCatalog.GlowId)==7&&legacy.GetOwnedCount("wally.coated-metallic")==5&&legacy.Save.Version==4,"Tier changes preserve legacy ownership without a save migration.");
        Check(CharacterMotion.TryGetProfile("wally",out _),"Wally needs its own shelf idle.");
        foreach(WallyAction action in Enum.GetValues(typeof(WallyAction)))
        {
            for(int i=0;i<=100;i++)
            {
                var p=WallyReactions.Sample(action,i/100f,false);
                Check(p.Lift>=0&&p.Lift<=.361f&&Math.Abs(p.Tail)<=1.001f&&Math.Abs(p.Flipper)<=1.001f&&Math.Abs(p.Roll)<=18.01f&&Math.Abs(p.Pitch)<=16.01f,"Whale poses stay bounded.");
                var calm=WallyReactions.Sample(action,i/100f,true);
                Check(calm.Lift==0&&calm.Pitch==0&&calm.Yaw==0&&calm.Roll==0&&calm.Tail==0&&calm.Flipper==0&&calm.Face.Blink==0,"Calm mode has no whale movement.");
            }
            foreach(float t in new[]{-1f,0,1,float.NaN,float.PositiveInfinity})
            {var p=WallyReactions.Sample(action,t,false);Check(p.Lift==0&&p.Tail==0&&p.Flipper==0&&p.Face.Happy==0,"Endpoints and invalid time return to rest.");}
        }
        for(int last=-1;last<5;last++)
        {
            var selected=new System.Collections.Generic.HashSet<WallyAction>();
            for(int draw=0;draw<(last<0?5:4);draw++){var a=WallyReactions.Choose(last,draw);Check((int)a!=last,"No immediate repeat.");selected.Add(a);}
            Check(selected.Count==(last<0?5:4),"Every allowed whale reaction is reachable with equal draw weight.");
        }
        Check(WallyReactions.Sample(WallyAction.BreachJump,.5f,false).Lift>.35f,"Breach must jump.");
        Check(Math.Abs(WallyReactions.Sample(WallyAction.TailSplash,.25f,false).Tail)>.49f,"Tail splash must articulate.");
        Check(WallyReactions.Sample(WallyAction.FlipperWave,.5f,false).Flipper>.69f,"Greeting must wave a flipper.");
        var progress=new CollectionProgress(null,42);progress.Save.Steps=571;progress.Save.Claimed=true;progress.Save.PendingRevealId=ToyCatalog.MoonId;
        var untouched=progress.OwnedCounts.ToDictionary(x=>x.Key,x=>x.Value);
        Check(progress.EnsureWallyTestCopies(),"Wally copies are granted.");
        Check(progress.OwnedCounts.Count(x=>x.Key.StartsWith("wally.")&&x.Value==3)==6,"Six finishes have a display copy and two spares.");
        Check(progress.OwnedCounts.Count(x=>x.Key.StartsWith("wally.")&&x.Value==1)==4,"The four other finishes keep a single test copy.");
        Check(!progress.EnsureWallyTestCopies(),"Grant minimums are idempotent.");
        foreach(var old in untouched)Check(progress.GetOwnedCount(old.Key)==old.Value,"Unrelated inventory remains unchanged.");
        Check(progress.Save.Steps==571&&progress.Save.Claimed&&progress.Save.PendingRevealId==ToyCatalog.MoonId,"Walking and pending reveals survive the grant.");
        Check(!progress.Save.BadgeState!.EarnedCopies.Any(x=>x.Id.StartsWith("wally.")),"Owner test gifts are not earned rewards.");
        progress.Save.Inventory!.Single(x=>x.Id==WallyCatalog.ClassicId).Count=7;
        var restarted=new CollectionProgress(progress.Save,42);Check(!restarted.EnsureWallyTestCopies()&&restarted.GetOwnedCount(WallyCatalog.ClassicId)==7,"Larger existing stacks are preserved across restart.");
        Console.WriteLine("PASS: Wally collection, fifteen finishes, five local whale reactions, Calm and no-repeat random choice.");
    }
}
