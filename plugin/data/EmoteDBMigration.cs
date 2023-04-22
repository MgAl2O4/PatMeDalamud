using System;
using System.Collections.Generic;

namespace PatMe
{
    public class EmoteDBMigration
    {
        public static List<EmoteOwnerDB> CreateFromVer0(Dictionary<string, int> mapPats)
        {
            var result = new List<EmoteOwnerDB>();

            foreach (var kvp in mapPats)
            {
                var playerData = new EmoteOwnerDB() { Name = kvp.Key };
                var emoteCounter = new EmoteCounterDB() { Name = EmoteConstants.PatName, Value = (uint)Math.Max(kvp.Value, 0) };

                playerData.Counters.Add(emoteCounter);
                result.Add(playerData);
            }

            return result;
        }

        public static List<EmoteOwnerDB> CreateFromVer1(List<EmoteDataConfig> emoteCounters)
        {
            var mapDataByName = new Dictionary<string, EmoteOwnerDB>();
            foreach (var kvp in emoteCounters)
            {
                var playerData = new EmoteOwnerDB() { Name = kvp.OwnerName };
                mapDataByName.TryAdd(kvp.OwnerName, playerData);
            }

            foreach (var kvp in emoteCounters)
            {
                if (!mapDataByName.TryGetValue(kvp.OwnerName, out EmoteOwnerDB playerData))
                {
                    continue;
                }

                switch (kvp.EmoteId)
                {
                    case EmoteConstants.PatEmoteID: playerData.Counters.Add(new EmoteCounterDB() { Name = EmoteConstants.PatName, Value = (uint)Math.Max(kvp.Counter, 0) }); break;
                    case EmoteConstants.DoteEmoteID: playerData.Counters.Add(new EmoteCounterDB() { Name = EmoteConstants.DoteName, Value = (uint)Math.Max(kvp.Counter, 0) }); break;
                    default: break; // custom stuff? not supported in migration
                }

                mapDataByName.TryAdd(kvp.OwnerName, playerData);
            }

            var result = new List<EmoteOwnerDB>();
            foreach (var kvp in mapDataByName)
            {
                if (kvp.Value.Counters.Count > 0)
                {
                    result.Add(kvp.Value);
                }
            }

            return result;
        }
    }
}
