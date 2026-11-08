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
        public static string ActionResource(string character, System.Enum action)
        {
            System.Type expected;
            switch (character)
            {
                case "pip": expected = typeof(PipAction); break;
                case "fox": expected = typeof(FoxAction); break;
                case "axolotl": expected = typeof(AxolotlAction); break;
                case "firb": expected = typeof(FirbAction); break;
                case "coco": expected = typeof(CocoAction); break;
                case "snowy": expected = typeof(SnowyAction); break;
                case "ginger": expected = typeof(GingerAction); break;
                case "reiny": case "guin": case "bundy": expected = typeof(WinterTrioAction); break;
                case "wally": expected = typeof(WallyAction); break;
                default: return "";
            }
            if (action == null || action.GetType() != expected || !System.Enum.IsDefined(expected, action)) return "";
            return "Audio/Collections/" + character + "/" + Slug(action.ToString());
        }
        public static string TemperamentResource(string character, PipTemperament temperament)
        {
            if (ActionResource(character, FirstAction(character)).Length == 0 || !System.Enum.IsDefined(typeof(PipTemperament), temperament)) return "";
            return "Audio/Collections/" + character + "/temperament-" + Slug(temperament.ToString());
        }
        private static System.Enum FirstAction(string character)
        {
            switch (character)
            {
                case "pip": return PipAction.Hop; case "fox": return FoxAction.Jump;
                case "axolotl": return AxolotlAction.HelloWave; case "firb": return FirbAction.BranchWave;
                case "coco": return CocoAction.HandleHello; case "snowy": return SnowyAction.MittenWave;
                case "ginger": return GingerAction.CookieWave; case "wally": return WallyAction.BreachJump;
                default: return WinterTrioAction.Greeting;
            }
        }
        private static string Slug(string name)
        {
            var result = new System.Text.StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                if (i > 0 && char.IsUpper(name[i])) result.Append('-');
                result.Append(char.ToLowerInvariant(name[i]));
            }
            return result.ToString();
        }
        public static string Resource(string character, CollectionVoiceMood mood)
        {
            if (!HasPilot(character) || !System.Enum.IsDefined(typeof(CollectionVoiceMood), mood)) return "";
            return "Audio/Collections/" + character + "-" + (mood == CollectionVoiceMood.Happy ? "happy" : mood == CollectionVoiceMood.Curious ? "curious" : "playful");
        }
    }
}
