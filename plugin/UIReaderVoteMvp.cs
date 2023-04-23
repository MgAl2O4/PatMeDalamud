using FFXIVClientStructs.FFXIV.Component.GUI;
using MgAl2O4.Utils;
using System;

namespace PatMe
{
    public class UIReaderVoteMvp
    {
        public const float UpdateInterval = 0.5f;

        private float updateTimeRemaining = 0.0f;
        private IntPtr cachedAddonPtr;

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
            var addonBaseNode = (AtkUnitBase*)addonPtr;

            if (addonBaseNode == null || addonBaseNode->RootNode == null || !addonBaseNode->RootNode->IsVisible)
            {
                // reset when closed
                cachedAddonPtr = IntPtr.Zero;
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
                                textNode->SetText(playerName);
                            }
                            else if (numPats > 1)
                            {
                                playerName += $" [ {numPats} pats ]";
                                textNode->SetText(playerName);
                            }
                        }
                    }
                }
            }
        }
    }
}
