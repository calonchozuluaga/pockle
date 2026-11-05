#nullable enable
using System;
using System.Collections.Generic;

namespace Pockle.Core
{
    /// <summary>Immutable approved store policy; price and draw authority stay outside the client.</summary>
    public sealed class PaidBoxProduct
    {
        public string Id { get; }
        public string Package { get; }
        public string? CollectionId { get; }
        public int Count { get; }
        internal PaidBoxProduct(string id, string package, string? collection, int count)
        { Id = id; Package = package; CollectionId = collection; Count = count; }
    }

    public static class PaidBoxProducts
    {
        public const string Offering = "launch_boxes_v1";
        public static readonly IReadOnlyList<PaidBoxProduct> All = Build().AsReadOnly();

        private static List<PaidBoxProduct> Build()
        {
            var result = new List<PaidBoxProduct>();
            foreach (string name in new[] { "pip", "fox", "axolotl", "firb", "coco", "snowy", "ginger", "reiny", "guin", "bundy", "wally" })
                result.Add(new PaidBoxProduct("pockle.box." + name + ".v1", "collection_" + name, name, 1));
            result.Add(new PaidBoxProduct("pockle.mix.3.v1", "mix_3", null, 3));
            result.Add(new PaidBoxProduct("pockle.mix.5.v1", "mix_5", null, 5));
            return result;
        }

        public static PaidBoxProduct? Find(string? id)
        {
            foreach (var item in All) if (string.Equals(item.Id, id, StringComparison.Ordinal)) return item;
            return null;
        }
    }
}
