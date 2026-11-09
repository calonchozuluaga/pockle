using System;
namespace Pockle.Core
{
    /// <summary>One local decision owns the face, written cue and approved recording.</summary>
    public readonly struct ToyReactionResponse
    {
        public readonly string Text, AudioResource;
        public readonly PipTemperament Face;
        public readonly bool AllowsMotionExpression;
        public ToyReactionResponse(string text, string audio, PipTemperament face, bool motion = true)
        { Text = text; AudioResource = audio; Face = face; AllowsMotionExpression = motion; }
    }
    public static class ToyReactionResponses
    {
        public static PipFacePose FacePose(ToyReactionResponse response, float progress)
        {
            if(response.Face!=PipTemperament.Sleepy)return PipFaces.Reaction(response.Face,progress);
            float p=Numeric.Clamp(Numeric.FiniteOr(progress,0),0,1);
            return new PipFacePose(blink:.72f,lids:.8f,lidAngle:-.65f,sleepCurve:1,
                mouthOpen:(float)Math.Sin(p*Math.PI)*.7f,mouthScale:.8f);
        }
        public static ToyReactionResponse Temperament(string character, PipTemperament face)
        {
            string text;
            switch (face)
            {
                case PipTemperament.Grumpy: text = "hmph!"; break;
                case PipTemperament.Shy: text = "eep!"; break;
                case PipTemperament.ShowOff: text = "ta-da!"; break;
                case PipTemperament.Dreamy: text = "mmm~"; break;
                case PipTemperament.Sleepy: text = "*yawn*"; break;
                case PipTemperament.Mischief: text = "hehe!"; break;
                case PipTemperament.Calm: text = "ahh~"; break;
                default: text = "chirp!"; break;
            }
            return new ToyReactionResponse(text, CollectionVoicePolicy.TemperamentResource(character, face), face, false);
        }
        public static ToyReactionResponse Action(string character, Enum action)
        {
            string resource = CollectionVoicePolicy.ActionResource(character, action);
            if (string.IsNullOrEmpty(resource)) return default;
            string name = action.ToString();
            if (name.StartsWith("Sleepy", StringComparison.Ordinal)) return new ToyReactionResponse("*yawn*", resource, PipTemperament.Sleepy, false);
            if (name == "CozySway" || name == "CozyNod") return new ToyReactionResponse("mmm~", resource, PipTemperament.Calm, false);
            if (name.StartsWith("Curious", StringComparison.Ordinal)) return new ToyReactionResponse("ooh?", resource, PipTemperament.Dreamy);
            if (name == "Peekaboo") return new ToyReactionResponse("eep!", resource, PipTemperament.Shy);
            if (name == "ThankYouBow" || name == "GreetingBow" || name == "SweetBow" || name == "Bow") return new ToyReactionResponse("mmm!", resource, PipTemperament.Calm);
            return new ToyReactionResponse("chirp!", resource, PipTemperament.Sunny);
        }
    }
}
