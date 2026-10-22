using System;
namespace Pockle.Runtime
{
    /// <summary>All platform notification scheduling/presentation must pass this preference gate.</summary>
    public sealed class NotificationGate : IDisposable
    {
        private readonly Action<string> cancel;
        public NotificationGate(Action<string> cancelScheduled) { cancel=cancelScheduled;NotificationPrefs.Changed+=Changed; }
        public bool Schedule(string kind,Action schedule) { if(!Allowed(kind))return false;schedule();return true; }
        public bool Show(string kind,Action show) { if(!Allowed(kind))return false;show();return true; }
        private static bool Allowed(string kind)=>Array.IndexOf(NotificationPrefs.Kinds,kind)>=0&&NotificationPrefs.IsOn(kind);
        private void Changed(string kind,bool on) { if(!on)cancel?.Invoke(kind); }
        public void Dispose()=>NotificationPrefs.Changed-=Changed;
    }
}
