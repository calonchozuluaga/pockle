using System;
using System.Linq;
using System.Text.Json;
using Pockle.Core;

// Export the actual engine-free client catalog; a second hand-maintained fixture
// would miss the server/client drift that rejected every non-Wally quote.
var collections = PaidBoxProducts.All.Where(p => p.CollectionId != null)
    .Select(p => p.CollectionId).ToHashSet();
Console.WriteLine(JsonSerializer.Serialize(new
{
    products = PaidBoxProducts.All.Select(p => p.Id).OrderBy(id => id),
    finishes = ToyCatalog.Collectibles.Where(t => t.Available && collections.Contains(t.CollectionId))
        .OrderBy(t => t.Id).Select(t => new { id = t.Id, collection_id = t.CollectionId, tier = (int)t.Tier })
}));
