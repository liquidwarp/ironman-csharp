namespace IronManServer.Helpers;

using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Utils;
using Path=Path;

[Injectable(InjectionType = InjectionType.Singleton)]
public class ProfileLoadHelper(
    ModHelper modHelper,
    JsonUtil jsonUtil) 
{
    public Dictionary<string, ProfileSides> ProfileData { get; set; } = new();
    
    public async Task<Dictionary<string, ProfileSides>> GetCustomProfiles(CancellationToken ct = default)
    {
        var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var profileDataPath = Path.Combine(modPath, "Data", "profiles.json");
        ProfileData = await jsonUtil.DeserializeFromFileAsync<Dictionary<string, ProfileSides>>(profileDataPath, ct) ?? new Dictionary<string, ProfileSides>();

        return ProfileData;
    }

    public async Task<Dictionary<string, string>> GetLauncherProfileLocales(CancellationToken ct = default)
    {
        var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var localesDataPath = Path.Combine(modPath, "Data", "locales.json");
        var localesData = await jsonUtil.DeserializeFromFileAsync<Dictionary<string, string>>(localesDataPath, ct);

        return localesData ?? new Dictionary<string, string>();
    }
}
