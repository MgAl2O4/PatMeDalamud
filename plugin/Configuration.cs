using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace PatMe
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public Dictionary<string, int> mapPats { get; set; } = new();

        [NonSerialized]
        private DalamudPluginInterface pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public int GetPats(string playerName)
        {
            if (mapPats.TryGetValue(playerName, out int numPats))
            {
                return Math.Max(0, numPats);
            }

            return 0;
        }

        public void SetPats(string playerName, int value)
        {
            if (mapPats.ContainsKey(playerName))
            {
                mapPats[playerName] = value;
            }
            else
            {
                mapPats.Add(playerName, value);
            }

            pluginInterface?.SavePluginConfig(this);
        }
    }
}
