using Dalamud.Game.ClientState.Objects.SubKinds;
using System;
using System.Collections.Generic;

namespace PatMe
{
    public class EmoteCounter
    {
        private EmoteInstigatorCounter instigatorsCurrentZone = new();
        private EmoteInstigatorCounter instigators = new();
        private int[] emoteIds;
        private string emoteName;

        public EmoteCounterDB dataLink;
        public bool isActive = true;
        public List<IEmoteReward> rewards;

        public string descSingular;
        public string descPlural;
        public string descUI;

        public string Name => emoteName;
        public uint Value => dataLink?.Value ?? 0;

        public void Initialize(string emoteName, int[] emoteIds)
        {
            this.emoteName = emoteName;
            this.emoteIds = emoteIds;
        }

        public bool OnEmote(PlayerCharacter instigator, ushort emoteId)
        {
            if (!isActive || emoteIds == null || instigator == null || dataLink == null)
            {
                return false;
            }

            var isEmoteMatching = Array.FindIndex(emoteIds, x => x == emoteId) >= 0;
            if (!isEmoteMatching)
            {
                return false;
            }

            dataLink.Value++;

            var instigatorKey = EmoteInstigatorCounter.InstigatorData.Create(instigator);
            instigators.Increment(instigatorKey);
            instigatorsCurrentZone.Increment(instigatorKey);

            foreach (var rewardOb in rewards)
            {
                if (rewardOb != null)
                {
                    rewardOb.OnCounterChanged(this, instigator, out bool stopProcessing);
                    if (stopProcessing)
                    {
                        break;
                    }
                }
            }

            return true;
        }

        public void OnTerritoryChanged()
        {
            instigatorsCurrentZone.Clear();
        }

        public bool GetTopEmotes(out string instigatorName, out uint score) => instigators.GetHighestScore(out instigatorName, out score);

        public bool GetTopEmotesInCurrentZone(out string instigatorName, out uint score) => instigatorsCurrentZone.GetHighestScore(out instigatorName, out score);

        public uint GetEmoteCounter(string instigatorName) => instigators.GetCounter(instigatorName);

        public uint GetEmoteCounterInCurrentZone(string instigatorName) => instigatorsCurrentZone.GetCounter(instigatorName);
    }
}
