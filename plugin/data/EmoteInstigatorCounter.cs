using Dalamud.Game.ClientState.Objects.SubKinds;
using PatMe.plugin.data;
using System;
using System.Collections.Generic;

namespace PatMe
{
    public class EmoteInstigatorCounter
    {
        public class InstigatorData : IEquatable<InstigatorData>
        {
            public string Name = string.Empty;
            public uint HomeWorld;

            public bool Equals(InstigatorData? other)
            {
                return (other != null) && (HomeWorld == other.HomeWorld) && (Name == other.Name);
            }

            public static InstigatorData Create(IPlayerCharacter instigator)
            {
                return new InstigatorData() { Name = instigator.Name.ToString(), HomeWorld = instigator.HomeWorld.Id };
            }
        }

        private Dictionary<string, InstigatorWithCount> mapPlayerCounter = new();

        public void Clear()
        {
            mapPlayerCounter.Clear();
        }

        public void Increment(IPlayerCharacter instigator) => Increment(InstigatorData.Create(instigator));

        public void Increment(InstigatorData key)
        {
            if (mapPlayerCounter.TryGetValue(key.Name, out var counter))
            {
                counter.count++;
                mapPlayerCounter[key.Name] = counter;
            }
            else
            {
                Service.logger.Debug("key {0} not found, adding one.", key);
                mapPlayerCounter.Add(key.Name, new InstigatorWithCount(key,1));
            }
        }

        public uint GetCounter(IPlayerCharacter instigator)
        {
            var key = InstigatorData.Create(instigator);
            if (mapPlayerCounter.TryGetValue(key.Name, out var counter))
            {
                return counter.count;
            }

            return 0;
        }

        public uint GetCounter(string playerName)
        {
            foreach (var kvp in mapPlayerCounter)
            {
                if (kvp.Value.data.Name == playerName)
                {
                    return kvp.Value.count;
                }
            }

            return 0;
        }

        public bool GetHighestScore(out string name, out uint score)
        {
            name = string.Empty;
            score = 0;

            foreach (var kvp in mapPlayerCounter)
            {
                Service.logger.Debug("name: {0}: {1}", kvp.Value.data.Name,kvp.Value);
                if (kvp.Value.count > score)
                {
                    score = kvp.Value.count;
                    name = kvp.Value.data.Name;
                }
            }

            return score > 0;
        }
    }
}
