using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System.Collections.Generic;

namespace PatMe
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "Pat Me";

        private readonly WindowSystem windowSystem = new("PatMe");
        private EmoteReaderHooks emoteReader;
        private UIReaderVoteMvp uiReaderVoteMvp;
        private UIReaderBannerMIP uiReaderBannerMIP;
        private PluginWindowConfig windowConfig;
        private PluginWindowCounter windowCounters;

        [PluginService] internal static IDalamudPluginInterface pluginInterface { get; private set; } = null!;

        public Plugin()
        {
            pluginInterface.Create<Service>();

            Service.plugin = this;
            Service.pluginInterface = pluginInterface;
            Service.pluginConfig = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

            CreateEmoteCounters();

            Service.emoteDataManager = new EmoteDataManager();
            Service.splashScreen = new SplashScreenUI();

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
            pluginInterface.UiBuilder.OpenMainUi += () => windowCounters.Toggle();

            emoteReader = new EmoteReaderHooks();
            emoteReader.OnEmote += (instigator, emoteId) => Service.emoteDataManager.OnEmote(instigator as IPlayerCharacter, emoteId);

            Service.framework.Update += Framework_Update;
            Service.clientState.TerritoryChanged += ClientState_TerritoryChanged;
            Service.clientState.Login += ClientState_Login;
            Service.clientState.Logout += ClientState_Logout;

            OnCounterWindowConfigChanged();
        }

        private void Framework_Update(IFramework framework)
        {
            float deltaSeconds = (float)framework.UpdateDelta.TotalSeconds;
            uiReaderVoteMvp.Tick(deltaSeconds);
            uiReaderBannerMIP.Tick(deltaSeconds);
        }

        private void ClientState_Login()
        {
            Service.emoteDataManager.OnLogin();
            OnCounterWindowConfigChanged();
        }

        private void ClientState_Logout(int type, int code)
        {
            Service.emoteDataManager.OnLogout();
            windowCounters.IsOpen = false;
        }

        private void ClientState_TerritoryChanged(ushort e)
        {
            Service.emoteCounters.ForEach(counter => counter.OnTerritoryChanged());
        }

        public void Dispose()
        {
            Service.splashScreen.Dispose();

            emoteReader.Dispose();
            Service.emoteDataManager.Dispose();
            uiReaderVoteMvp.Dispose();
            uiReaderBannerMIP.Dispose();
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
                descUI = "Headpats",
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
            
            var heartCounter = new EmoteCounter()
            {
                descSingular = "heart",
                descPlural = "hearts",
                descUI = "Hearts",
            };
            heartCounter.Initialize(EmoteConstants.HeartName, new int[] { EmoteConstants.HeartEmoteID });
            heartCounter.rewards = rewardsDefault;
            heartCounter.isActive = Service.pluginConfig.canTrackHearts;
            Service.emoteCounters.Add(heartCounter);
            
            var flowerCounter = new EmoteCounter()
            {
                descSingular = "petals",
                descPlural = "petals",
                descUI = "Petals",
            };
            flowerCounter.Initialize(EmoteConstants.FlowerName, new int[] { EmoteConstants.FlowerEmoteID });
            flowerCounter.rewards = rewardsDefault;
            flowerCounter.isActive = Service.pluginConfig.canTrackFlowers;
            Service.emoteCounters.Add(flowerCounter);
        }

        private void OnCommandListInChat(string command, string args)
        {
            Service.emoteDataManager.ConditionalInitialize();

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

        private void OnCommandTestImage(string command, string args)
        {
            Service.splashScreen.Show();
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
    }
}
