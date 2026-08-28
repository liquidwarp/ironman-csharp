namespace IronManClient.Patches.Experience;

using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using Models;
using SPT.Reflection.Patching;
using ProfileStatus=Utils.ProfileStatus;

public class EndStatisticsSessionPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BaseStatisticsManager), nameof(BaseStatisticsManager.EndStatisticsSession), new[] { typeof(ExitStatus), typeof(float) });
    }

    [PatchPostfix]
    public static void Postfix(BaseStatisticsManager __instance, ExitStatus exitStatus, float pastTime)
    {
        if (exitStatus == ExitStatus.Transit)
            return;

        if (ProfileStatus.ProfileType != ProfileType.Ultimate && ProfileStatus.ProfileType != ProfileType.Hardcore)
            return;

        var mainPlayer = Singleton<GameWorld>.Instance?.MainPlayer;
        if (mainPlayer is null || __instance.Player != mainPlayer)
            return;

        var eftStats = __instance.Profile.EftStats;
        var multiplier = GetXpMultiplier(ProfileStatus.ProfileType);

        var originalTotal = eftStats.TotalSessionExperience;
        var newTotal = (int)(originalTotal * multiplier);
        var delta = newTotal - originalTotal;

        eftStats.TotalSessionExperience = newTotal;
        __instance.Profile.Info.Experience += delta;
    }

    private static float GetXpMultiplier(ProfileType profileType)
    {
        return profileType switch
        {
            ProfileType.Ultimate => 1.25f,
            ProfileType.Hardcore => 2.0f,
            _ => 1.0f
        };
    }
}
