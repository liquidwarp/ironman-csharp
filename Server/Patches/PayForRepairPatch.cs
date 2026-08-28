namespace IronManServer.Patches;

using System.Reflection;
using HarmonyLib;
using Helpers;
using Models.Enums;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Services.Commerce;

[Injectable]
public class PayForRepairPatch : AbstractPatch
{
    private static ProfileTypeHelper _profileTypeHelper = null!;

    public PayForRepairPatch(ProfileTypeHelper profileTypeHelper)
    {
        _profileTypeHelper = profileTypeHelper;
    }

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(RepairService), nameof(RepairService.PayForRepair));
    }

    [PatchPrefix]
    public static void Prefix(MongoId sessionID, ref double repairCost)
    {
        if (!_profileTypeHelper.IsIronmanProfile(sessionID))
            return;

        var profileType = _profileTypeHelper.GetProfileType(sessionID);
        var multiplier = GetInsuranceMultiplier(profileType);

        if (Math.Abs(multiplier - 1f) < 0.001)
            return;
        
        repairCost *= multiplier;
    }

    private static float GetInsuranceMultiplier(ProfileType profileType)
    {
        return profileType switch
        {
            ProfileType.Ultimate => 2f,
            ProfileType.Hardcore => 5f,
            _ => 1f
        };
    }
}
