using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Gui.Toast;

namespace PatMe
{
    internal class RewardProgressNotify : IEmoteReward
    {
        public void OnCounterChanged(EmoteCounter counterOb, PlayerCharacter instigator, out bool stopProcessing)
        {
            var isSpecial = (counterOb.Value < 25) ? (counterOb.Value == 5 || counterOb.Value == 15) : ((counterOb.Value % 25) == 0);
            var canShow = isSpecial && Service.pluginConfig.showProgressNotify;

            if (canShow)
            {
                var useDesc = counterOb.descPlural.ToUpper();
                Service.toastGui?.ShowQuest($"{counterOb.Value} {useDesc}!", new QuestToastOptions
                {
                    Position = QuestToastPosition.Centre,
                    DisplayCheckmark = true,
                    IconId = 0,
                    PlaySound = true
                });

                stopProcessing = true;
            }
            else
            {
                stopProcessing = false;
            }
        }
    }
}
