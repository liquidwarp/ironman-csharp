namespace IronManClient.Patches.Health;

using System.Reflection;
using EFT.UI.HealthTreatment;
using HarmonyLib;
using Models;
using SPT.Reflection.Patching;
using Utils;

internal class HealPricePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(HealthObserver), nameof(HealthObserver.HealPrice));
    }

    [PatchPostfix]
    public static void Postfix(HealthObserver __instance, ref float __result)
    {
        if (__instance._profile is null)
            return;

        var profileType = ProfileStatus.ProfileType;

        __result *= profileType switch
        {
            ProfileType.Standard => 2f,
            ProfileType.Ultimate => 5f,
            ProfileType.Hardcore => 50f,
            _ => 1f
        };
    }
}