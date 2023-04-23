using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Gui.FlyText;
using System;
using System.Collections.Generic;

namespace PatMe
{
    internal class RewardFlyTextPat : IEmoteReward
    {
        public List<DateTime> recentPatTimes = new();
        private Random rand = new Random();

        public void OnCounterChanged(EmoteCounter counterOb, PlayerCharacter instigator, out bool stopProcessing)
        {
            if (Service.pluginConfig.showFlyText)
            {
                var useDesc = counterOb.descSingular.ToUpper();
                var useSubDesc = Service.pluginConfig.showFlyTextNames && instigator != null ? instigator.Name : " ";
                var useColor = 0xff00ff00;

                bool isLongRange = instigator.YalmDistanceX > 7 || instigator.YalmDistanceZ > 7;
                bool isOwnerAFK = (Service.clientState.LocalPlayer.StatusFlags & StatusFlags.OffhandOut) != 0;
                bool isOwnerInCombat = (Service.clientState.LocalPlayer.StatusFlags & StatusFlags.InCombat) != 0;
                UpdateTimestamps(out int numPatsInLast3s);

                if (isLongRange)
                {
                    useDesc = "Distant " + useDesc;
                }
                else if (isOwnerAFK)
                {
                    useDesc = "Sneaky " + useDesc;
                }
                else if (isOwnerInCombat)
                {
                    useDesc = "Calming " + useDesc;
                }
                else if (numPatsInLast3s >= 3)
                {
                    useDesc = "Quick " + useDesc;

                    if (numPatsInLast3s >= 6)
                    {
                        useSubDesc = "HEAD TRAUMA WARNING !!";
                        useColor = 0xff0040ff;
                    }
                }
                else if (rand.NextSingle() < 0.01)
                {
                    useDesc = "PERFECT " + useDesc;
                    useColor = 0xff00d7ff;
                }

                Service.flyTextGui?.AddFlyText(FlyTextKind.NamedCriticalDirectHit, 0, counterOb.Value, 0, useDesc, useSubDesc, useColor, 0, 0);
            }

            stopProcessing = false;
        }

        private void UpdateTimestamps(out int numPatsInLast3s)
        {
            while (recentPatTimes.Count > 16)
            {
                recentPatTimes.RemoveAt(0);
            }

            var timeNow = DateTime.Now;
            recentPatTimes.Add(timeNow);

            numPatsInLast3s = 1;
            for (int idx = recentPatTimes.Count - 2; idx >= 0; idx--) // ignore len -1, it's == timeNow
            {
                var timeSince = timeNow.Subtract(recentPatTimes[idx]);
                if (timeSince.TotalMilliseconds > 3000)
                {
                    break;
                }

                numPatsInLast3s++;
            }
        }
    }
}
