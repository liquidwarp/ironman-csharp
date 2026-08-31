namespace IronManServer.Services;

using System.Reflection;
using Helpers;
using Models.Enums;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Services.Commerce;
using SPTarkov.Server.Core.Utils;

[Injectable(InjectionType = InjectionType.Singleton, TypePriority = OnLoadOrder.Preload + 15)]
public class LevelGiftService(
    ISptLogger<LevelGiftService> logger,
    ProfileTypeHelper profileTypeHelper,
    GiftService giftService,
    GiftsConfig giftsConfig,
    ModHelper modHelper,
    JsonUtil jsonUtil) : IOnLoad
{
    // Every Ironman profile earns the same storage cases. Point a profile at its
    // own table here if it should ever diverge.
    private static readonly Dictionary<int, string> SharedLevelGifts = new()
    {
        [15] = "ironman-level-15",
        [25] = "ironman-level-25"
    };

    private readonly Dictionary<ProfileType, Dictionary<int, string>> _levelGifts = new()
    {
        [ProfileType.Standard] = SharedLevelGifts,
        [ProfileType.Ultimate] = SharedLevelGifts,
        [ProfileType.Hardcore] = SharedLevelGifts
    };

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var modPath = modHelper.GetAbsolutePathToModFolder(assembly);
        var giftsFile = Path.Combine(modPath, "Data", "gifts.json");

        var customGifts = jsonUtil.DeserializeFromFile<Dictionary<string, Gift>>(giftsFile);

        if (customGifts is null)
        {
            logger.Error($"Failed to load custom items from '{giftsFile}'");
            return Task.CompletedTask;
        }

        foreach (var (giftId, gift) in customGifts)
        {
            giftsConfig.Gifts.TryAdd(giftId, gift);
        }

        return Task.CompletedTask;
    }

    public void CheckLevelMilestone(MongoId sessionId, int level)
    {
        if (!profileTypeHelper.IsIronmanProfile(sessionId))
            return;

        var profileType = profileTypeHelper.GetProfileType(sessionId);

        if (!_levelGifts.TryGetValue(profileType, out var gifts))
            return;

        foreach (var (requiredLevel, giftId) in gifts)
        {
            if (level < requiredLevel)
                continue;

            giftService.SendGiftToPlayer(sessionId, giftId);
        }
    }
}
