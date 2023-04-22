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
            bool showSpecialPats = Service.pluginConfig.showSpecialPats;
            bool showFlyText = Service.pluginConfig.showFlyText;
            bool showCounterUI = Service.pluginConfig.showCounterUI;
            bool canTrackDotes = Service.pluginConfig.canTrackDotes;
            bool canTrackHugs = Service.pluginConfig.canTrackHugs;
            bool hasChanges = false;

            hasChanges = ImGui.Checkbox("Show emote counter on screen", ref showCounterUI) || hasChanges;
            hasChanges = ImGui.Checkbox("Use splash screen", ref showSpecialPats) || hasChanges;
            hasChanges = ImGui.Checkbox("Use fly text counters", ref showFlyText) || hasChanges;

            ImGui.Separator();
            hasChanges = ImGui.Checkbox("Track emote: dote", ref canTrackDotes) || hasChanges;
            hasChanges = ImGui.Checkbox("Track emote: hug", ref canTrackHugs) || hasChanges;

            if (showCounterUI != Service.pluginConfig.showCounterUI)
            {
                Service.plugin.OnShowCounterConfigChanged(showCounterUI);
            }

            if (hasChanges)
            {
                Service.pluginConfig.showSpecialPats = showSpecialPats;
                Service.pluginConfig.showFlyText = showFlyText;
                Service.pluginConfig.showCounterUI = showCounterUI;
                Service.pluginConfig.canTrackDotes = canTrackDotes;
                Service.pluginConfig.canTrackHugs = canTrackHugs;

                Service.pluginConfig.Save();

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
        }
    }
}
