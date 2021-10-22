﻿using Dalamud.Hooking;
using Dalamud.Logging;
using System;
using System.Linq;

namespace PatMe
{
    public class EmoteReaderHooks : EmoteReader
    {
        public const ushort petEmoteId = 105;  // TODO: read from lumina?

#if DEBUG
        private delegate void OnMagicDetourDelegate(uint a1, uint a2, uint a3, uint a4, uint a5, uint a6, int a7, int a8, ulong a9, byte a10);
        private readonly Hook<OnMagicDetourDelegate> hookMagic;
        private bool useMagicDetour = false;
#endif // DEBUG

        public delegate void OnEmoteFuncDelegate(ulong unk, ulong instigatorAddr, ushort emoteId, ulong targetId, ulong emoteData);
        private readonly Hook<OnEmoteFuncDelegate> hookEmote;

        public bool IsValid = false;

        public EmoteReaderHooks()
        {
            try
            {
#if DEBUG
                // Aireil's hook (net packet processing?)
                // - OnMagicFuncDelegate(uint a1, uint a2, uint a3, uint a4, uint a5, uint a6, int a7, int a8, Int64 a9, byte a10)
                // - a1 = addr of instigator
                // - a2 = 290 for emote (fixed? only in 5.58?)
                // - a3 = 105 (emote id for /pet)
                // - a9 = object id of target
                // 
                // keep for reference, don't use, i'm too dumb to understand that function and following emote calls puffs a9 into oblivion
                // note: enabling this will break emote hook below!
                if (useMagicDetour)
                {
                    var magicFuncPtr = Service.sigScanner.ScanText("E8 ?? ?? ?? ?? 0F B7 0B 83 E9 64");

                    hookMagic = new Hook<OnMagicDetourDelegate>(magicFuncPtr, OnMagicDetour);
                    hookMagic.Enable();
                }
#endif // DEBUG

                // func header signature: 48 89 5c 24 18 48 89 7c 24 20 41 56 48 83 ec 20 48 8b 02 4c 8b f1
                var emoteFuncPtr = Service.sigScanner.ScanText("e8 ?? ?? ?? ?? 48 8d 8f d0 01 00 00 4c 8b ce");
                hookEmote = new Hook<OnEmoteFuncDelegate>(emoteFuncPtr, OnEmoteDetour);
                hookEmote.Enable();

                IsValid = true;
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "oh noes!");
            }
        }

        public override void Dispose()
        {
#if DEBUG
            hookMagic?.Dispose();
#endif // DEBUG

            hookEmote?.Dispose();
            IsValid = false;
        }

#if DEBUG
        void OnMagicDetour(uint a1, uint a2, uint a3, uint a4, uint a5, uint a6, int a7, int a8, ulong a9, byte a10)
        {
            if (a2 == 290 || a3 == 105)
            {
                PluginLog.Log($"OnMagicDetour>> a1:{a1}, a2:{a2}, a3:{a3}, a9:{a9}");
            }
        }
#endif // DEBUG

        void OnEmoteDetour(ulong unk, ulong instigatorAddr, ushort emoteId, ulong targetId, ulong emoteData)
        {
            // unk - some field of event framework singleton? doesn't matter here anyway
            // PluginLog.Log($"Emote >> unk:{unk:X}, instigatorAddr:{instigatorAddr:X}, emoteId:{emoteId}, targetId:{targetId:X}, emoteData:{emoteData:X}");

            if (emoteId == petEmoteId && Service.clientState.LocalPlayer != null)
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
                            var instigatorName = (instigatorOb != null) ? instigatorOb.Name.ToString() : "??";

                            OnPetEmote?.Invoke(instigatorName);
                        }
                    }
                }
            }
        }
    }
}