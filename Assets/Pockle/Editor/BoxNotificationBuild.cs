using Unity.Notifications;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Pockle.Editor
{
    /// <summary>Keep OS permission at Enable walking; this slice never enables remote push or exact alarms.</summary>
    public sealed class BoxNotificationBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder => int.MinValue;
        public void OnPreprocessBuild(BuildReport report)
        {
            NotificationSettings.iOSSettings.RequestAuthorizationOnAppLaunch = false;
            NotificationSettings.iOSSettings.AddRemoteNotificationCapability = false;
            NotificationSettings.iOSSettings.NotificationRequestAuthorizationForRemoteNotificationsOnAppLaunch = false;
            NotificationSettings.AndroidSettings.ExactSchedulingOption = (AndroidExactSchedulingOption)0;
        }
    }
}
