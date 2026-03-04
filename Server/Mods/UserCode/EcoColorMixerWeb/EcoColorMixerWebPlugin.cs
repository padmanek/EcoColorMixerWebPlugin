namespace Eco.Mods.UserCode
{
    using Eco.Core.Plugins.Interfaces;
    using Eco.Core.Utils;
    using Eco.Shared.Localization;
    using Eco.Shared.Logging;

    /// <summary>
    /// Adds an Eco web UI plugin entry in the left sidebar and serves a powder color calculator page
    /// from Eco's built-in web server under /plugins/EcoColorMixerWebPlugin/index.html.
    /// </summary>
    // Important: implement IModKitPlugin so Eco.PluginManager discovers and instantiates this mod plugin.
    public sealed class EcoColorMixerWebPlugin : IModKitPlugin, IInitializablePlugin, IWebPlugin
    {
        public string GetStatus() => "Ready";

        public string GetCategory() => "Web";

        public void Initialize(TimedTask timer)
        {
            Log.WriteLineLoc($"[EcoColorMixerWebPlugin] Initialize called. Static path: {this.GetStaticFilesPath()}, Index: {this.GetPluginIndexUrl()}");
        }

        public LocString GetMenuTitle() => Localizer.DoStr("Powder Color Calculator");

        // Plugin.vue builds iframe src as `/plugins/${PluginIndexUrl}` so this should be path relative to /plugins.
        public string GetPluginIndexUrl() => $"{this.GetType().Name}/index.html";

        public string GetFontAwesomeIcon() => "fa fa-fw fa-eyedropper";

        // Relative to this plugin assembly location as resolved by EcoPluginUtils.UseEcoWebPluginStaticContent.
        public string GetStaticFilesPath() => "EcoColorMixerWeb";

        public string GetEmbeddedResourceNamespace() => null;
    }
}
