using System;
using System.Collections.Generic;
using System.Text.Json;
using Pockle.Core;
internal static class PaidDeliveryChecks
{
    private static void Assert(bool condition,string message){if(!condition)throw new Exception(message);}
    public static void Run()
    {
        var ids=new HashSet<string>();var packages=new HashSet<string>();
        Assert(PaidBoxProducts.All.Count==13,"Thirteen approved products required.");
        foreach(var p in PaidBoxProducts.All){Assert(ids.Add(p.Id)&&packages.Add(p.Package),"Duplicate product or package.");Assert(p.Count==(p.CollectionId==null?p.Id.Contains(".3.")?3:5:1),"Product grant count changed.");}
        var original=new CollectionSave{Version=4,Day=123,Inventory=new List<OwnedCollectible>{new OwnedCollectible(ToyCatalog.PeachId,3),new OwnedCollectible(ToyCatalog.MoonId,2)}};
        var progress=new CollectionProgress(original,123);
        Assert(progress.Save.Version==CollectionProgress.SaveVersion&&progress.GetOwnedCount(ToyCatalog.PeachId)==3,"v4 inventory migration lost ownership.");
        string receipt=Guid.NewGuid().ToString();
        Assert(progress.ApplyPaidDelivery(receipt,"pockle.box.pip.v1",new[]{ToyCatalog.PeachId}),"Valid delivery rejected.");
        Assert(progress.GetOwnedCount(ToyCatalog.PeachId)==4,"Single purchase must add one copy.");
        Assert(progress.ApplyPaidDelivery(receipt,"pockle.box.pip.v1",new[]{ToyCatalog.PeachId})&&progress.GetOwnedCount(ToyCatalog.PeachId)==4,"Replay granted twice.");
        string fox="";foreach(var c in ToyCatalog.Collectibles)if(c.CollectionId=="fox"&&c.Available){fox=c.Id;break;}
        Assert(!progress.ApplyPaidDelivery(Guid.NewGuid().ToString(),"pockle.box.pip.v1",new[]{fox}),"Wrong collection awarded.");
        Assert(!progress.ApplyPaidDelivery(Guid.NewGuid().ToString(),"pockle.mix.5.v1",new[]{ToyCatalog.PeachId}),"Pack underdelivery accepted.");
        Assert(!progress.ApplyPaidDelivery("not-receipt","pockle.box.pip.v1",new[]{ToyCatalog.PeachId}),"Invalid receipt accepted.");
        Assert(!progress.ApplyPaidDelivery(Guid.NewGuid().ToString(),"pockle.mix.3.v1",new[]{ToyCatalog.PeachId,"unknown",fox}),"Partial invalid pack accepted.");
        Assert(progress.GetOwnedCount(ToyCatalog.PeachId)==4,"Rejected pack partially granted.");
        var pack=new[]{ToyCatalog.PeachId,ToyCatalog.PeachId,fox};string packReceipt=Guid.NewGuid().ToString();
        Assert(progress.ApplyPaidDelivery(packReceipt,"pockle.mix.3.v1",pack),"Mixed collection pack rejected.");
        Assert(progress.GetOwnedCount(ToyCatalog.PeachId)==6&&progress.GetOwnedCount(fox)==1,"Pack quantities lost.");
        var options=new JsonSerializerOptions{IncludeFields=true};
        var restored=new CollectionProgress(JsonSerializer.Deserialize<CollectionSave>(JsonSerializer.Serialize(progress.Save,options),options),123);
        Assert(restored.ApplyPaidDelivery(packReceipt,"pockle.mix.3.v1",pack)&&restored.GetOwnedCount(ToyCatalog.PeachId)==6,"Restart replay granted twice.");
        Assert(restored.GetOwnedCount(ToyCatalog.MoonId)==2,"Existing inventory changed.");
        Console.WriteLine("PASS: paid product policies, v4 migration, atomic packs, replay and restart preservation.");
    }
}
