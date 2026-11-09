using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Pockle.Core;
using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>Reports sensor counters and presents saved server rewards. No local draw or local-clock earning.</summary>
    public sealed class ServerWalkingBoxService : MonoBehaviour
    {
        [Serializable] private sealed class Counter { public string device_id,boot; public long sequence,sensor_total; }
        [Serializable] private sealed class BoxPayload { public string box_id; }
        [Serializable] private sealed class Banked { public string id,collection_id; }
        [Serializable] private sealed class Item { public string collectible_id,collection_id; public int tier; public bool is_new; }
        [Serializable] private sealed class Delivery { public string box_id,collection_id; public Item item; public bool finished; public int earned_day; }
        [Serializable] private sealed class Odds { public int tier; public double chance; }
        [Serializable] private sealed class Snapshot
        {
            public string server_time,next_box_at,error;
            public int earned_today,step_credit,cooldown_seconds,steps_per_box,daily_limit,bank_limit;
            public Banked[] boxes; public Delivery[] deliveries; public Odds[] odds;
        }
        private PrototypeHud hud;
        private CollectionSession collection;
        private SupabaseAccountService account;
        private string owner;
        private bool busy,stopped,reportBlocked;
        private float retryAt;
        private Snapshot snapshot;
        private readonly Dictionary<string,Delivery> saved = new Dictionary<string,Delivery>();
        /// <summary>Validated server progress, for in-memory presentation edges; never a local earning signal.</summary>
        public event Action<WalkingMomentSnapshot> SnapshotReceived;
        private string Key => "pockle.walking.server.v1." + owner;
        private string ProgressKey(string id) => Key + ".reveal." + id;

        public void Initialize(PrototypeHud view,CollectionSession session,SupabaseAccountService accounts)
        {
            hud=view;collection=session;account=accounts;
            hud.DailyBoxRequested+=Open;
            hud.BoxRevealProgress+=Progress;
            hud.BoxRevealFinished+=Finished;
            ShowUnavailable("Sign in to sync walking boxes.");
        }
        private void Update()
        {
            if(stopped||hud==null||account==null)return;
            string current=account.UserId;
            if(current!=owner)
            {
                owner=current;snapshot=null;saved.Clear();retryAt=0;reportBlocked=false;
                ShowUnavailable(string.IsNullOrEmpty(owner)?"Sign in to sync walking boxes.":"Syncing walking boxes…");
            }
            if(busy||string.IsNullOrEmpty(owner)||collection.IsReadOnlySave||Time.unscaledTime<retryAt)return;
            retryAt=Time.unscaledTime+30;
            StartCoroutine(Sync(null));
        }
        private void Open()
        {
            if(busy||string.IsNullOrEmpty(owner)||owner!=account.UserId||snapshot?.boxes==null||snapshot.boxes.Length==0||collection.IsReadOnlySave)return;
            StartCoroutine(Sync(snapshot.boxes[0].id));
        }
        private IEnumerator Sync(string boxId)
        {
            busy=true;string requestingOwner=owner;
            Bind(WalkingBoxConnection.Reconnecting);
            // Finishing a reveal is durable locally first. Retry its server acknowledgement after interruptions.
            var acknowledgements=new List<Delivery>(saved.Values);
            foreach(var d in acknowledgements)
            {
                if(!d.finished&&PlayerPrefs.GetInt(ProgressKey(d.box_id),0)<0)
                    yield return Request("box_finish",JsonUtility.ToJson(new BoxPayload{box_id=d.box_id}),(_,ok)=>{if(ok)d.finished=true;});
                if(requestingOwner!=account.UserId){busy=false;yield break;}
            }
            string action="boxes",payload="{}";
            if(boxId!=null){action="box_open";payload=JsonUtility.ToJson(new BoxPayload{box_id=boxId});}
            else if(!reportBlocked&&collection.WalkingActive&&collection.Progress.Save.SensorTotal>=0)
            {
                string device=PlayerPrefs.GetString(Key+".device","");
                if(!Guid.TryParseExact(device,"D",out _)){device=Guid.NewGuid().ToString("D");PlayerPrefs.SetString(Key+".device",device);}
                long.TryParse(PlayerPrefs.GetString(Key+".sequence","0"),NumberStyles.None,CultureInfo.InvariantCulture,out long sequence);
                if(sequence<0||sequence>=long.MaxValue-1){ShowUnavailable("Walking counter needs support.");busy=false;yield break;}
                sequence++;
                PlayerPrefs.SetString(Key+".sequence",sequence.ToString(CultureInfo.InvariantCulture));PlayerPrefs.Save();
                action="walking_report";payload=JsonUtility.ToJson(new Counter{device_id=device,sequence=sequence,
                    sensor_total=collection.Progress.Save.SensorTotal,boot=collection.Progress.Save.SensorBoot.ToString(CultureInfo.InvariantCulture)});
            }
            string raw=null;bool success=false;
            yield return Request(action,payload,(r,ok)=>{raw=r;success=ok;});
            if(requestingOwner!=account.UserId){busy=false;yield break;}
            var next=Parse(raw);
            if(!success&&action=="walking_report"&&next?.error=="walking_device_change_requires_review")
            {
                // Another phone can still recover/open saved boxes. Never replace its active sensor counter implicitly.
                reportBlocked=true;
                yield return Request("boxes","{}",(r,ok)=>{raw=r;success=ok;});
                if(requestingOwner!=account.UserId){busy=false;yield break;}
                next=Parse(raw);
            }
            if(!success||!Valid(next))
            {
                busy=false;retryAt=Time.unscaledTime+30;
                Bind(WalkingBoxConnection.Offline,next?.error=="walking_device_change_requires_review"
                    ?"Walking is linked to another phone. Contact support to switch it."
                    :"Couldn't sync walking boxes. We'll try again.");
                yield break;
            }
            snapshot=next;saved.Clear();
            bool failedSave=false;
            foreach(var d in next.deliveries)
            {
                if(d?.item==null||!Guid.TryParseExact(d.box_id,"D",out _)||d.item.collection_id!=d.collection_id||
                    !ToyCatalog.TryGetCollectible(d.item.collectible_id,out var toy)||!toy.Available||toy.CollectionId!=d.collection_id)
                {failedSave=true;continue;}
                bool known=collection.Progress.Save.WalkingDeliveryIds?.Contains(d.box_id)==true;
                if(!known&&!collection.CommitWalkingDelivery(d.box_id,d.collection_id,d.item.collectible_id,d.earned_day))
                {failedSave=true;continue;}
                saved[d.box_id]=d;
            }
            busy=false;retryAt=Time.unscaledTime+30;
            Bind(WalkingBoxConnection.Online,failedSave?"A saved reward is waiting to sync. Please try again.":null);
            var bankedIds = new string[next.boxes.Length];
            for (int i = 0; i < bankedIds.Length; i++) bankedIds[i] = next.boxes[i].id;
            var serverDay = DateTimeOffset.Parse(next.server_time,CultureInfo.InvariantCulture).UtcDateTime.Date;
            SnapshotReceived?.Invoke(new WalkingMomentSnapshot(requestingOwner,
                (int)(serverDay - new DateTime(1970,1,1)).TotalDays,next.earned_today,next.step_credit,bankedIds));
            if(hud.OnboardingVisible||hud.BoxRevealVisible||hud.CheckoutVisible||!string.IsNullOrEmpty(collection.Progress.Save.PendingFusionId))yield break;
            foreach(var d in next.deliveries)
            {
                if(!saved.ContainsKey(d.box_id)||d.finished||PlayerPrefs.GetInt(ProgressKey(d.box_id),0)<0)continue;
                hud.ShowBoxReveal(new BoxRevealView{ReceiptId=d.box_id,Source=BoxRevealSource.WalkingBox,
                    StartAt=Math.Min(1,Math.Max(0,PlayerPrefs.GetInt(ProgressKey(d.box_id),0))),
                    Items=new[]{new BoxRevealItem{CollectibleId=d.item.collectible_id,CollectionId=d.collection_id,
                        Tier=RarityLook.FromTier((ToyTier)d.item.tier),IsNew=d.item.is_new}}});
                break;
            }
        }
        private static bool Valid(Snapshot value)
        {
            if(value==null||value.boxes==null||value.deliveries==null||value.steps_per_box!=1000||value.daily_limit!=4||
                value.bank_limit!=2||value.cooldown_seconds!=3600||value.earned_today<0||value.earned_today>4||
                value.step_credit<0||value.boxes.Length>2)return false;
            foreach(var b in value.boxes)
                if(b==null||!Guid.TryParseExact(b.id,"D",out _)||!ToyCatalog.TryGetCollection(b.collection_id,out _))return false;
            return DateTimeOffset.TryParse(value.server_time,CultureInfo.InvariantCulture,DateTimeStyles.None,out _)&&
                DateTimeOffset.TryParse(value.next_box_at,CultureInfo.InvariantCulture,DateTimeStyles.None,out _);
        }
        private void Bind(WalkingBoxConnection connection,string message=null)
        {
            if(snapshot==null){ShowUnavailable(message??"Syncing walking boxes…");return;}
            var bank=new List<string>();foreach(var b in snapshot.boxes)bank.Add(b.collection_id);
            var odds=new List<TierOdds>();if(snapshot.odds!=null)foreach(var row in snapshot.odds)
                if(row.tier>=0&&row.tier<=3&&row.chance>=0&&row.chance<=1)odds.Add(new TierOdds(RarityLook.FromTier((ToyTier)row.tier),row.chance));
            hud.SetWalkingBoxes(new WalkingBoxView{BankedCollectionIds=bank,EarnedToday=snapshot.earned_today,
                StepCredit=Math.Min(snapshot.steps_per_box,snapshot.step_credit),StepsPerBox=snapshot.steps_per_box,
                MaxPerDay=snapshot.daily_limit,MaxBanked=snapshot.bank_limit,CooldownHours=snapshot.cooldown_seconds/3600,
                SecondsUntilNextBox=Countdown(snapshot.server_time,snapshot.next_box_at,bank.Count>=snapshot.bank_limit||snapshot.earned_today>=snapshot.daily_limit),
                Connection=connection,Busy=busy,CanOpen=bank.Count>0,Odds=odds,Message=message??(reportBlocked
                    ?"Steps sync from your other phone. You can still open your saved boxes here.":null)});
        }
        internal static double? Countdown(string serverTime,string nextBoxAt,bool paused)
        {
            if(paused||!DateTimeOffset.TryParse(serverTime,CultureInfo.InvariantCulture,DateTimeStyles.None,out var now)||
                !DateTimeOffset.TryParse(nextBoxAt,CultureInfo.InvariantCulture,DateTimeStyles.None,out var next)||next<=now)return null;
            return (next-now).TotalSeconds;
        }
        private void ShowUnavailable(string message)
        {hud.SetWalkingBoxes(new WalkingBoxView{Connection=WalkingBoxConnection.Offline,Busy=busy,CanOpen=false,Message=message});}
        private void Progress(string id,int count)
        {
            if(!saved.ContainsKey(id)||owner!=account.UserId)return;
            PlayerPrefs.SetInt(ProgressKey(id),Math.Min(1,Math.Max(0,count)));PlayerPrefs.Save();
        }
        private void Finished(string id)
        {
            if(!saved.ContainsKey(id)||owner!=account.UserId)return;
            PlayerPrefs.SetInt(ProgressKey(id),-1);PlayerPrefs.Save();retryAt=0;
        }
        private IEnumerator Request(string action,string payload,Action<string,bool> done)
        {yield return account.AuthorizedRequest("pockle-api","{\"action\":\""+action+"\",\"payload\":"+payload+"}",done);}
        private static Snapshot Parse(string raw){try{return JsonUtility.FromJson<Snapshot>(raw);}catch{return null;}}
        private void OnApplicationPause(bool paused){if(!paused)retryAt=0;}
        private void OnDestroy()
        {
            stopped=true;if(hud==null)return;
            hud.DailyBoxRequested-=Open;hud.BoxRevealProgress-=Progress;hud.BoxRevealFinished-=Finished;
        }
    }
}
