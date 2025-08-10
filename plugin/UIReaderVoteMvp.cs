using FFXIVClientStructs.FFXIV.Component.GUI;
using MgAl2O4.Utils;
using System;
using System.Collections.Generic;

namespace PatMe
{
    public class UIReaderVoteMvp : IDisposable
    {
        public const float UpdateInterval = 0.5f;

        private float updateTimeRemaining = 0.0f;
        private IntPtr cachedAddonPtr;
        private List<NodeTextWrapper> textWrappers = new();

        public void Tick(float deltaSeconds)
        {
            updateTimeRemaining -= deltaSeconds;
            if (updateTimeRemaining < 0.0f)
            {
                updateTimeRemaining = UpdateInterval;
                UpdateAddon();
            }
        }

        private unsafe void UpdateAddon()
        {
            var addonPtr = Service.gameGui.GetAddonByName("VoteMvp", 1);
            var addonBaseNode = (AtkUnitBase*)addonPtr.Address;

            if (addonBaseNode == null || addonBaseNode->RootNode == null || !addonBaseNode->RootNode->IsVisible())
            {
                // reset when closed
                cachedAddonPtr = IntPtr.Zero;
                FreeTextWrappers();

                return;
            }

            // update once
            if (cachedAddonPtr == addonPtr)
            {
                return;
            }

            cachedAddonPtr = addonPtr;

            var childNodesL0 = GUINodeUtils.GetImmediateChildNodes(addonBaseNode->RootNode);
            if (childNodesL0 != null)
            {
                foreach (var nodeL0 in childNodesL0)
                {
                    var nodeL1 = GUINodeUtils.PickChildNode(nodeL0, 3, 7);
                    if (nodeL1 != null && nodeL1->Type == NodeType.Text)
                    {
                        var textNode = (AtkTextNode*)nodeL1;
                        var playerName = textNode->NodeText.ToString();

                        if (!playerName.Contains("pats ]") && !playerName.Contains("pat ]"))
                        {
                            var patCounter = Service.emoteCounters.Find(x => x.Name == EmoteConstants.PatName);
                            uint numPats = patCounter != null ? patCounter.GetEmoteCounterInCurrentZone(playerName) : 0;

                            if (numPats == 1)
                            {
                                playerName += " [ 1 pat ]";

                                var textWrapper = new NodeTextWrapper(playerName);
                                textWrappers.Add(textWrapper);
                                textNode->SetText(textWrapper.Get());
                            }
                            else if (numPats > 1)
                            {
                                playerName += $" [ {numPats} pats ]";

                                var textWrapper = new NodeTextWrapper(playerName);
                                textWrappers.Add(textWrapper);
                                textNode->SetText(textWrapper.Get());
                            }
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            FreeTextWrappers();
        }

        private void FreeTextWrappers()
        {
            foreach (var wrapper in textWrappers)
            {
                wrapper.Free();
            }
            textWrappers.Clear();
        }
    }
}
