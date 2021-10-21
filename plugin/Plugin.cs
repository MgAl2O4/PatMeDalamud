using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Text;
using Dalamud.Logging;
using Dalamud.Plugin;
using ImGuiScene;
using System;

namespace PatMe
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "Pat Me";

        private PluginUI pluginUI;
        private PatCounter patCounter;
        private EmoteReader emoteReader;

        private bool canUseHooks = true;

        public Plugin(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Service>();

            Service.plugin = this;

            Service.pluginConfig = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Service.pluginConfig.Initialize(pluginInterface);

            patCounter = new PatCounter();
            patCounter.OnPatPat += OnPatReward;

            pluginUI = new PluginUI();
            pluginUI.overlayImage = LoadEmbeddedImage("fan-kit-lala.png");

            Service.commandManager.AddHandler("/patme", new(OnCommand) { HelpMessage = "Show pat counter" });
            pluginInterface.UiBuilder.Draw += OnDraw;

            var readerHooks = canUseHooks ? new EmoteReaderHooks() : null;
            emoteReader = (readerHooks?.IsValid ?? false) ? readerHooks : new EmoteReaderChat();
            emoteReader.OnPetEmote += (instigator) => patCounter.IncCounter(instigator);
        }

        public void Dispose()
        {
            pluginUI.Dispose();
            emoteReader.Dispose();
            patCounter.Dispose();

            Service.commandManager.RemoveHandler("/patme");
        }

        private void OnCommand(string command, string args)
        {
            if (patCounter.GetPats(out int numPats))
            {
                Service.chatGui.PrintChat(new XivChatEntry() { Message = $"Pat counter: {numPats}", Type = XivChatType.SystemMessage });
            }
        }

        private void OnDraw()
        {
            pluginUI.Draw();
        }

        private void OnPatReward(int numPats)
        {
            // thresholds on: 5, 15, 25, 50, 75, ...
            bool isSpecial = (numPats < 25) ? (numPats == 5 || numPats == 15) : ((numPats % 25) == 0);
            if (isSpecial)
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
            else
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
