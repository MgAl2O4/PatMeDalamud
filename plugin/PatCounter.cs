using System;

namespace PatMe
{
    public class PatCounter : IDisposable
    {
        public Action<int> OnPatPat;

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

            if (currentPlayerName != null)
            {
                if (Service.pluginConfig.mapPats.TryGetValue(currentPlayerName, out numPats))
                {
                    return true;
                }
            }

            numPats = 0;
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
