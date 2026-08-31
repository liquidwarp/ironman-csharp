namespace IronManServer.Patches;

using System.Reflection;
using HarmonyLib;
using Helpers;
using Models;
using Models.Enums;
using Services;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Services.InRaid;
using SPTarkov.Server.Core.Utils;

[Injectable]
public class HandlePostRaidPmcPatch : AbstractPatch
{
    private static ProfileTypeHelper _profileTypeHelper = null!;
    private static RandomUtil _randomUtil = null!;
    private static ProfileHelper _profileHelper = null!;
    private static ItemHelper _itemHelper = null!;
    private static InventoryHelper _inventoryHelper = null!;
    private static ISptLogger<HandlePostRaidPmcPatch> _logger = null!;
    private static LevelGiftService _levelGiftService = null!;
    private static ProfileStatusHelper _profileStatusHelper = null!;
    private static IronManConfig _ironManConfig = null!;

    public HandlePostRaidPmcPatch(
        ProfileTypeHelper profileTypeHelper, 
        RandomUtil randomUtil, 
        ProfileHelper profileHelper,
        ItemHelper itemHelper,
        InventoryHelper inventoryHelper,
        ISptLogger<HandlePostRaidPmcPatch> logger,
        LevelGiftService levelGiftService,
        ProfileStatusHelper profileStatusHelper,
        IronManConfig ironmanConfig)
    {
        _profileTypeHelper = profileTypeHelper;
        _randomUtil = randomUtil;
        _profileHelper = profileHelper;
        _itemHelper = itemHelper;
        _inventoryHelper = inventoryHelper;
        _logger = logger;
        _levelGiftService = levelGiftService;
        _profileStatusHelper = profileStatusHelper;
        _ironManConfig = ironmanConfig;
    }
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(LocationLifecycleService), "HandlePostRaidPmc");
    }

    [PatchPrefix]
    public static void Prefix(MongoId sessionId, SptProfile fullServerProfile, bool isDead, EndLocalRaidRequestData request)
    {
        if (!isDead)
            return;
        
        var serverDetails = request.ServerId?.Split(".");
        var isPmc = serverDetails != null && serverDetails[1].Contains("pmc", StringComparison.InvariantCultureIgnoreCase);

        if (!isPmc)
            return;

        if (!_profileTypeHelper.IsIronmanProfile(sessionId))
            return;

        var profileType = _profileTypeHelper.GetProfileType(sessionId);

        var preRaidProfile = fullServerProfile.CharacterData?.PmcData;
        if (preRaidProfile is null)
            return;
        
        var postRaidProfile = request.Results?.Profile;
        if (postRaidProfile is null)
            return;

        var postRaidSkills = postRaidProfile.Skills;
        if (postRaidSkills is null)
            return;
        
        var level = preRaidProfile.Info?.Level ?? 0;
        _profileStatusHelper.RecordDeath(sessionId, level);
        
        var roublesBeforeDeath = GetCurrencyBalance(preRaidProfile, ItemTpl.MONEY_ROUBLES);
        _profileStatusHelper.RecordRoubleBalance(sessionId, roublesBeforeDeath, level);
        
        RemoveUnprotectedItems(preRaidProfile, sessionId);
        AdjustCurrency(preRaidProfile, profileType, sessionId);
        
        switch (profileType)
        {
            case ProfileType.Standard:
                AdjustForSkillLoss(postRaidSkills, 1, _ironManConfig.Standard.ChanceOfSkillLevelLoss);
                AdjustForCharacterExperienceLoss(postRaidProfile, 1, _ironManConfig.Standard.ChanceOfPlayerLevelLoss);
                break;
            
            case ProfileType.Ultimate:
                AdjustForSkillLoss(postRaidSkills, 1, _ironManConfig.Ultimate.ChanceOfSkillLevelLoss);
                AdjustForCharacterExperienceLoss(postRaidProfile, 1, _ironManConfig.Ultimate.ChanceOfPlayerLevelLoss);
                break;

            case ProfileType.Hardcore:
                AdjustForSkillLoss(postRaidSkills, 3, _ironManConfig.Hardcore.ChanceOfSkillLevelLoss);
                AdjustForCharacterExperienceLoss(postRaidProfile, 3, _ironManConfig.Hardcore.ChanceOfPlayerLevelLoss);
                break;

            case ProfileType.None:
            default:
                break;
        }
    }

    [PatchPostfix]
    public static void Postfix(MongoId sessionId, SptProfile fullServerProfile)
    {
        if (!_profileTypeHelper.IsIronmanProfile(sessionId))
            return;

        var pmcProfile = fullServerProfile.CharacterData?.PmcData;

        if (pmcProfile?.Info is null)
            return;

        if (pmcProfile.Info.Level is null)
            return;
        
        var level = pmcProfile.Info.Level.Value;
        _profileStatusHelper.RecordLevel(sessionId, level);
        _levelGiftService.CheckLevelMilestone(sessionId, level);
    }

    private static void AdjustForSkillLoss(Skills skills, int maxSkillLevelLoss, int chanceOfLoss)
    {
        foreach (var skill in skills.Common)
        {
            var currentProgress = Math.Floor(skill.Progress / 100) * 100;
            var currentLevel = (int)(currentProgress / 100);

            var maxLoss = Math.Max(0, Math.Min(currentLevel - 1, maxSkillLevelLoss));
            var skillLevelLoss = _randomUtil.GetChance100(chanceOfLoss) ? _randomUtil.GetInt(0, maxLoss) : 0;

            var newProgress = currentProgress - skillLevelLoss * 100;

            if (skillLevelLoss > 0 || Math.Abs(skill.Progress - newProgress) > 0.001)
                LogLoss($"Skill {skill.Id} - Level {currentLevel} ({skill.Progress:N0} XP) -> Level {(int)(newProgress / 100)} ({newProgress:N0} XP)");
            
            skill.Progress = newProgress;
            skill.PointsEarnedDuringSession = 0;
        }
    }

    private static void AdjustForCharacterExperienceLoss(PmcData pmcProfile, int maxPlayerLevelLoss, int chanceOfLoss)
    {
        if (pmcProfile.Info is null)
            return;

        var experience = pmcProfile.Info.Experience ?? 0;
        var currentLevel = _profileHelper.GetLevelFromExperience(experience);
        var currentLevelExperience = _profileHelper.GetExperience(currentLevel) ?? 0;

        var maxLevelLoss = Math.Max(0, Math.Min(currentLevel - 1, maxPlayerLevelLoss));
        var levelLoss = _randomUtil.GetChance100(chanceOfLoss) ? _randomUtil.GetInt(0, maxLevelLoss) : 0;

        var newLevel = currentLevel - levelLoss;
        var newExperience = levelLoss > 0 ? _profileHelper.GetExperience(newLevel) ?? 0 : currentLevelExperience;

        if (levelLoss > 0 || experience != newExperience)
            LogLoss($"Character - Level {currentLevel} ({experience:N0} XP) -> Level {newLevel} ({newExperience:N0} XP)");

        pmcProfile.Info.Experience = newExperience;
        pmcProfile.Info.Level = newLevel;
    }
    
    private static bool IsProtectedItem(Item item, Dictionary<MongoId, Item> itemsById)
    {
        List<MongoId> protectedBaseClasses = [
            BaseClasses.SIMPLE_CONTAINER, 
            BaseClasses.HIDEOUT_AREA_CONTAINER, 
            BaseClasses.MOB_CONTAINER,
            BaseClasses.LOCKABLE_CONTAINER
        ];

        var current = item;
        var visited = new HashSet<MongoId> { current.Id };
        const int maxDepth = 32;
        var depth = 0;

        while (true)
        {
            if (Enum.TryParse<EquipmentSlots>(current.SlotId, true, out _) ||
                string.Equals(current.SlotId, "dogtag", StringComparison.OrdinalIgnoreCase))
                return true;

            if (_itemHelper.IsOfBaseclasses(current.Template, protectedBaseClasses))
                return true;

            if (current.ParentId is null || !itemsById.TryGetValue(current.ParentId, out current))
                return false;

            if (!visited.Add(current.Id))
            {
                _logger.Error($"[IronMan] Recursive cycle detected in item parent chain starting at {item.Id}, at {current.Id}. Treating as unprotected.");
                return false;
            }

            if (++depth > maxDepth)
            {
                _logger.Error($"[IronMan] Item parent chain exceeded max depth ({maxDepth}) starting at {item.Id}. Possible corrupt data. Treating as unprotected.");
                return false;
            }
        }
    }
    
    private static void RemoveUnprotectedItems(PmcData pmcProfile, MongoId sessionId)
    {
        var inventory = pmcProfile.Inventory;
        var items = inventory?.Items;

        if (items is null)
            return;

        var itemsById = items.ToDictionary(x => x.Id);

        var rootIds = new HashSet<MongoId?>
        {
            inventory?.Equipment,
            inventory?.Stash,
            inventory?.SortingTable,
            inventory?.QuestRaidItems,
            inventory?.QuestStashItems,
            inventory?.HideoutCustomizationStashId,
        };

        double totalGearValueLost = 0;
        var lostMillionPlusItem = false;

        foreach (var item in items.ToList())
        {
            if (rootIds.Contains(item.Id))
                continue;

            if (IsProtectedItem(item, itemsById))
                continue;

            if (IsCurrency(item.Template))
                continue;

            var price = _itemHelper.GetItemMaxPrice(item.Template);

            totalGearValueLost += price;
            if (price >= 1000000)
                lostMillionPlusItem = true;

            LogLoss(item, $"value {price:N0}₽");
            _inventoryHelper.RemoveItem(pmcProfile, item.Id, sessionId);
        }

        if (totalGearValueLost > 0)
        {
            _profileStatusHelper.RecordLostGear(sessionId, totalGearValueLost, lostMillionPlusItem, pmcProfile.Info?.Level ?? 0);
        }
    }
    
    private static double GetCurrencyBalance(PmcData pmcProfile, MongoId currencyTemplate)
    {
        return pmcProfile.Inventory?.Items?.Where(item => item.Template == currencyTemplate).Sum(item => item.Upd?.StackObjectsCount ?? 0) ?? 0;
    }
    
    private static void AdjustCurrency(PmcData pmcProfile, ProfileType profileType, MongoId sessionId)
    {
        var items = pmcProfile.Inventory?.Items;

        if (items is null)
            return;

        var limits = GetCurrencyLimits(profileType);

        foreach (var currency in new[]
        {
            ItemTpl.MONEY_ROUBLES,
            ItemTpl.MONEY_DOLLARS,
            ItemTpl.MONEY_EUROS,
            ItemTpl.MONEY_GP_COIN
        })
        {
            var limit = GetCurrencyLimit(currency, limits)!.Value;
            var stacks = items.Where(item => item.Template == currency).ToList();
            var remaining = limit;

            foreach (var stack in stacks)
            {
                var count = stack.Upd?.StackObjectsCount ?? 0;

                if (remaining <= 0)
                {
                    LogLoss(stack, $"currency stack of {count:N0}");
                    _inventoryHelper.RemoveItem(pmcProfile, stack.Id, sessionId);
                    continue;
                }

                if (count <= remaining)
                {
                    remaining -= (int)count;
                    continue;
                }

                var lost = (int)count - remaining;

                LogLoss(stack, $"currency loss of {lost:N0} (stack reduced from {count:N0} to {remaining:N0})");

                stack.Upd!.StackObjectsCount = remaining;
                remaining = 0;
            }
        }
    }
    
    private static bool IsCurrency(MongoId template)
    {
        return template == ItemTpl.MONEY_ROUBLES || template == ItemTpl.MONEY_DOLLARS || template == ItemTpl.MONEY_EUROS || template == ItemTpl.MONEY_GP_COIN;
    }
    
    private sealed record CurrencyLimits(int Roubles, int Dollars, int Euros, int GpCoins);
    private static CurrencyLimits GetCurrencyLimits(ProfileType profileType)
    {
        return profileType switch
        {
            ProfileType.Standard => new CurrencyLimits(
            Roubles: 1000000,
            Dollars: 2000,
            Euros: 1000,
            GpCoins: 50),

            ProfileType.Ultimate => new CurrencyLimits(
            Roubles: 500000,
            Dollars: 1000,
            Euros: 500,
            GpCoins: 25),

            ProfileType.Hardcore => new CurrencyLimits(
            Roubles: 100000,
            Dollars: 0,
            Euros: 0,
            GpCoins: 0),

            _ => new CurrencyLimits(0, 0, 0, 0)
        };
    }
    
    private static int? GetCurrencyLimit(MongoId template, CurrencyLimits limits)
    {
        if (template == ItemTpl.MONEY_ROUBLES)
            return limits.Roubles;

        if (template == ItemTpl.MONEY_DOLLARS)
            return limits.Dollars;

        if (template == ItemTpl.MONEY_EUROS)
            return limits.Euros;

        if (template == ItemTpl.MONEY_GP_COIN)
            return limits.GpCoins;

        return null;
    }
    
    private static void LogLoss(Item item, string reason)
    {
        if (!_ironManConfig.DebugSettings.DebugLosses)
            return;

        var itemName = _itemHelper.GetItemName(item.Template);
        _logger.Warning($"[IronMan] Death loss: {itemName} ({item.Template} - Parent: {item.ParentId} - {reason}");
    }
    
    private static void LogLoss(string reason)
    {
        if (!_ironManConfig.DebugSettings.DebugLosses)
            return;

        _logger.Warning($"[IronMan] Death loss: {reason}");
    }
}
