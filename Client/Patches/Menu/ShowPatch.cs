namespace IronManClient.Patches.Menu;

using System;
using System.Reflection;
using System.Threading.Tasks;
using EFT.Communications;
using EFT.UI;
using HarmonyLib;
using Models;
using Newtonsoft.Json;
using SPT.Common.Http;
using SPT.Reflection.Patching;
using ProfileStatus = Utils.ProfileStatus;

internal class ShowPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(MenuScreen), nameof(MenuScreen.Show), new[] { typeof(MenuScreen.MainMenuBaseScreenController) });
    }

    [PatchPostfix]
    public static void Postfix()
    {
        if (!ProfileStatus.DowngradeAvailable || ProfileStatus.DowngradeTarget == ProfileType.None)
            return;
        
        _ = HandleDowngradeConfirmation();
    }
    
    private static async Task HandleDowngradeConfirmation()
    {
        var accepted = await ShowConfirmation(
        $"You died on an Ironman {ProfileStatus.ProfileType} profile.\n\n " +
        $"If the losses are too painful, might I suggest downgrading to {ProfileStatus.DowngradeTarget}?\n\n" +
        $"This change is not reversible, and this option will only appear once per day.\n\n" +
        $"If you regret your choices, click Yes to downgrade.",
        "Ironman");

        if (accepted)
        {
            var profileType = await AcceptDowngradeConfirmation();
            if (profileType != null)
            {
                ProfileStatus.ProfileType = profileType.Value;
            }
        }
        else
        {
            _ = await DeclineDowngradeConfirmation();
        }

        ProfileStatus.DowngradeAvailable = false;
        ProfileStatus.DowngradeTarget = ProfileType.None;
    }
    
    private static Task<bool> ShowConfirmation(string description, string caption)
    {
        var itemUiContext = ItemUiContext.Instance;

        if (itemUiContext == null)
        {
            Plugin.Log.LogInfo("ItemUiContext == null");
            return Task.FromResult(false);
        }

        var result = new TaskCompletionSource<bool>();

        var context = itemUiContext.ShowMessageWindow(
        description,
        () => result.TrySetResult(true),
        () => result.TrySetResult(false),
        caption,
        0f,
        true
        );

        context.OnClose += () => result.TrySetResult(false);

        return result.Task;
    }
    
    private static async Task<ProfileType?> AcceptDowngradeConfirmation()
    {
        try
        {
            var payload = await RequestHandler.GetJsonAsync("/ironman/profile/downgrade/accept");
            var response = JsonConvert.DeserializeObject<DowngradeResponse>(payload);

            ProfileStatus.DowngradeLastOfferedAt = response.DowngradeLastOfferedAt;

            return response.ProfileType;
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError("Failed to load profile type: " + ex);
            NotificationManager.DisplayWarningNotification("Failed to load Ironman profile type - check the server");

            return null;
        }
    }
    
    private static async Task<ProfileType?> DeclineDowngradeConfirmation()
    {
        try
        {
            var payload = await RequestHandler.GetJsonAsync("/ironman/profile/downgrade/decline");
            var response = JsonConvert.DeserializeObject<DowngradeResponse>(payload);

            ProfileStatus.DowngradeLastOfferedAt = response.DowngradeLastOfferedAt;

            return response.ProfileType;
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError("Failed to load profile type: " + ex);
            NotificationManager.DisplayWarningNotification("Failed to load Ironman profile type - check the server");

            return null;
        }
    }
}