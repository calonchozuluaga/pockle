namespace Pockle.Core
{
    /// <summary>Fixed owner material studies; not a crafting service or production recipe registry.</summary>
    public static class PipFusionStudies
    {
        public const string CollectionId="pip-fusion-lab", DuskId="pip.fusion-dusk", SorbetId="pip.fusion-sorbet", HoneydewId="pip.fusion-honeydew", EclipseId="pip.fusion-eclipse";
        public static readonly System.Collections.Generic.IReadOnlyList<string> Ids=System.Array.AsReadOnly(new[]{DuskId,SorbetId,HoneydewId,EclipseId});
        public static bool Contains(string id)=>id==DuskId||id==SorbetId||id==HoneydewId||id==EclipseId;
    }
}
