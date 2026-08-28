namespace IronManClient.VersionCheck;

using System.Diagnostics;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

internal class VersionChecker(int version) {
    private readonly int _version = version;
    public VersionChecker() : this(0) { }

    private static int BuildVersion
    {
        get => Assembly.GetExecutingAssembly()
            .GetCustomAttributes(typeof(VersionChecker), false)
            .Cast<VersionChecker>().FirstOrDefault()?._version ?? 40743;
    }

    // Make sure the version of EFT being run is the correct version, throw an exception and output log message if it isn't
    /// <summary>
    /// Check the currently running program's version against the plugin assembly TarkovVersion attribute, and
    /// return false if they do not match. 
    /// Optionally add a fake setting to the F12 menu if Config is passed in
    /// </summary>
    /// <param name="logger">The ManualLogSource to output an error to</param>
    /// <param name="info">The PluginInfo object for the plugin, used to get the plugin name and version</param>
    /// <param name="config">A BepinEx ConfigFile object, if provided, a custom message will be added to the F12 menu</param>
    /// <returns></returns>
    public static bool CheckEftVersion(ManualLogSource logger, PluginInfo info, ConfigFile config = null)
    {
        var currentVersion = FileVersionInfo.GetVersionInfo(Paths.ExecutablePath).FilePrivatePart;
        var buildVersion = BuildVersion;
        if (currentVersion == buildVersion)
            return true;
        
        var errorMessage = $"ERROR: This version of {info.Metadata.Name} v{info.Metadata.Version} was built for Tarkov {buildVersion}, but you are running {currentVersion}. Please download the correct plugin version.";
        logger.LogError(errorMessage);
        Chainloader.DependencyErrors.Add(errorMessage);

        // This results in a bogus config entry in the BepInEx config file for the plugin, but it shouldn't hurt anything
        // We leave the "section" parameter empty so there's no section header drawn
        config?.Bind("", "TarkovVersion", "", new ConfigDescription(
        errorMessage, null, new ConfigurationManagerAttributes
        {
            CustomDrawer = ErrorLabelDrawer,
            ReadOnly = true,
            HideDefaultButton = true,
            HideSettingName = true,
            Category = null
        }
        ));

        return false;

    }

    static void ErrorLabelDrawer(ConfigEntryBase entry)
    {
        var styleNormal = new GUIStyle(GUI.skin.label);
        styleNormal.wordWrap = true;
        styleNormal.stretchWidth = true;

        var styleError = new GUIStyle(GUI.skin.label);
        styleError.stretchWidth = true;
        styleError.alignment = TextAnchor.MiddleCenter;
        styleError.normal.textColor = Color.red;
        styleError.fontStyle = FontStyle.Bold;

        // General notice that we're the wrong version
        GUILayout.BeginVertical();
        GUILayout.Label(entry.Description.Description, styleNormal, new[] { GUILayout.ExpandWidth(true) });

        // Centered red disabled text
        GUILayout.Label("Plugin has been disabled!", styleError, new[] { GUILayout.ExpandWidth(true) });
        GUILayout.EndVertical();
    }

    public struct VersionResponse
    {
        public string Version { get; set; }
    }

#pragma warning disable 0169, 0414, 0649
    internal sealed class ConfigurationManagerAttributes
    {
        public bool? ShowRangeAsPercent;
        public System.Action<ConfigEntryBase> CustomDrawer;
        public CustomHotkeyDrawerFunc CustomHotkeyDrawer;
        public delegate void CustomHotkeyDrawerFunc(ConfigEntryBase setting, ref bool isCurrentlyAcceptingInput);
        public bool? Browsable;
        public string Category;
        public object DefaultValue;
        public bool? HideDefaultButton;
        public bool? HideSettingName;
        public string Description;
        public string DispName;
        public int? Order;
        public bool? ReadOnly;
        public bool? IsAdvanced;
        public System.Func<object, string> ObjToStr;
        public System.Func<string, object> StrToObj;
    }
}
