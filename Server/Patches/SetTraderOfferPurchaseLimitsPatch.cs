namespace IronManServer.Patches;

using System.Reflection;
using HarmonyLib;
using Helpers;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Enums;

[Injectable]
public class SetTraderOfferPurchaseLimitsPatch : AbstractPatch
{
    private static ProfileTypeHelper _profileTypeHelper = null!;
    private static ItemHelper _itemHelper = null!;

    public SetTraderOfferPurchaseLimitsPatch(ProfileTypeHelper profileTypeHelper, ItemHelper itemHelper)
    {
        _profileTypeHelper = profileTypeHelper;
        _itemHelper = itemHelper;
    }

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(RagfairController), "SetTraderOfferPurchaseLimits");
    }

    [PatchPostfix]
    public static void Postfix(RagfairOffer offer, SptProfile fullProfile)
    {
        var playerId = fullProfile.CharacterData?.PmcData?.SessionId ?? new MongoId();
        if (!_profileTypeHelper.IsIronmanProfile(playerId))
            return;

        var offerRootItem = offer.Items?.FirstOrDefault();
        if (offerRootItem is null)
            return;

        var maxBuyCount = GetIronmanBuyRestrictionMax(offerRootItem.Template);

        if (offer.BuyRestrictionMax > maxBuyCount)
            offer.BuyRestrictionMax = maxBuyCount;
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
