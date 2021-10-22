using System;
using System.Collections.Generic;

namespace PatMe
{
    public class PatCounter : IDisposable
    {
        public Action<int> OnPatPat;

        private Dictionary<string, int> mapPatsInZone = new();
        private string currentPlayerName;

        public PatCounter()
        {
            Service.clientState.Logout += ClientState_Logout;
        }

        public void Dispose()
        {
            Service.clientState.Logout -= ClientState_Logout;
        }

        private void ClientState_Logout(object sender, EventArgs e)
        {
            currentPlayerName = null;
        }

        public bool GetPats(out int numPats)
        {
            if (currentPlayerName == null)
            {
                currentPlayerName = GetCurrentPlayerName();
            }

            numPats = 0;
            if (currentPlayerName != null)
            {
                Service.pluginConfig.mapPats.TryGetValue(currentPlayerName, out numPats);
                return true;
            }

            return false;
        }

        public void SetPats(int value)
        {
            if (currentPlayerName == null)
            {
                currentPlayerName = GetCurrentPlayerName();
            }

            if (currentPlayerName != null)
            {
                var mapPats = Service.pluginConfig.mapPats;
                if (mapPats.ContainsKey(currentPlayerName))
                {
                    mapPats[currentPlayerName] = value;
                }
                else
                {
                    mapPats.Add(currentPlayerName, value);
                }

                Service.pluginConfig.Save();
                OnPatPat?.Invoke(value);
            }
        }

        public void IncCounter(string instigatorName)
        {
            if (GetPats(out int numPats))
            {
                if (numPats < int.MaxValue)
                {
                    numPats = Math.Max(1, numPats + 1);
                }

                SetPats(numPats);
            }

            if (mapPatsInZone.TryGetValue(instigatorName, out int numPatsInZone))
            {
                mapPatsInZone[instigatorName] = numPatsInZone + 1;
            }
            else
            {
                mapPatsInZone.Add(instigatorName, 1);
            }
        }

        private string GetCurrentPlayerName()
        {
            if (Service.clientState == null || Service.clientState.LocalPlayer == null || Service.clientState.LocalPlayer.Name == null)
            {
                return null;
            }

            return Service.clientState.LocalPlayer.Name.TextValue;
        }

        public (string, int) GetTopPatsInZone()
        {
            string maxPatsPlayer = null;
            int maxPats = 0;

            foreach (var kvp in mapPatsInZone)
            {
                if (kvp.Value > maxPats)
                {
                    maxPats = kvp.Value;
                    maxPatsPlayer = kvp.Key;
                }
            }

            return (maxPatsPlayer, maxPats);
        }

        public int GetPatsInCurrentZone(string instigatorName)
        {
            if (mapPatsInZone.TryGetValue(instigatorName, out int numPats))
            {
                return numPats;
            }

            return 0;
        }

        public void OnTerritoryChanged(ushort territoryId)
        {
            mapPatsInZone.Clear();
        }
    }
}
