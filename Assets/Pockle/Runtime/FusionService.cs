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

    /// <summary>Until the runtime exists, fusing shows a preview of the result and uses nothing.</summary>
    public sealed class PreviewOnlyFusion : IFusionService
    {
        public bool Available => false;
        public bool TryFuse(string firstSpareId, string secondSpareId) => false;
    }
}
