using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Hooking;
using System;
using System.Linq;

namespace PatMe
{
    public class EmoteReaderHooks : IDisposable
    {
        public Action<IPlayerCharacter, ushort>? OnEmote;

        public delegate void OnEmoteFuncDelegate(ulong unk, ulong instigatorAddr, ushort emoteId, ulong targetId, ulong unk2);
        private readonly Hook<OnEmoteFuncDelegate>? hookEmote;

        public bool IsValid = false;

        public EmoteReaderHooks()
        {
            try
            {
                hookEmote = Service.sigScanner.HookFromSignature<OnEmoteFuncDelegate>("40 53 56 41 54 41 57 48 83 EC ?? 48 8B 02", OnEmoteDetour);
                hookEmote.Enable();

                IsValid = true;
            }
            catch (Exception ex)
            {
                Service.logger.Error(ex, "failed to hook emotes!");
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
                if (targetId == Service.clientState.LocalPlayer.GameObjectId)
                {
                    var instigatorOb = Service.objectTable.FirstOrDefault(x => (ulong)x.Address == instigatorAddr) as IPlayerCharacter;
                    if (instigatorOb != null)
                    {
                        bool canCount = (instigatorOb.ObjectIndex != targetId);
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

            hookEmote?.Original(unk, instigatorAddr, emoteId, targetId, unk2);
        }
    }
}
