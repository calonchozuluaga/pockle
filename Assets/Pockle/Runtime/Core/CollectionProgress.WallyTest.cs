namespace Pockle.Core
{
    public sealed partial class CollectionProgress
    {
        /// <summary>One display copy of each Wally; two spares in six contrasting finishes. The session owns the one-time gate.</summary>
        public bool EnsureWallyTestCopies()
        {
            bool changed=false;
            foreach(var toy in ToyCatalog.Collectibles)
            {
                if(toy.CharacterId!="wally"||!toy.Available||toy.Id==WallyCatalog.KelpId||toy.Id==WallyCatalog.LeafId||toy.Id==WallyCatalog.RoseId||toy.Id==WallyCatalog.DiamondId||toy.Id==WallyCatalog.SpecialId)continue;
                bool spares=toy.FinishId=="clear-jelly"||toy.FinishId=="confetti-gel"||toy.FinishId=="boucle-plush"
                    ||toy.FinishId=="gloss-vinyl"||toy.FinishId=="coated-metallic"||toy.FinishId=="glow-jelly";
                int minimum=spares?3:1;
                if(GetOwnedCount(toy.Id)>=minimum)continue;
                counts[toy.Id]=minimum;changed=true;
            }
            if(changed)SynchronizeInventory();
            return changed;
        }
    }
}
