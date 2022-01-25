using Dalamud.Game;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Text;
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
        private UIReaderVoteMvp uiReaderVoteMvp;
        private PluginWindowConfig windowConfig;
        private PatCountUI patCountUI;

        public Plugin(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Service>();

            Service.plugin = this;

            Service.pluginConfig = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Service.pluginConfig.Initialize(pluginInterface);

            Service.patCounter = new PatCounter();
            Service.patCounter.OnPatPat += OnPatReward;

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
            emoteReader.OnPetEmote += (instigator) => Service.patCounter.IncCounter(instigator);

            Service.framework.Update += Framework_Update;
            Service.clientState.TerritoryChanged += ClientState_TerritoryChanged;
        }

        private void Framework_Update(Framework framework)
        {
            float deltaSeconds = (float)framework.UpdateDelta.TotalSeconds;
            uiReaderVoteMvp.Tick(deltaSeconds);
        }

        private void ClientState_TerritoryChanged(object sender, ushort e)
        {
            Service.patCounter.OnTerritoryChanged(e);
        }

        public void Dispose()
        {
            pluginUI.Dispose();

            emoteReader.Dispose();
            Service.patCounter.Dispose();
            windowSystem.RemoveAllWindows();

            Service.commandManager.RemoveHandler("/patme");
            Service.commandManager.RemoveHandler("/patcount");
            Service.framework.Update -= Framework_Update;
            Service.clientState.TerritoryChanged -= ClientState_TerritoryChanged;
        }

        private void OnCommand(string command, string args)
        {
            if (command == "/patme")
            {
                if (Service.patCounter.GetPats(out int numPats))
                {
                    Service.chatGui.PrintChat(new XivChatEntry() { Message = $"Pat counter: {numPats}", Type = XivChatType.SystemMessage });

                    var (maxPlayerName, maxCount) = Service.patCounter.GetTopPatsInZone();
                    if (maxCount > 0)
                    {
                        string countDesc = (maxCount == 1) ? "1 pat" : $"{maxCount} pats";
                        Service.chatGui.PrintChat(new XivChatEntry() { Message = $"♥ {maxPlayerName}: {countDesc}", Type = XivChatType.SystemMessage });
                    }
                }
            }
            else if (command == "/patcount")
            {
                patCountUI.Toggle();
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

        private void OnPatReward(int numPats)
        {
            // thresholds on: 5, 15, 25, 50, 75, ...
            bool isSpecial = (numPats < 25) ? (numPats == 5 || numPats == 15) : ((numPats % 25) == 0);
            if (isSpecial && Service.pluginConfig.showSpecialPats)
            {
                pluginUI.Show();

                Service.toastGui?.ShowQuest($"{numPats} PATS!", new QuestToastOptions
                {
                    Position = QuestToastPosition.Centre,
                    DisplayCheckmark = true,
                    IconId = 0,
                    PlaySound = true
                });
            }
            else if (Service.pluginConfig.showFlyText)
            {
                Service.flyTextGui?.AddFlyText(FlyTextKind.NamedCriticalDirectHit, 0, (uint)numPats, 0, "PAT", " ", 0xff00ff00, 0);
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
