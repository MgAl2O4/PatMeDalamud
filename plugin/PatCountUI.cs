
using System;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace PatMe.plugin
{
    public class PatCountUI : Window, IDisposable
    {
        public PatCountUI() : base("Pat Count")
        {
        }

        public void Dispose()
        {
        }

        public override void Draw()
        {
            if (Service.pluginConfig.showPatCount)
            {
                Service.patCounter.GetPats(out var pats);

                ImGui.Begin("Pat Count", flags: ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoScrollbar);
                ImGui.Text($"Head pats: {pats}");
            }
        }
    }
}
