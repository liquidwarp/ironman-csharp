namespace IronManClient.Utils;

using Comfort.Common;
using EFT;
using Models;
using SPT.Reflection.Utils;

public static class ProfileStatus
{
    
    public static IEftSession Session
    {
        get => ClientAppUtils.GetMainApp().GetClientBackEndSession();
    }
    
    private static ProfileType _profileType = ProfileType.None;

    public static ProfileType ProfileType
    {
        get => _profileType;
        set
        {
            _profileType = value;
            ApplyGlobalOverrides(value);
        }
    }

    private static void ApplyGlobalOverrides(ProfileType profileType)
    {
        var config = Singleton<GlobalConfiguration>.Instance;
        if (config is null)
        {
            Plugin.Log.LogInfo($"[IronMan] ApplyGlobalOverrides: Singleton<GlobalConfiguration>.Instance is null, skipping override (ProfileType: {profileType})");
            return;
        }

        if (profileType is not (ProfileType.Standard or ProfileType.Ultimate or ProfileType.Hardcore))
        {
            Plugin.Log.LogInfo($"[IronMan] ApplyGlobalOverrides: ProfileType is {profileType}, not an Ironman profile, skipping override");
            return;
        }

        config.RagFair.minUserLevel = 99;
        config.Health.HealPrice.HealthPointPrice = 30;

        Plugin.Log.LogInfo($"[IronMan] ApplyGlobalOverrides: Applied overrides for {profileType} - RagFair.MinUserLevel = {config.RagFair.minUserLevel}, Health.HealPrice.HealthPointPrice = {config.Health.HealPrice.HealthPointPrice}");
    }
    
    public static long DowngradeLastOfferedAt { get; set; } = long.MinValue;
    public static bool DowngradeAvailable { get; set; }
    public static ProfileType DowngradeTarget { get; set; } = ProfileType.None;
}
