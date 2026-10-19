using System;
using System.Linq;
using Pockle.Core;
internal static class WallyChecks
{
    private static void Check(bool value,string message){if(!value)throw new Exception(message);}
    public static void Run()
    {
        Check(ToyCatalog.TryGetCollection("wally",out var c)&&c.DisplayName=="Wally"&&c.Members.Count==10&&c.SecretId==WallyCatalog.SpecialId,"Wally needs ten own finishes and one secret.");
        Check(c.Members.All(id=>ToyCatalog.TryGetCollectible(id,out var t)&&t.Available&&t.CharacterId=="wally"&&!ToyCatalog.IsDailyEligible(id)),"Wally art cannot change rewards.");
        Check(ToyCatalog.TryGetCharacter("wally",out var ch)&&ch.LeadFinishId=="gloss-vinyl","Wally should lead with the approved blue whale.");
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
        Console.WriteLine("PASS: Wally collection, ten finishes, five local whale reactions, Calm and no-repeat random choice.");
    }
}
