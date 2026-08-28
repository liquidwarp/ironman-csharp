namespace IronManServer.Services;

using Helpers;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Tables;

[Injectable(InjectionType = InjectionType.Singleton, TypePriority = OnLoadOrder.Preload + 5)]
public class ProfileService(
    LocaleTable localeTable,
    TemplateTable templateTable,
    ProfileLoadHelper profileLoadHelper) : IOnLoad
{
    public async Task OnLoadAsync(CancellationToken ct)
    {
        await AddCustomProfiles(ct);
    }
    
    private void AddLocales(string description, string profileKey)
    {
        foreach (var (_, lazyLoadLocaleData) in localeTable.Global)
        {
            lazyLoadLocaleData.AddTransformer(localeData => {
                if (localeData is null)
                    return localeData;

                localeData.Add(profileKey, description);

                return localeData;
            });
        }
    }
    
    private async Task AddCustomProfiles(CancellationToken ct)
    {
        var profiles = templateTable.Profiles;
        var customProfileData = await profileLoadHelper.GetCustomProfiles(ct);
        var customProfileLocales = await profileLoadHelper.GetLauncherProfileLocales(ct);

        foreach (var customProfile in customProfileData)
        {
            profiles.TryAdd(customProfile.Key, customProfile.Value);

            var localeKey = customProfile.Value.DescriptionLocaleKey ?? customProfile.Key.ToLowerInvariant().Replace(' ', '-');
            if (!customProfileLocales.TryGetValue(localeKey, out var localeData))
            {
                localeData = customProfile.Key.ToLowerInvariant().Replace(' ', '-');
            }
            AddLocales(localeData, localeKey);
        }
    }
}
