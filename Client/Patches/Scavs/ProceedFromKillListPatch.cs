namespace IronManClient.Patches.Scavs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using Models;
using SPT.Reflection.Patching;
using SPT.SinglePlayer.Patches.ScavMode;
using ProfileStatus = Utils.ProfileStatus;

public class ProceedFromKillListPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(SessionResultShowOperation), nameof(SessionResultShowOperation.ProceedFromKillList));
    }

    [PatchPrefix]
    public static bool Prefix(SessionResultShowOperation __instance)
    {
        if (ProfileStatus.ProfileType is not (ProfileType.Ultimate or ProfileType.Hardcore))
            return true;

        var controller = __instance.ScavInventoryScreenController;

        if (controller == null)
            return true;

        Plugin.Log.LogInfo($"[IronMan] Auto-selling scav inventory ({ProfileStatus.ProfileType})");
        ScavAutoSell.SellAndProceed(controller, __instance.ShowStatistics);
        
        return false;
    }
}

public static class ScavAutoSell
{
    private const string RoubleTemplateId = "5449016a4bdc2d6f028b456f";

    public static async void SellAndProceed(ScavengerInventoryScreen.ScavengerInventoryScreenController controller, Action proceed)
    {
        try
        {
            var trader = controller.Session.Traders.FirstOrDefault(x => x.Settings.BuyerUp);

            if (trader == null)
            {
                proceed();
                return;
            }

            await trader.RefreshAssortment(true, true);

            var items = controller.ScavController.Inventory.Equipment.GetFirstLevelItems().Where(item => !item.Parent.IsSpecialSlotAddress()).ToList();
            if (items.Count == 0)
            {
                proceed();
                return;
            }

            var multiplier = GetScavSellMultiplier(ProfileStatus.ProfileType);
            var totalPrice = 0;

            foreach (var item in items)
            {
                if (item.TemplateId == RoubleTemplateId)
                {
                    totalPrice += item.StackObjectsCount;
                    continue;
                }

                var price = trader.GetItemPriceOnScavSell(item, true);
                totalPrice += (int)(price * multiplier);
            }

            ScavSellAllPriceStorePatch.StoredPrice = totalPrice;
            Plugin.Log.LogInfo($"[IronMan] [{ProfileStatus.ProfileType}] Scav sell price: {totalPrice} ({multiplier}x)");

            var removeResults = new List<RemoveResult>();

            foreach (var item in items)
            {
                var operationResult = ItemManipulator.Remove(item, controller.ScavController, true);
                if (operationResult.Failed)
                {
                    continue;
                }

                trader.CurrentAssortment.QuickFindTradingAppropriatePlace(item, null);
                removeResults.Add(operationResult.Value);
            }

            foreach (var removeResult in removeResults)
            {
                removeResult.RaiseEvents(controller.ScavController, CommandStatus.Begin);
            }

            var profile = controller.Session.Profile;

            if (profile == null)
            {
                proceed();
                return;
            }

            var result = await trader.CurrentAssortment.SellAsSavage(profile.Id, profile.PetId);
            foreach (var removeResult in removeResults)
            {
                removeResult.RaiseEvents(controller.ScavController, result.Succeed ? CommandStatus.Succeed : CommandStatus.Failed);
            }

            if (result.Failed)
            {
                proceed();
                return;
            }

            foreach (var removeResult in removeResults)
            {
                removeResult.Execute();
            }

            proceed();
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"[IronMan] Exception while auto-selling scav inventory: {ex}");
            proceed();
        }
    }

    private static float GetScavSellMultiplier(ProfileType profileType)
    {
        return profileType switch
        {
            ProfileType.Ultimate => 0.75f,
            ProfileType.Hardcore => 0.5f,
            _ => 1f
        };
    }
}