using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;

namespace PatMe
{
    public class PluginWindowCounter : Window, IDisposable
    {
        public PluginWindowCounter() : base("Pat Count")
        {
            IsOpen = false;
            RespectCloseHotkey = false;

            UpdateConfig();
        }

        public void Dispose()
        {
        }

        public void UpdateConfig()
        {
            Flags = ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.AlwaysAutoResize |
                ImGuiWindowFlags.NoFocusOnAppearing |
                ImGuiWindowFlags.NoNav;

            if (Service.pluginConfig.lockCounterUI)
            {
                Flags |= ImGuiWindowFlags.NoMove |
                    ImGuiWindowFlags.NoDocking |
                    ImGuiWindowFlags.NoMouseInputs;
            }

            if (Service.pluginConfig.showCounterUI && Service.clientState.IsLoggedIn)
            {
                IsOpen = true;
            }
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
