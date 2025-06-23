using Dalamud.Configuration;
using System;
using System.Collections.Generic;

namespace PatMe
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        private static int VersionLatest = 2;

        public int Version { get; set; } = VersionLatest;

        // data source, version: 0 (deprecated)
        public Dictionary<string, int> mapPats { internal get; set; } = new();

        // data source, version: 1 (deprecated)
        public List<EmoteDataConfig> Emotes { internal get; set; } = new();

        // data source, version: 2
        public List<EmoteOwnerDB> EmoteData { get; set; } = new();

        public bool showSpecialPats { get; set; } = true;
        public bool showProgressNotify { get; set; } = true;
        public bool showFlyText { get; set; } = true;
        public bool showFlyTextNames { get; set; } = true;
        public bool showCounterUI { get; set; } = false;
        public bool lockCounterUI { get; set; } = false;
        public bool collapseCounterUI { get; set; } = true;
        public bool canTrackDotes { get; set; } = true;
        public bool canTrackHugs { get; set; } = true;
        public bool canTrackHearts { get; set; } = true;
        public bool canTrackFlowers { get; set; } = true;

        public void Initialize()
        {
            var needsResave = (Version != VersionLatest);
            switch (Version)
            {
                case 0: EmoteData = EmoteDBMigration.CreateFromVer0(mapPats); break;
                case 1: EmoteData = EmoteDBMigration.CreateFromVer1(Emotes); break;
                default: break;
            }

            if (needsResave)
            {
                Version = VersionLatest;
                Save();
            }
        }

        public void Save()
        {
            Service.pluginInterface.SavePluginConfig(this);
        }
    }
}
