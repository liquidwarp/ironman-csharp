namespace IronManClient.Patches.Skills;

using System.Collections.Generic;
using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using Models;
using SPT.Reflection.Patching;
using ProfileStatus=Utils.ProfileStatus;

public class OnTriggerPatch : ModulePatch
{
    private static readonly HashSet<ESkillId> ExcludedSkills =
    [
        ESkillId.Metabolism,
        ESkillId.BotReload,
        ESkillId.BotSound
    ];
    
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Skill), nameof(Skill.OnTrigger));
    }

    [PatchPrefix]
    public static void Prefix(Skill __instance, ref float val)
    {
        if (ProfileStatus.ProfileType != ProfileType.Ultimate && ProfileStatus.ProfileType != ProfileType.Hardcore)
            return;
        
        if (ExcludedSkills.Contains(__instance.Id))
            return;

        var mainPlayer = Singleton<GameWorld>.Instance?.MainPlayer;
        if (mainPlayer is null || __instance.SkillManager != mainPlayer.Skills)
            return;

        val *= GetSkillGainMultiplier(ProfileStatus.ProfileType);
    }

    private static float GetSkillGainMultiplier(ProfileType profileType)
    {
        return profileType switch
        {
            ProfileType.Ultimate => 1.25f,
            ProfileType.Hardcore => 2f,
            _ => 1.0f
        };
    }
}
