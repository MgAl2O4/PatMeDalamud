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
            var patCounter = Service.emoteCounters.Find(x => x.Name == EmoteConstants.PatName);
            if (patCounter != null)
            {
                ImGui.Text($"{patCounter.descUI}: {patCounter.Value}");
            }

            // add more counters if they want to be there
            foreach (var counter in Service.emoteCounters)
            {
                if (counter == null || counter == patCounter || !counter.isActive || counter.Value == 0 || !string.IsNullOrEmpty(counter.descUI))
                {
                    continue;
                }

                ImGui.Text($"{counter.descUI}: {counter.Value}");
            }
        }
    }
}
