namespace IronManClient.Patches.Player;

using System.Reflection;
using EFT.Ballistics;
using EFT.HealthSystem;
using Models;
using Utils;
using SPT.Reflection.Patching;
using ProfileStatus=Utils.ProfileStatus;

internal class ApplyDamagePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ActiveHealthController).GetMethod(nameof(ActiveHealthController.ApplyDamage));
    }

    [PatchPrefix]
    public static void Prefix(ActiveHealthController __instance, EBodyPart bodyPart, float damage, DamageInfo damageInfo)
    {
        if (__instance.Player.IsYourPlayer || damageInfo.DamageType.IsSelfInflicted())
            return;
        
        if (damageInfo.Player?.iPlayer?.Profile == null)
            return;

        if (ProfileStatus.ProfileType != ProfileType.Ultimate && ProfileStatus.ProfileType != ProfileType.Hardcore)
            return;

        var actorWhoWasDamaged = __instance.Player.Profile.ProfileId;
        var actorWhoDamaged = damageInfo.Player.iPlayer.Profile.ProfileId;

        CorpseStatus.RecordDamage(actorWhoWasDamaged, actorWhoDamaged, damage);
    }
}