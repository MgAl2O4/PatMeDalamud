using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Gui.Toast;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using System.Collections.Generic;

namespace PatMe
{
    internal class Service
    {
        public static Plugin plugin;

        public static Configuration pluginConfig;
        public static SplashScreenUI splashScreen;
        public static List<EmoteCounter> emoteCounters;
        public static ICallGateProvider<string, ushort, string, uint, object> counterBroadcast;

        [PluginService]
        public static DalamudPluginInterface pluginInterface { get; private set; } = null!;

        [PluginService]
        public static CommandManager commandManager { get; private set; } = null!;

        [PluginService]
        public static FlyTextGui flyTextGui { get; private set; } = null!;

        [PluginService]
        public static ToastGui toastGui { get; private set; } = null!;

        [PluginService]
        public static ClientState clientState { get; private set; } = null!;

        [PluginService]
        public static ChatGui chatGui { get; private set; } = null!;

        [PluginService]
        public static SigScanner sigScanner { get; private set; } = null!;

        [PluginService]
        public static ObjectTable objectTable { get; private set; } = null!;

        [PluginService]
        public static Framework framework { get; private set; } = null!;

        [PluginService]
        public static GameGui gameGui { get; private set; } = null!;
    }
}
