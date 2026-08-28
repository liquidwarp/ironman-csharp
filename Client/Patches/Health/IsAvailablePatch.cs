namespace IronManClient.Patches.Health;

using System.Reflection;
using EFT.UI.SessionEnd;
using HarmonyLib;
using Models;
using Utils;
using SPT.Reflection.Patching;

internal class IsAvailablePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(HealthTreatmentScreen), nameof(HealthTreatmentScreen.IsAvailable));
    }

    [PatchPrefix]
    public static bool Prefix(ref bool __result)
    {
        if (ProfileStatus.ProfileType != ProfileType.Hardcore)
            return true;
        
        __result = false;
        return false;

    }
}