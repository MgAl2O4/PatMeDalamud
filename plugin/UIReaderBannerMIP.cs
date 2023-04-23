using FFXIVClientStructs.FFXIV.Component.GUI;
using MgAl2O4.Utils;
using System;
using System.Collections.Generic;

namespace PatMe
{
    public class UIReaderBannerMIP
    {
        public const float UpdateInterval = 0.5f;

        private float updateTimeRemaining = 0.0f;
        private IntPtr cachedAddonPtr;

        private Dictionary<int, string> playerNames = new Dictionary<int, string>();

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
            var addonPtr = Service.gameGui.GetAddonByName("BannerMIP", 1);
            var addonBaseNode = (AtkUnitBase*)addonPtr;

            if (addonBaseNode == null || addonBaseNode->RootNode == null || !addonBaseNode->RootNode->IsVisible)
            {
                // reset when closed
                cachedAddonPtr = IntPtr.Zero;
                playerNames.Clear();
                return;
            }

            cachedAddonPtr = addonPtr;

            var level0 = GUINodeUtils.GetImmediateChildNodes(addonBaseNode->RootNode);
            var listRoot = GUINodeUtils.PickNode(level0, 2, 4);
            var listNodes = GUINodeUtils.GetImmediateChildNodes(listRoot);
            var collectsPlayerNames = playerNames.Count == 0;

            if (listNodes != null)
            {
                for (int idx = 0; idx < listNodes.Length; idx++)
                {
                    var innerList = GUINodeUtils.GetChildNode(listNodes[idx]);
                    if (innerList != null && innerList->IsVisible)
                    {
                        var nodeLastName = GUINodeUtils.PickChildNode(innerList, 20, 27);
                        var nodeFirstName = GUINodeUtils.PickChildNode(innerList, 21, 27);
                        var nodeCombined = GUINodeUtils.PickChildNode(innerList, 22, 27);

                        UpdatePlayerNames(idx, collectsPlayerNames, nodeLastName, nodeFirstName, nodeCombined);
                    }
                }
            }
        }

        private unsafe void UpdatePlayerNames(int entryIdx, bool collectsPlayerNames, AtkResNode* nodeLastName, AtkResNode* nodeFirstName, AtkResNode* nodeCombined)
        {
            if (nodeLastName == null || nodeFirstName == null || nodeCombined == null ||
                nodeLastName->Type != NodeType.Text || nodeFirstName->Type != NodeType.Text || nodeCombined->Type != NodeType.Text)
            {
                return;
            }

            var lastName = GUINodeUtils.GetNodeText(nodeLastName);
            if (lastName.Length == 0 || lastName.StartsWith("pats:"))
            {
                return;
            }

            var playerName = "";

            if (collectsPlayerNames)
            {
                var firstName = GUINodeUtils.GetNodeText(nodeFirstName);
                playerName = $"{firstName} {lastName}";
                playerNames.Add(entryIdx, playerName);
            }
            else
            {
                playerNames.TryGetValue(entryIdx, out playerName);
            }

            if (playerName.Length <= 1) // include separator
            {
                return;
            }

            var patCounter = Service.emoteCounters.Find(x => x.Name == EmoteConstants.PatName);
            uint numPats = patCounter != null ? patCounter.GetEmoteCounterInCurrentZone(playerName) : 0;

            if (numPats > 0)
            {
                ((AtkTextNode*)nodeLastName)->SetText($"pats: {numPats}");

                nodeCombined->Flags &= ~0x10; // hide
                nodeLastName->Flags |= 0x10; // show
                nodeFirstName->Flags |= 0x10; // show
            }
        }
    }
}
