
using System;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace PatMe
{
    public class PatCountUI : Window, IDisposable
    {
        private bool visible = false;

        public bool Visible
        {
            get { return visible; }
            set { this.visible = value; }
        }

        public PatCountUI() : base("Pat Count")
        {
            IsOpen = false;
        }

        public void Dispose()
        {
        }

        public override void Draw()
        {
            if (!Visible)
            {
                return;
            }

            Service.patCounter.GetPats(out var pats);


            if (ImGui.Begin("Pat Count", ref this.visible, flags: ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoScrollbar))
            {
                ImGui.Text($"Head pats: {pats}");
            }

            
        }
    }
}
