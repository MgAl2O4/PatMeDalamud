using Dalamud.Game.ClientState.Objects.Types;
using System;
using System.Collections.Generic;

namespace PatMe
{
    public class EmoteCounter
    {
        public Action<int> OnChanged;

        public int counterEmoteId;
        public string counterDesc;
        public string counterDescPlural;
        public string uiDesc;

        private Dictionary<string, int> mapEmotesInZone = new();
        private string currentPlayerName;

        public void OnEmote(GameObject instigator, int emoteId)
        {
            if (emoteId == counterEmoteId)
            {
                IncCounter();

                var instigatorName = (instigator != null) ? instigator.Name.ToString() : "??";
                if (mapEmotesInZone.TryGetValue(instigatorName, out int counter))
                {
                    mapEmotesInZone[instigatorName] = counter + 1;
                }
                else
                {
                    mapEmotesInZone.Add(instigatorName, 1);
                }
            }
        }

        public int GetCounter()
        {
            if (currentPlayerName == null)
            {
                currentPlayerName = GetCurrentPlayerName();
            }

            var emoteData = Service.pluginConfig.FindOrAddEmote(currentPlayerName, counterEmoteId);
            if (emoteData != null)
            {
                return emoteData.Counter;
            }

            return 0;
        }

        public bool IncCounter()
        {
            if (currentPlayerName == null)
            {
                currentPlayerName = GetCurrentPlayerName();
            }

            var emoteData = Service.pluginConfig.FindOrAddEmote(currentPlayerName, counterEmoteId);
            if (emoteData != null)
            {
                emoteData.Counter++;
                Service.pluginConfig.Save();

                OnChanged?.Invoke(emoteData.Counter);
                return true;
            }

            return false;
        }

        public void OnLogout()
        {
            currentPlayerName = null;
        }

        public void OnTerritoryChanged()
        {
            mapEmotesInZone.Clear();
        }

        public (string, int) GetTopEmotesInZone()
        {
            string maxPatsPlayer = null;
            int maxPats = 0;

            foreach (var kvp in mapEmotesInZone)
            {
                if (kvp.Value > maxPats)
                {
                    maxPats = kvp.Value;
                    maxPatsPlayer = kvp.Key;
                }
            }

            return (maxPatsPlayer, maxPats);
        }

        public int GetEmotesInCurrentZone(string instigatorName)
        {
            if (mapEmotesInZone.TryGetValue(instigatorName, out int numPats))
            {
                return numPats;
            }

            return 0;
        }

        private string GetCurrentPlayerName()
        {
            if (Service.clientState == null || Service.clientState.LocalPlayer == null || Service.clientState.LocalPlayer.Name == null)
            {
                return null;
            }

            return Service.clientState.LocalPlayer.Name.TextValue;
        }
    }
}
