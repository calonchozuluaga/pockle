using System;
using System.Linq;
using Pockle.Core;
internal static class WinterTrioChecks
{
    private static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    public static void Run()
    {
        foreach(string id in new[]{"reiny","guin","bundy"})
        {
            Check(ToyCatalog.TryGetCollection(id,out var collection)&&collection.Members.Count==10,"Winter trio collection must have ten finishes.");
            Check(collection.Members.Count(m=>ToyCatalog.TryGetCollectible(m,out var toy)&&toy.Available&&toy.IsSecret)==1,"Exactly one playable rare required.");
            Check(collection.CoverArt=="Collections/"+HolidayCatalog.Title(id)+"/Cover"&&collection.BoxArt=="UI/Boxes/Collection-"+id&&collection.BoxModel=="Boxes/"+HolidayCatalog.Title(id)+"CollectionBox"&&collection.Worlds.Count==5,"Winter trio requires its own five worlds, cover and opening carton.");
            Check(collection.Members.All(m=>!ToyCatalog.IsDailyEligible(m)),"New art must not silently alter walking odds.");
            foreach(WinterTrioAction action in Enum.GetValues(typeof(WinterTrioAction)))
            {
                Check(WinterTrioReactions.Name(id,action).Length>0,"Missing signature action name.");
                foreach(float time in new[]{-1,0,.01f,.25f,.5f,.77f,.99f,1,2,float.NaN,float.PositiveInfinity})
                {
                    var p=WinterTrioReactions.Sample(id,action,time,false);var calm=WinterTrioReactions.Sample(id,action,time,true);
                    foreach(float v in new[]{p.Lift,p.Pitch,p.Yaw,p.Roll,p.Wave,p.Feature,p.Nod,p.Face.Blink,p.Face.Happy,p.Face.Surprise})Check(!float.IsNaN(v)&&!float.IsInfinity(v)&&Math.Abs(v)<=15,"Winter pose escaped finite bounds.");
                    Check(p.Lift>=0&&p.Lift<=.27f&&p.Face.Blink>=0&&p.Face.Blink<=1,"Winter pose bounds.");
                    Check(calm.Lift==0&&calm.Pitch==0&&calm.Yaw==0&&calm.Roll==0&&calm.Wave==0&&calm.Feature==0&&calm.Nod==0,"Calm must neutralize body motion.");
                    if(time<=0||time>=1||float.IsNaN(time))Check(p.Lift==0&&p.Feature==0&&p.Face.Happy==0,"Action must return to rest.");
                }
            }
        }
        for(int previous=-1;previous<5;previous++)
        {
            var selected=Enumerable.Range(0,previous<0?5:4).Select(i=>(int)WinterTrioReactions.Choose(previous,i)).ToArray();
            Check(selected.Distinct().Count()==selected.Length&&!selected.Contains(previous),"No-repeat selection lost actions.");
        }
        var progress=new CollectionProgress(null,42);progress.Save.Steps=571;progress.Save.PendingRevealId=ToyCatalog.MoonId;
        Check(progress.EnsureWinterTrioTestCopies(),"Initial review grant missing.");
        Check(!progress.EnsureWinterTrioTestCopies(),"Review grant duplicated.");
        var restored=new CollectionProgress(progress.Save,42);
        foreach(var toy in ToyCatalog.Collectibles.Where(t=>HolidayCatalog.IsWinterTrio(t.CharacterId)))Check(restored.GetOwnedCount(toy.Id)==1,"Grant did not persist all thirty toys.");
        Check(restored.Save.Steps==571&&restored.Save.PendingRevealId==ToyCatalog.MoonId&&restored.GetOwnedCount(ToyCatalog.PeachId)==1,"Grant reset prior state.");
        Console.WriteLine("PASS: three winter collections, 30 finishes, three rares, 15 actions, no-repeat/Calm and idempotent saved review grant.");
    }
}
