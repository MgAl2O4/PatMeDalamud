using Dalamud.Game.ClientState.Objects.SubKinds;
using System;
using System.Collections.Generic;

namespace PatMe
{
    public class EmoteInstigatorCounter
    {
        public class InstigatorData : IEquatable<InstigatorData>
        {
            public string Name;
            public uint HomeWorld;

            public bool Equals(InstigatorData other)
            {
                return HomeWorld == other.HomeWorld && Name == other.Name;
            }

            public static InstigatorData Create(PlayerCharacter instigator)
            {
                return new InstigatorData() { Name = instigator.Name.ToString(), HomeWorld = instigator.HomeWorld.Id };
            }
        }

        private Dictionary<InstigatorData, uint> mapPlayerCounter = new();

        public void Clear()
        {
            mapPlayerCounter.Clear();
        }

        public void Increment(PlayerCharacter instigator) => Increment(InstigatorData.Create(instigator));

        public void Increment(InstigatorData key)
        {
            if (mapPlayerCounter.TryGetValue(key, out var counter))
            {
                mapPlayerCounter[key] = counter + 1;
            }
            else
            {
                mapPlayerCounter.Add(key, 1);
            }
        }

        public uint GetCounter(PlayerCharacter instigator)
        {
            var key = InstigatorData.Create(instigator);
            if (mapPlayerCounter.TryGetValue(key, out var counter))
            {
                return counter;
            }

            return 0;
        }

        public uint GetCounter(string playerName)
        {
            foreach (var kvp in mapPlayerCounter)
            {
                if (kvp.Key.Name == playerName)
                {
                    return kvp.Value;
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
                if (kvp.Value > score)
                {
                    score = kvp.Value;
                    name = kvp.Key.Name;
                }
            }

            return score > 0;
        }
    }
}
