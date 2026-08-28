namespace IronManClient.Patches.Menu;

using System;
using System.Reflection;
using System.Threading.Tasks;
using EFT.Communications;
using Models;
using Utils;
using Newtonsoft.Json;
using SPT.Common.Http;
using SPT.Reflection.Patching;
using Version=EFT.Version;

internal class CreatePatch : ModulePatch
{
    private static bool _profileLoaded;
    
    protected override MethodBase GetTargetMethod()
    {
        return typeof(Version).GetMethod(nameof(Version.Create));
    }

    [PatchPostfix]
    public static async void Postfix()
    {
        try
        {
            if (_profileLoaded)
                return;

            await LoadFromServer();

            _profileLoaded = true;
        }
        catch (Exception e)
        {
            Plugin.Log.LogError("Caught error while trying to load profile data");
            Plugin.Log.LogError(e.ToString());
        }
    }
    
    private static async Task LoadFromServer()
    {
        try
        {
            var payload = await RequestHandler.GetJsonAsync("/ironman/profile/status");
            var response = JsonConvert.DeserializeObject<DowngradeResponse>(payload);

            ProfileStatus.DowngradeLastOfferedAt = response.DowngradeLastOfferedAt;
            ProfileStatus.ProfileType = response.ProfileType;
            
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError("Failed to load profile type: " + ex);
            NotificationManager.DisplayWarningNotification("Failed to load Ironman profile type - check the server");
        }
    }
}
