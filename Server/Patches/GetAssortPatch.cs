namespace IronManServer.Patches;

using System.Reflection;
using HarmonyLib;
using Helpers;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Helpers.Traders;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;

[Injectable]
public class GetAssortPatch : AbstractPatch
{
    private static ProfileTypeHelper _profileTypeHelper = null!;
    private static ItemHelper _itemHelper = null!;

    public GetAssortPatch(ProfileTypeHelper profileTypeHelper, ItemHelper itemHelper)
    {
        _profileTypeHelper = profileTypeHelper;
        _itemHelper = itemHelper;
    }

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(TraderAssortHelper), nameof(TraderAssortHelper.GetAssort));
    }

    [PatchPostfix]
    public static void Postfix(MongoId sessionId, ref TraderAssort __result)
    {
        if (!_profileTypeHelper.IsIronmanProfile(sessionId))
            return;

        foreach (var item in __result.Items)
        {
            if (item.Upd is not { } upd)
                continue;

            var maxBuyCount = GetIronmanBuyRestrictionMax(item.Template);

            if (upd.BuyRestrictionMax > maxBuyCount)
                upd.BuyRestrictionMax = maxBuyCount;
        }
    }
    
    private static int GetIronmanBuyRestrictionMax(MongoId templateId)
    {
        if (_itemHelper.IsOfBaseclasses(templateId, [BaseClasses.WEAPON, BaseClasses.MEDS]))
            return 1;

        if (_itemHelper.IsOfBaseclass(templateId, BaseClasses.AMMO))
            return 60;

        return 5;
    }
}
