using System;
using System.Collections;
using Pockle.Core;
using UnityEngine;
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
using Unity.Notifications;
#endif

namespace Pockle.Runtime
{
    public enum BoxNotificationPermission { NotAsked, Pending, Granted, Denied, Unsupported }

    /// <summary>Local presentation only. All reward decisions remain on the server.</summary>
    public sealed class LocalBoxNotifications : MonoBehaviour
    {
        public const string EligibilityTitle = "Ready for another walk?";
        public const string EligibilityText = "Your next box can now be earned. Walk with Pockle to fill it.";
        public const string EarnedTitle = "You earned a box!";
        public const string EarnedPluralTitle = "Your walking boxes are ready!";
        public const string EarnedText = "Come see who's inside Pockle.";
        private const int EligibilityId = 27001, EarnedId = 27002;
        private const string PromptKey = "pockle.notifications.permission-requested.v1";
        public BoxNotificationPermission Permission { get; private set; } = BoxNotificationPermission.Unsupported;
        public event Action<BoxNotificationPermission> PermissionChanged;
        private CollectionSession session;
        private SupabaseAccountService account;
        private ServerWalkingBoxService walking;
        private PrototypeHud hud;
        private NotificationGate gate;
        private BoxNotificationPolicy policy;
        private string owner;
        private WalkingBoxView latest;
        private bool paused, permissionBusy, routeHome;
        private double receivedAt, eligibilityDue;
        private DateTimeOffset serverTime;
        private string PolicyKey => "pockle.notifications.history.v1." + owner;

