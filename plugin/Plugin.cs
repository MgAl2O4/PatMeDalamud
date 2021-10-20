using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Logging;
using Dalamud.Plugin;
using ImGuiScene;
using System;

namespace PatMe
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "Pat Me";

        private readonly DalamudPluginInterface pluginInterface;
        private readonly CommandManager commandManager;
        private readonly FlyTextGui flyTextGui;
        private readonly ToastGui toastGui;
        private readonly ClientState clientState;
        private readonly ChatGui chatGui;

        private Configuration configuration { get; init; }
        private PluginUI pluginUI;

        private static readonly string[] patternPetEmote = { "gently pats you", "なでた", "streichelt dich sanft", "vous caresse" };

        public Plugin(DalamudPluginInterface pluginInterface, CommandManager commandManager, FlyTextGui flyTextGui, ToastGui toastGui, ClientState clientState, ChatGui chatGui)
        {
            this.pluginInterface = pluginInterface;
            this.commandManager = commandManager;
            this.flyTextGui = flyTextGui;
            this.toastGui = toastGui;
            this.clientState = clientState;
            this.chatGui = chatGui;

            configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            configuration.Initialize(pluginInterface);

            pluginUI = new PluginUI();
            pluginUI.overlayImage = LoadEmbeddedImage("fan-kit-lala.png");

            commandManager.AddHandler("/patme", new(OnCommand) { HelpMessage = "Show pat counter" });
            pluginInterface.UiBuilder.Draw += OnDraw;
            chatGui.ChatMessage += OnChatMessage;
        }

        private void OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (message != null && type == XivChatType.StandardEmote)
            {
                // pet emote payloads:
                // - instigator player, raw text (name), unknown, raw text (emote text)
                // - instigator player, raw text (name), unknown, icon, raw text (emote text)
                if (message.Payloads.Count >= 4 && 
                    message.Payloads[message.Payloads.Count - 1].Type == PayloadType.RawText)
                {
                    var textPayload = (message.Payloads[message.Payloads.Count - 1] as TextPayload);
                    var textPayloadContent = textPayload?.Text;

                    if (!string.IsNullOrEmpty(textPayloadContent))
                    {
                        foreach (var testStr in patternPetEmote)
                        {
                            if (textPayloadContent.Contains(testStr))
                            {
                                OnPatPat();
                                break;
                            }
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            pluginUI.Dispose();
            commandManager.RemoveHandler("/patme");
            chatGui.ChatMessage -= OnChatMessage;
        }

        private void OnCommand(string command, string args)
        {
            var playerName = GetCurrentPlayerName();
            if (playerName != null)
            {
                int numPats = configuration.GetPats(playerName);
                chatGui.PrintChat(new XivChatEntry() { Message = $"Pat counter: {numPats}", Type = XivChatType.SystemMessage });
            }
        }

        private void OnDraw()
        {
            pluginUI.Draw();
        }

        private void OnPatPat()
        {
            var playerName = GetCurrentPlayerName();
            if (playerName != null)
            {
                int numPats = configuration.GetPats(playerName);
                if (numPats < int.MaxValue)
                {
                    numPats = Math.Max(1, numPats + 1);
                }

                configuration.SetPats(playerName, numPats);

                bool reachedThreshold = IsSpecialPatPat(numPats);
                if (reachedThreshold)
                {
                    pluginUI.Show();
                    toastGui?.ShowQuest($"{numPats} PATS!", new QuestToastOptions
                    {
                        Position = QuestToastPosition.Centre,
                        DisplayCheckmark = true,
                        IconId = 0,
                        PlaySound = true
                    });
                }
                else
                {
                    flyTextGui?.AddFlyText(FlyTextKind.NamedCriticalDirectHit, 0, (uint)numPats, 0, "PAT", " ", 0xff00ff00, 0);
                }
            }
        }

        private bool IsSpecialPatPat(int value)
        {
            // thresholds on: 5, 15, 25, 50, 75, ...
            if (value < 25)
            {
                return (value == 5) || (value == 15);
            }

            return (value % 25) == 0;
        }

        private string GetCurrentPlayerName()
        {
            if (clientState == null || clientState.LocalPlayer == null || clientState.LocalPlayer.Name == null)
            {
                return null;
            }

            return clientState.LocalPlayer.Name.TextValue;
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

                    resultImage = pluginInterface.UiBuilder.LoadImage(contentBytes);
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
