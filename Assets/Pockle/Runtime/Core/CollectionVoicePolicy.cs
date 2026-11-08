namespace Pockle.Core
{
    public enum CollectionVoiceMood { Happy, Curious, Playful }
    /// <summary>Approved collection voices, independent of finish pigment, rarity and temperament.</summary>
    public static class CollectionVoicePolicy
    {
        public static bool HasPilot(string character) => character == "pip" || character == "fox" || character == "wally";
        public static CollectionVoiceMood For(PipAction action) => action == PipAction.CuriousTilt || action == PipAction.Peekaboo ? CollectionVoiceMood.Curious : action == PipAction.ThankYouBow ? CollectionVoiceMood.Happy : CollectionVoiceMood.Playful;
        public static CollectionVoiceMood For(FoxAction action) => action == FoxAction.CuriousTilt || action == FoxAction.SleepyBlink ? CollectionVoiceMood.Curious : action == FoxAction.GreetingBow ? CollectionVoiceMood.Happy : CollectionVoiceMood.Playful;
        public static CollectionVoiceMood For(WallyAction action) => action == WallyAction.CuriousRoll || action == WallyAction.SleepyBob ? CollectionVoiceMood.Curious : action == WallyAction.FlipperWave ? CollectionVoiceMood.Happy : CollectionVoiceMood.Playful;
        public static string Resource(string character, CollectionVoiceMood mood)
        {
            if (!HasPilot(character) || !System.Enum.IsDefined(typeof(CollectionVoiceMood), mood)) return "";
            return "Audio/Collections/" + character + "-" + (mood == CollectionVoiceMood.Happy ? "happy" : mood == CollectionVoiceMood.Curious ? "curious" : "playful");
        }
    }
}
