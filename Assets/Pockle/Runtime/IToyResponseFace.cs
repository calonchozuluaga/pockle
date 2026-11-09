using Pockle.Core;
namespace Pockle.Runtime
{
    public interface IToyResponseFace
    {
        PipTemperament CurrentTemperament { get; }
        bool CycleEnabled { get; }
        void BeginResponse(ToyReactionResponse response, float duration);
    }
}
