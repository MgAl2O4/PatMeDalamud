using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;

namespace PatMe
{
    public class PluginWindowConfig : Window, IDisposable
    {
        public PluginWindowConfig() : base("Pat Config")
        {
            IsOpen = false;

            SizeConstraints = new WindowSizeConstraints() { MinimumSize = new Vector2(100, 0), MaximumSize = new Vector2(300, 3000) };
            Flags = ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoScrollbar;
        }

        public void Dispose()
        {
        }

        public override void Draw()
        {
            bool showCounterUI = Service.pluginConfig.showCounterUI;
            bool lockCounterUI = Service.pluginConfig.lockCounterUI;
            bool collapseCounterUI = Service.pluginConfig.collapseCounterUI;
            bool hasChangesCounterUI = false;

            ImGui.Text("Counter UI:");
            hasChangesCounterUI = ImGui.Checkbox("Show emote counter on screen", ref showCounterUI) || hasChangesCounterUI;
            hasChangesCounterUI = ImGui.Checkbox("Lock counter position", ref lockCounterUI) || hasChangesCounterUI;
            hasChangesCounterUI = ImGui.Checkbox("Auto collapse counter", ref collapseCounterUI) || hasChangesCounterUI;

            if (hasChangesCounterUI)
            {
                Service.pluginConfig.showCounterUI = showCounterUI;
                Service.pluginConfig.lockCounterUI = lockCounterUI;
                Service.pluginConfig.collapseCounterUI = collapseCounterUI;

                Service.plugin.OnCounterWindowConfigChanged();
            }

            bool showSpecialPats = Service.pluginConfig.showSpecialPats;
            bool showProgressNotify = Service.pluginConfig.showProgressNotify;
            bool showFlyText = Service.pluginConfig.showFlyText;
            bool showFlyTextNames = Service.pluginConfig.showFlyTextNames;
            bool bHasChangesRewards = false;

            ImGui.Separator();
            ImGui.Text("Rewards:");
            bHasChangesRewards = ImGui.Checkbox("Use splash screen", ref showSpecialPats) || bHasChangesRewards;
            bHasChangesRewards = ImGui.Checkbox("Use progress notify", ref showProgressNotify) || bHasChangesRewards;
            bHasChangesRewards = ImGui.Checkbox("Use fly text", ref showFlyText) || bHasChangesRewards;
            bHasChangesRewards = ImGui.Checkbox("Include names in fly text", ref showFlyTextNames) || bHasChangesRewards;

            if (bHasChangesRewards)
            {
                Service.pluginConfig.showSpecialPats = showSpecialPats;
                Service.pluginConfig.showProgressNotify = showProgressNotify;
                Service.pluginConfig.showFlyText = showFlyText;
                Service.pluginConfig.showFlyTextNames = showFlyTextNames;
            }

            bool canTrackDotes = Service.pluginConfig.canTrackDotes;
            bool canTrackHugs = Service.pluginConfig.canTrackHugs;
            bool canTrackHearts = Service.pluginConfig.canTrackHearts;
            bool canTrackFlowers = Service.pluginConfig.canTrackFlowers;
            bool bHasChangesEmotes = false;

            ImGui.Separator();
            ImGui.Text("Emotes:");
            bHasChangesEmotes = ImGui.Checkbox("Track: dote", ref canTrackDotes) || bHasChangesEmotes;
            bHasChangesEmotes = ImGui.Checkbox("Track: hug & embrace", ref canTrackHugs) || bHasChangesEmotes;
            bHasChangesEmotes = ImGui.Checkbox("Track: heart", ref canTrackHearts) || bHasChangesEmotes;
            bHasChangesEmotes = ImGui.Checkbox("Track: petals", ref canTrackFlowers) || bHasChangesEmotes;

            if (bHasChangesEmotes)
            {
                Service.pluginConfig.canTrackDotes = canTrackDotes;
                Service.pluginConfig.canTrackHugs = canTrackHugs;
                Service.pluginConfig.canTrackHearts = canTrackHearts;
                Service.pluginConfig.canTrackFlowers = canTrackFlowers;

                var doteCounter = Service.emoteCounters.Find(x => x.Name == EmoteConstants.DoteName);
                if (doteCounter != null)
                {
                    doteCounter.isActive = canTrackDotes;
                }

                var hugCounter = Service.emoteCounters.Find(x => x.Name == EmoteConstants.HugName);
                if (hugCounter != null)
                {
                    hugCounter.isActive = canTrackHugs;
                }
                
                var heartCounter = Service.emoteCounters.Find(x => x.Name == EmoteConstants.HeartName);
                if (heartCounter != null)
                {
                    heartCounter.isActive = canTrackHearts;
                }
                
                var flowerCounter = Service.emoteCounters.Find(x => x.Name == EmoteConstants.FlowerName);
                if (flowerCounter != null)
                {
                    flowerCounter.isActive = canTrackFlowers;
                }
            }

            if (hasChangesCounterUI || bHasChangesRewards || bHasChangesEmotes)
            {
                Service.pluginConfig.Save();
            }
        }
    }
}
