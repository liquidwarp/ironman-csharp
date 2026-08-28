namespace IronManClient.Patches.Player;

using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using Models;
using SPT.Reflection.Patching;
using ProfileStatus=Utils.ProfileStatus;

internal class OnGameSessionEndPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.OnGameSessionEnd));
    }

    [PatchPostfix]
    static void Postfix(Player __instance, ExitStatus exitStatus)
    {
        if (!__instance.IsYourPlayer || exitStatus != ExitStatus.Killed)
            return;

        if (__instance.Profile.Side == EPlayerSide.Savage)
            return;
        
        var downgradeTarget = ProfileStatus.ProfileType switch
        {
            ProfileType.Hardcore => ProfileType.Ultimate,
            ProfileType.Ultimate => ProfileType.Standard,
            _ => ProfileType.None
        };
            
        if (downgradeTarget == ProfileType.None)
            return;

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var cooldown = 24 * 60 * 60; // 24hr

        if (now - ProfileStatus.DowngradeLastOfferedAt < cooldown)
            return;
        
        Plugin.Log.LogInfo($"Downgrade is available, will prompt on menu load. {ProfileStatus.ProfileType} -> {downgradeTarget}");

        ProfileStatus.DowngradeAvailable = true;
        ProfileStatus.DowngradeTarget = downgradeTarget;
    }
}