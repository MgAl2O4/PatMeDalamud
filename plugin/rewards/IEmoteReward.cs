using Dalamud.Game.ClientState.Objects.SubKinds;

namespace PatMe
{
    public interface IEmoteReward
    {
        void OnCounterChanged(EmoteCounter counterOb, PlayerCharacter instigator, out bool stopProcessing);
    }
}
