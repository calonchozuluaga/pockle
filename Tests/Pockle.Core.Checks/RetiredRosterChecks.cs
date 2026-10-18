using System;
using System.Collections.Generic;
using System.Linq;
using Pockle.Core;
internal static class RetiredRosterChecks
{
    private static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    public static void Run()
    {
        string[] retired={"loop","mallow","moss","nook","ripple","rolo","sprig","wisp","bop","dew"};
        Check(ToyCatalog.Characters.Count==11&&ToyCatalog.Collections.Count==12&&ToyCatalog.Collectibles.Count==114,"Eleven regular collections plus four owner fusion studies.");
        foreach(string id in retired)
        {
            Check(!ToyCatalog.TryGetCharacter(id,out _)&&!ToyCatalog.TryGetCollection(id,out _)&&!CharacterMotion.TryGetProfile(id,out _),"Retired character still active.");
            foreach(var finish in ToyCatalog.Finishes)Check(!ToyCatalog.TryGetCollectible(id+"."+finish.Id,out _),"Retired finish still offered.");
        }
        Check(ToyCatalog.TryGetCollection("tula",out _),"Tula was not requested for removal.");
        var entries=retired.Select(id=>new OwnedCollectible(id+".clear-jelly",3)).ToList();entries.Add(new OwnedCollectible(ToyCatalog.PeachId,9));
        var save=new CollectionSave{Version=2,Day=42,Steps=789,Inventory=entries,PendingRevealId=ToyCatalog.MoonId};
        var progress=new CollectionProgress(save,42);var restored=new CollectionProgress(progress.Save,42);
        foreach(var item in entries)Check(restored.GetOwnedCount(item.Id)==item.Count,"Removing a catalog entry lost saved ownership.");
        Check(restored.Save.Steps==789&&restored.Save.PendingRevealId==ToyCatalog.MoonId,"Roster cleanup changed walking or pending reveal.");
        foreach(var pool in ToyCatalog.BoxPools)foreach(var entry in pool.Entries)Check(ToyCatalog.TryGetCollectible(entry.CollectibleId,out var toy)&&toy.Available,"Retired reward remains in a pool.");
        Console.WriteLine("PASS: exact retired roster, remaining Tula, preserved historical inventory and reward state.");
    }
}
