namespace IronManServer.Services;

using System.Reflection;
using Helpers;
using Models;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Services.Modding.Custom;
using SPTarkov.Server.Core.Utils;

[Injectable(InjectionType = InjectionType.Singleton, TypePriority = OnLoadOrder.Preload + 10)]
public class ItemService(
    ISptLogger<ItemService> logger,
    ProfileLoadHelper profileLoadHelper,
    CustomItemService customItemService,
    TemplateTable templateTable,
    ModHelper modHelper,
    JsonUtil jsonUtil) : IOnLoad {
    
    private record StashConfig(MongoId GridId, int CellsV);
    private Dictionary<MongoId, StashConfig> StashConfigs { get; } = new()
    {
        ["6a8a86040fc828233480b117"] = new StashConfig("6a8a9be42932412ff8c717b3", 25),
        ["6a8a86040fc828233480b118"] = new StashConfig("6a8a9be42932412ff8c717b4", 20)
    };
    
    public Task OnLoadAsync(CancellationToken ct = default)
    {
        AddCustomStashes();
        AddCustomItems();
        
        return Task.CompletedTask;
    }
    private void AddCustomItems()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var modPath = modHelper.GetAbsolutePathToModFolder(assembly);
        var itemsFile = Path.Combine(modPath, "Data", "items.json");

        var customItems = jsonUtil.DeserializeFromFile<List<CustomItemDefinition>>(itemsFile);

        if (customItems is null)
        {
            logger.Error($"Failed to load custom items from '{itemsFile}'");
            return;
        }

        foreach (var item in customItems)
        {
            var cloneDetails = new NewItemFromCloneDetails
            {
                NewId = item.NewId,
                ParentId = item.ParentId,
                NewItemName = item.NewItemName,
                ItemTplToClone = item.ItemTplToClone,
                AddToFleaPriceDb = item.AddToFleaPriceDb,
                AddToHandbook = item.AddToHandbook,
                AddToWeaponShelf = item.AddToWeaponShelf,
                FleaPriceRoubles = item.FleaPriceRoubles,
                HandbookPriceRoubles = item.HandbookPriceRoubles,
                Locales = item.Locales
            };

            var result = customItemService.CreateItemFromClone(cloneDetails, assembly);

            if (!result.Success)
            {
                logger.Error($"Failed to create item for '{item.NewItemName}': {string.Join(", ", result.Errors)}");
                continue;
            }

            ApplyGridOverride(result.ItemId, item.Grid);
        }
    }
    
    private void ApplyGridOverride(string itemId, GridOverride? gridOverride)
    {
        if (gridOverride is null)
            return;

        if (!templateTable.Items.TryGetValue(itemId, out var item))
        {
            logger.Error($"Created custom item '{itemId}' could not be found");
            return;
        }

        var grid = item.Properties?.Grids?.FirstOrDefault();

        if (grid?.Properties is null)
        {
            logger.Error($"Custom item '{itemId}' has no grid to override");
            return;
        }

        grid.Id = gridOverride.Id;
        grid.Parent = itemId;

        if (gridOverride.CellsH.HasValue)
            grid.Properties.CellsH = gridOverride.CellsH;

        if (gridOverride.CellsV.HasValue)
            grid.Properties.CellsV = gridOverride.CellsV;
    }
    
    private void AddCustomStashes()
    {
        foreach (var (profileKeyName, profileData) in profileLoadHelper.ProfileData)
        {
            var newStashId = profileData.Bear?.Character?.Bonuses?[0].TemplateId;

            if (newStashId is null || newStashId == ItemTpl.STASH_STANDARD_STASH_10X30)
                continue;
            
            if (!StashConfigs.TryGetValue(newStashId.Value, out var config))
                continue;
            
            var cloneDetails = new NewItemFromCloneDetails
            {
                NewId = new MongoId(newStashId),
                ParentId = BaseClasses.STASH,
                NewItemName = $"{profileKeyName.ToLowerInvariant().Replace(' ', '-')}-stash-10x{config.CellsV}",
                ItemTplToClone = ItemTpl.STASH_STANDARD_STASH_10X30,
                AddToFleaPriceDb = false,
                AddToHandbook = false,
                AddToWeaponShelf = false,
                FleaPriceRoubles = 100000000,
                HandbookPriceRoubles = 100000000,
                Locales = new Dictionary<string, LocaleDetails>
                {
                    {
                        "en", new LocaleDetails
                        {
                            Name = $"{profileKeyName} Stash",
                            ShortName = $"{profileKeyName} Stash",
                            Description = "Somebody's badly camouflaged stash."
                        }
                    }
                }
            };

            var result = customItemService.CreateItemFromClone(cloneDetails);
            if (!result.Success)
            {
                logger.Warning($"Failed to create stash for {profileKeyName}: {string.Join(", ", result.Errors)}");
                continue;
            }

            if (templateTable.Items.TryGetValue(result.ItemId, out var newStashItem))
            {
                var grid = newStashItem.Properties?.Grids?.FirstOrDefault();
                if (grid?.Properties is null)
                {
                    logger.Warning($"Stash '{result.ItemId}' has no grid to resize for profile '{profileKeyName}'");
                    continue;
                }

                grid.Properties.CellsV = config.CellsV;
                grid.Id = config.GridId;
                grid.Parent = result.ItemId;
            }
        }
    }
}
