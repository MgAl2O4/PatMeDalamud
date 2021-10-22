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
            bool hasChanges = false;

            hasChanges = ImGui.Checkbox("Show notify on reaching pat goals", ref showSpecialPats) || hasChanges;

            if (hasChanges)
            {
                Service.pluginConfig.showSpecialPats = showSpecialPats;
                Service.pluginConfig.Save();
            }
        }
    }
}
