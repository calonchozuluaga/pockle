using System;
using System.Collections;
using System.Collections.Generic;
using Pockle.Core;
using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>Store prices and payment UI only. Every award is recovered from the authenticated server ledger.</summary>
    public sealed class PaidBoxCheckoutService : MonoBehaviour
    {
        [Serializable] private sealed class Config { public string androidPublicKey="",applePublicKey=""; }
        [Serializable] private sealed class Status { public bool available; public string environment=""; }
        [Serializable] private sealed class ProductPayload { public string product_id; }
        [Serializable] private sealed class Row { public int tier; public double percent; }
        [Serializable] private sealed class Content { public string collectible_id; public int tier; public double percent,within_collection_percent; }
        [Serializable] private sealed class Group { public string collection_id; public double collection_percent; public Row[] rows; public Content[] contents; }
        [Serializable] private sealed class Quote { public string quote_id,product_id,collection_id; public int box_count; public Group[] odds; public string[] notes; }
        [Serializable] private sealed class Item { public string collectible_id,collection_id; public int tier; public bool is_new; }
        [Serializable] private sealed class Delivery { public string receipt_id,product_id; public Item[] items; public int revealed; public bool finished; }
        [Serializable] private sealed class Recovery { public Delivery[] deliveries; public int pending; }
        [Serializable] private sealed class ProgressPayload { public string receipt_id; public int revealed; public bool finished; }
        [Serializable] private sealed class Pending { public string state,product; public List<string> before; }
        private PrototypeHud hud;
        private CollectionSession collection;
        private SupabaseAccountService account;
        private Purchases purchases;
        private Config config;
        private string user,configuredUser,quoteId,quoteProduct;
        private bool working,storeOpen,stopped;
        private float retryAt;
        private readonly Dictionary<string,Purchases.Package> packages = new Dictionary<string,Purchases.Package>();
        private readonly Dictionary<string,Delivery> deliveries = new Dictionary<string,Delivery>();
        private string PendingKey => "pockle.checkout.pending.v1." + user;
        private string ProgressKey(string receipt) => "pockle.checkout.reveal.v1." + user + "." + receipt;

        public void Initialize(PrototypeHud view,CollectionSession session,SupabaseAccountService accounts)
        {
            hud=view;collection=session;account=accounts;
            var asset=Resources.Load<TextAsset>("Backend/PocklePurchases");
            if(asset!=null)config=Parse<Config>(asset.text);
            hud.CheckoutQuoteRequested+=RequestQuote;
            hud.CheckoutConfirmed+=Confirm;
            hud.CheckoutRecoveryRequested+=RecoverRequested;
            hud.BoxRevealProgress+=RevealProgress;
            hud.BoxRevealFinished+=RevealFinished;
            // Until both the store and server are ready, preserve Claude's non-purchasable launch preview.
            hud.SetCheckoutCatalog(null);
        }
        private void Update()
        {
            if(hud==null||stopped)return;
            string current=account.UserId;
            if(current!=user)
            {
                user=current;quoteId=null;quoteProduct=null;packages.Clear();deliveries.Clear();
                hud.SetCheckoutCatalog(null);hud.SetPendingDeliveries(0);retryAt=0;
            }
            if(Application.isEditor||string.IsNullOrEmpty(user)||working||storeOpen||Time.unscaledTime<retryAt)return;
            retryAt=Time.unscaledTime+30;StartCoroutine(Refresh());
        }
        private IEnumerator Refresh()
        {
            working=true;string owner=user;string raw=null;bool ok=false;
            yield return Request("status","{}",(r,s)=>{raw=r;ok=s;});
            var status=ok?Parse<Status>(raw):null;
            if(owner!=account.UserId){working=false;yield break;}
            if(status==null||!status.available||status.environment!="SANDBOX")
            {hud.SetCheckoutCatalog(null);working=false;yield break;}
            string prefix=Application.platform==RuntimePlatform.Android?"goog_":Application.platform==RuntimePlatform.IPhonePlayer?"appl_":"";
            string key=Application.platform==RuntimePlatform.Android?config?.androidPublicKey:Application.platform==RuntimePlatform.IPhonePlayer?config?.applePublicKey:null;
            if(prefix.Length==0||string.IsNullOrEmpty(key)||!key.StartsWith(prefix,StringComparison.Ordinal))
            {hud.SetCheckoutCatalog(null);working=false;yield break;}
            if(purchases==null)
            {
                purchases=gameObject.AddComponent<Purchases>();purchases.useRuntimeSetup=true;
                yield return null; // Purchases.Start initializes its native bridge before Configure.
                if(owner!=account.UserId){working=false;yield break;}
                purchases.Configure(Purchases.PurchasesConfiguration.Builder.Init(key).SetAppUserId(owner)
                    .SetDiagnosticsEnabled(false).SetAutomaticDeviceIdentifierCollectionEnabled(false).Build());
                configuredUser=owner;
            }
            if(configuredUser!=owner)
            {
                bool completed=false,identified=false;
                purchases.LogIn(owner,(info,created,error)=>{identified=error==null;completed=true;});
                yield return Await(()=>completed);
                if(!completed||!identified||owner!=account.UserId){working=false;yield break;}
                configuredUser=owner;
            }
            bool loaded=false;Purchases.Offerings offering=null;
            purchases.GetOfferings((o,e)=>{if(e==null)offering=o;loaded=true;});
            yield return Await(()=>loaded);
            if(owner!=account.UserId){working=false;yield break;}
            packages.Clear();
            if(offering!=null&&offering.All.TryGetValue(PaidBoxProducts.Offering,out var launch))
                foreach(var package in launch.AvailablePackages)
                {
                    var policy=PaidBoxProducts.Find(package.StoreProduct.Identifier);
                    if(policy!=null&&policy.Package==package.Identifier)packages[policy.Id]=package;
                }
            yield return Recover(owner);
            if(owner==account.UserId)BindCatalog();
            working=false;
        }
        private void BindCatalog()
        {
            var offers=new List<CheckoutOffer>();
            bool pending=PlayerPrefs.HasKey(PendingKey);
            foreach(var policy in PaidBoxProducts.All)
            {
                bool found=packages.TryGetValue(policy.Id,out var package);
                offers.Add(new CheckoutOffer{OfferId=policy.Id,CollectionId=policy.CollectionId,BoxCount=policy.Count,
                    Kind=policy.CollectionId==null?CheckoutOfferKind.MysteryMix:CheckoutOfferKind.CollectionBox,
                    LocalizedPrice=found?package.StoreProduct.PriceString:null,Available=found&&!pending,
                    ComingSoon=!found,UnavailableReason=pending?"Checking your previous purchase":"Not available in this store yet"});
            }
            hud.SetCheckoutCatalog(new CheckoutCatalog{Status=CheckoutCatalogStatus.Ready,Offers=offers,Message="Internal purchase test"});
        }
        private void RequestQuote(string offer,string collectionId)
        {
            var product=PaidBoxProducts.Find(offer);
            if(working||storeOpen||product==null||!product.MatchesCollection(collectionId)||!packages.ContainsKey(offer)||PlayerPrefs.HasKey(PendingKey))
            {hud.SetCheckoutState(null,CheckoutState.Failed);return;}
            StartCoroutine(LoadQuote(offer));
        }
        private IEnumerator LoadQuote(string product)
        {
            working=true;string owner=user;quoteId=null;quoteProduct=null;string raw=null;bool ok=false;
            yield return Request("quote",JsonUtility.ToJson(new ProductPayload{product_id=product}),(r,s)=>{raw=r;ok=s;});
            var view=ok&&owner==account.UserId&&packages.TryGetValue(product,out var package)?DecodeQuote(raw,product,package.StoreProduct.PriceString):null;
            if(owner!=account.UserId||view==null)
            {hud.SetCheckoutState(null,CheckoutState.Failed);working=false;yield break;}
            quoteId=view.QuoteId;quoteProduct=product;hud.ShowCheckoutQuote(view);
            working=false;
        }

        // Keep wire decoding/validation independently testable with actual SQL quote fixtures.
        // Store prices remain local and a valid quote never grants inventory or calls the store.
        internal static CheckoutQuote DecodeQuote(string raw,string product,string price)
        {
            var q=Parse<Quote>(raw);
            var policy=PaidBoxProducts.Find(product);
            if(policy==null||q==null||q.product_id!=product||q.box_count!=policy.Count||!policy.MatchesCollection(q.collection_id)||q.odds==null)return null;
            var groups=new List<CheckoutOddsGroup>();
            foreach(var group in q.odds)
            {
                if(group==null)return null;
                var rows=new List<CheckoutOddsRow>();
                if(group.rows!=null)foreach(var row in group.rows)
                {if(row==null)return null;rows.Add(new CheckoutOddsRow{Tier=RarityLook.FromTier((ToyTier)row.tier),Percent=row.percent});}
                var contents=new List<CheckoutContentsItem>();
                if(group.contents!=null)foreach(var item in group.contents)
                {if(item==null)return null;contents.Add(new CheckoutContentsItem{CollectibleId=item.collectible_id,
                    Tier=RarityLook.FromTier((ToyTier)item.tier),Percent=item.percent,WithinCollectionPercent=item.within_collection_percent});}
                groups.Add(new CheckoutOddsGroup{CollectionId=group.collection_id,CollectionPercent=policy.CollectionId==null?(double?)group.collection_percent:null,Rows=rows,Contents=contents});
            }
            var view=new CheckoutQuote{QuoteId=q.quote_id,OfferId=product,CollectionId=policy.CollectionId,BoxCount=q.box_count,
                Kind=policy.CollectionId==null?CheckoutOfferKind.MysteryMix:CheckoutOfferKind.CollectionBox,
                LocalizedPrice=price,Odds=groups,Notes=q.notes};
            return view.Complete?view:null;
        }
        private void Confirm(string id)
        {
            if(working||storeOpen||id!=quoteId||user!=account.UserId||!packages.TryGetValue(quoteProduct??"",out var package))return;
            storeOpen=true;
            var pending=new Pending{state="waiting",product=quoteProduct,before=new List<string>(collection.Progress.Save.PaidDeliveryIds??new List<string>())};
            PlayerPrefs.SetString(PendingKey,JsonUtility.ToJson(pending));PlayerPrefs.Save();
            string owner=user;hud.SetCheckoutState(id,CheckoutState.WaitingForStore);
            purchases.PurchasePackage(package,result=>
            {
                storeOpen=false;
                if(owner!=account.UserId)return; // Recover only under the original authenticated account.
                if(result.UserCancelled){PlayerPrefs.DeleteKey(PendingKey);PlayerPrefs.Save();hud.SetCheckoutState(id,CheckoutState.Cancelled);BindCatalog();return;}
                // A store callback is never permission to grant. Even an error can follow a completed payment.
                pending.state=result.Error==null?"paid":"uncertain";
                PlayerPrefs.SetString(PendingKey,JsonUtility.ToJson(pending));PlayerPrefs.Save();
                hud.SetCheckoutState(id,result.Error==null?CheckoutState.DeliveryPending:CheckoutState.PaymentPending);
                if(result.Error==null)hud.SetPendingDeliveries(PaidBoxProducts.Find(quoteProduct)?.Count??1);
                retryAt=0;
            });
        }
        private void RecoverRequested(){if(!working&&!storeOpen&&!string.IsNullOrEmpty(user)){retryAt=0;}}
        private IEnumerator Recover(string owner)
        {
            string raw=null;bool ok=false;
            yield return Request("recover","{}",(r,s)=>{raw=r;ok=s;});
            var result=ok?Parse<Recovery>(raw):null;
            if(owner!=account.UserId||result?.deliveries==null)yield break;
            bool gotNew=false;
            var pending=Parse<Pending>(PlayerPrefs.GetString(PendingKey,""));
            foreach(var d in result.deliveries)
            {
                if(d.items==null)continue;
                var ids=new List<string>();foreach(var i in d.items)ids.Add(i.collectible_id);
                if(!collection.CommitPaidDelivery(d.receipt_id,d.product_id,ids))continue;
                gotNew|=pending!=null&&pending.product==d.product_id&&pending.before!=null&&!pending.before.Contains(d.receipt_id);
                deliveries[d.receipt_id]=d;
            }
            if(gotNew){PlayerPrefs.DeleteKey(PendingKey);PlayerPrefs.Save();if(quoteId!=null)hud.SetCheckoutState(quoteId,CheckoutState.Delivered);}
            hud.SetPendingDeliveries(result.pending+(!gotNew&&pending?.state=="paid"?1:0));
            foreach(var d in deliveries.Values)
            {
                int local=PlayerPrefs.GetInt(ProgressKey(d.receipt_id),0);
                if(local>d.revealed||local<0)
                    yield return SaveProgress(d.receipt_id,local<0?d.items.Length:local,local<0);
                if(d.finished||local<0||hud.BoxRevealVisible)continue;
                var policy=PaidBoxProducts.Find(d.product_id);var items=new List<BoxRevealItem>();
                foreach(var i in d.items)items.Add(new BoxRevealItem{CollectibleId=i.collectible_id,CollectionId=i.collection_id,IsNew=i.is_new,Tier=RarityLook.FromTier((ToyTier)i.tier)});
                hud.ShowBoxReveal(new BoxRevealView{ReceiptId=d.receipt_id,Source=policy.CollectionId==null?BoxRevealSource.MysteryMix:BoxRevealSource.CollectionBox,
                    StartAt=Math.Max(d.revealed,Math.Max(0,local)),Items=items});
                break;
            }
        }
        private void RevealProgress(string receipt,int count)
        {
            if(!deliveries.ContainsKey(receipt))return;
            PlayerPrefs.SetInt(ProgressKey(receipt),count);PlayerPrefs.Save();
            StartCoroutine(SaveProgress(receipt,count,false));
        }
        private void RevealFinished(string receipt)
        {
            if(!deliveries.TryGetValue(receipt,out var delivery))return;
            PlayerPrefs.SetInt(ProgressKey(receipt),-1);PlayerPrefs.Save();
            StartCoroutine(SaveProgress(receipt,delivery.items.Length,true));retryAt=0;
        }
        private IEnumerator SaveProgress(string receipt,int count,bool finished)
        {
            yield return Request("progress",JsonUtility.ToJson(new ProgressPayload{receipt_id=receipt,revealed=count,finished=finished}),(r,s)=>{});
        }
        private IEnumerator Request(string action,string payload,Action<string,bool> done)
        {yield return account.AuthorizedRequest("pockle-paid-boxes","{\"action\":\""+action+"\",\"payload\":"+payload+"}",done);}
        private static IEnumerator Await(Func<bool> done){float end=Time.realtimeSinceStartup+30;while(!done()&&Time.realtimeSinceStartup<end)yield return null;}
        private static T Parse<T>(string raw) where T:class {try{return JsonUtility.FromJson<T>(raw);}catch{return null;}}
        private void OnDestroy()
        {
            stopped=true;if(hud==null)return;
            hud.CheckoutQuoteRequested-=RequestQuote;hud.CheckoutConfirmed-=Confirm;hud.CheckoutRecoveryRequested-=RecoverRequested;
            hud.BoxRevealProgress-=RevealProgress;hud.BoxRevealFinished-=RevealFinished;
        }
    }
}
