using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace PatMe
{
    public class PluginWindowCounter : Window, IDisposable
    {
        class CounterUIData
        {
            public uint lastValue;
            public float flashRemaining;
            public bool canShow;
            public bool expandWhenUpdated;
        }

        private readonly Vector4 colorName = new(0.75f, 0.75f, 0.75f, 1.0f);
        private readonly Vector4 colorValue = new(1.0f, 1.0f, 1.0f, 1.0f);
        private const uint colorUpdateFlash = 0x008000;
        private const uint colorCollapseTimer = 0xff808000;

        private List<CounterUIData> counterUI = new();
        private const float updateFlashDuration = 1.0f;
        private const float collapseTimeDuration = 15.0f;
        private float collapseTimeRemaining = -1.0f;

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
                Flags |= ImGuiWindowFlags.NoMove;

                if (!Service.pluginConfig.collapseCounterUI)
                {
                    Flags |= ImGuiWindowFlags.NoMouseInputs;
                }
            }

            if (Service.pluginConfig.showCounterUI)
            {
                if (Service.clientState.IsLoggedIn)
                {
                    IsOpen = true;

                    if (Service.pluginConfig.collapseCounterUI)
                    {
                        collapseTimeRemaining = collapseTimeDuration;
                    }
                }
            }
            else
            {
                IsOpen = false;
            }
        }

        private void UpdateCounterData()
        {
            Service.emoteDataManager.ConditionalInitialize();

            if (counterUI.Count != Service.emoteCounters.Count)
            {
                counterUI.Clear();

                for (int idx = 0; idx < Service.emoteCounters.Count; idx++)
                {
                    counterUI.Add(new CounterUIData()
                    {
                        lastValue = Service.emoteCounters[idx].Value,
                        flashRemaining = -1.0f,
                    });
                }
            }

            for (int idx = 0; idx < Service.emoteCounters.Count; idx++)
            {
                var counter = Service.emoteCounters[idx];
                var uiData = counterUI[idx];

                if (counter == null || !counter.isActive || string.IsNullOrEmpty(counter.descUI))
                {
                    uiData.canShow = false;
                    continue;
                }

                var isPatCounter = counter.Name == EmoteConstants.PatName;
                if (!isPatCounter)
                {
                    if (counter.Value == 0)
                    {
                        uiData.canShow = false;
                        continue;
                    }

                    uiData.expandWhenUpdated = true;
                }

                if (uiData.lastValue != counter.Value)
                {
                    uiData.lastValue = counter.Value;
                    uiData.flashRemaining = updateFlashDuration;
                }

                uiData.canShow = true;
            }
        }

        private void UpdateAnimations()
        {
            var deltaTime = ImGui.GetIO().DeltaTime;
            var showExpanded = ImGui.IsWindowHovered();
            int numCountersToShow = 0;

            foreach (var uiData in counterUI)
            {
                if (uiData.flashRemaining > 0.0f)
                {
                    uiData.flashRemaining -= deltaTime;

                    showExpanded = showExpanded || uiData.expandWhenUpdated;
                }

                numCountersToShow += uiData.canShow ? 1 : 0;
            }

            if (numCountersToShow >= 2)
            {
                if (showExpanded)
                {
                    collapseTimeRemaining = collapseTimeDuration;
                }

                if (collapseTimeRemaining >= 0.0f)
                {
                    collapseTimeRemaining -= deltaTime;
                }
            }
            else
            {
                collapseTimeRemaining = -1.0f;
            }
        }

        public override void Draw()
        {
            UpdateCounterData();
            UpdateAnimations();

            var drawCollapseAnim = Service.pluginConfig.collapseCounterUI && collapseTimeRemaining > 0.0f;
            if (drawCollapseAnim)
            {
                var collapseAlpha = Math.Ceiling(collapseTimeRemaining) / collapseTimeDuration;
                var globalScale = Dalamud.Interface.Utility.ImGuiHelpers.GlobalScale;
                var startPos = ImGui.GetCursorPos() + ImGui.GetWindowPos();
                var endPos = startPos + new Vector2(ImGui.GetContentRegionAvail().X * (float)collapseAlpha, 2.0f * globalScale);

                ImGui.GetWindowDrawList().AddRectFilled(startPos, endPos, colorCollapseTimer);
            }

            var hideCollapsedTimers = Service.pluginConfig.collapseCounterUI && collapseTimeRemaining < 0.0f;
            if (ImGui.BeginTable("##counters", 2, ImGuiTableFlags.None))
            {
                for (int idx = 0; idx < counterUI.Count; idx++)
                {
                    var uiData = counterUI[idx];
                    if (!uiData.canShow)
                    {
                        continue;
                    }

                    if (uiData.expandWhenUpdated && hideCollapsedTimers)
                    {
                        continue;
                    }

                    var counter = Service.emoteCounters[idx];
                    float updateFlashAlpha = (uiData.flashRemaining > 0.0f) ? (uiData.flashRemaining / updateFlashDuration) : 0.0f;
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
