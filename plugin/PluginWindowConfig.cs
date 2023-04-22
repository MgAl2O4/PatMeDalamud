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
            bool hasChangesCounterUI = false;

            hasChangesCounterUI = ImGui.Checkbox("Show emote counter on screen", ref showCounterUI) || hasChangesCounterUI;
            hasChangesCounterUI = ImGui.Checkbox("Lock emote counter position", ref lockCounterUI) || hasChangesCounterUI;

            if (hasChangesCounterUI)
            {
                Service.pluginConfig.showCounterUI = showCounterUI;
                Service.pluginConfig.lockCounterUI = lockCounterUI;

                Service.plugin.OnCounterWindowConfigChanged();
            }

            bool showSpecialPats = Service.pluginConfig.showSpecialPats;
            bool showFlyText = Service.pluginConfig.showFlyText;
            bool bHasChangesRewards = false;

            ImGui.Separator();
            bHasChangesRewards = ImGui.Checkbox("Use splash screen", ref showSpecialPats) || bHasChangesRewards;
            bHasChangesRewards = ImGui.Checkbox("Use fly text counters", ref showFlyText) || bHasChangesRewards;

            if (bHasChangesRewards)
            {
                Service.pluginConfig.showSpecialPats = showSpecialPats;
                Service.pluginConfig.showFlyText = showFlyText;
            }

            bool canTrackDotes = Service.pluginConfig.canTrackDotes;
            bool canTrackHugs = Service.pluginConfig.canTrackHugs;
            bool bHasChangesEmotes = false;

            ImGui.Separator();
            bHasChangesEmotes = ImGui.Checkbox("Track emote: dote", ref canTrackDotes) || bHasChangesEmotes;
            bHasChangesEmotes = ImGui.Checkbox("Track emote: hug", ref canTrackHugs) || bHasChangesEmotes;

            if (bHasChangesEmotes)
            {
                Service.pluginConfig.canTrackDotes = canTrackDotes;
                Service.pluginConfig.canTrackHugs = canTrackHugs;

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
            }

            if (hasChangesCounterUI || bHasChangesRewards || bHasChangesEmotes)
            {
                Service.pluginConfig.Save();
            }
        }
    }
}
