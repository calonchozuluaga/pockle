using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Pockle.Core;
internal static class WalkingDeliveryChecks
{
    private static void Check(bool value,string message){if(!value)throw new Exception(message);}
    public static void Run()
    {
        const int day=20000;
        var inventory=ToyCatalog.Collectibles.Where(x=>x.Available).Take(87).Select((x,i)=>new OwnedCollectible(x.Id,i+1)).ToList();
        var fusions=Enumerable.Range(0,13).Select(i=>new FusedToy{Id="fusion.walking-migration-"+i,
            First=ToyCatalog.PeachId,Second=ToyCatalog.MoonId,Name="Saved fusion",Material="Mixed",Personality="Calm",
            Temperament=7,Energy=40,Mood=70,Boldness=45,Bedtime=40}).ToList();
        string paid=Guid.NewGuid().ToString("D");
        var old=new CollectionSave{Version=5,Day=day,Inventory=inventory,Fusions=fusions,PendingFusionId=fusions[0].Id,
            PaidDeliveryIds=new List<string>{paid},Steps=700,SensorTotal=75000,SensorUptime=2000,SensorBoot=2};
        var before=inventory.ToDictionary(x=>x.Id,x=>x.Count);
        var progress=new CollectionProgress(old,day);
        Check(progress.Save.Version==6&&progress.Save.Fusions!.Count==13&&progress.Save.PendingFusionId==fusions[0].Id,"v6 migration lost saved fusion data.");
        foreach(var pair in before)Check(progress.GetOwnedCount(pair.Key)==pair.Value,"v6 migration changed existing inventory.");
        Check(progress.Save.PaidDeliveryIds!.Contains(paid)&&progress.Save.Steps==700&&progress.Save.SensorTotal==75000,"v6 migration lost receipts or steps.");
        string fox=ToyCatalog.Collectibles.First(x=>x.CollectionId=="fox"&&x.Available).Id;
        int starting=progress.GetOwnedCount(fox);string receipt=Guid.NewGuid().ToString("D");
        Check(!progress.ApplyWalkingDelivery(receipt,"pip",fox,day),"Walking delivery accepted wrong collection.");
        Check(!progress.ApplyWalkingDelivery("invalid","fox",fox,day),"Walking delivery accepted invalid receipt.");
        Check(!progress.ApplyWalkingDelivery(receipt,"fox","missing.toy",day),"Walking delivery accepted missing toy.");
        Check(progress.ApplyWalkingDelivery(receipt,"fox",fox,day)&&progress.GetOwnedCount(fox)==starting+1,"Walking receipt did not grant one copy.");
        Check(progress.ApplyWalkingDelivery(receipt.ToUpperInvariant(),"fox",fox,day)&&progress.GetOwnedCount(fox)==starting+1,"Receipt replay granted again.");
        var json=new JsonSerializerOptions{IncludeFields=true};
        progress=new CollectionProgress(JsonSerializer.Deserialize<CollectionSave>(JsonSerializer.Serialize(progress.Save,json),json),day);
        Check(progress.ApplyWalkingDelivery(receipt,"fox",fox,day)&&progress.GetOwnedCount(fox)==starting+1,"Restart replay granted again.");
        Check(progress.WalkingBoxDays.Count==1&&progress.WalkingBoxDays[0]==day,"Walking badge day repeated or missing.");
        Check(progress.BadgeEligibleOwnedCounts[fox]==1,"Walking earned-copy provenance missing.");
        foreach(var c in ToyCatalog.Collections.Where(x=>x.CharacterId==x.Id))
        {
            var toy=ToyCatalog.Collectibles.First(x=>x.CollectionId==c.Id&&x.Available);
            Check(progress.ApplyWalkingDelivery(Guid.NewGuid().ToString("D"),c.Id,toy.Id,day),"A non-Pip collection was rejected.");
        }
        FusionCatalog.Restore(null);
        Console.WriteLine("PASS: walking receipts across11collections, v5/v6 inventory and13fusion preservation, restart/replay, badge provenance and invalid delivery rejection.");
    }
}
