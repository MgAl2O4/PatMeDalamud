using Dalamud.Game.ClientState.Objects.SubKinds;
using System;
using System.Collections.Generic;

namespace PatMe
{
    public class EmoteCounter
    {
        private Dictionary<string, int> mapEmotesInZone = new();
        private int[] emoteIds;
        private string emoteName;

        public Action<uint> OnChanged;
        public EmoteCounterDB dataLink;
        public bool isActive = true;

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
            OnChanged?.Invoke(Value);

            var instigatorName = instigator.Name.ToString();
            if (mapEmotesInZone.TryGetValue(instigatorName, out int counter))
            {
                mapEmotesInZone[instigatorName] = counter + 1;
            }
            else
            {
                mapEmotesInZone.Add(instigatorName, 1);
            }

            return true;
        }

        public void OnTerritoryChanged()
        {
            mapEmotesInZone.Clear();
        }

        public (string, int) GetTopEmotesInZone()
        {
            string maxEmotesPlayer = null;
            int maxEmotes = 0;

            foreach (var kvp in mapEmotesInZone)
            {
                if (kvp.Value > maxEmotes)
                {
                    maxEmotes = kvp.Value;
                    maxEmotesPlayer = kvp.Key;
                }
            }

            return (maxEmotesPlayer, maxEmotes);
        }

        public int GetEmotesInCurrentZone(string instigatorName)
        {
            if (mapEmotesInZone.TryGetValue(instigatorName, out int numEmotes))
            {
                return numEmotes;
            }

            return 0;
        }
    }
}
