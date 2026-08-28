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
public class InsurancePricePatch : AbstractPatch
{
    private static ProfileTypeHelper _profileTypeHelper = null!;

    public InsurancePricePatch(ProfileTypeHelper profileTypeHelper)
    {
        _profileTypeHelper = profileTypeHelper;
    }

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(InsuranceService), nameof(InsuranceService.GetRoublePriceToInsureItemWithTrader));
    }

    [PatchPostfix]
    public static void Postfix(ref double __result, PmcData? pmcData)
    {
        if (pmcData is null)
            return;
        
        var sessionId = pmcData.SessionId ?? new MongoId();

        if (!_profileTypeHelper.IsIronmanProfile(sessionId))
            return;

        var profileType = _profileTypeHelper.GetProfileType(sessionId);
        var multiplier = GetInsuranceMultiplier(profileType);

        if (Math.Abs(multiplier - 1f) < 0.001)
            return;

        __result *= multiplier;
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
