using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;

namespace Pockle.Tests
{
    public sealed class PaidQuoteRuntimeTests
    {
        [TestCase("null")]
        [TestCase("\"\"")]
        [TestCase("missing")]
        public void MixOptionalCollectionMatchesProductAfterUnityDecode(string value)
        {
            var type = typeof(PaidBoxCheckoutService).GetNestedType("Quote", BindingFlags.NonPublic);
            string field = value == "missing" ? "" : ",\"collection_id\":" + value;
            var quote = JsonUtility.FromJson("{\"product_id\":\"pockle.mix.3.v1\",\"box_count\":3" + field + "}", type);
            var collection = (string)type.GetField("collection_id").GetValue(quote);
            Assert.IsTrue(PaidBoxProducts.Find("pockle.mix.3.v1").MatchesCollection(collection),
                "Wire collection_id=" + value + "; Unity decoded " + (collection == null ? "null" : "string length " + collection.Length));
            TestContext.WriteLine("Wire collection_id=" + value + "; Unity decoded " + (collection == null ? "null" : "string length " + collection.Length));
        }

        private static string[] Products => PaidBoxProducts.All.Select(p => p.Id).ToArray();
        private static string Fixture(string product) => File.ReadAllText(Path.Combine(Application.dataPath,"Pockle/Tests/Fixtures/PaidQuotes",product+".json"));
        private static CheckoutQuote Decode(string raw,string product,string price="$2.49") =>
            (CheckoutQuote)typeof(PaidBoxCheckoutService).GetMethod("DecodeQuote",BindingFlags.NonPublic|BindingFlags.Static)
                .Invoke(null,new object[]{raw,product,price});

        [TestCaseSource(nameof(Products))]
        public void ActualSqlQuotesDecodeWithCompleteOddsForEveryProduct(string product)
        {
            var quote=Decode(Fixture(product),product,"2,49 €");
            Assert.IsNotNull(quote,product);
            Assert.IsTrue(quote.Complete);
            Assert.AreEqual("2,49 €",quote.LocalizedPrice,"The server fixture never replaces the actual store price string.");
            var policy=PaidBoxProducts.Find(product);
            Assert.AreEqual(policy.Count,quote.BoxCount);Assert.AreEqual(policy.CollectionId,quote.CollectionId);
            Assert.AreEqual(policy.CollectionId==null?165:15,quote.Odds.Sum(g=>g.Contents.Count));
        }

        [TestCase("pockle.mix.3.v1")]
        [TestCase("pockle.mix.5.v1")]
        public void MixAcceptsAbsentCollectionButRejectsForgedCollectionCountOrProduct(string product)
        {
            string raw=Fixture(product);
            Assert.IsNotNull(Decode(raw.Replace("\"collection_id\": null","\"collection_id\": \"\""),product));
            // The optional top-level field is last in the deterministic SQL fixture.
            Assert.IsNotNull(Decode(raw.Replace(",\n  \"collection_id\": null",""),product));
            Assert.IsNull(Decode(raw.Replace("\"collection_id\": null","\"collection_id\": \"wally\""),product));
            Assert.IsNull(Decode(raw.Replace("\"box_count\": "+PaidBoxProducts.Find(product).Count,"\"box_count\": 1"),product));
            Assert.IsNull(Decode(raw,product=="pockle.mix.3.v1"?"pockle.mix.5.v1":"pockle.mix.3.v1"));
            Assert.IsNull(Decode(raw,product,""));
            Assert.IsNull(Decode(raw.Replace("\"percent\": 88","\"percent\": 100"),product),"Incomplete or inconsistent odds remain unbuyable.");
        }

        [Test]
        public void OmittedMixFieldPassesTheInstalledBuild9IdentityGuard()
        {
            foreach(string product in new[]{"pockle.mix.3.v1","pockle.mix.5.v1"})
            {
                string raw=Fixture(product).Replace(",\n  \"collection_id\": null","");
                var type=typeof(PaidBoxCheckoutService).GetNestedType("Quote",BindingFlags.NonPublic);
                var wire=JsonUtility.FromJson(raw,type);
                string actual=(string)type.GetField("collection_id").GetValue(wire);
                // This is deliberately the old, exact-null check shipped in build 9, before MatchesCollection.
                Assert.IsTrue(actual==PaidBoxProducts.Find(product).CollectionId);
                Assert.IsNotNull(Decode(raw,product),"The compatibility response also keeps complete odds for all 165 possible toys.");
            }
        }

        [Test]
        public void CollectionBoxStillRequiresItsExactCollectionIdentity()
        {
            const string product="pockle.box.wally.v1";
            string raw=Fixture(product);
            // Replace only the final top-level identity, leaving every group and item untouched.
            int at=raw.LastIndexOf("\"collection_id\": \"wally\"",System.StringComparison.Ordinal);
            Assert.Greater(at,0);
            foreach(string value in new[]{"null","\"\"","\"fox\""})
                Assert.IsNull(Decode(raw.Substring(0,at)+raw.Substring(at).Replace("\"collection_id\": \"wally\"","\"collection_id\": "+value),product));
        }
    }
}
