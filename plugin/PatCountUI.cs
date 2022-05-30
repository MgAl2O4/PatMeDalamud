using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;

namespace PatMe
{
    public class PatCountUI : Window, IDisposable
    {
        public PatCountUI() : base("Pat Count")
        {
            IsOpen = false;

            Flags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize;
            RespectCloseHotkey = false;
        }

        public void Dispose()
        {
        }

        public override void Draw()
        {
            int pats = Service.patCounter.GetCounter();

            ImGui.Text($"Head pats: {pats}");
        }
    }
}
