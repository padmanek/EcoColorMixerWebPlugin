namespace Eco.Mods.UserCode
{
    using System;
    using System.IO;
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
        private const string StaticFolderName = "EcoColorMixerWeb";

        public string GetStatus() => "Ready";

        public string GetCategory() => "Web";

        public void Initialize(TimedTask timer)
        {
            this.EnsureStaticContentFallback();
            Log.WriteLineLoc($"[EcoColorMixerWebPlugin] Initialize called. Static path: {this.GetStaticFilesPath()}, Index: {this.GetPluginIndexUrl()}");
        }

        public LocString GetMenuTitle() => Localizer.DoStr("Powder Color Calculator");

        // Plugin.vue builds iframe src as `/plugins/${PluginIndexUrl}` so this should be path relative to /plugins.
        public string GetPluginIndexUrl() => $"{this.GetType().Name}/index.html";

        public string GetFontAwesomeIcon() => "fa fa-fw fa-eyedropper";

        // Relative to plugin folder when available; falls back to WebClient/<StaticFolderName> for UserCode scenarios.
        public string GetStaticFilesPath() => StaticFolderName;

        public string GetEmbeddedResourceNamespace() => null;

        private void EnsureStaticContentFallback()
        {
            try
            {
                // Eco.WebServer static files are served from WebConstants.WebRoot (current working directory / WebClient),
                // not AppContext.BaseDirectory when running single-file bundles.
                var webRootCandidates = new[]
                {
                    Path.Combine(Directory.GetCurrentDirectory(), "WebClient"),
                    Path.Combine(AppContext.BaseDirectory, "WebClient")
                };

                var sourceCandidates = new[]
                {
                    Path.Combine(Directory.GetCurrentDirectory(), "Mods", "UserCode", StaticFolderName, "index.html"),
                    Path.Combine(Directory.GetCurrentDirectory(), "Server", "Mods", "UserCode", StaticFolderName, "index.html"),
                    Path.Combine(AppContext.BaseDirectory, "Mods", "UserCode", StaticFolderName, "index.html"),
                    Path.Combine(AppContext.BaseDirectory, "Server", "Mods", "UserCode", StaticFolderName, "index.html")
                };

                string sourcePath = null;
                foreach (var candidate in sourceCandidates)
                {
                    if (!File.Exists(candidate))
                        continue;

                    sourcePath = candidate;
                    break;
                }

                if (sourcePath == null)
                {
                    Log.WriteWarningLineLoc($"[EcoColorMixerWebPlugin] Could not find source index.html for fallback copy. Checked: {string.Join(" | ", sourceCandidates)}");
                    return;
                }

                // Copy once to the first valid web root to avoid duplicate copies/log spam.
                foreach (var webRootFolder in webRootCandidates)
                {
                    if (string.IsNullOrWhiteSpace(webRootFolder))
                        continue;

                    try
                    {
                        var targetFolder = Path.Combine(webRootFolder, StaticFolderName);
                        var targetIndex = Path.Combine(targetFolder, "index.html");
                        Directory.CreateDirectory(targetFolder);
                        File.Copy(sourcePath, targetIndex, overwrite: true);
                        Log.WriteLineLoc($"[EcoColorMixerWebPlugin] Copied static index fallback from '{sourcePath}' to '{targetIndex}'.");
                        break;
                    }
                    catch (Exception copyEx)
                    {
                        Log.WriteWarningLineLoc($"[EcoColorMixerWebPlugin] Failed fallback copy to '{webRootFolder}': {copyEx.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                Log.WriteErrorLineLoc($"[EcoColorMixerWebPlugin] Failed preparing static fallback content: {e.Message}");
                Log.WriteException(e);
            }
        }
    }
}
