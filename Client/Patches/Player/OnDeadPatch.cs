namespace IronManClient.Patches.Player;

using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using Models;
using Utils;
using SPT.Reflection.Patching;
using ProfileStatus=Utils.ProfileStatus;

internal class OnDeadPatch : ModulePatch
{
    private static FieldInfo _playerCorpse;

    protected override MethodBase GetTargetMethod()
    {
        _playerCorpse = AccessTools.Field(typeof(Player), "Corpse");
        return AccessTools.Method(typeof(Player), nameof(Player.OnDead));
    }

    [PatchPostfix]
    static void Postfix(Player __instance)
    {
        if (__instance.IsYourPlayer)
            return;

        if (ProfileStatus.ProfileType != ProfileType.Ultimate && ProfileStatus.ProfileType != ProfileType.Hardcore)
            return;
        
        var victimId = __instance.Profile.ProfileId;
        var localPlayerId = Singleton<GameWorld>.Instance.MainPlayer.Profile.ProfileId;

        if (CorpseStatus.CanLoot(victimId, localPlayerId))
            return;
        
        var thisCorpse = (Corpse)_playerCorpse.GetValue(__instance);
        thisCorpse.IsZombieCorpse = true;
    }
}