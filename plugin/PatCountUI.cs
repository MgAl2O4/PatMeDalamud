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
            ImGui.Text($"{Service.patCounter.uiDesc}: {pats}");

            // add more counters if they want to be there
            foreach (var counter in Service.plugin.emoteCounters)
            {
                if (counter != null && counter != Service.patCounter && !string.IsNullOrEmpty(counter.uiDesc))
                {
                    int numEmotes = counter.GetCounter();
                    if (numEmotes > 0)
                    {
                        ImGui.Text($"{counter.uiDesc}: {numEmotes}");
                    }
                }
            }
        }
    }
}
