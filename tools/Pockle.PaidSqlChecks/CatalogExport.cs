using System;
using System.Linq;
using System.Text.Json;
using Pockle.Core;

// Export the actual engine-free client catalog; a second hand-maintained fixture
// would miss the server/client drift that rejected every non-Wally quote.
var collections = PaidBoxProducts.All.Where(p => p.CollectionId != null)
    .Select(p => p.CollectionId).ToHashSet();
foreach (var product in PaidBoxProducts.All)
{
    if (!product.MatchesCollection(product.CollectionId) || product.MatchesCollection("unrelated"))
        throw new Exception("Collection quote identity guard failed: " + product.Id);
    if (product.MatchesCollection(null) != (product.CollectionId == null)
        || product.MatchesCollection("") != (product.CollectionId == null))
        throw new Exception("Only Mystery Mix can accept an absent collection: " + product.Id);
}
Console.WriteLine(JsonSerializer.Serialize(new
{
    products = PaidBoxProducts.All.Select(p => p.Id).OrderBy(id => id),
    finishes = ToyCatalog.Collectibles.Where(t => t.Available && collections.Contains(t.CollectionId))
        .OrderBy(t => t.Id).Select(t => new { id = t.Id, collection_id = t.CollectionId, tier = (int)t.Tier })
}));
