using Dalamud.Game;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Gui.Toast;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using Dalamud.Plugin;
using ImGuiScene;
using System;
using System.Collections.Generic;

namespace PatMe
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "Pat Me";

        private readonly WindowSystem windowSystem = new("PatMe");
        private PluginUI pluginUI;
        private EmoteReaderHooks emoteReader;
        private UIReaderVoteMvp uiReaderVoteMvp;
        private PluginWindowConfig windowConfig;
        private PatCountUI patCountUI;

        public readonly List<EmoteCounter> emoteCounters = new();

        public Plugin(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Service>();

            Service.plugin = this;

            Service.pluginConfig = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Service.pluginConfig.Initialize(pluginInterface);

            Service.patCounter = new EmoteCounter()
            {
                counterEmoteId = EmoteReaderHooks.petEmoteId,
                counterDesc = "pat",
                counterDescPlural = "pats",
                uiDesc = "Head pats",
            };
            Service.patCounter.OnChanged += (num) => OnEmoteReward(Service.patCounter, num);
            emoteCounters.Add(Service.patCounter);

            // two different emote ids?
            Service.doteCounter = new EmoteCounter()
            {
                counterEmoteId = 146,
                triggerEmoteIds = new int[] { 146, 147 },
                counterDesc = "dote",
                counterDescPlural = "dotes",
                uiDesc = "Ranged pats",
            };
            Service.doteCounter.OnChanged += (num) => OnEmoteReward(Service.doteCounter, num);
            Service.doteCounter.isActive = Service.pluginConfig.canTrackDotes;
            emoteCounters.Add(Service.doteCounter);

            pluginUI = new PluginUI();
            pluginUI.overlayImage = LoadEmbeddedImage("fan-kit-lala.png");

            uiReaderVoteMvp = new UIReaderVoteMvp();

            windowConfig = new PluginWindowConfig();
            windowSystem.AddWindow(windowConfig);

            patCountUI = new PatCountUI();
            windowSystem.AddWindow(patCountUI);

            Service.commandManager.AddHandler("/patme", new(OnCommand) { HelpMessage = "Show pat counter" });
            Service.commandManager.AddHandler("/patcount", new(OnCommand) { HelpMessage = "Show persistent pat counter" });
            pluginInterface.UiBuilder.Draw += OnDraw;
            pluginInterface.UiBuilder.OpenConfigUi += OnOpenConfig;

            emoteReader = new EmoteReaderHooks();
            emoteReader.OnEmote += (instigator, emoteId) => emoteCounters.ForEach(x => x.OnEmote(instigator, emoteId));

            Service.framework.Update += Framework_Update;
            Service.clientState.TerritoryChanged += ClientState_TerritoryChanged;
            Service.clientState.Logout += ClientState_Logout;

            if (Service.pluginConfig.showCounterUI)
            {
                patCountUI.IsOpen = true;
            }
        }

        private void Framework_Update(Framework framework)
        {
            float deltaSeconds = (float)framework.UpdateDelta.TotalSeconds;
            uiReaderVoteMvp.Tick(deltaSeconds);
        }

        private void ClientState_TerritoryChanged(object sender, ushort e)
        {
            emoteCounters.ForEach(x => x.OnTerritoryChanged());
        }

        private void ClientState_Logout(object sender, EventArgs e)
        {
            emoteCounters.ForEach(x => x.OnLogout());
        }

        public void Dispose()
        {
            pluginUI.Dispose();

            emoteReader.Dispose();
            windowSystem.RemoveAllWindows();

            Service.commandManager.RemoveHandler("/patme");
            Service.commandManager.RemoveHandler("/patcount");
            Service.framework.Update -= Framework_Update;
            Service.clientState.TerritoryChanged -= ClientState_TerritoryChanged;
            Service.clientState.Logout -= ClientState_Logout;
        }

        private void OnCommand(string command, string args)
        {
            if (command == "/patme")
            {
                DescribeCounter(Service.patCounter, false);
                foreach (var counter in emoteCounters)
                {
                    if (counter != Service.patCounter)
                    {
                        DescribeCounter(counter);
                    }
                }
            }
            else if (command == "/patcount")
            {
                patCountUI.Toggle();
            }
        }

        private void DescribeCounter(EmoteCounter counter, bool hideEmpty = true)
        {
            if (counter == null || string.IsNullOrEmpty(counter.counterDesc) || !counter.isActive)
            {
                return;
            }

            int numEmotes = counter.GetCounter();
            if (numEmotes <= 0 && hideEmpty)
            {
                return;
            }

            var useName = counter.counterDesc[0].ToString().ToUpper() + counter.counterDesc.Substring(1);
            Service.chatGui.Print($"{useName} counter: {numEmotes}");

            var (maxPlayerName, maxCount) = counter.GetTopEmotesInZone();
            if (maxCount > 0)
            {
                string countDesc = (maxCount == 1) ? counter.counterDesc : counter.counterDescPlural;
                Service.chatGui.Print($"♥ {maxPlayerName}: {maxCount} {countDesc}");
            }
        }

        private void OnDraw()
        {
            pluginUI.Draw();
            windowSystem.Draw();
        }

        private void OnOpenConfig()
        {
            windowConfig.IsOpen = true;
        }

        public void OnShowCounterConfigChanged(bool wantsUI)
        {
            patCountUI.IsOpen = wantsUI;
        }

        private void OnEmoteReward(EmoteCounter counter, int numEmotes)
        {
            // thresholds on: 5, 15, 25, 50, 75, ...
            bool isSpecial = (numEmotes < 25) ? (numEmotes == 5 || numEmotes == 15) : ((numEmotes % 25) == 0);
            if (isSpecial && Service.pluginConfig.showSpecialPats)
            {
                // pats get special rewards.
                if (counter == Service.patCounter)
                {
                    pluginUI.Show();
                }

                var useDesc = counter.counterDescPlural.ToUpper();
                Service.toastGui?.ShowQuest($"{numEmotes} {useDesc}!", new QuestToastOptions
                {
                    Position = QuestToastPosition.Centre,
                    DisplayCheckmark = true,
                    IconId = 0,
                    PlaySound = true
                });
            }
            else if (Service.pluginConfig.showFlyText)
            {
                var useDesc = counter.counterDesc.ToUpper();
                Service.flyTextGui?.AddFlyText(FlyTextKind.NamedCriticalDirectHit, 0, (uint)numEmotes, 0, useDesc, " ", 0xff00ff00, 0);
            }
        }

        private TextureWrap LoadEmbeddedImage(string name)
        {
            TextureWrap resultImage = null;
            try
            {
                var myAssembly = GetType().Assembly;
                var myAssemblyName = myAssembly.GetName().Name;
                var resourceName = $"{myAssemblyName}.assets.{name}";

                var resStream = myAssembly.GetManifestResourceStream(resourceName);
                if (resStream != null && resStream.Length > 0)
                {
                    var contentBytes = new byte[(int)resStream.Length];
                    resStream.Read(contentBytes, 0, contentBytes.Length);

                    resultImage = Service.pluginInterface.UiBuilder.LoadImage(contentBytes);
                    resStream.Close();
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "failed to load overlay image");
            }

            return resultImage;
        }
    }
}
