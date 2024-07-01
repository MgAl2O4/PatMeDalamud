using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System.Collections.Generic;

namespace PatMe
{
    internal class Service
    {
        public static Plugin plugin = null!;
        public static IDalamudPluginInterface pluginInterface = null!;
        public static Configuration pluginConfig = null!;

        public static SplashScreenUI splashScreen = null!;
        public static List<EmoteCounter> emoteCounters = [];

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
        public static ITextureProvider textureProvider { get; private set; } = null!;

        [PluginService]
        public static IPluginLog logger { get; private set; } = null!;
    }
}
