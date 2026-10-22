using System;
using System.Linq;
using Pockle.Core;
int position=0;
foreach(var collection in ToyCatalog.Collections.Where(c=>c.Id!=PipFusionStudies.CollectionId))
{
 var ids=ToyCatalog.Collectibles.Where(t=>t.CollectionId==collection.Id&&t.Available).Select(t=>"'"+t.Id.Replace("'","''")+"'");
 Console.WriteLine("('"+collection.Id+"',"+(++position)+",array["+string.Join(",",ids)+"]),");
}
