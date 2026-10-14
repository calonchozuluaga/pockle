using System.Collections;
using System.Linq;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class AxolotlOwnerGrantTests
    {
        [UnityTest]
        public IEnumerator GrantIsDurableBeforeRefreshAndDoesNotRepeat()
        {
            const string saveKey="pockle.collection.v1";
            bool hadSave=PlayerPrefs.HasKey(saveKey),hadMarker=PlayerPrefs.HasKey(CollectionSession.AxolotlFinishesGrantKey);
            string oldSave=PlayerPrefs.GetString(saveKey);int marker=PlayerPrefs.GetInt(CollectionSession.AxolotlFinishesGrantKey);
            var root=new GameObject("Axolotl owner grant regression");
            try
            {
                PlayerPrefs.DeleteKey(CollectionSession.AxolotlFinishesGrantKey);
                PlayerPrefs.SetString(saveKey,JsonUtility.ToJson(new CollectionSave {Version=CollectionProgress.SaveVersion,Day=CollectionSession.Today,Steps=521,
                    PendingRevealId=ToyCatalog.MoonId,Inventory=new System.Collections.Generic.List<OwnedCollectible>{new OwnedCollectible(ToyCatalog.PeachId,3)}}));
                var session=root.AddComponent<CollectionSession>();session.Initialize();bool refreshed=false;
                session.Changed+=()=>{
                    var durable=new CollectionProgress(JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(saveKey)),CollectionSession.Today);
                    Assert.That(durable.GetOwnedCount(ToyCatalog.AxolotlSpecialId),Is.EqualTo(1));refreshed=true;
                };
                Assert.That(session.TryApplyOwnerAxolotlFinishesGrant(),Is.True);Assert.That(refreshed,Is.True);
                Assert.That(session.TryApplyOwnerAxolotlFinishesGrant(),Is.False);
                var restored=new CollectionProgress(JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(saveKey)),CollectionSession.Today);
                foreach(var toy in ToyCatalog.Collectibles.Where(c=>c.CharacterId=="axolotl"))
                    Assert.That(restored.GetOwnedCount(toy.Id),Is.EqualTo(toy.Id==ToyCatalog.PeachId?3:1));
                Assert.That(restored.Save.Steps,Is.EqualTo(521));Assert.That(restored.Save.PendingRevealId,Is.EqualTo(ToyCatalog.MoonId));
                Assert.That(PlayerPrefs.GetInt(CollectionSession.AxolotlFinishesGrantKey),Is.EqualTo(1));
            }
            finally
            {
                Object.Destroy(root);
                if(hadSave)PlayerPrefs.SetString(saveKey,oldSave);else PlayerPrefs.DeleteKey(saveKey);
                if(hadMarker)PlayerPrefs.SetInt(CollectionSession.AxolotlFinishesGrantKey,marker);else PlayerPrefs.DeleteKey(CollectionSession.AxolotlFinishesGrantKey);
                PlayerPrefs.Save();
            }
            yield return null;
        }
    }
}