        public void Initialize(CollectionSession collection, SupabaseAccountService accounts, ServerWalkingBoxService boxes, PrototypeHud view)
        {
            session = collection; account = accounts; walking = boxes; hud = view;
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            var args = NotificationCenterArgs.Default;
            args.AndroidChannelId = "pockle-boxes"; args.AndroidChannelName = "Walking boxes";
            args.AndroidChannelDescription = "Walking box reminders";
            args.PresentationOptions = NotificationPresentation.Alert | NotificationPresentation.Sound;
            try { NotificationCenter.Initialize(args); }
            catch (Exception e) { Debug.LogWarning("Notifications unavailable: " + e.GetType().Name); return; }
#endif
            gate = new NotificationGate(kind => { if (kind == NotificationPrefs.BoxReady) CancelAll(); });
            session.WalkingEnableRequested += RequestFromWalking;
            walking.NotificationSnapshotReceived += Observe;
            RefreshPermission();
            hud.SetNotificationPermission(Permission, RequestFromSettings, OpenPhoneSettings);
            StartCoroutine(CheckLaunch());
        }
        private void Update()
        {
            if (account != null && owner != account.UserId)
            {
                CancelAll(); owner = account.UserId; latest = null; eligibilityDue = 0;
                policy = string.IsNullOrEmpty(owner) ? null : Load();
            }
            if (routeHome && hud != null && !hud.OnboardingVisible && !hud.BoxRevealVisible && !hud.CheckoutVisible)
            { for (int i = 0; i < 16 && hud.CurrentPage != AppPage.Home; i++)
                    if (!hud.GoBack()) break;
                routeHome = false; }
        }
        private BoxNotificationPolicy Load()
        {
            try { var loaded = JsonUtility.FromJson<BoxNotificationPolicy>(PlayerPrefs.GetString(PolicyKey, "")) ?? new BoxNotificationPolicy();
                if (loaded.Seen == null) loaded.Seen = new System.Collections.Generic.List<string>();
                return loaded; }
            catch { return new BoxNotificationPolicy(); }
        }
        private void Save() { PlayerPrefs.SetString(PolicyKey, JsonUtility.ToJson(policy)); PlayerPrefs.Save(); }
        private void Observe(string user, WalkingBoxView view, DateTimeOffset now, string[] banked)
        {
            if (string.IsNullOrEmpty(user) || user != account.UserId) return;
            if (owner != user) { CancelAll(); owner = user; policy = Load(); }
            foreach (string previous in policy.Seen)
                if (Array.IndexOf(banked, previous) < 0) { CancelAll(); break; }
            int day = (int)(now.UtcDateTime.Date - new DateTime(1970,1,1)).TotalDays;
            latest = view; serverTime = now; receivedAt = Time.realtimeSinceStartupAsDouble;
            bool earned = policy.Observe(day, view.EarnedToday, banked, !paused);
            Save();
            if (view.BankFull || view.DayDone || !view.SecondsUntilNextBox.HasValue) Cancel(EligibilityId);
            if (earned && paused && Permission == BoxNotificationPermission.Granted)
                gate.Show(NotificationPrefs.BoxReady, () => Send(EarnedId, view.Banked > 1 ? EarnedPluralTitle : EarnedTitle, EarnedText, 1));
            if (paused) ScheduleReminder();
        }
        private void ScheduleReminder()
        {
            if (latest == null || policy == null || Permission != BoxNotificationPermission.Granted ||
                !latest.Online || latest.BankFull || latest.DayDone || !latest.SecondsUntilNextBox.HasValue ||
                !NotificationPrefs.IsOn(NotificationPrefs.BoxReady)) return;
            double seconds = latest.SecondsUntilNextBox.Value - (Time.realtimeSinceStartupAsDouble - receivedAt);
            // Never turn an expired or stale snapshot into a notification, nor carry yesterday's limits forward.
            if (seconds <= 1 || seconds > 3600 || Time.realtimeSinceStartupAsDouble - receivedAt > 60) return;
            int fireDay = (int)((serverTime.AddSeconds(latest.SecondsUntilNextBox.Value).UtcDateTime.Date - new DateTime(1970,1,1)).TotalDays);
            bool pending = eligibilityDue > Time.realtimeSinceStartupAsDouble && policy.EligibilityDay == fireDay;
            if (!pending && !policy.ReserveEligibility(fireDay)) return;
            Cancel(EligibilityId);
            eligibilityDue = Time.realtimeSinceStartupAsDouble + seconds;
            Save();
            gate.Schedule(NotificationPrefs.BoxReady, () => Send(EligibilityId, EligibilityTitle, EligibilityText, seconds));
        }
        public void RequestFromSettings()
        {
            if (session == null) return;
            if (!session.WalkingActive) session.EnableWalking();
            else RequestFromWalking();
        }
        public void RequestFromWalking()
        {
            if (!permissionBusy && PlayerPrefs.GetInt(PromptKey, 0) == 0 && NotificationPrefs.IsOn(NotificationPrefs.BoxReady))
                StartCoroutine(RequestPermission());
        }
        private IEnumerator RequestPermission()
        {
            permissionBusy = true;
            // Let the motion permission sheet finish before requesting another OS permission.
            float end = Time.realtimeSinceStartup + 60;
            while (session != null && !session.WalkingActive && Time.realtimeSinceStartup < end) yield return null;
            if (session == null || !session.WalkingActive || !NotificationPrefs.IsOn(NotificationPrefs.BoxReady)) { permissionBusy = false; yield break; }
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            PlayerPrefs.SetInt(PromptKey, 1); PlayerPrefs.Save();
            SetPermission(BoxNotificationPermission.Pending);
            NotificationsPermissionRequest request;
            try { request = NotificationCenter.RequestPermission(); }
            catch (Exception e) { Debug.LogWarning("Notification permission unavailable: " + e.GetType().Name);
                SetPermission(BoxNotificationPermission.Denied); permissionBusy = false; yield break; }
            yield return request;
#endif
            RefreshPermission(); permissionBusy = false;
        }
        private void RefreshPermission()
        {
            try
            {
#if UNITY_ANDROID && !UNITY_EDITOR
            var status = Unity.Notifications.Android.AndroidNotificationCenter.UserPermissionToPost;
            SetPermission(status == Unity.Notifications.Android.PermissionStatus.Allowed ? BoxNotificationPermission.Granted :
                PlayerPrefs.GetInt(PromptKey, 0) == 0 ? BoxNotificationPermission.NotAsked : BoxNotificationPermission.Denied);
#elif UNITY_IOS && !UNITY_EDITOR
            var status = Unity.Notifications.iOS.iOSNotificationCenter.GetNotificationSettings().AuthorizationStatus;
            SetPermission(status == Unity.Notifications.iOS.AuthorizationStatus.Authorized || status == Unity.Notifications.iOS.AuthorizationStatus.Provisional
                ? BoxNotificationPermission.Granted : status == Unity.Notifications.iOS.AuthorizationStatus.NotDetermined
                ? BoxNotificationPermission.NotAsked : BoxNotificationPermission.Denied);
#else
            SetPermission(BoxNotificationPermission.Unsupported);
#endif
            }
            catch (Exception e) { Debug.LogWarning("Notification settings unavailable: " + e.GetType().Name); SetPermission(BoxNotificationPermission.Unsupported); }
            if (Permission != BoxNotificationPermission.Granted) CancelAll();
        }
        private void SetPermission(BoxNotificationPermission value)
        { if (Permission != value) { Permission = value; hud?.SetNotificationPermission(value, RequestFromSettings, OpenPhoneSettings); PermissionChanged?.Invoke(value); } }
        public void OpenPhoneSettings()
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            NotificationCenter.OpenNotificationSettings();
#endif
        }
        private static void Send(int id, string title, string text, double seconds)
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            try { NotificationCenter.ScheduleNotification(new Notification { Identifier = id, Title = title, Text = text,
                Data = "pockle.home.boxes." + Guid.NewGuid().ToString("N"), ShowInForeground = false }, new NotificationIntervalSchedule(TimeSpan.FromSeconds(seconds))); }
            catch (Exception e) { Debug.LogWarning("Box notification scheduling failed: " + e.GetType().Name); }
#endif
        }
        private static void Cancel(int id)
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            try { NotificationCenter.CancelScheduledNotification(id); NotificationCenter.CancelDeliveredNotification(id); }
            catch (Exception e) { Debug.LogWarning("Box notification cancellation failed: " + e.GetType().Name); }
#endif
        }
        private void CancelAll() { Cancel(EligibilityId); Cancel(EarnedId); eligibilityDue = 0; }
        private IEnumerator CheckLaunch()
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            var result = NotificationCenter.QueryLastRespondedNotification();
            yield return result;
            if (result.State == QueryLastRespondedNotificationState.HaveRespondedNotification)
            {
                string data = result.Notification.Data;
                const string key = "pockle.notifications.last-opened.v1";
                if (data != null && data.StartsWith("pockle.home.boxes.", StringComparison.Ordinal) && PlayerPrefs.GetString(key, "") != data)
                { PlayerPrefs.SetString(key, data); PlayerPrefs.Save(); routeHome = true; }
            }
#endif
            yield break;
        }
        private void OnApplicationPause(bool value)
        {
            paused = value;
            if (gate == null) return;
            if (value) ScheduleReminder();
            else { StartCoroutine(CheckLaunch()); CancelAll(); latest = null; RefreshPermission(); }
        }
        private void OnDestroy()
        {
            gate?.Dispose();
            if (session != null) session.WalkingEnableRequested -= RequestFromWalking;
            if (walking != null) walking.NotificationSnapshotReceived -= Observe;
        }
    }
}
