using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Services;
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
        public static ICommandManager commandManager { get; private set; } = null!;

        [PluginService]
        public static IFlyTextGui flyTextGui { get; private set; } = null!;

        [PluginService]
        public static IToastGui toastGui { get; private set; } = null!;

        [PluginService]
        public static IClientState clientState { get; private set; } = null!;

        [PluginService]
        public static IChatGui chatGui { get; private set; } = null!;

        [PluginService]
        public static IGameInteropProvider sigScanner { get; private set; } = null!;

        [PluginService]
        public static IObjectTable objectTable { get; private set; } = null!;

        [PluginService]
        public static IFramework framework { get; private set; } = null!;

        [PluginService]
        public static IGameGui gameGui { get; private set; } = null!;

        [PluginService]
        public static IPluginLog logger { get; private set; } = null!;
    }
}
