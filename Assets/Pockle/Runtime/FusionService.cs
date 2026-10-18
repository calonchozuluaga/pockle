using System;
using Pockle.Core;

namespace Pockle.Runtime
{
    /// <summary>
    /// Fusion (docs/FUSION.md): consumes two spares and grants one keep-forever fused toy. Inventory, saves, the ledger and
    /// fused-toy art are Codex's lane; the HUD only calls this contract. A real service must persist the result before the
    /// reveal, never use a player's last copy, and (once accounts exist) be validated by the server.
    /// </summary>
    public interface IFusionService
    {
        bool Available { get; }
        /// <summary>Attempts the fusion. Returns false, and changes nothing, when it can't happen.</summary>
        bool TryFuse(string firstSpareId, string secondSpareId);
    }

    /// <summary>Local beta transaction: the session saves consumption and result together before revealing either.</summary>
    public sealed class LocalFusionService : IFusionService
    {
        private readonly CollectionSession session;
        public LocalFusionService(CollectionSession collection) { session = collection; }
        public bool Available => session != null && session.Progress != null && !session.IsReadOnlySave;
        public bool TryFuse(string first, string second)
        {
            if (!Available || !FusionCatalog.IsSource(first) || !FusionCatalog.IsSource(second)) return false;
            if (string.CompareOrdinal(first, second) > 0) { string swap = first; first = second; second = swap; }
            ToyCatalog.TryGetCollectible(first, out var a); ToyCatalog.TryGetCollectible(second, out var b);
            if (a.CharacterId != b.CharacterId || !CharacterArt.TryLoad(first, out _) || !CharacterArt.TryLoad(second, out _)) return false;
            var outcome = PersonalityBook.DescribeFusion(first, second);
            if (outcome == null) return false;
            var dials = PersonalityBook.PreviewFusedDials(first, second);
            var hybrid = PersonalityBook.HybridOf(PersonalityBook.TemperamentOf(first), PersonalityBook.TemperamentOf(second));
            var result = new FusedToy {
                Id = "fusion." + a.CharacterId + "-" + Guid.NewGuid().ToString("N"), First = first, Second = second,
                Name = outcome.Name, Material = outcome.Material, Personality = outcome.Personality, PersonalityLine = outcome.PersonalityLine,
                Energy = dials.Energy, Mood = dials.Mood, Boldness = dials.Boldness, Bedtime = dials.Bedtime,
                Temperament = (int)PersonalityBook.Nearest(dials), HybridId = hybrid != null ? hybrid.Id : ""
            };
            return session.CommitFusion(result);
        }
    }

    /// <summary>Until the runtime exists, fusing shows a preview of the result and uses nothing.</summary>
    public sealed class PreviewOnlyFusion : IFusionService
    {
        public bool Available => false;
        public bool TryFuse(string firstSpareId, string secondSpareId) => false;
    }
}
