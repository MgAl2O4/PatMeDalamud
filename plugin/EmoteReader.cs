using System;

namespace PatMe
{
    public abstract class EmoteReader : IDisposable
    {
        public delegate void PetEmoteDelegate(string instigatorName);

        public PetEmoteDelegate OnPetEmote;

        public abstract void Dispose();
    }
}
