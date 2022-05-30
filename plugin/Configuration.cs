using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace PatMe
{
    [Serializable]
    public class EmoteDataConfig
    {
        public string OwnerName { get; set; }
        public int EmoteId { get; set; } = 0;
        public int Counter { get; set; } = 0;

        public bool IsValid() { return !string.IsNullOrEmpty(OwnerName) && (EmoteId > 0) && (Counter > 0); }
    }

    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        private static int VersionLatest = 1;

        public int Version { get; set; } = VersionLatest;

        public Dictionary<string, int> mapPats { internal get; set; } = new();

        public List<EmoteDataConfig> Emotes { get; set; } = new();

        public bool showSpecialPats { get; set; } = true;
        public bool showFlyText { get; set; } = true;
        public bool showCounterUI { get; set; } = false;

        [NonSerialized]
        private DalamudPluginInterface pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;

            switch (Version)
            {
                case 0:
                    MigrateVersion0();
                    break;

                default: break;
            }

            Emotes.RemoveAll(x => !x.IsValid());
        }

        public void Save()
        {
            Emotes.RemoveAll(x => !x.IsValid());

            pluginInterface.SavePluginConfig(this);
        }

        public EmoteDataConfig FindOrAddEmote(string ownerName, int emoteId)
        {
            if (string.IsNullOrEmpty(ownerName) || emoteId <= 0)
            {
                return null;
            }

            EmoteDataConfig emoteOb = Emotes.Find(x => (x.EmoteId == emoteId && x.OwnerName.Equals(ownerName, StringComparison.OrdinalIgnoreCase)));
            if (emoteOb != null)
            {
                return emoteOb;
            }

            emoteOb = new EmoteDataConfig() { OwnerName = ownerName, EmoteId = emoteId };
            Emotes.Add(emoteOb);
            return emoteOb;
        }

        private void MigrateVersion0()
        {
            foreach (var kvp in mapPats)
            {
                var emoteConfig = new EmoteDataConfig() { OwnerName = kvp.Key, EmoteId = EmoteReaderHooks.petEmoteId, Counter = kvp.Value };
                Emotes.Add(emoteConfig);
            }

            mapPats.Clear();
            Version = VersionLatest;
        }
    }
}
