namespace IronManClient.Patches.Menu;

using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using Models;
using SPT.Reflection.Patching;
using ProfileStatus=Utils.ProfileStatus;

public class GlobalConfigurationCreatePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Singleton<GlobalConfiguration>), nameof(Singleton<GlobalConfiguration>.Create), [typeof(GlobalConfiguration)]);
    }

    [PatchPostfix]
    public static void Postfix(GlobalConfiguration instance)
    {
        ApplyGlobalOverrides(instance, ProfileStatus.ProfileType);
    }

    private static void ApplyGlobalOverrides(GlobalConfiguration config, ProfileType profileType)
    {
        if (config is null)
            return;

        if (profileType is not (ProfileType.Standard or ProfileType.Ultimate or ProfileType.Hardcore))
            return;

        config.RagFair.minUserLevel = 99;
        config.Health.HealPrice.HealthPointPrice = 30;

        Plugin.Log.LogInfo($"Applied overrides for Ironman: {profileType} - RagFair.MinUserLevel = {config.RagFair.minUserLevel}, Health.HealPrice.HealthPointPrice = {config.Health.HealPrice.HealthPointPrice}");
    }
}
