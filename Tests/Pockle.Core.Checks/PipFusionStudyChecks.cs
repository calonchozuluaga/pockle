using System;
using System.Collections.Generic;
using System.Linq;
using Pockle.Core;
internal static class PipFusionStudyChecks
{
    private static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    public static void Run()
    {
        Check(ToyCatalog.TryGetCollection("pip",out var regular)&&regular.Members.Count==10,"Original Pip completion changes.");
        Check(ToyCatalog.TryGetCollection(PipFusionStudies.CollectionId,out var lab)&&lab.Members.Count==4&&!lab.HasSecret&&!lab.HasApprovedOdds&&lab.Worlds.Count==5,"Material lab metadata incorrect.");
        foreach(string id in lab.Members)
        {
            Check(PipFusionStudies.Contains(id)&&ToyCatalog.TryGetCollectible(id,out var toy)&&toy.Available&&toy.CharacterId=="pip"&&toy.CollectionId==lab.Id,"Study not playable or in wrong collection.");
            Check(!regular.Members.Contains(id)&&!ToyCatalog.BoxPools.Any(p=>p.ContainsCollectible(id)),"Study entered regular completion or rewards.");
        }
        var progress=new CollectionProgress(new CollectionSave{Version=2,Day=42,Steps=321,PendingRevealId=ToyCatalog.MoonId,Inventory=new List<OwnedCollectible>{new OwnedCollectible(ToyCatalog.PeachId,3),new OwnedCollectible(PipFusionStudies.DuskId,2)}},42);
        Check(progress.EnsurePipFusionStudyCopies(),"Missing Sorbet not granted.");
        Check(progress.GetOwnedCount(PipFusionStudies.DuskId)==2&&progress.GetOwnedCount(PipFusionStudies.SorbetId)==1,"Study grant altered existing copies.");
        Check(progress.GetOwnedCount(PipFusionStudies.HoneydewId)==1&&progress.GetOwnedCount(PipFusionStudies.EclipseId)==1,"Expanded study grant missed new studies.");
        var restored=new CollectionProgress(progress.Save,42);
        Check(!restored.EnsurePipFusionStudyCopies()&&restored.Save.Steps==321&&restored.Save.PendingRevealId==ToyCatalog.MoonId&&restored.GetOwnedCount(ToyCatalog.PeachId)==3,"Restart changed inventory or rewards.");
        var normal=new CollectionProgress(null,42);normal.EnsurePipFinishTestCopies();Check(normal.GetOwnedCount(PipFusionStudies.DuskId)==0&&normal.GetOwnedCount(PipFusionStudies.SorbetId)==0,"Regular grant awarded study copies.");
        Console.WriteLine("PASS: Four fusion studies in separate lab, exact idempotent study grant, preserved save/completion/odds.");
    }
}
