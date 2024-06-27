using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using System;
using System.Linq;

namespace PatMe
{
    public class EmoteReaderHooks : IDisposable
    {
        public Action<GameObject, ushort> OnEmote;

        public delegate void OnEmoteFuncDelegate(ulong unk, ulong instigatorAddr, ushort emoteId, ulong targetId, ulong unk2);
        private readonly Hook<OnEmoteFuncDelegate> hookEmote;

        public bool IsValid = false;

        public EmoteReaderHooks()
        {
            try
            {
                hookEmote = Service.sigScanner.HookFromSignature<OnEmoteFuncDelegate>("48 89 5C 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC ?? 48 8B 02 4C 8B F1", OnEmoteDetour);
                hookEmote.Enable();

                IsValid = true;
            }
            catch (Exception ex)
            {
                Service.logger.Error(ex, "oh noes!");
            }
        }

        public void Dispose()
        {
            hookEmote?.Dispose();
            IsValid = false;
        }

        void OnEmoteDetour(ulong unk, ulong instigatorAddr, ushort emoteId, ulong targetId, ulong unk2)
        {
            // unk - some field of event framework singleton? doesn't matter here anyway
            // Service.logger.Info($"Emote >> unk:{unk:X}, instigatorAddr:{instigatorAddr:X}, emoteId:{emoteId}, targetId:{targetId:X}, unk2:{unk2:X}");

            if (Service.clientState.LocalPlayer != null)
            {
                if (targetId == Service.clientState.LocalPlayer.ObjectId)
                {
                    var instigatorOb = Service.objectTable.FirstOrDefault(x => (ulong)x.Address == instigatorAddr);
                    if (instigatorOb != null)
                    {
                        bool canCount = (instigatorOb.ObjectId != targetId);
#if DEBUG
                        canCount = true;
#endif 
                        if (canCount)
                        {
                            OnEmote?.Invoke(instigatorOb, emoteId);
                        }
                    }
                }
            }

            hookEmote.Original(unk, instigatorAddr, emoteId, targetId, unk2);
        }
    }
}
