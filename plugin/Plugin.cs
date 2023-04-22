using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Gui.Toast;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using Dalamud.Plugin;
using ImGuiScene;
using System;

namespace PatMe
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "Pat Me";

        private readonly WindowSystem windowSystem = new("PatMe");
        private PluginUI pluginUI;
        private EmoteReaderHooks emoteReader;
        private EmoteDataManager emoteDataManager;
        private UIReaderVoteMvp uiReaderVoteMvp;
        private UIReaderBannerMIP uiReaderBannerMIP;
        private PluginWindowConfig windowConfig;
        private PatCountUI patCountUI;

        public Plugin(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Service>();

            Service.plugin = this;

            Service.pluginConfig = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Service.pluginConfig.Initialize(pluginInterface);

            CreateEmoteCounters();

            emoteDataManager = new EmoteDataManager();
            emoteDataManager.Initialize(); // config and counters must be ready

            pluginUI = new PluginUI();
            pluginUI.overlayImage = LoadEmbeddedImage("fan-kit-lala.png");

            uiReaderVoteMvp = new UIReaderVoteMvp();
            uiReaderBannerMIP = new UIReaderBannerMIP();

            windowConfig = new PluginWindowConfig();
            windowSystem.AddWindow(windowConfig);

            patCountUI = new PatCountUI();
            windowSystem.AddWindow(patCountUI);

            Service.commandManager.AddHandler("/patme", new(OnCommand) { HelpMessage = "Show pat counter" });
            Service.commandManager.AddHandler("/patcount", new(OnCommand) { HelpMessage = "Show persistent pat counter" });
            pluginInterface.UiBuilder.Draw += OnDraw;
            pluginInterface.UiBuilder.OpenConfigUi += OnOpenConfig;

            emoteReader = new EmoteReaderHooks();
            emoteReader.OnEmote += (instigator, emoteId) => emoteDataManager.OnEmote(instigator as PlayerCharacter, emoteId);

            if (Service.pluginConfig.showCounterUI)
            {
                patCountUI.IsOpen = true;
            }

            Service.counterBroadcast = pluginInterface.GetIpcProvider<string, ushort, string, uint, object>("patMeEmoteCounter");
            Service.framework.Update += Framework_Update;
        }

        private void Framework_Update(Framework framework)
        {
            float deltaSeconds = (float)framework.UpdateDelta.TotalSeconds;
            uiReaderVoteMvp.Tick(deltaSeconds);
            uiReaderBannerMIP.Tick(deltaSeconds);
        }

        public void Dispose()
        {
            pluginUI.Dispose();

            emoteReader.Dispose();
            emoteDataManager.Dispose();
            windowSystem.RemoveAllWindows();

            Service.commandManager.RemoveHandler("/patme");
            Service.commandManager.RemoveHandler("/patcount");
            Service.framework.Update -= Framework_Update;
        }

        private void CreateEmoteCounters()
        {
            Service.emoteCounters = new();

            var patCounter = new EmoteCounter()
            {
                descSingular = "pat",
                descPlural = "pats",
                descUI = "Head pats",
            };
            patCounter.Initialize(EmoteConstants.PatName, new int[] { EmoteConstants.PatEmoteID });
            patCounter.OnChanged += (num) => OnEmoteReward(patCounter, num);
            Service.emoteCounters.Add(patCounter);

            var doteCounter = new EmoteCounter()
            {
                descSingular = "dote",
                descPlural = "dotes",
                descUI = "Ranged pats",
            };
            doteCounter.Initialize(EmoteConstants.DoteName, new int[] { EmoteConstants.DoteEmoteID, EmoteConstants.DoteEmoteID2 });
            doteCounter.OnChanged += (num) => OnEmoteReward(doteCounter, num);
            doteCounter.isActive = Service.pluginConfig.canTrackDotes;
            Service.emoteCounters.Add(doteCounter);
        }

        private void OnCommand(string command, string args)
        {
            if (command == "/patme")
            {
                var patCounter = Service.emoteCounters.Find(x => x.Name == EmoteConstants.PatName);
                if (patCounter != null)
                {
                    DescribeCounter(patCounter, false);
                }

                foreach (var counter in Service.emoteCounters)
                {
                    if (counter != patCounter)
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
            if (counter == null || string.IsNullOrEmpty(counter.descSingular) || !counter.isActive)
            {
                return;
            }

            uint numEmotes = counter.Value;
            if (numEmotes <= 0 && hideEmpty)
            {
                return;
            }

            var useName = counter.descSingular[0].ToString().ToUpper() + counter.descSingular.Substring(1);
            Service.chatGui.Print($"{useName} counter: {numEmotes}");

            var (maxPlayerName, maxCount) = counter.GetTopEmotesInZone();
            if (maxCount > 0)
            {
                string countDesc = (maxCount == 1) ? counter.descSingular : counter.descPlural;
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

        private void OnEmoteReward(EmoteCounter counter, uint numEmotes)
        {
            // thresholds on: 5, 15, 25, 50, 75, ...
            bool isSpecial = (numEmotes < 25) ? (numEmotes == 5 || numEmotes == 15) : ((numEmotes % 25) == 0);
            if (isSpecial && Service.pluginConfig.showSpecialPats)
            {
                // pats get special rewards.
                if (counter.Name == EmoteConstants.PatName)
                {
                    pluginUI.Show();
                }

                var useDesc = counter.descPlural.ToUpper();
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
                var useDesc = counter.descSingular.ToUpper();
                Service.flyTextGui?.AddFlyText(FlyTextKind.NamedCriticalDirectHit, 0, (uint)numEmotes, 0, useDesc, " ", 0xff00ff00, 0, 0);
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
