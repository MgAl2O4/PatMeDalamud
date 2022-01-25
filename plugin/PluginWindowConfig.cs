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
            bool hasChanges = false;

            hasChanges = ImGui.Checkbox("Show notify on reaching pat goals", ref showSpecialPats) || hasChanges;
            hasChanges = ImGui.Checkbox("Show fly text counter on each emote", ref showFlyText) || hasChanges;

            if (hasChanges)
            {
                Service.pluginConfig.showSpecialPats = showSpecialPats;
                Service.pluginConfig.showFlyText = showFlyText;

                Service.pluginConfig.Save();
            }
        }
    }
}
