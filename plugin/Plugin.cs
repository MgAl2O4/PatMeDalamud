using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.SubKinds;
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
        private EmoteReaderHooks emoteReader;
        private EmoteDataManager emoteDataManager;
        private UIReaderVoteMvp uiReaderVoteMvp;
        private UIReaderBannerMIP uiReaderBannerMIP;
        private PluginWindowConfig windowConfig;
        private PluginWindowCounter windowCounters;

        public Plugin(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Service>();

            Service.plugin = this;

            Service.pluginConfig = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Service.pluginConfig.Initialize(pluginInterface);

            CreateEmoteCounters();

            emoteDataManager = new EmoteDataManager();
            emoteDataManager.Initialize(); // config and counters must be ready

            Service.splashScreen = new SplashScreenUI();
            Service.splashScreen.overlayImage = LoadEmbeddedImage("fan-kit-lala.png");

            uiReaderVoteMvp = new UIReaderVoteMvp();
            uiReaderBannerMIP = new UIReaderBannerMIP();

            windowConfig = new PluginWindowConfig();
            windowSystem.AddWindow(windowConfig);

            windowCounters = new PluginWindowCounter();
            windowSystem.AddWindow(windowCounters);

            Service.commandManager.AddHandler("/patme", new(OnCommandListInChat) { HelpMessage = "Show counters in chat" });
            Service.commandManager.AddHandler("/patcount", new(OnCommandCounterWindow) { HelpMessage = "Toggle counter UI" });
            pluginInterface.UiBuilder.Draw += OnDraw;
            pluginInterface.UiBuilder.OpenConfigUi += OnOpenConfig;

            emoteReader = new EmoteReaderHooks();
            emoteReader.OnEmote += (instigator, emoteId) => emoteDataManager.OnEmote(instigator as PlayerCharacter, emoteId);

            Service.counterBroadcast = pluginInterface.GetIpcProvider<string, ushort, string, uint, object>("patMeEmoteCounter");
            Service.framework.Update += Framework_Update;
            Service.clientState.TerritoryChanged += ClientState_TerritoryChanged;
            Service.clientState.Login += ClientState_Login;
            Service.clientState.Logout += ClientState_Logout;

            OnCounterWindowConfigChanged();
        }

        private void Framework_Update(Framework framework)
        {
            float deltaSeconds = (float)framework.UpdateDelta.TotalSeconds;
            uiReaderVoteMvp.Tick(deltaSeconds);
            uiReaderBannerMIP.Tick(deltaSeconds);
        }

        private void ClientState_Login(object sender, EventArgs e)
        {
            emoteDataManager.OnLogin();
            OnCounterWindowConfigChanged();
        }

        private void ClientState_Logout(object sender, EventArgs e)
        {
            emoteDataManager.OnLogout();
            windowCounters.IsOpen = false;
        }

        private void ClientState_TerritoryChanged(object sender, ushort e)
        {
            Service.emoteCounters.ForEach(counter => counter.OnTerritoryChanged());
        }

        public void Dispose()
        {
            Service.splashScreen.Dispose();

            emoteReader.Dispose();
            emoteDataManager.Dispose();
            windowSystem.RemoveAllWindows();

            Service.commandManager.RemoveHandler("/patme");
            Service.commandManager.RemoveHandler("/patcount");
            Service.framework.Update -= Framework_Update;
            Service.clientState.TerritoryChanged -= ClientState_TerritoryChanged;
            Service.clientState.Login -= ClientState_Login;
            Service.clientState.Logout -= ClientState_Logout;
        }

        private void CreateEmoteCounters()
        {
            Service.emoteCounters = new();
            var rewardsDefault = new List<IEmoteReward>() { new RewardProgressNotify(), new RewardFlyText() };

            var patCounter = new EmoteCounter()
            {
                descSingular = "pat",
                descPlural = "pats",
                descUI = "Head pats",
            };
            patCounter.Initialize(EmoteConstants.PatName, new int[] { EmoteConstants.PatEmoteID });
            patCounter.rewards = new List<IEmoteReward>() { new RewardSplashScreen(), new RewardProgressNotify(), new RewardFlyTextPat() };
            Service.emoteCounters.Add(patCounter);

            var doteCounter = new EmoteCounter()
            {
                descSingular = "dote",
                descPlural = "dotes",
                descUI = "Dotes",
            };
            doteCounter.Initialize(EmoteConstants.DoteName, new int[] { EmoteConstants.DoteEmoteID, EmoteConstants.DoteEmoteID2 });
            doteCounter.rewards = rewardsDefault;
            doteCounter.isActive = Service.pluginConfig.canTrackDotes;
            Service.emoteCounters.Add(doteCounter);

            var hugCounter = new EmoteCounter()
            {
                descSingular = "hug",
                descPlural = "hugs",
                descUI = "Hugs",
            };
            hugCounter.Initialize(EmoteConstants.HugName, new int[] { EmoteConstants.HugEmoteID, EmoteConstants.EmbraceEmoteID });
            hugCounter.rewards = rewardsDefault;
            hugCounter.isActive = Service.pluginConfig.canTrackHugs;
            Service.emoteCounters.Add(hugCounter);
        }

        private void OnCommandListInChat(string command, string args)
        {
            foreach (var counter in Service.emoteCounters)
            {
                if (counter == null || string.IsNullOrEmpty(counter.descSingular) || !counter.isActive)
                {
                    continue;
                }

                uint numEmotes = counter.Value;
                if (numEmotes == 0 && counter.Name != EmoteConstants.PatName)
                {
                    continue;
                }

                var useName = counter.descSingular[0].ToString().ToUpper() + counter.descSingular.Substring(1);
                Service.chatGui.Print($"{useName} counter: {numEmotes}");

                if (counter.GetTopEmotes(out string playerName, out uint score))
                {
                    string countDesc = (score == 1) ? counter.descSingular : counter.descPlural;
                    Service.chatGui.Print($"♥ {playerName}: {score} {countDesc}");
                }

                if (counter.GetTopEmotesInCurrentZone(out string playerNameZone, out uint scoreZone))
                {
                    if (playerNameZone != playerName || score != scoreZone)
                    {
                        string countDesc = (scoreZone == 1) ? counter.descSingular : counter.descPlural;
                        Service.chatGui.Print($"\uE0BB {playerNameZone}: {scoreZone} {countDesc}");
                    }
                }
            }
        }

        private void OnCommandCounterWindow(string command, string args)
        {
            windowCounters.Toggle();
        }

        private void OnDraw()
        {
            Service.splashScreen.Draw();
            windowSystem.Draw();
        }

        private void OnOpenConfig()
        {
            windowConfig.IsOpen = true;
        }

        public void OnCounterWindowConfigChanged()
        {
            windowCounters.UpdateConfig();
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
