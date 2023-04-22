using System;
using System.Collections.Generic;

namespace PatMe
{
    [Serializable]
    public class EmoteOwnerDB
    {
        public string Name { get; set; }
        public ulong CID { get; set; }
        public List<EmoteCounterDB> Counters { get; set; } = new();

        [NonSerialized]
        public bool isFromConfig = false;

        public bool IsOwnerMatching(EmoteOwnerDB other) => IsOwnerMatching(other.CID, other.Name);

        public bool IsOwnerMatching(ulong otherCID, string otherName)
        {
            // match by CID if known
            if (CID != 0 && otherCID != 0)
            {
                return CID == otherCID;
            }

            // fallback to matching by name (migrated or manually editted data)
            return string.Equals(Name, otherName, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Serializable]
    public class EmoteCounterDB
    {
        public string Name { get; set; }
        public uint Value { get; set; } = 0;
    }

    // deprecated, version 1
    [Serializable]
    public class EmoteDataConfig
    {
        public string OwnerName { get; set; }
        public int EmoteId { get; set; } = 0;
        public int Counter { get; set; } = 0;

        public bool IsValid() { return !string.IsNullOrEmpty(OwnerName) && (EmoteId > 0) && (Counter > 0); }
    }
}
