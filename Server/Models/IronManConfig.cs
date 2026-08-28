namespace IronManServer.Models;

public class IronManConfig {
    public ProfileSettings Standard { get; set; } = new ProfileSettings
    {
        ChanceOfSkillLevelLoss = 0,
        ChanceOfPlayerLevelLoss = 0
    };
    public ProfileSettings Ultimate { get; set; } = new ProfileSettings
    {
        ChanceOfSkillLevelLoss = 20,
        ChanceOfPlayerLevelLoss = 20
    };
    public ProfileSettings Hardcore { get; set; } = new ProfileSettings
    {
        ChanceOfSkillLevelLoss = 40,
        ChanceOfPlayerLevelLoss = 40
    };
    public DebugSettings DebugSettings { get; set; } = new DebugSettings();
    public ConfigAppSettings ConfigAppSettings { get; set; } =  new ConfigAppSettings();
}

public class ProfileSettings {
    public int ChanceOfSkillLevelLoss { get; set; }
    public int ChanceOfPlayerLevelLoss { get; set; }
}

public class DebugSettings {
    public bool DebugLosses { get; set; }
}

public class ConfigAppSettings {
    public bool DisableAnimations { get; set; }
    public bool AllowUpdateChecks { get; set; }
    public bool ShowDefaultButton { get; set; }
}
