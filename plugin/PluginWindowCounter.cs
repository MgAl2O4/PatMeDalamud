using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace PatMe
{
    public class PluginWindowCounter : Window, IDisposable
    {
        private Vector4 colorName = new(0.75f, 0.75f, 0.75f, 1.0f);
        private Vector4 colorValue = new(1.0f, 1.0f, 1.0f, 1.0f);
        private uint colorUpdateFlash = 0x008000;

        private List<float> updateFlashRemaining = new();
        private List<uint> updateFlashLastValue = new();
        private float updateFlashDuration = 1.0f;

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

        private void UpdateAnimations()
        {
            if (updateFlashRemaining.Count != Service.emoteCounters.Count || updateFlashLastValue.Count != Service.emoteCounters.Count)
            {
                updateFlashRemaining.Clear();
                updateFlashLastValue.Clear();

                for (int idx = 0; idx < Service.emoteCounters.Count; idx++)
                {
                    updateFlashRemaining.Add(-1.0f);
                    updateFlashLastValue.Add(Service.emoteCounters[idx].Value);
                }
            }

            for (int idx = 0; idx < Service.emoteCounters.Count; idx++)
            {
                if (updateFlashLastValue[idx] != Service.emoteCounters[idx].Value)
                {
                    updateFlashLastValue[idx] = Service.emoteCounters[idx].Value;
                    updateFlashRemaining[idx] = updateFlashDuration;
                }
            }

            var deltaTime = ImGui.GetIO().DeltaTime;
            for (int idx = 0; idx < updateFlashRemaining.Count; idx++)
            {
                if (updateFlashRemaining[idx] >= 0.0f)
                {
                    updateFlashRemaining[idx] -= deltaTime;
                }
            }
        }

        public override void Draw()
        {
            UpdateAnimations();

            if (ImGui.BeginTable("##counters", 2, ImGuiTableFlags.None))
            {
                for (int idx = 0; idx < Service.emoteCounters.Count; idx++)
                {
                    var counter = Service.emoteCounters[idx];
                    if (counter == null || !counter.isActive || string.IsNullOrEmpty(counter.descUI))
                    {
                        continue;
                    }

                    if (counter.Value == 0 && counter.Name != EmoteConstants.PatName)
                    {
                        // only pats can stay with 0
                        continue;
                    }

                    float updateFlashAlpha = (updateFlashRemaining[idx] > 0.0f) ? (updateFlashRemaining[idx] / updateFlashDuration) : 0.0f;
                    uint cellBgColor = colorUpdateFlash | (uint)(updateFlashAlpha * 255) << 24;

                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.TextColored(colorName, $" {counter.descUI}:");
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, cellBgColor);
                    ImGui.TableSetColumnIndex(1);
                    ImGui.TextColored(colorValue, $" {counter.Value} ");
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, cellBgColor);
                }

                ImGui.EndTable();
            }
        }
    }
}
